using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Headless;

/// <summary>
/// Runs one headless verb against a composed application and returns its exit code.
/// </summary>
/// <remarks>
/// <para>
/// Every verb writes to the supplied <see cref="TextWriter"/> rather than the console, so the verb's
/// output and exit code are testable without a process. <see cref="App"/> passes <c>Console.Out</c>.
/// </para>
/// <para>
/// An unexpected exception is reported through <see cref="IFailureReporter"/> and returns
/// <see cref="OgExitCode.Failed"/>; a stated refusal (no backup service, an unavailable verb) returns
/// <see cref="OgExitCode.Refused"/> and is not dressed up as a failure.
/// </para>
/// </remarks>
public static class OgHeadlessRunner
{
    public static int Run(OgApplication application, OgCommandLine command, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(output);

        try
        {
            return command.Verb switch
            {
                OgHeadlessVerb.Smoke => Smoke(application, output),
                OgHeadlessVerb.DriversReport => Drivers(application, output),
                OgHeadlessVerb.EntitlementReport => Entitlements(application, output),
                OgHeadlessVerb.LicenceCheck => Licence(application, command, output),
                OgHeadlessVerb.Restore => Restore(application, command, output),
                OgHeadlessVerb.SchemaApply => NotAvailable(application, output, "creating the schema", "DAT-02"),
                OgHeadlessVerb.SchemaResume => NotAvailable(application, output, "resuming a schema run", "DAT-02"),
                OgHeadlessVerb.SeedBenchmark => SeedBenchmark(application, output),
                OgHeadlessVerb.ReportBenchmark => Benchmark(application, output),
                OgHeadlessVerb.Soak => Soak(application, command, output),
                OgHeadlessVerb.Unknown => Unknown(output),
                OgHeadlessVerb.None => Usage(output),
            };
        }
        catch (Exception ex)
        {
            var reference = application.Diagnostics.ReportHandled(
                ex,
                $"running the '{command.Describe()}' command",
                "the command did not produce its output",
                FailureSeverity.Error);

            output.WriteLine($"result: failed");
            output.WriteLine($"reference: {reference}");
            return OgExitCode.Failed;
        }
    }

    private static int Smoke(OgApplication application, TextWriter output)
    {
        var report = SmokeRunner.Run(application);

        Directory.CreateDirectory(OgPaths.DataRoot);
        File.WriteAllText(OgPaths.SmokeResultPath, report.Report);
        output.Write(report.Report);

        return report.Ok ? OgExitCode.Ok : OgExitCode.Failed;
    }

    private static int Drivers(OgApplication application, TextWriter output)
    {
        // BeepDM writes its informational log to the console while it loads. A headless verb's stdout has
        // to be readable, so the runtime's chatter is sent to the rolling log only (BEEP_LOG_DIR) for the
        // duration of the load; our own report is written afterwards through the original writer.
        var console = Console.Out;
        Console.SetOut(TextWriter.Null);

        try
        {
            var host = application.Services.GetRequiredService<OgBeepLifetime>().Host;
            var drivers = host.Beep.Config_editor?.DataDriversClasses ?? [];

            WriteDriversReport(drivers, output);
            return OgExitCode.Ok;
        }
        finally
        {
            Console.SetOut(console);
        }
    }

    /// <summary>
    /// Writes the drivers report. Separate from the runtime so its shape is testable without loading
    /// BeepDM for eight seconds.
    /// </summary>
    public static void WriteDriversReport(
        IReadOnlyList<ConnectionDriversConfig> drivers,
        TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(drivers);
        ArgumentNullException.ThrowIfNull(output);

        var directory = Path.Combine(AppContext.BaseDirectory, "ConnectionDrivers");

        output.WriteLine("drivers-report");
        output.WriteLine($"driver-directory: {directory}");
        output.WriteLine($"driver-directory-exists: {Directory.Exists(directory)}");
        output.WriteLine($"bundled-providers: {OgBundledDrivers.AssemblyAnchors.Count}");
        output.WriteLine($"drivers-known: {drivers.Count}");

        foreach (var driver in drivers
                     .OrderBy(item => item.DatasourceType.ToString(), StringComparer.OrdinalIgnoreCase)
                     .ThenBy(item => item.PackageName, StringComparer.OrdinalIgnoreCase))
        {
            output.WriteLine(
                $"driver: {driver.DatasourceType} | package={driver.PackageName} | " +
                $"category={driver.DatasourceCategory} | local={driver.CreateLocal} | " +
                $"in-memory={driver.InMemory} | class={driver.classHandler}");
        }

        var local = drivers.FirstOrDefault(driver => driver.CreateLocal)
                    ?? drivers.FirstOrDefault(driver => !driver.InMemory);

        output.WriteLine($"local-driver: {local?.DatasourceType.ToString() ?? "(none)"}");
        output.WriteLine("result: ok");
    }

