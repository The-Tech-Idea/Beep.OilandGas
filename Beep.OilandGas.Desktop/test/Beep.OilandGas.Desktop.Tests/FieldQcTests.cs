using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-06: the rules the page states - a finding carries its entity, field and rule (AC-5); coverage read from no well is unknown rather
/// than zero (AC-2); a cancelled run says what it covered (AC-3); a duplicate stays a proposal (AC-4); and the export is the grid's
/// filter (AC-6). The run itself is exercised in the opt-in test, because it reads a configured source.
/// </summary>
public sealed class FieldQcTests
{
    private const string PageFileName = "FieldQcPageView.cs";

    private static OgQcFinding Finding(string well, string field, string rule, string severity, string message) =>
        new("well", well, "well", field, rule, severity, message, 1);

    [Fact]
    public void A_finding_carries_its_entity_its_field_and_its_rule()
    {
        // AC-5: the three things a person needs to act on it, and the engine's own code as the rule.
        var finding = Finding("W-2", "position", "QC003", OgFieldQcRules.Warning,
            "no wellhead latitude / longitude - the well cannot be placed with others (maps, anti-collision)");

        Assert.Equal("well", finding.Entity);
        Assert.Equal("position", finding.Field);
        Assert.Equal("QC003", finding.Rule);
        Assert.False(finding.IsError);

        Assert.Contains("QC003", finding.Describe(), StringComparison.Ordinal);
        Assert.Contains("well.position", finding.Describe(), StringComparison.Ordinal);
        Assert.Contains("W-2", finding.Describe(), StringComparison.Ordinal);

        // A field-scope finding says so rather than inventing a well it does not have.
        var load = new OgQcFinding("field", "Alpha 1; Alpha 2", "well", "(not attributed)", "LOAD", OgFieldQcRules.Warning,
            "warning WI001: identifier", 2);

        Assert.Contains("field ·", load.Describe(), StringComparison.Ordinal);
        Assert.Contains("(not attributed)", load.Describe(), StringComparison.Ordinal);

        // The engine's completeness codes are the ones the page attributes to a field, and each names one item of the model.
        Assert.Equal("name", OgFieldQcRules.CodeFields["QC001"]);
        Assert.Equal("position", OgFieldQcRules.CodeFields["QC003"]);
        Assert.Equal("tubulars", OgFieldQcRules.CodeFields["QC006"]);
        Assert.False(OgFieldQcRules.CodeFields.ContainsKey("LOAD"));
    }

    [Fact]
    public void Coverage_read_from_no_well_is_unknown_and_not_zero()
    {
        // AC-2: the sentence must not be readable as "nothing is populated".
        var workspace = new OgFieldQcWorkspace(
            "Wells", true, "csv", false, 0, 0, [], [], CoverageKnown: false, [], [], [], false, [], [], [], DateTime.UtcNow);

        Assert.False(workspace.CoverageKnown);
        Assert.Contains("Coverage unknown", workspace.CoverageText, StringComparison.Ordinal);
        Assert.Contains("not the same as nothing being populated", workspace.CoverageText, StringComparison.Ordinal);
        Assert.DoesNotContain("0 of 0", workspace.CoverageText, StringComparison.Ordinal);

        // A known coverage counts against the wells actually read.
        var known = workspace with
        {
            CoveredWells = 3,
            CoverageKnown = true,
            Coverage = [new OgQcCompleteness("position", 2, 3), new OgQcCompleteness("survey", 3, 3)],
        };

        Assert.Contains("position · 2 of 3 well(s) (67 %)", known.CoverageText, StringComparison.Ordinal);
        Assert.Contains("survey · 3 of 3 well(s) (100 %)", known.CoverageText, StringComparison.Ordinal);
        Assert.Equal(2d / 3d, known.Coverage[0].Fraction!.Value, 6);
    }

