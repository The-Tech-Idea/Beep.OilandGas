using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProductionOperations.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class ProductionManagementRoutingTests
{
    [Theory]
    [InlineData("list")]
    [InlineData("read")]
    [InlineData("create")]
    [InlineData("reports")]
    [InlineData("well")]
    [InlineData("facility")]
    [InlineData("declarations")]
    public async Task AllPersistencePathsRequireBindingBeforeDatasourceAccess(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        using var cancellation = new CancellationTokenSource();
        var calls = 0;
        Task<string> Resolve(CancellationToken token)
        {
            Assert.Equal(cancellation.Token, token);
            calls++;
            throw new InvalidOperationException("Unbound");
        }
        var service = new ProductionManagementService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "global-db", Resolve);
        var token = cancellation.Token;
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "list" => service.GetProductionOperationsAsync(cancellationToken: token),
            "read" => service.GetProductionOperationAsync("operation", token),
            "create" => service.CreateProductionOperationAsync(new(), token),
            "reports" => service.GetProductionReportsAsync(cancellationToken: token),
            "well" => service.GetWellOperationsAsync("well", token),
            "facility" => service.GetFacilityOperationsAsync("facility", token),
            _ => service.ListFacilityPdenDeclarationsAsync(cancellationToken: token)
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
