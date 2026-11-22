using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-03: the rules that hold without a database - a sidetrack names its parent, the kind is one of three, a depth
/// without a unit disables the survey screens, and a duplicate identity is refused.
/// </summary>
public sealed class WellboreTests
{
    private static readonly OgWellbores Service = new(() => null!, new Quiet());

    [Fact]
    public void A_depth_is_shown_with_its_unit_and_without_it_the_survey_screens_are_disabled()
    {
        var withUnit = Bore(totalDepth: 3000m, depthUnit: "m");
        var withoutUnit = Bore(totalDepth: 3000m, depthUnit: null);

        // AC-4
        Assert.True(withUnit.HasDepthUnit);
        Assert.True(withUnit.SurveyEnabled);
        Assert.Equal("3000 m", withUnit.DepthText);

        Assert.False(withoutUnit.HasDepthUnit);
        Assert.False(withoutUnit.SurveyEnabled);
        Assert.Contains("unit unspecified", withoutUnit.DepthText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_sidetrack_must_name_the_bore_it_comes_from()
    {
        var created = await Service.CreateAsync(
            new OgWellboreRequest("UWI-1", "WB-2", Kind: "SIDETRACK"),
            "fahad");

        // AC-5
        Assert.False(created.Success);
        Assert.Equal(OgWellbores.CodeParentBoreRequired, created.Reference);
    }

    [Theory]
    [InlineData("ORIGINAL")]
    [InlineData("SIDETRACK")]
    [InlineData("REDRILL")]
    public void The_three_kinds_are_accepted(string kind)
    {
        var request = new OgWellboreRequest("UWI-1", "WB-1", Kind: kind, ParentId: kind == "ORIGINAL" ? null : "WB-0");

        // Parsing and parent rules pass; the failure is the absent connection, not the kind.
        var result = Service.CreateAsync(request, "fahad").GetAwaiter().GetResult();

        Assert.Equal(OgWellbores.CodeNoConnection, result.Reference);
    }

    [Fact]
    public async Task An_unknown_kind_is_refused()
    {
        var created = await Service.CreateAsync(new OgWellboreRequest("UWI-1", "WB-1", Kind: "SLANT"), "fahad");

        Assert.False(created.Success);
        Assert.Equal(OgWellbores.CodeInvalidKind, created.Reference);
    }

    [Fact]
    public async Task A_change_must_record_who_made_it()
    {
        var created = await Service.CreateAsync(new OgWellboreRequest("UWI-1", "WB-1"), "  ");
        Assert.Equal(OgWellbores.CodeActorRequired, created.Reference);

        var deleted = await Service.DeleteAsync("UWI-1", "WB-1", " ");
        Assert.Equal(OgWellbores.CodeActorRequired, deleted.Reference);
    }

    [Fact]
    public async Task A_wellbore_needs_its_well_and_its_own_identity()
    {
        var created = await Service.CreateAsync(new OgWellboreRequest("", "WB-1"), "fahad");

        Assert.False(created.Success);
        Assert.Equal("wellbore-identity-required", created.Reference);
    }

    private static OgWellboreSummary Bore(decimal? totalDepth, string? depthUnit) =>
        new("UWI-1", "WB-1", "ORIGINAL BORE", null, "ORIGINAL", "ACTIVE", new DateTime(2020, 1, 1), totalDepth, depthUnit);

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
