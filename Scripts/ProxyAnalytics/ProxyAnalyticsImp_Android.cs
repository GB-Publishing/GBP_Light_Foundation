using System.Collections;
using UnityEngine;

namespace Gamebee.ProxyAnalytics
{
    internal class ProxyAnalyticsImp_Android : IProxyAnalyticsImp
    {
        private const string ModuleRoot = "com/gamebee/proxyanalytics";
        private static string ProxyAnalyticsStoreName => ModuleRoot + "/ProxyAnalyticsStore";
        private static string ListenerName => ModuleRoot + "/IProxyAnalyticsListener";
        private static AndroidJavaClass _proxyAnalyticsStoreClass;

        internal bool IsWaitingForCallback;

        internal static ProxyAnalyticsImp_Android GetInstance()
        {
            var store = new ProxyAnalyticsImp_Android();
            Debug.Log("Using ProxyAnalyticsImp_Android for ProxyAnalytics");
            Debug.Log("Loading... " + ProxyAnalyticsStoreName);
            _proxyAnalyticsStoreClass = new AndroidJavaClass(ProxyAnalyticsStoreName);
            return store;
        }

        public void RegisterListener(IProxyAnalyticsListener listener)
        {
            var l = new ProxyAnalyticsListener(this, ListenerName, listener);
            _proxyAnalyticsStoreClass.CallStatic("registerListener", l);
        }

        public IEnumerator OptInAsync()
        {
            IsWaitingForCallback = true;
            _proxyAnalyticsStoreClass.CallStatic("setOptIn");

            yield return new WaitWhile(() => IsWaitingForCallback);
        }

        public IEnumerator OptOutAsync()
        {
            IsWaitingForCallback = true;
            _proxyAnalyticsStoreClass.CallStatic("setOptOut");

            yield return new WaitWhile(() => IsWaitingForCallback);
        }
    }

    internal class ProxyAnalyticsListener : AndroidJavaProxy, IProxyAnalyticsListener
    {
        private readonly ProxyAnalyticsImp_Android _imp;
        private readonly IProxyAnalyticsListener _clientListener;

        public ProxyAnalyticsListener(ProxyAnalyticsImp_Android imp, string listenerName, IProxyAnalyticsListener clientListener) : base(listenerName)
        {
            _imp = imp;
            _clientListener = clientListener;
        }

        public void OnOptedIn()
        {
            _clientListener.OnOptedIn();
            _imp.IsWaitingForCallback = false;
        }

        public void OnOptedOut()
        {
            _clientListener.OnOptedOut();
            _imp.IsWaitingForCallback = false;
        }
    }
}