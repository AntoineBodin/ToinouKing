using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuNavigator : MonoBehaviour
{

    [Header("Panels")]
    public UIPanelAnimationManager Panel_Landing;
    public UIPanelAnimationManager Panel_Play;
    public UIPanelAnimationManager Panel_Play_Local;
    public UIPanelAnimationManager Panel_Play_Online;
    public UIPanelAnimationManager Panel_Lobby;
    public UIPanelAnimationManager Panel_Board;
    public UIPanelAnimationManager Panel_EndGame;
    private UIPanelAnimationManager currentPanel;
    private Action pannelToGoBackTo;

    [Header("Buttons")]
    public Button PlayButton;
    public Button PlayLocalButton;
    public Button PlayOnlineButton;
    public Button StartGameOnlineButton;
    public Button StartGameOfflineButton;
    public Button HeaderBackButton;

    [Header("Toggles")]
    public Toggle PlayWith2PlayersToggle;
    public Toggle PlayWith3PlayersToggle;
    public Toggle PlayWith4PlayersToggle;

    [Header("InputFields")]
    public TMP_InputField Player1NameInputField;
    public TMP_InputField Player2NameInputField;
    public TMP_InputField Player3NameInputField;
    public TMP_InputField Player4NameInputField;

    [Header("Elements")]
    public GameObject Spinner;


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
        currentPanel = Panel_Landing;
        SetupButtons();
        SetupToggles();
        SetupInputFieldsListeners();
    }

    #region BUTTONS

    private void SetupButtons()
    {
        PlayButton.onClick.AddListener(DisplayPlayPanel);
        PlayLocalButton.onClick.AddListener(DisplayPlayLocalPanel);
        PlayOnlineButton.onClick.AddListener(DisplayPlayOnlinePanel);
        HeaderBackButton.onClick.AddListener(DisplayPanelToGoBackTo);
        StartGameOfflineButton.onClick.AddListener(StartGameOffline);
    }

    private void DisplayLandingPanel()
    {
        currentPanel.HidePanel();
        Panel_Landing.ShowPanel();
        currentPanel = Panel_Landing;
        HeaderBackButton.gameObject.SetActive(false);
        pannelToGoBackTo = null;
    }

    private void DisplayPlayPanel()
    {
        currentPanel.HidePanel();
        Panel_Play.ShowPanel();
        currentPanel = Panel_Play;
        pannelToGoBackTo = DisplayLandingPanel;
        HeaderBackButton.gameObject.SetActive(true);
    }

    public void DisplayPlayLocalPanel()
    {
        currentPanel.HidePanel();
        Panel_Play_Local.ShowPanel();
        currentPanel = Panel_Play_Local;
        pannelToGoBackTo = DisplayPlayPanel;
        HeaderBackButton.gameObject.SetActive(true);
    }

    public void DisplayPlayOnlinePanel()
    {
        currentPanel.HidePanel();
        Panel_Play_Online.ShowPanel();
        currentPanel = Panel_Play_Online;
        pannelToGoBackTo = DisplayPlayPanel;
        HeaderBackButton.gameObject.SetActive(true);
    }


    public void DisplayLobbyPanel()
    {
        currentPanel.HidePanel();
        Panel_Lobby.ShowPanel();
        currentPanel = Panel_Lobby;
        pannelToGoBackTo = async () => await DisconnectFromLobbyBeforeGoingBack();
        HeaderBackButton.gameObject.SetActive(true);
        DisableSpinner();
    }

    public void DisplayBoardPanel()
    {
        currentPanel.HidePanel();
        Panel_Board.ShowPanel();
        currentPanel = Panel_Board;
        HeaderBackButton.gameObject.SetActive(false);
    }

    public void DisplayEndGamePannel()
    {
        currentPanel.ForceHidePanel();
        Panel_EndGame.ShowPanel();
        currentPanel = Panel_EndGame;
        pannelToGoBackTo = DisplayPlayPanel;
        HeaderBackButton.gameObject.SetActive(false);
    }

    public async Task DisconnectFromLobbyBeforeGoingBack()
    {
        await LobbyServiceManager.Instance.DisconnectFromLobby();
        DisplayPlayOnlinePanel();
    }

    private void DisplayPanelToGoBackTo()
    {
        pannelToGoBackTo.Invoke();
    }

    private void StartGameOffline()
    {
        DisplayBoardPanel();
        GameManager.Instance.StartGame(GetOfflineGameParameters());
    }

    #endregion

    #region TOGGLES

    private void SetupToggles()
    {
        PlayWith2PlayersToggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                SelectPlayWith2Players();
            }
        });

        PlayWith3PlayersToggle.onValueChanged.AddListener((bool isOn) =>
        {
            if (isOn)
            {
                SelectPlayWith3Players();
            }
        });

        PlayWith4PlayersToggle.onValueChanged.AddListener((bool isOn) =>
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
        Player3NameInputField.interactable = false;
        Player4NameInputField.interactable = false; 
    }

    private void SelectPlayWith3Players()
    {
        UnSelect2And4PlayersOptions();
        Player3NameInputField.interactable = true;
        Player4NameInputField.interactable = false;
    }

    private void SelectPlayWith4Players()
    {
        UnSelect2And3PlayersOptions();
        Player3NameInputField.interactable = true;
        Player4NameInputField.interactable = true;
    }

    private void UnSelect2And3PlayersOptions()
    {
        PlayWith2PlayersToggle.isOn = false;
        PlayWith3PlayersToggle.isOn = false;
    }

    private void UnSelect3And4PlayersOptions()
    {
        PlayWith3PlayersToggle.isOn = false;
        PlayWith4PlayersToggle.isOn = false;
    }

    private void UnSelect2And4PlayersOptions()
    {
        PlayWith2PlayersToggle.isOn = false;
        PlayWith4PlayersToggle.isOn = false;
    }

    #endregion

    private void SetupInputFieldsListeners()
    {
        Player1NameInputField.onValueChanged.AddListener(_ => CheckIfEmpty());
        Player2NameInputField.onValueChanged.AddListener(_ => CheckIfEmpty());
        Player3NameInputField.onValueChanged.AddListener(_ => CheckIfEmpty());
        Player4NameInputField.onValueChanged.AddListener(_ => CheckIfEmpty());
    }

    private void CheckIfEmpty()
    {
        StartGameOfflineButton.interactable = !string.IsNullOrEmpty(Player1NameInputField.text)
                                    && !string.IsNullOrEmpty(Player2NameInputField.text)
                                    && (!PlayWith3PlayersToggle.isOn || !string.IsNullOrEmpty(Player3NameInputField.text))
                                    && (!PlayWith4PlayersToggle.isOn || !string.IsNullOrEmpty(Player4NameInputField.text));
    }

    private GameParameters GetOfflineGameParameters()
    {
        return GameParametersManager.Instance.GetOfflineParameters(GetOfflinePlayerList());
    }

    private List<LudoPlayerInfo> GetOfflinePlayerList()
    {
        if (PlayWith2PlayersToggle.isOn)
        {
            return new List<LudoPlayerInfo>()
            {
                new()
                {
                    Name = Player1NameInputField.text,
                },
                new()
                {
                    Name = Player2NameInputField.text,
                }
            };
        }
        else if (PlayWith3PlayersToggle.isOn)
        {
            return new List<LudoPlayerInfo>()
                {
                new()
                {
                    Name = Player1NameInputField.text,
                },
                new()
                {
                    Name = Player2NameInputField.text,
                },
                new()
                {
                    Name = Player3NameInputField.text,
                }
            };
        }
        else if (PlayWith4PlayersToggle.isOn)
        {
            return new List<LudoPlayerInfo>()
                {
                new()
                {
                    Name = Player1NameInputField.text,
                },
                new()
                {
                    Name = Player2NameInputField.text,
                },
                new()
                {
                    Name = Player3NameInputField.text,
                },
                new()
                {
                    Name = Player4NameInputField.text,
                }
            };
        }
        else
        {
            Debug.LogError("No players selected");
            return new();
        }
    }

    public void EnableSpinner()
    {
        Spinner.SetActive(true);
    }

    public void DisableSpinner()
    {
        Spinner.SetActive(false);
    }
}
