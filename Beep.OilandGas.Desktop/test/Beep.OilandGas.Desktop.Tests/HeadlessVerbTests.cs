using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Headless;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdea.Beep.Utilities;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// FND-01 AC-2: every headless verb returns its documented exit code, writes the report an operator can
/// read, and never leaks exception text. The BeepDM-backed drivers report is separated into a pure writer
/// so its shape is testable without an eight-second runtime load.
/// </summary>
public sealed class HeadlessVerbTests
{
    [Fact]
    public void The_entitlement_report_lists_both_gates_for_every_module_and_page()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(application, new OgCommandLine(OgHeadlessVerb.EntitlementReport), output);

        var text = output.ToString();
        Assert.Equal(OgExitCode.Ok, exit);
        Assert.Contains("entitlement-report", text, StringComparison.Ordinal);
        Assert.Contains($"modules: {Core.OgModuleKeys.All.Count}", text, StringComparison.Ordinal);
        Assert.Contains($"pages: {Core.OgModuleCatalog.Pages.Count}", text, StringComparison.Ordinal);
        Assert.Contains("module: wells = allowed", text, StringComparison.Ordinal);
        Assert.Contains("page: dashboard | module=core", text, StringComparison.Ordinal);
        Assert.Contains("entitlement=allowed", text, StringComparison.Ordinal);

        foreach (var page in Core.OgModuleCatalog.Pages)
        {
            Assert.Contains($"page: {page.Route} |", text, StringComparison.Ordinal);
        }

        Assert.EndsWith("result: ok" + Environment.NewLine, text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_missing_licence_file_is_refused_with_exit_two()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.LicenceCheck, Path.Combine(OgPaths.DataRoot, "no-such.json")),
            output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("was not found", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("result: refused", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_licence_verb_without_a_path_is_refused_and_does_not_guess()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(application, new OgCommandLine(OgHeadlessVerb.LicenceCheck), output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("needs a licence file path", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_signed_licence_passes_and_the_verb_names_the_key_that_verified_it()
    {
        using var application = new OgBootstrapper().Create();
        var file = TestLicences.Write(TestLicences.Signed(
            TestLicences.Claim("Probe Energy", "2999-01-01T00:00:00Z", modules: ["core", "wells", "drilling"])));

        var output = new StringWriter();
        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.LicenceCheck, file),
            output);

        var text = output.ToString();
        Assert.Equal(OgExitCode.Ok, exit);
        Assert.Contains("edition: Professional", text, StringComparison.Ordinal);
        Assert.Contains("customer: Probe Energy", text, StringComparison.Ordinal);
        Assert.Contains("modules: core,wells,drilling", text, StringComparison.Ordinal);
        Assert.Contains($"signature: verified (key {TestLicences.KeyId})", text, StringComparison.Ordinal);
        Assert.Contains($"result: verified-ok (key {TestLicences.KeyId})", text, StringComparison.Ordinal);
    }

    [Fact]
    public void An_unsigned_licence_is_refused_because_nobody_vouched_for_it()
    {
        using var application = new OgBootstrapper().Create();
        var file = TestLicences.Write(
            """
            { "edition": "Professional", "customer": "Probe Energy", "modules": ["wells"] }
            """);

        var output = new StringWriter();
        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.LicenceCheck, file),
            output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("signature: not verified - licence-unsigned", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void An_expired_licence_is_refused_with_the_date()
    {
        using var application = new OgBootstrapper().Create();
        var file = TestLicences.Write(TestLicences.Signed(
            TestLicences.Claim("Probe Energy", "2001-01-01T00:00:00Z", graceDays: 0, modules: ["core", "wells"])));

        var output = new StringWriter();
        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.LicenceCheck, file),
            output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("expired on 2001-01-01", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void An_unknown_module_key_in_a_licence_is_refused()
    {
        using var application = new OgBootstrapper().Create();
        var file = TestLicences.Write(TestLicences.Signed(
            TestLicences.Claim("Probe Energy", "2999-01-01T00:00:00Z", modules: ["core", "wells", "wellz"])));

        var output = new StringWriter();
        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.LicenceCheck, file),
            output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("'wellz' is not a known module key", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void A_licence_that_is_not_json_is_refused_with_a_reference_and_no_exception_text()
    {
        using var application = new OgBootstrapper().Create();
        var failuresBefore = application.Failures.Count();
        var file = WriteLicence("{ this is not json ");

        var output = new StringWriter();
        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.LicenceCheck, file),
            output);

        var text = output.ToString();
        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("result: refused", text, StringComparison.Ordinal);
        Assert.Contains("reference:", text, StringComparison.Ordinal);

        // The parser's own words belong in the store, not on the console (BEEP0004).
        Assert.DoesNotContain("JsonReaderException", text, StringComparison.Ordinal);
        Assert.DoesNotContain("LineNumber", text, StringComparison.Ordinal);

        // And it was recorded, so the failure is not lost.
        Assert.True(application.Failures.Count() > failuresBefore);
    }

    [Fact]
    public void Restore_is_refused_with_exit_two_because_no_backup_service_exists()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.Restore, Path.Combine(OgPaths.DataRoot, "backup.bak")),
            output);

        var text = output.ToString();
        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("not-configured", text, StringComparison.Ordinal);
        Assert.Contains("REL-01", text, StringComparison.Ordinal);
    }

