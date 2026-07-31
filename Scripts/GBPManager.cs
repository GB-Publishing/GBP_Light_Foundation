using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Gamebee.Ad;
using Gamebee.Analytics;
using Gamebee.IAP;
using Gamebee.ProxyAnalytics;
using Gamebee.RemoteConfig;
using Gamebee.Utils;
using GBP.GB_FreeTimer;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gamebee
{
    /// <summary>
    /// Manager class for GBP Light
    /// Attach this script to an empty GameObject in the scene
    /// </summary>
    public class GBPManager : MonoBehaviour
    {
        public static GBPManager Instance { get; private set; }

        private static PurchaseStore Purchase { get; set; } = null;
        private static AdStore Ad { get; set; } = null;
        private static AnalyticsStore Analytics { get; set; } = null;
        private static ProxyAnalyticsStore ProxyAnalytics { get; set; } = null;
        private static RemoteConfigStore RemoteConfigStore { get; set; } = null;

        private static Queue<Action> _mainThreadActions = new();
        private static int mainThreadId;

        public static int CurrentFPS { get; private set; }

        private GBPInitData _gbpInitData = null;

        private static void CreateModules()
        {
            Purchase = PurchaseStoreBuilder.Prepare();
            Ad = AdStoreBuilder.Prepare();
            Analytics = AnalyticsBuilder.Prepare();
            ProxyAnalytics = ProxyAnalyticsBuilder.Prepare();
            RemoteConfigStore = RemoteConfigStore.Prepare();

            InitSafely(
                Purchase.Init,
                Ad.Init,
                Analytics.Init,
                ProxyAnalytics.Init,
                RemoteConfigStore.Init
            );
        }

        private static void InitSafely(params Action<GBPInitData>[] initAction)
        {
            foreach (var action in initAction)
            {
                try
                {
                    action(Instance?._gbpInitData);
                }
                catch (Exception e)
                {
                    Debug.LogError("Error in GBPManager Init: " + e.Message);
                }
            }
        }


        public SubscriptionPopup SubscriptionPopupPrefab;
        public FreeTimer FreeTimer;
        public bool isTV = true;

        public static bool IsFireTV { get; private set; }

        /// <summary>
        /// Check if the device is a TV
        /// </summary>
        /// <returns> True if the device is a TV </returns>
        public static bool IsTV() => IsFireTV;

        /// <summary>
        /// Call this method to initialize GBP Light
        /// </summary>
        /// <param name="data"> Initialization data </param>
        public static void Init(GBPInitData data)
        {
            Debug.Log("Welcome To GBP Light v1.0.0");
            if (Instance == null) CreateOrLoadInstance();
            if (Instance == null) return;
            if (Instance?._gbpInitData is { }) return;
            Instance._gbpInitData = data;
            CreateModules();
        }

        internal static bool IsMainThread => mainThreadId == Thread.CurrentThread.ManagedThreadId;

        internal static void RunOnBg(ThreadStart action)
        {
            new Thread(action)
            {
                IsBackground = true
            }.Start();
        }

        internal static void RunOnMain(Action action)
        {
            if (action == null) return;

            if (IsMainThread)
            {
                action();
                return;
            }

            _mainThreadActions.Enqueue(action);
        }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR
            var deviceModel = SystemInfo.deviceModel.ToLower();
            isTV = deviceModel.Contains("aft") || deviceModel.Contains("aeohy");
#endif
            IsFireTV = isTV;
        }

        private void Start()
        {
            Application.targetFrameRate = 60;
            name = nameof(GBPManager);
            mainThreadId = Thread.CurrentThread.ManagedThreadId;

            PlayerPrefs.SetInt("LaunchCount", LaunchCount() + 1);

            FreeTimer.Init(this);
            StartCoroutine(GBPDebugConfig.Load());
        }

        private static GBPManager CreateOrLoadInstance()
        {
            GBPManager instance = null;
            var prefab = Resources.Load<GameObject>("GBPManager");
            if (prefab) instance = Instantiate(prefab).GetComponent<GBPManager>();
#if UNITY_EDITOR
            else instance = CreateNewInstance();
#endif
            return instance;
        }

#if UNITY_EDITOR
        private const string PrefabPath = "Assets/GB_Plugin/Resources/GBPManager.prefab";

        private static GBPManager CreateNewInstance()
        {
            var gameObject = new GameObject("GBPManager");
            var gbpManager = gameObject.AddComponent<GBPManager>();

            var canvas = gameObject.AddComponent<Canvas>();
            var scaler = gameObject.AddComponent<CanvasScaler>();
            var raycaster = gameObject.AddComponent<GraphicRaycaster>();

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;

            raycaster.ignoreReversedGraphics = true;
            Debug.Log("GBPManager instance created");

            Directory.CreateDirectory("Assets/GB_Plugin/Resources");
            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, PrefabPath, InteractionMode.AutomatedAction);
            return gbpManager;
        }
#endif

        private void Update()
        {
            CurrentFPS = GetFPS();
        }

        private void LateUpdate()
        {
            GBPDebugConfig.CalculateStats();
            while (_mainThreadActions.Count > 0)
            {
                _mainThreadActions.Dequeue().Invoke();
            }
        }

        private void OnGUI() => GBPDebugConfig.OnGUI();

        private float lastFPS = 0;

        private int GetFPS()
        {
            lastFPS = Mathf.Lerp(lastFPS, 1.0f / Time.unscaledDeltaTime, 0.1f);
            return (int)lastFPS;
        }

        public static int LaunchCount() => PlayerPrefs.GetInt("LaunchCount", 0);

        [SerializeField] private string screenLocation;

        public static string ScreenLocation
        {
            get => Instance?.screenLocation;
            set => Instance.screenLocation = value;
        }

        private void OnApplicationQuit()
        {
            FreeTimer.SaveData();
        }

#if UNITY_EDITOR || GBP_DEBUG_MODE
        public TestRCStoreImp RemoteConfigStoreImp;
#endif
    }
}