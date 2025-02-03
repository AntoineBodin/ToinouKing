using System;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Unity.Services.Core;
using System.Threading.Tasks;
using Assets.Scripts;
using System.Linq;
using Unity.Services.Authentication;

public class LobbyCallBackManager : MonoBehaviour
{
    public static LobbyCallBackManager Instance;

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


    public LobbyEventCallbacks GetLobbyEventCallbacks()
    {
        LobbyEventCallbacks callbacks = new LobbyEventCallbacks();
        callbacks.LobbyChanged += OnLobbyChanged;
        callbacks.PlayerJoined += OnPlayerJoined;
        callbacks.PlayerLeft += OnPlayerLeft;
        callbacks.DataChanged += OnDataChanged;
        callbacks.PlayerDataChanged += OnPlayerDataChanged;
        callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

        return callbacks;
    }

    // Gestion des changements dans le lobby
    private void OnLobbyChanged(ILobbyChanges changes)
    {
        Debug.Log("Changes to lobby detected");
        changes.ApplyToLobby(LobbyServiceManager.Instance.CurrentLobby);
        UpdateLobbyUI();
    }

    // Un joueur rejoint le lobby
    private void OnPlayerJoined(List<LobbyPlayerJoined> players)
    {
        foreach (var player in players)
        {
            Debug.Log($"{player.Player.Id} a rejoint le lobby.");
        }
        UpdateLobbyUI();
    }

    // Un joueur quitte le lobby
    private void OnPlayerLeft(List<int> playerIds)
    {
        foreach (var playerId in playerIds)
        {
            Debug.Log($"Player {playerId} disconnected.");
        }
        UpdateLobbyUI();
    }

    private async void OnDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> data)
    {
        Debug.Log("Lobby data changed.");

        if (data.TryGetValue("RelayHostId", out ChangedOrRemovedLobbyValue<DataObject> newRelayHostIdLobbyData))
        {
            var newRelayHostId = newRelayHostIdLobbyData.Value.Value;
            Debug.Log($"Detected change in hostId : {newRelayHostId}.");
            if (LobbyServiceManager.Instance.PlayerId == newRelayHostId)
            {
                Debug.Log("Host => Create new server !");
                await LobbyServiceManager.Instance.ChangeHost();
            }
            RelayServiceManager.Instance.HostId = newRelayHostId;
        }

        if (data.TryGetValue("RelayCode", out ChangedOrRemovedLobbyValue<DataObject> newRelayJoinCodeLobbyData))
        {
            var newRelayJoinCode = newRelayJoinCodeLobbyData.Value.Value;
            Debug.Log($"Detected change in relay join code : {newRelayJoinCode}.");
            if (LobbyServiceManager.Instance.PlayerId != RelayServiceManager.Instance.HostId)
            {
                Debug.Log("Client => Join new server !");
                await RelayServiceManager.Instance.JoinNewRelayServer(newRelayJoinCode);
            }
            else
            {
                Debug.Log("Host does not need to join new relay server.");
            }
        }
    }

    private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> playerData)
    {
        Debug.Log("Les données des joueurs ont changé.");
        // Traite les changements de données des joueurs ici
    }

    private void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
    {
        Debug.Log($"L'état de la connexion au lobby a changé : {state}");
    }

    // Mise à jour de l'UI avec les informations du lobby
    private void UpdateLobbyUI()
    {
        LobbyUIManager.Instance.UpdateUI();
    }

    private async void OnDestroy()
    {
        // Désabonnement des événements
        /*
        if (lobbyEvents != null)
        {
            await lobbyEvents.UnsubscribeAsync();
        }*/
    }
}