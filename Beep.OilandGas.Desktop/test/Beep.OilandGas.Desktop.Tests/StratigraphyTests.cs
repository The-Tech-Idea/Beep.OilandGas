using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-05: the rules that hold without a database - three-level hierarchy with a stable order, overlap flagging,
/// theme colour tokens (no colour values) and the input checks.
/// </summary>
public sealed class StratigraphyTests
{
    [Fact]
    public void The_hierarchy_renders_three_levels_in_a_stable_order()
    {
        var units = new[]
        {
            Unit("M2", "Member 2", "F1", "MEMBER", 30),
            Unit("G1", "Group 1", null, "GROUP", 10),
            Unit("F1", "Formation 1", "G1", "FORMATION", 20),
            Unit("M1", "Member 1", "F1", "MEMBER", 10),
            Unit("F2", "Formation 2", "G1", "FORMATION", 40),
        };

        var hierarchy = OgStratigraphy.BuildHierarchy(units);

        var root = Assert.Single(hierarchy);
        Assert.Equal("G1", root.Unit.Id);
        Assert.Equal(1, root.Level);

        Assert.Equal(["F1", "F2"], root.Children.Select(node => node.Unit.Id));
        Assert.All(root.Children, node => Assert.Equal(2, node.Level));

        var formation = Assert.Single(root.Children, node => node.Unit.Id == "F1");
        Assert.Equal(["M1", "M2"], formation.Children.Select(node => node.Unit.Id));
        Assert.All(formation.Children, node => Assert.Equal(3, node.Level));

        // Stable: a second build gives the same shape.
        var again = OgStratigraphy.BuildHierarchy(units);
        Assert.Equal(
            root.Children.Select(node => node.Unit.Id),
            again.Single().Children.Select(node => node.Unit.Id));
    }

    [Fact]
    public void A_cyclic_parent_does_not_recurse_forever()
    {
        var units = new[]
        {
            Unit("A", "A", "B", "GROUP", 1),
            Unit("B", "B", "A", "GROUP", 2),
        };

        var hierarchy = OgStratigraphy.BuildHierarchy(units);

        Assert.Empty(hierarchy);
    }

    [Fact]
    public void Two_picks_at_the_same_depth_are_flagged_and_thickness_is_computed_between_picks()
    {
        var picks = new[]
        {
            Pick("F1", 1000m),
            Pick("F2", 1000m),
            Pick("M1", 1200m),
        };

        var flagged = OgStratigraphy.FlagOverlaps(picks);

        // AC-3: both sides of the pair at 1000 are flagged, nothing is reordered away.
        Assert.Equal(3, flagged.Count);
        Assert.Equal(2, flagged.Count(pick => pick.Overlapping));
        Assert.All(flagged.Where(pick => pick.Overlapping),
            pick => Assert.Contains("Overlaps", pick.Flag!, StringComparison.Ordinal));

        var first = flagged.Single(pick => pick.UnitId == "F1");
        Assert.Equal(1000m, first.BaseMd);
        Assert.Null(first.Thickness);

        var last = flagged.Single(pick => pick.UnitId == "M1");
        Assert.Null(last.Thickness);
        Assert.False(last.Overlapping);
    }

    [Fact]
    public void A_pick_without_a_top_is_excluded_from_thickness()
    {
        var flagged = OgStratigraphy.FlagOverlaps([Pick("F1", null)]);

        var pick = Assert.Single(flagged);
        Assert.Null(pick.TopMd);
        Assert.Null(pick.Thickness);
        Assert.Contains("No top depth", pick.Flag!, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("GROUP", "Highlight")]
    [InlineData("FORMATION", "Info")]
    [InlineData("MEMBER", "Control")]
    [InlineData("BED", "Control")]
    [InlineData("", "Window")]
    [InlineData("SOMETHING ELSE", "Light")]
    public void Colour_is_a_theme_token_by_rank(string rank, string token)
    {
        // AC-5
        Assert.Equal(token, OgStratigraphy.ColourFor(rank));
        Assert.Contains(token, OgStratigraphy.ColourTokens);
    }

    [Fact]
    public void No_stratigraphy_source_contains_a_colour_value()
    {
        var root = RepositoryRoot();
        var files = new[]
        {
            Path.Combine(root, "Beep.OilandGas.Desktop", "src", "Beep.OilandGas.Desktop", "Views", "StratigraphyPageView.cs"),
            Path.Combine(root, "Beep.OilandGas.Desktop", "src", "Beep.OilandGas.Desktop.Data", "Wells", "OgStratigraphy.cs"),
        };

        foreach (var file in files)
        {
            Assert.True(File.Exists(file), file);
            var text = File.ReadAllText(file);

            Assert.DoesNotMatch(@"#[0-9A-Fa-f]{6}\b", text);
            Assert.DoesNotMatch(@"rgb\(", text);
        }
    }

    [Fact]
    public async Task A_pick_needs_a_unit_a_depth_and_an_actor()
    {
        var service = new OgStratigraphy(() => null!, new Quiet());

        Assert.Equal(
            OgStratigraphy.CodeActorRequired,
            (await service.AddPickAsync("UWI-1", "N1", "F1", 100m, "m", " ")).Reference);

        Assert.Equal(
            OgStratigraphy.CodeUnitRequired,
            (await service.AddPickAsync("UWI-1", "N1", " ", 100m, "m", "fahad")).Reference);

        Assert.Equal(
            OgStratigraphy.CodeDepthRequired,
            (await service.AddPickAsync("UWI-1", "N1", "F1", 0m, "m", "fahad")).Reference);

        Assert.Equal(
            OgStratigraphy.CodeNoConnection,
            (await service.AddPickAsync("UWI-1", "N1", "F1", 100m, "m", "fahad")).Reference);
    }

    private static OgStratUnit Unit(string id, string name, string? parent, string rank, decimal age) =>
        new(id, "N1", name, id, parent, rank, age, OgStratigraphy.ColourFor(rank));

    private static OgWellPick Pick(string unit, decimal? top) =>
        new("UWI-1", unit, "N1", unit, top, null, "m", null, null, false, null);

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? AppContext.BaseDirectory;
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
