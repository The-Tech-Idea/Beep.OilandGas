using Beep.OilandGas.Models.Data.Accounting;
using Xunit;

namespace Beep.OilandGas.Repository.Tests;

public sealed class ExactDecimalArithmeticTests
{
    [Fact]
    public void SumDoesNotRoundAwayAnUnrepresentableQuantity()
    {
        Assert.Null(ExactDecimalArithmetic.SumOrUnavailable([decimal.MaxValue, 0.1m]));
        Assert.Null(ExactDecimalArithmetic.SumOrUnavailable([decimal.MaxValue, 1m]));
        Assert.Equal(decimal.MaxValue, ExactDecimalArithmetic.SumOrUnavailable([decimal.MaxValue - 1m, 1m]));
    }

    [Fact]
    public void SumRetainsExactSignsAndFractionalScale()
    {
        var result = ExactDecimalArithmetic.SumOrUnavailable([-1.20m, 2.30m]);
        Assert.Equal(1.10m, result);
        Assert.Equal(2, (decimal.GetBits(result!.Value)[3] >> 16) & 255);
        Assert.Equal(0m, ExactDecimalArithmetic.SumOrUnavailable([]));
        Assert.Equal(-decimal.MaxValue, ExactDecimalArithmetic.SumOrUnavailable([-decimal.MaxValue]));
    }

    [Fact]
    public void MultiplicationRefusesPrecisionLossAndMagnitudeOverflow()
    {
        Assert.Null(ExactDecimalArithmetic.MultiplyOrUnavailable(0.0000000000000000000000000001m, 0.1m));
        Assert.Null(ExactDecimalArithmetic.MultiplyOrUnavailable(decimal.MaxValue, 2m));
        Assert.Equal(decimal.MaxValue, ExactDecimalArithmetic.MultiplyOrUnavailable(decimal.MaxValue, 1m));
    }

    [Fact]
    public void ExactMultiplicationPreservesNativePrecisionWithoutRounding()
    {
        var result = ExactDecimalArithmetic.MultiplyOrUnavailable(20.10m, 3.00m);
        Assert.Equal(60.3000m, result);
        Assert.Equal(4, (decimal.GetBits(result!.Value)[3] >> 16) & 255);
        Assert.Equal(-2.5m, ExactDecimalArithmetic.MultiplyOrUnavailable(-1.25m, 2m));
        Assert.Equal(0m, ExactDecimalArithmetic.MultiplyOrUnavailable(0.0000000000000000000000000001m, 0m));
    }
}
