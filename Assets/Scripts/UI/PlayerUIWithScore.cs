using DG.Tweening;
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
        private const float LONG_TIME_TO_PLAY = 20f;
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

            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = StartCoroutine(CooldownRoutine(timeToPlay));
        }

        public void ResetTimer()
        {
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

/*        private void UpdateScore()
        {
            if (PlayerScoreText != null)
            {
                // write an animation with dotween to scale up the text, change it and then scale down to normal size
                


                PlayerScoreText.text = this.PlayerInfo.Score.ToString();
            }
        }*/

        private void UpdateScore()
        {
            if (PlayerScoreText != null)
            {
                // Animate: scale up, change text, scale down
                float scaleUp = 1.3f;
                float duration = 0.15f;

                // Kill any existing tweens on the transform to avoid overlap
                PlayerScoreText.transform.DOKill();

                // Scale up
                PlayerScoreText.transform
                    .DOScale(scaleUp, duration)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        // Change the text after scaling up
                        PlayerScoreText.text = this.PlayerInfo.Score.ToString();

                        // Scale back down to normal
                        PlayerScoreText.transform
                            .DOScale(1f, duration)
                            .SetEase(Ease.InBack);
                    });
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
