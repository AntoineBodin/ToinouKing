using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.UI;

public class EndGameUIManager : MonoBehaviour
{
    [SerializeField] private List<PlayerUIWithRank> playersUI;
    [SerializeField] private List<PlayerResultsLine> resultLines;

    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text backButtonText;

    public static EndGameUIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        backButton.onClick.AddListener(PlayAgainAction);
    }

    public void UpdateUI(List<LudoPlayer> players)
    {
        SetPlayersLines(players);
        backButtonText.text = GameManager.Instance.IsOnline ? "Back to Lobby" : "Play Again";
    }

    private void SetPlayers(List<LudoPlayer> players)
    {
        int playerUIIndex = 0;
        players.OrderBy(p => p.Rank).ToList().ForEach(p =>
            {
                playersUI[playerUIIndex].SetPlayerInfo(p.PlayerInfo);
                playersUI[playerUIIndex].UpdateUI();
                playersUI[playerUIIndex].UpdateColor(p.PlayerParameter.TokenColor);
                playerUIIndex++;
            });
        for (int i = playerUIIndex; i < 4; i++)
        {
            playersUI[i].Clear();
        }
    }

    private void SetPlayersLines(List<LudoPlayer> players)
    {
        int playerUIIndex = 0;
        players.OrderBy(p => p.Rank).ToList().ForEach(p =>
        {
            resultLines[playerUIIndex].UpdateUI(p.PlayerInfo.Name.ToString(), p.PlayerInfo.DeadTokens, p.PlayerInfo.KilledTokens, p.PlayerParameter.TokenColor);
            playerUIIndex++;
        });
        for (int i = playerUIIndex; i < 4; i++)
        {
            resultLines[i].gameObject.SetActive(false);
        }
    }

    private void PlayAgainAction()
    {
        GameMenuNavigator.Instance.GoBackFromResult();
    }
}
