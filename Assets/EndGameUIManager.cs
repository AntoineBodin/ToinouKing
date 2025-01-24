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

    public static EndGameUIManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void SetPlayers(List<LudoPlayer> players)
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

    public void SetOnline(bool isOnlineGame)
    {
        backButton.interactable = isOnlineGame;
    }
}
