using Assets.Scripts;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuNavigator : MonoBehaviour
{

    [Header("Panels")]
    public GameObject LandingPanel;
    public GameObject PlayPanel;
    public GameObject PanelPlay_Local;
    public GameObject PanelPlay_Online;

    [Header("Canvases")]
    public Canvas LobbyCanvas;
    public Canvas GameMenuCanvas;
    public Canvas BoardCanvas;
    public Canvas EndGameCanvas;
    private Canvas currentCanvas;

    [Header("Buttons")]
    public Button PlayButton;
    public Button PlayLocalButton;
    public Button PlayOnlineButton;
    public Button StartGameOnlineButton;
    public Button StartGameOfflineButton;

    [Header("Toggles")]
    public Toggle PlayWith2Players;
    public Toggle PlayWith3Players;
    public Toggle PlayWith4Players;

    [Header("InputFields")]
    public TMP_InputField Player1Name;
    public TMP_InputField Player2Name;
    public TMP_InputField Player3Name;
    public TMP_InputField Player4Name;
    public TMP_InputField OnlinePlayerName;

    public static GameMenuNavigator Instance;

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
        currentCanvas = GameMenuCanvas;
        SetupToggles();
        SetupButtons();
    }

    #region BUTTONS

    private void SetupButtons()
    {
        PlayButton.onClick.AddListener(DisplayPlayPanel);
        PlayLocalButton.onClick.AddListener(DisplayPlayLocalPanel);
        PlayOnlineButton.onClick.AddListener(DisplayPlayOnlinePanel);
        
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

    public void DisplayEndGamePanel()
    {
        currentCanvas.enabled = false;
        EndGameCanvas.enabled = true;
        currentCanvas = EndGameCanvas;
    }

    public void DisplayLobbyCanvas()
    {
        currentCanvas.enabled = false;
        LobbyCanvas.enabled = true;
        currentCanvas = LobbyCanvas;
    }

    private void StartGameOffline()
    {
        DisplayBoardCanvas();
        GameManager.Instance.StartGame(GetOfflineGameParameters());
    }

    public void DisplayBoardCanvas()
    {
        currentCanvas.enabled = false;
        BoardCanvas.enabled = true;
        currentCanvas = BoardCanvas;
    }

    public void DisplayGameMenuCanvas()
    {
        currentCanvas.enabled = false;
        GameMenuCanvas.enabled = true;
        currentCanvas = GameMenuCanvas;
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
        return GameParametersManager.Instance.GetOfflineParameters(GetOfflinePlayerList());

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
