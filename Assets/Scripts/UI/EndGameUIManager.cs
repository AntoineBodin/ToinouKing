using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts.UI;

public class EndGameUIManager : MonoBehaviour
{
    [SerializeField] private List<PlayerResultsLine> resultLines;
    [SerializeField] private PlayerResultsLine header;

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

    public void UpdateUI(List<LudoPlayer> players, bool isTimeAttack)
    {
        SetPlayersLines(players, isTimeAttack);
        backButtonText.text = GameManager.Instance.IsOnline ? "Back to Lobby" : "Play Again";
    }

    private void SetPlayersLines(List<LudoPlayer> players, bool isTimeAttack)
    {
        int playerUIIndex = 0;

        header.SetColumns(isTimeAttack);

        var orderedList = isTimeAttack ? players.OrderByDescending(p => p.PlayerInfo.Score).ToList() : players.OrderBy(p => p.Rank).ToList();

        orderedList.ForEach(p =>
        {
            resultLines[playerUIIndex].UpdateUI(p.PlayerInfo.Name.ToString(), p.PlayerInfo.Score, p.PlayerInfo.DeadTokens, p.PlayerInfo.KilledTokens, p.PlayerInfo.EnteredTokens, p.PlayerInfo.SpawnTokens, p.PlayerInfo.HouseTokens, p.PlayerParameter.TokenColor, isTimeAttack);
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
