using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    internal class PlayerResultsLine : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text deadTokensCountText;
        [SerializeField] private TMP_Text killedTokensCountText;
        [SerializeField] private List<Image> backgroundElements;

        public void UpdateUI(string playerName, int deadTokens, int killedTokens, Color color)
        {
            playerNameText.text = playerName;
            deadTokensCountText.text = deadTokens.ToString();
            killedTokensCountText.text = killedTokens.ToString();
            backgroundElements.ForEach(b => b.color = color);
        }

    }
}
