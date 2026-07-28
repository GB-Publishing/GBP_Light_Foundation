namespace Gamebee.Ad.Models
{
    internal interface IAdListener
    {
        public void OnInitialized(string error);
        public void OnAdLoaded(AdType type);
        public void OnAdShown(AdType type, string error);
        public void OnAdClosed(AdType type, bool rewarded);
    }
}