namespace Gamebee.RemoteConfig
{
    public interface IRemoteConfigStoreImp
    {
        public void RegisterListener(IRemoteConfigListener listener);
        public void Fetch();
    }

    public interface IRemoteConfigListener
    {
        void OnFetched(string data);
        void OnFailed(string error);
    }
}