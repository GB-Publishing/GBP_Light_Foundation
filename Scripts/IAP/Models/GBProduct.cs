using System;

namespace Gamebee.IAP.Models
{
    [Serializable]
    public class GBProduct
    {
        public enum ProductType
        {
            Consumable,
            NonConsumable,
            Subscription
        }

        public enum SubscriptionTerm
        {
            NA,
            Weekly,
            Monthly,
            Quarterly,
            Annually
        }

        public string Name;
        public string Title;
        public string Description;
        public string Price;
        public ProductType Type;
        public SubscriptionTerm Term;
    }
}