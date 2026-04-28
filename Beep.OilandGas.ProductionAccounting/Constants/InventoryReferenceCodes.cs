using System;

namespace Beep.OilandGas.ProductionAccounting.Constants
{
    /// <summary>
    /// <c>INVENTORY_TRANSACTION.TRANSACTION_TYPE</c> values used by valuation and reconciliation.
    /// </summary>
    public static class InventoryTransactionTypeCodes
    {
        public const string Receipt = "RECEIPT";
        public const string Purchase = "PURCHASE";
        public const string In = "IN";
        public const string Issue = "ISSUE";
        public const string Sale = "SALE";
        public const string Out = "OUT";
    }

    /// <summary><c>INVENTORY_VALUATION.VALUATION_METHOD</c> and costing layer consumption order.</summary>
    public static class InventoryValuationMethodCodes
    {
        public const string WeightedAvg = "WEIGHTED_AVG";
        public const string Lifo = "LIFO";
    }

    /// <summary><c>INVENTORY_ADJUSTMENT.ADJUSTMENT_TYPE</c> for LCM and similar book adjustments.</summary>
    public static class InventoryAdjustmentTypeCodes
    {
        public const string LcmWritedown = "LCM_WRITEDOWN";
    }

    /// <summary><c>PRICE_INDEX.COMMODITY_TYPE</c> tokens used when resolving market price for LCM.</summary>
    public static class PriceIndexCommodityTypeCodes
    {
        public const string Oil = "OIL";
        public const string Gas = "GAS";
        public const string Ngl = "NGL";

        /// <summary>Values seeded under <c>PRICE_INDEX_COMMODITY_TYPE</c>.</summary>
        public static readonly string[] AllSeeded = { Oil, Gas, Ngl };

        /// <summary>True if <paramref name="commodityType"/> matches a seeded code (case-insensitive).</summary>
        public static bool IsSeededCommodityType(string? commodityType) =>
            !string.IsNullOrWhiteSpace(commodityType) &&
            Array.Exists(AllSeeded, c => string.Equals(c, commodityType, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Keys in <c>INVENTORY_ITEM.REMARK</c> for NRV-style adjustments.</summary>
    public static class InventoryItemRemarkKeys
    {
        public const string TransportCost = "TRANSPORT_COST";
        public const string QualityDed = "QUALITY_DED";
        public const string QualityAdj = "QUALITY_ADJ";
    }

    /// <summary>Human-readable <c>INVENTORY_ADJUSTMENT.REASON</c> text for LCM flows.</summary>
    public static class InventoryAdjustmentReasonPhrases
    {
        public const string LowerOfCostOrMarket = "Lower of cost or market";
    }
}
