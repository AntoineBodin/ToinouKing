using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class Popup : MonoBehaviour
    {
        [SerializeField] private TMP_Text popupTitle;
        [SerializeField] private TMP_Text popupBody;
        private Action onClose;
        private PopupType popupType;

        public void Setup(string title, string message, PopupType type, Action onCloseAction = null)
        {
            onClose = onCloseAction;
            popupType = type;

            popupBody.text = message;
            SetTitle(title);
            SetColor();
        }

        private void SetTitle(string title)
        {
            popupTitle.text = title;
            if (string.IsNullOrEmpty(title))
            {
                popupTitle.text = popupType switch
                {
                    PopupType.Error => "Error",
                    PopupType.Info => "Information",
                    PopupType.Warning => "Warning",
                    _ => "Popup"
                };
            }
        }

        public void Show()
        {
            transform.DOScale(1, 0.2f).SetEase(Ease.OutBack);
        }

        public void ClosePopup()
        {
            transform.DOScale(0, 0.2f).SetEase(Ease.InBack).OnComplete(() => {
                    onClose?.Invoke();
                    Destroy(gameObject);
                });
        }

        private void SetColor()
        {
            var textColor = popupType switch
            {
                PopupType.Error => Color.red,
                PopupType.Info => Color.blue,
                PopupType.Warning => Color.yellow,
                _ => Color.white
            };

            popupTitle.color = textColor;
            popupBody.color = textColor;
        }

    }
}
