using System.Reflection;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProspectIdentification;
using Beep.OilandGas.ProspectIdentification.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class ProspectEvaluationRoutingTests
{
    [Theory]
    [InlineData("list")]
    [InlineData("read")]
    [InlineData("evaluate")]
    [InlineData("seismic")]
    public async Task StorageFactoriesRequireTheirOwningModule(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        string? selected = null;
        Task<string> Resolve(string module) { selected = module; throw new InvalidOperationException("Unbound"); }
        var service = new ProspectEvaluationService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(), "global-db",
            resolveModuleConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "list" => service.GetProspectsAsync(),
            "read" => service.GetProspectAsync("prospect"),
            "evaluate" => service.EvaluateProspectAsync("prospect", new()),
            _ => (Task)typeof(ProspectEvaluationService).GetMethod("CreateSeismicSurveyRepositoryAsync",
                BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(service, null)!
        });
        Assert.Equal(operation == "seismic" ? "PPDM_CORE" : ExplorationReferenceCodes.ExplorationModuleRegistryId, selected);
        editor.VerifyNoOtherCalls();
    }
}
