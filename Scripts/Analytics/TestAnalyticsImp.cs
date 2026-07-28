#if UNITY_EDITOR || GBP_DEBUG_MODE
using System.Collections.Generic;
using UnityEngine;

namespace Gamebee.Analytics
{
    public class TestAnalyticsImp : IAnalyticsEvents
    {
        public void Log(string name)
        {
            Debug.Log("Analytics Event: " + name);
        }

        public void Log(string name, Dictionary<string, object> parameters)
        {
            if (parameters == null || parameters.Count == 0)
                Debug.Log("Analytics Event: " + name);
            else
                Debug.Log("Analytics Event: " + name + " with parameters: " + parameters);
        }
    }
}
#endif