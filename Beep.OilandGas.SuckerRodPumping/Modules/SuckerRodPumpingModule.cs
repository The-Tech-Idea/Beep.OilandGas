using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;

namespace Beep.OilandGas.SuckerRodPumping.Modules;

/// <summary>
/// Registers the sucker rod design record (LFT-04). The project's other types are calculation objects, not tables.
/// </summary>
/// <remarks>
/// There is no reference set to seed: the rod grades, diameters and pump sizes are the module's constants
/// (<c>SuckerRodConstants</c>), which travel with the assembly rather than with the database.
/// </remarks>
public sealed class SuckerRodPumpingModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(SRP_DESIGN),
    };

    public SuckerRodPumpingModule(ModuleSetupContext context)
        : base(context)
    {
    }

    public override string ModuleId => "SUCKER_ROD_PUMPING";

    public override string ModuleName => "Sucker rod pumping (rod string design)";

    /// <summary>After pump performance (76), before production forecasting (75) is not possible, so after both.</summary>
    public override int Order => 77;

    public override IReadOnlyList<Type> EntityTypes => _entityTypes;

    public override Task<ModuleSetupResult> SeedAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(NewResult());
}
