using System;
using System.Collections.Generic;

namespace Gamebee.Analytics
{
    public class AnalyticsStore
    {
        private static AnalyticsStore instance;
        internal IAnalyticsEvents AnalyticsImp;

        internal void Init(GBPInitData data)
        {
            instance = this;
        }

        public static void Log(string name)
        {
            try
            {
                instance?.AnalyticsImp?.Log(name);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        public static void Log(string name, Dictionary<string, object> parameters)
        {
            try
            {
                instance?.AnalyticsImp?.Log(name, parameters);
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}