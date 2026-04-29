using System;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Constants;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionAccountingReferenceSeedCatalogTests
{
    [Fact]
    public void GetAllSeedRows_HasUniqueReferenceSetReferenceCodePairs()
    {
        var rows = ProductionAccountingReferenceCodeSeed.GetAllSeedRows().ToList();

        var duplicateKeys = rows
            .GroupBy(r => (r.ReferenceSet, r.ReferenceCode))
            .Where(g => g.Count() > 1)
            .Select(g => $"{g.Key.ReferenceSet}/{g.Key.ReferenceCode}")
            .ToList();

        Assert.Empty(duplicateKeys);
    }

    [Fact]
    public void GetAllSeedRows_ContainsKeyFamiliesUsedByActiveApis()
    {
        var rows = ProductionAccountingReferenceCodeSeed.GetAllSeedRows().ToList();

        Assert.Contains(rows, r => r.ReferenceSet == "ALLOCATION_METHOD" && r.ReferenceCode == AllocationMethods.ProRata);
        Assert.Contains(rows, r => r.ReferenceSet == "ROYALTY_PAYMENT_STATUS" && r.ReferenceCode == RoyaltyPaymentStatusCodes.Pending);
        Assert.Contains(rows, r => r.ReferenceSet == "REVENUE_RECOGNITION_STATUS" && r.ReferenceCode == RevenueRecognitionStatusCodes.Recognized);
        Assert.Contains(rows, r => r.ReferenceSet == "PERIOD_CLOSE_STATUS" && r.ReferenceCode == PeriodCloseStatusCodes.Open);
        Assert.Contains(rows, r => r.ReferenceSet == "AFE_STATUS" && r.ReferenceCode == AfeStatusCodes.Draft);
        Assert.Contains(rows, r => r.ReferenceSet == "INVENTORY_VALUATION_METHOD" && r.ReferenceCode == InventoryValuationMethodCodes.WeightedAvg);
        Assert.Contains(rows, r => r.ReferenceSet == "PRICE_INDEX_COMMODITY_TYPE" && r.ReferenceCode == PriceIndexCommodityTypeCodes.Oil);
    }
}
