using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.HSE.Models;
using Xunit;

namespace CompanyEnvironment.Tests;

public sealed class GhgArithmeticTests
{
    private static readonly GhgFactorDefinition Factor = new("test", "flare", "volume", "mass", 1.25m, new(2026, 1, 1), new(2026, 12, 31), "source");
    private static readonly GhgDispositionFact Row = new("measured", "flare", "gas", "month", 1, 0, 12m, "volume", new(2026, 2, 1), new(2026, 2, 28));

    [Fact]
    public void CheckUsesTheExplicitRecordedVolumeAndFactorWithoutUnitConversion()
    {
        var result = GhgArithmetic.Check(Row, Factor);
        Assert.Equal(15m, result.Result); Assert.Equal("mass", result.OutputUnit); Assert.Equal(Row, result.Disposition); Assert.Equal(Factor, result.Factor);
        Assert.Equal(0m, GhgArithmetic.Check(Row with { Volume = 0m }, Factor).Result);
    }

    [Fact]
    public void MissingOrIncompatibleEvidenceNeverProducesAResult()
    {
        Assert.Throws<InvalidOperationException>(() => GhgArithmetic.Check(Row with { Unit = "other" }, Factor));
        Assert.Throws<InvalidOperationException>(() => GhgArithmetic.Check(Row with { ActivityType = "other" }, Factor));
        Assert.Throws<InvalidOperationException>(() => GhgArithmetic.Check(Row with { StartDate = null }, Factor));
        Assert.Throws<InvalidOperationException>(() => GhgArithmetic.Check(Row with { EndDate = new(2027, 1, 1) }, Factor));
        Assert.Throws<InvalidOperationException>(() => GhgArithmetic.Check(Row with { Volume = -1m }, Factor));
        Assert.Throws<InvalidOperationException>(() => GhgArithmetic.Check(Row with { Volume = decimal.MaxValue }, Factor));
    }
}
