using System.Linq;
using System.Text;
using Gamebee;
using GBP.GB_FreeTimer;
using TMPro;
using UnityEngine;
using static GBP.GB_FreeTimer.FreeTimer.OfferStatus;

namespace GBP.Core.UI.FreeTimerUI
{
    public class FreeTimerCounter : MonoBehaviour
    {
        public static FreeTimerCounter Instance { get; private set; }
        public TMP_Text counterText;
        public TMP_Text statusText;
        private string counterTemplate;
        private StringBuilder builder;
        private bool pauseTimer = false;
        private RectTransform _rectTransform;
        private RectTransform RectTransform => _rectTransform ?? (_rectTransform = GetComponent<RectTransform>());

        public static void Show()
        {
            // if (IAP_Core.IsSuperPremiumPlayer()) return;

            Instance ??= Instantiate(FreeTimer.Instance.freeTimerCounterPrefab, GBPManager.Instance.transform);
            Instance.transform.localScale = Vector3.one;
            Instance.pauseTimer = false;
            Instance.UpdatePosition();
        }

        public static void DestroyCounter()
        {
            if (!Instance) return;
            Destroy(Instance.gameObject);
            Instance = null;
        }

        public static void Pause()
        {
            if (!Instance) return;
            Instance.pauseTimer = true;
        }

        public static void Resume()
        {
            if (!Instance) return;
            Instance.pauseTimer = false;
        }

        protected virtual void Start()
        {
            counterTemplate = counterText.text;
            builder = new StringBuilder();
        }

        protected virtual void Update()
        {
            // if (IAP_Core.IsSuperPremiumPlayer())
            // {
            //     Destroy(gameObject);
            //     return;
            // }
#if UNITY_EDITOR
            UpdatePosition();
#endif

            if (pauseTimer) return;

            FreeTimer.UtilizeTime(Time.unscaledDeltaTime);
            counterText.text = builder
                .Clear()
                .Insert(0, counterTemplate)
                .Replace("{MINUTES}", GetMinutesString())
                .Replace("{SECONDS}", GetSecondsString())
                .ToString();

            statusText.text = FreeTimer.CurrentOfferStatus is Active ? "Free Time" : "Subscribe";

            pauseTimer = FreeTimer.CurrentOfferStatus == TimeOver;
        }

        public void UpdatePosition()
        {
            var currentLocation = GBPManager.ScreenLocation;
            if (string.IsNullOrEmpty(currentLocation)) goto HIDE;
            if (FreeTimer.CurrentOfferStatus is NoOffer or Available) goto HIDE;


            var location = FreeTimer.Instance.GetTimerLocations().FirstOrDefault(tl => tl.LocationName == currentLocation);
            if (location.LocationName != currentLocation) goto HIDE;

            SHOW:
            RectTransform.localScale = Vector3.one;
            RectTransform.anchoredPosition = location.Position;
            RectTransform.anchorMin = RectTransform.anchorMax = location.Anchor;
            return;

            HIDE:
            RectTransform.localScale = Vector3.zero;
        }

        protected virtual void OnApplicationPause(bool pauseStatus) => pauseTimer = pauseStatus;
        protected virtual void OnDestroy() => Instance = null;

        public static string GetMinutesString() => (FreeTimer.RemainingTimeSeconds / 60).ToString("D2");
        public static string GetSecondsString() => (FreeTimer.RemainingTimeSeconds % 60).ToString("D2");
    }
}