using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Gamebee.Utils
{
    public class RCService
    {
        public const string AndroidClassName = "com.gamebee.remoteconfig.RCMiddleware";
        static AndroidJavaClass Clazz = new(AndroidClassName);

        public static void Init()
        {
            Clazz.CallStatic("registerListener", new RCMiddlewareListener());
        }

        public static void SetRCData(RCData data)
        {
            var text = JsonConvert.SerializeObject(data);
            Clazz.CallStatic("setRCData", text);
            Debug.Log(text);
        }

        public static String GetCurrentTimezone()
        {
#if UNITY_EDITOR
            return TimeZoneInfo.Local.Id;
#else
            return Clazz.CallStatic<string>("getCurrentTimezone");
#endif
        }
    }

    public class RCMiddlewareListener : AndroidJavaProxy
    {
        private const string AndroidClassName = "com.gamebee.remoteconfig.RCMiddlewareListener";

        public RCMiddlewareListener() : base(AndroidClassName) { }

        public void onDataReceived(string data)
        {
            Debug.Log(data);
            if (!GBPManager.IsMainThread)
            {
                GBPManager.RunOnMain(() => onDataReceived(data));
                return;
            }

            var rcData = JsonConvert.DeserializeObject<RCData>(data);
            var evaluator = new ExpressionEvaluator { ExpValueProvider = new RCVarProvider() };

            evaluator.EvaluateExpressions(rcData);
            RCService.SetRCData(rcData);
        }
    }

    public class RCData
    {
        public JObject Data;
        public JObject Overrides;
    }

    public class RCVarProvider : IExpValueProvider
    {
        private string CachedTimeZone;
        public string DeviceModel() => SystemInfo.deviceModel.ToLower();

        public string TimeZone()
        {
            if (CachedTimeZone == null)
            {
                CachedTimeZone = RCService.GetCurrentTimezone();
            }

            return CachedTimeZone;
        }


        public long TimeStamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public string Locale() => CultureInfo.CurrentCulture.Name;
        public bool IsTV() => GBPManager.IsTV();
        public int LaunchCount() => GBPManager.LaunchCount();
        public int ScreenWidth() => Screen.width;
        public int ScreenHeight() => Screen.height;
        public string Version() => Application.version;

        public int PP(string key, int def) => PlayerPrefs.GetInt(key, def);
        public float PP(string key, float def) => PlayerPrefs.GetFloat(key, def);
        public string PP(string key, string def) => PlayerPrefs.GetString(key, def);
        public bool PP(string key, bool def) => PlayerPrefs.HasKey(key);

        public bool IsUS() => usTimezones.Contains(TimeZone());
        public bool IsCA() => caTimezones.Contains(TimeZone());
        public bool IsEU() => euTimezones.Contains(TimeZone());
        public bool IsJP() => jpTimezones.Contains(TimeZone());
        public bool IsAU() => auTimezones.Contains(TimeZone());
        public bool IsBR() => brTimezones.Contains(TimeZone());

        public bool IsOSX() => false;
        public bool IsIOS() => false;
        public bool IsTVOS() => false;

        private readonly IList<string> usTimezones = new List<string>(new string[]
        {
            "America/Adak", "Pacific/Honolulu", "America/Anchorage", "America/Juneau",
            "America/Metlakatla", "America/Nome", "America/Sitka", "America/Yakutat",
            "America/Los_Angeles", "America/Boise", "America/Denver", "America/Phoenix",
            "America/North_Dakota/Beulah", "America/North_Dakota/Center", "America/Chicago",
            "America/Indiana/Knox", "America/Menominee", "America/North_Dakota/New_Salem",
            "America/Indiana/Tell_City", "America/Detroit", "America/Indiana/Indianapolis",
            "America/Kentucky/Louisville", "America/Indiana/Marengo", "America/Kentucky/Monticello",
            "America/New_York", "America/Indiana/Petersburg", "America/Indiana/Vevay",
            "America/Indiana/Vincennes", "America/Indiana/Winamac", "America/Puerto_Rico",
            "America/St_Thomas", "Pacific/Guam", "Pacific/Saipan", "Pacific/Wake"
        });

        private readonly IList<string> caTimezones = new List<string>(new string[]
        {
            "America/Vancouver", "America/Cambridge_Bay", "America/Creston", "America/Dawson_Creek",
            "America/Edmonton", "America/Fort_Nelson", "America/Inuvik", "America/Dawson",
            "America/Whitehorse", "America/Rankin_Inlet", "America/Regina", "America/Resolute",
            "America/Swift_Current", "America/Winnipeg", "America/Atikokan", "America/Iqaluit",
            "America/Toronto", "America/Blanc-Sablon", "America/Glace_Bay", "America/Goose_Bay",
            "America/Halifax", "America/Moncton", "America/St_Johns"
        });

        private readonly IList<string> euTimezones = new List<string>(new string[]
        {
            "Europe/Amsterdam", "Europe/Andorra", "Europe/Athens", "Europe/Belgrade",
            "Europe/Berlin", "Europe/Bratislava", "Europe/Brussels", "Europe/Budapest",
            "Europe/Busingen", "Europe/Chisinau", "Europe/Copenhagen", "Europe/Dublin",
            "Europe/Gibraltar", "Europe/Helsinki", "Europe/Istanbul", "Europe/Kaliningrad",
            "Europe/Kiev", "Europe/Lisbon", "Europe/Ljubljana", "Europe/London",
            "Europe/Luxembourg", "Europe/Madrid", "Europe/Malta", "Europe/Mariehamn",
            "Europe/Minsk", "Europe/Monaco", "Europe/Moscow", "Europe/Oslo",
            "Europe/Paris", "Europe/Podgorica", "Europe/Prague", "Europe/Riga",
            "Europe/Rome", "Europe/Samara", "Europe/San_Marino", "Europe/Sarajevo",
            "Europe/Simferopol", "Europe/Skopje", "Europe/Sofia", "Europe/Stockholm",
            "Europe/Tallinn", "Europe/Tirane", "Europe/Ulyanovsk", "Europe/Vaduz",
            "Europe/Vatican", "Europe/Vienna", "Europe/Vilnius", "Europe/Volgograd", "Europe/Warsaw",
            "Europe/Zagreb", "Europe/Zaporozhye", "Europe/Zurich"
        });

        private readonly IList<string> auTimezones = new List<string>(new string[]
        {
            "Australia/Perth", "Australia/Eucla", "Australia/Adelaide", "Australia/Broken_Hill",
            "Australia/Darwin", "Australia/Brisbane", "Australia/Hobart", "Australia/Lindeman",
            "Australia/Melbourne", "Australia/Sydney", "Australia/Lord_Howe", "Antarctica/Macquarie"
        });

        private readonly IList<string> jpTimezones = new List<string>(new string[]
        {
            "Asia/Tokyo"
        });

        private readonly IList<string> brTimezones = new List<string>(new string[]
        {
            "America/Sao_Paulo"
        });
    }
}