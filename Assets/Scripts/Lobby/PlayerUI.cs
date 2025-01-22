using Assets.Scripts;
using Assets.Scripts.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_Text PlayerName;

    [SerializeField]
    private Image PlayerAvatar;

    [SerializeField]
    private TMP_Text PlayerScore;
    
    [SerializeField]
    private TMP_Text Ranking;

    private LudoPlayerInfo PlayerInfo;

    public void SetPlayerInfo(LudoPlayerInfo playerInfo)
    {
        PlayerInfo = playerInfo;
        UpdateUI();
    }

    public void UpdateUI()
    {
        gameObject.SetActive(true);
        PlayerName.text = PlayerInfo.Name.ToString();
        PlayerAvatar.sprite = AvatarIDToImage.Instance.GetAvatarByID(PlayerInfo.AvatarID);
    }

    public void UpdateScore(int score)
    {
        PlayerScore.text = score.ToString();
    }

    public void SetRank(int rank)
    {
        PlayerScore.text = rank.ToString();
    }

    internal void Clear()
    {
        PlayerName.text = null;
        PlayerAvatar.sprite = null;
        PlayerInfo = LudoPlayerInfo.nullInstance;
        gameObject.SetActive(false);
    }
}