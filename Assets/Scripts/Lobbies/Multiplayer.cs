using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;

public enum EncryptionType
{
    WSS,
    DTLS
}
public class Multiplayer : MonoBehaviour
{
    [SerializeField] private string lobbyName;
    [SerializeField] private int maxPlayers;
    [SerializeField] private EncryptionType encryption = EncryptionType.WSS;
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private GameManager gameManager;

    private string connectionType => 
        encryption == EncryptionType.WSS ? k_wssEncryptionTypeString : k_dtlsEncryptionTypeString;
    
    public static Multiplayer Instance { get; private set; }
    public Lobby CurrentLobby;

    const float k_lobbyhHeartbeatInterval = 20f;
    const float k_lobbyPollForUpdatesInterval = 65f;
    const float k_clientKeepAliveInterval = 8f;
    const string k_relayJoinCodeKey = "RelayJoinCode";
    const string k_wssEncryptionTypeString = "wss";
    const string k_dtlsEncryptionTypeString = "dtls";

    private CountdownTimer heartbeatTimer = new CountdownTimer(k_lobbyhHeartbeatInterval);
    private CountdownTimer pollForUpdatesTimer = new CountdownTimer(k_lobbyPollForUpdatesInterval);
    private CountdownTimer keepAliveTimer = new CountdownTimer(k_clientKeepAliveInterval);
    private Action heartBeatAction;
    private Action keepAliveAction;
    //private Action lobbyPollingAction;


    void Start()
    {
        Instance = this;
        heartBeatAction = HeartBeatAction();
        keepAliveAction = KeepAliveAction();
        
/*        pollForUpdatesTimer.OnTimerStop += () =>
        {
            //Task.Run(() => HandlePollForUpdatesAsync());
            pollForUpdatesTimer.Start();
        };*/
    }

    private Action HeartBeatAction()
    {
        return () =>
        {
            Task.Run(() => HandleHeartBeatAsync());
            heartbeatTimer.Start();
        };
    }

    private void SetupHeartbeat()
    {
        heartbeatTimer.OnTimerStop += heartBeatAction;
    }
    private async Task HandleHeartBeatAsync()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(CurrentLobby.Id);
            Debug.Log("Send heartbeat ping to lobby: " + CurrentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to heartbeat lobby: " + e.Message);
        }
    }

    private Action KeepAliveAction()
    {
        return () =>
        {
            HandleKeepAlive();
            keepAliveTimer.Start();
        };
    }

    private void SetupKeepAlive()
    {
        keepAliveTimer.OnTimerStop += keepAliveAction;
    }

    private void HandleKeepAlive()
    {
        try 
        {
            Debug.Log("Ping keep-alive");
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Ping", NetworkManager.ServerClientId, new FastBufferWriter(0, Allocator.Temp));
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to send keepAlive ping: " + e.Message);
        }
    }

    private void UnsubscribeHeartbeat()
    {
        heartbeatTimer.OnTimerStop -= heartBeatAction;

    }

    private void Update()
    {
        heartbeatTimer.Tick(Time.deltaTime);
        pollForUpdatesTimer.Tick(Time.deltaTime);
        if (NetworkManager.Singleton.IsClient)
        {
            keepAliveTimer.Tick(Time.deltaTime);
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

    public async Task<string> CreateLobby(LudoPlayerInfo playerInfo)
    {
        try
        {
            await Authenticate();
            Allocation allocation = await AllocateRelay();
            string relayJoinCode = await GetRelayJoinCode(allocation);

            CreateLobbyOptions options = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = playerInfo.GetPlayerData()
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            Debug.Log("Created Lobby: " + CurrentLobby.Name + " with code: " + CurrentLobby.LobbyCode);

            SetupHeartbeat();

            heartbeatTimer.Start();
            pollForUpdatesTimer.Start();

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, new UpdateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { k_relayJoinCodeKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
            NetworkManager.Singleton.StartHost();


            await lobbyManager.SubscribeToLobbyEventsAsync(CurrentLobby.Id);

            gameManager.OnlinePlayerIdentity = new LudoPlayerInfo(options.Player);
            lobbyManager.UpdateUI_ServerRpc();

            return relayJoinCode;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to create lobby:" + e.Message);
            return null;
        }
    }

    public async Task QuickJoinLobby(LudoPlayerInfo playerInfo)
    {
        try
        {
            await Authenticate();


            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions()
            {
                Player = playerInfo.GetPlayerData()
            };

            CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(options);
            pollForUpdatesTimer.Start();
            
            await lobbyManager.SubscribeToLobbyEventsAsync(CurrentLobby.Id);


            string relayJoinCode = CurrentLobby.Data[k_relayJoinCodeKey].Value;

            await JoinLobbyWithCode(relayJoinCode, options);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join lobby:" + e.Message);
        }
    }

    public async Task JoinLobbyWithCode(string relayJoinCode, QuickJoinLobbyOptions options)
    {
        try
        {
            await Authenticate();


            SubscribeToConnectionEvent(options);

            JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, connectionType));

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join lobby with code " + relayJoinCode + ". message: " + e.Message);
        }
    }

    private void SubscribeToConnectionEvent(QuickJoinLobbyOptions options)
    {
        if (!NetworkManager.Singleton.IsClient) return;
        NetworkManager.Singleton.OnConnectionEvent += (networkManager, connectionEventData) =>
        {
            Debug.Log("Connection event of type '" + connectionEventData.EventType + "' detected: " + connectionEventData.ClientId + " LocalClientId: " + NetworkManager.Singleton.LocalClientId);
            if (connectionEventData.ClientId == NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("Client connected with ID " + connectionEventData.ClientId);
                SetupKeepAlive();
                keepAliveTimer.Start();

                lobbyManager.UpdateUI_ServerRpc();
                lobbyManager.UpdateUI();
                gameManager.OnlinePlayerIdentity = new LudoPlayerInfo(options.Player);
            }
            else
            {
                Debug.Log("Connexion event detected: " + connectionEventData.ClientId);
            }
        };
    }

    private async Task WaitForClientConnection()
    {
        var isConnected = false;
        Debug.Log("Waiting for client connection...");
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                isConnected = true;
                Debug.Log("OnClientConnectedCallback triggered!");
            }
        };

        while (!isConnected)
        {
            Debug.Log($"IsConnected: {isConnected}");
            await Task.Delay(100);
        }
        Debug.Log("Client connected!");
    }

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            return allocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to allocate relay :" + e.Message);
            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            return relayJoinCode;
        }
        catch (RelayServiceException e) 
        {
            Debug.LogError("Failed to get relay join code:" + e.Message);
            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string relayJoinCode)
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            return joinAllocation;
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Failed to join relay:" + e.Message);
            return default;
        }
    }

    public async Task HandlePollForUpdatesAsync()
    {
        try
        {
            await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id);
            Debug.Log("Poled for updates on lobby: " + CurrentLobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Failed to poll for updates on lobby: " + e.Message);
        }
    }

}
