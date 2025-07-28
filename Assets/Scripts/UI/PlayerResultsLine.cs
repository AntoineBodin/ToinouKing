using System;
using System.Collections.Generic;
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
        [SerializeField] private TMP_Text enteredTokensCountText;
        [SerializeField] private TMP_Text spawnTokensCountText;
        [SerializeField] private TMP_Text houseTokensCountText;
        [SerializeField] private TMP_Text scoreText;

        private GameObject houseTokensCountCol;
        private GameObject spawnTokensCountCol;
        private GameObject enteredTokensCountCol;
        private GameObject scoreCol;

        [SerializeField] private List<Image> backgroundElements;

        private void Awake()
        {
            houseTokensCountCol = houseTokensCountText.transform.parent.gameObject;
            spawnTokensCountCol = spawnTokensCountText.transform.parent.gameObject;
            enteredTokensCountCol = enteredTokensCountText.transform.parent.gameObject;
            scoreCol = scoreText.transform.parent.gameObject;
        }

        public void SetColumns(bool isTimeAttack)
        {
            enteredTokensCountCol.SetActive(isTimeAttack);
            spawnTokensCountCol.SetActive(isTimeAttack);
            houseTokensCountCol.SetActive(isTimeAttack);
            scoreCol.SetActive(isTimeAttack);
        }

        public void UpdateUI(string playerName, int score, int deadTokens, int killedTokens, int enteredTokens, int spawnTokens, int houseTokens, Color color, bool isTimeAttack)
        {
            playerNameText.text = playerName;
            deadTokensCountText.text = deadTokens.ToString();
            killedTokensCountText.text = killedTokens.ToString();
            backgroundElements.ForEach(b => b.color = color);

            SetColumns(isTimeAttack);

            if (isTimeAttack)
            {
                scoreText.text = score.ToString();
                enteredTokensCountText.text = enteredTokens.ToString();
                spawnTokensCountText.text = spawnTokens.ToString();
                houseTokensCountText.text = houseTokens.ToString();
            }
        }
    }
}
