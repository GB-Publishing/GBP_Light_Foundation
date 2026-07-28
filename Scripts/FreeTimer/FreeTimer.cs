using System;
using System.Collections;
using System.Collections.Generic;
using Gamebee;
using Gamebee.FreeTimerUI;
using Gamebee.IAP;
using Gamebee.RemoteConfig;
using GBP.Core.UI.FreeTimerUI;
using UnityEngine;
using UnityEngine.Serialization;

namespace GBP.GB_FreeTimer
{
    [CreateAssetMenu(menuName = "GBP_Light/FreeTimer", fileName = "FreeTimer")]
    public class FreeTimer : ScriptableObject
    {
        public static FreeTimer Instance { get; private set; }

        public static RCVariable<int> free_timer_minutes { get; private set; }
        public static int OfferTimeSeconds { get; private set; } = 0;
        public static float UtilizedTimeSeconds { get; private set; } = 0;
        public static int RemainingTimeSeconds => Mathf.Max(0, OfferTimeSeconds - (int)UtilizedTimeSeconds);

        public static event Action OnOfferAvailable;
        public static event Action OnOfferActivated;
        public static event Action OnOfferTimeOver;

        public enum OfferStatus
        {
            NoOffer,
            Available,
            Active,
            TimeOver
        }

        public static OfferStatus CurrentOfferStatus => 1 switch
        {
            _ when PurchaseStore.IsSuperPremiumPlayer() => OfferStatus.NoOffer,
            _ when OfferTimeSeconds <= 0 => OfferStatus.NoOffer, // No offer available
            _ when UtilizedTimeSeconds == 0 => OfferStatus.Available, // Offer available but not started
            _ when RemainingTimeSeconds == 0 => OfferStatus.TimeOver, // Offer time used up
            _ when UtilizedTimeSeconds > 0 => OfferStatus.Active, // Offer is being used
            _ => OfferStatus.NoOffer
        };

        private static int OfferTimeSeconds_Persisted
        {
            get => PlayerPrefs.GetInt(nameof(FreeTimer) + nameof(OfferTimeSeconds), 0);
            set => PlayerPrefs.SetInt(nameof(FreeTimer) + nameof(OfferTimeSeconds), value);
        }

        private static int UtilizedTimeSeconds_Persisted
        {
            get => PlayerPrefs.GetInt(nameof(FreeTimer) + nameof(UtilizedTimeSeconds), 0);
            set => PlayerPrefs.SetInt(nameof(FreeTimer) + nameof(UtilizedTimeSeconds), value);
        }

        private static IEnumerator InitializeAsync()
        {
            PurchaseStore.OnAvailableProductsUpdate += _ => CheckSuperPremiumStatus();

            // Load persisted values
            OfferTimeSeconds = OfferTimeSeconds_Persisted;
            UtilizedTimeSeconds = UtilizedTimeSeconds_Persisted;

            yield return new WaitUntil(PurchaseStore.IsReady);

            if (CurrentOfferStatus is OfferStatus.Active or OfferStatus.TimeOver) yield break;
            RemoteConfigStore.RegisterCallback(() => OnOfferTimeUpdated(free_timer_minutes));
        }

        private static bool CheckSuperPremiumStatus()
        {
            if (!PurchaseStore.IsSuperPremiumPlayer()) return false;

            OfferTimeSeconds = 0;
            UtilizedTimeSeconds = 0;
            SaveData();
            PlayerPrefs.Save();
            FreeTimerCounter.Instance?.UpdatePosition();

            Debug.Log("[GB_FreeTimer] Super Premium Player detected. Free Timer offer disabled.");
            return true;
        }

        private static void OnOfferTimeUpdated(int offerTimeMinutes)
        {
            if (offerTimeMinutes <= 0 || PurchaseStore.IsSuperPremiumPlayer() || OfferTimeSeconds > 0) return;

            OfferTimeSeconds = Mathf.Abs(offerTimeMinutes * 60);
            UtilizedTimeSeconds = 0;
            SaveData();

            Debug.Log($"[GB_FreeTimer] Free time offer available: {OfferTimeSeconds} seconds.");
            OnOfferAvailable?.Invoke();
        }

        public static void UtilizeTime(float deltaTime)
        {
            if (CurrentOfferStatus is not OfferStatus.Active) return;

            UtilizedTimeSeconds += Mathf.Min(deltaTime, 0.1f);

            if (CurrentOfferStatus is OfferStatus.TimeOver)
                OnOfferTimeOver?.Invoke();
        }

        public static void ActivateOffer()
        {
            if (CurrentOfferStatus is not OfferStatus.Available) return;
            UtilizedTimeSeconds = Time.deltaTime;
            if (CurrentOfferStatus is OfferStatus.Active) OnOfferActivated?.Invoke();
            SaveData();
        }

        public static void SaveData()
        {
            OfferTimeSeconds_Persisted = OfferTimeSeconds;
            UtilizedTimeSeconds_Persisted = (int)UtilizedTimeSeconds;
            PlayerPrefs.Save();

            Debug.Log("FreeTimer : Offer=" + OfferTimeSeconds_Persisted + " , Utilized=" + UtilizedTimeSeconds_Persisted + " seconds");
        }

#if UNITY_EDITOR
        private OfferStatus Status => CurrentOfferStatus;
        private int RemainingSeconds => RemainingTimeSeconds;

        [ContextMenu("Reset Free Timer")]
        private void ResetOffer()
        {
            OfferTimeSeconds = 0;
            UtilizedTimeSeconds = 0;
            SaveData();
        }
#endif
        public FreeTimerConsentPanel freeTimerConsentPanelPrefab;
        public FreeTimerCounter freeTimerCounterPrefab;

        public void Init(GBPManager manager)
        {
            manager.StartCoroutine(InitializeAsync());
            free_timer_minutes = RemoteConfigStore.Define(nameof(free_timer_minutes), 0);
        }

        protected void OnEnable()
        {
            Instance = this;
        }

        [Serializable]
        public struct TimerLocation
        {
            public string LocationName;
            public Vector2 Position;
            public Vector2 Anchor;
        }

        [SerializeField] private List<TimerLocation> TimerLocations = new();
        public TimerLocation[] GetTimerLocations() => TimerLocations.ToArray();

        public static void RegisterSceneLocation(string location)
        {
            FreeTimerCounter.Instance?.UpdatePosition();
        }

        [ContextMenu("Print Status")]
        private void PrintStatus()
        {
            Debug.Log("[GB_FreeTimer] Status: " + CurrentOfferStatus);
            Debug.Log("[GB_FreeTimer] TimeRemaining: " + RemainingTimeSeconds);
        }
    }
}