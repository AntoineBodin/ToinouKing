using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.VisualScripting;
using System.Collections.Generic;
using Assets.Scripts;
using TMPro;
using Unity.Netcode;

public class LobbyUIManager : MonoBehaviour
{
    public Button hostJoinButton;
    public Button startGameButton;
    public TMP_InputField joinCodeInputField;
    public TMP_InputField playerName;
    public TMP_Text LobbyCode;

    private Lobby currentLobby;
    public List<SimplePlayerUI> PlayerLobbyUIs;

    public static LobbyUIManager Instance;

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

    private void Start()
    {
        var buttonText = hostJoinButton.GetComponentInChildren<TMP_Text>();
        hostJoinButton.onClick.AddListener(CreateLobby);
        buttonText.text = "CREATE";

        playerName.onValueChanged.AddListener((value) =>
        {
            hostJoinButton.interactable = !string.IsNullOrEmpty(value);
        });

        joinCodeInputField.onValueChanged.AddListener((value) =>
        {
            buttonText.text = string.IsNullOrEmpty(value) ? "CREATE" : "JOIN";
            hostJoinButton.onClick.RemoveAllListeners();
            if (string.IsNullOrEmpty(value))
            {
                hostJoinButton.onClick.AddListener(CreateLobby);
            }
            else
            {
                hostJoinButton.onClick.AddListener(JoinLobby);
            }
        });

        startGameButton.onClick.AddListener(() => {

            if (NetworkManager.Singleton.IsHost)
            {
                if (currentLobby.Players.Count < 2)
                {
                    Debug.Log("Not enough players.");
                }
                else
                {
                    StartGame();
                }
            }
            else
            {
                Debug.Log("Only host can start the game.");
            }
        });
    }

    public void UpdateLobbyInfo(Lobby lobby)
    {
        currentLobby = lobby;
    }

    public async void CreateLobby()
    {
        Debug.Log("Creating Lobby...");

        LudoPlayerInfo playerInfo = GetPlayerInfo();

        currentLobby = await LobbyServiceManager.Instance.CreateLobbyAsync("New Lobby", 4, playerInfo);
        if (currentLobby != null)
        {
            SwitchToLobbyScreen();
            Debug.Log("Successfully created lobby!");
        }
        else
        {
            Debug.LogError("Failed to create lobby.");

        }
    }

    public async void JoinLobby()
    {
        Debug.Log("Joining Lobby...");

        var playerInfo = GetPlayerInfo();

        string joinCode = joinCodeInputField.text;

        if (!string.IsNullOrEmpty(joinCode))
        {
            var lobby = await LobbyServiceManager.Instance.JoinLobbyAsync(joinCode, playerInfo);
            
            if (lobby != null)
            {
                currentLobby = lobby;
                Debug.Log("Successfully joined lobby!");
                SwitchToLobbyScreen();
            }
            else
            {
                Debug.LogError("Failed to join lobby.");
            }
        }
        else
        {
            Debug.LogError("Code is empty!");
        }
    }

    private void SwitchToLobbyScreen()
    {
        GameMenuNavigator.Instance.DisplayLobbyCanvas();
        UpdateUI();
    }

    public void UpdateUI()
    {
        LobbyCode.text = currentLobby.LobbyCode;
        int index = 0;
        Debug.Log("Update UI with " + currentLobby.Players.Count + " players.");
        currentLobby.Players.ForEach(player =>
        {
            LudoPlayerInfo playerInfo = new(player);
            PlayerLobbyUIs[index].SetPlayerInfo(playerInfo);
            PlayerLobbyUIs[index].UpdateUI();
            index++;
        });
        for (int i = index; i < PlayerLobbyUIs.Count; i++)
        {
            PlayerLobbyUIs[i].Clear();
        }
    }

    private LudoPlayerInfo GetPlayerInfo()
    {
        return new LudoPlayerInfo
        {
            ID = new(PlayerConfiguration.Instance.PlayerID),
            AvatarID = 0,
            Name = new(playerName.text)
        };
    }

    private void StartGame()
    {
        LobbyServiceManager.Instance.StartGame();
    }

}