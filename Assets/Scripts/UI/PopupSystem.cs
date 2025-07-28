using System;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class PopupSystem : MonoBehaviour
    { 
        public static PopupSystem Instance { get; private set; }
        [SerializeField] private GameObject PopupPrefab;
        [SerializeField] private Transform CanvasTransform;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PopupInfo(string message, string title = "", Action onClose = null)
        {
            Popup(message, PopupType.Info, title, onClose);
        }
        public void PopupError(string message, string title = "", Action onClose = null)
        {
            Popup(message, PopupType.Warning, title, onClose);
        }
        public void PopupWarning(string message, string title = "", Action onClose = null)
        {
            Popup(message, PopupType.Warning, title, onClose);
        }

        private void Popup(string message, PopupType type, string title = "", Action onClose = null)
        {
            var popupGO = Instantiate(PopupPrefab, CanvasTransform);
            var popup = popupGO.GetComponent<Popup>();
            popup.Setup(title, message, type, onClose);
            popup.Show();
        }
    }
    public enum PopupType
    {
        Error,
        Info,
        Warning
    }
}