using System.Collections.Generic;

namespace Gamebee.Analytics
{
    public interface IAnalyticsEvents
    {
        public void Log(string name);
        public void Log(string name, Dictionary<string, object> parameters);
    }
}