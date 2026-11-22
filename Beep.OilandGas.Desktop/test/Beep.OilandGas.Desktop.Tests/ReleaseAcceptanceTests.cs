using System.IO.Compression;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Release;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-05: the checklist itself - thirty items, the five that are computed from artefacts, the rule that nothing unproven is ever a pass,
/// attestation with evidence, waivers with an approver and an expiry, and the evidence pack.
/// </summary>
public sealed class ReleaseAcceptanceTests
{
    private const string PageFileName = "ReleaseAcceptancePageView.cs";

    private static OgPermissions Administrator => new(["SystemAdmin"], [], true);

    private static OgPermissions WithoutPermissions => new(["Viewer"], [], true);

    private static OgPermissions With(params string[] permissions) => new(["Release"], permissions, true);

    private static OgReleaseAcceptanceService Service(Scratch scratch, string? smoke = null, string? sourceRoot = null,
        string? planFolder = null) =>
        new(new Quiet(),
            () => Path.Combine(scratch.Root, "release-acceptance.json"),
            () => smoke,
            () => sourceRoot,
            () => planFolder,
            () => "9.9.9");

    [Fact]
    public void The_checklist_is_the_thirty_items_the_feature_lists()
    {
        var checklist = OgReleaseRules.Checklist;

        Assert.Equal(30, checklist.Count);
        Assert.Equal(Enumerable.Range(1, 30), checklist.Select(item => item.Number));
        Assert.Equal(checklist.Count, checklist.Select(item => item.Key).Distinct(StringComparer.Ordinal).Count());

        // The five areas the feature groups the items into, and nothing else.
        Assert.Equal(["Build and code", "Behaviour", "Data and safety", "Quality", "Delivery"],
            checklist.Select(item => item.Category).Distinct(StringComparer.Ordinal));

        // Five are computed from an artefact; the other twenty-five need a person.
        Assert.Equal(5, checklist.Count(item => item.Kind == OgAcceptanceKind.Computed));
        Assert.Equal(["no-package-version", "smoke-ok", "no-db-outside-data", "accessibility", "budgets"],
            checklist.Where(item => item.Kind == OgAcceptanceKind.Computed).Select(item => item.Key));

        Assert.All(checklist, item => Assert.False(string.IsNullOrWhiteSpace(item.Text)));
    }

