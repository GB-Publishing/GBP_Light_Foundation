namespace Gamebee.ProxyAnalytics
{
    public static class ProxyAnalyticsBuilder
    {
        internal static ProxyAnalyticsStore Prepare()
        {
            var analytics = new ProxyAnalyticsStore();
#if UNITY_EDITOR || GBP_DEBUG_MODE
            ProxyAnalyticsStore.AnalyticsImp = new ProxyAnalyticsImp_Test();
#elif UNITY_ANDROID
            ProxyAnalyticsStore.AnalyticsImp = ProxyAnalyticsImp_Android.GetInstance();
#endif
            return analytics;
        }
    }
}