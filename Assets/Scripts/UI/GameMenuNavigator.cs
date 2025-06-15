using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuNavigator : MonoBehaviour
{
    [Header("Offline Elements")]
    public Transform PlayersRow1;
    public Transform PlayersRow2;
    public GameObject Player1;
    public GameObject Player2;
    public GameObject Player3;
    public GameObject Player4;
    public GameObject AddPlayerPlaceHolder;
    public Button PlayOfflineButton;
    private Button AddPlayerButton, RemovePlayer3Button, RemovePlayer4Button;
    private TMP_InputField Player1NameInputField, Player2NameInputField, Player3NameInputField, Player4NameInputField;

    [Header("Buttons")]
    public Button PlayButton;
    public Button BackButton;

    [Header("Elements")]
    public GameObject Spinner;

    [Header("Switches")]
    public Slider OnlineLocalSwitch;
    public Animator PlayPanelAnimator;

    public static event Action<bool> OnOnlineLocalSwitched;
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
        SetupButtons();
        SetupOfflineInputFields();
        OnlineLocalSwitch.onValueChanged.AddListener((i) =>
        {
            PlayPanelAnimator.SetTrigger("Switch");
            PlayPanelAnimator.SetBool("PlayedOnline", false);

            if (i == 0)
            {
                OnOnlineLocalSwitched?.Invoke(false);
            }
            else if (i == 1)
            {
                OnOnlineLocalSwitched?.Invoke(true);
            }
            else
            {
                Debug.LogError("Invalid value for OnlineLocalSwitch: " + i);
            }
        });
    }

    public void DeletePlayer3()
    {
        Player3NameInputField.text = string.Empty;
        Player3.SetActive(false);

        if (AddPlayerPlaceHolder.activeSelf == false)
        {
            AddPlayerPlaceHolder.SetActive(true);
        }

        SetStartOfflineButtonInteractable();
    }

    public void DeletePlayer4()
    {
        Player4NameInputField.text = string.Empty;
        Player4.SetActive(false);

        if (AddPlayerPlaceHolder.activeSelf == false)
        {
            AddPlayerPlaceHolder.SetActive(true);
        }
        
        SetStartOfflineButtonInteractable();
    }

    private void AddPlayer()
    {
        if (Player3.activeSelf && Player4.activeSelf)
        {
            Debug.LogWarning("Cannot add more players, already at maximum.");
            return;
        }
        if (!Player3.activeSelf)
        {
            Player3.SetActive(true);
            Player3NameInputField.text = string.Empty;
        }
        else if (!Player4.activeSelf)
        {
            Player4.SetActive(true);
            Player4NameInputField.text = string.Empty;
        }

        if (Player3.activeSelf && Player4.activeSelf)
        {
            AddPlayerPlaceHolder.SetActive(false);
        }

        SetStartOfflineButtonInteractable();
    }

    public void DisplayBoardPanel()
    {
        BackButton.gameObject.SetActive(false);
        PlayPanelAnimator.SetTrigger("SwitchToBoard");
    }

    #region BUTTONS

    private void SetupButtons()
    {
        AddPlayerButton = AddPlayerPlaceHolder.GetComponentInChildren<Button>();
        RemovePlayer3Button = Player3.GetComponentInChildren<Button>(true);
        RemovePlayer4Button = Player4.GetComponentInChildren<Button>(true);

        RemovePlayer3Button.onClick.AddListener(DeletePlayer3);
        RemovePlayer4Button.onClick.AddListener(DeletePlayer4);
        AddPlayerButton.onClick.AddListener(AddPlayer);
        BackButton.onClick.AddListener(async () => await BackButtonAction());
        PlayOfflineButton.onClick.AddListener(StartGameOffline);
        PlayButton.onClick.AddListener(StartUpGameScreen);

    }
    private void StartUpGameScreen()
    {
        PlayPanelAnimator.SetTrigger("Start");
        BackButton.gameObject.SetActive(true);
    }

    private async Task BackButtonAction()
    {
        await DisconnectFromLobbyBeforeGoingBack();
        PlayPanelAnimator.SetTrigger("Back");
    }

    public void DisplayEndGamePannel()
    {
        PlayPanelAnimator.SetTrigger("SwitchToResults");
        BackButton.gameObject.SetActive(true);
    }

    public async Task DisconnectFromLobbyBeforeGoingBack()
    {
        await LobbyServiceManager.Instance.DisconnectFromLobby();
        //DisplayPlayPanel();
    }

    private void StartGameOffline()
    {
        DisplayBoardPanel();
        Debug.Log("Starting offline game with parameters: " + GetOfflineGameParameters());
        StartCoroutine(StartGameAfter2Seconds());
    }

    private IEnumerator StartGameAfter2Seconds()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.StartGame(GetOfflineGameParameters());
    }

    #endregion


    private void SetupOfflineInputFields()
    {
        Player1NameInputField = Player1.GetComponentInChildren<TMP_InputField>(true);
        Player2NameInputField = Player2.GetComponentInChildren<TMP_InputField>(true);
        Player3NameInputField = Player3.GetComponentInChildren<TMP_InputField>(true);
        Player4NameInputField = Player4.GetComponentInChildren<TMP_InputField>(true);

        Player1NameInputField.onValueChanged.AddListener(_ => SetStartOfflineButtonInteractable());
        Player2NameInputField.onValueChanged.AddListener(_ => SetStartOfflineButtonInteractable());
        Player3NameInputField.onValueChanged.AddListener(_ => SetStartOfflineButtonInteractable());
        Player4NameInputField.onValueChanged.AddListener(_ => SetStartOfflineButtonInteractable());
    }

    private void SetStartOfflineButtonInteractable()
    {
        bool canStartGame = true;

        bool canPlayer1Start = !string.IsNullOrEmpty(Player1NameInputField.text);
        canStartGame &= canPlayer1Start;
        bool canPlayer2Start = !string.IsNullOrEmpty(Player2NameInputField.text);
        canStartGame &= canPlayer2Start;
        bool canPlayer3Start = !Player3.activeSelf || !string.IsNullOrEmpty(Player3NameInputField.text);
        canStartGame &= canPlayer3Start;
        bool canPlayer4Start = !Player4.activeSelf || !string.IsNullOrEmpty(Player4NameInputField.text);
        canStartGame &= canPlayer4Start;

        PlayOfflineButton.interactable = canStartGame;
    }

    private GameParameters GetOfflineGameParameters()
    {
        return GameParametersManager.Instance.GetOfflineParameters(GetOfflinePlayerList());
    }

    private List<LudoPlayerInfo> GetOfflinePlayerList()
    {
        List<LudoPlayerInfo> playerList = new();

        if (Player1.activeSelf && !string.IsNullOrEmpty(Player1NameInputField.text))
        {
            playerList.Add(new LudoPlayerInfo() { Name = Player1NameInputField.text });
        }
        if (Player2.activeSelf && !string.IsNullOrEmpty(Player2NameInputField.text))
        {
            playerList.Add(new LudoPlayerInfo() { Name = Player2NameInputField.text });
        }
        if (Player3.activeSelf && !string.IsNullOrEmpty(Player3NameInputField.text))
        {
            playerList.Add(new LudoPlayerInfo() { Name = Player3NameInputField.text });
        }
        if (Player4.activeSelf && !string.IsNullOrEmpty(Player4NameInputField.text))
        {
            playerList.Add(new LudoPlayerInfo() { Name = Player4NameInputField.text });
        }

        return playerList; 
    }

    public void EnableSpinner()
    {
        Spinner.SetActive(true);
    }

    public void DisableSpinner()
    {
        Spinner.SetActive(false);
    }

    internal void DisplayLobbyPanel()
    {
        PlayPanelAnimator.SetTrigger("SwitchToLobby");
        PlayPanelAnimator.SetBool("PlayedOnline", true);
    }

    internal void GoBackFromResult()
    {
        PlayPanelAnimator.SetTrigger("SwitchBackFromResults");
    }

    public void ShowPlayer(int playerIndex)
    {
        PlayPanelAnimator.SetTrigger($"ShowPlayer{playerIndex + 1}");
    }
}
