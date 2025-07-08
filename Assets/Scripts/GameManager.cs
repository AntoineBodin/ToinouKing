using Assets.Scripts;
using Assets.Scripts.DataStructures;
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

public class GameManager : NetworkBehaviour
{
    public GameObject TokenPrefab;
    public GameObject PlayerPrefab;
    public Dice Dice;
    public ParticleSystem ConfettisParticleSystem;
    private List<Token> tokens = new();

    private GameParameters gameParameters;
    public bool AnimateDice => gameParameters != null && gameParameters.animaterDice;
    public bool AnimateTokenMovement => gameParameters != null && gameParameters.animateTokenMovement;

    private int playerCount = 0;
    private int currentPlayerIndex = 0;
    private int winningPlayerIndex = 1;

    public LudoPlayer CurrentPlayer { get; private set; }

    private GameState gameState;

    [Header("Spaces")]
    public List<TokenSpace> TokenSpaces;

    [Header("Player 1")]
    public PlayerUIWithScore Player1UI;
    public List<TokenSpace> HomeSpacesPlayer1;
    public PlayerParameter Player1Parameters;

    [Header("Player 2")]
    public PlayerUIWithScore Player2UI;
    public List<TokenSpace> HomeSpacesPlayer2;
    public PlayerParameter Player2Parameters;

    [Header("Player 3")]
    public PlayerUIWithScore Player3UI;
    public List<TokenSpace> HomeSpacesPlayer3;
    public PlayerParameter Player3Parameters;

    [Header("Player 4")]
    public PlayerUIWithScore Player4UI;
    public List<TokenSpace> HomeSpacesPlayer4;
    public PlayerParameter Player4Parameters;

    private List<List<TokenSpace>> spawnSpaces = new();
    private List<PlayerParameter> playerParameters = new();
    private List<PlayerUIWithScore> playerUIs = new();

    private RoundInfo roundInfo = new();

    public List<LudoPlayer> PlayingPlayers => Players.Where(p => p.CanPlay).ToList();
    public List<LudoPlayer> Players = new();

    public bool IsMyTurn => OnlinePlayerIdentity.ID == CurrentPlayer.ID;
    public bool IsOnline => gameParameters != null && gameParameters.IsOnline;
    public bool CanPlayIfOnline => !IsOnline || IsMyTurn;

    public LudoPlayerInfo OnlinePlayerIdentity { get; internal set; }

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

    internal void StartGame(GameParameters gameParameters)
    {
        this.gameParameters = gameParameters;
        playerCount = this.gameParameters.Players.Count;

        //Debug.Log("Start game with " + playerCount + " players");
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
        StartCoroutine(SpawnTokensCoroutine());

        currentPlayerIndex = this.gameParameters.FirstPlayerIndex;

        UpdateCurrentPlayerWithIndex();
        UpdateCurrentPlayerDisplay(false);

        SwitchToStateStartRound();
    }

