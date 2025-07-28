using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    internal class GameParametersSettings : MonoBehaviour
    {
        [SerializeField] private Toggle isTimeAttackCheckBox;
        [SerializeField] private Toggle spawnWithTokensCheckBox;
        [SerializeField] private Slider timeLimitSlider;
        [SerializeField] private TMP_Text timeLimitText;
        [SerializeField] private GameObject sliderElements;
        [SerializeField] private GameObject spawnWithTokensElements;

        public event Action<int> OnSliderValueChanged;
        public event Action<bool> OnGameModeBoxChecked;
        public event Action<bool> OnSpawnWithTokenBoxChecked;

        private void Start()
        {
            sliderElements.SetActive(isTimeAttackCheckBox.isOn);
            spawnWithTokensElements.SetActive(isTimeAttackCheckBox.isOn);
            timeLimitSlider.onValueChanged.AddListener((value) =>
            {
                OnSliderValueChanged?.Invoke((int)value * 60);
                UpdateTimeLimitText();
            });
            isTimeAttackCheckBox.onValueChanged.AddListener((isOn) =>
            {
                OnGameModeBoxChecked?.Invoke(isOn);
                sliderElements.SetActive(isOn);
                spawnWithTokensElements.SetActive(isOn);
            });
            spawnWithTokensCheckBox.onValueChanged.AddListener((isOn) => 
            {
                OnSpawnWithTokenBoxChecked?.Invoke(isOn);
            });
        }

        private void UpdateTimeLimitText()
        {
            int value = (int)timeLimitSlider.value * 60;
            if (timeLimitText != null) {
                int minutes = value / 60;
                int seconds = value % 60;
                timeLimitText.text = $"{minutes:D2}:{seconds:D2}";
            }
        }

        public void SetIsTimeAttackChecked(bool isChecked)
        {
            isTimeAttackCheckBox.isOn = isChecked;
            sliderElements.SetActive(isChecked);
            spawnWithTokensElements.SetActive(isChecked);
        }

        internal void SetSpawnWithTokensChecked(bool isChecked)
        {
            spawnWithTokensCheckBox.isOn = isChecked;
        }

        public void SetTimerValue(int value)
        {
            timeLimitSlider.value = value /60;
            UpdateTimeLimitText();
        }

        public GameMode GetGameMode()
        {
            return isTimeAttackCheckBox.isOn? GameMode.TimeAttack : GameMode.Classic;
        }

        public bool IsSpawnWithTokensChecked()
        {
            return spawnWithTokensCheckBox.isOn;
        }

        public int GetTimerValue()
        {
            return (int)timeLimitSlider.value * 60;
        }

        public void SetIsHost(bool isHost)
        {
            timeLimitSlider.interactable = isHost;
            spawnWithTokensCheckBox.interactable = isHost;
            isTimeAttackCheckBox.interactable = isHost;
        }
    }
}
