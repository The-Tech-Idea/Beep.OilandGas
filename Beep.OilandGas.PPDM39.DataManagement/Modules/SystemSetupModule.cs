using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;

namespace Beep.OilandGas.PPDM39.DataManagement.Modules;

/// <summary>Installation-wide setup data; migrated separately from tenant PPDM module databases.</summary>
public sealed class SystemSetupModule(ModuleSetupContext context) : ModuleSetupBase(context)
{
    public const string Id = "SYSTEM_SETUP";
    public override string ModuleId => Id;
    public override string ModuleName => "System setup repository";
    public override int Order => -10;
    public override IReadOnlyList<Type> EntityTypes { get; } = Array.AsReadOnly(new[] { typeof(SYSTEM_SETUP_RECORD), typeof(COMPANY_OILGAS_ENVIRONMENT), typeof(COMPANY_GHG_CATALOG_SELECTION) });

    public override Task<ModuleSetupResult> SeedAsync(string connectionName, string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ModuleSetupResult { ModuleId = ModuleId, ModuleName = ModuleName, Success = true,
            SkipReason = "Shared configuration is created through reviewed system setup commands." });
    }
}
