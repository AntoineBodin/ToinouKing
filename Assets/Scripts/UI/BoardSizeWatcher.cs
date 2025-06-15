using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.UI
{
    internal class BoardSizeWatcher : MonoBehaviour
    {
        public static BoardSizeWatcher Instance { get; private set; }

        public event Action OnResolutoinChanged;

        private float? lastHeight;


        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            lastHeight = GameObject.FindGameObjectWithTag("Board")?.GetComponent<RectTransform>().rect.height;
        }

        private void Update()
        {
            float? boardHeight = GameObject.FindGameObjectWithTag("Board")?.GetComponent<RectTransform>().rect.height;

            if (boardHeight.HasValue && boardHeight.Value != lastHeight)
            {
                lastHeight = boardHeight.Value;
                OnResolutoinChanged?.Invoke();
            }
        }
    }
}
