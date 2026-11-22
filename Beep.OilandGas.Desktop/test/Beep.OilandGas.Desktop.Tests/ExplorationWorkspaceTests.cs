using System.Text.RegularExpressions;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// EXP-01: the rules that hold without a database - a failed read is never an empty page of zeros, every source
/// link is decided by the page's own permission, only prospects actually missing risk elements are counted, and
/// the exploration persona's landing route is the one the RBAC seed names.
/// </summary>
public sealed class ExplorationWorkspaceTests
{
    [Fact]
    public async Task A_failed_read_is_a_failure_and_never_a_zero_filled_workspace()
    {
        var workspace = new OgExplorationWorkspace(() => null!, new Quiet());

        var loaded = await workspace.LoadAsync("FIELD-1", "North", "2026", "Exploration geologist", _ => true);

        // AC-1
        Assert.False(loaded.Success);
        Assert.Equal(OgExplorationWorkspace.CodeNoConnection, loaded.Reference);
        Assert.Null(loaded.Value);
    }

    [Fact]
    public async Task A_read_that_fails_after_the_connection_is_a_failure_with_a_reference()
    {
        var reporter = new Recording();
        var workspace = new OgExplorationWorkspace(() => throw new InvalidOperationException("the provider dropped"), reporter);

        var loaded = await workspace.LoadAsync("FIELD-1", "North", "2026", "Exploration geologist", _ => true);

        Assert.False(loaded.Success);
        Assert.Null(loaded.Value);
        Assert.NotNull(loaded.Reference);
        Assert.Single(reporter.Failures);
        Assert.Contains("exploration workspace", reporter.Failures[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Every_source_link_is_decided_by_the_pages_own_permission()
    {
        var allowed = new HashSet<string>(["wells"], StringComparer.OrdinalIgnoreCase);

        var links = OgExplorationWorkspace.BuildLinks(route => allowed.Contains(route));

        // AC-2: availability is exactly the permission answer, for every link.
        Assert.All(links, link => Assert.Equal(allowed.Contains(link.Route), link.Available));

        var wells = Assert.Single(links, link => link.Route == "wells");
        Assert.True(wells.Available);
        Assert.Null(wells.WhyNot);

        var board = Assert.Single(links, link => link.Route == "prospects");
        Assert.False(board.Available);
        Assert.Contains("EXP-02", board.WhyNot!, StringComparison.Ordinal);
    }

    [Fact]
    public void A_link_with_no_page_in_this_build_says_which_feature_brings_it()
    {
        var links = OgExplorationWorkspace.BuildLinks(_ => false);

        Assert.All(links, link => Assert.False(link.Available));
        Assert.Contains(links, link => link.WhyNot!.Contains("EXP-04", StringComparison.Ordinal));
        Assert.Contains(links, link => link.WhyNot!.Contains("LSE-01", StringComparison.Ordinal));
    }

    [Fact]
    public void Only_a_prospect_missing_a_risk_element_is_incomplete()
    {
        var complete = OgExplorationWorkspace.RiskElements
            .ToDictionary(element => element, element => (decimal?)0.25m);

        // AC-3: a complete assessment is never counted.
        Assert.False(OgExplorationWorkspace.IsIncomplete(complete));

        // No assessment at all is incomplete.
        Assert.True(OgExplorationWorkspace.IsIncomplete(null));

        // One element missing is incomplete.
        foreach (var element in OgExplorationWorkspace.RiskElements)
        {
            var partial = new Dictionary<string, decimal?>(complete, StringComparer.OrdinalIgnoreCase)
            {
                [element] = null,
            };

            Assert.True(
                OgExplorationWorkspace.IsIncomplete(partial),
                $"{element} missing should make the prospect incomplete.");
        }

        // A value that is present is complete even when the others are zero rather than null.
        var zeroed = OgExplorationWorkspace.RiskElements
            .ToDictionary(element => element, element => (decimal?)0m);
        Assert.False(OgExplorationWorkspace.IsIncomplete(zeroed));

        Assert.Equal(5, OgExplorationWorkspace.RiskElements.Count);
    }

    [Fact]
    public void The_exploration_persona_lands_on_the_exploration_workspace()
    {
        var landing = OgPersonaLanding.Resolve([OgRoleNames.ExplorationGeologist], _ => true);

        Assert.Equal("exploration", landing.Route);
        Assert.Equal("Exploration geologist", landing.Persona);
        Assert.False(landing.IsFallback);
    }

    [Theory]
    [InlineData("/lifecycle/exploration/dashboard", "exploration")]
    [InlineData("/lifecycle/development/dashboard", "development")]
    [InlineData("/reservoir/dashboard", "reservoir")]
    [InlineData("/hse/dashboard", "hse")]
    [InlineData("/admin/dashboard", "admin")]
    [InlineData("/exploration", "exploration")]
    [InlineData("", OgPersonaLanding.DefaultRoute)]
    [InlineData(null, OgPersonaLanding.DefaultRoute)]
    public void An_rbac_landing_route_maps_to_the_module_segment(string? rbacRoute, string expected)
    {
        Assert.Equal(expected, OgPersonaLanding.RouteFor(rbacRoute));
    }

    /// <summary>
    /// AC-4: the desktop's exploration route is read from the RBAC seed's <c>DEFAULT_LANDING_ROUTE</c> for
    /// <c>EXPLORATION_GEOLOGIST</c>. If either side moves, this fails.
    /// </summary>
    [Fact]
    public void The_persona_route_matches_the_rbac_default_landing_route()
    {
        var seed = Path.Combine(
            RepositoryRoot(),
            "Beep.OilandGas.UserManagement",
            "Services",
            "DefaultSecuritySeedService.cs");

        Assert.True(File.Exists(seed), seed);
        var text = File.ReadAllText(seed);

        var match = Regex.Match(
            text,
            "Code\\s*=\\s*\"EXPLORATION_GEOLOGIST\".*?Route\\s*=\\s*\"([^\"]+)\"");

        Assert.True(match.Success, "The exploration persona's RBAC route was not found in the seed.");

        var rbacRoute = match.Groups[1].Value;
        var persona = Assert.Single(OgPersonaLanding.All, row =>
            string.Equals(row.Role, OgRoleNames.ExplorationGeologist, StringComparison.OrdinalIgnoreCase));

        Assert.Equal(OgPersonaLanding.RouteFor(rbacRoute), persona.Route);
        Assert.Contains(persona.Route, OgModuleCatalog.Pages.Select(page => page.Route));
    }

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

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
