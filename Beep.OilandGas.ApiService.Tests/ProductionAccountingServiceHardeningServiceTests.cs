using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProductionAccounting.Exceptions;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Reflection;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionAccountingServiceHardeningServiceTests
{
    [Fact]
    public async Task AllocationService_ValidateAsync_RethrowsAllocationException_ForInvalidTotalVolume()
    {
        var service = new AllocationService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            new Mock<IPPDMMetadataRepository>().Object,
            new Mock<IAllocationEngine>().Object,
            NullLogger<AllocationService>.Instance);

        var allocation = new ALLOCATION_RESULT
        {
            ALLOCATION_RESULT_ID = "ALR-1",
            TOTAL_VOLUME = 0m
        };

        await Assert.ThrowsAsync<AllocationException>(() => service.ValidateAsync(allocation));
    }

    [Fact]
    public async Task InventoryService_ValidateAsync_RethrowsInvalidOperationException_ForMissingTankBatteryId()
    {
        var service = new InventoryService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            new Mock<IPPDMMetadataRepository>().Object,
            NullLogger<InventoryService>.Instance);

        var inventory = new TANK_INVENTORY
        {
            TANK_BATTERY_ID = string.Empty
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateAsync(inventory));
    }

    [Fact]
    public async Task RevenueService_ValidateAsync_RethrowsAccountingException_ForInvalidInterestPercentage()
    {
        var service = new RevenueService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            new Mock<IPPDMMetadataRepository>().Object,
            NullLogger<RevenueService>.Instance);

        var allocation = new REVENUE_ALLOCATION
        {
            REVENUE_ALLOCATION_ID = "REV-1",
            INTEREST_PERCENTAGE = 150m
        };

        await Assert.ThrowsAsync<AccountingException>(() => service.ValidateAsync(allocation));
    }

    [Fact]
    public async Task TakeOrPayService_GetScheduleAsync_ReturnsNull_WhenMetadataLookupFails()
    {
        var metadata = new Mock<IPPDMMetadataRepository>();
        metadata.Setup(x => x.GetTableMetadataAsync("TAKE_OR_PAY_SCHEDULE"))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = new TakeOrPayService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            metadata.Object,
            NullLogger<TakeOrPayService>.Instance);

        var result = await InvokePrivateAsync<TAKE_OR_PAY_SCHEDULE?>(
            service,
            "GetScheduleAsync",
            "SC-1",
            DateTime.UtcNow,
            "PPDM39");

        Assert.Null(result);
    }

    [Fact]
    public async Task CopasOverheadService_GetScheduleRateAsync_ReturnsZero_WhenMetadataLookupFails()
    {
        var metadata = new Mock<IPPDMMetadataRepository>();
        metadata.Setup(x => x.GetTableMetadataAsync("COPAS_OVERHEAD_SCHEDULE"))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = new CopasOverheadService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            metadata.Object,
            NullLogger<CopasOverheadService>.Instance);

        var result = await InvokePrivateAsync<decimal>(
            service,
            "GetScheduleRateAsync",
            "LEASE-1",
            DateTime.UtcNow,
            "PPDM39");

        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task CopasOverheadService_RecordOverheadAuditAsync_DoesNotThrow_WhenMetadataLookupFails()
    {
        var metadata = new Mock<IPPDMMetadataRepository>();
        metadata.Setup(x => x.GetTableMetadataAsync("COPAS_OVERHEAD_SCHEDULE"))
            .ThrowsAsync(new InvalidOperationException("metadata unavailable"));

        var service = new CopasOverheadService(
            new Mock<IDMEEditor>().Object,
            new Mock<ICommonColumnHandler>().Object,
            new Mock<IPPDM39DefaultsRepository>().Object,
            metadata.Object,
            NullLogger<CopasOverheadService>.Instance);

        await InvokePrivateAsync(
            service,
            "RecordOverheadAuditAsync",
            "LEASE-2",
            DateTime.UtcNow,
            "user-1",
            "PPDM39");
    }

    private static async Task<T> InvokePrivateAsync<T>(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        var task = (Task<T>)method!.Invoke(target, args)!;
        return await task;
    }

    private static async Task InvokePrivateAsync(object target, string methodName, params object[] args)
    {
        var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);

        var task = (Task)method!.Invoke(target, args)!;
        await task;
    }
}
