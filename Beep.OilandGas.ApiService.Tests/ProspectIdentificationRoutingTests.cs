using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProspectIdentification.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class ProspectIdentificationRoutingTests
{
    [Theory]
    [InlineData("list")]
    [InlineData("create")]
    [InlineData("evaluate")]
    [InlineData("rank")]
    public async Task ProspectStorageCannotFallBackToGlobalConnection(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; throw new InvalidOperationException("Unbound"); }
        var service = new ProspectIdentificationService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "global-db", resolveConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "list" => service.GetProspectsAsync(),
            "create" => service.CreateProspectAsync(new() { ProspectId = "prospect" }, "actor"),
            "evaluate" => service.EvaluateProspectAsync("prospect"),
            _ => service.RankProspectsAsync(["prospect"], new() { ["RiskFactor"] = 1m })
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
    }
}
