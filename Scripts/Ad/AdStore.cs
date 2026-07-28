using System;
using System.Collections;
using Gamebee.Ad.Models;
using UnityEngine;

namespace Gamebee.Ad
{
    public class AdStore : IAdListener
    {
        public static event Action<AdType> OnAdLoaded;
        public static event Action OnAdShown, OnAdClosed;
        public static bool IsInterstitialReady { get; private set; }
        public static bool IsRewardedReady { get; private set; }

        private static AdStore Instance { get; set; }
        private bool grantReward;
        private bool isAdShowing;
        internal IAdStoreImp StoreImp;

        internal AdStore()
        {
        }

        internal void Init(GBPInitData data)
        {
            Instance = this;
            StoreImp.Init(this);
        }

        public static void ShowAd(AdType type, Action<bool> callback)
        {
            GBPManager.Instance.StartCoroutine(Instance.ShowAdAsync(type, callback));
        }

        private IEnumerator ShowAdAsync(AdType type, Action<bool> callback)
        {
            grantReward = false;
            GBPManager.Instance.StartCoroutine(StoreImp.ShowAd(type));
            
            yield return new WaitForSecondsRealtime(3);
            if (!isAdShowing) Debug.LogError("Failed to receive OnAdShown event");
            
            yield return new WaitWhile(() => isAdShowing);
            callback?.Invoke(grantReward);
        }

        void IAdListener.OnInitialized(string error)
        {
            GBPManager.RunOnMain(() =>
            {
                if (string.IsNullOrEmpty(error)) return;
                Debug.LogError("AdStore initialized with error: " + error);
            });
        }

        void IAdListener.OnAdLoaded(AdType type)
        {
            if (type == AdType.Interstitial) IsInterstitialReady = true;
            else if (type == AdType.Rewarded) IsRewardedReady = true;

            GBPManager.RunOnMain(() => OnAdLoaded?.Invoke(type));
        }

        void IAdListener.OnAdShown(AdType type, string error)
        {
            isAdShowing = true;
            if (type == AdType.Interstitial) IsInterstitialReady = false;
            else if (type == AdType.Rewarded) IsRewardedReady = false;

            GBPManager.RunOnMain(() =>
            {
                if (string.IsNullOrEmpty(error)) OnAdShown?.Invoke();
                else Debug.LogError("AdStore failed to show ad: " + error);
            });
        }

        void IAdListener.OnAdClosed(AdType type, bool rewarded)
        {
            GBPManager.RunOnMain(() =>
            {
                grantReward = type == AdType.Rewarded && rewarded;
                OnAdClosed?.Invoke();
                isAdShowing = false;
            });
        }
    }
}