using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.PPDM39.Core.Interfaces;

namespace Beep.OilandGas.OilProperties.Modules;

public sealed class OilPropertiesModule : IModuleSetup
{
    public string ModuleId => "OIL_PROPERTIES";
    public string ModuleName => "Oil Properties";
    public int Order => 71;
    public IReadOnlyList<Type> EntityTypes { get; } = new[] { typeof(OIL_COMPOSITION), typeof(OIL_PROPERTY_RESULT) };
    public Task<ModuleSetupResult> SeedAsync(string connectionName, string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ModuleSetupResult { ModuleId = ModuleId, ModuleName = ModuleName,
            Success = true, SkipReason = "No default oil compositions or results are seeded." });
    }
}
