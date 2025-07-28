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

public class LobbyUIManager : NetworkBehaviour
{
    [SerializeField] private Button hostJoinButton;
    [SerializeField] private Button startGameButton;
    public TMP_InputField joinCodeInputField;
    public TMP_InputField playerNameInput;
    public TMP_Text LobbyCode;
    [SerializeField] private GameParametersSettings gameParametersSettings;

    private Lobby currentLobby;
    public List<SimplePlayerUI> PlayerLobbyUIs;

    public static LobbyUIManager Instance;
    private TMP_Text buttonText;

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
        buttonText = hostJoinButton.GetComponentInChildren<TMP_Text>();
        hostJoinButton.onClick.AddListener(CreateLobby);

        playerNameInput.onValueChanged.AddListener((value) =>
        {
            UpdateJoinStartButton();
        });

        joinCodeInputField.onValueChanged.AddListener((value) =>
        {
            UpdateJoinStartButton();
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

        NetworkManager.Singleton.OnClientConnectedCallback += (id) => UpdateGameParameters_ServerRpc();
    }

    public void SetupTimeAttackParameters()
    {
        gameParametersSettings.SetIsHost(NetworkManager.Singleton.IsHost);

        if (NetworkManager.Singleton.IsHost)
        {
            gameParametersSettings.OnSliderValueChanged += (value) =>
            {
                UpdateTimerSlider_ClientRpc(value);
                //await LobbyServiceManager.Instance.UpdateLobbyDataTimeLimitInSeconds(value);
            };
            gameParametersSettings.OnGameModeBoxChecked += (isChecked) =>
            {
                CheckTimeAttackMode_ClientRpc(isChecked);
                //await LobbyServiceManager.Instance.UpdateLobbyDataIsTimeAttackToggled(isChecked);
            };
            gameParametersSettings.OnSpawnWithTokenBoxChecked += (isChecked) =>
            {
                CheckSpawnWithToken_ClientRpc(isChecked);
                // This is not used in the lobby, but could be used to update the game settings
                // await LobbyServiceManager.Instance.UpdateLobbyDataSpawnWithTokensToggled(isChecked);
            };
        }
    }

    [ClientRpc]
    private void CheckTimeAttackMode_ClientRpc(bool isChecked)
    {
        if (IsHost) return;
        Debug.Log("CheckTimeAttackMode_ClientRpc called");
        gameParametersSettings.SetIsTimeAttackChecked(isChecked);
    }

    [ClientRpc]
    private void CheckSpawnWithToken_ClientRpc(bool isChecked)
    {
        if (IsHost) return;
        Debug.Log("CheckSpawnWithToken_ClientRpc called");
        gameParametersSettings.SetSpawnWithTokensChecked(isChecked);
    }

    [ClientRpc]
    private void UpdateTimerSlider_ClientRpc(int value)
    {
        if (IsHost) return;
        Debug.Log("UpdateTimerSlider_ClientRpc called");
        gameParametersSettings.SetTimerValue(value);
    }

    private void UpdateJoinStartButton()
    {
        hostJoinButton.onClick.RemoveAllListeners();
        hostJoinButton.interactable = !string.IsNullOrEmpty(playerNameInput.text);
        buttonText.text = "Create";
        if (!string.IsNullOrEmpty(joinCodeInputField.text))
        {
            buttonText.text = "Join";
            hostJoinButton.onClick.AddListener(JoinLobby);
        }
        else
        {
            hostJoinButton.onClick.AddListener(CreateLobby);
        }
    }

    public void UpdateLobbyInfo(Lobby lobby)
    {
        currentLobby = lobby;
    }

    public async void CreateLobby()
    {
        Debug.Log("Creating Lobby...");

        LudoPlayerInfo playerInfo = GetPlayerInfo();
        
        GameMenuNavigator.Instance.EnableSpinner();

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
        GameMenuNavigator.Instance.DisableSpinner();
    }

    public async void JoinLobby()
    {
        Debug.Log("Joining Lobby...");

        var playerInfo = GetPlayerInfo();

        string joinCode = joinCodeInputField.text;

        if (!string.IsNullOrEmpty(joinCode))
        {
            GameMenuNavigator.Instance.EnableSpinner();

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
            GameMenuNavigator.Instance.DisableSpinner();
        }
        else
        {
            Debug.LogError("Code is empty!");
        }
    }

    private void SwitchToLobbyScreen()
    {
        GameMenuNavigator.Instance.DisplayLobbyPanel();
        SetupTimeAttackParameters();

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

        if (!IsHost)
        {
            startGameButton.interactable = false;
        }

    }
    [ServerRpc(RequireOwnership = false)]
    public void UpdateGameParameters_ServerRpc()
    {
        CheckTimeAttackMode_ClientRpc(gameParametersSettings.GetGameMode() == GameMode.TimeAttack);
        UpdateTimerSlider_ClientRpc(gameParametersSettings.GetTimerValue());
        CheckSpawnWithToken_ClientRpc(gameParametersSettings.IsSpawnWithTokensChecked());
    }
    private LudoPlayerInfo GetPlayerInfo()
    {
        return new LudoPlayerInfo
        {
            ID = new(PlayerConfiguration.Instance.GetPlayerID()),
            AvatarID = 0,
            Name = new(playerNameInput.text)
        };
    }

    private void StartGame()
    {
        LobbyServiceManager.Instance.StartGame(gameParametersSettings.GetGameMode(), gameParametersSettings.GetTimerValue(), gameParametersSettings.IsSpawnWithTokensChecked());
    }

}