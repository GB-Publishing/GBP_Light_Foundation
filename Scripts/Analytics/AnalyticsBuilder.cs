namespace Gamebee.Analytics
{
    public static class AnalyticsBuilder
    {
        public static AnalyticsStore Prepare()
        {
            var analytics = new AnalyticsStore();
#if UNITY_EDITOR || GBP_DEBUG_MODE
            analytics.AnalyticsImp = new TestAnalyticsImp();
#elif UNITY_ANDROID
            analytics.AnalyticsImp = AndroidAnalyticsImp.getInstance();
#endif
            return analytics;
        }
    }
}