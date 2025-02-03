using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class EndGameUIManager : MonoBehaviour
{
    [SerializeField] private List<SimplePlayerUI> playersUI;

    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text backButtonText;

    public static EndGameUIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateUI(List<LudoPlayer> players)
    {
        SetPlayers(players);
        backButton.onClick.RemoveAllListeners();
        backButtonText.text = GameManager.Instance.IsOnline ? "Back to Lobby" : "Play Again";
        backButton.onClick.AddListener(GameManager.Instance.IsOnline ? BackToLobbyAction : PlayAgainAction);
    }

    private void SetPlayers(List<LudoPlayer> players)
    {
        int playerUIIndex = 0;
        players.OrderBy(p => p.Rank).ToList().ForEach(p =>
            {
                playersUI[playerUIIndex].SetPlayerInfo(p.PlayerInfo);
                playersUI[playerUIIndex].UpdateUI();
                playerUIIndex++;
            });
        for (int i = playerUIIndex; i < 4; i++)
        {
            playersUI[i].Clear();
        }
    }

    private void BackToLobbyAction()
    {
        GameMenuNavigator.Instance.DisplayLobbyCanvas();
    }

    private void PlayAgainAction()
    {
        GameMenuNavigator.Instance.DisplayGameMenuCanvas();
    }
}
