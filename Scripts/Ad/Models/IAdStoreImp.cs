using System.Collections;

namespace Gamebee.Ad.Models
{
    internal interface IAdStoreImp
    {
        internal void Init(IAdListener listener);
        internal IEnumerator ShowAd(AdType type);
    }
}