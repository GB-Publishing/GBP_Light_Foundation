#if GBP_DEBUG_MODE || UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Threading;
using Gamebee.Ad.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gamebee.Ad
{
    public class TestAdStoreImp :
#if UNITY_EDITOR
        ScriptableObject,
#endif
        IAdStoreImp
    {
        private const string ObjectName = "AdStore_Test";
        private const string ObjectPath = "Assets/GB_Plugin/" + ObjectName + ".asset";

        public bool loadFakeAds = true;
        public int delayInLoadingSeconds = 15;

        private AdHandler _interstitialHandler, _rewardedHandler;

#if UNITY_EDITOR
        internal static IAdStoreImp GetInstance()
        {
            var store = AssetDatabase.LoadAssetAtPath<TestAdStoreImp>(ObjectPath);
            if (store == null)
            {
                Directory.CreateDirectory("Assets/GB_Plugin");
                store = CreateInstance<TestAdStoreImp>();
                AssetDatabase.CreateAsset(store, ObjectPath);
                AssetDatabase.SaveAssets();
            }

            return store;
        }
#elif GBP_DEBUG_MODE
        internal static IAdStoreImp GetInstance()
        {
            var store = new TestAdStoreImp();
            return store;
        }
#endif

        void IAdStoreImp.Init(IAdListener listener)
        {
            Debug.Log("TestAdStoreImp.Init()");

            _interstitialHandler = new AdHandler(AdType.Interstitial, this, listener);
            _rewardedHandler = new AdHandler(AdType.Rewarded, this, listener);

            GBPManager.RunOnBg(() =>
            {
                Thread.Sleep(2000);
                listener.OnInitialized(null);
            });

            _interstitialHandler.StartCaching();
            _rewardedHandler.StartCaching();
        }

        IEnumerator IAdStoreImp.ShowAd(AdType type)
        {
            Debug.Log("TestAdStoreImp.ShowAd()");

            var handler = type == AdType.Interstitial ? _interstitialHandler : _rewardedHandler;
            if (handler.IsReady) yield return handler.Show();
        }
    }

    internal class AdHandler
    {
        internal bool IsReady => adInstance;
        internal bool isCaching { get; private set; }

        private readonly AdType type;
        private readonly TestAdStoreImp testAdStoreImp;
        private readonly IAdListener listener;
        private GameObject adInstance;
        private bool grantReward;

        public AdHandler(AdType type, TestAdStoreImp testAdStoreImp, IAdListener listener)
        {
            this.type = type;
            this.testAdStoreImp = testAdStoreImp;
            this.listener = listener;
        }

        public void StartCaching()
        {
            if (isCaching) return;

            isCaching = true;
            GBPManager.Instance.StartCoroutine(CachingRoutine());
        }

        private IEnumerator CachingRoutine()
        {
            while (isCaching)
            {
                yield return new WaitForSecondsRealtime(testAdStoreImp.delayInLoadingSeconds);
                if (adInstance) continue;

                adInstance = CreateFakeAd();
                if (!adInstance) continue;

                GBPManager.RunOnBg(() => listener.OnAdLoaded(type));
            }
        }

        internal IEnumerator Show()
        {
            var toShowAdInstance = adInstance;
            adInstance = null;
            GBPManager.RunOnBg(() => listener.OnAdShown(type, null));

            grantReward = false;
            toShowAdInstance.SetActive(true);
            var selectButton =
                toShowAdInstance.transform.Find("FakeAdRewardButton") ??
                toShowAdInstance.transform.Find("FakeAdCloseButton");

            var prevSelected = EventSystem.current.currentSelectedGameObject;
            EventSystem.current.SetSelectedGameObject(selectButton.gameObject);

            yield return new WaitWhile(() => toShowAdInstance);

            GBPManager.RunOnBg(() => listener.OnAdClosed(type, grantReward));

            EventSystem.current.SetSelectedGameObject(prevSelected);
        }

        private GameObject CreateFakeAd()
        {
            if (!testAdStoreImp.loadFakeAds) return null;

            var panel = new GameObject("FakeAdPanel");
            var rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.SetParent(GBPManager.Instance.transform);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.localScale = Vector3.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            var image = panel.AddComponent<Image>();
            image.color = Color.white;

            var text = new GameObject("FakeAdText");
            var textTransform = text.AddComponent<RectTransform>();
            textTransform.SetParent(rectTransform);
            textTransform.anchoredPosition = new Vector2(0, 300);
            textTransform.sizeDelta = new Vector2(500, 100);
            var txt = text.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.color = Color.black;
            txt.text = "This is a fake Ad";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 45;

            var closeButton = new GameObject("FakeAdCloseButton");
            var closeTransform = closeButton.AddComponent<RectTransform>();
            closeTransform.SetParent(rectTransform);
            closeTransform.anchorMin = new Vector2(1, 1);
            closeTransform.anchorMax = new Vector2(1, 1);
            closeTransform.pivot = new Vector2(1, 1);
            closeTransform.anchoredPosition = new Vector2(0, 0);
            txt = closeButton.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.color = Color.black;
            txt.text = "X";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 45;

            var button = closeButton.AddComponent<Button>();
            button.onClick.AddListener(() => Object.Destroy(panel));

            if (type == AdType.Interstitial) goto exit;

            var rewardButton = new GameObject("FakeAdRewardButton");
            var rewardTransform = rewardButton.AddComponent<RectTransform>();
            image = rewardButton.AddComponent<Image>();
            image.color = Color.green;

            rewardTransform.SetParent(rectTransform);
            rewardTransform.anchoredPosition = Vector2.zero;
            rewardTransform.sizeDelta = new Vector2(350, 150);
            var reward = rewardButton.AddComponent<Button>();

            text = new GameObject("txt");
            textTransform = text.AddComponent<RectTransform>();
            textTransform.SetParent(rewardTransform);
            textTransform.anchoredPosition = Vector2.zero;
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            txt = text.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.color = Color.black;
            txt.text = "Grant Reward";
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 45;

            reward.onClick.AddListener(() =>
            {
                grantReward = true;
                Object.Destroy(panel);
            });

            exit:
            panel.SetActive(false);
            return panel;
        }
    }
}
#endif