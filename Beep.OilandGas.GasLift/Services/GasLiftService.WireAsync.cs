using System;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.GasLift.Mapping;
using Beep.OilandGas.GasLift.Validation;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.GasLift.Services;

public partial class GasLiftService
{
    /// <inheritdoc />
    public Task<GAS_LIFT_POTENTIAL_RESULT> AnalyzeGasLiftPotentialAsync(
        GAS_LIFT_WELL_PROPERTIES wellProperties,
        decimal minGasInjectionRate,
        decimal maxGasInjectionRate,
        int numberOfPoints = 50,
        CancellationToken cancellationToken = default)
    {
        if (wellProperties == null)
            throw new ArgumentNullException(nameof(wellProperties));
        if (numberOfPoints < 2)
            throw new ArgumentOutOfRangeException(nameof(numberOfPoints), numberOfPoints, "Number of analysis points must be at least 2 (matches request validation range).");
        if (minGasInjectionRate > maxGasInjectionRate)
            throw new ArgumentException("Minimum gas injection rate must not exceed maximum.", nameof(minGasInjectionRate));

        cancellationToken.ThrowIfCancellationRequested();
        GasLiftValidator.ValidateWellProperties(wellProperties);
        GasLiftValidator.ValidateGasInjectionRate(minGasInjectionRate);
        GasLiftValidator.ValidateGasInjectionRate(maxGasInjectionRate);

        _logger?.LogInformation(
            "Analyzing gas lift potential (async) for well {WellUWI}: Range {MinRate}-{MaxRate} Mscf/day",
            wellProperties.WELL_UWI, minGasInjectionRate, maxGasInjectionRate);

        var raw = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(
            wellProperties, minGasInjectionRate, maxGasInjectionRate, numberOfPoints, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        var mapped = GasLiftPotentialWireMapper.ToPotentialResult(wellProperties, raw);

        _logger?.LogInformation(
            "Gas lift potential analysis (async) completed: OptimalGasInjectionRate={Rate} Mscf/day, MaximumProductionRate={Production} BPD",
            mapped.OPTIMAL_GAS_INJECTION_RATE, mapped.MAXIMUM_PRODUCTION_RATE);

        return Task.FromResult(mapped);
    }

    /// <inheritdoc />
    public Task<GAS_LIFT_VALVE_DESIGN_RESULT> DesignValvesAsync(
        GAS_LIFT_WELL_PROPERTIES wellProperties,
        decimal gasInjectionPressure,
        int numberOfValves,
        bool useSIUnits = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = DesignValves(wellProperties, gasInjectionPressure, numberOfValves, useSIUnits);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(result);
    }
}