    [Fact]
    public void A_cancelled_run_states_the_scope_it_covered()
    {
        // AC-3: the sentence names how many wells were read of how many, and says the findings are partial.
        var cancelled = new OgFieldQcWorkspace(
            "Wells", true, "csv", Cancelled: true, CoveredWells: 1, TotalWells: 3,
            [Finding("W-1", "survey", "QC002", OgFieldQcRules.Warning, "no survey")],
            [new OgQcCompleteness("survey", 0, 1)], true, [], [], [], false, [], [], [], DateTime.UtcNow);

        Assert.True(cancelled.Cancelled);
        Assert.Contains("cancelled after 1 of 3 well(s)", cancelled.SummaryText, StringComparison.Ordinal);
        Assert.Contains("cover only the wells read", cancelled.SummaryText, StringComparison.Ordinal);
        Assert.Contains("(cancelled)", cancelled.CountsText, StringComparison.Ordinal);

        // A completed run of the same size makes no such claim.
        Assert.DoesNotContain("cancelled", (cancelled with { Cancelled = false }).SummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_duplicate_is_a_proposal_until_a_person_decides()
    {
        // AC-4: the wording says proposed only, and a decision is recorded rather than assumed.
        var proposal = new OgQcDuplicate(
            "Wells|W-1", "Wells|W-3", "Alpha 1", "Alpha 1", "30/1-A", "30/1-A", 0.95,
            ["the same name", "the same UWI"], [], string.Empty);

        Assert.False(proposal.Decided);
        Assert.Contains("proposed only", proposal.Describe(), StringComparison.Ordinal);
        Assert.Contains("the same UWI", proposal.Describe(), StringComparison.Ordinal);

        Assert.True((proposal with { Decision = "confirmed" }).Decided);
        Assert.Contains("confirmed", (proposal with { Decision = "confirmed" }).Describe(), StringComparison.Ordinal);
        Assert.Contains("rejected", (proposal with { Decision = "rejected" }).Describe(), StringComparison.Ordinal);

        // A conflict is shown beside the evidence, not hidden behind the score.
        var conflicted = proposal with { Conflicts = ["the operators differ"] };

        Assert.Contains("against: the operators differ", conflicted.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_export_is_the_grids_filter_and_nothing_else()
    {
        // AC-6: what the grid shows is what the CSV holds, header included.
        var findings = new List<OgQcFinding>
        {
            Finding("W-1", "survey", "QC002", OgFieldQcRules.Warning, "no survey (2+ stations)"),
            Finding("W-2", "position", "QC003", OgFieldQcRules.Warning, "no wellhead latitude / longitude"),
            Finding("W-3", "name", "QC001", OgFieldQcRules.Error, "well has no name"),
        };

        var all = OgFieldQcRules.Csv(findings);
        var lines = all.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("scope,well,entity,field,rule,severity,occurrences,message", lines[0]);
        Assert.Equal(1 + findings.Count, lines.Length);
        // The export keeps the order the grid holds - the service sorts, the export does not re-sort.
        Assert.Contains("\"QC002\"", lines[1], StringComparison.Ordinal);
        Assert.Contains("\"QC001\"", lines[3], StringComparison.Ordinal);
        Assert.Contains("\"QC001\"", all, StringComparison.Ordinal);

        // A severity filter exports only those rows, so an export cannot claim more than the grid shows.
        var errors = OgFieldQcRules.Filter(findings, OgFieldQcRules.Error, null);

        Assert.Single(errors);

        Assert.Equal(2,
            OgFieldQcRules.Csv(errors).Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length);

        // A needle narrows it further, and the two filters combine.
        Assert.Single(OgFieldQcRules.Filter(findings, OgFieldQcRules.Warning, "position"));
        // Every message begins with "no ", so that needle matches all three - which is the point of testing it.
        Assert.Equal(3, OgFieldQcRules.Filter(findings, null, "no ").Count);
        Assert.Single(OgFieldQcRules.Filter(findings, null, "survey"));
        Assert.Empty(OgFieldQcRules.Filter(findings, null, "nothing matches this"));

        // A message with a comma is quoted rather than breaking the row.
        var quoted = OgFieldQcRules.Csv([Finding("W-9", "depth", "QC004", OgFieldQcRules.Warning, "no elevation, so no datum")]);

        Assert.Contains("\"no elevation, so no datum\"", quoted, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_run_names_its_source_and_a_source_that_is_not_configured_is_disabled()
    {
        var offline = new OgFieldQcService(new Quiet(), new NoSetup(),
            () => Path.Combine(Path.GetTempPath(), "no-environment.json"));

        var unnamed = await offline.RunAsync(new OgFieldQcRequest(" ", 100, false));

        Assert.False(unnamed.Success);
        Assert.Equal(OgFieldQcRules.CodeSourceRequired, unnamed.Reference);

        // A source the environment does not define is not run: the workspace says so, and the run is disabled with its reason.
        var unknown = await offline.RunAsync(new OgFieldQcRequest("Not-Configured-Anywhere", 100, false));

        Assert.True(unknown.Success, unknown.Message);
        Assert.False(unknown.Value!.SourceConfigured);
        Assert.False(unknown.Value.CoverageKnown);
        Assert.Contains("not one of the environment's data sources", unknown.Value.SummaryText, StringComparison.Ordinal);

        // A decision needs an actor and two records.
        Assert.Equal(OgFieldQcRules.CodeActorRequired,
            (await offline.DecideDuplicateAsync(new OgFieldQcRequest("X", 10, true), "a", "b", true, " ")).Reference);

        Assert.Equal(OgFieldQcRules.CodeDuplicateUnknown,
            (await offline.DecideDuplicateAsync(new OgFieldQcRequest("X", 10, true), " ", "b", true, "sara")).Reference);
    }

    [Fact]
    public void The_page_shows_the_thresholds_and_exports_the_filtered_rows()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // The feature's open question: why a finding or a proposal exists is on the page, not left to the reader.
        Assert.Contains("workspace.ThresholdsText", page, StringComparison.Ordinal);

        // AC-6: the export is rebuilt from the grid's filter whenever the filter changes.
        Assert.Contains("OgFieldQcRules.Csv(filtered)", page, StringComparison.Ordinal);
        Assert.Contains("OgFieldQcRules.Filter(", page, StringComparison.Ordinal);
        Assert.Contains("_severity.SelectionChanged += (_, _) => RefreshExport();", page, StringComparison.Ordinal);
        Assert.Contains("_needle.TextChanged += (_, _) => RefreshExport();", page, StringComparison.Ordinal);

        // AC-3: the run is off the UI thread, so Cancel can be pressed while it is going.
        Assert.Contains("Task.Run(() => _qc.RunAsync(request, _running.Token), CancellationToken.None)", page,
            StringComparison.Ordinal);

        // AC-1 and AC-4: each decision is its own command, and each needs a selected row.
        Assert.Contains("Select the proposal to accept", page, StringComparison.Ordinal);
        Assert.Contains("never merged without a decision", page, StringComparison.Ordinal);
    }

    private sealed class NoSetup : IOgWellboreSetup
    {
        public Task<OgResult<IReadOnlyList<OgCountryOption>>> CountriesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<IReadOnlyList<OgCountryOption>>.Ok([]));

        public Task<OgResult<OgSetupWorkspace>> LoadAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> CreateAsync(string countryCode, string? companyName, string actor,
            CancellationToken cancellationToken = default) => Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> SetAsync(string path, string? value, string actor,
            CancellationToken cancellationToken = default) => Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> AddSourceAsync(string name, string definitionJson, string actor,
            CancellationToken cancellationToken = default) => Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> RemoveSourceAsync(string name, string actor,
            CancellationToken cancellationToken = default) => Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> TestSourceAsync(string name, string? wellId,
            CancellationToken cancellationToken = default) => Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> CoverageAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> SuggestAsync(string name, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> AcceptAsync(string name, string area, string where, string value,
            string target, string actor, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> VerifyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> HandoverAsync(string archiveFolder, string actor,
            CancellationToken cancellationToken = default) => Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));
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
