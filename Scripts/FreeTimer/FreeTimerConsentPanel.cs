using System;
using System.Text;
using GBP.Core.UI.FreeTimerUI;
using GBP.GB_FreeTimer;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gamebee.FreeTimerUI
{
    public class FreeTimerConsentPanel : MonoBehaviour
    {
        private static FreeTimerConsentPanel instance;

        public TMP_Text message;
        public Button acceptButton;

        private Action onCloseCallback;

        public static void Show(Action onClose = null)
        {
            if (instance) return; // Already pop up is live
            instance = Instantiate(FreeTimer.Instance.freeTimerConsentPanelPrefab, GBPManager.Instance.transform);
            instance.onCloseCallback = onClose;
        }

        protected virtual void Start()
        {
            var minutes = FreeTimer.OfferTimeSeconds / 60;
            var seconds = FreeTimer.OfferTimeSeconds % 60;

            message.text = new StringBuilder(message.text)
                .Replace("{MINUTES}", FreeTimerCounter.GetMinutesString())
                .Replace("{SECONDS}", FreeTimerCounter.GetSecondsString())
                .ToString();

            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnAcceptOffer);

            EventSystem.current.SetSelectedGameObject(acceptButton.gameObject);
        }

        protected virtual void Update()
        {
            EventSystem.current.SetSelectedGameObject(acceptButton.gameObject); // Offer player cannot refuse
        }

        protected virtual void OnAcceptOffer()
        {
            FreeTimer.ActivateOffer();
            Destroy(gameObject);
            onCloseCallback?.Invoke();
        }
    }
}