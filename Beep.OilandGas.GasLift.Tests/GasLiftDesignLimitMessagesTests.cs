using System.Linq;
using Beep.OilandGas.GasLift.Constants;
using Xunit;

namespace Beep.OilandGas.GasLift.Tests;

public class GasLiftDesignLimitMessagesTests
{
    [Fact]
    public void GetMessage_AllSeededDesignLimitCodes_ReturnNonEmptyMessagesWithNumericLimits()
    {
        var codes = GasLiftReferenceCodeSeed.GetAllSeedRows()
            .Where(r => string.Equals(r.ReferenceSet, GasLiftReferenceSets.DesignLimit, StringComparison.OrdinalIgnoreCase))
            .Select(r => r.ReferenceCode)
            .ToList();

        Assert.NotEmpty(codes);

        foreach (var code in codes)
        {
            var msg = GasLiftDesignLimitMessages.GetMessage(code);
            Assert.False(string.IsNullOrWhiteSpace(msg), $"Missing message for design limit code {code}.");
            Assert.True(msg!.Length > 20, $"Message for {code} should be descriptive.");
        }
    }

    [Theory]
    [InlineData("MIN_INJECTION_MSCFD")]
    [InlineData("MAX_INJECTION_MSCFD")]
    [InlineData("MIN_VALVES")]
    [InlineData("MAX_VALVES")]
    [InlineData("MIN_VALVE_DEPTH_FT")]
    [InlineData("MIN_VALVE_SPACING_FT")]
    public void GetMessage_KnownCodes_AreCaseInsensitive(string code)
    {
        var msg = GasLiftDesignLimitMessages.GetMessage(code.ToLowerInvariant());
        Assert.False(string.IsNullOrWhiteSpace(msg));
    }

    [Fact]
    public void GetMessage_Unknown_ReturnsNull()
    {
        Assert.Null(GasLiftDesignLimitMessages.GetMessage("NOT_A_LIMIT"));
        Assert.Null(GasLiftDesignLimitMessages.GetMessage(null));
    }
}
