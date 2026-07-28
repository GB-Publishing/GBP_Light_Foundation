#if GBP_DEBUG_MODE || UNITY_EDITOR
using System.Collections;
using UnityEngine;

namespace Gamebee.ProxyAnalytics
{
    internal class ProxyAnalyticsImp_Test : IProxyAnalyticsImp
    {
        private static bool IsActivePeer
        {
            get => PlayerPrefs.GetInt("ProxyAnalyticsImp_Test_IsActivePeer", 0) == 1;
            set => PlayerPrefs.SetInt("ProxyAnalyticsImp_Test_IsActivePeer", value ? 1 : 0);
        }

        private IProxyAnalyticsListener listener;

        public void RegisterListener(IProxyAnalyticsListener listener)
        {
            this.listener = listener;

            if (IsActivePeer) GBPManager.RunOnBg(listener.OnOptedIn);
            else GBPManager.RunOnBg(listener.OnOptedOut);
        }

        public IEnumerator OptInAsync()
        {
            IsActivePeer = true;
            PlayerPrefs.Save();

            yield return new WaitForSecondsRealtime(1);
            GBPManager.RunOnBg(listener.OnOptedIn);
        }

        public IEnumerator OptOutAsync()
        {
            IsActivePeer = false;
            PlayerPrefs.Save();

            yield return new WaitForSecondsRealtime(1);
            GBPManager.RunOnBg(listener.OnOptedOut);
        }
    }
}
#endif