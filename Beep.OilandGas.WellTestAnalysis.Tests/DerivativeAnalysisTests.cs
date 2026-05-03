using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Calculations;
using Xunit;

namespace Beep.OilandGas.WellTestAnalysis.Tests;

public class DerivativeAnalysisTests
{
    [Fact]
    public void CalculateDerivative_returns_empty_when_fewer_than_three_points()
    {
        var data = new List<PRESSURE_TIME_POINT>
        {
            new() { TIME = 1, PRESSURE = 100 },
            new() { TIME = 2, PRESSURE = 99 }
        };

        var result = DerivativeAnalysis.CalculateDerivative(data, smoothingFactor: 0);

        Assert.Empty(result);
    }

    [Fact]
    public void CalculateDerivative_returns_one_row_per_input_point()
    {
        var data = new List<PRESSURE_TIME_POINT>
        {
            new() { TIME = 1, PRESSURE = 3000 },
            new() { TIME = 2, PRESSURE = 2990 },
            new() { TIME = 4, PRESSURE = 2980 },
            new() { TIME = 8, PRESSURE = 2970 },
            new() { TIME = 16, PRESSURE = 2960 }
        };

        var result = DerivativeAnalysis.CalculateDerivative(data, smoothingFactor: 0);

        Assert.Equal(data.Count, result.Count);
        Assert.All(result, p => Assert.NotNull(p.PRESSURE_DERIVATIVE));
    }
}
