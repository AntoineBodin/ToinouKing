using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Assets.Scripts.Helpers;
using Assets.Scripts.UI;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public GameObject PlayerPrefab;
    public Dice Dice;
    public ParticleSystem ConfettisParticleSystem;

    private GameParameters gameParameters;
    public bool AnimateDice => gameParameters != null && gameParameters.animaterDice;
    public bool AnimateTokenMovement => gameParameters != null && gameParameters.animateTokenMovement;

    private int playerCount = 0;
    private int currentPlayerIndex = 0;
    private int winningPlayerIndex = 1;

    public LudoPlayer CurrentPlayer { get; private set; }

    private GameState gameState;

    [Header("Board")]
    [SerializeField] private Image boardImage;
    [SerializeField] private Sprite BoardImageClassic;
    [SerializeField] private Sprite boardImageTA;

    [Header("Spaces")]
    public List<TokenSpace> TokenSpaces;

    [Header("Player 1")]
    public PlayerUIWithScore Player1UI;
    public List<TokenSpace> HomeSpacesPlayer1Classic;
    public TokenSpace HomeSpacePlayer1TA;
    public PlayerParameter Player1Parameters;

    [Header("Player 2")]
    public PlayerUIWithScore Player2UI;
    public List<TokenSpace> HomeSpacesPlayer2Classic;
    public TokenSpace HomeSpacePlayer2TA;
    public PlayerParameter Player2Parameters;

    [Header("Player 3")]
    public PlayerUIWithScore Player3UI;
    public List<TokenSpace> HomeSpacesPlayer3Classic;
    public TokenSpace HomeSpacePlayer3TA;
    public PlayerParameter Player3Parameters;

    [Header("Player 4")]
    public PlayerUIWithScore Player4UI;
    public List<TokenSpace> HomeSpacesPlayer4Classic;
    public TokenSpace HomeSpacePlayer4TA;
    public PlayerParameter Player4Parameters;

    private readonly List<List<TokenSpace>> spawnSpaces = new();
    private readonly List<PlayerParameter> playerParameters = new();
    private readonly List<PlayerUIWithScore> playerUIs = new();

    private readonly RoundInfo roundInfo = new();

    public List<LudoPlayer> PlayingPlayers => Players.Where(p => p.CanPlay).ToList();
    public List<LudoPlayer> Players = new();

    public bool IsMyTurn => OnlinePlayerIdentity.ID == CurrentPlayer.ID;
    public bool IsOnline => gameParameters != null && gameParameters.IsOnline;
    public bool CanPlayIfOnline => !IsOnline || IsMyTurn;

    public LudoPlayerInfo OnlinePlayerIdentity { get; internal set; }
    public GameMode GameMode => gameParameters.gameMode;

    public static GameManager Instance;

    #region SETUP

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BoardSizeWatcher.Instance.OnResolutoinChanged += () =>
        {
            Players.ForEach(p => p.ResetTokenSize());
        };

        Dice.OnDiceRollEnd += RollDice;
        Dice.OnDiceRollStarts += () =>
        {
            if (gameState == GameState.WaitingForDice)
            {
                CurrentPlayer.ResetTimer();
            }
        };
    }

    internal void SetupGame(GameParameters gameParameters)
    {
        this.gameParameters = gameParameters;
        playerCount = this.gameParameters.Players.Count;

        SetupBoardForGameMode();

        if (playerCount == 2)
        {
            SetupListsFor2Players();
        }
        else
        {
            SetupLists(playerCount);
        }

        playerUIs.ForEach(p => p.OnPlayerTimeToPlayEnd += EndTimer);

        for (int i = 0; i < TokenSpaces.Count; i++)
        {
            TokenSpaces[i].Index = i;
        }

        CreatePlayers();
        StartCoroutine(StartGameAnimationCoroutine(gameParameters.spawnWithToken));
    }

    private void StartGame()
    {
        currentPlayerIndex = this.gameParameters.FirstPlayerIndex;

        UpdateCurrentPlayerWithIndex();
        UpdateCurrentPlayerDisplay(false);
        SwitchToStateStartRound();
    }

    private void SetupBoardForGameMode()
    {
        switch (gameParameters.gameMode)
        {
            case GameMode.Classic:
                boardImage.sprite = BoardImageClassic;
                break;
            case GameMode.TimeAttack:
                boardImage.sprite = boardImageTA;
                break;
        }

        if (gameParameters.gameMode == GameMode.TimeAttack)
        {
            InGameUIManager.Instance.DisplayTimer();
            InGameUIManager.Instance.StartTimer(gameParameters.timeLimitInSeconds);
            InGameUIManager.Instance.OnTimerEnd += () => roundInfo.IsLastTurn = true;
        }
        else
        {
            InGameUIManager.Instance.HideTimer();
        }
    }

    private void EndTimer()
    {
        if (CanPlayIfOnline) 
        { 
            if (gameState == GameState.WaitingForDice)
            {
                Dice.OnMouseDown();
            }
            else if (gameState == GameState.ChoosingToken)
            {
                PlayRandomToken();
            }
            else
            {
                Debug.Log("End Timer in other state: " + gameState);
            }
        }
    }

    private void CreatePlayers()
    {
        int playerIndex = 0;
        gameParameters.Players.ForEach(playerInfo =>
        {
            if (playerInfo.AvatarID == 0)
            {
                playerInfo.AvatarID = gameParameters.DefaultAvatarID;
            }

            LudoPlayer player = CreatePlayer(playerIndex, playerInfo);
            Players.Add(player);
            playerIndex++;
        });
    }

    private LudoPlayer CreatePlayer(int playerIndex, LudoPlayerInfo playerInfo)
    {
        var playerObject = Instantiate(PlayerPrefab);
        LudoPlayer player = playerObject.GetComponent<LudoPlayer>();

        player.Setup(playerInfo, playerUIs[playerIndex], playerParameters[playerIndex], spawnSpaces[playerIndex]);
        player.SetupLocalBoard();

        return player;
    }

    private IEnumerator StartGameAnimationCoroutine(bool spawnWithToken)
    {
        TokenSpawner.Instance.Setup(Players);
        foreach (var player in Players)
        {
            if (!player.IsBlank)
            {
                StartCoroutine(player.SpawnTokensCoroutine(gameParameters.gameMode == GameMode.Classic? gameParameters.tokenCount : 1, spawnWithToken));
                yield return new WaitForSeconds(1f);
            }
        }

        StartGame();
    }

    #endregion

    private void ResetGame()
    {
        TokenSpawner.Instance.DestroyAndClear();
        Players.Clear();
        playerUIs.ForEach(p => p.Clear());
        Player1UI.gameObject.SetActive(false);
        Player2UI.gameObject.SetActive(false);
        Player3UI.gameObject.SetActive(false);
        Player4UI.gameObject.SetActive(false);
        playerUIs.Clear();
        playerParameters.Clear();
        spawnSpaces.Clear();
        playerCount = 0;
        currentPlayerIndex = 0;
        winningPlayerIndex = 1;
        roundInfo.Reset();
        TokenSpaces.ForEach(t => t.TokensByPlayer.Clear());
    }
    private void Update()
    {
        switch (gameState)
        {
            case GameState.StartRound:
                break;
            case GameState.WaitingForDice:
                break;
            case GameState.AutoPlay:
                break;
            case GameState.ChoosingToken:
                break;
            case GameState.EndRound:
                break;
            case GameState.NextRound:
                break;
        }
    }

    #region ChangeState

    private void SwitchToStateStartRound()
    {
        gameState = GameState.StartRound;
        roundInfo.Reset();
        SwitchToStateWaitingForDice();
    }

    private void SaveGameToLobby()
    {
        Snapshot snapshot = new()
        {
            //Tokens = tokens,
            Players = Players,
            CurrentPlayer = CurrentPlayer
        };

        string snapshotJson = JsonConvert.SerializeObject(snapshot);
        Debug.Log(snapshotJson);

        LobbyService.Instance.UpdateLobbyAsync(LobbyServiceManager.Instance.CurrentLobby.Id, new UpdateLobbyOptions()
        {
            Data = new()
            {
                {
                    "test", new DataObject(DataObject.VisibilityOptions.Member, snapshotJson)
                }
            }
        });

    }

    private void SwitchToStateWaitingForDice()
    {
        CurrentPlayer.StartTimer();
        gameState = GameState.WaitingForDice;
        if (IsMyTurn)
        {
            Dice.UpdateIdling(true);
        }
    }

    private void SwitchToStateAutoPlay()
    {
        gameState = GameState.AutoPlay;
        AutoPlay();
    }

    private void SwitchToStateChoosingToken() 
    {
        gameState = GameState.ChoosingToken;
        if (CanPlayIfOnline)
        {
            UpdateIdlingTokens(true);
        }
    }

    private void SwitchToStateEndRound()
    {
        gameState = GameState.EndRound;
        EndRound();
    }

    private void SwitchToStateNextRound()
    {
        gameState = GameState.NextRound;
        NextRound();
    }

    private void SwitchToEndGame()
    {
        //Debug.Log("Switch to state END_GAME");
        gameState = GameState.EndGame;

        StartCoroutine(EndGameCoroutine());
    }


    #endregion

    private void AutoPlay()
    {
        CurrentPlayer.StartTimer();

        roundInfo.TokensWithNewPosition.AddRange(CurrentPlayer.GetTokensNewPositions(Dice.Value));
        if (roundInfo.TokensWithNewPosition.Count == 0)
        {
            CurrentPlayer.ResetTimer();
            SwitchToStateNextRound();
            return;
        }

        SwitchToStateChoosingToken();

        if (roundInfo.TokensWithNewPosition.Count == 1)
        {
            PlayToken(roundInfo.TokensWithNewPosition.First().Key);
        }
    }

    public void PickToken(int tokenID)
    {
        if (gameParameters.IsOnline)
        {
            PickToken_ServerRPC(tokenID);
        }
        else
        {
            PlayToken(tokenID);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PickToken_ServerRPC(int tokenID)
    {
        PlayToken_ClientRPC(tokenID);
    }

    [ClientRpc]
    public void PlayToken_ClientRPC(int tokenID)
    {
        PlayToken(tokenID);
    }

    private async void PlayToken(int tokenID)
    {
        if (gameState != GameState.ChoosingToken)
        {
            return;
        }
        CurrentPlayer.ResetTimer();

        UpdateIdlingTokens(false);

        TokenSpace newPosition = roundInfo.TokensWithNewPosition[tokenID];

        Token tokenToEat = null;
        if (!newPosition.IsSafe)
        {
            LudoPlayer foundPlayer = newPosition.TokensByPlayer.Keys.FirstOrDefault();
            if (foundPlayer != null)
            {
                tokenToEat = newPosition.TokensByPlayer[foundPlayer].FirstOrDefault();
            }
        }

        var tokenToPlay = TokenSpawner.Instance.TokensByPlayer.FindById(tokenID);

        if (tokenToPlay.IsInHouse && gameParameters.gameMode == GameMode.TimeAttack)
        {
            TokenSpawner.Instance.SpawnTokenForPlayer(CurrentPlayer, false);
        }

        await CurrentPlayer.MoveToken(tokenToPlay, newPosition);

        if (tokenToEat != null)
        {
            await EatToken(tokenToEat);
            if (GameMode == GameMode.TimeAttack)
                CurrentPlayer.Score(1);
        }

        if (CurrentPlayer.IsWinningIndex(newPosition.Index))
        {
            int winningTokenPoints = 1;
            if (gameParameters.gameMode == GameMode.TimeAttack)
            {
                winningTokenPoints = gameParameters.pointsForEnteredToken;
            }

            roundInfo.EnterAToken();
            CurrentPlayer.Score(winningTokenPoints);
            CurrentPlayer.EnterAToken();
            ConfettisParticleSystem.Play();
        }

        if (roundInfo.IsLastTurn)
        {
            SwitchToEndGame();
        }

        if (CurrentPlayer.GetPlayableTokens().Count() == 0)
        {
            PlayerWins();
        }

        if (gameState != GameState.EndGame)
        {
            SwitchToStateEndRound();
        }
    }

    private void PlayRandomToken()
    {
        int keyIndex = UnityEngine.Random.Range(0, roundInfo.TokensWithNewPosition.Keys.Count);
        int tokenId = roundInfo.TokensWithNewPosition.Keys.ElementAt(keyIndex);
        PickToken(tokenId);
    }

    private void PlayerWins()
    {
        roundInfo.PlayerWon();
        CurrentPlayer.Win(winningPlayerIndex);
        winningPlayerIndex++;
        if (PlayingPlayers.Count == 1)
        {
            SwitchToEndGame();
        }
    }

    private async Task EatToken(Token tokenToEat)
    {
        if (tokenToEat == null)
        {
            return;
        }
        roundInfo.Eat();

        CurrentPlayer.PlayerInfo.KilledTokens++;
        tokenToEat.player.PlayerInfo.DeadTokens++; ;

        await tokenToEat.player.MoveTokenToHouse(tokenToEat, gameParameters.gameMode);
    }

    public void EndRound()
    {
        if (!roundInfo.PlayerHasWon && (Dice.Value == 6 || roundInfo.HasEaten || roundInfo.HasEnteredAToken))
        {
            SwitchToStateStartRound();
        }
        else
        {
            SwitchToStateNextRound();
        }
    }

    private void NextRound()
    {
        currentPlayerIndex++;
        currentPlayerIndex %= PlayingPlayers.Count;

        UpdateCurrentPlayerWithIndex();

        UpdateCurrentPlayerDisplay(true);

        SwitchToStateStartRound();
    }

    public void RollDice()
    {
        CurrentPlayer.ResetTimer();
        if (gameState != GameState.WaitingForDice)
        {
            return;
        }
        Dice.UpdateIdling(false);

        SwitchToStateAutoPlay();
    }

    public void RollDiceOnline(int diceValue)
    {
        Roll_ServerRpc(diceValue);
    }

    [ServerRpc(RequireOwnership = false)]
    public void Roll_ServerRpc(int diceValue)
    {
        Roll_ClientRpc(diceValue);
    }

    [ClientRpc]
    private void Roll_ClientRpc(int value)
    {
        Dice.AnimateRoll(value);
    }

    private void UpdateIdlingTokens(bool isIdling)
    {
        foreach (var playableTokenID in roundInfo.TokensWithNewPosition.Keys)
        {
            TokenSpawner.Instance.TokensByPlayer.FindById(playableTokenID).UpdateIdling(isIdling);
        }
    }

    private void UpdateCurrentPlayerWithIndex()
    {
        CurrentPlayer = PlayingPlayers[currentPlayerIndex];
    }

    private void UpdateCurrentPlayerDisplay(bool withAnimation)
    {
        if (withAnimation)
        {
            InGameUIManager.Instance.UpdateCurrentPlayer(CurrentPlayer);
        }
        else
        {
            InGameUIManager.Instance.DisplayCurrentPlayer(CurrentPlayer);
        }
    }

    private void SetupLists(int playerCount)
    {
        if (gameParameters.gameMode == GameMode.TimeAttack)
        {
            spawnSpaces.Add(new List<TokenSpace> { HomeSpacePlayer1TA });
            spawnSpaces.Add(new List<TokenSpace> { HomeSpacePlayer2TA });
            spawnSpaces.Add(new List<TokenSpace> { HomeSpacePlayer3TA });
        }
        else
        {
            spawnSpaces.Add(HomeSpacesPlayer1Classic);
            spawnSpaces.Add(HomeSpacesPlayer2Classic);
            spawnSpaces.Add(HomeSpacesPlayer3Classic);
        }
        
        playerParameters.Add(Player1Parameters);
        playerParameters.Add(Player2Parameters);
        playerParameters.Add(Player3Parameters);

        Player1UI.gameObject.SetActive(true);
        Player2UI.gameObject.SetActive(true);
        Player3UI.gameObject.SetActive(true);

        playerUIs.Add(Player1UI);
        playerUIs.Add(Player2UI);
        playerUIs.Add(Player3UI);
        
        if (playerCount == 4)
        {
            if (gameParameters.gameMode == GameMode.TimeAttack)
            {
                spawnSpaces.Add(new List<TokenSpace> { HomeSpacePlayer4TA });
            }
            else
            {
                spawnSpaces.Add(HomeSpacesPlayer4Classic);
            }
            playerParameters.Add(Player4Parameters);
            Player4UI.gameObject.SetActive(true);
            playerUIs.Add(Player4UI);
        }
    }

    private void SetupListsFor2Players()
    {
        if (gameParameters.gameMode == GameMode.TimeAttack)
        {
            spawnSpaces.Add(new List<TokenSpace> { HomeSpacePlayer1TA });
            spawnSpaces.Add(new List<TokenSpace> { HomeSpacePlayer3TA });
        }
        else
        {
            spawnSpaces.Add(HomeSpacesPlayer1Classic);
            spawnSpaces.Add(HomeSpacesPlayer3Classic);
        }

        playerParameters.Add(Player1Parameters);
        playerParameters.Add(Player3Parameters);
 
        Player1UI.gameObject.SetActive(true);
        Player3UI.gameObject.SetActive(true);
        playerUIs.Add(Player1UI);
        playerUIs.Add(Player3UI);
    }
    
    private IEnumerator EndGameCoroutine()
    {
        Players.Find(t => t.CanPlay).Win(winningPlayerIndex);

        if (gameParameters.gameMode == GameMode.TimeAttack)
        {
            foreach (var kvp in TokenSpawner.Instance.TokensByPlayer.ToList())
            {
                var player = kvp.Key;
                var tokens = kvp.Value;

                foreach (var token in tokens.ToList())
                {
                    if (token.currentPosition == player.StartSpace)
                    {
                        player.DecreaseScore(1);
                        player.AddSpawnToken();
                    }
                    if (token.currentPosition.IsFinishLine)
                    {
                        player.Score(1);
                        player.AddHouseToken();
                    }
                    yield return new WaitForSeconds(0.25f);
                    TokenSpawner.Instance.TokensByPlayer.RemoveToken(token);
                    Destroy(token.gameObject);
                }
            }
        }
        InGameUIManager.Instance.ResetCurrentPlayer();
        EndGameUIManager.Instance.UpdateUI(Players, gameParameters.gameMode == GameMode.TimeAttack);
        GameMenuNavigator.Instance.DisplayEndGamePannel();
        ResetGame();
    }
}
