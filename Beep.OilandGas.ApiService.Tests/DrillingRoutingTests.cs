using System.Reflection;
using Beep.OilandGas.DrillingAndConstruction.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingRoutingTests
{
    [Theory]
    [InlineData("GetWellRepositoryAsync")]
    [InlineData("GetDrillReportRepositoryAsync")]
    public async Task RepositoryFactoriesRequireBindingAndForwardCancellation(string factory)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        using var source = new CancellationTokenSource();
        var calls = 0;
        Task<string> Resolve(CancellationToken token)
        {
            Assert.Equal(source.Token, token);
            calls++;
            throw new InvalidOperationException("Unbound");
        }
        var service = new DrillingOperationService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "global-db", resolveConnection: Resolve);
        var method = typeof(DrillingOperationService).GetMethod(factory, BindingFlags.Instance | BindingFlags.NonPublic)!;
        await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)method.Invoke(service, [source.Token])!);
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
