using System;
using System.Collections.Generic;
using Gamebee.Utils;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Gamebee.RemoteConfig
{
    public class RemoteConfigStore : IRemoteConfigStoreImp, IRemoteConfigListener
    {
        private static bool isConfigUpdated = false;
        private static event Action OnUpdated;
        internal static event Action<JObject> ConfigDataChanged;
        private static readonly Dictionary<string, object> rcVariables = new Dictionary<string, object>();
        private static IRemoteConfigStoreImp _storeImp;

        public static IEnumerable<(string, object)> AllValues()
        {
            foreach (var (key, value) in rcVariables)
            {
                yield return (key, value);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Reset() => rcVariables.Clear();

        public static RCVariable<T> Define<T>(string key, T defaultValue)
        {
            if (rcVariables.TryGetValue(key, out var variable))
                return (RCVariable<T>)variable;

            var instance = new RCVariable<T>(key, defaultValue);
            rcVariables.Add(key, instance);

            return instance;
        }

        public static void RegisterCallback(Action callback)
        {
            OnUpdated += callback;
            if (isConfigUpdated) callback.Invoke();
        }

        public static void UnregisterCallback(Action callback) => OnUpdated -= callback;

        public static RemoteConfigStore Prepare()
        {
            var store = new RemoteConfigStore();
#if UNITY_EDITOR || UNITY_STANDALONE || GBP_DEBUG_MODE
            _storeImp = TestRCStoreImp.Instance;
#else
            _storeImp = store;
#endif
            return store;
        }

        void IRemoteConfigListener.OnFetched(string data)
        {
            Debug.Log("Remote Config data fetched");
            var obj = JObject.Parse(data);
            ConfigDataChanged?.Invoke(obj);
            PlayerPrefs.Save();
            isConfigUpdated = true;
            OnUpdated?.Invoke();
        }

        void IRemoteConfigListener.OnFailed(string error) { }

        private const string ModuleRoot = "com/gamebee/remoteconfig";
        private const string RemoteConfigClass = ModuleRoot + "/RemoteConfigStore";
        private const string RemoteConfigListener = ModuleRoot + "/IRemoteConfigListener";
        private AndroidJavaClass _remoteConfigClass;

        public void Init(GBPInitData gbpInitData)
        {
            _storeImp.RegisterListener(this);
        }

        public void RegisterListener(IRemoteConfigListener listener)
        {
            RCService.Init();
            _remoteConfigClass = new AndroidJavaClass(RemoteConfigClass);
            _remoteConfigClass.CallStatic("registerListener", new RemoteConfigListener(RemoteConfigListener, this));
        }

        public void Fetch()
        {
            _remoteConfigClass.CallStatic("fetch");
        }
    }

    public class RemoteConfigListener : AndroidJavaProxy, IRemoteConfigListener
    {
        private readonly IRemoteConfigListener _listener;

        internal RemoteConfigListener(string javaInterface, IRemoteConfigListener listener) : base(javaInterface)
        {
            _listener = listener;
        }

        public void OnFetched(string data)
        {
            GBPManager.RunOnMain(() => _listener.OnFetched(data));
        }

        public void OnFailed(string error)
        {
            GBPManager.RunOnMain(() => _listener.OnFailed(error));
        }
    }
}