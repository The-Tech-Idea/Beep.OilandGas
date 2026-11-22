using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.ModuleSetup;

namespace Beep.OilandGas.PumpPerformance.Modules;

/// <summary>
/// Registers the pump-performance entity types: the ESP string (design properties, result, motor, cable and curve points)
/// and the duty-point analysis.
/// </summary>
/// <remarks>
/// The ESP tables existed in the models but no module setup registered them, so a provisioned database never created them.
/// This module closes that gap and adds <see cref="PUMP_DESIGN"/>, the curve-checked duty point LFT-03 saves. There is no
/// pump-curve reference set to seed, so <see cref="SeedAsync"/> writes nothing and says so.
/// </remarks>
public sealed class PumpPerformanceModule : ModuleSetupBase
{
    private static readonly IReadOnlyList<Type> _entityTypes = new List<Type>
    {
        typeof(PUMP_DESIGN),
        typeof(ESP_DESIGN_PROPERTIES),
        typeof(ESP_DESIGN_RESULT),
        typeof(ESP_MOTOR_PROPERTIES),
        typeof(ESP_CABLE_PROPERTIES),
        typeof(ESP_PUMP_POINT),
    };

    public PumpPerformanceModule(ModuleSetupContext context)
        : base(context)
    {
    }

    public override string ModuleId => "PUMP_PERFORMANCE";

    public override string ModuleName => "Pump performance (duty point, ESP)";

    /// <summary>After gas lift (74), before production forecasting (75).</summary>
    public override int Order => 76;

    public override IReadOnlyList<Type> EntityTypes => _entityTypes;

    public override Task<ModuleSetupResult> SeedAsync(
        string connectionName,
        string userId,
        CancellationToken cancellationToken = default)
    {
        var result = NewResult();

        // No reference rows: there is no pump-curve catalog in this repository, so nothing is seeded.
        return Task.FromResult(result);
    }
}
