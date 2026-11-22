using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-05 against a real company folder: a country preset produces an environment the engine's own validation accepts (AC-1), a data
/// source is tested and its result recorded (AC-2), a proposal is only applied when it is accepted (AC-3), coverage is per source with
/// an explicit unknown (AC-4), a hand-over is refused while a case fails (AC-5), and a failed save leaves the environment as it was
/// (AC-6).
/// </summary>
/// <remarks>
/// The setup engine works on files, not on a database, so this test drives it against a temporary company folder under the same
/// opt-in switch as the other integration tests.
/// </remarks>
public sealed class WellboreSetupGatewayTests
{
    private const string WellCsv =
        "well_id,name,uwi,operator,field,country,total_depth,depth_unit,depth_datum,survey_north\n" +
        "W-1,Demo 1,30/1-A,Probe Energy,North,Norway,8500,ft,MSL,grid\n";

    private const string SectionCsv =
        "well_id,section_id,top,bottom,bit_size,diameter_unit,depth_unit\n" +
        "W-1,H1,0,1500,26,in,ft\n" +
        "W-1,H2,1500,7000,12.25,in,ft\n";

    private const string StringCsv =
        "well_id,string_id,kind,name,top,bottom,od,id_diameter,grade,depth_unit,diameter_unit\n" +
        "W-1,S1,Casing,Surface,0,1500,20,18.73,K55,ft,in\n" +
        "W-1,S2,Casing,Production,0,7000,9.625,8.681,L80,ft,in\n";

    private const string ComponentCsv =
        "well_id,component_id,type,name,top,bottom,depth_unit\n" +
        "W-1,C1,PACKR,Packer 1,6000,6010,ftt\n";

    private const string TopCsv =
        "well_id,name,depth,lithology,interpreter,depth_unit\n" +
        "W-1,Hod,7500,Sandstne,sara,ft\n";

    private const string SurveyCsv =
        "well_id,md,inclination,azimuth,tvd,depth_unit\n" +
        "W-1,0,0,0,0,ft\n" +
        "W-1,3000,12.5,45,2976.3,ft\n";

