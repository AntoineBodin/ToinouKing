using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class PlayerUIWithScore : SimplePlayerUI
    {

        private const float LONG_TIME_TO_PLAY = 5f;
        private const float SHORT_TIME_TO_PLAY = 5f;
        private bool isAFK = false;

        [SerializeField]
        private TMP_Text PlayerScoreText;

        [SerializeField]
        private Image PlayerTimeToPlay;

        public event Action OnPlayerTimeToPlayEnd;

        private Coroutine currentCoroutine;

        public override void UpdateUI()
        {
            base.UpdateUI();
            UpdateScore();
        }

        public void StartTimer()
        {
            float timeToPlay = GetTimeToPlay();

            Debug.Log("\tStart timer");
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = StartCoroutine(CooldownRoutine(timeToPlay));
        }

        public void ResetTimer()
        {
            Debug.Log("\tStop timer");
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            PlayerTimeToPlay.fillAmount = 0f;
            isAFK = false;
        }

        private IEnumerator CooldownRoutine(float duration)
        {
            PlayerTimeToPlay.fillAmount = 1f;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                PlayerTimeToPlay.fillAmount = 1f - (elapsed / duration);
                yield return null;
            }
            currentCoroutine = null;
            PlayerTimeToPlay.fillAmount = 0f;
            OnPlayerTimeToPlayEnd.Invoke();
            isAFK = true;
        }

        private float GetTimeToPlay() => isAFK ? SHORT_TIME_TO_PLAY : LONG_TIME_TO_PLAY;

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
            PlayerNameText.text = null;
            base.Clear();
        }

        public void Show()
        {
            gameObject.GetComponent<Animator>().SetTrigger("Show");
        }
    }
}
