using Beep.OilandGas.FlashCalculations.Constants;
using Xunit;

namespace Beep.OilandGas.FlashCalculations.Tests;

public class FlashEquationOfStateMappingTests
{
    [Fact]
    public void ToReferenceCode_NullOrWhitespace_DefaultsToIdealK()
    {
        Assert.Equal("IDEAL_K", FlashEquationOfStateMapping.ToReferenceCode(null));
        Assert.Equal("IDEAL_K", FlashEquationOfStateMapping.ToReferenceCode(""));
        Assert.Equal("IDEAL_K", FlashEquationOfStateMapping.ToReferenceCode("   "));
    }

    [Theory]
    [InlineData("PR", "PR")]
    [InlineData("pr", "PR")]
    [InlineData("SRK", "SRK")]
    [InlineData("SRK_MODIFIED", "SRK_MODIFIED")]
    [InlineData("IDEAL_K", "IDEAL_K")]
    [InlineData("Peng-Robinson", "PR")]
    [InlineData("Soave-Redlich-Kwong", "SRK")]
    [InlineData("Wilson", "IDEAL_K")]
    [InlineData("unknown-model-xyz", "IDEAL_K")]
    public void ToReferenceCode_NormalizesInput(string? input, string expected) =>
        Assert.Equal(expected, FlashEquationOfStateMapping.ToReferenceCode(input));
}
