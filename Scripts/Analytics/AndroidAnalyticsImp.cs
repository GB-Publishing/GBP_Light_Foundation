#if UNITY_ANDROID && !UNITY_EDITOR
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Gamebee.Analytics
{
    public class AndroidAnalyticsImp : IAnalyticsEvents
    {
        private const string ModuleRoot = "com/gamebee/analytics";
        private const string AnalyticsClass = ModuleRoot + "/AnalyticsStore";

        internal static AndroidAnalyticsImp getInstance()
        {
            return new AndroidAnalyticsImp();
        }

        private AndroidJavaClass _analyticsClass;

        private AndroidAnalyticsImp()
        {
            _analyticsClass = new AndroidJavaClass(AnalyticsClass);
        }

        public void Log(string name)
        {
            _analyticsClass.CallStatic("log", name);
        }

        void IAnalyticsEvents.Log(string name, Dictionary<string, object> parameters)
        {
            var json = new JObject();
            foreach (var (k, v) in parameters)
            {
                json.Add(k, JToken.FromObject(v));
            }

            var str = json.ToString();
            _analyticsClass.CallStatic("log", name, str);
        }
    }
}
#endif