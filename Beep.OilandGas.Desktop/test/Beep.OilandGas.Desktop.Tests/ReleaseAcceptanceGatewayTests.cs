using System.IO.Compression;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Release;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-05 end to end: a release is not ready until every item passes or is waived, the checklist's own numbers keep a failing check failed,
/// the attestations and waivers survive on disk, and the evidence pack holds the plan's evidence with a manifest.
/// </summary>
/// <remarks>
/// Opt-in because it writes a store and a zip. The smoke artefact is written by the test rather than taken from this machine, so what the
/// computed items read is stated in the test itself - once with the build's real accessibility numbers, once with a run that meets them.
/// </remarks>
public sealed class ReleaseAcceptanceGatewayTests
{
    [IntegrationFact]
    public async Task A_release_is_not_ready_until_every_item_passes_or_is_waived_and_the_pack_holds_the_evidence()
    {        var root = SourceRoot.Locate();
        var plan = Path.Combine(root, "plan");
        var sources = Path.Combine(root, "src");

        using var scratch = new Scratch();

        var store = Path.Combine(scratch.Root, "Diagnostics", "release-acceptance.json");
        var smoke = Path.Combine(scratch.Root, "smoke-result.txt");

        var reporter = new Recording();

        OgReleaseAcceptanceService Build() => new(reporter, () => store, () => smoke, () => sources, () => plan, () => "1.2.3");

        // ---- the build's own numbers: the checklist refuses to call it ready ----
        File.WriteAllText(smoke, string.Join(Environment.NewLine,
            "result: ok",
            "pages-declared: 124",
            "views-constructed: True",
            "performance-measured: 6",
            "performance-over-budget: 0",
            "accessibility-unnamed-count: 0",
            "accessibility-untabbable-count: 0",
            "skin-contrast-failures: 66"));

        var service = Build();
        var honest = await service.EvaluateAsync();

        Assert.True(honest.Success, honest.Message + " :: " + string.Join(" | ", reporter.Failures));

        var build = honest.Value!;

        Assert.False(build.Ready);
        Assert.Equal(30, build.Items.Count);

        // Four computed items pass on this checkout and this artefact; the accessibility item keeps its failure.
        Assert.Equal(4, build.Passed);
        Assert.Equal(1, build.Failed);
        Assert.Equal(25, build.NotRun);

        Assert.Equal(OgAcceptanceState.Fail, build.Items.Single(item => item.Key == "accessibility").State);
        Assert.Contains("66 skin contrast failure(s)",
            build.Blockers.Single(item => item.Key == "accessibility").Detail, StringComparison.Ordinal);

        Assert.Contains("NOT READY TO SHIP", build.SummaryText, StringComparison.Ordinal);

        // ---- a run that meets every item, and every attestation signed ----
        File.WriteAllText(smoke, string.Join(Environment.NewLine,
            "result: ok",
            "pages-declared: 124",
            "views-constructed: True",
            "performance-measured: 6",
            "performance-over-budget: 0",
            "accessibility-unnamed-count: 0",
            "accessibility-untabbable-count: 0",
            "skin-contrast-failures: 0"));

        var administrator = new OgPermissions(["SystemAdmin"], [], true);

        foreach (var item in build.Items.Where(item => item.Kind == OgAcceptanceKind.Attested))
        {
            var attested = await service.AttestAsync(item.Key, "the release run of 2026-11-09, log attached",
                "sara", administrator);

            Assert.True(attested.Success, item.Key + ": " + attested.Message);
        }

        var ready = await service.EvaluateAsync();

        Assert.True(ready.Success, ready.Message);
        Assert.True(ready.Value!.Ready, ready.Value.BlockerText);
        Assert.Equal(30, ready.Value.Passed);
        Assert.Empty(ready.Value.Blockers);
        Assert.Contains("READY TO SHIP", ready.Value.SummaryText, StringComparison.Ordinal);

        // ---- the attestations and the waivers are on disk, and a fresh service reads them ----
        Assert.True(File.Exists(store));

        var reopened = Build();
        var reread = await reopened.EvaluateAsync();

        Assert.True(reread.Value!.Ready, reread.Value.BlockerText);
        Assert.Equal(30, reread.Value.Passed);

        var waived = await reopened.WaiveAsync("localization", "nadia", "Arabic is in; two lower-priority cultures follow in 1.3",
            DateTime.UtcNow.AddDays(30), "sara", administrator);

        Assert.True(waived.Success, waived.Message);
        Assert.Equal(OgAcceptanceState.Waived, waived.Value!.State);

        var withWaiver = await reopened.EvaluateAsync();

        Assert.Equal(29, withWaiver.Value!.Passed);
        Assert.Equal(1, withWaiver.Value.Waived);
        Assert.True(withWaiver.Value.Ready);

        // The same person cannot approve their own waiver, and an expiry in the past is refused.
        Assert.Equal(OgReleaseRules.CodeWaiverSamePerson,
            (await reopened.WaiveAsync("localization", "sara", "because", DateTime.UtcNow.AddDays(7), "sara", administrator)).Reference);

        Assert.Equal(OgReleaseRules.CodeWaiverExpiry,
            (await reopened.WaiveAsync("localization", "nadia", "because", DateTime.UtcNow.AddDays(-1), "sara", administrator)).Reference);

        // ---- evidence that names a file: present passes, absent fails with the path ----
        var present = await reopened.AttestAsync("criteria-tested", "evidence/STG-18-REL-01/observed-evidence.json",
            "sara", administrator);

        Assert.True(present.Success, present.Message);

        var afterPresent = await reopened.EvaluateAsync();

        Assert.Equal(OgAcceptanceState.Pass,
            afterPresent.Value!.Items.Single(item => item.Key == "criteria-tested").State);

        await reopened.AttestAsync("criteria-tested", "evidence/STG-18-REL-05/not-written.json", "sara", administrator);

        var afterMissing = await reopened.EvaluateAsync();
        var broken = afterMissing.Value!.Items.Single(item => item.Key == "criteria-tested");

        Assert.Equal(OgAcceptanceState.Fail, broken.State);
        Assert.Contains("not-written.json", broken.Detail, StringComparison.Ordinal);
        Assert.False(afterMissing.Value.Ready);

        // ---- the evidence pack: the plan's evidence, with a manifest, and no missing links ----
        await reopened.AttestAsync("criteria-tested", "evidence/STG-18-REL-01/observed-evidence.json", "sara", administrator);

        var zip = Path.Combine(scratch.Root, "release-evidence.zip");
        var packed = await reopened.ExportEvidenceAsync(plan, zip, "sara");

        Assert.True(packed.Success, packed.Message + " :: " + string.Join(" | ", reporter.Failures));
        Assert.True(packed.Value!.Files >= 100,
            packed.Value.Files.ToString(System.Globalization.CultureInfo.InvariantCulture) + " evidence file(s)");
        Assert.Empty(packed.Value.Missing);

        using (var archive = ZipFile.OpenRead(zip))
        {
            Assert.NotNull(archive.GetEntry("manifest.json"));
            Assert.Contains(archive.Entries, entry =>
                entry.FullName.StartsWith("evidence/", StringComparison.Ordinal) &&
                entry.FullName.EndsWith("observed-evidence.json", StringComparison.Ordinal));
        }

        // ---- and the store survives a corrupt write without stopping the checklist ----
        File.WriteAllText(store, "{ not json");

        var afterCorrupt = await reopened.EvaluateAsync();

        Assert.True(afterCorrupt.Success, afterCorrupt.Message);
        Assert.Equal(30, afterCorrupt.Value!.Items.Count);
        Assert.Equal(5, afterCorrupt.Value.Passed);          // the computed ones still run
        Assert.True(afterCorrupt.Value.NotRun >= 25);
    }

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "rel05-it-" + Guid.NewGuid().ToString("n")[..8]);

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
