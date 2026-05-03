using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.GasLift.Mapping;

/// <summary>
/// Maps calculator output on <see cref="GAS_LIFT_WELL_PROPERTIES"/> to API / persistence wire type <see cref="GAS_LIFT_POTENTIAL_RESULT"/>.
/// </summary>
public static class GasLiftPotentialWireMapper
{
    /// <summary>
    /// Builds a <see cref="GAS_LIFT_POTENTIAL_RESULT"/> from the screening potential run (performance points live on the analysis object as <see cref="GasLiftPerformancePoint"/>).
    /// </summary>
    public static GAS_LIFT_POTENTIAL_RESULT ToPotentialResult(
        GAS_LIFT_WELL_PROPERTIES request,
        GAS_LIFT_WELL_PROPERTIES analysisOutput)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (analysisOutput == null) throw new ArgumentNullException(nameof(analysisOutput));

        var potentialResultId = Guid.NewGuid().ToString("N");
        var performancePoints = new List<GAS_LIFT_PERFORMANCE_POINT>();

        if (analysisOutput.PERFORMANCE_POINTS != null)
        {
            for (var index = 0; index < analysisOutput.PERFORMANCE_POINTS.Count; index++)
            {
                var point = analysisOutput.PERFORMANCE_POINTS[index];
                performancePoints.Add(new GAS_LIFT_PERFORMANCE_POINT
                {
                    GAS_LIFT_PERFORMANCE_POINT_ID = Guid.NewGuid().ToString("N"),
                    GAS_LIFT_POTENTIAL_RESULT_ID = potentialResultId,
                    GAS_INJECTION_RATE = point.GasInjectionRate,
                    PRODUCTION_RATE = point.ProductionRate,
                    GAS_LIQUID_RATIO = point.GasLiquidRatio,
                    BOTTOM_HOLE_PRESSURE = point.BottomHolePressure,
                    POINT_ORDER = index + 1
                });
            }
        }

        return new GAS_LIFT_POTENTIAL_RESULT
        {
            GAS_LIFT_POTENTIAL_RESULT_ID = potentialResultId,
            GAS_LIFT_WELL_PROPERTIES_ID = string.IsNullOrWhiteSpace(request.GAS_LIFT_WELL_PROPERTIES_ID)
                ? analysisOutput.GAS_LIFT_WELL_PROPERTIES_ID
                : request.GAS_LIFT_WELL_PROPERTIES_ID,
            OPTIMAL_GAS_INJECTION_RATE = analysisOutput.OPTIMAL_GAS_INJECTION_RATE,
            MAXIMUM_PRODUCTION_RATE = analysisOutput.MAXIMUM_PRODUCTION_RATE,
            OPTIMAL_GAS_LIQUID_RATIO = analysisOutput.OPTIMAL_GAS_LIQUID_RATIO,
            PERFORMANCE_POINTS = performancePoints
        };
    }
}
