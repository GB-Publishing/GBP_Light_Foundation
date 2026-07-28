using UnityEngine;

namespace Gamebee.Ad
{
    internal class AdStoreBuilder
    {
        internal static AdStore Prepare()
        {
            var adStore = new AdStore();
#if GBP_DEBUG_MODE || UNITY_EDITOR
            if (Debug.isDebugBuild)
                adStore.StoreImp = TestAdStoreImp.GetInstance();
#elif UNITY_ANDROID
            adStore.StoreImp = AndroidAdStoreImp.GetInstance();
#endif
            return adStore;
        }
    }
}