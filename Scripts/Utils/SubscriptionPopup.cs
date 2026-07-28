using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Gamebee.Utils
{
    public class SubscriptionPopup : MonoBehaviour
    {
        public static SubscriptionPopup instance { get; private set; }

        public Button subscriptionButton;
        public Button playWithAdsButton;
        public Button closeButton;

        protected Action onAllow, onDeny;

        private GameObject lastSelectedGameObject;

        public static void Show(Action onAllow, Action onDeny)
        {
            if (instance) return;

            instance = Instantiate(GBPManager.Instance.SubscriptionPopupPrefab, GBPManager.Instance.transform);
            instance.onAllow = onAllow;
            instance.onDeny = onDeny;
        }

        protected virtual void Start()
        {
            closeButton.onClick.AddListener(Close);
            subscriptionButton.onClick.AddListener(Subscribe);
            playWithAdsButton.onClick.AddListener(PlayWithAdsClicked);

            EventSystem.current.SetSelectedGameObject(subscriptionButton.gameObject);
        }

        private void Update()
        {
            if (GBPManager.IsTV()) HandleButtonSelection();

            if (EventSystem.current.currentSelectedGameObject?.transform.IsChildOf(transform) ?? false) return;
            EventSystem.current.SetSelectedGameObject(subscriptionButton.gameObject);
        }

        private void HandleButtonSelection()
        {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (currentSelectedGameObject == lastSelectedGameObject) return;

            if (!currentSelectedGameObject.transform.IsChildOf(transform))
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedGameObject);
                return;
            }

            lastSelectedGameObject = currentSelectedGameObject;
        }

        protected virtual void OnDestroy()
        {
            instance = null;
        }

        protected virtual void Subscribe() { }

        private void PlayWithAdsClicked()
        {
            onAllow();
            Destroy(gameObject);
        }

        private void Close()
        {
            onDeny();
            Destroy(gameObject);
        }
    }
}