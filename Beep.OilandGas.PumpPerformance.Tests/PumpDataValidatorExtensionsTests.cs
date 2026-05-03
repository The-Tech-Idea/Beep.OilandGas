using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;
using Xunit;

namespace Beep.OilandGas.PumpPerformance.Tests;

public class PumpDataValidatorExtensionsTests
{
    [Fact]
    public void ValidateNpshMargin_WhenSufficient_DoesNotThrow()
    {
        PumpDataValidator.ValidateNpshMargin(20, 12, 2);
    }

    [Fact]
    public void ValidateNpshMargin_WhenInsufficient_Throws()
    {
        Assert.Throws<InvalidInputException>(() =>
            PumpDataValidator.ValidateNpshMargin(13, 12, 2));
    }

    [Fact]
    public void ValidateStrictlyPositiveFlowRate_WhenZero_Throws()
    {
        Assert.Throws<InvalidInputException>(() =>
            PumpDataValidator.ValidateStrictlyPositiveFlowRate(0, nameof(PumpDataValidatorExtensionsTests)));
    }
}
