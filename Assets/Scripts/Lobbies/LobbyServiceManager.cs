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
using Unity.Services.Relay;

public class LobbyServiceManager : NetworkBehaviour
{
    public Lobby CurrentLobby;
    public string PlayerId;
    private ILobbyEvents lobbyEvents;

    public static LobbyServiceManager Instance { get; private set; }

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

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            HeartBeatPingManager.Instance.Setup();

            var relayCode = await RelayServiceManager.Instance.StartRelayHosting(maxPlayers);
            RelayServiceManager.Instance.HostId = PlayerId;

            var relayData = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) },
                    { "RelayHostId", new DataObject(DataObject.VisibilityOptions.Member, PlayerId) }
                }
            };

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, relayData);

            Debug.Log($"Lobby created with ID: {CurrentLobby.Id} and Relay Code: {relayCode}");

            var callbacks = LobbyCallBackManager.Instance.GetLobbyEventCallbacks();
            lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);

            GameManager.Instance.OnlinePlayerIdentity = playerInfo;

            HeartBeatPingManager.Instance.StartHeartBeatTimer();

            return CurrentLobby;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create lobby: {ex.Message}");
            return null;
        }
    }

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
            else
            {
                throw new Exception("Current lobby data did not contain the key 'RelayCode'.");
            }

            if (CurrentLobby.Data.ContainsKey("RelayHostId"))
            {
                RelayServiceManager.Instance.HostId = CurrentLobby.Data["RelayHostId"].Value;
            }
            else
            {
                throw new Exception("Current lobby data did not contain the key 'RelayHostId'.");
            }

            HeartBeatPingManager.Instance.StartKeepAliveTimer();

            var callbacks = LobbyCallBackManager.Instance.GetLobbyEventCallbacks();
            lobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(CurrentLobby.Id, callbacks);

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
        PlayerId = AuthenticationService.Instance.PlayerId;
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
            FirstPlayerIndex = firstPlayerIndex,
            tokenCount = 4
        };

        GameMenuNavigator.Instance.DisplayBoardPanel();

        GameManager.Instance.StartGame(gameParameters);
    }

    internal async Task DisconnectFromLobby()
    {
        if (CurrentLobby == null) return;
        await lobbyEvents.UnsubscribeAsync();
        lobbyEvents = null;

        HeartBeatPingManager.Instance.UnsubscribeToAll();

        if (CurrentLobby.Players.Count > 1)
        {
            if (PlayerId == RelayServiceManager.Instance.HostId)
            {
                var newHostId = CurrentLobby.Players.Find(p => p.Id != PlayerId).Id;
                Debug.Log($"Leaving lobby, transfering host to Player: {newHostId}.");

                var lobbyUpdate = new UpdateLobbyOptions()
                {
                    Data = new Dictionary<string, DataObject>()
                {
                    { "RelayHostId", new DataObject(DataObject.VisibilityOptions.Member, newHostId) }
                }
                };

                await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, lobbyUpdate);
            }

            await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id, PlayerId);
        }
        else
        {
            Debug.Log("Last player to leave, deleting lobby.");
            await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
        }
        CurrentLobby = null;
        NetworkManager.Singleton.Shutdown();
    }

    internal async Task ChangeHost()
    {
        Debug.Log("Starting new Relay server");
        try
        {
            NetworkManager.Singleton.Shutdown();
            var newJoinCode = await RelayServiceManager.Instance.StartRelayHosting(4);

            Debug.Log($"Updating Lobby with new relay join code: {newJoinCode}");
            var lobbyUpdate = new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, newJoinCode) },
                { "RelayHostId", new DataObject(DataObject.VisibilityOptions.Member, PlayerId) }
            }
            };

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, lobbyUpdate);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to change host: " + e.Message);
        }
    }
}