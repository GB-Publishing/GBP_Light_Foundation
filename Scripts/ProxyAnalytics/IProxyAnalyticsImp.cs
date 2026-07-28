using System.Collections;

namespace Gamebee.ProxyAnalytics
{
    internal interface IProxyAnalyticsImp
    {
        void RegisterListener(IProxyAnalyticsListener listener);
        IEnumerator OptInAsync();
        IEnumerator OptOutAsync();
    }

    internal interface IProxyAnalyticsListener
    {
        void OnOptedIn();
        void OnOptedOut();
    }
}