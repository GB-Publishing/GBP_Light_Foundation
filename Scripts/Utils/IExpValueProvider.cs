namespace Gamebee.Utils
{
    public interface IExpValueProvider
    {
        /// <summary>
        /// Device Model provided by <code>SystemInfo.deviceModel.ToLower()</code>
        /// </summary>
        /// <returns></returns>
        public string DeviceModel();

        /// <summary>
        /// Provides the current time zone of the user
        /// </summary>
        /// <returns>
        /// C# time zone ID. Example: "America/New_York".
        /// <code>TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id).ToArray()</code>
        /// </returns>
        public string TimeZone();

        /// <summary>
        /// Provides the current Unix time stamp of the device
        /// </summary>
        /// <returns></returns>
        public long TimeStamp();

        /// <summary>
        /// Provides the current locale of the user
        /// </summary>
        /// <returns>
        /// C# locale ID. Example: "en".
        /// <code>CultureInfo.GetCultures(CultureTypes.AllCultures).Select(c => c.Name).ToArray()</code>
        /// </returns>
        public string Locale();

        /// <summary>
        /// Whether the user is using a TV device
        /// </summary>
        /// <returns>True if the user is using a TV device</returns> 
        public bool IsTV();

        /// <summary>
        /// Launch count of the application
        /// </summary>
        /// <returns></returns>
        public int LaunchCount();

        /// <summary>
        /// Screen width in pixels of the device
        /// </summary>
        /// <returns></returns>
        public int ScreenWidth();

        /// <summary>
        /// Screen height in pixels of the device
        /// </summary>
        /// <returns></returns>
        public int ScreenHeight();


        /// <summary>
        /// Application version (1.2.3, 1.4.4, 2.3.1)
        /// </summary>
        /// <returns></returns>
        public string Version();

        /// <summary>
        /// PlayerPrefs data
        /// </summary>
        /// <param name="key"></param>
        /// <param name="def"></param>
        /// <returns></returns>
        public int PP(string key, int def);

        public float PP(string key, float def);
        public string PP(string key, string def);
        public bool PP(string key, bool def);

        public bool IsUS();
        public bool IsCA();
        public bool IsEU();
        public bool IsJP();
        public bool IsAU();
        public bool IsBR();

        public bool IsOSX();
        public bool IsIOS();
        public bool IsTVOS();
    }
}