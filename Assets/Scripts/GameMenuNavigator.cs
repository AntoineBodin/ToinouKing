using Assets.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuNavigator : MonoBehaviour
{
    public GameObject LandingPanel;
    public GameObject PlayPanel;
    public GameObject PanelPlay_Local;
    public GameObject PanelPlay_Online;

    public Canvas LobbyCanvas;
    public Canvas GameMenuCanvas;
    public Canvas boardCanvas;

    public Button PlayButton;
    public Button PlayLocalButton;
    public Button PlayOnlineButton;
    public Button HostGameButton;
    public Button JoinGameButton;
    public Button StartGameOnlineButton;
    public Button StartGameOfflineButton;

    public Toggle PlayWith2Players;
    public Toggle PlayWith3Players;
    public Toggle PlayWith4Players;

    public TMP_InputField Player1Name;
    public TMP_InputField Player2Name;
    public TMP_InputField Player3Name;
    public TMP_InputField Player4Name;

    public TMP_InputField OnlinePlayerName;

    public NetworkManager NetworkManager;
    public LobbyManager LobbyManager;
    public GameManager gameManager;

    public static GameMenuNavigator Instance;
    private void Start()
    {
        Instance = this;
        SetupToggles();
        SetupButtons();
    }

    #region BUTTONS

    private void SetupButtons()
    {
        PlayButton.onClick.AddListener(DisplayPlayPanel);
        PlayLocalButton.onClick.AddListener(DisplayPlayLocalPanel);
        PlayOnlineButton.onClick.AddListener(DisplayPlayOnlinePanel);
        HostGameButton.onClick.AddListener(HostGame);
        JoinGameButton.onClick.AddListener(QuickJoinGame);
        StartGameOnlineButton.onClick.AddListener(StartGameOnline);
        StartGameOfflineButton.onClick.AddListener(StartGameOffline);
    }

    private void DisplayPlayPanel()
    {
        PlayPanel.SetActive(true);
        LandingPanel.SetActive(false);
    }

    private void DisplayPlayLocalPanel()
    {
        PanelPlay_Local.SetActive(true);
        PlayPanel.SetActive(false);
    }

    private void DisplayPlayOnlinePanel()
    {
        PanelPlay_Online.SetActive(true);
        PlayPanel.SetActive(false);
    }

    private async void HostGame()
    {
        LudoPlayerInfo playerInfo = new()
        {
            ID = new(PlayerConfiguration.Instance.PlayerID),
            AvatarID = 0,
            Name = new(OnlinePlayerName.text),
        };

        var joinCode = await Multiplayer.Instance.CreateLobby(playerInfo);
        DisplayLobbyCanvas();
    }

    private void DisplayLobbyCanvas()
    {
        LobbyCanvas.enabled = true;
        GameMenuCanvas.enabled = false;
    }

    private async void QuickJoinGame()
    {
        LudoPlayerInfo playerInfo = new()
        {
            ID = new(PlayerConfiguration.Instance.PlayerID),
            AvatarID = 0,
            Name = new(OnlinePlayerName.text),
        };

        await Multiplayer.Instance.QuickJoinLobby(playerInfo);

        DisplayLobbyCanvas();
    }


    private void StartGameOnline()
    {
        LobbyManager.StartGame();
    }

    private void StartGameOffline()
    {
        DisplayBoardCanvas();
        GameManager.Instance.StartGame(GetOfflineGameParameters());
    }

    private void StartGameLocal() { }

    public void DisplayBoardCanvas()
    {
        GameMenuCanvas.enabled = false;
        LobbyCanvas.enabled = false;
        boardCanvas.enabled = true;
    }

    #endregion

    #region TOGGLES

    private void SetupToggles()
    {
        PlayWith2Players.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                SelectPlayWith2Players();
            }
        });

        PlayWith3Players.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                SelectPlayWith3Players();
            }
        });

        PlayWith4Players.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                SelectPlayWith4Players();
            }
        });
    }

    private void SelectPlayWith2Players()
    {
        UnSelect3And4PlayersOptions();
        Player3Name.interactable = false;
        Player4Name.interactable = false;
    }

    private void SelectPlayWith3Players()
    {
        UnSelect2And4PlayersOptions();
        Player3Name.interactable = true;
        Player4Name.interactable = false;
    }

    private void SelectPlayWith4Players()
    {
        UnSelect2And3PlayersOptions();
        Player3Name.interactable = true;
        Player4Name.interactable = true;
    }

    private void UnSelect2And3PlayersOptions()
    {
        PlayWith2Players.isOn = false;
        PlayWith3Players.isOn = false;
    }

    private void UnSelect3And4PlayersOptions()
    {
        PlayWith3Players.isOn = false;
        PlayWith4Players.isOn = false;
    }

    private void UnSelect2And4PlayersOptions()
    {
        PlayWith2Players.isOn = false;
        PlayWith4Players.isOn = false;
    }

    #endregion

    private GameParameters GetOfflineGameParameters()
    {
        return new GameParameters()
        {
            IsOnline = false,
            Players = GetOfflinePlayerList(),
            FirstPlayerIndex = 0,
        };
    }

    private List<LudoPlayerInfo> GetOfflinePlayerList()
    {
        if (PlayWith2Players.isOn)
        {
            return new List<LudoPlayerInfo>()
            {
                new()
                {
                    Name = Player1Name.text,
                },
                new()
                {
                    Name = Player2Name.text,
                }
            };
        }
        else if (PlayWith3Players.isOn)
        {
            return new List<LudoPlayerInfo>()
                {
                new()
                {
                    Name = Player1Name.text,
                },
                new()
                {
                    Name = Player2Name.text,
                },
                new()
                {
                    Name = Player3Name.text,
                }
            };
        }
        else if (PlayWith4Players.isOn)
        {
            return new List<LudoPlayerInfo>()
                {
                new()
                {
                    Name = Player1Name.text,
                },
                new()
                {
                    Name = Player2Name.text,
                },
                new()
                {
                    Name = Player3Name.text,
                },
                new()
                {
                    Name = Player4Name.text,
                }
            };
        }
        else
        {
            Debug.LogError("No players selected");
            return new();
        }
    }
}