    private void EndTimer()
    {
        if (CanPlayIfOnline) 
        { 
            if (gameState == GameState.WaitingForDice)
            {
                Debug.Log("End Timer Rolling Dice");
                Dice.OnMouseDown();
            }
            else if (gameState == GameState.ChoosingToken)
            {
                Debug.Log("End Timer Choosing Token");
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

    private IEnumerator SpawnTokensCoroutine()
    {
        int playerIndex = 0;
        foreach (var player in Players)
        {
            if (!player.IsBlank)
            {
                StartCoroutine(player.SpawnTokensCoroutine(TokenPrefab, playerIndex, gameParameters.tokenCount));
                yield return new WaitForSeconds(1f);
            }
            playerIndex++;
        }
    }

    public void AddTokens(List<Token> newTokens)
    {
        tokens.AddRange(newTokens);
    }

    #endregion

    private void ResetGame()
    {
        tokens.ForEach(t => Destroy(t.gameObject));
        tokens.Clear();
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
        //Debug.Log("Switch to state START_ROUND");
        gameState = GameState.StartRound;
        roundInfo.Reset();
        //SaveGameToLobby();
        SwitchToStateWaitingForDice();
    }

    private void SaveGameToLobby()
    {
        Snapshot snapshot = new()
        {
            Tokens = tokens,
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
        //Debug.Log("Switch to state WAITING_FOR_DICE : " + OnlinePlayerIdentity.ID);
        CurrentPlayer.StartTimer();
        gameState = GameState.WaitingForDice;
        if (IsMyTurn)
        {
            Dice.UpdateIdling(true);
        }
    }

    private void SwitchToStateAutoPlay()
    {
        //Debug.Log("Switch to state AUTO_PLAY");

        gameState = GameState.AutoPlay;
        AutoPlay();
    }

    private void SwitchToStateChoosingToken() 
    {
        //Debug.Log("Switch to state CHOOSING_TOKEN");

        gameState = GameState.ChoosingToken;
        if (CanPlayIfOnline)
        {
            UpdateIdlingTokens(true);
        }
    }

    private void SwitchToStateEndRound()
    {
        //Debug.Log("Switch to state END_ROUND");

        gameState = GameState.EndRound;
        EndRound();
    }

    private void SwitchToStateNextRound()
    {
        //Debug.Log("Switch to state NEXT_ROUND");
        gameState = GameState.NextRound;
        NextRound();
    }

    private void SwitchToEndGame()
    {
        //Debug.Log("Switch to state END_GAME");
        gameState = GameState.EndGame;

        EndGame();
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
        //Debug.Log("Token chosen: " + tokenID);
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
        await CurrentPlayer.MoveToken(tokens.Find(t => t.ID == tokenID), newPosition);

        if (tokenToEat != null)
        {
            await EatToken(tokenToEat);
        }

        if (CurrentPlayer.IsWinningIndex(newPosition.Index))
        {
            roundInfo.EnterAToken();
            CurrentPlayer.Score();
            ConfettisParticleSystem.Play();
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
        int keyIndex = UnityEngine.Random.Range(0, roundInfo.TokensWithNewPosition.Keys.Count - 1);
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

        await tokenToEat.player.MoveTokenToHouse(tokenToEat);
    }

    public void EndRound()
    {
        // Play again
        if (!roundInfo.PlayerHasWon && (Dice.Value == 6 || roundInfo.HasEaten || roundInfo.HasEnteredAToken))
        {
            //Debug.Log("Playing Again");
            SwitchToStateStartRound();
        }

        // Next Player
        else
        {
            //Debug.Log("Next Turn");
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
        //Dice.SetDiceSprite(value);
        Dice.AnimateRoll(value);
        //RollDice();
    }

    private void UpdateIdlingTokens(bool isIdling)
    {
        foreach (var playableTokenID in roundInfo.TokensWithNewPosition.Keys)
        {
            tokens.Find(t => t.ID == playableTokenID).UpdateIdling(isIdling);
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
        spawnSpaces.Add(HomeSpacesPlayer1);
        spawnSpaces.Add(HomeSpacesPlayer2);
        spawnSpaces.Add(HomeSpacesPlayer3);
        
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
            spawnSpaces.Add(HomeSpacesPlayer4);
            playerParameters.Add(Player4Parameters);
            Player4UI.gameObject.SetActive(true);
            playerUIs.Add(Player4UI);
        }
    }

    private void SetupListsFor2Players()
    {
        spawnSpaces.Add(HomeSpacesPlayer1);
        spawnSpaces.Add(HomeSpacesPlayer3);

        playerParameters.Add(Player1Parameters);
        playerParameters.Add(Player3Parameters);
 
        Player1UI.gameObject.SetActive(true);
        Player3UI.gameObject.SetActive(true);
        playerUIs.Add(Player1UI);
        playerUIs.Add(Player3UI);
    }
    
    private void EndGame()
    {
        Players.Find(t => t.CanPlay).Win(winningPlayerIndex);
        InGameUIManager.Instance.ResetCurrentPlayer();
        EndGameUIManager.Instance.UpdateUI(Players);
        GameMenuNavigator.Instance.DisplayEndGamePannel();
        ResetGame();
    }
}
