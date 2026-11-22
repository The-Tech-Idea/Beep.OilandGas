using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-06 against a real CSV source: the engine's codes come back attributed to the well and the item they are about (AC-5); coverage
/// is counted against the wells read and is unknown when none was (AC-2); a run cancelled after one well reports that scope and only
/// those findings (AC-3); a proposal is applied only by an acceptance (AC-1); a duplicate stays a proposal until it is decided
/// (AC-4); and the export is the grid's filter (AC-6).
/// </summary>
/// <remarks>
/// The field QC run reads the environment's configured data sources, so this test registers a temporary source and removes it again.
/// It is opt-in for that reason: it mutates process-wide configuration while it runs.
/// </remarks>
public sealed class FieldQcGatewayTests
{
    private const string SourceName = "QC-FIXTURE";

    [IntegrationFact]
    public async Task A_field_is_checked_and_its_scope_duplicates_and_filters_are_reported()
    {        using var scope = new Scratch();

        var folder = scope.Sub("wells");

        WriteFixture(folder);

        var environment = WellboreEnvironment.Current;
        var reporter = new Recording();
        var environmentFile = Path.Combine(scope.Sub("env"), "environment.json");

        environment.DataSources[SourceName] = new DataSourceDefinition { Kind = "csv", Path = folder };

        try
        {
            var service = new OgFieldQcService(reporter, new NoSetup(), () => environmentFile);

            // ---- AC-5, AC-2: the run, the findings and the coverage ----
            var ran = await service.RunAsync(new OgFieldQcRequest(SourceName, 50, WithDuplicates: true));

            Assert.True(ran.Success, ran.Message + " :: " + string.Join(" | ", reporter.Failures));

            var workspace = ran.Value!;

            Assert.True(workspace.SourceConfigured);
            Assert.Equal(3, workspace.CoveredWells);
            Assert.False(workspace.Cancelled);
            Assert.True(workspace.HasFindings, workspace.SummaryText);

            // Every finding names the entity, the field and the engine's rule.
            Assert.All(workspace.Findings, finding =>
            {
                Assert.False(string.IsNullOrWhiteSpace(finding.Entity));
                Assert.False(string.IsNullOrWhiteSpace(finding.Field));
                Assert.False(string.IsNullOrWhiteSpace(finding.Rule));
            });

            // W-2 has no wellhead position, which the engine reports as QC003 on the position field.
            var position = workspace.Findings.Single(finding =>
                finding.Scope == "well" && finding.Rule == "QC003");

            Assert.Equal("W-2", position.WellId);
            Assert.Equal("well", position.Entity);
            Assert.Equal("position", position.Field);
            Assert.Equal(OgFieldQcRules.Warning, position.Severity);

            // W-1 and W-3 have no survey station, which is QC002 on the survey field - one finding each.
            var survey = workspace.Findings
                .Where(finding => finding.Scope == "well" && finding.Rule == "QC002")
                .ToList();

            Assert.Equal(2, survey.Count);
            Assert.Equal(["W-1", "W-3"], survey.Select(finding => finding.WellId).OrderBy(id => id).ToList());
            Assert.All(survey, finding => Assert.Equal("survey", finding.Field));

            // AC-2: coverage counts against the wells read - W-2 has no position, so two of three carry one; only W-2 has a survey.
            Assert.True(workspace.CoverageKnown);
            Assert.Equal(3, workspace.Coverage.Single(item => item.Field == "position").WellsTotal);
            Assert.Equal(2, workspace.Coverage.Single(item => item.Field == "position").WellsWith);
            Assert.Equal(1, workspace.Coverage.Single(item => item.Field == "survey").WellsWith);

            // ---- AC-4: the duplicate is a proposal, and nothing has been merged ----
            Assert.True(workspace.DuplicatesRun);

            var proposal = workspace.Duplicates.FirstOrDefault(duplicate =>
                (duplicate.NameA == "Alpha 1" && duplicate.NameB == "Alpha 1"));

            Assert.NotNull(proposal);
            Assert.False(proposal.Decided);
            Assert.Contains("proposed only", proposal.Describe(), StringComparison.Ordinal);
            Assert.True(proposal.Reasons.Count > 0, proposal.Describe());

            // Deciding it rejects the pair, and the decision is written beside the environment.
            var decided = await service.DecideDuplicateAsync(
                new OgFieldQcRequest(SourceName, 50, WithDuplicates: true),
                proposal.KeyA, proposal.KeyB, confirmed: false, "sara");

            Assert.True(decided.Success, decided.Message + " :: " + string.Join(" | ", reporter.Failures));

            var rejected = decided.Value!.Duplicates.FirstOrDefault(duplicate =>
                (duplicate.KeyA == proposal.KeyA && duplicate.KeyB == proposal.KeyB) ||
                (duplicate.KeyA == proposal.KeyB && duplicate.KeyB == proposal.KeyA));

            // A rejected pair is not proposed again, which is the engine's own rule - so it is gone from the list.
            Assert.Null(rejected);
            Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(environmentFile)!, "well-crosswalk.json")));

            // Confirming a pair that is no longer proposed is refused rather than recorded against nothing.
            var nothing = await service.DecideDuplicateAsync(
                new OgFieldQcRequest(SourceName, 50, WithDuplicates: true),
                proposal.KeyA, proposal.KeyB, confirmed: true, "sara");

            Assert.False(nothing.Success);
            Assert.Equal(OgFieldQcRules.CodeDuplicateUnknown, nothing.Reference);

            // ---- AC-1: nothing is applied by looking at a proposal ----
            Assert.False(File.Exists(environmentFile));

            var fabricated = new OgSetupSuggestion("units", "component.top.unit", "ftt", "ft", "foot", 0.9, "similarity");
            var accepted = await service.AcceptSuggestionAsync(SourceName, fabricated, "sara");

            Assert.False(accepted.Success);
            Assert.False(File.Exists(environmentFile));

            // ---- AC-3: a cancelled run reports the scope it covered, and only those findings ----
            using var cancellation = new CancellationTokenSource();

            var cancelAfterFirst = new Callback(progress =>
            {
                if (progress.Done == 1)
                {
                    cancellation.Cancel();
                }
            });

            var cancelled = await service.RunAsync(
                new OgFieldQcRequest(SourceName, 50, WithDuplicates: false, Progress: cancelAfterFirst),
                cancellation.Token);

            Assert.True(cancelled.Success, cancelled.Message + " :: " + string.Join(" | ", reporter.Failures));

            var partial = cancelled.Value!;

            Assert.True(partial.Cancelled);
            Assert.Equal(1, partial.CoveredWells);
            Assert.Equal(3, partial.TotalWells);
            Assert.Contains("cancelled after 1 of 3 well(s)", partial.SummaryText, StringComparison.Ordinal);
            // Whatever well came first, the findings are about that one well only - and they match its findings in the full run.
            var coveredWell = partial.Findings
                .Where(finding => finding.Scope == "well")
                .Select(finding => finding.WellId)
                .Distinct()
                .ToList();

            Assert.Single(coveredWell);
            Assert.Equal(
                workspace.Findings.Count(finding => finding.Scope == "well" && finding.WellId == coveredWell[0]),
                partial.Findings.Count(finding => finding.Scope == "well"));

            // ---- AC-2 again: a run cancelled before the first well has no coverage, which is unknown and not zero ----
            using var alreadyCancelled = new CancellationTokenSource();
            alreadyCancelled.Cancel();

            var none = await service.RunAsync(
                new OgFieldQcRequest(SourceName, 50, WithDuplicates: false), alreadyCancelled.Token);

            Assert.True(none.Success, none.Message);
            Assert.Equal(0, none.Value!.CoveredWells);
            Assert.False(none.Value.CoverageKnown);
            Assert.Contains("Coverage unknown", none.Value.CoverageText, StringComparison.Ordinal);

            // ---- AC-6: the export is whatever filter the grid is showing ----
            var full = workspace.Findings;

            Assert.Equal(1 + full.Count,
                OgFieldQcRules.Csv(full).Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length);

            var warningsOnly = OgFieldQcRules.Filter(full, OgFieldQcRules.Warning, null);

            Assert.All(warningsOnly, finding => Assert.Equal(OgFieldQcRules.Warning, finding.Severity));
            Assert.Equal(1 + warningsOnly.Count,
                OgFieldQcRules.Csv(warningsOnly)
                    .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length);

            var narrowed = OgFieldQcRules.Filter(full, null, "position");

            Assert.All(narrowed, finding => Assert.Contains("position", finding.Describe(), StringComparison.Ordinal));
            Assert.Equal(1 + narrowed.Count,
                OgFieldQcRules.Csv(narrowed)
                    .Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Length);

            // The thresholds the run used are stated, so a finding can be explained.
            Assert.Contains(workspace.Thresholds, threshold =>
                threshold.Contains("QC001..QC006", StringComparison.Ordinal));
            Assert.Contains(workspace.Thresholds, threshold =>
                threshold.Contains("minimumMatchScore", StringComparison.Ordinal));
        }
        finally
        {
            environment.DataSources.Remove(SourceName);
        }
    }

    // ------------------------------------------------------------------ the fixture

    /// <summary>
    /// Three wells in one source: W-1 has no survey, W-2 has no wellhead position, and W-3 repeats W-1's name, UWI and location so the
    /// reconciliation has something to propose.
    /// </summary>
    private static void WriteFixture(string folder)
    {
        File.WriteAllText(Path.Combine(folder, "wells.csv"),
            "well_id,name,uwi,operator,field,country,latitude,longitude,total_depth,depth_unit,depth_datum,survey_north\n" +
            "W-1,Alpha 1,30/1-A,Probe Energy,North,Norway,58.5,2.1,2000,ft,MSL,grid\n" +
            "W-2,Alpha 2,30/1-B,Probe Energy,North,Norway,,,1800,ft,MSL,grid\n" +
            "W-3,Alpha 1,30/1-A,Probe Energy,North,Norway,58.5,2.1,2000,ft,MSL,grid\n");

        File.WriteAllText(Path.Combine(folder, "hole_sections.csv"),
            "well_id,section_id,top,bottom,bit_size,diameter_unit,depth_unit\n" +
            "W-1,H1,0,2000,8.5,in,ft\n" +
            "W-2,H1,0,1800,8.5,in,ft\n" +
            "W-3,H1,0,2000,8.5,in,ft\n");

        File.WriteAllText(Path.Combine(folder, "strings.csv"),
            "well_id,string_id,kind,name,top,bottom,od,id_diameter,grade,depth_unit,diameter_unit\n" +
            "W-1,S1,Casing,Production,0,2000,9.625,8.681,L80,ft,in\n" +
            "W-2,S1,Casing,Production,0,1800,9.625,8.681,L80,ft,in\n" +
            "W-3,S1,Casing,Production,0,2000,9.625,8.681,L80,ft,in\n");

        // Only W-2 has a survey, so W-1 and W-3 are missing one - which is the finding the cancellation test relies on.
        File.WriteAllText(Path.Combine(folder, "survey.csv"),
            "well_id,md,inclination,azimuth,tvd,depth_unit\n" +
            "W-2,0,0,0,0,ft\n" +
            "W-2,1800,0,0,1800,ft\n");
    }

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "int06-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Sub(string name)
        {
            var folder = Path.Combine(_root, name);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_root))
                {
                    Directory.Delete(_root, recursive: true);
                }
            }
            catch (IOException)
            {
                // The OS temp folder is its own to clean.
            }
        }
    }

    /// <summary>A progress callback that can act on the run as it goes - how the cancellation test stops it at a well boundary.</summary>
    private sealed class Callback : IProgress<(int done, int total)>
    {
        private readonly Action<(int Done, int Total)> _onProgress;

        public Callback(Action<(int Done, int Total)> onProgress) => _onProgress = onProgress;

        public void Report((int done, int total) value) => _onProgress(value);
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
            Task.FromResult(OgResult<OgSetupWorkspace>.Failed(
                "there is no environment to write to, so nothing is applied", OgSetupRules.CodeNoEnvironment));

        public Task<OgResult<OgSetupWorkspace>> VerifyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));

        public Task<OgResult<OgSetupWorkspace>> HandoverAsync(string archiveFolder, string actor,
            CancellationToken cancellationToken = default) => Task.FromResult(OgResult<OgSetupWorkspace>.Failed("none", "none"));
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
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
