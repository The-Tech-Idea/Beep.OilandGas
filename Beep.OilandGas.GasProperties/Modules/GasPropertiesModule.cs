using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.PPDM39.Core.Interfaces;

namespace Beep.OilandGas.GasProperties.Modules;

public sealed class GasPropertiesModule : IModuleSetup
{
    public string ModuleId => "GAS_PROPERTIES";
    public string ModuleName => "Gas Properties";
    public int Order => 72;
    public IReadOnlyList<Type> EntityTypes { get; } = new[] { typeof(GAS_COMPOSITION), typeof(GAS_COMPOSITION_COMPONENT) };
    public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ModuleSetupResult { ModuleId = ModuleId, ModuleName = ModuleName,
            Success = true, SkipReason = "No default gas compositions are seeded." });
    }
}
