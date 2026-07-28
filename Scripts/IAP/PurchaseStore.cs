using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Gamebee.IAP.Models;
using UnityEngine;

namespace Gamebee.IAP
{
    /// <summary>
    /// PurchaseStore class for GBP Light IAP
    /// To access the PurchaseStore instance, use <b>GBPManager.PurchaseStore</b>
    /// </summary>
    public class PurchaseStore : IPurchaseListener
    {
        public static event Action<string> OnInitialized;
        public static event Action<GBProduct[]> OnProductMetadataUpdate;
        public static event Action<string[]> OnAvailableProductsUpdate;
        public static event Action<string, string> OnPurchaseUpdate;
        public static Func<bool> IsSuperPremiumPlayer = () => false;

        private static PurchaseStore instance;

        internal IPurchaseStoreImp StoreImp;

        private static readonly List<GBProduct> Products = new();
        private static readonly List<string> AvailableProducts = new();

        private static bool _isManagedReady, _isNativeReady, _purchaseInProgress, _restoreInProgress;

        /// <summary>
        /// To check if the store is ready
        /// </summary>
        /// <returns></returns>
        public static bool IsReady() => _isManagedReady && _isNativeReady;

        internal void Init(GBPInitData data)
        {
            instance = this;
            StoreImp.RegisterListener(this);

            _isManagedReady = true;

            GBPManager.Instance.StartCoroutine(InitNative(data.ProductNames));

            Debug.Log("Using " + StoreImp.GetType().Name + " for IAP");
        }

        public static IEnumerable<GBProduct> AllProducts => Products.AsReadOnly();

        /// <summary>
        /// Get the product metadata from Product Name
        /// </summary>
        /// <param name="currentProduct">Product Name</param>
        /// <returns>Product metadata</returns>
        public static GBProduct GetProductData(string currentProduct) => Products.FirstOrDefault(p => p.Name == currentProduct);

        private IEnumerator InitNative(string[] iapProducts)
        {
            yield return new WaitUntil(IsReady);
            StoreImp.InitProducts(iapProducts);
        }

        /// <summary>
        /// Call this method to restore the purchases.
        /// Upon completion, the available products will be returned in the <see cref="PurchaseStore.OnAvailableProductsUpdate"/>
        /// </summary>
        /// <returns></returns>
        public static IEnumerator RestorePurchasesAsync()
        {
            yield return new WaitUntil(IsReady);

            _restoreInProgress = true;

            instance.StoreImp.RestorePurchases();

            yield return new WaitWhile(() => _restoreInProgress);
        }

        /// <summary>
        /// Call this method to start purchase flow for the given product.
        /// Upon completion, the purchase result will be returned in the <see cref="PurchaseStore.OnPurchaseUpdate"/>
        /// </summary>
        /// <param name="productName">Product Name</param>
        /// <returns></returns>
        public static IEnumerator PurchaseAsync(string productName)
        {
            yield return new WaitUntil(IsReady);
            if (AvailableProducts.Contains(productName))
            {
                // Product is already available
                OnPurchaseUpdate?.Invoke(productName, "Product is already available");
                yield break;
            }

            _purchaseInProgress = true;

            instance.StoreImp.Purchase(productName);

            yield return new WaitWhile(() => _purchaseInProgress);
        }

        /// <summary>
        /// Call this method to mark the purchased consumable product as consumed.
        /// </summary>
        /// <param name="productName">Product Name</param>
        public static void ReportAsConsumed(string productName)
        {
            if (!IsReady()) return;
            instance.StoreImp.ReportAsConsumed(productName);
        }

        /// <summary>
        /// To check if the product is available
        /// </summary>
        /// <param name="productName">Product Name</param>
        /// <returns>True if the product is available</returns>
        public static bool IsAvailable(string productName) => !string.IsNullOrEmpty(productName) && AvailableProducts.Contains(productName);

        void IPurchaseListener.OnInitialized(string error)
        {
            _isNativeReady = string.IsNullOrEmpty(error);
            if (OnInitialized == null) return;

            GBPManager.RunOnMain(() => OnInitialized(error));
        }

        void IPurchaseListener.OnProductMetadataUpdate(GBProduct[] products)
        {
            Products.Clear();
            Products.AddRange(products);

            if (OnProductMetadataUpdate == null) return;
            GBPManager.RunOnMain(() => OnProductMetadataUpdate(products));
        }

        void IPurchaseListener.OnAvailableProductsUpdate(string[] availableProductNames)
        {
            _restoreInProgress = false;
            AvailableProducts.Clear();
            AvailableProducts.AddRange(availableProductNames);

            if (OnAvailableProductsUpdate == null) return;
            GBPManager.RunOnMain(() => OnAvailableProductsUpdate(availableProductNames));
        }

        void IPurchaseListener.OnPurchaseUpdate(string productName, string error)
        {
            _purchaseInProgress = false;

            if (OnPurchaseUpdate == null) return;
            GBPManager.RunOnMain(() => OnPurchaseUpdate(productName, error));
        }
    }
}