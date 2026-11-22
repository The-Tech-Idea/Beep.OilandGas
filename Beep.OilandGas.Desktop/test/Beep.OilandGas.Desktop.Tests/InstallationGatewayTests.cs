using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Release;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-01 end to end over a data root of its own: the check, its report, the uninstall decision and the upgrade record, plus the assertion
/// that every path the application resolves really is under the per-user data root (AC-2).
/// </summary>
/// <remarks>
/// Opt-in because it writes and then removes a folder tree. The application's own <see cref="OgPaths"/> is asserted as it resolves on this
/// machine - that is the AC-2 claim, and it is the one thing here that cannot be simulated.
/// </remarks>
public sealed class InstallationGatewayTests
{
    [IntegrationFact]
    public async Task The_installation_is_verified_manifested_upgraded_and_removed_by_the_exact_words()
    {        using var scratch = new Scratch();

        var reporter = new Recording();
        var root = Path.Combine(scratch.Root, "data");
        var install = AppContext.BaseDirectory;

        // ---- a data root with the things a used installation holds ----
        Directory.CreateDirectory(Path.Combine(root, "Diagnostics"));
        Directory.CreateDirectory(Path.Combine(root, "Logs"));
        Directory.CreateDirectory(Path.Combine(root, "Data", "Local"));
        Directory.CreateDirectory(Path.Combine(root, "Wellbore"));
        Directory.CreateDirectory(Path.Combine(root, "Exports"));

        File.WriteAllText(Path.Combine(root, "Diagnostics", "failures.db"), "failure store");
        File.WriteAllText(Path.Combine(root, "Logs", "app-20261109.log"), "a log line");
        File.WriteAllText(Path.Combine(root, "Data", "Local", "BeepOilGas.db"), "a database");
        File.WriteAllText(Path.Combine(root, "Wellbore", "environment.json"), """{ "units": { "depthUnit": "m" } }""");
        File.WriteAllText(Path.Combine(root, "Exports", "report.pdf"), "%PDF");
        File.WriteAllText(Path.Combine(root, "setup.json"), """{ "step": "handover", "complete": true }""");

        var setupBefore = File.ReadAllBytes(Path.Combine(root, "setup.json"));

        var service = new OgInstallationService(
            () => new OgInstallProbe(install, root, "2.0.0", Path.Combine(install, OgInstallRules.ExecutableName)),
            reporter);

        // ---- AC-2 and AC-3: the check ----
        var checkedNow = await service.CheckAsync();

        Assert.True(checkedNow.Success, checkedNow.Message + " :: " + string.Join(" | ", reporter.Failures));

        var report = checkedNow.Value!;

        Assert.True(report.LocationSound, report.LocationText);
        Assert.True(report.Healthy, report.MissingText);
        Assert.All(report.Assets.Where(asset => asset.Kind == OgAssetKind.Embedded && asset.Key != "location"),
            asset => Assert.True(asset.Present, asset.Key + " :: " + asset.Describe()));

        // The sets report the engine's own sizes, not numbers copied into a document.
        Assert.True(report.Assets.Single(asset => asset.Key == "countries").Count > 0);

        // ---- the report goes under the data root ----
        var written = await service.WriteReportAsync("sara");

        Assert.True(written.Success, written.Message);
        Assert.True(OgInstallRules.IsUnder(written.Value!, root));
        Assert.False(OgInstallRules.IsUnder(written.Value!, install));

        using (var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(written.Value!)))
        {
            Assert.True(document.RootElement.GetProperty("healthy").GetBoolean());
            Assert.Equal(Path.GetFullPath(root), document.RootElement.GetProperty("dataRoot").GetString());
        }

        // ---- AC-4: the inventory names the store and the logs, and removal needs the exact words ----
        var inventory = await service.InventoryAsync();

