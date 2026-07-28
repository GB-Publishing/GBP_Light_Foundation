using System;
using Gamebee.IAP.Models;
using Gamebee.Utils;
using UnityEngine;

namespace Gamebee.IAP
{
    public class AndroidPurchaseStoreImp : IPurchaseStoreImp
    {
        private const string ModuleRoot = "com/gamebee/iap";

        private static string PurchaseStoreName => ModuleRoot + "/PurchaseStore";
        private static string ListenerName => ModuleRoot + "/IPurchaseListener";

        private static AndroidJavaClass PurchaseStoreClass;

        internal static AndroidPurchaseStoreImp GetInstance()
        {
            var store = new AndroidPurchaseStoreImp();
            Debug.Log("Using AndroidPurchaseStoreImp for IAP");
            Debug.Log("Loading... " + PurchaseStoreName);
            PurchaseStoreClass = new AndroidJavaClass(PurchaseStoreName);
            return store;
        }

        void IPurchaseStoreImp.InitProducts(string[] productNames)
        {
            PurchaseStoreClass.CallStatic("initProducts", productNames);
        }

        void IPurchaseStoreImp.RegisterListener(IPurchaseListener listener)
        {
            var l = new PurchaseListener(ListenerName, listener);
            PurchaseStoreClass.CallStatic("registerListener", l);
        }

        void IPurchaseStoreImp.RestorePurchases()
        {
            PurchaseStoreClass.CallStatic("restorePurchases");
        }

        void IPurchaseStoreImp.Purchase(string productName)
        {
            PurchaseStoreClass.CallStatic("purchase", productName);
        }

        void IPurchaseStoreImp.ReportAsConsumed(string productName)
        {
            PurchaseStoreClass.CallStatic("reportAsConsumed", productName);
        }
    }

    internal class PurchaseListener : AndroidJavaProxy
    {
        private readonly IPurchaseListener _clientListener;

        public PurchaseListener(string listenerName, IPurchaseListener clientListener) : base(listenerName)
        {
            _clientListener = clientListener;
        }

        public void OnInitialized(string error)
        {
            _clientListener.OnInitialized(string.IsNullOrEmpty(error) ? null : error);
        }

        public void OnProductMetadataUpdate(string productsData)
        {
            var data = Serializer.Deserialize<ProductsMetadata>(productsData);
            _clientListener.OnProductMetadataUpdate(data.Products);
        }

        public void OnAvailableProductsUpdate(string[] availableProductNames)
        {
            _clientListener.OnAvailableProductsUpdate(availableProductNames);
        }

        public void OnPurchaseUpdate(string productName, string error)
        {
            _clientListener.OnPurchaseUpdate(productName, error);
        }
    }

    [Serializable]
    public class ProductsMetadata
    {
        public GBProduct[] Products;
    }
}