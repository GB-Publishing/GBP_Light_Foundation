using UnityEditor;
using UnityEngine;

namespace Gamebee.ProxyAnalytics
{
    public class ProxyAnalyticsConfig : ScriptableObject
    {
        public GameObject consentBoxPrefab;

#if UNITY_EDITOR
        public static ProxyAnalyticsConfig GetOrCreateInstance()
        {
            var path = "Assets/GB_Plugin/ProxyAnalyticsConfig_Live.asset";
            var instance = AssetDatabase.LoadAssetAtPath<ProxyAnalyticsConfig>(path);
            if (instance != null) return instance;

            instance = CreateInstance<ProxyAnalyticsConfig>();
            instance.name = "ProxyAnalyticsConfig_Live";
            instance.consentBoxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Packages/com.gamebee.light.foundation/Prefabs/BrightData_ConsentBox.prefab");

            // Save it to path
            AssetDatabase.CreateAsset(instance, path);
            return instance;
        }
#endif
    }
}