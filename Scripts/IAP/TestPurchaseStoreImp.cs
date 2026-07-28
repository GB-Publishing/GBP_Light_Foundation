#if GBP_DEBUG_MODE || UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Gamebee.IAP.Models;
using Gamebee.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gamebee.IAP
{
    public class TestPurchaseStoreImp :
#if UNITY_EDITOR
        ScriptableObject,
#endif
        IPurchaseStoreImp
    {
        private const string ObjectName = "PurchaseStore_Test";
        private const string ObjectPath = "Assets/GB_Plugin/" + ObjectName + ".asset";

        public enum PurchaseEmulationMode
        {
            Success,
            Failure,
        }

        public List<GBProduct> ProductMetadata;

        public PurchaseEmulationMode purchaseEmulationMode = PurchaseEmulationMode.Success;

        public List<string> AvailableProducts = new();

        private IPurchaseListener EventListener;

#if UNITY_EDITOR
        public static TestPurchaseStoreImp GetInstance()
        {
            var store = AssetDatabase.LoadAssetAtPath<TestPurchaseStoreImp>(ObjectPath);
            if (store == null)
            {
                Directory.CreateDirectory("Assets/GB_Plugin");
                store = CreateInstance<TestPurchaseStoreImp>();
                AssetDatabase.CreateAsset(store, ObjectPath);
                AssetDatabase.SaveAssets();
            }

            return store;
        }

#elif GBP_DEBUG_MODE
        public static TestPurchaseStoreImp GetInstance()
        {
            var store = new TestPurchaseStoreImp();
            store.LoadData();
            Application.quitting += store.SaveData;
            return store;
        }

        private void LoadData()
        {
            AvailableProducts.Clear();
            PlayerPrefs.GetString("AvailableProducts", "").Split(',').ToList().ForEach(p => AvailableProducts.Add(p));

            ProductMetadata = new List<GBProduct>();
            var productMetadata = PlayerPrefs.GetString("ProductMetadata", "");
            Serializer.Deserialize<List<ProductsMetadata>>(productMetadata);
        }
#endif

        private void SaveData()
        {
            PlayerPrefs.SetString("AvailableProducts", string.Join(",", AvailableProducts));
            PlayerPrefs.SetString("ProductMetadata", Serializer.Serialize(ProductMetadata));
            PlayerPrefs.Save();
        }

        void IPurchaseStoreImp.RegisterListener(IPurchaseListener listener)
        {
            EventListener = listener;
            EventListener.OnInitialized("");
        }

        void IPurchaseStoreImp.InitProducts(string[] productNames)
        {
            GBPManager.RunOnBg(() =>
            {
                Thread.Sleep(1000);

                var products = ProductMetadata.Where(p => productNames.Contains(p.Name)).ToArray();
                EventListener.OnProductMetadataUpdate(products);
            });
        }

        public GBProduct GetProductData(string currentProduct) => null;

        void IPurchaseStoreImp.RestorePurchases()
        {
            GBPManager.RunOnBg(() =>
            {
                Thread.Sleep(1000);
                var availableProducts = AvailableProducts.ToArray();
                EventListener.OnAvailableProductsUpdate(availableProducts);
            });
        }

        void IPurchaseStoreImp.Purchase(string productName)
        {
            SaveData();
            GBPManager.RunOnBg(() =>
            {
                Thread.Sleep(1000);

                if (AvailableProducts.Contains(productName))
                {
                    EventListener.OnAvailableProductsUpdate(AvailableProducts.ToArray());

                    Thread.Sleep(200);
                    EventListener.OnPurchaseUpdate(productName);
                    return;
                }

                if (purchaseEmulationMode == PurchaseEmulationMode.Success)
                {
                    AvailableProducts.Add(productName);
                    EventListener.OnAvailableProductsUpdate(AvailableProducts.ToArray());

                    Thread.Sleep(200);

                    EventListener.OnPurchaseUpdate(productName);
                    return;
                }

                EventListener.OnPurchaseUpdate(productName, "Purchase Failed");
            });
        }

        void IPurchaseStoreImp.ReportAsConsumed(string productName)
        {
            SaveData();
            GBPManager.RunOnBg(() =>
            {
                AvailableProducts.Remove(productName);
                EventListener.OnAvailableProductsUpdate(AvailableProducts.ToArray());
            });
        }
    }
}
#endif