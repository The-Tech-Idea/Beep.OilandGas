using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.PPDM39.Core.Interfaces;

namespace Beep.OilandGas.PipelineAnalysis.Modules;

public sealed class PipelineAnalysisModule : IModuleSetup
{
    public string ModuleId => "PIPELINE_ANALYSIS";
    public string ModuleName => "Pipeline Analysis";
    public int Order => 79;
    public IReadOnlyList<Type> EntityTypes { get; } = new[] { typeof(PIPELINE), typeof(PIPELINE_ANALYSIS_RESULT) };
    public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ModuleSetupResult { ModuleId = ModuleId, ModuleName = ModuleName,
            Success = true, SkipReason = "No sample pipelines or analysis results are seeded." });
    }
}
