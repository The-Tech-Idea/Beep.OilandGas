using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.ProspectIdentification;
using Beep.OilandGas.ProspectIdentification.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class SeismicModuleRoutingTests
{
    [Theory]
    [InlineData("list")]
    [InlineData("read")]
    [InlineData("prospect")]
    public async Task SurveyAndProspectStorageRequireTheirOwnBindings(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        string? selected = null;
        Task<string> Resolve(string module) { selected = module; throw new InvalidOperationException("Unbound"); }
        var service = new SeismicAnalysisService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            "global-db", resolveModuleConnection: Resolve);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "list" => service.GetSeismicSurveysAsync(),
            "read" => service.GetSeismicSurveyAsync("survey"),
            _ => service.CreateSeismicSurveyAsync(new() { ProspectId = "prospect" }, "actor")
        });
        Assert.Equal(operation == "prospect" ? ExplorationReferenceCodes.ExplorationModuleRegistryId : "PPDM_CORE", selected);
        editor.VerifyNoOtherCalls();
    }
}