        Assert.True(inventory.Success, inventory.Message);
        Assert.True(inventory.Value!.Exists);
        Assert.True(inventory.Value.Files >= 6, inventory.Value.Describe());
        Assert.Equal(1, inventory.Value.LogFiles);
        Assert.Contains("Logs", inventory.Value.LogDirectory, StringComparison.Ordinal);
        Assert.Contains("failures.db", inventory.Value.FailureStore, StringComparison.Ordinal);
        Assert.Contains("Data", inventory.Value.Directories);

        var wrong = await service.PurgeDataRootAsync("remove " + root);

        Assert.False(wrong.Success);
        Assert.Equal(OgInstallRules.CodePurgeToken, wrong.Reference);
        Assert.Contains("failures.db", wrong.Message!, StringComparison.Ordinal);
        Assert.Contains("Logs", wrong.Message!, StringComparison.Ordinal);
        Assert.True(Directory.Exists(root));

        // ---- AC-5: the upgrade record, and what it preserves ----
        var fresh = await service.PlanUpgradeAsync();

        Assert.True(fresh.Success, fresh.Message);
        Assert.Equal(OgUpgradePlan.Fresh, fresh.Value!.Kind);
        Assert.Contains(fresh.Value.Preserved, item => item.Contains("setup state", StringComparison.Ordinal));

        var recorded = await service.RecordAsync("sara");

        Assert.True(recorded.Success, recorded.Message);
        Assert.Equal("2.0.0", recorded.Value!.Version);

        var again = await service.PlanUpgradeAsync();

        Assert.Equal(OgUpgradePlan.Same, again.Value!.Kind);

        var newer = new OgInstallationService(
            () => new OgInstallProbe(install, root, "2.1.0", Path.Combine(install, OgInstallRules.ExecutableName)),
            reporter);

        var upgrade = await newer.PlanUpgradeAsync();

        Assert.Equal(OgUpgradePlan.Upgrade, upgrade.Value!.Kind);
        Assert.Equal("2.0.0", upgrade.Value.PreviousVersion);
        Assert.Equal(setupBefore, File.ReadAllBytes(Path.Combine(root, "setup.json")));

        // ---- AC-4: the exact words remove the root, and nothing beside it ----
        var neighbour = Path.Combine(scratch.Root, "neighbour");

        Directory.CreateDirectory(neighbour);
        File.WriteAllText(Path.Combine(neighbour, "keep.txt"), "keep");

        var purged = await service.PurgeDataRootAsync(OgInstallRules.ConfirmationToken(root));

        Assert.True(purged.Success, purged.Message);
        Assert.False(Directory.Exists(root));
        Assert.True(File.Exists(Path.Combine(neighbour, "keep.txt")));

        // ---- AC-2 as the application itself resolves it: every writable path is under the data root ----
        Assert.All(OgPaths.WritableDirectories, directory => Assert.True(
            OgPaths.IsWritableLocation(directory), directory + " is not under " + OgPaths.DataRoot));

        foreach (var path in new[]
                 {
                     OgPaths.FailureStorePath, OgPaths.LogDirectory, OgPaths.WellboreEnvironmentPath,
                     OgPaths.LocalDatabasePath, OgPaths.ExportsDirectory,
                     OgPaths.SkinPath, OgPaths.SmokeResultPath, OgPaths.BenchmarkReportPath,
                     OgPaths.SoakReportPath, OgPaths.ModuleSetupStatePath, OgPaths.FailureOutboxDirectory,
                     OgPaths.LicencePath, OgPaths.LicenceSettingsPath,
                 })
        {
            Assert.True(OgPaths.IsWritableLocation(path), path + " is not under " + OgPaths.DataRoot);
            Assert.False(OgInstallRules.IsUnder(path, install), path + " is inside the install folder");
        }

        // And the two folders really are separate on this machine.
        Assert.True(OgInstallRules.WritesOnlyToDataRoot(install, OgPaths.DataRoot));
    }

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "rel01-it-" + Guid.NewGuid().ToString("n")[..8]);

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
