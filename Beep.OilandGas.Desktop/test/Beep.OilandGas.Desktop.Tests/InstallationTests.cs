using TheTechIdea.Beep.OilandGas.Desktop.Data.Release;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-01: the installation check, the uninstall decision, the upgrade record and the packaging scripts.
/// </summary>
/// <remarks>
/// The checks run against the assembles this test host loaded, so they are real: an embedded asset that the engine could not read would
/// fail here. The file-kind assets are checked against a folder that really has them (this build) and against an empty folder that does
/// not, because a check that can only ever pass is not a check.
/// </remarks>
public sealed class InstallationTests
{
    private const string PageFileName = "InstallationPageView.cs";

    private static OgInstallationService Service(string installDirectory, string dataRoot, string version = "1.2.3") =>
        new(
            () => new OgInstallProbe(installDirectory, dataRoot, version,
                Path.Combine(installDirectory, OgInstallRules.ExecutableName)),
            new Quiet());

    [Fact]
    public void Every_asset_states_its_source_and_what_stops_working_without_it()
    {
        using var scratch = new Scratch();

        var report = Service(AppContext.BaseDirectory, scratch.Root).CheckAsync().GetAwaiter().GetResult();

        Assert.True(report.Success, report.Message);

        var assets = report.Value!.Assets;

        // The checklist the feature asks for: every entry has a name, a source and a consequence - the page prints all three, so an
        // asset without a consequence would print an empty column rather than an answer.
        Assert.NotEmpty(assets);
        Assert.All(assets, asset =>
        {
            Assert.False(string.IsNullOrWhiteSpace(asset.Key));
            Assert.False(string.IsNullOrWhiteSpace(asset.Name));
            Assert.False(string.IsNullOrWhiteSpace(asset.Source));
            Assert.False(string.IsNullOrWhiteSpace(asset.Consequence));
            Assert.True(asset.Count >= 0);
        });

        Assert.Equal(assets.Count, assets.Select(asset => asset.Key).Distinct(StringComparer.Ordinal).Count());

        // AC-3's list, by the key the page and the tests share.
        foreach (var expected in new[]
                 {
                     "executable", "skia", "drivers", "countries", "symbols", "mapping-profiles", "lithology",
                     "lithology-standards", "geomagnetic", "environment-template", "environment-schema",
                     "environment-defaults", "pdf-renderer", "location",
                 })
        {
            Assert.Contains(assets, asset => asset.Key == expected);
        }

        // AC-2: the data root and the install folder are separate, and the report says so.
        Assert.True(report.Value.LocationSound);
        Assert.Contains("separate from the install folder", report.Value.LocationText, StringComparison.Ordinal);
    }

    [Fact]
    public void Every_embedded_asset_is_present_in_this_build()
    {
        using var scratch = new Scratch();

        var report = Service(AppContext.BaseDirectory, scratch.Root).CheckAsync().GetAwaiter().GetResult();

        Assert.True(report.Success, report.Message);

        // What is compiled or embedded cannot be missing from a build that runs: if any of these is absent the engine has been trimmed or
        // a resource has been lost, and the check is the thing that notices.
        var embedded = report.Value!.Assets.Where(asset => asset.Kind == OgAssetKind.Embedded && asset.Key != "location").ToList();

        Assert.NotEmpty(embedded);
        Assert.All(embedded, asset => Assert.True(asset.Present, asset.Key + ": " + asset.Describe()));

        // The sets carry their real size, read from the engine rather than a number in a document.
        var countries = report.Value.Assets.Single(asset => asset.Key == "countries");

        Assert.True(countries.Count > 0);

        var symbols = report.Value.Assets.Single(asset => asset.Key == "symbols");

        Assert.True(symbols.Count > 0, symbols.Describe());

        var profiles = report.Value.Assets.Single(asset => asset.Key == "mapping-profiles");

        Assert.True(profiles.Count > 0, profiles.Describe());

        var standards = report.Value.Assets.Single(asset => asset.Key == "lithology-standards");

        Assert.True(standards.Count >= 2, standards.Describe());
    }

