using Beep.OilandGas.ChokeAnalysis.Constants;
using Xunit;

namespace Beep.OilandGas.ChokeAnalysis.Tests;

public class ChokeAnalysisOrchestrationRulesTests
{
    [Theory]
    [InlineData(null, false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("GAS_SINGLE_PHASE", false)]
    [InlineData(" GAS_SINGLE_PHASE ", false)]
    [InlineData("GILBERT", true)]
    [InlineData("MULTIPHASE", true)]
    [InlineData("ROS", true)]
    [InlineData("ACHONG", true)]
    [InlineData("PILEHVARI", true)]
    [InlineData("SACHDEVA", true)]
    [InlineData("BAXENDELL", true)]
    [InlineData("CUSTOM_UNKNOWN", false)]
    public void UseMultiphaseOrchestration_MatchesExpected(string? correlation, bool expected)
    {
        Assert.Equal(expected, ChokeAnalysisReferenceCodes.UseMultiphaseOrchestration(correlation));
    }
}
