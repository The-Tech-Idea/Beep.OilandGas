using Beep.OilandGas.Models.Data.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis;
using Xunit;

namespace Beep.OilandGas.WellTestAnalysis.Tests;

public class DerivativeIdentifyModelTests
{
    [Fact]
    public void IdentifyReservoirModel_flat_middle_and_rising_late_returns_closed_boundary()
    {
        var points = new List<PRESSURE_TIME_POINT>();
        for (int i = 0; i < 20; i++)
        {
            double d;
            if (i < 6)
                d = 70 + i * 2.0;
            else if (i < 14)
                d = 100;
            else
                d = 100 + (i - 13) * 6.0;

            points.Add(new PRESSURE_TIME_POINT { TIME = i + 1.0, PRESSURE = 3000, PRESSURE_DERIVATIVE = d });
        }

        var model = WellTestAnalyzer.IdentifyReservoirModel(points);
        Assert.Equal(ReservoirModel.ClosedBoundary, model);
    }

    [Fact]
    public void IdentifyReservoirModel_fewer_than_five_points_returns_infinite_acting()
    {
        var points = new List<PRESSURE_TIME_POINT>
        {
            new() { TIME = 1, PRESSURE = 100, PRESSURE_DERIVATIVE = 1 },
            new() { TIME = 2, PRESSURE = 99, PRESSURE_DERIVATIVE = 1 }
        };

        Assert.Equal(ReservoirModel.InfiniteActing, WellTestAnalyzer.IdentifyReservoirModel(points));
    }
}
