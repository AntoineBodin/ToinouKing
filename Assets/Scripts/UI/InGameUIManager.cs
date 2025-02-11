using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Multiplayer.Playmode;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class InGameUIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text currentPlayerText;
        [SerializeField] private Animator currentPlayerDisplayAnimator;
        [SerializeField] private float pauseDuration;

        public string ApparitionAnimationName = "";
        public string DisparitionAnimationName = "";

        public static InGameUIManager Instance;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public void DisplayCurrentPlayer(LudoPlayer currentPlayer)
        {
            currentPlayerText.text = currentPlayer.Name.ToString();
            currentPlayerText.color = currentPlayer.PlayerParameter.TokenColor; 
            currentPlayerDisplayAnimator.Play(ApparitionAnimationName, -1, 0);
        }

        public void UpdateCurrentPlayer(LudoPlayer currentPlayer)
        {
            StartCoroutine(AnimatePlayerChange(currentPlayer));
        }

        private IEnumerator AnimatePlayerChange(LudoPlayer currentPlayer)
        {
            currentPlayerDisplayAnimator.Play(DisparitionAnimationName, -1, 0);
            yield return new WaitForSeconds(pauseDuration);
            DisplayCurrentPlayer(currentPlayer);
        }
    }
}
