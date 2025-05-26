using Assets.Scripts;
using Assets.Scripts.Helpers;
using Assets.Scripts.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimplePlayerUI : MonoBehaviour
{
    [SerializeField]
    protected TMP_Text PlayerNameText;

    [SerializeField]
    private Image PlayerAvatarImage;

    protected LudoPlayerInfo PlayerInfo;

    public void SetPlayerInfo(LudoPlayerInfo playerInfo)
    {
        PlayerInfo = playerInfo;
    }

    public virtual void UpdateUI()
    {
        gameObject.SetActive(true);
        PlayerNameText.text = PlayerInfo.Name.ToString();
        PlayerAvatarImage.sprite = AvatarIDToImage.Instance.GetAvatarByID(PlayerInfo.AvatarID);
    }

    public virtual void Clear()
    {
        PlayerNameText.text = null;
        PlayerAvatarImage.sprite = null;
        PlayerInfo = LudoPlayerInfo.nullInstance;
        gameObject.SetActive(false);
    }

    public virtual void UpdateColor(Color color)
    {
        PlayerNameText.color = color;
    }
}