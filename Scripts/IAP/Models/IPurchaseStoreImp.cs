namespace Gamebee.IAP.Models
{
    internal interface IPurchaseStoreImp
    {
        public void InitProducts(string[] productNames);
        public void RegisterListener(IPurchaseListener listener);
        public void RestorePurchases();
        public void Purchase(string productName);
        public void ReportAsConsumed(string productName);
    }
}