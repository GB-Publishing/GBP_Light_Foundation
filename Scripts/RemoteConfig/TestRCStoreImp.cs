#if UNITY_EDITOR || UNITY_STANDALONE || GBP_DEBUG_MODE

using Gamebee.RemoteConfig;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gamebee
{
    public class TestRCStoreImp :
        ScriptableObject,
        IRemoteConfigStoreImp
    {
        public static IRemoteConfigStoreImp Instance =>
#if UNITY_EDITOR
            GBPManager.Instance.RemoteConfigStoreImp ?? CreateInstance();
#else
            GBPManager.Instance.RemoteConfigStoreImp;
#endif


#if UNITY_EDITOR
        private static IRemoteConfigStoreImp CreateInstance()
        {
            GBPManager.Instance.RemoteConfigStoreImp = CreateInstance<TestRCStoreImp>();
            AssetDatabase.CreateAsset((TestRCStoreImp)Instance, "Assets/GB_Plugin/RCStore_Test.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return GBPManager.Instance.RemoteConfigStoreImp;
        }
#endif

        public TextAsset TestRcConfigJson;

        public void RegisterListener(IRemoteConfigListener listener)
        {
            listener.OnFetched(TestRcConfigJson?.text ?? "");
        }

        public void Fetch() { }
    }
}
#endif