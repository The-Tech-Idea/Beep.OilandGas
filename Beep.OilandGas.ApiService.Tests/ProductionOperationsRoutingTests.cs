using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionOperations.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class ProductionOperationsRoutingTests
{
    [Theory]
    [InlineData("volumes", "PPDM_CORE")]
    [InlineData("well", "PPDM_CORE")]
    [InlineData("maintenance", "PPDM_CORE")]
    [InlineData("cost", "PRODUCTION")]
    public async Task ReadsResolveTheirStorageOwnerBeforeDatasourceAccess(string operation, string expected)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        string? selected = null;
        Task<string> Resolve(string module) { selected = module; throw new InvalidOperationException("Unbound"); }
        var service = new ProductionOperationsService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            Mock.Of<IFacilityManagementService>(), "global-db", resolveModuleConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "volumes" => service.GetProductionDataAsync("well", null, DateTime.UtcNow.AddDays(-1), DateTime.UtcNow),
            "well" => service.GetWellStatusAsync("well"),
            "maintenance" => service.GetUpcomingMaintenanceAsync(DateTime.UtcNow, DateTime.UtcNow.AddDays(1)),
            _ => service.GetOperationStatusAsync("operation")
        });
        Assert.Equal(expected, selected);
        editor.VerifyNoOtherCalls();
    }
}
