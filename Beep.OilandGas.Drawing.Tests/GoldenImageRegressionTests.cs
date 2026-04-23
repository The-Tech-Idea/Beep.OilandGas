using Beep.OilandGas.Drawing.Tests.GoldenImages;
using Xunit;

namespace Beep.OilandGas.Drawing.Tests;

public sealed class GoldenImageRegressionTests
{
    public static IEnumerable<object[]> Snapshots => DrawingRegressionSceneFactory
        .CreateSnapshots()
        .Select(snapshot => new object[] { snapshot });

    [Theory]
    [MemberData(nameof(Snapshots))]
    public void Regression_scene_matches_golden_image(GoldenImageSnapshot snapshot)
    {
        GoldenImageAssert.AssertMatches(snapshot);
    }
}