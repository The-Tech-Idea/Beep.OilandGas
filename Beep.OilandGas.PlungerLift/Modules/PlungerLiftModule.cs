using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;

namespace Beep.OilandGas.PlungerLift.Modules;

/// <summary>
/// Registers the plunger lift assessment record (LFT-05). The project's other types are calculation objects, not tables.
/// </summary>
/// <remarks>
/// There is no reference set to seed: the plunger types and their velocities are <c>PlungerLiftConstants</c>, which travel
/// with the assembly rather than with the database.
/// </remarks>
public sealed class PlungerLiftModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(PLUNGER_DESIGN),
    };

    public PlungerLiftModule(ModuleSetupContext context)
        : base(context)
    {
    }

    public override string ModuleId => "PLUNGER_LIFT";

    public override string ModuleName => "Plunger lift (feasibility and cycle)";

    public override int Order => 78;

    public override IReadOnlyList<Type> EntityTypes => _entityTypes;

    public override Task<ModuleSetupResult> SeedAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(NewResult());
}
