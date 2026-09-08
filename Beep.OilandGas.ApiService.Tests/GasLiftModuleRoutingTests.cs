using Beep.OilandGas.GasLift.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class GasLiftModuleRoutingTests
{
    [Theory]
    [InlineData("read")]
    [InlineData("save")]
    [InlineData("history")]
    public async Task ForecastingSeparatesHistoryFromForecastPersistence(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var columns = new Mock<ICommonColumnHandler>(MockBehavior.Strict);
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var requested = new List<string>();
        Task<string> Resolve(string module) { requested.Add(module); throw new InvalidOperationException("Unbound."); }
        var service = new Beep.OilandGas.ProductionForecasting.Services.ProductionForecastingService(
            editor.Object, columns.Object, defaults.Object, metadata.Object, "legacy-db",
            resolveConnection: () => Resolve("PRODUCTION_FORECASTING"),
            resolveHistoryConnection: () => Resolve("PPDM_CORE"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "read" => service.GetForecastAsync("forecast"),
            "save" => service.SaveForecastAsync(new(), "actor"),
            _ => service.GenerateForecastAsync(new Beep.OilandGas.Models.Data.ProductionForecasting.GenerateForecastRequest
            {
                WellUWI = "well", ForecastPeriod = 12,
                ForecastMethod = Beep.OilandGas.Models.Data.ProductionForecasting.ForecastType.Exponential
            })
        });
        Assert.Equal(operation == "history" ? "PPDM_CORE" : "PRODUCTION_FORECASTING", Assert.Single(requested));
        editor.VerifyNoOtherCalls();
        columns.VerifyNoOtherCalls();
        defaults.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("oil-read")]
    [InlineData("oil-save")]
    [InlineData("oil-history")]
    [InlineData("oil-result")]
    [InlineData("gas-read")]
    [InlineData("gas-save")]
    public async Task FluidPersistenceRejectsUnboundModule(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var columns = new Mock<ICommonColumnHandler>(MockBehavior.Strict);
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; throw new InvalidOperationException("Unbound."); }
        var oil = new Beep.OilandGas.OilProperties.Services.OilPropertiesService(
            editor.Object, columns.Object, defaults.Object, metadata.Object, "legacy-db", resolveConnection: Resolve);
        var gas = new Beep.OilandGas.GasProperties.Services.GasPropertiesService(
            editor.Object, columns.Object, defaults.Object, metadata.Object, "legacy-db", resolveConnection: Resolve);
        Assert.Equal(0, calls);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "oil-read" => oil.GetOilCompositionAsync("composition"),
            "oil-save" => oil.SaveOilCompositionAsync(new(), "actor"),
            "oil-history" => oil.GetOilPropertyHistoryAsync("composition"),
            "oil-result" => oil.SaveOilPropertyResultAsync(new(), "actor"),
            "gas-read" => gas.GetGasCompositionAsync("composition"),
            _ => gas.SaveGasCompositionAsync(new(), "actor")
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        columns.VerifyNoOtherCalls();
        defaults.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("nodal-read")]
    [InlineData("nodal-save")]
    [InlineData("flash-read")]
    [InlineData("flash-save")]
    public async Task NodalAndFlashRejectUnboundPersistence(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var columns = new Mock<ICommonColumnHandler>(MockBehavior.Strict);
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();
        var calls = 0;
        Task<string> Resolve(CancellationToken token)
        {
            Assert.Equal(cancellation.Token, token);
            calls++;
            throw new InvalidOperationException("Unbound.");
        }
        var nodal = new Beep.OilandGas.NodalAnalysis.Services.NodalAnalysisService(
            editor.Object, columns.Object, defaults.Object, metadata.Object, "legacy-db", resolveConnection: Resolve);
        var flash = new Beep.OilandGas.FlashCalculations.Services.FlashCalculationService(
            editor.Object, columns.Object, defaults.Object, metadata.Object, "legacy-db", resolveConnection: Resolve);
        Assert.Equal(0, calls);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "nodal-read" => nodal.GetAnalysisHistoryAsync("well", cancellation.Token),
            "nodal-save" => nodal.SaveAnalysisResultAsync(new() { WellUWI = "well" }, "actor", cancellation.Token),
            "flash-read" => flash.GetFlashHistoryAsync(cancellationToken: cancellation.Token),
            _ => flash.SaveFlashResultAsync(new(), "actor", cancellation.Token)
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        columns.VerifyNoOtherCalls();
        defaults.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EconomicsPersistenceRejectsUnboundModuleBeforeLegacyAccess(bool save)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var columns = new Mock<ICommonColumnHandler>(MockBehavior.Strict);
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        var service = new Beep.OilandGas.EconomicAnalysis.Services.EconomicAnalysisService(
            editor.Object, columns.Object, defaults.Object, metadata.Object, "legacy-db",
            resolveConnection: () => { calls++; throw new InvalidOperationException("Unbound."); });
        Assert.Equal(0, calls);
        await Assert.ThrowsAsync<InvalidOperationException>(() => save
            ? service.SaveAnalysisResultAsync("analysis", new(), "actor")
            : service.GetAnalysisResultAsync("analysis"));
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        columns.VerifyNoOtherCalls();
        defaults.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("read")]
    [InlineData("design")]
    [InlineData("performance")]
    public async Task PersistenceResolvesBindingBeforeOpeningLegacyDatabase(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var columns = new Mock<ICommonColumnHandler>(MockBehavior.Strict);
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();
        var calls = 0;
        var service = new GasLiftService(editor.Object, columns.Object, defaults.Object, metadata.Object,
            "legacy-db", resolveConnection: token =>
            {
                Assert.Equal(cancellation.Token, token);
                calls++;
                throw new InvalidOperationException("Module is not bound.");
            });
        Assert.Equal(0, calls);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "read" => service.GetGasLiftPerformanceAsync("well", cancellation.Token),
            "design" => service.SaveGasLiftDesignAsync(new() { DESIGN_ID = "design", WELL_UWI = "well" }, "actor", cancellation.Token),
            _ => service.SavePerformanceDataAsync(new() { WELL_UWI = "well" }, "actor", cancellation.Token)
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        columns.VerifyNoOtherCalls();
        defaults.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }
}