    [IntegrationFact]
    public async Task A_preset_environment_is_created_tested_and_handed_over()
    {        using var scope = new Scratch();

        var root = scope.Folder;
        var environmentFile = Path.Combine(root, "environment.json");
        var reporter = new Recording();
        var service = new OgWellboreSetupService(reporter, () => environmentFile);

        // ---- before anything exists: the states are readable and a command says why it cannot run ----
        var empty = await service.LoadAsync();

        Assert.True(empty.Success, empty.Message);
        Assert.False(empty.Value!.Exists);
        Assert.NotEmpty(empty.Value.Steps);
        Assert.All(empty.Value.Steps, step => Assert.Equal(OgSetupRules.NotStarted, step.State));
        Assert.Contains("choose a country preset", empty.Value.SummaryText, StringComparison.Ordinal);

        var noEnvironment = await service.SetAsync("name", "Probe", "sara");

        Assert.False(noEnvironment.Success);
        Assert.Equal(OgSetupRules.CodeNoEnvironment, noEnvironment.Reference);

        // ---- AC-1: a country preset produces an environment the engine validates ----
        var countries = await service.CountriesAsync();

        Assert.True(countries.Success, countries.Message);
        Assert.True(countries.Value!.Count >= 40, "the presets are shipped: " + countries.Value.Count);

        var norway = countries.Value!.Single(country => country.Code == "NO");

        var created = await service.CreateAsync("NO", "Probe Energy", "sara");

        Assert.True(created.Success, created.Message + " :: " + string.Join(" | ", reporter.Failures));

        var workspace = created.Value!;

        Assert.True(workspace.Exists);
        Assert.True(File.Exists(environmentFile));
        Assert.Equal("NO", workspace.Country);
        Assert.Equal(norway.Name, workspace.CountryName);
        Assert.Equal("Probe Energy", workspace.CompanyName);

        // AC-1's own sentence: the engine's validation accepts what the preset produced.
        Assert.Empty(workspace.Validation);
        Assert.Contains("passes the engine's own validation", workspace.ValidationText, StringComparison.Ordinal);

        // The engine's own steps are what the page shows, in its own words.
        Assert.NotEmpty(workspace.Steps);
        Assert.All(workspace.Steps, step => Assert.Contains(step.State, OgSetupRules.States));

        // A second create is refused rather than splitting the settings across two files.
        var again = await service.CreateAsync("NO", "Probe Energy", "sara");

        Assert.False(again.Success);
        Assert.Equal(OgSetupRules.CodeAlreadyExists, again.Reference);

        // ---- AC-2: a data source is tested and its result is recorded ----
        var sourceFolder = scope.Sub("wells");

        File.WriteAllText(Path.Combine(sourceFolder, "wells.csv"), WellCsv);
        File.WriteAllText(Path.Combine(sourceFolder, "hole_sections.csv"), SectionCsv);
        File.WriteAllText(Path.Combine(sourceFolder, "strings.csv"), StringCsv);
        File.WriteAllText(Path.Combine(sourceFolder, "components.csv"), ComponentCsv);
        File.WriteAllText(Path.Combine(sourceFolder, "tops.csv"), TopCsv);
        File.WriteAllText(Path.Combine(sourceFolder, "survey.csv"), SurveyCsv);

        var added = await service.AddSourceAsync(
            "Wells", """{ "kind": "csv", "path": "wells" }""", "sara");

        Assert.True(added.Success, added.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Contains(added.Value!.Sources, source => source.Name == "Wells");

        var tested = await service.TestSourceAsync("Wells", null);

        Assert.True(tested.Success, tested.Message);

        // AC-2: the result is recorded whether the source loaded or not, and the loader's own warnings travel with it.
        var wells = tested.Value!.Sources.Single(source => source.Name == "Wells");

        Assert.True(wells.Loaded, wells.Describe());
        Assert.NotEmpty(wells.Counts);
        Assert.Contains(wells.Warnings, warning => warning.Contains("strings.kind", StringComparison.Ordinal));

        // A source that is not configured is not tested - the refusal names it.
        var unknown = await service.CoverageAsync("Nope");

        Assert.False(unknown.Success);
        Assert.Equal(OgSetupRules.CodeSourceUnknown, unknown.Reference);

        // A definition that is not JSON is refused with what a definition is, and nothing is written.
        var beforeBadDefinition = File.ReadAllText(environmentFile);

        var badDefinition = await service.AddSourceAsync("Broken", "not json", "sara");

        Assert.False(badDefinition.Success);
        Assert.Equal(OgSetupRules.CodeSourceInvalid, badDefinition.Reference);
        Assert.Contains("a source is described by its kind and its path", badDefinition.Message!, StringComparison.Ordinal);
        Assert.Equal(beforeBadDefinition, File.ReadAllText(environmentFile));

        // ---- AC-4: coverage per source, and an explicit unknown when it cannot be computed ----
        var coverage = await service.CoverageAsync("Wells");

        Assert.True(coverage.Success, coverage.Message + " :: " + string.Join(" | ", reporter.Failures));

        var report = Assert.Single(coverage.Value!.Coverage);

        Assert.Equal("Wells", report.Source);
        Assert.True(report.Known, report.Describe());
        Assert.True(report.Wells >= 1, report.Describe());


        // A source the environment cannot open is reported as unknown with the reason, never as nil problems. A missing table file
        // is not that case: the CSV source reads a missing file as an empty table, so this uses a kind nothing can open.
        Assert.True((await service.AddSourceAsync("Broken", """{ "kind": "nonsense", "path": "nothing" }""", "sara")).Success);

        var broken = await service.CoverageAsync("Broken");

        Assert.True(broken.Success, broken.Message);
        Assert.False(broken.Value!.Coverage.Single().Known);
        Assert.Contains("coverage unknown", broken.Value.CoverageText, StringComparison.Ordinal);

        // ---- AC-3: a proposal is shown and only applied when it is accepted ----
        var beforeSuggest = File.ReadAllText(environmentFile);

        var suggested = await service.SuggestAsync("Wells");

        Assert.True(suggested.Success, suggested.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.NotNull(suggested.Value);

        // AC-3: computing the proposals writes nothing at all.
        Assert.Equal(beforeSuggest, File.ReadAllText(environmentFile));

        var beforeAccept = File.ReadAllText(environmentFile);

        // An acceptance the engine does not offer is refused, and the file is untouched.
        var refused = await service.AcceptAsync("Wells", "codes", "component.type", "NOPE", "NOTHING", "sara");

        Assert.False(refused.Success);
        Assert.Equal(OgSetupRules.CodeSuggestionUnknown, refused.Reference);
        Assert.Equal(beforeAccept, File.ReadAllText(environmentFile));

        // And with a proposal the engine does offer, accepting it is what writes the mapping.
        Assert.NotEmpty(suggested.Value.Suggestions);

        if (suggested.Value.Suggestions.FirstOrDefault() is { } proposal)
        {
            var accepted = await service.AcceptAsync(
                "Wells", proposal.Area, proposal.Where, proposal.Value, proposal.Target, "sara");

            Assert.True(accepted.Success, accepted.Message + " :: " + string.Join(" | ", reporter.Failures));
            Assert.Contains(accepted.Value!.Warnings,
                warning => warning.Contains("because it was accepted", StringComparison.Ordinal));
            Assert.True(File.ReadAllText(environmentFile).Length > beforeAccept.Length,
                "an accepted proposal is written to the environment");
        }

        // ---- AC-5: a hand-over is refused while a case fails ----
        var verified = await service.VerifyAsync();

        Assert.True(verified.Success, verified.Message);
        Assert.Equal(verified.Value!.Outstanding, verified.Value.Verification.Count(row => !row.Passed));

        // A case whose expected value is wrong makes the hand-over refuse and name it.
        var captured = await service.VerifyAsync();

        Assert.True(captured.Success, captured.Message);

        // A unit case with an impossible expectation: 1 ft is 0.3048 m, and the engine's own evaluator compares it with 99.
        var failing = await service.SetAsync(
            "verification.cases", """
            [ { "name": "A case that cannot pass", "kind": "unit", "from": "ft", "to": "m",
                "value": 1, "expected": 99, "tolerance": 0.000001 } ]
            """, "sara");

        Assert.True(failing.Success, failing.Message + " :: " + string.Join(" | ", reporter.Failures));

        var archive = scope.Sub("handover");

        var blocked = await service.HandoverAsync(archive, "sara");

        Assert.False(blocked.Success);
        Assert.Equal(OgSetupRules.CodeHandoverBlocked, blocked.Reference);
        Assert.Contains("refused while", blocked.Message!, StringComparison.Ordinal);
        Assert.Contains("A case that cannot pass", blocked.Message!, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(archive, "environment.json")));

        // With the case removed, nothing is outstanding and the hand-over writes the archive.
        Assert.True((await service.SetAsync("verification.cases", null, "sara")).Success);

        var handed = await service.HandoverAsync(archive, "sara");

        Assert.True(handed.Success, handed.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.True(handed.Value!.HandedOver);
        Assert.True(File.Exists(Path.Combine(archive, "environment.json")));
        Assert.NotEmpty(handed.Value.Warnings);

        // ---- AC-6: a failed save leaves the previous environment intact ----
        var before = File.ReadAllText(environmentFile);

        File.SetAttributes(environmentFile, FileAttributes.ReadOnly);

        OgResult<OgSetupWorkspace> failed;

        try
        {
            failed = await service.SetAsync("name", "A different company", "sara");
        }
        finally
        {
            File.SetAttributes(environmentFile, FileAttributes.Normal);
        }

        Assert.False(failed.Success);
        Assert.Contains("previous environment is unchanged", failed.Message!, StringComparison.Ordinal);
        Assert.Equal(before, File.ReadAllText(environmentFile));

        // ---- and a reload reads the file that is there ----
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);
        Assert.Equal("Probe Energy", reloaded.Value!.CompanyName);

        // A blank actor is refused on every write, so the trail names someone.
        Assert.Equal(OgSetupRules.CodeActorRequired, (await service.SetAsync("name", "x", " ")).Reference);
        Assert.Equal(OgSetupRules.CodeActorRequired, (await service.HandoverAsync(archive, " ")).Reference);
        Assert.Equal(OgSetupRules.CodeActorRequired,
            (await service.AddSourceAsync("Wells", """{ "kind": "csv", "path": "wells" }""", " ")).Reference);
    }

    // ------------------------------------------------------------------ fixtures

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "int05-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Folder => _root;

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
                foreach (var file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                }

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