    private static int Entitlements(OgApplication application, TextWriter output)
    {
        var entitlements = application.Services.GetRequiredService<IOgEntitlementService>();

        WriteEntitlementReport(entitlements, application.Permissions, application.Navigation, output);
        return OgExitCode.Ok;
    }

    /// <summary>Writes the entitlement report: both gates, module by module and page by page.</summary>
    public static void WriteEntitlementReport(
        IOgEntitlementService entitlements,
        OgPermissions permissions,
        OgNavigationService navigation,
        TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(entitlements);
        ArgumentNullException.ThrowIfNull(permissions);
        ArgumentNullException.ThrowIfNull(navigation);
        ArgumentNullException.ThrowIfNull(output);

        var licence = entitlements.Current;

        output.WriteLine("entitlement-report");
        output.WriteLine($"entitlement-service: {entitlements.GetType().Name}");
        output.WriteLine($"licence-edition: {licence.Edition}");
        output.WriteLine($"licence-customer: {licence.Customer}");
        output.WriteLine($"licence-modules: {(licence.Modules.Count == 0 ? "(none)" : string.Join(',', licence.Modules))}");
        output.WriteLine($"licence-expires: {(licence.ExpiresAt is { } at ? at.ToString("yyyy-MM-dd") : "(none)")}");
        output.WriteLine($"licence-signature-verified: {licence.SignatureVerified}");
        output.WriteLine($"licence-key: {licence.KeyId ?? "(none)"}");
        output.WriteLine($"licence-enforcement: {licence.Enforcement}");
        output.WriteLine($"licence-usable: {licence.IsUsable}");
        output.WriteLine($"licence-status: {licence.StatusText}");
        output.WriteLine($"roles: {string.Join(',', permissions.Roles)}");
        output.WriteLine($"permission-source-available: {permissions.PermissionSourceAvailable}");
        output.WriteLine($"modules: {OgModuleKeys.All.Count}");

        foreach (var module in OgModuleKeys.All)
        {
            output.WriteLine(
                $"module: {module} = {(entitlements.Allows(module) ? "allowed" : "denied")}" +
                $" | write={(entitlements.AllowsWriting(module) ? "allowed" : "blocked")}");
        }

        output.WriteLine($"pages: {OgModuleCatalog.Pages.Count}");

        foreach (var page in OgModuleCatalog.Pages.OrderBy(item => item.Route, StringComparer.OrdinalIgnoreCase))
        {
            var entitlementAllows = !page.RequiresEntitlement || entitlements.Allows(page.ModuleKey);
            var roleAllows = permissions.IsSystemAdmin
                             || page.RequiredRoles.Count == 0
                             || page.RequiredRoles.Any(permissions.HasRole);

            output.WriteLine(
                $"page: {page.Route} | module={page.ModuleKey} | " +
                $"entitlement={(entitlementAllows ? "allowed" : "denied")} | " +
                $"roles={(page.RequiredRoles.Count == 0 ? "(none)" : string.Join('|', page.RequiredRoles))} | " +
                $"role={(roleAllows ? "allowed" : "denied")} | " +
                $"open={navigation.CanAccess(page.Route, permissions)}");
        }

        output.WriteLine($"result: ok");
    }

