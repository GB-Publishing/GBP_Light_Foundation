using UnityEngine;

namespace Gamebee.IAP
{
    public class PurchaseStoreBuilder
    {
        public static PurchaseStore Prepare()
        {
            var store = new PurchaseStore();

#if GBP_DEBUG_MODE || UNITY_EDITOR
            if (Debug.isDebugBuild)
                store.StoreImp = TestPurchaseStoreImp.GetInstance();
#elif UNITY_ANDROID
            store.StoreImp = AndroidPurchaseStoreImp.GetInstance();
#endif

            return store;
        }
    }
}