    [Fact]
    public async Task With_no_artefact_and_no_attestation_nothing_is_a_pass()
    {
        // The feature exists because a build that compiled was once taken for a release: an unproven item is NotRun, never Pass.
        using var scratch = new Scratch();

        var report = await Service(scratch).EvaluateAsync();

        Assert.True(report.Success, report.Message);
        Assert.False(report.Value!.Ready);
        Assert.Equal(0, report.Value.Passed);
        Assert.Equal(30, report.Value.NotRun);
        Assert.Equal(30, report.Value.Blockers.Count);

        Assert.Contains("NOT READY TO SHIP", report.Value.SummaryText, StringComparison.Ordinal);
        Assert.Contains("NOT RUN", report.Value.ItemsText, StringComparison.Ordinal);

        // A computed item with no source folder and no smoke result says why, and does not guess.
        var smoke = report.Value.Items.Single(item => item.Key == "smoke-ok");

        Assert.Equal(OgAcceptanceState.NotRun, smoke.State);
        Assert.Contains("--smoke", smoke.Detail, StringComparison.Ordinal);

        var packages = report.Value.Items.Single(item => item.Key == "no-package-version");

        Assert.Equal(OgAcceptanceState.NotRun, packages.State);
        Assert.Contains("source folder", packages.Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_smoke_result_computes_three_items_and_a_failure_stays_a_failure()
    {
        // The build's own numbers: the smoke run passes and its budgets are inside their limits, but its skin contrast audit fails. A
        // checklist that turned that into "not run" would be the thing this feature was written to prevent.
        using var scratch = new Scratch();

        var smoke = Path.Combine(scratch.Root, "smoke-result.txt");

        File.WriteAllText(smoke, string.Join(Environment.NewLine,
            "result: ok",
            "pages-declared: 124",
            "views-constructed: True",
            "performance-measured: 6",
            "performance-over-budget: 0",
            "accessibility-unnamed-count: 0",
            "accessibility-untabbable-count: 0",
            "skin-contrast-failures: 66"));

        var report = await Service(scratch, smoke).EvaluateAsync();

        Assert.True(report.Success, report.Message);

        var items = report.Value!.Items;

        var smokeItem = items.Single(item => item.Key == "smoke-ok");

        Assert.Equal(OgAcceptanceState.Pass, smokeItem.State);
        Assert.Contains("views-constructed: True", smokeItem.Detail, StringComparison.Ordinal);

        var budgets = items.Single(item => item.Key == "budgets");

        Assert.Equal(OgAcceptanceState.Pass, budgets.State);
        Assert.Contains("6 budget(s)", budgets.Detail, StringComparison.Ordinal);

        var accessibility = items.Single(item => item.Key == "accessibility");

        Assert.Equal(OgAcceptanceState.Fail, accessibility.State);
        Assert.Contains("66 skin contrast failure(s)", accessibility.Detail, StringComparison.Ordinal);
        Assert.Contains(accessibility, report.Value.Blockers);

        // And the report is not ready, on its own numbers: two computed items pass, the accessibility item does not, and the two
        // source scans need a checkout this test did not give it.
        Assert.False(report.Value.Ready);
        Assert.Equal(2, report.Value.Passed);

        // A smoke run that failed is a failure, not the absence of one.
        File.WriteAllText(smoke, string.Join(Environment.NewLine,
            "result: failed",
            "views-constructed: True",
            "performance-over-budget: 0",
            "accessibility-unnamed-count: 0",
            "accessibility-untabbable-count: 0",
            "skin-contrast-failures: 0"));

        var failed = await Service(scratch, smoke).EvaluateAsync();

        Assert.Equal(OgAcceptanceState.Fail, failed.Value!.Items.Single(item => item.Key == "smoke-ok").State);

        // A result without the lines is a failure too: it cannot be read as a pass.
        File.WriteAllText(smoke, "nothing useful here");

        var unreadable = await Service(scratch, smoke).EvaluateAsync();

        Assert.Equal(OgAcceptanceState.Fail, unreadable.Value!.Items.Single(item => item.Key == "smoke-ok").State);
        Assert.Contains("no 'result' or 'views-constructed' line",
            unreadable.Value.Items.Single(item => item.Key == "smoke-ok").Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_source_scans_pass_on_this_checkout()
    {
        // Items 7 and 18, computed from the real sources: central package management, and no data source outside the data project.
        var root = SourceRoot.Locate();
        using var scratch = new Scratch();

        var report = await Service(scratch, sourceRoot: Path.Combine(root, "src")).EvaluateAsync();

        Assert.True(report.Success, report.Message);

        var packages = report.Value!.Items.Single(item => item.Key == "no-package-version");

        Assert.Equal(OgAcceptanceState.Pass, packages.State);
        Assert.Contains("none carries one", packages.Detail, StringComparison.Ordinal);

        var database = report.Value.Items.Single(item => item.Key == "no-db-outside-data");

        Assert.Equal(OgAcceptanceState.Pass, database.State);
        Assert.Contains("no file outside the data project", database.Detail, StringComparison.Ordinal);

        // A folder that holds a versioned reference and a screen that names the editor is a failure naming both.
        var broken = Path.Combine(scratch.Root, "src");

        Directory.CreateDirectory(Path.Combine(broken, "Some.App", "Views"));

        File.WriteAllText(Path.Combine(broken, "Some.App", "Some.App.csproj"),
            "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <ItemGroup>\n    <PackageReference Include=\"SkiaSharp\" Version=\"3.0.0\" />\n  </ItemGroup>\n</Project>");

        File.WriteAllText(Path.Combine(broken, "Some.App", "Views", "Page.cs"),
            "public sealed class Page { private IDMEEditor _editor; }");

        var failing = await Service(scratch, sourceRoot: broken).EvaluateAsync();

        Assert.Equal(OgAcceptanceState.Fail, failing.Value!.Items.Single(item => item.Key == "no-package-version").State);
        Assert.Equal(OgAcceptanceState.Fail, failing.Value.Items.Single(item => item.Key == "no-db-outside-data").State);
        Assert.Contains("Some.App/Views/Page.cs",
            failing.Value.Items.Single(item => item.Key == "no-db-outside-data").Detail, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Attesting_needs_the_permission_an_actor_and_evidence()
    {
        using var scratch = new Scratch();

        var service = Service(scratch);

        Assert.Equal(OgReleaseRules.CodeUnknownItem,
            (await service.AttestAsync("no-such-item", "x", "sara", Administrator)).Reference);

        var noPermission = await service.AttestAsync("build-clean", "seen", "sara", WithoutPermissions);

        Assert.False(noPermission.Success);
        Assert.Equal(OgReleaseRules.CodePermission, noPermission.Reference);
        Assert.Contains("release.attest", noPermission.Message!, StringComparison.Ordinal);

        Assert.Equal(OgReleaseRules.CodeActorRequired,
            (await service.AttestAsync("build-clean", "seen", " ", Administrator)).Reference);

        // An attestation without evidence is an assertion, and the checklist exists to keep assertions out.
        var noEvidence = await service.AttestAsync("build-clean", "  ", "sara", Administrator);

        Assert.False(noEvidence.Success);
        Assert.Equal(OgReleaseRules.CodeEvidenceRequired, noEvidence.Reference);

        var attested = await service.AttestAsync("build-clean", "the release build log, attached", "sara", Administrator);

        Assert.True(attested.Success, attested.Message);
        Assert.Equal(OgAcceptanceState.Pass, attested.Value!.State);
        Assert.Equal("sara", attested.Value.AttestedBy);
        Assert.NotNull(attested.Value.AttestedAt);

        // It survives a re-evaluation, because it was written to the store.
        var again = await service.EvaluateAsync();

        Assert.Equal(OgAcceptanceState.Pass, again.Value!.Items.Single(item => item.Key == "build-clean").State);

        // A person holding the permission may attest, and one holding nothing at all may not.
        Assert.True((await service.AttestAsync("analyzers-clean", "build log, attached",
            "sara", With(OgReleaseRules.AttestPermission))).Success);
        Assert.Equal(OgReleaseRules.CodePermission,
            (await service.AttestAsync("no-silent-handler", "guard test output, attached", "sara",
                new OgPermissions([], [], permissionSourceAvailable: false))).Reference);
    }

    [Fact]
    public async Task Attested_evidence_that_is_not_there_fails_with_the_path()
    {
        // The feature's state table: an evidence link that is broken fails and names the missing path.
        using var scratch = new Scratch();

        var plan = Path.Combine(scratch.Root, "plan");

        Directory.CreateDirectory(Path.Combine(plan, "evidence", "STG-18-REL-05"));

        var service = Service(scratch, planFolder: plan);

        var attested = await service.AttestAsync("criteria-tested",
            "evidence/STG-18-REL-05/observed-evidence.json", "sara", Administrator);

        Assert.True(attested.Success, attested.Message);

        // The file is not there yet, so the item is a failure that names it.
        var missing = await service.EvaluateAsync();
        var item = missing.Value!.Items.Single(candidate => candidate.Key == "criteria-tested");

        Assert.Equal(OgAcceptanceState.Fail, item.State);
        Assert.Contains("is not there", item.Detail, StringComparison.Ordinal);
        Assert.Contains(Path.Combine("STG-18-REL-05", "observed-evidence.json"), item.Detail, StringComparison.Ordinal);

        File.WriteAllText(Path.Combine(plan, "evidence", "STG-18-REL-05", "observed-evidence.json"), "{}");

        var present = await service.EvaluateAsync();

        Assert.Equal(OgAcceptanceState.Pass,
            present.Value!.Items.Single(candidate => candidate.Key == "criteria-tested").State);
    }

    [Fact]
    public async Task A_waiver_needs_another_person_a_reason_and_a_future_expiry_and_expires_back_to_failure()
    {
        using var scratch = new Scratch();

        var service = Service(scratch);
        var future = DateTime.UtcNow.AddDays(14);

        Assert.Equal(OgReleaseRules.CodePermission,
            (await service.WaiveAsync("page-states", "nadia", "seen on the clean VM", future, "sara", WithoutPermissions)).Reference);

        // The person asking cannot be the person approving.
        var samePerson = await service.WaiveAsync("page-states", "sara", "seen on the clean VM", future, "sara", Administrator);

        Assert.False(samePerson.Success);
        Assert.Equal(OgReleaseRules.CodeWaiverSamePerson, samePerson.Reference);

        Assert.Equal(OgReleaseRules.CodeWaiverReason,
            (await service.WaiveAsync("page-states", "nadia", " ", future, "sara", Administrator)).Reference);

        var past = await service.WaiveAsync("page-states", "nadia", "seen on the clean VM", DateTime.UtcNow.AddDays(-1),
            "sara", Administrator);

        Assert.False(past.Success);
        Assert.Equal(OgReleaseRules.CodeWaiverExpiry, past.Reference);

        var waived = await service.WaiveAsync("page-states", "nadia", "seen on the clean VM", future, "sara", Administrator);

        Assert.True(waived.Success, waived.Message);
        Assert.Equal(OgAcceptanceState.Waived, waived.Value!.State);
        Assert.Equal("nadia", waived.Value.Waiver!.Approver);
        Assert.True(waived.Value.Satisfied(DateTime.UtcNow));

        var evaluated = await service.EvaluateAsync();

        Assert.Equal(OgAcceptanceState.Waived,
            evaluated.Value!.Items.Single(item => item.Key == "page-states").State);

        // The rule itself: a waiver whose expiry has passed is a failure, and the sentence says when and who.
        var expired = new OgAcceptanceWaiver("nadia", "it was fine in March", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(-30));
        var applied = OgReleaseRules.Apply(OgAcceptanceState.NotRun, "not attested yet", expired, DateTime.UtcNow);

        Assert.Equal(OgAcceptanceState.Fail, applied.State);
        Assert.Contains("the waiver expired on", applied.Detail, StringComparison.Ordinal);
        Assert.Contains("nadia", applied.Detail, StringComparison.Ordinal);

        Assert.False(expired.Active(DateTime.UtcNow));
    }

    [Fact]
    public void The_evidence_pack_holds_the_evidence_and_a_manifest()
    {
        using var scratch = new Scratch();

        var plan = Path.Combine(scratch.Root, "plan");

        Directory.CreateDirectory(Path.Combine(plan, "evidence", "STG-01-ONE"));
        Directory.CreateDirectory(Path.Combine(plan, "evidence", "STG-02-TWO"));

        File.WriteAllText(Path.Combine(plan, "evidence", "STG-01-ONE", "observed-evidence.json"), """{"id":"STG-01/ONE"}""");
        File.WriteAllText(Path.Combine(plan, "evidence", "STG-02-TWO", "observed-evidence.json"), """{"id":"STG-02/TWO"}""");
        File.WriteAllText(Path.Combine(plan, "evidence", "STG-02-TWO", "notes.txt"), "not evidence");

        var service = Service(scratch, planFolder: plan);
        var zip = Path.Combine(scratch.Root, "out", "evidence.zip");

        var exported = service.ExportEvidenceAsync(plan, zip, "sara").GetAwaiter().GetResult();

        Assert.True(exported.Success, exported.Message);
        Assert.True(File.Exists(zip));
        Assert.Equal(2, exported.Value!.Files);
        Assert.True(exported.Value.Bytes > 200, exported.Value.Describe());

        using var archive = ZipFile.OpenRead(zip);

        Assert.Contains(archive.Entries, entry => entry.FullName == "manifest.json");
        Assert.Contains(archive.Entries, entry => entry.FullName == "evidence/STG-01-ONE/observed-evidence.json");
        Assert.Contains(archive.Entries, entry => entry.FullName == "evidence/STG-02-TWO/observed-evidence.json");
        Assert.DoesNotContain(archive.Entries, entry => entry.FullName.EndsWith("notes.txt", StringComparison.Ordinal));

        using var reader = new StreamReader(archive.GetEntry("manifest.json")!.Open());

        var manifest = reader.ReadToEnd();

        Assert.Contains("\"productVersion\": \"9.9.9\"", manifest, StringComparison.Ordinal);
        Assert.Contains("\"files\": 2", manifest, StringComparison.Ordinal);

        // An item that names evidence which is not in the pack is reported before the release is called ready.
        var attested = service.AttestAsync("criteria-tested",
            "evidence/STG-09-MISSING/observed-evidence.json", "sara", Administrator).GetAwaiter().GetResult();

        Assert.True(attested.Success, attested.Message);

        var withMissing = service.ExportEvidenceAsync(plan, Path.Combine(scratch.Root, "out", "second.zip"), "sara")
            .GetAwaiter().GetResult();

        Assert.True(withMissing.Success, withMissing.Message);
        Assert.NotEmpty(withMissing.Value!.Missing);
        Assert.Contains("STG-09-MISSING", withMissing.Value.MissingText, StringComparison.Ordinal);

        // Refusals: no folder, no path, no actor, and a folder with no evidence.
        Assert.Equal(OgReleaseRules.CodeNoPlan,
            service.ExportEvidenceAsync(Path.Combine(scratch.Root, "absent"), zip, "sara").GetAwaiter().GetResult().Reference);
        Assert.Equal(OgReleaseRules.CodeExportPath,
            service.ExportEvidenceAsync(plan, " ", "sara").GetAwaiter().GetResult().Reference);
        Assert.Equal(OgReleaseRules.CodeActorRequired,
            service.ExportEvidenceAsync(plan, zip, " ").GetAwaiter().GetResult().Reference);

        var bare = Path.Combine(scratch.Root, "bare");

        Directory.CreateDirectory(bare);

        Assert.Equal(OgReleaseRules.CodeNoPlan,
            service.ExportEvidenceAsync(bare, Path.Combine(scratch.Root, "bare.zip"), "sara").GetAwaiter().GetResult().Reference);
    }

    [Fact]
    public void The_page_shows_every_state_the_blockers_and_the_two_separate_commands()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // The checklist with its state and its evidence, and the blockers worked out from those states.
        Assert.Contains("_itemGrid.ItemsSource = evaluated.Value.Items", page, StringComparison.Ordinal);
        Assert.Contains("evaluated.Value.BlockerText", page, StringComparison.Ordinal);
        Assert.Contains("evaluated.Value.SummaryText", page, StringComparison.Ordinal);

        // The feature's two commands, each going through the service with the session's permissions.
        Assert.Contains(".AttestAsync(key, _evidence.Text", page, StringComparison.Ordinal);
        Assert.Contains(".WaiveAsync(key, _approver.Text", page, StringComparison.Ordinal);
        Assert.Contains("_application.Permissions", page, StringComparison.Ordinal);

        // The waiver's approver and expiry are asked for on the page rather than assumed.
        Assert.Contains("_expires.Text", page, StringComparison.Ordinal);
        Assert.Contains("_approver.Text", page, StringComparison.Ordinal);

        // The evidence pack is its own command, and its missing links are shown.
        Assert.Contains(".ExportEvidenceAsync(_planFolder.Text", page, StringComparison.Ordinal);
        Assert.Contains("exported.Value.MissingText", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "rel05-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Root => _root;

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

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
