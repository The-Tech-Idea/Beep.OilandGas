using Beep.OilandGas.Models.Data.HeatMap;
using Beep.OilandGas.PPDM39.Core.Interfaces;

namespace Beep.OilandGas.HeatMap.Modules;

public sealed class HeatMapModule : IModuleSetup
{
    public string ModuleId => "HEAT_MAP";
    public string ModuleName => "Heat Map";
    public int Order => 75;
    public IReadOnlyList<Type> EntityTypes { get; } = new[] { typeof(HEAT_MAP_CONFIGURATION) };

    public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ModuleSetupResult { ModuleId = ModuleId, ModuleName = ModuleName,
            Success = true, SkipReason = "No heat map configurations are seeded." });
    }
}