    private static int Licence(OgApplication application, OgCommandLine command, TextWriter output)
    {
        output.WriteLine("licence-check");
        output.WriteLine($"file: {command.Argument ?? "(none)"}");
        output.WriteLine($"signature-policy: {OgLicenceFile.SignaturePolicy}");

        if (command.IsMissingArgument)
        {
            output.WriteLine("reason: the verb needs a licence file path.");
            output.WriteLine("result: refused");
            return OgExitCode.Refused;
        }

        OgLicenceCheckResult result;

        try
        {
            result = OgLicenceFile.Read(command.Argument);
        }
        catch (Exception unreadable) when (unreadable is IOException or UnauthorizedAccessException or JsonException)
        {
            // A file that cannot be read or parsed is a stated refusal (exit 2), not an unexpected failure —
            // but it is still reported, and only the reference reaches the operator. The exception's own
            // text stays in the store (BEEP0004).
            var reference = application.Diagnostics.ReportHandled(
                unreadable,
                $"reading the licence file '{Path.GetFileName(command.Argument ?? string.Empty)}'",
                "the licence was not checked",
                FailureSeverity.Error);

            output.WriteLine("reason: the licence file could not be read.");
            output.WriteLine($"reference: {reference}");
            output.WriteLine("result: refused");
            return OgExitCode.Refused;
        }
        output.WriteLine($"signature: {(result.SignatureVerified ? $"verified (key {result.KeyId})" : $"not verified - {result.Code}")}");
        output.WriteLine($"edition: {result.Edition ?? "(none)"}");
        output.WriteLine($"customer: {result.Customer ?? "(none)"}");
        output.WriteLine($"modules: {(result.Modules.Count == 0 ? "(none)" : string.Join(',', result.Modules))}");
        output.WriteLine($"expires: {(result.ExpiresAt is { } at ? at.ToString("yyyy-MM-dd") : "(none)")}");
        output.WriteLine($"usable: {result.IsUsable}");

        foreach (var problem in result.Problems)
        {
            output.WriteLine($"problem: {problem}");
        }

        if (result.IsUsable)
        {
            output.WriteLine($"result: verified-ok (key {result.KeyId})");
            return OgExitCode.Ok;
        }

        application.Diagnostics.Add(
            "licence.check",
            "licence",
            command.Argument ?? "(none)",
            "refused",
            string.Join(" ", result.Problems));

        output.WriteLine("result: refused");
        return OgExitCode.Refused;
    }

    private static int Restore(OgApplication application, OgCommandLine command, TextWriter output)
    {
        output.WriteLine("restore");
        output.WriteLine($"file: {command.Argument ?? "(none)"}");

        // FND-01 §9: restore needs a backup service, which arrives with REL-01. Saying so is the honest
        // answer; pretending to restore, or exiting 0, would be worse than refusing.
        application.Diagnostics.Add(
            "restore",
            "database",
            command.Argument is null ? "(none)" : Path.GetFileName(command.Argument),
            "refused",
            "no backup service is configured; REL-01 provides it");

        output.WriteLine("reason: no backup service is configured; REL-01 provides it.");
        output.WriteLine("result: not-configured");
        return OgExitCode.Refused;
    }

