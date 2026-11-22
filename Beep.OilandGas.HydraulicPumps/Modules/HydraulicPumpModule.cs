using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.HydraulicPumps;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;

namespace Beep.OilandGas.HydraulicPumps.Modules;

/// <summary>
/// Registers the hydraulic pump entity types: the well and pump inputs, the two results, and the saved design (LFT-06).
/// </summary>
/// <remarks>
/// The four calculation entities existed in the models but no module setup registered them, so a provisioned database
/// never created them. This module closes that gap and adds <see cref="HYDRAULIC_PUMP_DESIGN"/>, the record a design is
/// saved into. There is no reference set to seed: the nozzle, throat, piston and stroke sizes are the module's constants.
/// </remarks>
public sealed class HydraulicPumpModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(HYDRAULIC_PUMP_DESIGN),
        typeof(HYDRAULIC_PUMP_WELL_PROPERTIES),
        typeof(HYDRAULIC_JET_PUMP_PROPERTIES),
        typeof(HYDRAULIC_JET_PUMP_RESULT),
        typeof(HYDRAULIC_PISTON_PUMP_PROPERTIES),
        typeof(HYDRAULIC_PISTON_PUMP_RESULT),
    };

    public HydraulicPumpModule(ModuleSetupContext context)
        : base(context)
    {
    }

    public override string ModuleId => "HYDRAULIC_PUMPS";

    public override string ModuleName => "Hydraulic pumps (jet and piston)";

    /// <summary>After plunger lift (78), so the lift modules run in feature order.</summary>
    public override int Order => 79;

    public override IReadOnlyList<Type> EntityTypes => _entityTypes;

    public override Task<ModuleSetupResult> SeedAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        // No reference rows: the sizes are HydraulicPumpConstants, which travel with the assembly.
        var result = NewResult();

        return Task.FromResult(result);
    }
}
