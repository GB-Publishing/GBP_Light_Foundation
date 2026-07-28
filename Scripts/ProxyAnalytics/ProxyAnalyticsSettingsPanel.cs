using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gamebee.ProxyAnalytics
{
    public class ProxyAnalyticsSettingsPanel : MonoBehaviour
    {
        public string optInText, optOutText;
        public string learnMoreLink;
        public Button button_LearnMore;
        public Image image_LearnMoreQR;
        public Button button_OptInOut;
        public Image image_OptInOut;
        public Sprite sprite_On;
        public Sprite sprite_Off;
        public Text text_Benefits;

        private void OnEnable()
        {
            ProxyAnalyticsStore.OnOptedIn += UpdateUI;
            ProxyAnalyticsStore.OnOptedOut += UpdateUI;
            button_LearnMore.onClick.AddListener(() => Application.OpenURL(learnMoreLink));
            button_OptInOut.onClick.AddListener(() => StartCoroutine(OptInOutAsync()));

            UpdateUI();
        }

        private void OnDisable()
        {
            ProxyAnalyticsStore.OnOptedIn -= UpdateUI;
            ProxyAnalyticsStore.OnOptedOut -= UpdateUI;

            button_LearnMore.onClick.RemoveAllListeners();
            button_OptInOut.onClick.RemoveAllListeners();
        }

        private IEnumerator OptInOutAsync()
        {
            if (ProxyAnalyticsStore.IsActivePeer)
                yield return ProxyAnalyticsStore.OptOutAsync();
            else
                yield return ProxyAnalyticsStore.ShowConsentBoxAsync();

            UpdateUI();
        }

        private void UpdateUI()
        {
            image_LearnMoreQR.gameObject.SetActive(GBPManager.IsFireTV);
            button_LearnMore.gameObject.SetActive(!GBPManager.IsFireTV);

            var text = ProxyAnalyticsStore.IsActivePeer ? optInText : optOutText;
            text_Benefits.text = text;

            image_OptInOut.sprite = ProxyAnalyticsStore.IsActivePeer ? sprite_On : sprite_Off;
        }
    }
}