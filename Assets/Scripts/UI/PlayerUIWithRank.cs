using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class PlayerUIWithRank : SimplePlayerUI
    {

        [SerializeField]
        private TMP_Text PlayerRankingText;

        public override void UpdateUI()
        {
            base.UpdateUI();
            UpdateRank();
        }

        private void UpdateRank()
        {
            if (PlayerRankingText != null)
            {
                PlayerRankingText.text = this.PlayerInfo.Rank.ToString();
            }
        }

        public override void Clear()
        {
            PlayerRankingText.text = null;
            base.Clear();
        }
    }
}
