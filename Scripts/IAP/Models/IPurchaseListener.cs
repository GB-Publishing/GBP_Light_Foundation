namespace Gamebee.IAP.Models
{
    internal interface IPurchaseListener
    {
        /// <summary>
        /// Called when the purchase store is initialized.
        /// Will be called only once per game session.
        /// </summary>
        /// <param name="error">if error is not Null Or Empty then initialization failed for some reason</param>
        internal void OnInitialized(string error);

        /// <summary>
        /// Called when the product metadata is available from the store.
        /// Will be called only once per game session.
        /// </summary>
        /// <param name="products"></param>
        internal void OnProductMetadataUpdate(GBProduct[] products);

        /// <summary>
        /// Called when the available products are updated.
        /// Will be called multiple times during the game session.
        /// Developer can use this array to check whether the product is available to be used by the player.
        /// <param name="availableProductNames">The names of the available products.</param>
        /// </summary>
        internal void OnAvailableProductsUpdate(string[] availableProductNames);


        /// <summary>
        /// Called when a purchase flow has finished.
        /// Developers can use this to update the UI with the purchase status.
        /// <param name="productName">The name of the product that was being purchased</param>
        /// <param name="error">Reason of purchase failure if it was not successful</param>
        ///</summary>
        internal void OnPurchaseUpdate(string productName, string error = null);
    }
}