    [Fact]
    public void An_unknown_verb_prints_the_usage_and_exits_two()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(application, new OgCommandLine(OgHeadlessVerb.Unknown), output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("Usage:", output.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(OgHeadlessVerb.SchemaApply)]
    [InlineData(OgHeadlessVerb.SchemaResume)]
    public void The_schema_verbs_say_which_feature_owns_them(OgHeadlessVerb verb)
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(application, new OgCommandLine(verb), output);

        Assert.Equal(OgExitCode.Refused, exit);
        Assert.Contains("DAT-02", output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_benchmark_verb_measures_the_budgets_and_writes_the_report()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.ReportBenchmark),
            output);

        var text = output.ToString();

        Assert.Equal(OgExitCode.Ok, exit);
        Assert.Contains("report-benchmark", text, StringComparison.Ordinal);
        Assert.Contains("machine:", text, StringComparison.Ordinal);
        Assert.Contains("measured:", text, StringComparison.Ordinal);
        Assert.Contains("report:", text, StringComparison.Ordinal);
        Assert.True(File.Exists(OgPaths.BenchmarkReportPath));

        // The report is machine-readable and names the budgets it could not measure, with the reason.
        var report = OgBenchmarkReport.FromJson(File.ReadAllText(OgPaths.BenchmarkReportPath));
        Assert.Equal(OgBudgets.All.Count, report.Measurements.Count);
        Assert.True(report.MeasuredCount > 0);
        Assert.Contains(
            report.Measurements,
            row => row.Key == "well-search" && !row.Measured && row.Note!.Contains("DAT-02", StringComparison.Ordinal));
    }

    [Fact]
    public void The_seed_verb_writes_the_large_synthetic_log()
    {
        using var application = new OgBootstrapper().Create();
        var output = new StringWriter();

        var exit = OgHeadlessRunner.Run(
            application,
            new OgCommandLine(OgHeadlessVerb.SeedBenchmark),
            output);

        var text = output.ToString();

        Assert.Equal(OgExitCode.Ok, exit);
        Assert.Contains("seed-benchmark", text, StringComparison.Ordinal);
        Assert.Contains("file:", text, StringComparison.Ordinal);
        Assert.Contains(Path.Combine("Benchmark", "log-100000.csv"), text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_drivers_report_lists_each_driver_and_names_the_local_one()
    {
        var drivers = new List<ConnectionDriversConfig>
        {
            new()
            {
                DatasourceType = DataSourceType.SqlServer,
                DatasourceCategory = DatasourceCategory.RDBMS,
                PackageName = "System.Data.SqlClient",
                classHandler = "SQLServerDataSource",
                CreateLocal = false,
            },
            new()
            {
                DatasourceType = DataSourceType.SqlLite,
                DatasourceCategory = DatasourceCategory.RDBMS,
                PackageName = "System.Data.SQLite",
                classHandler = "SQLiteDataSource",
                CreateLocal = true,
            },
        };

        var output = new StringWriter();
        OgHeadlessRunner.WriteDriversReport(drivers, output);
        var text = output.ToString();

        Assert.Contains("drivers-known: 2", text, StringComparison.Ordinal);
        Assert.Contains(
            "driver: SqlLite | package=System.Data.SQLite | category=RDBMS | local=True | in-memory=False | class=SQLiteDataSource",
            text,
            StringComparison.Ordinal);
        Assert.Contains("local-driver: SqlLite", text, StringComparison.Ordinal);
        Assert.EndsWith("result: ok" + Environment.NewLine, text, StringComparison.Ordinal);
    }

    [Fact]
    public void An_empty_driver_list_is_reported_honestly_rather_than_as_success_with_a_driver()
    {
        var output = new StringWriter();

        OgHeadlessRunner.WriteDriversReport([], output);

        var text = output.ToString();
        Assert.Contains("drivers-known: 0", text, StringComparison.Ordinal);
        Assert.Contains("local-driver: (none)", text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_bundled_driver_set_includes_the_local_database_provider()
    {
        Assert.NotEmpty(OgBundledDrivers.AssemblyAnchors);
        Assert.Contains(
            OgBundledDrivers.AssemblyAnchors,
            anchor => anchor.Name.Contains("SQLiteDataSource", StringComparison.Ordinal));
    }

    [Fact]
    public void A_failed_smoke_run_still_writes_its_result_file()
    {
        var path = Path.Combine(OgPaths.DataRoot, "smoke-fail-" + Guid.NewGuid().ToString("n")[..8], "smoke-result.txt");
        var console = new StringWriter();

        OgHeadlessFailureReport.Write(new OgCommandLine(OgHeadlessVerb.Smoke), "abcdef123456", console, path);

        Assert.True(File.Exists(path), "A failed smoke run left no result file.");
        var text = File.ReadAllText(path);
        Assert.Contains("result: failed", text, StringComparison.Ordinal);
        Assert.Contains("reference: abcdef123456", text, StringComparison.Ordinal);
        Assert.Contains("command: Smoke", text, StringComparison.Ordinal);

        // The console got the same report, so a scheduled run is readable in both places.
        Assert.Equal(text, console.ToString());
    }

    [Fact]
    public void A_failed_other_verb_does_not_write_the_smoke_result_file()
    {
        var path = Path.Combine(OgPaths.DataRoot, "smoke-fail-" + Guid.NewGuid().ToString("n")[..8], "smoke-result.txt");
        var console = new StringWriter();

        OgHeadlessFailureReport.Write(new OgCommandLine(OgHeadlessVerb.DriversReport), "abcdef123456", console, path);

        Assert.False(File.Exists(path));
        Assert.Contains("result: failed", console.ToString(), StringComparison.Ordinal);
    }

    private static string WriteLicence(string json)
    {
        var directory = Path.Combine(OgPaths.DataRoot, "licence-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);

        var file = Path.Combine(directory, "licence.json");
        File.WriteAllText(file, json);
        return file;
    }
}
