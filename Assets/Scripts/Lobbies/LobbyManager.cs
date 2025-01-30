using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{

    [SerializeField]
    private List<SimplePlayerUI> playerLobbyUIs;
    private ILobbyEvents m_LobbyEvents;

    public static LobbyManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void StartGame()
    {
        List<LudoPlayerInfo> players = Multiplayer.Instance.CurrentLobby.Players.Select(p => new LudoPlayerInfo(p)).ToList();
        int firstPlayerIndex = Random.Range(0, players.Count);
        
        StartGame_ClientRpc(firstPlayerIndex);
    }

    /// <summary>
    /// Sends a request to the server to Update the UI for every client
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateUI_ServerRpc()
    {
        UpdateUI_ClientRpc();
    }

    /// <summary>
    /// Request the update of the UI on every client
    /// </summary>
    [ClientRpc]
    private void UpdateUI_ClientRpc()
    {
        Debug.Log("Update UI Client RPC called");
        UpdateUI();
    }

    /// <summary>
    /// Start the game on every client
    /// </summary>
    [ClientRpc]
    public void StartGame_ClientRpc(int firstPlayerIndex)
    {
        Debug.Log("Start Game Client RPC called");
        List<LudoPlayerInfo> players = Multiplayer.Instance.CurrentLobby.Players.Select(p => new LudoPlayerInfo(p)).ToList();

        GameParameters gameParameters = new()
        {
            Players = players,
            IsOnline = true,
            FirstPlayerIndex = firstPlayerIndex
        };

        GameMenuNavigator.Instance.DisplayBoardCanvas();
        players.ForEach(p => Debug.Log(p.Name + "is in the lobby"));
        GameManager.Instance.StartGame(gameParameters);
    }


    /// <summary>
    /// Update the UI locally
    /// </summary>
    public void UpdateUI()
    { 
        int index = 0;
        Debug.Log("Update UI with " + Multiplayer.Instance.CurrentLobby.Players.Count + " players.");
        Multiplayer.Instance.CurrentLobby.Players.ForEach(player =>
        {
            LudoPlayerInfo playerInfo = new(player);
            playerLobbyUIs[index].SetPlayerInfo(playerInfo);
            playerLobbyUIs[index].UpdateUI();
            index++;
        });
        for (int i = index; i < playerLobbyUIs.Count; i++)
        {
            playerLobbyUIs[i].Clear();
        }
    }
    /// <summary>
    /// Subscribes to lobby events for the specified lobby and sets up event callbacks.
    /// </summary>
    /// <param name="lobbyId">The ID of the lobby to subscribe to events for.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="LobbyServiceException">Thrown if an error occurs during subscription.</exception>
    public async Task SubscribeToLobbyEventsAsync(string lobbyId)
    {
        // Set up the event callbacks
        var callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        callbacks.KickedFromLobby += OnKickedFromLobby;
        callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

        try
        {
            // Attempt to subscribe to lobby events
            m_LobbyEvents = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, callbacks);
        }
        catch (LobbyServiceException ex)
        {
            // Handle specific lobby service exceptions
            switch (ex.Reason)
            {
                case LobbyExceptionReason.AlreadySubscribedToLobby:
                    Debug.LogWarning($"Already subscribed to lobby[{lobbyId}]. No additional subscription required. Exception Message: {ex.Message}");
                    break;

                case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
                    Debug.LogError($"Subscription to lobby events was lost while attempting to subscribe. Exception Message: {ex.Message}");
                    throw;

                case LobbyExceptionReason.LobbyEventServiceConnectionError:
                    Debug.LogError($"Failed to connect to lobby events. Exception Message: {ex.Message}");
                    throw;

                default:
                    throw;
            }
        }
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed: /* Update the UI if necessary, as the subscription has been stopped. */ break;
            case LobbyEventConnectionState.Subscribing: /* Update the UI if necessary, while waiting to be subscribed. */ break;
            case LobbyEventConnectionState.Subscribed: /* Update the UI if necessary, to show subscription is working. */ break;
            case LobbyEventConnectionState.Unsynced: /* Update the UI to show connection problems. Lobby will attempt to reconnect automatically. */ break;
            case LobbyEventConnectionState.Error: /* Update the UI to show the connection has errored. Lobby will not attempt to reconnect as something has gone wrong. */break;
        }
    }

    private void OnKickedFromLobby()
    {
        // These events will never trigger again, so let’s remove it.
        m_LobbyEvents = null;
        UpdateUI();
    }

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        Debug.Log("Lobby changed !");
        if (changes.LobbyDeleted)
        {
            // Handle lobby being deleted 
            // Calling changes.ApplyToLobby will log a warning and do nothing
        }
        else
        {
            changes.ApplyToLobby(Multiplayer.Instance.CurrentLobby);
        }
        UpdateUI();
        //TEST
    }
}