    [Fact]
    public void A_folder_without_the_files_reports_them_missing_with_their_consequences()
    {
        // The guard that can fail: pointed at an empty folder, the check must say what is absent rather than passing.
        using var scratch = new Scratch();

        var empty = Path.Combine(scratch.Root, "empty-install");

        Directory.CreateDirectory(empty);

        var report = Service(empty, Path.Combine(scratch.Root, "data")).CheckAsync().GetAwaiter().GetResult();

        Assert.True(report.Success, report.Message);
        Assert.False(report.Value!.Healthy);

        var missing = report.Value.Missing;

        Assert.Contains(missing, asset => asset.Key == "executable");
        Assert.Contains(missing, asset => asset.Key == "skia");

        // The sentence names the asset and what it costs, so a person reading the page can act on it.
        Assert.Contains("executable", report.Value.MissingText, StringComparison.Ordinal);
        Assert.Contains("chart", report.Value.MissingText, StringComparison.Ordinal);

        // The optional renderer is reported as not installed, and is not a fault.
        var renderer = report.Value.Assets.Single(asset => asset.Key == "pdf-renderer");

        Assert.False(renderer.Present);
        Assert.Equal(OgAssetRequirement.Optional, renderer.Requirement);
        Assert.Contains(renderer, report.Value.OptionalAbsent);
        Assert.DoesNotContain(renderer, missing);
        Assert.Contains("permit", renderer.Consequence, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void A_data_root_inside_the_install_folder_is_an_unhealthy_location()
    {
        // AC-2's failure mode: a writable root under the install folder is removed by an uninstall along with the program.
        using var scratch = new Scratch();

        var install = Path.Combine(scratch.Root, "install");

        Directory.CreateDirectory(install);

        var inside = Path.Combine(install, "data");

        Assert.False(OgInstallRules.WritesOnlyToDataRoot(install, inside));
        Assert.False(OgInstallRules.WritesOnlyToDataRoot(install, install));
        Assert.True(OgInstallRules.WritesOnlyToDataRoot(install, Path.Combine(scratch.Root, "data")));
        Assert.False(OgInstallRules.WritesOnlyToDataRoot(" ", Path.Combine(scratch.Root, "data")));

        var report = Service(install, inside).CheckAsync().GetAwaiter().GetResult();

        Assert.True(report.Success, report.Message);
        Assert.False(report.Value!.LocationSound);

        var location = report.Value.Assets.Single(asset => asset.Key == "location");

        Assert.False(location.Present);
        Assert.Contains(location, report.Value.Missing);
        Assert.Contains("uninstall", location.Consequence, StringComparison.Ordinal);
    }

    [Fact]
    public void The_data_root_is_removed_only_by_the_exact_words_and_the_refusal_names_what_goes()
    {
        // AC-4: the failure store and the logs are named before anything is deleted, and the token is the path itself.
        using var scratch = new Scratch();

        var root = Path.Combine(scratch.Root, "data");
        var store = Path.Combine(root, "Diagnostics", "failures.db");

        Directory.CreateDirectory(Path.GetDirectoryName(store)!);
        Directory.CreateDirectory(Path.Combine(root, "Logs"));

        File.WriteAllText(store, "not a real database");
        File.WriteAllText(Path.Combine(root, "Logs", "app.log"), "a line");
        File.WriteAllText(Path.Combine(root, "setup.json"), "{}");

        var service = Service(AppContext.BaseDirectory, root);

        var inventory = service.InventoryAsync().GetAwaiter().GetResult();

        Assert.True(inventory.Success, inventory.Message);
        Assert.True(inventory.Value!.Exists);
        Assert.True(inventory.Value.Files >= 3, inventory.Value.Describe());
        Assert.Equal(1, inventory.Value.LogFiles);
        Assert.True(inventory.Value.FailureStoreFiles >= 1);
        Assert.Contains("failures.db", inventory.Value.FailureStore, StringComparison.Ordinal);

        // Every wrong answer is refused, and the refusal names the store and the logs.
        foreach (var wrong in new[] { string.Empty, "yes", "REMOVE", "REMOVE  C:\\other", root })
        {
            var refused = service.PurgeDataRootAsync(wrong).GetAwaiter().GetResult();

            Assert.False(refused.Success, "accepted: '" + wrong + "'");
            Assert.Equal(OgInstallRules.CodePurgeToken, refused.Reference);
            Assert.Contains("failures.db", refused.Message!, StringComparison.Ordinal);
            Assert.Contains("Logs", refused.Message!, StringComparison.Ordinal);
            Assert.Contains(OgInstallRules.ConfirmationToken(root), refused.Message!, StringComparison.Ordinal);
        }

        Assert.True(Directory.Exists(root));
        Assert.True(File.Exists(store));

        // The warning shown beside the inventory names the same things.
        var warning = OgInstallRules.PurgeWarning(inventory.Value);

        Assert.Contains("failure store", warning, StringComparison.Ordinal);
        Assert.Contains("logs", warning, StringComparison.Ordinal);
        Assert.Contains("install folder is not touched", warning, StringComparison.Ordinal);

        // The exact sentence removes the root and nothing beside it.
        var sibling = Path.Combine(scratch.Root, "keep-me");

        Directory.CreateDirectory(sibling);
        File.WriteAllText(Path.Combine(sibling, "important.txt"), "stay");

        var purged = service.PurgeDataRootAsync(OgInstallRules.ConfirmationToken(root)).GetAwaiter().GetResult();

        Assert.True(purged.Success, purged.Message);
        Assert.True(purged.Value!.Removed);
        Assert.False(Directory.Exists(root));
        Assert.True(File.Exists(Path.Combine(sibling, "important.txt")));
        Assert.True(purged.Value.Files >= 3);
    }

    [Fact]
    public void An_upgrade_preserves_the_data_root_and_the_setup_state_and_writes_nothing()
    {
        // AC-5: the plan reads the record and the data root, and changes neither.
        using var scratch = new Scratch();

        var root = Path.Combine(scratch.Root, "data");
        var setup = Path.Combine(root, "setup.json");

        Directory.CreateDirectory(root);
        File.WriteAllText(setup, """{"step":"handover"}""");

        var before = File.ReadAllBytes(setup);
        var stamp = File.GetLastWriteTimeUtc(setup);

        var fresh = Service(AppContext.BaseDirectory, root, "1.0.0");

        var plannedFresh = fresh.PlanUpgradeAsync().GetAwaiter().GetResult();

        Assert.True(plannedFresh.Success, plannedFresh.Message);
        Assert.Equal(OgUpgradePlan.Fresh, plannedFresh.Value!.Kind);
        Assert.Contains("fresh install", plannedFresh.Value.Describe(), StringComparison.Ordinal);
        Assert.Contains(plannedFresh.Value.Preserved, item => item.Contains("setup state", StringComparison.Ordinal));

        // Recording the version needs an actor, and is written under the data root.
        Assert.Equal(OgInstallRules.CodeActorRequired, fresh.RecordAsync(" ").GetAwaiter().GetResult().Reference);

        var recorded = fresh.RecordAsync("sara").GetAwaiter().GetResult();

        Assert.True(recorded.Success, recorded.Message);
        Assert.Equal("1.0.0", recorded.Value!.Version);
        Assert.True(File.Exists(Path.Combine(root, "Diagnostics", OgInstallRules.RecordFileName)));

        // The same version is not an upgrade; a different one is, and says which.
        var same = fresh.PlanUpgradeAsync().GetAwaiter().GetResult();

        Assert.Equal(OgUpgradePlan.Same, same.Value!.Kind);
        Assert.Contains("repair, not an upgrade", same.Value.Describe(), StringComparison.Ordinal);

        var newer = Service(AppContext.BaseDirectory, root, "1.1.0");

        var upgrade = newer.PlanUpgradeAsync().GetAwaiter().GetResult();

        Assert.Equal(OgUpgradePlan.Upgrade, upgrade.Value!.Kind);
        Assert.Equal("1.0.0", upgrade.Value.PreviousVersion);
        Assert.Equal("1.1.0", upgrade.Value.CurrentVersion);
        Assert.True(upgrade.Value.IsUpgrade);

        // Nothing in the data root changed, and the setup state is byte-identical - an upgrade keeps it.
        Assert.Equal(before, File.ReadAllBytes(setup));
        Assert.Equal(stamp, File.GetLastWriteTimeUtc(setup));

        // A record that cannot be read is reported and treated as fresh, not as a crash.
        File.WriteAllText(Path.Combine(root, "Diagnostics", OgInstallRules.RecordFileName), "{ not json");

        var afterCorrupt = newer.PlanUpgradeAsync().GetAwaiter().GetResult();

        Assert.False(afterCorrupt.Success);
        Assert.False(string.IsNullOrWhiteSpace(afterCorrupt.Reference));
        Assert.Contains("treated as fresh", afterCorrupt.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Verifying_the_installation_writes_its_report_beside_the_failure_store()
    {
        using var scratch = new Scratch();

        var service = Service(AppContext.BaseDirectory, scratch.Root);

        var report = await service.WriteReportAsync(" ");

        Assert.Equal(OgInstallRules.CodeActorRequired, report.Reference);

        var written = await service.WriteReportAsync("sara");

        Assert.True(written.Success, written.Message);

        var path = written.Value!;
        var expected = Path.Combine(Path.GetFullPath(scratch.Root), "Diagnostics", OgInstallRules.ReportFileName);

        Assert.Equal(expected, path);
        Assert.True(File.Exists(path));

        // The report is JSON with the assets and their verdicts, so a support call can read it without the application.
        using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(path));

        Assert.True(document.RootElement.GetProperty("assets").GetArrayLength() > 10);
        Assert.Equal(Path.GetFullPath(scratch.Root), document.RootElement.GetProperty("dataRoot").GetString());

        // It is under the data root, never beside the executable.
        Assert.False(OgInstallRules.IsUnder(path, AppContext.BaseDirectory));
    }

    [Fact]
    public void The_installer_scripts_are_per_user_and_the_uninstall_keeps_the_data_root_by_default()
    {
        // AC-2, AC-4 and AC-6 as the scripts actually read - the packaging is code too, and a guard that reads it is the only check that
        // runs on a machine that never installs.
        var root = SourceRoot.Locate();
        var install = File.ReadAllText(Path.Combine(root, "installer", "install.ps1"));
        var uninstall = File.ReadAllText(Path.Combine(root, "installer", "uninstall.ps1"));
        var readme = File.ReadAllText(Path.Combine(root, "installer", "README.md"));
        var manifest = File.ReadAllText(Path.Combine(root, "src", "Beep.OilandGas.Desktop", "app.manifest"));

        // AC-6: no elevation, and both folders are the user's own.
        Assert.Contains("asInvoker", manifest, StringComparison.Ordinal);
        Assert.DoesNotContain("requireAdministrator", manifest, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("LOCALAPPDATA", install, StringComparison.Ordinal);

        // No path is taken from ProgramData or from the machine-wide application-data folder; the words appear once, in the sentence
        // that says nothing is written to them.
        Assert.DoesNotContain("$env:ProgramData", install, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CommonApplicationData", install, StringComparison.Ordinal);
        Assert.DoesNotContain("Program Files (x86)", install, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("RunAs", install, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nothing was written to Program Files or ProgramData", install, StringComparison.Ordinal);

        // The one file the installer writes outside the install folder and the data root is the Start-menu shortcut, and the script says
        // so and can be told not to. (AC-2 is about normal use; this is the installer, and the uninstall removes it.)
        Assert.Contains("$env:APPDATA", install, StringComparison.Ordinal);
        Assert.Contains("NoShortcut", install, StringComparison.Ordinal);

        // AC-2: the install refuses a payload without the native renderer, because the health page would then report an unhealthy install.
        Assert.Contains("libSkiaSharp.dll", install, StringComparison.Ordinal);
        Assert.Contains("Program Files", install, StringComparison.Ordinal);   // named only as something that is NOT written to
        Assert.Contains("Nothing was written to Program Files", install, StringComparison.Ordinal);

        // AC-5: an install over an existing one is named as an upgrade and records the version.
        Assert.Contains("upgrading", install, StringComparison.Ordinal);
        Assert.Contains("installation.json", install, StringComparison.Ordinal);

        // AC-4: the uninstall keeps the data root unless the switch and the exact sentence are both given.
        Assert.Contains("RemoveDataRoot", uninstall, StringComparison.Ordinal);
        Assert.Contains("kept the data root", uninstall, StringComparison.Ordinal);
        Assert.Contains("failures.db", uninstall, StringComparison.Ordinal);
        Assert.Contains("Logs", uninstall, StringComparison.Ordinal);
        Assert.Contains("$Confirmation -ne $expected", uninstall, StringComparison.Ordinal);
        Assert.DoesNotContain("Remove-Item -LiteralPath $dataRoot -Recurse -Force", uninstall.Split("if ($RemoveDataRoot)")[0],
            StringComparison.Ordinal);

        // The README says which criterion is the owner's step rather than this build's, so nothing is claimed that was not run.
        Assert.Contains("clean VM", readme, StringComparison.Ordinal);
        Assert.Contains("owner's step", readme, StringComparison.Ordinal);
        Assert.Contains("Code signing", readme, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_shows_the_assets_the_location_the_inventory_and_the_upgrade()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-3: the asset grid, with the consequence beside each row.
        Assert.Contains("_assetGrid.ItemsSource = checkedNow.Value.Assets", page, StringComparison.Ordinal);
        Assert.Contains("checkedNow.Value.MissingText", page, StringComparison.Ordinal);

        // AC-2: the two folders are printed together.
        Assert.Contains("checkedNow.Value.LocationText", page, StringComparison.Ordinal);

        // AC-4: the inventory and the exact words, before the button that removes anything.
        Assert.Contains("OgInstallRules.PurgeWarning(inventory.Value)", page, StringComparison.Ordinal);
        Assert.Contains("OgInstallRules.ConfirmationToken(inventory.Value.Root)", page, StringComparison.Ordinal);
        Assert.Contains("_installation.PurgeDataRootAsync(_confirmation.Text", page, StringComparison.Ordinal);

        // AC-5: the plan is shown, and recording the version is its own command.
        Assert.Contains("planned.Value.PreservedText", page, StringComparison.Ordinal);
        Assert.Contains("_installation.RecordAsync", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    /// <summary>A temporary folder tree, deleted when the test ends.</summary>
    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "rel01-" + Guid.NewGuid().ToString("n")[..8]);

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
