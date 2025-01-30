using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Assets.Scripts;
using System;
using Assets.Scripts.Lobbies;
using Unity.Netcode;
using System.Linq;

public class LobbyServiceManager : NetworkBehaviour
{
    public Lobby CurrentLobby;
    public static LobbyServiceManager Instance { get; private set; }
    //private string relayCode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Créer un lobby et stocker le code Relay
    public async Task<Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, LudoPlayerInfo playerInfo)
    {
        try
        {
            await Authenticate();

            CreateLobbyOptions options = new()
            {
                IsPrivate = false,
                Player = playerInfo.GetPlayerData()
            };

            // Créer le lobby
            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            HeartBeatPingManager.Instance.Setup();

            // Créer une allocation Relay
            var relayCode = await RelayServiceManager.Instance.StartRelayHosting(maxPlayers);

            // Stocker le code Relay dans les données du lobby
            var relayData = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                { { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) } }
            };

            // Ajouter le code Relay dans les données du lobby
            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, relayData);

            Debug.Log($"Lobby created with ID: {CurrentLobby.Id} and Relay Code: {relayCode}");

            var callbacks = LobbyManager2.Instance.GetLobbyEventCallbacks();

            var lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);

            GameManager.Instance.OnlinePlayerIdentity = playerInfo;

            HeartBeatPingManager.Instance.StartHeartBeatTimer();
            //HeartBeatPingManager.Instance.StartKeepAliveTimer();

            return CurrentLobby;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            return null;
        }
    }

    // Rejoindre un lobby existant avec un code Relay
    public async Task<Lobby> JoinLobbyAsync(string joinCode, LudoPlayerInfo playerInfo)
    {
        try
        {
            await Authenticate();
            
            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions()
            {
                Player = playerInfo.GetPlayerData()
            };

            // Rejoindre le lobby
            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode, options);
            
            HeartBeatPingManager.Instance.Setup();

            // Récupérer le code Relay du lobby
            if (CurrentLobby.Data.ContainsKey("RelayCode"))
            {
                var relayCode = CurrentLobby.Data["RelayCode"].Value;
                Debug.Log($"Joined lobby with Relay Code: {relayCode}");

                await RelayServiceManager.Instance.JoinRelayAsync(relayCode);
            }

            HeartBeatPingManager.Instance.StartKeepAliveTimer();
            GameManager.Instance.OnlinePlayerIdentity = playerInfo;

            return CurrentLobby;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to join lobby: {ex.Message}");
            return null;
        }
    }

    private async Task Authenticate()
    {
        await Authenticate("Player" + UnityEngine.Random.Range(0, 1000));
    }

    private async Task Authenticate(string playerName)
    {
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            InitializationOptions options = new InitializationOptions();
            options.SetProfile(playerName);
            await UnityServices.InitializeAsync(options);
        }

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as " + AuthenticationService.Instance.PlayerId);
        };

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }


    public void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            List<LudoPlayerInfo> players = CurrentLobby.Players.Select(p => new LudoPlayerInfo(p)).ToList();
            int firstPlayerIndex = UnityEngine.Random.Range(0, players.Count);

            Debug.Log("Starting game with firt player index: " + firstPlayerIndex);

            StartGame_ClientRpc(firstPlayerIndex);
        }
        else
        {
            Debug.Log("Only host can start the game");
        }
    }

    /// <summary>
    /// Start the game on every client
    /// </summary>
    [ClientRpc]
    private void StartGame_ClientRpc(int firstPlayerIndex)
    {
        Debug.Log("Start Game Client RPC called");
        List<LudoPlayerInfo> players = CurrentLobby.Players.Select(p => new LudoPlayerInfo(p)).ToList();

        GameParameters gameParameters = new()
        {
            Players = players,
            IsOnline = true,
            FirstPlayerIndex = firstPlayerIndex
        };

        GameMenuNavigator.Instance.DisplayBoardCanvas();

        GameManager.Instance.StartGame(gameParameters);
    }
}