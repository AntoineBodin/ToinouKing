using Assets.Scripts;
using Assets.Scripts.DataStructures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public GameParameters GameParameters;
    public GameObject TokenPrefab;
    public GameObject PlayerPrefab;
    public Dice Dice;
    public TMP_Text CurrentPlayerText;
    public GameObject Canvas;
    private List<Token> tokens = new();

    private int playerCount = 0;
    private int currentPlayerIndex = 0;

    public LudoPlayer CurrentPlayer { get; private set; }

    private GameState gameState;

    [Header("Spaces")]
    public List<TokenSpace> TokenSpaces;

    [Header("Player 1")]
    public TMP_Text Player1Name;
    public List<TokenSpace> HomeSpacesPlayer1;
    public PlayerParameter Player1Parameters;

    [Header("Player 2")]
    public TMP_Text Player2Name;
    public List<TokenSpace> HomeSpacesPlayer2;
    public PlayerParameter Player2Parameters;

    [Header("Player 3")]
    public TMP_Text Player3Name;
    public List<TokenSpace> HomeSpacesPlayer3;
    public PlayerParameter Player3Parameters;

    [Header("Player 4")]
    public TMP_Text Player4Name;
    public List<TokenSpace> HomeSpacesPlayer4;
    public PlayerParameter Player4Parameters;

    private List<List<TokenSpace>> spawnSpaces = new();
    private List<TMP_Text> playerTexts = new();
    private List<PlayerParameter> playerParameters = new();

    private RoundInfo roundInfo = new();

    public List<LudoPlayer> Players = new();

    public bool IsMyTurn => OnlinePlayerIdentity.ID == CurrentPlayer.PlayerInfo.ID;

    public bool CanPlayIfOnline => !GameParameters.IsOnline || IsMyTurn;

    public LudoPlayerInfo OnlinePlayerIdentity { get; internal set; }// = new();


    #region SETUP
    internal void StartGame(GameParameters gameParameters)
    {
        GameParameters = gameParameters;
        playerCount = GameParameters.Players.Count;
        Debug.Log("Start game with " + playerCount + " players");
        if (playerCount == 2)
        {
            SetupListsFor2Players();
        }
        else
        {
            SetupLists();
        }

        for (int i = 0; i < TokenSpaces.Count; i++)
        {
            TokenSpaces[i].Index = i;
        }

        CreatePlayers();
        SpawnTokens();

        currentPlayerIndex = GameParameters.FirstPlayerIndex;

        UpdateCurrentPlayer();

        SwitchToStateStartRound();
    }

    private void SetupOnline() 
    {
        var gp = new GameParameters()
        {
            Players = new(), //get from lobby
        };
    }

    private void SetupLocal() { }

    private void CreatePlayers()
    {
        int playerIndex = 0;
        GameParameters.Players.ForEach(playerInfo => 
        {
            if (playerInfo.AvatarID == 0)
            {
                playerInfo.AvatarID = GameParameters.DefaultAvatarID;
            }

            playerTexts[playerIndex].text = playerInfo.Name.ToString();

            LudoPlayer player = CreatePlayer(playerIndex);
            player.PlayerInfo = playerInfo;
            Players.Add(player);
            playerIndex++;
        });
    }

    private LudoPlayer CreatePlayer(int playerIndex)
    {
        var playerObject = Instantiate(PlayerPrefab);
        LudoPlayer player = playerObject.GetComponent<LudoPlayer>();
        player.PlayerParameter = playerParameters[playerIndex];
        player.SpawnSpaces = spawnSpaces[playerIndex];
        player.StartSpace = TokenSpaces[player.PlayerParameter.StartingIndex];
        player.PlayerInfo = GameParameters.Players[playerIndex];
        player.GameManager = this;

        player.SetupLocalBoard();

        return player;
    }

    private void SpawnTokens()
    {
        int playerIndex = 0;
        Players.ForEach(player =>
        {
            if (!player.IsBlank)
            {
                player.InstantiateToHome(TokenPrefab, Canvas, playerIndex);
            }
            playerIndex++;
        });
    }

    public void AddToken(Token token)
    {
        tokens.Add(token);
    }

    #endregion

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
        Debug.Log("Switch to state START_ROUND");
        gameState = GameState.StartRound;
        roundInfo.Reset();
        //SaveGameToLobby();
        SwitchToStateWaitingForDice();
    }

    private void SaveGameToLobby()
    {
        Snapshot snapshot = new Snapshot()
        {
            Tokens = tokens,
            Players = Players,
            CurrentPlayer = CurrentPlayer
        };

        JsonSerializer serializer = new JsonSerializer();

        string snapshotJson = JsonConvert.SerializeObject(snapshot);
        Debug.Log(snapshotJson);

        LobbyService.Instance.UpdateLobbyAsync(Multiplayer.Instance.CurrentLobby.Id, new UpdateLobbyOptions()
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
        Debug.Log("Switch to state WAITING_FOR_DICE : " + OnlinePlayerIdentity.ID);
        gameState = GameState.WaitingForDice;
        if (IsMyTurn)
        {
            Dice.UpdateIdling(true);
        }
    }

    private void SwitchToStateAutoPlay()
    {
        Debug.Log("Switch to state AUTO_PLAY");

        gameState = GameState.AutoPlay;
        AutoPlay();
    }

    private void SwitchToStateChoosingToken() 
    {
        Debug.Log("Switch to state CHOOSING_TOKEN");

        gameState = GameState.ChoosingToken;
        if (CanPlayIfOnline)
        {
            UpdateIdlingTokens(true);
        }
    }

    private void SwitchToStateEndRound()
    {
        Debug.Log("Switch to state END_ROUND");

        gameState = GameState.EndRound;
        EndRound();
    }

    private void SwitchToStateNextRound()
    {
        Debug.Log("Switch to state NEXT_ROUND");
        gameState = GameState.NextRound;
        NextRound();
    }

    #endregion

    private void AutoPlay()
    {
        /*
        List<Token> tokensToConsider;
        if (Dice.Value != 6)
        {
            tokensToConsider = CurrentPlayer.Tokens.Where(t => !t.IsInHouse && !t.HasWon).ToList();
        }
        else
        {
            tokensToConsider = CurrentPlayer.Tokens.Where(t => !t.HasWon).ToList();
        }
        tokensToConsider.ForEach(token =>
        {
            TokenSpace newPosition = TryGetNewPosition(token);
            if (newPosition == null)
            {
                return;
            }
            if (!newPosition.IsSafe && newPosition.IsOccupied && newPosition.TokensByPlayer.ContainsKey(CurrentPlayer)) 
            {
                return;
            }
            roundInfo.TokensWithNewPosition.Add(token.ID, newPosition);
        });*/

        roundInfo.TokensWithNewPosition.AddRange(CurrentPlayer.GetTokensNewPositions(Dice.Value));
        if (roundInfo.TokensWithNewPosition.Count == 0)
        {
            SwitchToStateNextRound();
            return;
        }

        SwitchToStateChoosingToken();

        if (roundInfo.TokensWithNewPosition.Count == 1)
        {
            PlayToken_ClientRPC(roundInfo.TokensWithNewPosition.First().Key);
        }
    }

    public void PickToken(int tokenID)
    {
        Debug.Log("Token chosen: " + tokenID);
        if (GameParameters.IsOnline)
        {
            PickToken_ServerRPC(tokenID);
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
        if (gameState != GameState.ChoosingToken)
        {
            return;
        }

        UpdateIdlingTokens(false);

        TokenSpace newPosition = roundInfo.TokensWithNewPosition[tokenID];

        if (newPosition.Index == CurrentPlayer.PlayerParameter.WinningSpaceIndex)
        {
            roundInfo.EnterAToken();
        }

        if (!newPosition.IsSafe)
        {
            Token tokenToEat = null;
            LudoPlayer foundPlayer = newPosition.TokensByPlayer.Keys.FirstOrDefault();
            if (foundPlayer != null)
            {
                tokenToEat = newPosition.TokensByPlayer[foundPlayer].FirstOrDefault();
            }

            EatToken(tokenToEat);
        }
        CurrentPlayer.MoveToken(tokens[tokenID], newPosition, true);

        SwitchToStateEndRound();
    }

    private void EatToken(Token tokenToEat)
    {
        if (tokenToEat == null)
        {
            return;
        }
        roundInfo.Eat();
        tokenToEat.player.MoveTokenToHouse(tokenToEat);
    }

    public void EndRound()
    {
        // Play again
        if (Dice.Value == 6 || roundInfo.HasEaten || roundInfo.HasEnteredAToken)
        {
            Debug.Log("Playing Again");
            SwitchToStateStartRound();
        }

        // Next Player
        else
        {
            Debug.Log("Next Turn");
            SwitchToStateNextRound();
        }
    }

    private void NextRound()
    {
        currentPlayerIndex++;
        currentPlayerIndex %= GameParameters.Players.Count;

        UpdateCurrentPlayer();

        SwitchToStateStartRound();
    }

    public void RollDice()
    {
        if (gameState != GameState.WaitingForDice)
        {
            return;
        }
        Dice.UpdateIdling(false);

        SwitchToStateAutoPlay();
    }

    private void UpdateIdlingTokens(bool isIdling)
    {
        foreach (var playableTokenID in roundInfo.TokensWithNewPosition.Keys)
        {
            tokens[playableTokenID].UpdateIdling(isIdling);
        }
    }

    internal TokenSpace TryGetNewPosition(Token token)
    {
        if (token.IsInHouse)
        {
            if (Dice.Value == 6)
            {
                return token.player.StartSpace;
            }
            return null;
        }

        int newPositionIndex = token.currentPosition.Index + Dice.Value;

        newPositionIndex = token.GetNewPosition(newPositionIndex);

        if (newPositionIndex == -1)
        {
            return null;
        }

        return TokenSpaces[newPositionIndex];
    }

    private void UpdateCurrentPlayer()
    {
        CurrentPlayer = Players[currentPlayerIndex];
        CurrentPlayerText.text = CurrentPlayer.PlayerInfo.Name.ToString();
        CurrentPlayerText.color = playerParameters[currentPlayerIndex].TokenColor;
    }

    private void SetupLists()
    {
        spawnSpaces.Add(HomeSpacesPlayer1);
        spawnSpaces.Add(HomeSpacesPlayer2);
        spawnSpaces.Add(HomeSpacesPlayer3);
        spawnSpaces.Add(HomeSpacesPlayer4);

        playerParameters.Add(Player1Parameters);
        playerParameters.Add(Player2Parameters);
        playerParameters.Add(Player3Parameters);
        playerParameters.Add(Player4Parameters);

        playerTexts.Add(Player1Name);
        playerTexts.Add(Player2Name);
        playerTexts.Add(Player3Name);
        playerTexts.Add(Player4Name);
    }

    private void SetupListsFor2Players()
    {
        spawnSpaces.Add(HomeSpacesPlayer1);
        spawnSpaces.Add(HomeSpacesPlayer3);

        playerParameters.Add(Player1Parameters);
        playerParameters.Add(Player3Parameters);

        playerTexts.Add(Player1Name);
        playerTexts.Add(Player3Name);
    }
}
