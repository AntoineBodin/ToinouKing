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

public class LobbyManager2 : MonoBehaviour
{ 
    private Lobby currentLobby;
    private ILobbyEvents lobbyEvents;

    public static LobbyManager2 Instance;

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
            Debug.Log($"Le joueur {playerId} a quitté le lobby.");
        }
        UpdateLobbyUI();
    }

    // Données du lobby ont changé
    private void OnDataChanged(Dictionary<string, ChangedOrRemovedLobbyValue<DataObject>> data)
    {
        Debug.Log("Les données du lobby ont changé.");
        // Traite les changements de données ici
    }

    // Données des joueurs ont changé
    private void OnPlayerDataChanged(Dictionary<int, Dictionary<string, ChangedOrRemovedLobbyValue<PlayerDataObject>>> playerData)
    {
        Debug.Log("Les données des joueurs ont changé.");
        // Traite les changements de données des joueurs ici
    }

    // État de la connexion du service Lobby a changé
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
        if (lobbyEvents != null)
        {
            await lobbyEvents.UnsubscribeAsync();
        }
    }
}