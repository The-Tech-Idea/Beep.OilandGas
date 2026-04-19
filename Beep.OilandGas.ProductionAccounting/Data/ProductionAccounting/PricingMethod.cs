
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public enum PricingMethod
    {
        /// <summary>
        /// Fixed price per barrel.
        /// </summary>
        Fixed,

        /// <summary>
        /// Index-based pricing (WTI, Brent, etc.).
        /// </summary>
        IndexBased,

        /// <summary>
        /// Posted price.
        /// </summary>
        PostedPrice,

        /// <summary>
        /// Spot price.
        /// </summary>
        SpotPrice,

        /// <summary>
        /// Regulated pricing.
        /// </summary>
        Regulated
    }
}
