using System.Collections;
using Gamebee.Ad.Models;
using UnityEngine;

namespace Gamebee.Ad
{
    internal class AndroidAdStoreImp : IAdStoreImp
    {
        private const string ModuleRoot = "com/gamebee/ads";

        private static string AdStoreName => ModuleRoot + "/AdStore";
        private static string ListenerName => ModuleRoot + "/IAdListener";
        private static AndroidJavaClass AdStoreClass;

        internal bool adShown;
        internal bool adClosed;

        internal static AndroidAdStoreImp GetInstance()
        {
            var store = new AndroidAdStoreImp();
            Debug.Log("Using AndroidAdStoreImp for Ads");
            Debug.Log("Loading... " + AdStoreName);

            AdStoreClass = new AndroidJavaClass(AdStoreName);
            return store;
        }

        void IAdStoreImp.Init(IAdListener listener)
        {
            var l = new AndroidAdListener(ListenerName, this, listener);
            AdStoreClass.CallStatic("registerListener", l);
        }

        IEnumerator IAdStoreImp.ShowAd(AdType type)
        {
            adShown = true;
            AdStoreClass.CallStatic("showAd", type.ToString());

            yield return WaitUntilAdShown(5);
            yield return new WaitWhile(() => adClosed);
        }

        IEnumerator WaitUntilAdShown(float timeOutSec)
        {
            adShown = false;
            var timeOut = Time.time + timeOutSec;
            while (!adShown)
            {
                yield return null;
                if (Time.time < timeOut) continue;

                Debug.LogError("AdStore failed to show ad: Timeout");
                yield break;
            }
        }
    }

    internal class AndroidAdListener : AndroidJavaProxy
    {
        private readonly AndroidAdStoreImp _adStore;
        private readonly IAdListener _listener;

        internal AndroidAdListener(string javaInterface, AndroidAdStoreImp adStore, IAdListener listener) : base(javaInterface)
        {
            _adStore = adStore;
            _listener = listener;
        }

        public void OnInitialized(string error)
        {
            _listener.OnInitialized(error);
        }

        public void OnAdLoaded(string type)
        {
            var adType = type == "Interstitial" ? AdType.Interstitial : AdType.Rewarded;
            _listener.OnAdLoaded(adType);
        }

        public void OnAdShown(string type, string error)
        {
            var adType = type == "Interstitial" ? AdType.Interstitial : AdType.Rewarded;
            _adStore.adShown = true;
            _adStore.adClosed = false;
            _listener.OnAdShown(adType, error);
        }

        public void OnAdClosed(string type, bool rewarded)
        {
            var adType = type == "Interstitial" ? AdType.Interstitial : AdType.Rewarded;
            _adStore.adClosed = true;
            _listener.OnAdClosed(adType, rewarded);
        }
    }
}