using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace Gamebee.ProxyAnalytics
{
    public class ProxyAnalyticsStore : IProxyAnalyticsListener
    {
        #region Static Members

        public static bool IsActivePeer { get; private set; }
        public static event Action OnOptedIn;
        public static event Action OnOptedOut;

        public static int ConsentBoxShowCount
        {
            get => PlayerPrefs.GetInt("GBP_ConsentBoxShowCount", 0);
            set => PlayerPrefs.SetInt("GBP_ConsentBoxShowCount", value);
        }

        internal static IProxyAnalyticsImp AnalyticsImp { get; set; }


        public static IEnumerator ShowConsentBoxAsync()
        {
            // var prevSelected = EventSystem.current.currentSelectedGameObject;
            // var go = Object.Instantiate(GBPManager.Instance.proxyAnalyticsConfig.consentBoxPrefab, GBPManager.Instance.transform);
            // var t = go.GetComponent<RectTransform>();
            // t.anchoredPosition = Vector2.zero;
            // t.localScale = Vector3.one;
            // var consentBox = go.GetComponent<ConsentBox>();
            // bool? userConsent = null;
            //
            // consentBox.OnConsent += consent => userConsent = consent;
            //
            // ConsentBoxShowCount++;
            // PlayerPrefs.Save();
            //
            // yield return new WaitUntil(() => userConsent != null);
            //
            // Object.Destroy(go);
            //
            // if (userConsent == true) 
            //     yield return AnalyticsImp?.OptInAsync();
            // else yield return AnalyticsImp?.OptOutAsync();
            //
            // EventSystem.current.SetSelectedGameObject(prevSelected);
            yield break;
        }

        internal void Init(GBPInitData data)
        {
            AnalyticsImp?.RegisterListener(this);
        }

        public static IEnumerator OptOutAsync()
        {
            yield return AnalyticsImp?.OptOutAsync();
        }

        #endregion

        void IProxyAnalyticsListener.OnOptedIn()
        {
            Debug.Log("ProxyAnalytics: Opted In");
            IsActivePeer = true;
            if (OnOptedIn == null) return;
            GBPManager.RunOnMain(OnOptedIn);
        }

        void IProxyAnalyticsListener.OnOptedOut()
        {
            Debug.Log("ProxyAnalytics: Opted Out");
            IsActivePeer = false;
            if (OnOptedOut == null) return;
            GBPManager.RunOnMain(OnOptedOut);
        }
    }
}