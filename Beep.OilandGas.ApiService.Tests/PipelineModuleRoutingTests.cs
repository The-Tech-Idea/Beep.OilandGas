using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PipelineAnalysis.Modules;
using Beep.OilandGas.PipelineAnalysis.Services;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class PipelineModuleRoutingTests
{
    [Theory]
    [InlineData("save")]
    [InlineData("history")]
    [InlineData("read")]
    [InlineData("update")]
    public async Task PersistenceRequiresModuleBindingBeforeLegacyAccess(string operation)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var columns = new Mock<ICommonColumnHandler>(MockBehavior.Strict);
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Strict);
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Strict);
        var calls = 0;
        Task<string> Resolve() { calls++; throw new InvalidOperationException("Unbound."); }
        var service = new PipelineAnalysisService(editor.Object, columns.Object, defaults.Object,
            metadata.Object, "legacy-db", resolveConnection: Resolve);
        Assert.Equal(0, calls);
        await Assert.ThrowsAsync<InvalidOperationException>(() => operation switch
        {
            "save" => service.SaveAnalysisResultsAsync(new(), "actor"),
            "history" => service.GetAnalysisHistoryAsync("pipeline", DateTime.UtcNow.AddDays(-1), DateTime.UtcNow),
            "read" => service.GetPipelineConfigurationAsync("pipeline"),
            _ => service.UpdatePipelineConfigurationAsync(new(), "actor")
        });
        Assert.Equal(1, calls);
        editor.VerifyNoOtherCalls();
        columns.VerifyNoOtherCalls();
        defaults.VerifyNoOtherCalls();
        metadata.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ModuleDeclaresPersistedTablesWithoutSampleData()
    {
        var module = new PipelineAnalysisModule();
        Assert.Equal("PIPELINE_ANALYSIS", module.ModuleId);
        Assert.Equal(new[] { typeof(Beep.OilandGas.Models.Data.PipelineAnalysis.PIPELINE),
            typeof(Beep.OilandGas.Models.Data.PipelineAnalysis.PIPELINE_ANALYSIS_RESULT) }, module.EntityTypes);
        var seed = await module.SeedAsync("selected-db", "actor");
        Assert.True(seed.Success);
        Assert.Equal(0, seed.RecordsInserted);
    }
}
