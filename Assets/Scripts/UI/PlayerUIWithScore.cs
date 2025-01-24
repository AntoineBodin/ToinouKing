using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;

namespace Assets.Scripts.UI
{
    public class PlayerUIWithScore : SimplePlayerUI
    {

        [SerializeField]
        private TMP_Text PlayerScoreText;

        public override void UpdateUI()
        {
            base.UpdateUI();
            UpdateScore();
        }

        private void UpdateScore()
        {
            if (PlayerScoreText != null)
            {
                PlayerScoreText.text = this.PlayerInfo.Score.ToString();
            }
        }

        public override void Clear()
        {
            PlayerScoreText.text = null;
            base.Clear();
        }
    }
}
