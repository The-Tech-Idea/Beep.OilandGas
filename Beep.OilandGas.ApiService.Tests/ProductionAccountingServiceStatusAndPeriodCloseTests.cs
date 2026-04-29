using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Constants;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionAccountingServiceStatusAndPeriodCloseTests
{
    [Fact]
    public async Task GetAccountingStatusAsync_ReturnsFallbackSummary_WhenMetadataReadsFail()
    {
        var deps = CreateDependencies();
        deps.Metadata
            .Setup(x => x.GetTableMetadataAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = CreateService(deps);
        var asOfDate = new DateTime(2026, 4, 1);

        var result = await service.GetAccountingStatusAsync("FIELD-1", asOfDate);

        Assert.Equal("FIELD-1", result.FieldId);
        Assert.Equal(asOfDate, result.AsOfDate);
        Assert.Equal(0m, result.TotalProduction);
        Assert.Equal(0m, result.TotalRevenue);
        Assert.Equal(0m, result.TotalRoyalty);
        Assert.Equal(0m, result.TotalCosts);
        Assert.Equal(0m, result.NetIncome);
        Assert.Equal("SuccessfulEfforts", result.AccountingMethod);
        Assert.Equal(PeriodCloseStatusCodes.Open, result.PeriodStatus);
    }

    [Fact]
    public async Task ClosePeriodAsync_ThrowsArgumentException_WhenPeriodEndIsDefault()
    {
        var service = CreateService(CreateDependencies());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ClosePeriodAsync("FIELD-2", default, "user-1"));
    }

    [Fact]
    public async Task ClosePeriodAsync_ReturnsFalse_WhenPeriodCloseServiceReturnsFalse()
    {
        var deps = CreateDependencies();
        deps.PeriodClosingService
            .Setup(x => x.ClosePeriodAsync("FIELD-3", new DateTime(2026, 3, 31), "user-2", "PPDM39"))
            .ReturnsAsync(false);

        var service = CreateService(deps);
        var result = await service.ClosePeriodAsync("FIELD-3", new DateTime(2026, 3, 31), "user-2");

        Assert.False(result);
        deps.PeriodClosingService.VerifyAll();
    }

    [Fact]
    public async Task ClosePeriodAsync_ThrowsProductionAccountingException_WhenPeriodCloseServiceThrows()
    {
        var deps = CreateDependencies();
        deps.PeriodClosingService
            .Setup(x => x.ClosePeriodAsync("FIELD-4", new DateTime(2026, 3, 31), "user-3", "PPDM39"))
            .ThrowsAsync(new InvalidOperationException("close failed"));

        var service = CreateService(deps);

        var ex = await Assert.ThrowsAsync<ProductionAccountingException>(() =>
            service.ClosePeriodAsync("FIELD-4", new DateTime(2026, 3, 31), "user-3"));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("FIELD-4", ex.Message);
    }

    private static ProductionAccountingService CreateService(TestDependencies d)
    {
        return new ProductionAccountingService(
            d.AllocationService.Object,
            d.RoyaltyService.Object,
            d.JibService.Object,
            d.ImbalanceService.Object,
            d.SuccessfulEffortsService.Object,
            d.FullCostService.Object,
            d.AmortizationService.Object,
            d.JournalEntryService.Object,
            d.RevenueService.Object,
            d.MeasurementService.Object,
            d.PricingService.Object,
            d.InventoryService.Object,
            d.PeriodClosingService.Object,
            d.Metadata.Object,
            d.Editor.Object,
            d.CommonColumnHandler.Object,
            d.Defaults.Object,
            NullLogger<ProductionAccountingService>.Instance);
    }

    private static TestDependencies CreateDependencies() => new();

    private sealed class TestDependencies
    {
        public Mock<IAllocationService> AllocationService { get; } = new();
        public Mock<IRoyaltyService> RoyaltyService { get; } = new();
        public Mock<IJointInterestBillingService> JibService { get; } = new();
        public Mock<IImbalanceService> ImbalanceService { get; } = new();
        public Mock<ISuccessfulEffortsService> SuccessfulEffortsService { get; } = new();
        public Mock<IFullCostService> FullCostService { get; } = new();
        public Mock<IAmortizationService> AmortizationService { get; } = new();
        public Mock<IJournalEntryService> JournalEntryService { get; } = new();
        public Mock<IRevenueService> RevenueService { get; } = new();
        public Mock<IMeasurementService> MeasurementService { get; } = new();
        public Mock<IPricingService> PricingService { get; } = new();
        public Mock<IInventoryService> InventoryService { get; } = new();
        public Mock<IPeriodClosingService> PeriodClosingService { get; } = new();
        public Mock<IPPDMMetadataRepository> Metadata { get; } = new();
        public Mock<IDMEEditor> Editor { get; } = new();
        public Mock<ICommonColumnHandler> CommonColumnHandler { get; } = new();
        public Mock<IPPDM39DefaultsRepository> Defaults { get; } = new();
    }
}