    /// <summary>
    /// REL-04 AC-6: measures the budgets this build can measure and writes the machine-readable report.
    /// An over-budget measurement is a successful run that found something, so the exit code stays 0 and the
    /// result line says which.
    /// </summary>
    /// <summary>
    /// REL-04 AC-5: runs scripted typical use for the requested minutes and records the working set. An
    /// over-budget soak is still a successful measurement (exit 0) whose result line says which.
    /// </summary>
    private static int Soak(OgApplication application, OgCommandLine command, TextWriter output)
    {
        var minutes = 30d;

        if (!string.IsNullOrWhiteSpace(command.Argument)
            && (!double.TryParse(command.Argument, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out minutes)
                || minutes <= 0 || minutes > 240))
        {
            application.Diagnostics.Add(
                "command",
                "verb",
                "soak",
                "refused",
                $"'{command.Argument}' is not a duration between 0 and 240 minutes");

            output.WriteLine($"reason: '{command.Argument}' is not a number of minutes between 0 and 240.");
            output.WriteLine("result: refused");
            return OgExitCode.Refused;
        }

        var duration = TimeSpan.FromMinutes(minutes);
        var sampleEvery = TimeSpan.FromSeconds(Math.Clamp(duration.TotalSeconds / 20, 1, 60));

        output.WriteLine("soak");
        output.WriteLine($"duration: {duration.TotalMinutes:0.##} min");
        output.WriteLine($"sample-every: {sampleEvery.TotalSeconds:0} s");
        output.WriteLine($"budget: {Performance.OgSoakRunner.BudgetMegabytes:0} MB working set");

        try
        {
            var (report, path) = Performance.OgSoakRunner.RunAndSave(application, duration, sampleEvery);

            foreach (var note in report.Notes)
            {
                output.WriteLine($"note: {note}");
            }

            output.WriteLine($"cycles: {report.Cycles}");
            output.WriteLine($"start-mb: {report.StartMegabytes:0}");
            output.WriteLine($"end-mb: {report.EndMegabytes:0}");
            output.WriteLine($"peak-mb: {report.PeakMegabytes:0}");
            output.WriteLine($"growth-mb: {report.GrowthMegabytes:+0;-0;0}");
            output.WriteLine($"growth-mb-per-minute: {report.GrowthPerMinute:+0.0;-0.0;0.0}");
            output.WriteLine($"within-budget: {report.WithinBudget}");
            output.WriteLine($"report: {path}");
            output.WriteLine(report.WithinBudget ? "result: ok" : "result: over-budget");
            return OgExitCode.Ok;
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = application.Diagnostics.ReportHandled(
                ex,
                "running the soak",
                "no soak report was written",
                FailureSeverity.Error);

            output.WriteLine($"problem: the soak could not run. Reference: {reference}");
            output.WriteLine("result: failed");
            return OgExitCode.Failed;
        }
    }
    private static int Benchmark(OgApplication application, TextWriter output)
    {
        output.WriteLine("report-benchmark");

        OgBenchmarkReport report;

        try
        {
            report = Performance.OgBenchmarkRunner.Run(application);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            var reference = application.Diagnostics.ReportHandled(
                ex,
                "running the benchmark",
                "no benchmark report was written",
                FailureSeverity.Error);

            output.WriteLine($"problem: the benchmark could not run. Reference: {reference}");
            output.WriteLine("result: failed");
            return OgExitCode.Failed;
        }

        var path = Performance.OgBenchmarkRunner.Save(report);

        output.WriteLine($"machine: {report.Machine.Describe()}");

        foreach (var measurement in report.Measurements)
        {
            output.WriteLine(measurement.Describe());
        }

        foreach (var note in report.Notes)
        {
            output.WriteLine($"note: {note}");
        }

        output.WriteLine($"measured: {report.MeasuredCount}");
        output.WriteLine($"over-budget: {report.OverBudgetCount}");
        output.WriteLine($"report: {path}");
        output.WriteLine(report.Passed ? "result: ok" : "result: over-budget");
        return OgExitCode.Ok;
    }

    /// <summary>REL-04: writes the deterministic inputs a benchmark run uses, so no customer data is needed.</summary>
    private static int SeedBenchmark(OgApplication application, TextWriter output)
    {
        output.WriteLine("seed-benchmark");

        try
        {
            var path = Performance.OgBenchmarkRunner.SeedData();

            output.WriteLine($"file: {path}");
            output.WriteLine("result: ok");
            return OgExitCode.Ok;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            var reference = application.Diagnostics.ReportHandled(
                ex,
                "seeding the benchmark data",
                "no benchmark file was written",
                FailureSeverity.Error);

            output.WriteLine($"problem: the benchmark data could not be written. Reference: {reference}");
            output.WriteLine("result: failed");
            return OgExitCode.Failed;
        }
    }
    private static int NotAvailable(
        OgApplication application,
        TextWriter output,
        string what,
        string owner)
    {
        application.Diagnostics.Add(
            "command",
            "verb",
            what,
            "refused",
            $"not available until {owner} is delivered");

        output.WriteLine($"reason: {what} is not available until {owner} is delivered.");
        output.WriteLine("result: not-available");
        return OgExitCode.Refused;
    }

    private static int Unknown(TextWriter output)
    {
        output.WriteLine("reason: the command line named an unknown verb.");
        output.WriteLine(OgCommandLine.Usage);
        output.WriteLine("result: refused");
        return OgExitCode.Refused;
    }

    private static int Usage(TextWriter output)
    {
        output.WriteLine(OgCommandLine.Usage);
        output.WriteLine("result: refused");
        return OgExitCode.Refused;
    }
}
