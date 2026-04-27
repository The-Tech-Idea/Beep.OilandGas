using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionAccountingServiceProcessCycleTests
{
    [Fact]
    public async Task ProcessProductionCycleAsync_ReturnsFalse_WhenMeasurementRecordingFails()
    {
        var deps = CreateDependencies();
        deps.MeasurementService
            .Setup(x => x.RecordAsync(It.IsAny<RUN_TICKET>(), "user-1", "PPDM39"))
            .ReturnsAsync((MEASUREMENT_RECORD?)null);

        var service = CreateService(deps);
        var result = await service.ProcessProductionCycleAsync(CreateRunTicket("RT-1"), "user-1");

        Assert.False(result);
        deps.AllocationService.Verify(
            x => x.AllocateAsync(It.IsAny<RUN_TICKET>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessProductionCycleAsync_ReturnsFalse_WhenAllocationFails()
    {
        var deps = CreateDependencies();
        deps.MeasurementService
            .Setup(x => x.RecordAsync(It.IsAny<RUN_TICKET>(), "user-2", "PPDM39"))
            .ReturnsAsync(new MEASUREMENT_RECORD());
        deps.AllocationService
            .Setup(x => x.AllocateAsync(It.IsAny<RUN_TICKET>(), "ProRata", "user-2", "PPDM39"))
            .ReturnsAsync((ALLOCATION_RESULT?)null);

        var service = CreateService(deps);
        var result = await service.ProcessProductionCycleAsync(CreateRunTicket("RT-2"), "user-2");

        Assert.False(result);
        deps.RoyaltyService.Verify(
            x => x.CalculateAsync(It.IsAny<ALLOCATION_DETAIL>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessProductionCycleAsync_ReturnsFalse_WhenAllocationDetailsMissing()
    {
        var deps = CreateDependencies();
        deps.MeasurementService
            .Setup(x => x.RecordAsync(It.IsAny<RUN_TICKET>(), "user-3", "PPDM39"))
            .ReturnsAsync(new MEASUREMENT_RECORD());
        deps.AllocationService
            .Setup(x => x.AllocateAsync(It.IsAny<RUN_TICKET>(), "ProRata", "user-3", "PPDM39"))
            .ReturnsAsync(new ALLOCATION_RESULT { ALLOCATION_RESULT_ID = "AR-1" });
        deps.AllocationService
            .Setup(x => x.GetDetailsAsync("AR-1", "PPDM39"))
            .ReturnsAsync([]);

        var service = CreateService(deps);
        var result = await service.ProcessProductionCycleAsync(CreateRunTicket("RT-3"), "user-3");

        Assert.False(result);
    }

    [Fact]
    public async Task ProcessProductionCycleAsync_ThrowsProductionAccountingException_WhenUnexpectedErrorOccurs()
    {
        var deps = CreateDependencies();
        deps.MeasurementService
            .Setup(x => x.RecordAsync(It.IsAny<RUN_TICKET>(), "user-4", "PPDM39"))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var service = CreateService(deps);

        var ex = await Assert.ThrowsAsync<ProductionAccountingException>(
            () => service.ProcessProductionCycleAsync(CreateRunTicket("RT-4"), "user-4"));

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("RT-4", ex.Message);
    }

    private static RUN_TICKET CreateRunTicket(string id) => new()
    {
        RUN_TICKET_ID = id,
        RUN_TICKET_NUMBER = id
    };

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

