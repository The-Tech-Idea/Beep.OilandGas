using System.Linq;
using Beep.OilandGas.GasLift.Calculations;
using Beep.OilandGas.GasLift.Mapping;
using Beep.OilandGas.Models.Data.GasLift;
using Xunit;

namespace Beep.OilandGas.GasLift.Tests;

public class GasLiftPotentialWireMapperTests
{
    [Fact]
    public void ToPotentialResult_MapsOptimalAndPerformancePoints()
    {
        var request = new GAS_LIFT_WELL_PROPERTIES
        {
            GAS_LIFT_WELL_PROPERTIES_ID = "REQ-1",
            WELL_DEPTH = 10_000m,
            WELLHEAD_PRESSURE = 200m,
            BOTTOM_HOLE_PRESSURE = 2500m,
            WELLHEAD_TEMPERATURE = 560m,
            BOTTOM_HOLE_TEMPERATURE = 660m,
            OIL_GRAVITY = 35m,
            WATER_CUT = 0.1m,
            GAS_SPECIFIC_GRAVITY = 0.65m,
            GAS_OIL_RATIO = 500m,
            DESIRED_PRODUCTION_RATE = 400m
        };

        var raw = GasLiftPotentialCalculator.AnalyzeGasLiftPotential(request, 100m, 500m, 10, default);
        var dto = GasLiftPotentialWireMapper.ToPotentialResult(request, raw);

        Assert.False(string.IsNullOrEmpty(dto.GAS_LIFT_POTENTIAL_RESULT_ID));
        Assert.Equal("REQ-1", dto.GAS_LIFT_WELL_PROPERTIES_ID);
        Assert.NotNull(dto.OPTIMAL_GAS_INJECTION_RATE);
        Assert.NotNull(dto.PERFORMANCE_POINTS);
        Assert.Equal(11, dto.PERFORMANCE_POINTS!.Count());
        Assert.All(dto.PERFORMANCE_POINTS!, p => Assert.False(string.IsNullOrEmpty(p.GAS_LIFT_PERFORMANCE_POINT_ID)));
    }
}
