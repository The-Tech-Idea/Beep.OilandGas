using System.Net;
using System.Net.Http;
using System.Text;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Controls;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Headless;

/// <summary>
/// The <c>--smoke</c> verb: proves the pipeline, the store, the toast and every catalog page construct,
/// and writes the result where an operator can read it. It never shows a window.
/// </summary>
/// <summary>The smoke run's report and its verdict.</summary>
public sealed record OgSmokeReport(string Report, bool Ok);

public static class SmokeRunner
{
    public static OgSmokeReport Run(OgApplication application)
    {
        ArgumentNullException.ThrowIfNull(application);

        var report = new StringBuilder();

        report.AppendLine($"data-root: {OgPaths.DataRoot}");
        report.AppendLine($"failure-store: {OgPaths.FailureStorePath}");
        report.AppendLine($"store-under-root: {OgPaths.IsWritableLocation(OgPaths.FailureStorePath)}");

        // 1. The failure path: report, receive a reference, and find the record.
        var reference = application.Diagnostics.ReportHandled(
            new InvalidOperationException("token=smoke-sentinel-value"),
            operation: "loading the smoke fixture",
            consequence: "nothing was shown for the smoke selection",
            severity: FailureSeverity.Error);

        report.AppendLine($"reference: {reference}");
        report.AppendLine($"reference-well-formed: {FailureReferenceFormat.IsWellFormed(reference)}");

        var stored = application.Failures.Find(reference);
        report.AppendLine($"failure-stored: {stored is not null}");
        report.AppendLine($"secret-redacted: {stored is not null && !stored.Message.Contains("smoke-sentinel-value", StringComparison.Ordinal)}");
        report.AppendLine($"severity-stored: {stored?.Severity}");

        // FND-06 AC-6: the same failure is also in the rolling log, joinable by its reference.
        var logFiles = Directory.Exists(OgPaths.LogDirectory)
            ? Directory.GetFiles(OgPaths.LogDirectory, "*.log")
            : [];
        var logText = logFiles.Length == 0 ? string.Empty : File.ReadAllText(logFiles[0]);

        report.AppendLine($"log-file-written: {logFiles.Length > 0}");
        report.AppendLine($"log-carries-reference: {logText.Contains(reference, StringComparison.Ordinal)}");

        // 2. The audit path.
        application.Diagnostics.Add("smoke.run", "installation", "stage0", "ok", "smoke audit row");
        report.AppendLine($"audit-rows: {application.Audit.Count()}");

        // 3. The toast surface was driven by the pipeline, not by a page.
        var history = application.Notifications.History;
        report.AppendLine($"toasts-raised: {history.Count}");

        var last = history.LastOrDefault();
        report.AppendLine($"toast-carries-reference: {last?.Reference == reference}");
        report.AppendLine($"toast-carries-no-secret: {last is not null && !(last.MessageKey + string.Join('|', last.Args ?? [])).Contains("smoke-sentinel-value", StringComparison.Ordinal)}");

        // 4. Every catalog page must construct (this is what catches a broken resource or template).
        var constructed = 0;
        var pages = OgModuleCatalog.Pages.Count;
        foreach (var page in OgModuleCatalog.Pages)
        {
            var view = OgPageFactory.Create(application, page.Route);
            if (view is null)
            {
                report.AppendLine($"page-missing-factory: {page.Route}");
                continue;
            }

            constructed++;
        }

        report.AppendLine($"pages-declared: {pages}");
        report.AppendLine($"views-constructed: {constructed == pages}");

        // 5. The shell itself must construct; this catches a broken resource or template early.
        var shellConstructed = false;
        OgLanding? landing = null;
        string? landedRoute = null;
        var landingAudited = false;
        try
        {
            // A no-op close: the production shell shuts the application down when it closes (AC-5), and the
            // smoke run is still writing its report at this point.
            var shell = new TheTechIdea.Beep.OilandGas.Desktop.Shell.OgShellWindow(application, onClosed: () => { });

            // Loading is what selects the persona's page and writes the audit row; a constructed-but-never-shown
            // window has only computed the landing.
            shell.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.FrameworkElement.LoadedEvent));

            landing = shell.Landing;
            landedRoute = shell.LandedRoute;
            landingAudited = application.Audit.Recent(20).Any(row =>
                row.Verb == "landing"
                && row.SubjectId == shell.Landing.Route
                && string.Equals(row.UserId, application.Session.UserId, StringComparison.Ordinal));

            shell.Close();
            shellConstructed = true;
        }
#pragma warning disable BEEP0003 // the shell failure is reported through the pipeline below
        catch (Exception shellFailure)
#pragma warning restore BEEP0003
        {
            application.Diagnostics.ReportHandled(
                shellFailure,
                "constructing the application shell",
                "the shell could not be created",
                FailureSeverity.Critical);
        }

        report.AppendLine($"shell-constructed: {shellConstructed}");

        // 5b. FND-07 AC-7: the RBAC role set sets the landing route. The map is produced by asking the same
        //     guard the navigation uses, once per persona, with only that persona's role granted.
        var landingMap = OgPersonaLanding.All
            .Select(entry =>
            {
                var permissions = new OgPermissions([entry.Role], [], permissionSourceAvailable: true);
                var resolved = OgPersonaLanding.Resolve(
                    permissions.Roles,
                    route => application.Navigation.CanAccess(route, permissions));

                return $"{entry.Role}->{resolved.Route}";
            })
            .ToList();

        var landingFallback = OgPersonaLanding.Resolve(["DrillingEngineer"], route => route == "dashboard");

        report.AppendLine($"landing-persona: {landing?.Persona}");
        report.AppendLine($"landing-route: {landing?.Route}");
        report.AppendLine($"landing-because: {landing?.Because}");
        report.AppendLine($"landing-fallback: {landingFallback.Route} ({landingFallback.Persona}; fallback={landingFallback.IsFallback})");
        report.AppendLine($"landing-map: {string.Join(" ", landingMap)}");
        report.AppendLine($"landing-landed-route: {landedRoute}");
        report.AppendLine($"landing-audited: {landingAudited}");
        report.AppendLine($"landing-map-count: {landingMap.Count}");

        // 5c. REL-04: the budgets are measured, stored, read back and shown on the performance page.
        var benchmark = Performance.OgBenchmarkRunner.Run(application);
        var benchmarkPath = Performance.OgBenchmarkRunner.Save(benchmark);
        var benchmarkReload = Performance.OgBenchmarkRunner.LoadLast(benchmarkPath);
        var benchmarkPage = new Views.PerformancePageView(application);
        var cancelLatency = OgCancellationProbe.MeasureLatency();

        string? Measure(string key) =>
            benchmark.Measurements.FirstOrDefault(row => row.Key == key)?.MeasuredMilliseconds?.ToString("0.0");

        var log100k = benchmark.Measurements.FirstOrDefault(row => row.Key == "log-100k");

        report.AppendLine($"performance-report-path: {benchmarkPath}");
        report.AppendLine($"performance-measured: {benchmark.MeasuredCount}");
        report.AppendLine($"performance-over-budget: {benchmark.OverBudgetCount}");
        report.AppendLine($"performance-log-20k-ms: {Measure("log-20k")}");
        report.AppendLine($"performance-log-100k-ms: {Measure("log-100k")}");
        report.AppendLine($"performance-grid-page-ms: {Measure("grid-page")}");
        report.AppendLine($"performance-schematic-ms: {Measure("schematic")}");
        report.AppendLine($"performance-decimation: {log100k?.Note}");
        report.AppendLine($"performance-cancel-ms: {cancelLatency:0.0}");
        report.AppendLine($"performance-report-parseable: {benchmarkReload is not null}");
        report.AppendLine($"performance-page-rows: {benchmarkPage.Rows.Count}");
        report.AppendLine($"performance-page-over-budget: {benchmarkPage.Rows.Count(row => row.OverBudget)}");

        // 5b-b. REL-04 AC-5: a short soak exercises the same scripted use the 30-minute run uses, so the
        //       harness is proven by the smoke even though the budget is claimed from the long run.
        var shortSoak = Performance.OgSoakRunner.Run(
            application,
            TimeSpan.FromSeconds(1.5),
            TimeSpan.FromSeconds(0.5));

        report.AppendLine($"soak-cycles: {shortSoak.Cycles}");
        report.AppendLine($"soak-peak-mb: {shortSoak.PeakMegabytes:0}");
        report.AppendLine($"soak-within-budget: {shortSoak.WithinBudget}");



        // 5d. ENG-10: pay summary on the library's reference interval, the refusals the screen must make, an
        //     edit that marks the result stale, and exports that exist on disk.
        var payPage = new Views.PaySummaryPageView(application);
        var payShifted = payPage.Shift();
        var payStale = payPage.IsStale;
        var payUndone = payPage.Undo();
        var payRecomputed = payPage.Compute();
        var payExportedCsv = payPage.Export("csv");
        var payCsvPath = payPage.ExportedPath;
        var payExportedXlsx = payPage.Export("xlsx");
        var payXlsxPath = payPage.ExportedPath;

        var paySpliced = payPage.Splice();
        var paySpliceEntry = payPage.History.FirstOrDefault(row =>
            row.Field.Length == 0
            && row.After.Contains("main", StringComparison.Ordinal)
            && row.After.Contains("run 2", StringComparison.Ordinal));
        var paySpliceActor = paySpliceEntry?.Author;
        var paySpliceUndone = payPage.Undo();

        var payMissingCurve = WellTools.Bridging.OgPaySummaryWorkspace.ReferenceWell();
        payMissingCurve.LogSets[0].Curves.RemoveAll(curve => curve.Mnemonic is "VSH" or "GR");
        var payExcluded = WellTools.Bridging.OgPaySummaryWorkspace.Compute(
            payMissingCurve,
            WellTools.Bridging.OgPaySummaryWorkspace.ReferenceZones(),
            WellTools.Bridging.OgPaySummaryWorkspace.ReferenceParameters());

        var payOverlap = WellTools.Bridging.OgPaySummaryWorkspace.Compute(
            WellTools.Bridging.OgPaySummaryWorkspace.ReferenceWell(),
            [new("A", 1000, 1006), new("B", 1005, 1009)],
            WellTools.Bridging.OgPaySummaryWorkspace.ReferenceParameters());

        report.AppendLine($"pay-zones: {payPage.Rows.Count}");
        report.AppendLine($"pay-net: {payPage.Result?.NetReservoir:0.###}");
        report.AppendLine($"pay-net-pay: {payPage.Result?.NetPay:0.###}");
        report.AppendLine($"pay-ntg: {payPage.Result?.NetToGross:0.###}");
        report.AppendLine($"pay-shifted: {payShifted}");
        report.AppendLine($"pay-stale-after-edit: {payStale}");
        report.AppendLine($"pay-undone: {payUndone}");
        report.AppendLine($"pay-recomputed: {payRecomputed}");
        report.AppendLine($"pay-history-rows: {payPage.History.Count}");
        report.AppendLine($"pay-excluded: {payExcluded.Value?.Zones.All(zone => zone.Excluded)}");
        report.AppendLine($"pay-excluded-note: {payExcluded.Value?.Zones.FirstOrDefault()?.Note}");
        report.AppendLine($"pay-overlap-refused: {!payOverlap.Success}");
        report.AppendLine($"pay-overlap-code: {payOverlap.Reference}");
        report.AppendLine($"pay-csv: {payExportedCsv}");
        report.AppendLine($"pay-csv-path: {payCsvPath}");
        report.AppendLine($"pay-xlsx: {payExportedXlsx}");
        report.AppendLine($"pay-xlsx-path: {payXlsxPath}");
        report.AppendLine($"pay-spliced: {paySpliced}");
        report.AppendLine($"pay-splice-both-runs: {paySpliceEntry is not null}");
        report.AppendLine($"pay-splice-actor: {paySpliceActor}");
        report.AppendLine($"pay-splice-undone: {paySpliceUndone}");

        // 6. FND-04: the design-system picker has choices and one is applied.
        var skins = TheTechIdea.Beep.OilandGas.Desktop.Theme.OgSkins.All.Count;
        var skinApplied = TheTechIdea.Beep.OilandGas.Desktop.Theme.OgSkins.Current;
        report.AppendLine($"skins-available: {skins}");
        report.AppendLine($"skin-current: {skinApplied}");
        report.AppendLine($"skin-selfcheck: {string.Join(" | ", TheTechIdea.Beep.OilandGas.Desktop.Theme.OgSkins.SelfCheck())}");

        // 7. FND-05: the locale switch flips direction and translates the shell strings.
        var loc = application.Loc;
        var englishTitle = loc.Get("Page.dashboard");
        loc.SetCulture(OgLoc.Arabic);
        var arabicTitle = loc.Get("Page.dashboard");
        var rtl = loc.IsRightToLeft;
        loc.SetCulture(OgLoc.English);
        var restoredTitle = loc.Get("Page.dashboard");

        report.AppendLine($"loc-cultures: {loc.Cultures.Count}");
        report.AppendLine($"rtl-switch: {rtl}");
        report.AppendLine($"loc-translated: {!string.Equals(englishTitle, arabicTitle, StringComparison.Ordinal)}");
        report.AppendLine($"loc-restored: {string.Equals(englishTitle, restoredTitle, StringComparison.Ordinal)}");

        // 8. The notification centre opens over the same history.
        var centre = new TheTechIdea.Beep.OilandGas.Desktop.Shell.NotificationCenterWindow(
            application.Notifications,
            application.Loc);
        report.AppendLine($"notification-centre: {centre.Content is not null}");
        centre.Close();

        // 8c. FND-06: the admin pages read the local rows — the smoke's own failure and audit row are there.
        var failuresPage = new Views.AdminFailuresView(application);
        var auditPage = new Views.AuditLogView(application);
        report.AppendLine($"failures-page-rows: {failuresPage.Rows.Count}");
        report.AppendLine($"audit-page-rows: {auditPage.Rows.Count}");
        report.AppendLine($"failures-page-forwarding-label: {(failuresPage.Rows.Count > 0 ? failuresPage.Rows[0].ForwardingLabel : "(none)")}");

        // 8k. VSB-04 AC-6: a log with an image log and cement tracks draws them and stops saying "no data".
        var imageDocument = WellTools.Bridging.OgLogDemo.WithImageAndCement();
        var imageTracks = imageDocument.Tracks.OfType<TheTechIdea.OilGas.WellLogs.ImageLogTrack>().Count();
        var cementTracks = WellTools.Bridging.OgLogAvailability.CementTrackCount(imageDocument);
        var imageSentence = WellTools.Bridging.OgLogAvailability.Image(imageDocument);
        var cementSentence = WellTools.Bridging.OgLogAvailability.Cement(imageDocument);

        report.AppendLine($"logs-image-tracks: {imageTracks}");
        report.AppendLine($"logs-cement-tracks: {cementTracks}");
        report.AppendLine($"logs-image-sentence: {imageSentence}");
        report.AppendLine($"logs-cement-sentence: {cementSentence}");

        // 8j. VSB-07: the well path — stations, plan/section sameness, ladder and cylinder frames.
        var pathPage = new Views.WellPathPageView(application);
        pathPage.Load();

        report.AppendLine($"wellpath-stations: {pathPage.Stations.Count}");
        report.AppendLine($"wellpath-plan-points: {pathPage.PlanPoints.Count}");
        report.AppendLine($"wellpath-section-assumed: {pathPage.SectionAzimuth.Assumed}");
        report.AppendLine($"wellpath-min-separation: {pathPage.Minimum?.Factor:0.##}");
        report.AppendLine($"wellpath-uncertainty-available: {pathPage.Uncertainty.Available}");
        report.AppendLine($"wellpath-ellipses: {pathPage.Uncertainty.Ellipses.Count}");

        // 8j-b. VSB-07 AC-6: changing the offset set cancels the scan in flight, and a superseded scan is
        //       never applied.
        var manyOffsets = Enumerable.Range(1, 300)
            .Select(index => new WellTools.Bridging.OgOffsetWell(
                $"OFF-{index}",
                WellTools.Bridging.OgWellPath.OffsetSurvey,
                29.0 + (index * 0.00001),
                48.0))
            .ToList();

        var supersededOffsets = pathPage.ComputeOffsetAsync(manyOffsets);
        var latestOffsets = pathPage.ComputeOffsetAsync(WellTools.Bridging.OgWellPath.DemoOffsetWells);

        var supersededOffsetResult = supersededOffsets.GetAwaiter().GetResult();
        var latestOffsetResult = latestOffsets.GetAwaiter().GetResult();

        report.AppendLine($"wellpath-offsets-superseded: {supersededOffsetResult is null}");
        report.AppendLine($"wellpath-offsets-cancelled-flag: {pathPage.LastComputationCancelled}");
        report.AppendLine($"wellpath-offsets-latest: {latestOffsetResult?.Result.Offsets.Count ?? 0}");
        report.AppendLine($"wellpath-offset-sets: {string.Join('|', WellTools.Bridging.OgWellPath.OffsetSets.Keys)}");

        // 8i. VSB-06: three wells correlate, the missing datum is reported, and the panel exports.
        var correlationPage = new Views.CorrelationPageView(application);
        correlationPage.Load();
        var correlationExported = correlationPage.ExportPdf();

        report.AppendLine($"correlation-wells: {correlationPage.Wells.Count}");
        report.AppendLine($"correlation-issues: {correlationPage.Issues.Count}");
        report.AppendLine($"correlation-missing-datum: {correlationPage.Issues.Count(issue => issue.Code == WellTools.Bridging.OgCorrelation.CodeDatumMissing)}");
        report.AppendLine($"correlation-exported: {correlationExported}");

        // 8i-b. VSB-06 AC-3/AC-4: the cross-section is placed by the wells' coordinates and its horizon
        //       terminations are checks - a pinch-out is reported, not left inside the drawing.
        var correlationSection = correlationPage.Section;
        var correlationSectionFrame = correlationSection is null
            ? null
            : WellTools.Bridging.OgCorrelation.RenderSection(correlationSection, 800, 400);

        report.AppendLine($"correlation-section-wells: {correlationSection?.Wells.Count ?? 0}");
        report.AppendLine($"correlation-section-pinchouts: {correlationPage.PinchOuts}");
        report.AppendLine($"correlation-section-issues: {correlationPage.SectionIssues.Count}");
        report.AppendLine($"correlation-section-frame: {correlationSectionFrame is not null}");
        report.AppendLine($"correlation-section-frame-content: {correlationSectionFrame?.Any(value => value != 0)}");
        report.AppendLine($"correlation-section-x: {string.Join(',', correlationSection?.Wells.Select(well => well.X) ?? [])}");

        // 8i-c. VSB-06 AC-6: a render in flight is cancelled when the well set changes, and the superseded
        //       render returns nothing, so no stale frame can be applied.
        var manyWells = Enumerable.Range(1, 120)
            .Select(index => ($"W{index}", WellTools.Bridging.OgCorrelation.DemoWells()[index % 3].Document))
            .ToList();

        var supersededRender = correlationPage.RenderAsync(manyWells, WellTools.Bridging.OgCorrelation.DemoDatum);
        var latestRender = correlationPage.RenderAsync(WellTools.Bridging.OgCorrelation.DemoWells(), WellTools.Bridging.OgCorrelation.DemoDatum);

        var supersededResult = supersededRender.GetAwaiter().GetResult();
        var latestResult = latestRender.GetAwaiter().GetResult();

        report.AppendLine($"correlation-render-superseded: {supersededResult is null}");
        report.AppendLine($"correlation-render-cancelled-flag: {correlationPage.LastRenderCancelled}");
        report.AppendLine($"correlation-render-latest-wells: {latestResult?.Panel.Wells.Count ?? 0}");

        // 8l. REL-02: a signed licence verifies, an altered one is refused, and the state changes without a
        //     restart. Expiry is exercised through the same projection the service uses, with a future clock.
        var licenceService = application.Licence;
        var sampleLicence = Path.Combine(AppContext.BaseDirectory, "Security", "Samples", "sample-licence.json");
        var imported = licenceService.Import(sampleLicence);
        var licence = licenceService.Current;

        var sampleText = File.ReadAllText(sampleLicence);
        var tamperedPath = Path.Combine(OgPaths.DataRoot, "licence-tampered.json");
        string signature;

        using (var document = System.Text.Json.JsonDocument.Parse(sampleText))
        {
            signature = document.RootElement.GetProperty("signature").GetString()!;
        }

        // Alter exactly one signature character and keep everything else intact.
        var altered = (signature[0] == 'A' ? "B" : "A") + signature[1..];
        File.WriteAllText(tamperedPath, sampleText.Replace(signature, altered, StringComparison.Ordinal));

        var tampered = licenceService.Import(tamperedPath);
        var afterTamper = licenceService.Current;

        var verification = Security.OgLicenceVerifier.Verify(sampleText, Security.OgLicenceVerifier.Trusted);
        var expired = verification.Claims is { } claims
            ? Security.OgLicenceState.ToEntitlement(
                claims,
                verification.KeyId,
                Core.OgLicenceEnforcement.Enforced,
                DateTimeOffset.UtcNow.AddYears(10))
            : null;

        var licencePage = new Views.LicencePageView(application);

        report.AppendLine($"licence-imported: {imported.Success}");
        report.AppendLine($"licence-edition: {licence.Edition}");
        report.AppendLine($"licence-customer: {licence.Customer}");
        report.AppendLine($"licence-modules: {string.Join(',', licence.Modules)}");
        report.AppendLine($"licence-signature-verified: {licence.SignatureVerified}");
        report.AppendLine($"licence-key: {licence.KeyId}");
        report.AppendLine($"licence-usable: {licence.IsUsable}");
        report.AppendLine($"licence-tampered-refused: {!tampered.Success}");
        report.AppendLine($"licence-after-tamper-customer: {afterTamper.Customer}");
        report.AppendLine($"licence-expired-read-allowed: {expired?.Allows(Core.OgModuleKeys.Wells)}");
        report.AppendLine($"licence-expired-write-blocked: {expired is not null && !expired.AllowsWriting(Core.OgModuleKeys.Wells)}");
        report.AppendLine($"licence-page-rows: {licencePage.Rows.Count}");

        // 8m. INT-01: LAS 2.0 loads, LAS 3.0 exports and passes the checker, and reloading gives the rows back.
        var lasPage = new Views.LasInteropPageView(application);
        var lasExample = lasPage.CreateExample();
        var lasImported = lasPage.Import();
        var lasExported = lasPage.Export();
        var lasExportPath = lasPage.ExportDestination;
        var lasReload = File.Exists(lasExportPath)
            ? WellTools.Bridging.OgLasWorkspace.Load(lasExportPath)
            : null;
        var lasFindings = File.Exists(lasExportPath)
            ? WellTools.Bridging.OgLasWorkspace.CheckLas3(File.ReadAllText(lasExportPath))
            : [];
        var lasDecimated = WellTools.Bridging.OgLasWorkspace
            .LoadText(WellTools.Bridging.OgLasWorkspace.ExampleLas, maxSamples: 4)
            .Preview;

        report.AppendLine($"las-standard: {lasPage.PreviewResult?.Standard}");
        report.AppendLine($"las-encoding: {lasPage.PreviewResult?.Encoding}");
        report.AppendLine($"las-curves: {string.Join(',', lasPage.PreviewResult?.Curves ?? Enumerable.Empty<string>())}");
        report.AppendLine($"las-rows: {lasPage.PreviewResult?.Rows}");
        report.AppendLine($"las-imported: {lasImported}");
        report.AppendLine($"las-exported: {lasExported}");
        report.AppendLine($"las-reload-standard: {lasReload?.Preview.Standard}");
        report.AppendLine($"las-reload-rows: {lasReload?.Preview.Rows}");
        report.AppendLine($"las-las3-findings: {lasFindings.Count}");
        report.AppendLine($"las-decimated: {lasDecimated.Decimated}");
        report.AppendLine($"las-decimation-note: {lasDecimated.DecimationNote}");

        // 8n. INT-02: WITSML 1.4.1.1 import, 1.x/2.1 export, and a store check that tells "no answer" from
        //     "credentials refused" without printing them.
        var witsmlPage = new Views.WitsmlInteropPageView(application);
        var witsmlExample = witsmlPage.CreateExample();
        var witsmlTrajectory = witsmlPage.Preview;
        var witsmlImported = witsmlPage.ImportFile();
        var witsmlOneX = witsmlPage.ExportOneX();
        var witsmlOneXFiles = witsmlPage.ExportedFiles.ToList();
        var witsmlTwoOne = witsmlPage.ExportTwoOne();
        var witsmlTwoOneFiles = witsmlPage.ExportedFiles.ToList();

        var reopenedTrajectory = Path.Combine(witsmlPage.ExportFolder, "trajectory.xml");
        var witsmlReload = File.Exists(reopenedTrajectory)
            ? WellTools.Bridging.OgWitsmlWorkspace.LoadFile(reopenedTrajectory)
            : null;

        var logsFile = Path.Combine(OgPaths.ExportsDirectory, "witsml-logs.xml");
        var witsmlLogs = File.Exists(logsFile)
            ? WellTools.Bridging.OgWitsmlWorkspace.LoadFile(logsFile)
            : null;

        var wellFormed21 = witsmlTwoOneFiles
            .Where(path => path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
            .All(path =>
            {
                System.Xml.Linq.XDocument.Parse(File.ReadAllText(path));
                return true;
            });

        const string probePassword = "smoke-secret-probe";
        var storeOptions = new TheTechIdea.OilGas.Wellbore.Loaders.WitsmlStoreOptions
        {
            Endpoint = new Uri("https://store.example.test/Store"),
            UserName = "smoke-user",
            Password = probePassword,
            Retries = 0,
            Timeout = TimeSpan.FromSeconds(5),
        };

        var storeProbe = Task.Run(async () =>
        {
            var quiet = new SmokeFailureReporter();
            var unreachable = await WellTools.Bridging.OgWitsmlWorkspace
                .CheckStoreAsync(storeOptions, quiet, new SmokeThrowingHandler());
            var refused = await WellTools.Bridging.OgWitsmlWorkspace
                .CheckStoreAsync(storeOptions, quiet, new SmokeStatusHandler(System.Net.HttpStatusCode.Unauthorized));

            return (unreachable, refused);
        }).GetAwaiter().GetResult();

        var storeEvidence = storeProbe.unreachable.Detail + storeProbe.refused.Detail;

        report.AppendLine($"witsml-example: {witsmlExample}");
        report.AppendLine($"witsml-version: {witsmlTrajectory?.Version}");
        report.AppendLine($"witsml-stations: {witsmlTrajectory?.Stations}");
        report.AppendLine($"witsml-imported: {witsmlImported}");
        report.AppendLine($"witsml-onex-files: {witsmlOneXFiles.Count}");
        report.AppendLine($"witsml-onex-reload-stations: {witsmlReload?.Stations}");
        report.AppendLine($"witsml-21-files: {witsmlTwoOneFiles.Count}");
        report.AppendLine($"witsml-21-xml-wellformed: {wellFormed21}");
        report.AppendLine($"witsml-time-log-reported: {witsmlLogs?.Warnings.Any(warning => warning.Contains("time-based", StringComparison.OrdinalIgnoreCase))}");
        report.AppendLine($"witsml-store-unreachable: {storeProbe.unreachable.State}");
        report.AppendLine($"witsml-store-auth: {storeProbe.refused.State}");
        report.AppendLine($"witsml-store-password-leaked: {storeEvidence.Contains(probePassword, StringComparison.Ordinal)}");

        // 8o. INT-03: OSDU export, a dry run that sends nothing, a rejected token, a successful send, and
        //     duplicate proposals that are never merged. A scoped environment supplies the OSDU settings.
        const string osduConfig = """
                                  {
                                    "osdu": {
                                      "dataPartition": "opendes",
                                      "viewers": [ "data.default.viewers@opendes.example.com" ],
                                      "owners": [ "data.default.owners@opendes.example.com" ],
                                      "legalTags": [ "opendes-public-usa-dataset-1" ]
                                    },
                                    "toolboxes": {
                                      "dataai": {
                                        "osduStorageUrl": "https://osdu.example.com/api/storage/v2/records",
                                        "osduBatchSize": 1
                                      }
                                    }
                                  }
                                  """;

        bool osduExported;
        bool osduDryRun;
        bool osduTokenRejected;
        bool osduSent;
        bool osduReconciled;
        int osduRecords;
        int osduBatches;
        int osduCandidates;
        string osduEvidence;

        using (WellboreServices.Scope(new WellboreServices(WellboreEnvironment.FromJson(osduConfig))))
        {
            var osduPage = new Views.OsduInteropPageView(application)
            {
                AccessToken = "smoke-osdu-token",
            };

            osduExported = osduPage.ExportJson();
            osduRecords = osduPage.Plan?.RecordCount ?? 0;
            osduBatches = osduPage.Plan?.BatchCount ?? 0;
            osduDryRun = osduPage.DryRun();

            // The bridge is driven directly: the page's Send is asynchronous on the UI thread, and the smoke
            // run must not block the dispatcher inside its own method.
            using (var rejectedHttp = new HttpClient(new SmokeStatusHandler(HttpStatusCode.Unauthorized)))
            {
                var rejected = WellTools.Bridging.OgOsduWorkspace
                    .SendAsync(osduPage.Records, "smoke-osdu-token", rejectedHttp)
                    .GetAwaiter().GetResult();

                osduTokenRejected = rejected.TokenExpired;
                osduEvidence = rejected.Describe() + " " + string.Join(" ", rejected.Batches.Select(batch => batch.Error ?? string.Empty));
            }

            using (var acceptedHttp = new HttpClient(new SmokeStatusHandler(HttpStatusCode.OK)))
            {
                var osduAccepted = WellTools.Bridging.OgOsduWorkspace
                    .SendAsync(osduPage.Records, "smoke-osdu-token", acceptedHttp)
                    .GetAwaiter().GetResult();

                osduSent = osduAccepted.Succeeded;
                osduEvidence += " " + osduAccepted.Describe();
            }

            osduReconciled = osduPage.Reconcile();
            osduCandidates = osduPage.Candidates.Count;
            osduEvidence += " " + osduPage.SummaryText;
        }

        report.AppendLine($"osdu-exported: {osduExported}");
        report.AppendLine($"osdu-records: {osduRecords}");
        report.AppendLine($"osdu-batches: {osduBatches}");
        report.AppendLine($"osdu-dry-run: {osduDryRun}");
        report.AppendLine($"osdu-token-rejected: {osduTokenRejected}");
        report.AppendLine($"osdu-sent: {osduSent}");
        report.AppendLine($"osdu-reconciled: {osduReconciled}");
        report.AppendLine($"osdu-candidates: {osduCandidates}");
        report.AppendLine($"osdu-token-leaked: {osduEvidence.Contains("smoke-osdu-token", StringComparison.Ordinal)}");

        // 8p. RPT-03: every chart type renders through the domain's IRenderingService, an empty series says so,
        //     and an export writes a PNG of the same chart.
        var chartsPage = new Views.ChartsPageView(application);

        var chartLine = chartsPage.RenderType("line");
        var chartLineMessage = chartsPage.LastMessage;
        var chartPie = chartsPage.RenderType("pie");
        var chartBar = chartsPage.RenderType("bar");
        var chartEmpty = chartsPage.RenderEmpty();
        var chartEmptyMessage = chartsPage.LastMessage;

        // Export a real chart, not the empty one.
        chartsPage.RenderType("line");
        var chartExported = chartsPage.Export();
        var chartPath = chartsPage.ExportedPath;
        var chartBytes = chartPath is not null && File.Exists(chartPath)
            ? new FileInfo(chartPath).Length
            : 0;
        var chartSignature = chartPath is not null && File.Exists(chartPath)
            ? Convert.ToHexString(File.ReadAllBytes(chartPath), 0, 4)
            : string.Empty;

        report.AppendLine($"charts-line: {chartLine}");
        report.AppendLine($"charts-line-message: {chartLineMessage}");
        report.AppendLine($"charts-pie: {chartPie}");
        report.AppendLine($"charts-bar: {chartBar}");
        report.AppendLine($"charts-empty: {chartEmptyMessage}");
        var chartSeriesBeforeExport = chartsPage.RenderedSeriesCount;
        report.AppendLine($"charts-series-before-export: {chartSeriesBeforeExport}");
        report.AppendLine($"charts-exported: {chartExported}");
        report.AppendLine($"charts-png-bytes: {chartBytes}");
        report.AppendLine($"charts-png-signature: {chartSignature}");

        // 8q. VSB-04 AC-2: the lithology standard is a fill layer, so the same document draws differently under
        //     FGDC and the company sample, and restoring FGDC restores the picture exactly.
        var standardsDirectory = Path.Combine(OgPaths.DataRoot, "Standards");

        WellTools.Bridging.OgLogStandards.Apply(WellTools.Bridging.OgLogStandards.Fgdc, standardsDirectory);
        var lithologyFgdc = RenderLithology();

        WellTools.Bridging.OgLogStandards.Apply(WellTools.Bridging.OgLogStandards.SampleCompany, standardsDirectory);
        var lithologySample = RenderLithology();
        var lithologyActive = WellTools.Bridging.OgLogStandards.Active;

        WellTools.Bridging.OgLogStandards.Apply(WellTools.Bridging.OgLogStandards.Fgdc, standardsDirectory);
        var lithologyRestored = RenderLithology();

        report.AppendLine($"log-standards: {string.Join(" | ", WellTools.Bridging.OgLogStandards.Available)}");
        report.AppendLine($"log-standard-active-sample: {lithologyActive}");
        report.AppendLine($"log-standard-fill-changed: {!lithologyFgdc.AsSpan().SequenceEqual(lithologySample)}");
        report.AppendLine($"log-standard-fill-restored: {lithologyFgdc.AsSpan().SequenceEqual(lithologyRestored)}");

        // 8r. REL-03: contrast on the shipped tokens and on the high-contrast skin, an accessible-name audit over
        //     every page, the command palette's filter, and a chart's text alternative.
        var defaultColours = SmokeThemeColours();
        var defaultContrast = OgThemeContrast.Audit("tokens", defaultColours);

        // The desktop's own chrome tokens are skin-independent (checked below), so "contrast across the 66
        // skins" is not a variable this build has. The audit's ability to fail is proven instead with an
        // adverse palette in the same run, and by a test.
        var adverseColours = defaultColours with
        {
            TextSecondary = "#DDDDDD",
            Warning = "#F5D08A",
        };

        var adverseContrast = OgThemeContrast.Audit("adverse", adverseColours);

        var themeBefore = Theme.OgSkins.Current;
        Theme.OgSkins.Apply(TheTechIdea.Beep.WPF.Controls.Styling.BeepControlStyle.HighContrast, persist: false);
        var skinsChangeTokens = DescribeColours(SmokeThemeColours()) != DescribeColours(defaultColours);
        Theme.OgSkins.Apply(themeBefore, persist: false);

        var unnamed = new List<string>();
        var untabbable = new List<string>();

        foreach (var page in OgModuleCatalog.Pages)
        {
            if (OgPageFactory.Create(application, page.Route) is System.Windows.DependencyObject view)
            {
                unnamed.AddRange(OgAccessibilityAudit
                    .UnnamedInteractive(view)
                    .Select(problem => $"{page.Route}: {problem}"));

                untabbable.AddRange(OgAccessibilityAudit
                    .NotInTabOrder(view)
                    .Select(problem => $"{page.Route}: {problem}"));
            }
        }

        var paletteEntries = OgCommandPalette.FromCatalog(route => application.Loc.Get($"Page.{route}"));
        var paletteMatches = OgCommandPalette.Filter(paletteEntries, "chart");
        var paletteWrapped = OgCommandPalette.Move(0, -1, Math.Max(1, paletteEntries.Count)) == paletteEntries.Count - 1;

        var summaryChart = new WellTools.Reporting.ChartSurface
        {
            Renderer = application.Charts,
            Request = new global::Beep.OilandGas.Models.Data.Rendering.RenderChartRequest
            {
                ChartType = "line",
                Width = 320,
                Height = 200,
                ChartData = new Dictionary<string, object>
                {
                    ["labels"] = new[] { "A", "B" },
                    ["series"] = new List<WellTools.Reporting.OgChartSeries>
                    {
                        new("Oil", [10, 20]),
                    },
                },
            },
        };

        using (var summaryRenderer = new WellTools.Hosting.ViewSurfaceRenderer { Surface = summaryChart })
        {
            summaryRenderer.Render(320, 200);
        }

        report.AppendLine($"accessibility-contrast: {defaultContrast.Describe()}");
        report.AppendLine($"accessibility-contrast-adverse: {adverseContrast.Describe()}");
        report.AppendLine($"accessibility-tokens: {DescribeColours(defaultColours)}");
        report.AppendLine($"accessibility-skins-change-tokens: {skinsChangeTokens}");

        // 8r-b. REL-03 AC-3: audit the control library's own palette under every skin, not just the desktop's
        //       tokens. Text pairs at 4.5:1, accent/status at 3:1.
        var skinBeforeAudit = Theme.OgSkins.Current;
        var skinFailures = new List<string>();
        var adjustments = new List<string>();
        var skinsAudited = 0;
        var accentFailures = 0;
        var textFailures = 0;

        foreach (var skin in Theme.OgSkins.All)
        {
            Theme.OgSkins.Apply(skin.Style, persist: false);

            var colours = ReadSkinColours();
            var reportForSkin = OgThemeContrast.AuditSkin(skin.DisplayName, colours);

            skinsAudited++;

            // REL-03 AC-3: what the guard had to move for this skin, so the record says the palette was adjusted rather than silently
            // rewritten. A skin whose palette already passes contributes nothing.
            foreach (var change in Theme.OgSkins.LastContrastAdjustments)
            {
                adjustments.Add($"{skin.DisplayName}: {change}");
            }

            foreach (var pair in reportForSkin.Pairs.Where(pair => !pair.Passes))
            {
                skinFailures.Add($"{skin.DisplayName}: {pair.Describe()}");

                if (pair.Foreground == colours.Accent && pair.Background == colours.Surface)
                {
                    accentFailures++;
                }
                else
                {
                    textFailures++;
                }
            }
        }

        Theme.OgSkins.Apply(skinBeforeAudit, persist: false);

        report.AppendLine($"skin-contrast-audited: {skinsAudited}");
        report.AppendLine($"skin-contrast-failures: {skinFailures.Count}");
        report.AppendLine($"skin-contrast-accent-failures: {accentFailures}");
        report.AppendLine($"skin-contrast-text-failures: {textFailures}");
        report.AppendLine($"skin-contrast-first-failure: {(skinFailures.Count == 0 ? "(none)" : skinFailures[0])}");
        report.AppendLine(
            $"skin-contrast-failures-all: {(skinFailures.Count == 0 ? "(none)" : string.Join(" | ", skinFailures))}");
        report.AppendLine(
            $"skin-contrast-adjustments: {(adjustments.Count == 0 ? "(none)" : string.Join(" | ", adjustments))}");
        report.AppendLine($"accessibility-unnamed-count: {unnamed.Count}");
        report.AppendLine($"accessibility-unnamed-first: {(unnamed.Count == 0 ? "(none)" : unnamed[0])}");
        report.AppendLine($"accessibility-unnamed-all: {string.Join(" ;; ", unnamed)}");
        report.AppendLine($"accessibility-untabbable-count: {untabbable.Count}");
        report.AppendLine($"accessibility-untabbable-all: {string.Join(" ;; ", untabbable)}");
        report.AppendLine($"palette-chart-matches: {paletteMatches.Count}");
        report.AppendLine($"palette-wrap: {paletteWrapped}");
        report.AppendLine($"chart-text-alternative: {summaryChart.AccessibleDescription}");

        // 8h. VSB-09: the export page plans and writes a real PDF for the selected view.
        var exportPage = new Views.ExportPageView(application);
        var exportPlanned = exportPage.ShowPlan();
        var exportWritten = exportPage.RunExport();
        var exportedPath = exportPage.BuildRequest().Destination;
        var exportedHeader = File.Exists(exportedPath)
            ? System.Text.Encoding.ASCII.GetString(File.ReadAllBytes(exportedPath), 0, 5)
            : string.Empty;

        report.AppendLine($"export-planned: {exportPlanned}");
        report.AppendLine($"export-written: {exportWritten}");
        report.AppendLine($"export-pdf-signature: {exportedHeader == "%PDF-"}");
        report.AppendLine($"export-bytes: {(File.Exists(exportedPath) ? new FileInfo(exportedPath).Length : 0)}");
        report.AppendLine($"export-summary: {exportPage.SummaryText}");

        // 8h-b. VSB-09 AC-6: the map exports with a world file and coordinate metadata, and the transform in
        //       the file maps every well back inside the image.
        var mapDestination = Path.Combine(OgPaths.ExportsDirectory, "smoke-map.png");

        exportPage.SelectSource("Well map");
        exportPage.Destination = mapDestination;
        exportPage.SetGeoreferenced(true);

        var mapPlanned = exportPage.ShowPlan();
        var mapWritten = exportPage.RunExport();
        var mapResult = exportPage.LastMapExport;

        var mapWorld = mapResult?.WorldFilePath is { } worldPath && File.Exists(worldPath)
            ? WellTools.Export.OgGeoTransform.FromWorldFile(File.ReadAllText(worldPath))
            : null;

        var mapWellsInside = mapResult is not null
                             && mapWorld is not null
                             && WellTools.Export.OgMapExport.DemoMap().Wells.All(well =>
                             {
                                 var (column, row) = mapWorld.ToPixel(well.X, well.Y);
                                 return column >= 0 && column <= mapResult.Width && row >= 0 && row <= mapResult.Height;
                             });

        report.AppendLine($"map-export-planned: {mapPlanned}");
        report.AppendLine($"map-export-written: {mapWritten}");
        report.AppendLine($"map-export-world-file: {mapResult?.WorldFilePath is not null && File.Exists(mapResult.WorldFilePath)}");
        report.AppendLine($"map-export-metadata: {mapResult?.MetadataPath is not null && File.Exists(mapResult.MetadataPath)}");
        report.AppendLine($"map-export-crs: {mapResult?.Crs}");
        report.AppendLine($"map-export-pixel-size: {mapWorld?.PixelSize:0.###}");
        report.AppendLine($"map-export-wells-inside: {mapWellsInside}");
        report.AppendLine($"map-export-summary: {mapResult?.Describe()}");

        // 8g. VSB-08: the symbol gallery lists the catalog and renders a preview.
        var symbolPage = new Views.SymbolGalleryPageView(application);
        symbolPage.Reload(notice: null);
        report.AppendLine($"symbols-loaded: {symbolPage.Summary.Symbols}");
        report.AppendLine($"symbol-categories: {symbolPage.Summary.Categories}");
        report.AppendLine($"symbols-shown: {symbolPage.Rows.Count}");

        // 8f. VSB-05: the schematic page reads strings, annuli, barriers, findings and the tally.
        var schematicPage = new Views.WellSchematicPageView(application);
        schematicPage.Load();
        report.AppendLine($"schematic-strings: {schematicPage.Strings.Count}");
        report.AppendLine($"schematic-annuli: {schematicPage.Annuli.Count}");
        report.AppendLine($"schematic-barriers: {schematicPage.Barriers.Count}");
        report.AppendLine($"schematic-unverified-hatched: {schematicPage.UnverifiedBarriers.Count}");
        report.AppendLine($"schematic-findings: {schematicPage.Findings.Count}");
        report.AppendLine($"schematic-tally-rows: {schematicPage.Tally?.Rows.Count ?? 0}");

        // 8e. VSB-04: the well-log page lists real curves and states the log's extent.
        var logPage = new Views.WellLogPageView(application);
        logPage.Load();
        report.AppendLine($"logs-page-curves: {logPage.CurveRows.Count}");
        report.AppendLine($"logs-page-summary: {logPage.SummaryText.Split('|')[0].Trim()}");

        // 8d. FND-02: the data-path health window constructs over the live access mode.
        var healthWindow = new TheTechIdea.Beep.OilandGas.Desktop.Shell.ConnectionHealthWindow(application);
        report.AppendLine($"health-window: {healthWindow.Content is not null}");

        // 8t. FND-02 AC-3: a business read with no API configured is a failed state with a cause, not an
        //     empty list. The reader's other failure states are covered by RemoteReadTests.
        var remoteRead = healthWindow.TestReadAsync().GetAwaiter().GetResult();
        var remoteReadAudited = application.Audit.Recent(50)
            .Any(row => row.Verb == "api.read" && row.Outcome == "failed");

        report.AppendLine($"remote-read-outcome: {remoteRead}");
        report.AppendLine($"remote-read-text: {healthWindow.ReadText}");
        report.AppendLine($"remote-read-audited: {remoteReadAudited}");
        report.AppendLine($"access-mode: {application.Access.Mode}");
        healthWindow.Close();

        // 9. VSB-02: the toolboxes are registered before any environment is read.
        var toolboxes = TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment.OgToolboxBootstrap.RegisterAll();
        var missingToolboxes = TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment.OgToolboxBootstrap.Missing();
        var wellbore = new TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment.OgWellboreEnvironment();
        var environmentResult = wellbore.Load(null);
        var environmentStore = new TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment.OgEnvironmentStore(wellbore);
        var snapshot = environmentStore.Current();

        report.AppendLine($"toolboxes-registered: {toolboxes}");
        report.AppendLine($"toolboxes-missing: {missingToolboxes.Count}");
        report.AppendLine($"wellbore-environment: {(environmentResult.Success ? "defaults" : "failed")}");
        report.AppendLine($"wellbore-using-defaults: {snapshot.UsingDefaults}");
        report.AppendLine($"wellbore-units: {snapshot.UnitCount}");
        report.AppendLine($"wellbore-findings: {snapshot.Findings.Count}");

        // 10. VSB-03: a real WellboreDocument draws through the host, and the model mapping produces one too.
        var demoDocument = TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging.OgDocumentMapper.TripleComboDemo();
        var demoSummary = TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging.OgDocumentMapper.Summarise(demoDocument);
        var mappedSummary = TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging.OgDocumentMapper.Summarise(
            TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging.OgDocumentMapper.FromWellModel(
                TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging.OgDocumentMapper.BuildDemoWell()));

        report.AppendLine($"wellbore-demo-tracks: {demoSummary.TrackCount}");
        report.AppendLine($"wellbore-demo-depth: {demoSummary.Top:0.#}..{demoSummary.Bottom:0.#} {demoSummary.DepthUnit}");
        report.AppendLine($"wellbore-model-tracks: {mappedSummary.TrackCount}");

        // 11. FND-07: the startup gate is decided from the installation's database, not from a settings file. In a
        //     fresh smoke root no connection exists, so the first run is a setup run; provisioning the local database
        //     makes the connection real but leaves the database empty, which is still a setup run.
        var beforeProbe = application.ProbeInstallationAsync().GetAwaiter().GetResult();
        var before = beforeProbe.Value!;

        var gateBeforeSetup = Setup.OgStartupGate.Decide(
            connectionConfigured: before.ConnectionConfigured,
            companyRecorded: before.CompanyRecorded,
            accountsExist: before.AccountsExist,
            signedIn: application.IsSignedIn);

        var provisioned = application.Connections
            .EnsureLocalAsync(new Data.Connections.OgLocalDatabaseRequest(
                OgPaths.ConnectionName,
                OgPaths.LocalDatabaseDirectory,
                Path.GetFileName(OgPaths.LocalDatabasePath)))
            .GetAwaiter()
            .GetResult();

        var afterProbe = application.ProbeInstallationAsync().GetAwaiter().GetResult();
        var after = afterProbe.Value!;

        var gateEmptyDatabase = Setup.OgStartupGate.Decide(
            connectionConfigured: after.ConnectionConfigured,
            companyRecorded: after.CompanyRecorded,
            accountsExist: after.AccountsExist,
            signedIn: false);

        report.AppendLine($"setup-gate-first-run: {gateBeforeSetup}");
        report.AppendLine($"installation-connection-configured: {before.ConnectionConfigured}");
        report.AppendLine($"installation-connection-provisioned: {provisioned.Success}");
        report.AppendLine($"installation-company-recorded: {after.CompanyRecorded}");
        report.AppendLine($"setup-gate-empty-database: {gateEmptyDatabase}");

        // FND-06: local-first. With no API configured the installation must not pretend to forward.
        var relay = application.FailureRelay;
        report.AppendLine($"failure-forwarding-enabled: {relay.IsEnabled}");
        report.AppendLine($"failure-outbox-pending: {relay.Pending}");
        report.AppendLine($"failure-forwarded-this-session: {relay.Forwarded}");

        var fnd07 = gateBeforeSetup == Setup.OgStartupAction.RunSetup
                    && !before.ConnectionConfigured
                    && provisioned.Success
                    && after.ConnectionConfigured
                    && !after.CompanyRecorded
                    && gateEmptyDatabase == Setup.OgStartupAction.RunSetup
                    && !relay.IsEnabled
                    && relay.Pending == 0;

        // 5e. DAT-02: the module setup plans what it will do without writing anything. The installation's
        //     connection is provisioned first, as first run does (DAT-01), because a plan is refused when the
        //     bound connection cannot be opened (AC-6).
        var setupPage = new Views.ModuleSetupPageView(application);
        var setupPlanned = setupPage.Plan();
        var setupNeedsDemoConfirmation = setupPage.PlanModel?.NeedsDemoConfirmation == true;
        var setupDemoConfirmedByDefault = setupPage.Confirmation.IncludeDemoData;
        var setupDemoModuleListed = setupPage.PlanModel?.Modules
            .Any(module => module.RequiresConfirmation) == true;

        report.AppendLine($"modulesetup-provisioned: {provisioned.Success}");
        report.AppendLine($"modulesetup-planned: {setupPlanned}");
        report.AppendLine($"modulesetup-connection: {setupPage.ConnectionName}");
        report.AppendLine($"modulesetup-modules: {setupPage.Rows.Count}");
        report.AppendLine($"modulesetup-summary: {setupPage.SummaryText}");
        report.AppendLine($"modulesetup-notice: {setupPage.NoticeText}");
        report.AppendLine($"modulesetup-needs-demo-confirmation: {setupNeedsDemoConfirmation}");
        report.AppendLine($"modulesetup-demo-module-listed: {setupDemoModuleListed}");
        report.AppendLine($"modulesetup-demo-confirmed-by-default: {setupDemoConfirmedByDefault}");

        // 5f. DAT-03: the PPDM tree is built from the metadata registry, filters by name and description, and
        //     reads no row count on the first open.
        var ppdmPage = new Views.PpdmTreePageView(application);
        var ppdmLoaded = ppdmPage.LoadAsync().GetAwaiter().GetResult();

        var ppdmTablesOnOpen = ppdmPage.VisibleTables.Count;
        var ppdmCountReadsOnOpen = ppdmPage.CountReads;
        var ppdmCategories = ppdmPage.Root?.ChildNodes.Count ?? 0;
        var ppdmRegistryCount = ppdmPage.Root?.Count ?? 0;

        var ppdmFirstTable = ppdmPage.VisibleTables.FirstOrDefault()?.TableName;
        var ppdmFiltered = string.IsNullOrEmpty(ppdmFirstTable)
            ? []
            : Data.Ppdm.OgPpdmSearch.MatchingTables(ppdmPage.Root!, ppdmFirstTable);

        report.AppendLine($"ppdm-loaded: {ppdmLoaded}");
        report.AppendLine($"ppdm-categories: {ppdmCategories}");
        report.AppendLine($"ppdm-registry-count: {ppdmRegistryCount}");
        report.AppendLine($"ppdm-tables-on-open: {ppdmTablesOnOpen}");
        report.AppendLine($"ppdm-count-reads-on-open: {ppdmCountReadsOnOpen}");
        var ppdmFilterFound = ppdmFiltered.Any(node => node.TableName == ppdmFirstTable);
        report.AppendLine($"ppdm-filter-found: {ppdmFilterFound} ({ppdmFiltered.Count} match(es))");
        report.AppendLine($"ppdm-first-table: {ppdmFirstTable}");
        report.AppendLine($"ppdm-summary: {ppdmPage.SummaryText}");

        // 5g. DAT-03 AC-3 / DAT-04: the tree opens the workbench with the same table and filter, and a failed
        //     read is a failed state rather than an empty grid.
        var ppdmOpened = ppdmFirstTable is not null
                         && ppdmPage.OpenTableAsync(ppdmFirstTable, ppdmFirstTable).GetAwaiter().GetResult();
        var ppdmWorkbenchRequested = application.Pages.Requested.Contains(Views.TableWorkbenchPageView.Route);
        var ppdmWorkbenchNote = ppdmPage.NoticeText;

        // The shell built and showed the workbench during that navigation and consumed the request, so this
        // second page is given its own request to prove the read path in isolation.
        application.Workbench.Pending = new Core.OgTableOpenRequest(ppdmFirstTable!, ppdmFirstTable!);
        var workbenchPage = new Views.TableWorkbenchPageView(application);
        var workbenchOpened = workbenchPage.TableName is not null;
        var workbenchTable = workbenchPage.TableName;
        var workbenchFailedNotBlank = workbenchPage.Rows.Count == 0;
        var workbenchNotice = workbenchPage.NoticeText;

        report.AppendLine($"ppdm-opened-workbench: {ppdmOpened}");
        report.AppendLine($"ppdm-workbench-requested: {ppdmWorkbenchRequested}");
        report.AppendLine($"ppdm-workbench-note: {ppdmWorkbenchNote}");
        report.AppendLine($"workbench-opened-pending: {workbenchOpened}");
        report.AppendLine($"workbench-table: {workbenchTable}");
        report.AppendLine($"workbench-read-failed-not-empty: {workbenchPage.LastReadFailed}");
        report.AppendLine($"workbench-notice: {workbenchNotice}");

        // 5h. DAT-05: the form is generated from metadata, keys and audit columns are read-only, and a mandatory
        //     violation is refused before any write.
        var formDefinition = Data.Ppdm.OgEntityForm.Build(
            "T_SMOKE_FORM",
            [
                new Data.Ppdm.OgFormSourceField("FORM_ID", typeof(string), AllowNull: false),
                new Data.Ppdm.OgFormSourceField("NAME", typeof(string), AllowNull: false, MaxLength: 40),
                new Data.Ppdm.OgFormSourceField("ROW_CHANGED_BY", typeof(string), AllowNull: true),
            ],
            ["FORM_ID"]);

        var formProblems = Data.Ppdm.OgEntityForm.Validate(
            formDefinition,
            new Dictionary<string, string?> { ["FORM_ID"] = "1" },
            isNew: true);

        var formPage = new Views.EntityFormPageView(application);

        report.AppendLine($"form-fields: {formDefinition.Fields.Count}");
        report.AppendLine($"form-key-readonly: {formDefinition.Fields.Single(f => f.Column == "FORM_ID").ReadOnly}");
        report.AppendLine($"form-audit-readonly: {formDefinition.Fields.Single(f => f.Column == "ROW_CHANGED_BY").ReadOnly}");
        report.AppendLine($"form-problem-field: {formProblems.FirstOrDefault()?.Column}");
        report.AppendLine($"form-problem-message: {formProblems.FirstOrDefault()?.Message}");
        report.AppendLine($"form-page-open-with-no-request: {formPage.TableName is null}");

        // 5i. DAT-05 AC-3/AC-5: the reference picker and defaults sources are the owning services, resolved from
        //     the host rather than constructed by the form.
        var formLookupsWired = application.Services.GetService(typeof(Data.Ppdm.IOgFormLookups)) is not null;
        var formDefaultsWired = application.Services.GetService(typeof(Data.Ppdm.IOgFormDefaults)) is not null;

        report.AppendLine($"form-lookups-wired: {formLookupsWired}");
        report.AppendLine($"form-defaults-wired: {formDefaultsWired}");

        // 5j. DAT-06: a quoted CSV is parsed, mapped, and validated row by row - and an invalid row is reported
        //     with its line, column and reason without being written.
        var csvText = "FORM_ID,NAME,DEPTH\n1,Alpha,10.5\n2,,11\n3,Bravo,not-a-number\n";
        var csvDocument = Data.Ppdm.OgCsv.Parse(csvText);
        var csvMapping = Data.Ppdm.OgCsv.SuggestMapping(csvDocument, formDefinition);
        var csvImporter = new Data.Ppdm.OgCsvImporter(new Data.Ppdm.OgCsvPlanOnlyGateway());

        var csvPlan = csvImporter.Preview(
            formDefinition,
            csvDocument,
            csvMapping,
            System.Text.Encoding.UTF8.GetBytes(csvText));

        var csvPage = new Views.CsvImportPageView(application);

        report.AppendLine($"csv-rows: {csvPlan.Rows.Count}");
        report.AppendLine($"csv-acceptable: {csvPlan.Acceptable}");
        report.AppendLine($"csv-rejected: {csvPlan.Rejected}");
        report.AppendLine($"csv-first-rejection: {(csvPlan.Rows.First(r => !r.Acceptable).Problems[0].Column)} at line {csvPlan.Rows.First(r => !r.Acceptable).LineNumber}");
        report.AppendLine($"csv-hash-length: {csvPlan.FileHash.Length}");
        report.AppendLine($"csv-mapping-has-name: {csvPlan.MappingProfile.Contains("NAME", StringComparison.Ordinal)}");
        report.AppendLine($"csv-page-no-table: {csvPage.Plan is null}");

        // 5k. DAT-06: the encoding override and the batch/mode choices are real, not labels.
        var latinBytes = System.Text.Encoding.Latin1.GetBytes("caf".PadRight(3) + "\u00E9");
        var latinDecoded = Data.Ppdm.OgCsv.Decode(latinBytes, Data.Ppdm.OgCsvEncoding.Latin1);
        var utf8Decoded = Data.Ppdm.OgCsv.Decode(latinBytes, Data.Ppdm.OgCsvEncoding.Utf8);

        var csvBatchCount = csvPlan.Rows.Count == 0 ? 0 : (int)Math.Ceiling(csvPlan.Rows.Count / 2d);

        report.AppendLine($"csv-latin1-roundtrip: {latinDecoded.EndsWith('\u00E9')}");
        report.AppendLine($"csv-utf8-differs-on-latin1-bytes: {utf8Decoded != latinDecoded}");
        report.AppendLine($"csv-batches-at-2: {csvBatchCount}");
        report.AppendLine($"csv-modes-available: {string.Join('|', Enum.GetNames<Data.Ppdm.OgImportMode>())}");

        // 5l. DAT-07: a finding names table/key/column/rule, "no rules" differs from "no findings", the severity
        //     order is stable, a failed run keeps the last good timestamp, and a resolution records the actor.
        var qualityForm = Data.Ppdm.OgEntityForm.Build(
            "T_SMOKE_QUALITY",
            [
                new Data.Ppdm.OgFormSourceField("FORM_ID", typeof(string), AllowNull: false),
                new Data.Ppdm.OgFormSourceField("NAME", typeof(string), AllowNull: false, MaxLength: 4),
            ],
            ["FORM_ID"]);

        Data.Ppdm.OgRow QualityRow(string id, string? name) =>
            new(new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["FORM_ID"] = id,
                ["NAME"] = name,
            });

        var qualityRules = Data.Ppdm.OgQualityReviewer.RulesFor(qualityForm);
        var qualityRun = Data.Ppdm.OgQualityReviewer.Run(
            qualityForm,
            [QualityRow("1", null), QualityRow("1", "toolong")],
            qualityRules);

        var qualityNoRules = Data.Ppdm.OgQualityReviewer.Run(qualityForm, [QualityRow("9", "ok")], []);
        var qualityJournal = new Data.Ppdm.OgQualityJournal();
        qualityJournal.Record(qualityRun);
        var qualityGoodTime = qualityJournal.LastSuccess("T_SMOKE_QUALITY");
        qualityJournal.Record(qualityRun with { Succeeded = false, Findings = [] });

        var qualityResolved = Data.Ppdm.OgQualityReviewer.Resolve(
            qualityRun.Findings[0], application.Session.UserId, "smoke");
        var qualityPage = new Views.DataQualityPageView(application);

        report.AppendLine($"quality-findings: {qualityRun.Findings.Count}");
        report.AppendLine($"quality-first-severity: {qualityRun.Findings[0].Severity}");
        report.AppendLine($"quality-first-rule: {qualityRun.Findings[0].Rule}");
        report.AppendLine($"quality-first-key: {qualityRun.Findings[0].Key}");
        report.AppendLine($"quality-no-rules: {qualityNoRules.HasRules}");
        report.AppendLine($"quality-has-findings: {qualityRun.HasFindings}");
        report.AppendLine($"quality-journal-kept: {qualityJournal.LastSuccess("T_SMOKE_QUALITY") == qualityGoodTime}");
        report.AppendLine($"quality-resolved-actor: {qualityResolved.Value?.Actor}");
        report.AppendLine($"quality-page-constructed: {qualityPage.LastRun is null}");

        // 5m. DAT-08: a restore appends a version, the access audit exports exactly the shown rows, never-run and
        //     last-failed differ, a conflict needs an explicit choice, and a broken chain blocks writes.
        var versions = new Data.Ppdm.OgVersionStore();
        var versionOne = versions.Record(
            "T_SMOKE_GOV",
            "1",
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["NAME"] = "Alpha" },
            application.Session.UserId);
        versions.Record(
            "T_SMOKE_GOV",
            "1",
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["NAME"] = "Bravo" },
            application.Session.UserId);

        var restored = versions.Restore("T_SMOKE_GOV", "1", versionOne.Id, application.Session.UserId);
        var govHistory = versions.History("T_SMOKE_GOV", "1");

        var accessAudit = new Data.Ppdm.OgAccessAudit();
        accessAudit.Record(application.Session.UserId, "T_SMOKE_GOV", "none", 2);
        accessAudit.Record("other-user", "T_OTHER", "DEPTH>10", 40);
        var auditQuery = new Data.Ppdm.OgAccessAuditQuery(Table: "T_SMOKE_GOV");
        var auditCsv = accessAudit.ToCsv(auditQuery);

        var neverRun = new Data.Ppdm.OgSyncStatus("company-api", Data.Ppdm.OgSyncState.NeverRun);
        var failed = new Data.Ppdm.OgSyncStatus(
            "company-api",
            Data.Ppdm.OgSyncState.Failed,
            LastAttemptUtc: DateTimeOffset.UtcNow,
            Error: "refused");

        var conflict = new Data.Ppdm.OgSyncConflict(
            "c1",
            "T_SMOKE_GOV",
            "1",
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["NAME"] = "local" },
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["NAME"] = "remote" },
            DateTimeOffset.UtcNow);

        var unresolvedConflict = Data.Ppdm.OgSyncConflicts.Resolve(conflict, null, application.Session.UserId);
        var resolvedConflict = Data.Ppdm.OgSyncConflicts.Resolve(
            conflict,
            Data.Ppdm.OgConflictChoice.KeepLocal,
            application.Session.UserId);

        var chain = new Data.Ppdm.OgAuditChain();
        chain.Append("row.update", "T_SMOKE_GOV/1", application.Session.UserId);
        var intactCheck = chain.Verify();
        chain.Tamper(1, "tampered");
        var brokenCheck = chain.Verify();
        var writesBlocked = !chain.WritesAllowed(brokenCheck, null);
        var writesReopened = chain.WritesAllowed(
            brokenCheck,
            new Data.Ppdm.OgChainAcknowledgement(
                brokenCheck.BrokenAtSequence!.Value,
                application.Session.UserId,
                DateTimeOffset.UtcNow));

        var governancePage = new Views.GovernancePageView(application);

        report.AppendLine($"gov-history: {govHistory.Count}");
        report.AppendLine($"gov-restored-from: {restored.Value?.RestoredFrom == versionOne.Id}");
        report.AppendLine($"gov-history-unchanged: {govHistory[0].Values["NAME"]}");
        report.AppendLine($"gov-audit-export-rows: {auditCsv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length - 1}");
        report.AppendLine($"gov-audit-excludes-other: {!auditCsv.Contains("T_OTHER", StringComparison.Ordinal)}");
        report.AppendLine($"gov-sync-distinct: {neverRun.Describe() != failed.Describe() && neverRun.Badge != failed.Badge}");
        report.AppendLine($"gov-conflict-choice-required: {unresolvedConflict.Reference}");
        report.AppendLine($"gov-conflict-resolved: {resolvedConflict.Value?.Resolution}");
        report.AppendLine($"gov-chain-intact: {intactCheck.IsIntact}");
        report.AppendLine($"gov-chain-broken-at: {brokenCheck.BrokenAtSequence}");
        report.AppendLine($"gov-writes-blocked: {writesBlocked}");
        report.AppendLine($"gov-writes-reopened: {writesReopened}");
        report.AppendLine($"gov-page-constructed: {governancePage.SyncStatus.State}");
        report.AppendLine($"gov-chain-entries: {chain.Entries.Count}");

        // 5n. DAT-08 durable: the application's stores are the SQLite ones, and a version written through the
        //     application is readable by a second instance of the store - the trail outlives the writer.
        var durableVersions = application.Versions is Diagnostics.SqliteGovernanceStore;
        var durableAudit = application.AccessAudit is Diagnostics.SqliteGovernanceStore;
        var durableChain = application.AuditChain is Diagnostics.SqliteGovernanceStore;

        var durableTable = "T_SMOKE_DURABLE_" + Guid.NewGuid().ToString("n")[..6];
        var writtenVersion = application.Versions.Record(
            durableTable,
            "1",
            new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["NAME"] = "Alpha",
                ["DEPTH"] = 12.5m,
            },
            application.Session.UserId);

        application.AccessAudit.Record(application.Session.UserId, durableTable, "none", 1);
        application.AuditChain.Append("smoke.durable", durableTable, application.Session.UserId);

        var reopened = new Diagnostics.SqliteGovernanceStore();
        var reopenedHistory = reopened.History(durableTable, "1");
        var reopenedAudit = reopened.Query(new Data.Ppdm.OgAccessAuditQuery(Table: durableTable));
        var reopenedChainIntact = reopened.Verify().IsIntact;

        report.AppendLine($"gov-durable-versions: {durableVersions}");
        report.AppendLine($"gov-durable-audit: {durableAudit}");
        report.AppendLine($"gov-durable-chain: {durableChain}");
        report.AppendLine($"gov-reopened-history: {reopenedHistory.Count}");
        report.AppendLine($"gov-reopened-value: {reopenedHistory.FirstOrDefault()?.Values["NAME"]}");
        report.AppendLine($"gov-reopened-audit: {reopenedAudit.Count}");
        report.AppendLine($"gov-reopened-chain-intact: {reopenedChainIntact}");
        report.AppendLine($"gov-written-version: {writtenVersion.Version}");

        // 5o. DAT-07 durable: the quality journal is the SQLite one and survives a second instance.
        var durableQuality = application.QualityJournal is Diagnostics.SqliteGovernanceStore;
        var qualityTable = "T_SMOKE_QUALITY_" + Guid.NewGuid().ToString("n")[..6];
        var qualityAt = DateTimeOffset.UtcNow;

        application.QualityJournal.Record(new Data.Ppdm.OgQualityRun(
            qualityTable, qualityAt, qualityAt, 5, 2, Succeeded: true, Findings: []));
        application.QualityJournal.Record(new Data.Ppdm.OgQualityRun(
            qualityTable, qualityAt.AddDays(1), qualityAt.AddDays(1), 0, 0,
            Succeeded: false, Findings: [], Error: "dropped"));

        var reopenedQuality = new Diagnostics.SqliteGovernanceStore().LastSuccess(qualityTable);

        report.AppendLine($"quality-durable-journal: {durableQuality}");
        report.AppendLine($"quality-durable-kept: {reopenedQuality == qualityAt}");

        // 5p. WEL-01: the well index page, the shared well context (selection cancels a load) and saved searches
        //     that survive by being read back from the store.
        var wellPage = new Views.WellSearchPageView(application);
        var wellSearchWithoutTable = wellPage.SearchAsync().GetAwaiter().GetResult();
        var wellSearchFailedState = wellPage.LastSearchFailed;

        var wellContext = application.WellContext;
        var (wellToken, wellVersion) = wellContext.BeginLoad();
        wellContext.Select(new Core.Wells.OgWellSummary(
            "UWI-SMOKE-1", "ALPHA-SMOKE-1", "42-001-99999", "NORTH", "ACTIVE", "OIL",
            new DateTime(2020, 1, 1), null));
        var wellContextCancelled = wellToken.IsCancellationRequested;
        var wellContextStale = !wellContext.IsCurrent(wellVersion);
        var wellContextId = wellContext.Current?.Id;

        var savedWellName = "smoke well search " + Guid.NewGuid().ToString("n")[..6];
        application.SavedWellSearches.Upsert(new Diagnostics.OgSavedWellSearch(
            savedWellName, Text: "ALPHA", Field: "NORTH", SpudYearFrom: 2010));
        var wellSavedBack = application.SavedWellSearches.Load().Searches
            .Any(search => search.Name == savedWellName && search.Field == "NORTH");
        application.SavedWellSearches.Remove(savedWellName);

        report.AppendLine($"wells-page-route: {Views.WellSearchPageView.Route}");
        report.AppendLine($"wells-search-fails-without-table: {wellSearchFailedState && !wellSearchWithoutTable}");
        report.AppendLine($"wells-context-cancel: {wellContextCancelled}");
        report.AppendLine($"wells-context-stale: {wellContextStale}");
        report.AppendLine($"wells-context-id: {wellContextId}");
        report.AppendLine($"wells-saved-search-roundtrip: {wellSavedBack}");

        // 5q. WEL-02: the header page is reachable, status is read-only, a well with no header row fails gracefully,
        //     and the local notes record the coordinate system with who recorded it.
        var headerPage = new Views.WellHeaderPageView(application);
        var headerStatusReadOnly = headerPage.StatusIsReadOnly;

        var headerContextWell = new Core.Wells.OgWellSummary(
            "UWI-SMOKE-1", "ALPHA-SMOKE-1", null, "NORTH", "ACTIVE", "OIL", null, null);
        application.WellContext.Select(headerContextWell);
        var headerOpened = headerPage.OpenPending();
        var headerFailedCleanly = !headerOpened && headerPage.Header is null;

        var locationNote = application.WellHeaderNotes.RecordLocationSystem(
            "UWI-SMOKE-1", "EPSG:4326", application.Session.UserId);
        var locationNoteBack = application.WellHeaderNotes.LocationSystem("UWI-SMOKE-1")?.CoordinateSystem;

        report.AppendLine($"well-header-route: {Views.WellHeaderPageView.Route}");
        report.AppendLine($"well-header-status-readonly: {headerStatusReadOnly}");
        report.AppendLine($"well-header-fails-cleanly-without-row: {headerFailedCleanly}");
        report.AppendLine($"well-header-location-system: {locationNoteBack}");
        report.AppendLine($"well-header-location-actor: {locationNote.Actor}");

        // 5r. WEL-03: the wellbores page opens the context well, shows the "must have at least one" state and fails
        //     cleanly when the table is absent rather than pretending there are none.
        var wellboresPage = new Views.WellboresPageView(application);
        var wellboresFollowedContext = wellboresPage.WellId == "UWI-SMOKE-1";
        var wellboresFailedCleanly = wellboresPage.LastFailed && wellboresPage.Bores.Count == 0;

        application.WellContext.Clear();
        var wellboresBlank = new Views.WellboresPageView(application);
        var wellboresNoContext = wellboresBlank.WellId is null && wellboresBlank.Bores.Count == 0;

        report.AppendLine($"wellbores-route: {Views.WellboresPageView.Route}");
        report.AppendLine($"wellbores-no-context: {wellboresNoContext}");
        report.AppendLine($"wellbores-followed-context: {wellboresFollowedContext}");
        report.AppendLine($"wellbores-fails-cleanly-without-table: {wellboresFailedCleanly}");

        // 5s. WEL-04: the status page follows the context well and fails cleanly without its tables.
        application.WellContext.Select(headerContextWell);
        var statusPage = new Views.WellStatusPageView(application);
        var statusFollowedContext = statusPage.OpenPending();
        // Either the page read the tables and reports the seed state, or it failed cleanly; never a crash and
        // never a pending change.
        var statusFailedCleanly = statusPage.Pending is null
                                  && (statusFollowedContext ? statusPage.ReferenceDataMissing : statusPage.LastFailed);

        report.AppendLine($"well-status-route: {Views.WellStatusPageView.Route}");
        report.AppendLine($"well-status-fails-cleanly-without-table: {statusFailedCleanly}");
        report.AppendLine($"well-status-followed-context: {statusFollowedContext}");
        report.AppendLine($"well-status-last-failed: {statusPage.LastFailed}");
        report.AppendLine($"well-status-reference-missing: {statusPage.ReferenceDataMissing}");
        report.AppendLine($"well-status-notice: {statusPage.NoticeText}");

        // 5t. WEL-05: the stratigraphy page follows the context well; units/columns read whatever the database has.
        var stratigraphyPage = new Views.StratigraphyPageView(application);
        var stratigraphyFollowed = stratigraphyPage.OpenPending();
        var stratigraphyConsistent = stratigraphyFollowed
            ? !stratigraphyPage.LastFailed
            : stratigraphyPage.LastFailed;

        report.AppendLine($"stratigraphy-route: {Views.StratigraphyPageView.Route}");
        report.AppendLine($"stratigraphy-units: {stratigraphyPage.Units.Count}");
        report.AppendLine($"stratigraphy-depth: {stratigraphyPage.HierarchyDepth}");
        report.AppendLine($"stratigraphy-followed-context: {stratigraphyFollowed}");
        report.AppendLine($"stratigraphy-consistent-state: {stratigraphyConsistent}");

        // 5u. WEL-06: the comparison page refuses fewer than two wells and states its overlay position.
        var comparePage = new Views.WellComparePageView(application);
        var compareRefusedOne = !comparePage.CompareAsync().GetAwaiter().GetResult()
                                && comparePage.Matrix is null
                                && comparePage.NoticeText.Contains("at least two", StringComparison.OrdinalIgnoreCase);

        report.AppendLine($"well-compare-route: {Views.WellComparePageView.Route}");
        report.AppendLine($"well-compare-offers-wells: {comparePage.Wells.Count}");
        report.AppendLine($"well-compare-refuses-one: {compareRefusedOne}");
        report.AppendLine($"well-compare-overlay: {comparePage.OverlayText}");

        // 5v. EXP-01: the exploration workspace reports counts, offers only links this account may open, and its
        //     persona lands on it.
        var explorationPage = new Views.ExplorationWorkspacePageView(application);
        var explorationConsistent = explorationPage.LastFailed
            ? explorationPage.Snapshot is null && explorationPage.FunnelRows.Count == 0
            : explorationPage.Snapshot is not null;

        var explorationLinksGated = explorationPage.Snapshot is null
            || explorationPage.AvailableLinkRoutes.All(explorationPage.CanOpen);

        var explorationLanding = Core.OgPersonaLanding.Resolve(
            [Core.OgRoleNames.ExplorationGeologist],
            _ => true);

        report.AppendLine($"exploration-route: {Views.ExplorationWorkspacePageView.Route}");
        report.AppendLine($"exploration-consistent-state: {explorationConsistent}");
        report.AppendLine($"exploration-kpis: {explorationPage.KpisText}");
        report.AppendLine($"exploration-risk: {explorationPage.RiskText}");
        report.AppendLine($"exploration-links-gated: {explorationLinksGated}");
        report.AppendLine($"exploration-persona-route: {explorationLanding.Route}");

        // 5w. EXP-02: the prospect board, its refusal to advance without the process engine, and the drop path
        //     taking the same refusal.
        var boardPage = new Views.ProspectBoardPageView(application);
        var boardConsistent = boardPage.LastFailed
            ? boardPage.Snapshot is null
            : boardPage.Snapshot is not null;

        var sampleCard = new Data.Exploration.OgProspectCard(
            "PROS-SMOKE", "Smoke prospect", "Upper sand", "BASIN-1", "BA-1", "SCREENING", 1,
            DateTime.Today.AddDays(-3), 3, 60m, true, []);
        var advanceRefusal = application.ProspectBoard
            .AdvanceAsync(sampleCard, "APPRAISAL", application.Session.UserId)
            .GetAwaiter().GetResult();
        var dropRefusal = application.ProspectBoard
            .MoveAsync(sampleCard, "APPRAISAL", application.Session.UserId)
            .GetAwaiter().GetResult();

        var advanceRefused = !advanceRefusal.Success
                             && advanceRefusal.Reference == Data.Exploration.OgProspectBoard.CodeAdvanceNotWired;
        var dropSameCheck = dropRefusal.Reference == advanceRefusal.Reference
                            && dropRefusal.Message == advanceRefusal.Message;

        report.AppendLine($"prospect-board-route: {Views.ProspectBoardPageView.Route}");
        report.AppendLine($"prospect-board-consistent-state: {boardConsistent}");
        report.AppendLine($"prospect-board-columns: {boardPage.Columns.Count}");
        report.AppendLine($"prospect-board-cards: {boardPage.Cards.Count}");
        report.AppendLine($"prospect-board-advance-refused: {advanceRefused}");
        report.AppendLine($"prospect-board-drop-same-check: {dropSameCheck}");

        // 5x. EXP-03: the editor opens the selected prospect, holds no prospect when none is selected, and reports
        //     the economic evaluation link by its own permission.
        application.ProspectContext.Clear();
        var editorPage = new Views.ProspectEditorPageView(application);
        var editorEmpty = editorPage.Detail is null && !editorPage.LastFailed;

        application.ProspectContext.Select(new Core.OgProspectSelection("PROS-SMOKE", "Smoke prospect"));
        var editorOpened = editorPage.OpenPending();
        var editorConsistent = editorPage.LastFailed
            ? editorPage.Detail is null
            : editorPage.Detail is not null;

        var economicsAvailable = application.Navigation.CanAccess(
            Data.Exploration.OgProspectEconomicSummary.EvaluationRoute,
            application.Permissions);

        report.AppendLine($"prospect-editor-route: {Views.ProspectEditorPageView.Route}");
        report.AppendLine($"prospect-editor-empty-without-selection: {editorEmpty}");
        report.AppendLine($"prospect-editor-opened: {editorOpened}");
        report.AppendLine($"prospect-editor-consistent-state: {editorConsistent}");
        report.AppendLine($"prospect-editor-chance: {editorPage.ChanceText}");
        report.AppendLine($"prospect-editor-economics-route: {Data.Exploration.OgProspectEconomicSummary.EvaluationRoute}");
        report.AppendLine($"prospect-editor-economics-available: {economicsAvailable}");

        // 5y. EXP-04: the seismic register shows its "no data loading" notice whatever the state, and the map is
        //     off until a survey with coordinates is selected.
        var seismicPage = new Views.SeismicPageView(application);
        var seismicNotice = string.Equals(
            seismicPage.NoticeText,
            Data.Exploration.OgSeismicRegister.NoDataLoadingNotice,
            StringComparison.Ordinal);
        var seismicConsistent = !seismicPage.LastFailed || seismicPage.Surveys.Count == 0;
        var seismicMapDisabled = !seismicPage.MapAvailable;

        report.AppendLine($"seismic-route: {Views.SeismicPageView.Route}");
        report.AppendLine($"seismic-notice-always: {seismicNotice}");
        report.AppendLine($"seismic-consistent-state: {seismicConsistent}");
        report.AppendLine($"seismic-surveys: {seismicPage.Surveys.Count}");
        report.AppendLine($"seismic-map-disabled-without-selection: {seismicMapDisabled}");

        // 5z. EXP-05: the well program gate states that the generic engine is not wired, and the queue is either
        //     read or failed - never both.
        var programPage = new Views.WellProgramPageView(application);
        var programConsistent = !programPage.LastFailed || programPage.Programs.Count == 0;
        var programNotice = programPage.NoticeText.Contains("WFL-04", StringComparison.Ordinal);

        var ruleCard = new Data.Exploration.OgWellProgram(
            "WP-SMOKE", "PROS-SMOKE", "Smoke prospect", "Smoke program", "objective", 1000m, "m", 1m, "USD",
            Data.Exploration.OgWellProgram.StatePending, "author",
            new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero), 3, null, null, null, [], true, 1, []);

        var authorRefusal = application.WellProgramGate
            .DecideAsync(ruleCard, new Data.Exploration.OgWellProgramDecision(
                Data.Exploration.OgWellProgramDecision.Approve, null, [], "author"))
            .GetAwaiter().GetResult();

        var authorBlocked = !authorRefusal.Success
                            && authorRefusal.Reference == Data.Exploration.OgWellProgramApprovals.CodeAuthorCannotDecide;

        report.AppendLine($"well-program-route: {Views.WellProgramPageView.Route}");
        report.AppendLine($"well-program-consistent-state: {programConsistent}");
        report.AppendLine($"well-program-notice-wfl04: {programNotice}");
        report.AppendLine($"well-program-author-blocked: {authorBlocked}");

        var ok = stored is not null
                 && FailureReferenceFormat.IsWellFormed(reference)
                 && constructed == pages
                 && last?.Reference == reference
                 && shellConstructed
                 && landing is { Route: "dashboard", Persona: "Administrator" }
                 && landedRoute == "dashboard"
                 && landingAudited
                 && landingMap.Count == 10
                 && landingMap.Contains("DrillingEngineer->wellpath")
                 && landingMap.Contains("ReservoirEngineer->correlation")
                 && landingMap.Contains("ProductionEngineer->logs")
                 && landingMap.Contains("HseOfficer->hse")
        && landingMap.Contains("DecommissioningCoordinator->decommissioning")
                 && landingMap.Contains("Accountant->export")
        && landingMap.Contains("ExplorationGeologist->exploration")
                 && landingFallback is { Route: "dashboard", IsFallback: true }
                 && benchmark.MeasuredCount >= 5
                 && benchmarkReload is not null
                 && benchmarkPage.Rows.Count == OgBudgets.All.Count
                 && shortSoak.Cycles > 0
                 && provisioned.Success
                 && setupPlanned
                 && setupPage.Rows.Count > 0
                 && ppdmLoaded
                 && ppdmCategories > 0
                 && ppdmRegistryCount == ppdmTablesOnOpen
                 && ppdmCountReadsOnOpen == 0
                 && ppdmFilterFound
                 && ppdmOpened
                 && ppdmWorkbenchRequested
                 && workbenchTable == ppdmFirstTable
                 && workbenchPage.LastReadFailed
                 && formDefinition.Fields.Count == 3
                 && formDefinition.Fields.Single(f => f.Column == "FORM_ID").ReadOnly
                 && formDefinition.Fields.Single(f => f.Column == "ROW_CHANGED_BY").ReadOnly
                 && formProblems.Count == 1
                 && formProblems[0].Column == "NAME"
                 && formProblems[0].Message.Contains("Name", StringComparison.Ordinal)
                 && formPage.TableName is null
                 && formLookupsWired
                 && formDefaultsWired
                 && csvPlan.Rows.Count == 3
                 && csvPlan.Acceptable == 2
                 && csvPlan.Rejected == 1
                 && csvPlan.FileHash.Length == 64
                 && csvPage.Plan is null
                 && latinDecoded.EndsWith("\u00E9", StringComparison.Ordinal)
                 && utf8Decoded != latinDecoded
                 && csvBatchCount == 2
                 && Enum.GetNames<Data.Ppdm.OgImportMode>().Length == 2
                 && qualityRun.Findings.Count == 3
                 && qualityRun.Findings[0].Severity == Data.Ppdm.OgQualitySeverity.Critical
                 && qualityRun.Findings[0].Rule == "duplicate-key"
                 && qualityRun.Findings[0].Key == "1"
                 && !qualityNoRules.HasRules
                 && qualityRun.HasFindings
                 && qualityJournal.LastSuccess("T_SMOKE_QUALITY") == qualityGoodTime
                 && qualityResolved.Value?.Actor == application.Session.UserId
                 && qualityPage.LastRun is null
                 && govHistory.Count == 3
                 && restored.Value?.RestoredFrom == versionOne.Id
                 && govHistory[0].Values["NAME"] is "Alpha"
                 && auditCsv.Split('\n', StringSplitOptions.RemoveEmptyEntries).Length == 2
                 && !auditCsv.Contains("T_OTHER", StringComparison.Ordinal)
                 && neverRun.Describe() != failed.Describe()
                 && neverRun.Badge != failed.Badge
                 && unresolvedConflict.Reference == "conflict-choice-required"
                 && resolvedConflict.Value?.Resolution == Data.Ppdm.OgConflictChoice.KeepLocal
                 && intactCheck.IsIntact
                 && brokenCheck.BrokenAtSequence == 1
                 && writesBlocked
                 && writesReopened
                 && governancePage.SyncStatus.State == Data.Ppdm.OgSyncState.NeverRun
                 && durableVersions
                 && durableAudit
                 && durableChain
                 && reopenedHistory.Count == 1
                 && reopenedHistory[0].Values["NAME"] is "Alpha"
                 && reopenedAudit.Count == 1
                 && reopenedChainIntact
                 && durableQuality
                 && reopenedQuality == qualityAt
        && wellSearchFailedState
        && !wellSearchWithoutTable
        && wellContextCancelled
        && wellContextStale
        && wellContextId == "UWI-SMOKE-1"
        && wellSavedBack
        && headerStatusReadOnly
        && headerFailedCleanly
        && locationNoteBack == "EPSG:4326"
        && locationNote.Actor == application.Session.UserId
        && wellboresNoContext
        && wellboresFollowedContext
        && wellboresFailedCleanly
        && statusFailedCleanly
        && stratigraphyConsistent
        && compareRefusedOne
        && explorationConsistent
        && explorationLinksGated
        && explorationLanding.Route == Views.ExplorationWorkspacePageView.Route
        && boardConsistent
        && advanceRefused
        && dropSameCheck
        && editorEmpty
        && editorConsistent
        && seismicNotice
        && seismicConsistent
        && seismicMapDisabled
        && programConsistent
        && programNotice
        && authorBlocked
                 && setupNeedsDemoConfirmation
                 && setupDemoModuleListed
                 && !setupDemoConfirmedByDefault
                 && shortSoak.PeakMegabytes >= shortSoak.StartMegabytes
                 && payPage.Rows.Count == 2
                 && payPage.Result?.NetReservoir is 3.5
                 && payPage.Result?.NetPay is 2.5
                 && payShifted
                 && payStale
                 && payUndone
                 && payRecomputed
                 && payPage.History.Count > 0
                 && payExcluded.Value?.Zones.All(zone => zone.Excluded) == true
                 && payExcluded.Value.Zones[0].Note!.Contains("GR", StringComparison.Ordinal)
                 && !payOverlap.Success
                 && payOverlap.Reference == WellTools.Bridging.OgPaySummaryWorkspace.CodeOverlap
                 && payExportedCsv
                 && payExportedXlsx
                 && File.Exists(payCsvPath)
                 && File.Exists(payXlsxPath)
                 && new FileInfo(payXlsxPath!).Length > 1_000
                 && paySpliced
                 && paySpliceEntry is not null
                 && !string.IsNullOrWhiteSpace(paySpliceActor)
                 && paySpliceUndone
                 && log100k is { MeasuredMilliseconds: not null }
                 && log100k.Note?.Contains("Decimated", StringComparison.Ordinal) == true
                 && cancelLatency < OgCancellationProbe.BudgetMilliseconds
                 && healthWindow.Content is not null
                 && remoteRead == Services.OgRemoteReadOutcome.NotConfigured
                 && healthWindow.ReadText.Contains("No company API is configured", StringComparison.Ordinal)
                 && remoteReadAudited
                 && failuresPage.Rows.Count > 0
                 && auditPage.Rows.Count > 0
                 && logPage.CurveRows.Count > 0
                 && schematicPage.Strings.Count > 0
                 && schematicPage.Barriers.Count > 0
                 && schematicPage.UnverifiedBarriers.Count > 0
                 && symbolPage.Summary.Symbols > 0
                 && symbolPage.Summary.Categories > 0
                 && symbolPage.Rows.Count > 0
                 && exportPlanned
                 && exportWritten
                 && exportedHeader == "%PDF-"
                 && mapPlanned
                 && mapWritten
                 && mapResult is not null
                 && mapResult.WorldFilePath is not null && File.Exists(mapResult.WorldFilePath)
                 && mapResult.MetadataPath is not null && File.Exists(mapResult.MetadataPath)
                 && mapResult.Crs == "EPSG:32631"
                 && mapWorld is not null
                 && mapWorld.PixelSize > 0
                 && mapWellsInside
                 && correlationPage.Wells.Count == 3
                 && correlationPage.Issues.Count > 0
                 && correlationExported
                 && correlationSection is { Wells.Count: 3 }
                 && correlationPage.PinchOuts > 0
                 && correlationPage.SectionIssues.Any(issue => issue.Code == WellTools.Bridging.OgCorrelation.CodePinchOut)
                 && correlationSectionFrame is not null
                 && correlationSectionFrame.Any(value => value != 0)
                 && supersededResult is null
                 && correlationPage.LastRenderCancelled
                 && latestResult?.Panel.Wells.Count == 3
                 && pathPage.Stations.Count > 0
                 && pathPage.PlanPoints.Count == pathPage.Stations.Count
                 && supersededOffsetResult is null
                 && pathPage.LastComputationCancelled
                 && latestOffsetResult?.Result.Offsets.Count == 1
                 && imageTracks > 0
                 && cementTracks > 0
                 && imageSentence != WellTools.Bridging.OgLogAvailability.NoImageData
                 && cementSentence != WellTools.Bridging.OgLogAvailability.NoCementData
                 && (schematicPage.Tally?.Rows.Count ?? 0) > 0
                 && logFiles.Length > 0
                 && logText.Contains(reference, StringComparison.Ordinal)
                 && imported.Success
                 && licence.SignatureVerified
                 && licence.Customer == "Probe Energy"
                 && licence.Modules.Contains(Core.OgModuleKeys.Wells)
                 && !tampered.Success
                 && afterTamper.Customer == "Probe Energy"
                 && expired is not null
                 && expired.Allows(Core.OgModuleKeys.Wells)
                 && !expired.AllowsWriting(Core.OgModuleKeys.Wells)
                 && licencePage.Rows.Count > 0
                 && lasExample
                 && lasImported
                 && lasExported
                 && lasFindings.Count == 0
                 && lasReload is not null
                 && lasReload.Preview.Standard == "LAS 3.0"
                 && lasReload.Preview.Rows == lasPage.PreviewResult?.Rows
                 && lasDecimated.Decimated
                 && lasDecimated.Rows < 9
                 && witsmlExample
                 && witsmlTrajectory?.Version == "1.4.1.1"
                 && witsmlTrajectory.Stations == 3
                 && witsmlImported
                 && witsmlOneX
                 && witsmlOneXFiles.Count > 0
                 && witsmlReload?.Stations == 3
                 && witsmlTwoOne
                 && witsmlTwoOneFiles.Count > 0
                 && wellFormed21
                 && witsmlLogs is not null
                 && witsmlLogs.Logs == 1
                 && witsmlLogs.Warnings.Any(warning => warning.Contains("time-based", StringComparison.OrdinalIgnoreCase))
                 && storeProbe.unreachable.State == WellTools.Bridging.OgWitsmlStoreState.Unreachable
                 && storeProbe.refused.State == WellTools.Bridging.OgWitsmlStoreState.AuthenticationFailed
                 && !storeEvidence.Contains(probePassword, StringComparison.Ordinal)
                 && osduExported
                 && osduRecords == 3
                 && osduBatches == 3
                 && osduDryRun
                 && osduTokenRejected
                 && osduSent
                 && osduReconciled
                 && osduCandidates > 0
                 && !osduEvidence.Contains("smoke-osdu-token", StringComparison.Ordinal)
                 && chartLine
                 && chartLineMessage == "rendered"
                 && chartPie
                 && chartBar
                 && chartEmptyMessage == "no data"
                 && chartExported
                 && chartSeriesBeforeExport == 2
                 && chartBytes > 5_000
                 && chartSignature.StartsWith("89504E47", StringComparison.Ordinal)
                 && lithologyActive.Contains("sample-company", StringComparison.OrdinalIgnoreCase)
                 && !lithologyFgdc.AsSpan().SequenceEqual(lithologySample)
                 && lithologyFgdc.AsSpan().SequenceEqual(lithologyRestored)
                 && defaultContrast.Passes
                 && adverseContrast.Failures > 0
                 && unnamed.Count == 0
                 && untabbable.Count == 0
                 && paletteMatches.Count > 0
                 && paletteWrapped
                 && !string.IsNullOrWhiteSpace(summaryChart.AccessibleDescription)
                 && summaryChart.AccessibleDescription.Contains("10", StringComparison.Ordinal)
                 && skins > 0
                 && skinsAudited == skins
                 && rtl
                 && toolboxes > 0
                 && missingToolboxes.Count == 0
                 && fnd07;


        report.AppendLine($"result: {(ok ? "ok" : "failed")}");
        return new OgSmokeReport(report.ToString(), ok);
    }

    /// <summary>The theme tokens as they resolve right now, for the REL-03 contrast audit.</summary>
    private static OgThemeColours SmokeThemeColours()
    {
        string Colour(string key) =>
            System.Windows.Application.Current?.TryFindResource(key) is System.Windows.Media.SolidColorBrush brush
                ? $"#{brush.Color.R:X2}{brush.Color.G:X2}{brush.Color.B:X2}"
                : "#FFFFFF";

        return new OgThemeColours(
            Background: Colour("BeepBackgroundBrush"),
            Surface: Colour("BeepSurfaceBrush"),
            SurfaceAlt: Colour("BeepSurfaceAltBrush"),
            TextPrimary: Colour("BeepTextPrimaryBrush"),
            TextSecondary: Colour("BeepTextSecondaryBrush"),
            Accent: Colour("BeepAccentBrush"),
            Danger: Colour("BeepDangerBrush"),
            Success: Colour("BeepSuccessBrush"),
            Warning: Colour("BeepWarningBrush"));
    }

    /// <summary>The control library's semantic brushes as the applied skin resolves them.</summary>
    /// <summary>
    /// REL-03 AC-3: the palette the controls actually draw with. Read through the skin manager, so the audit measures exactly what the
    /// contrast guard wrote - one reader for the guard and for the audit, and no chance of them disagreeing.
    /// </summary>
    private static OgThemeContrast.OgSkinColours ReadSkinColours() => Theme.OgSkins.ReadPalette();

    /// <summary>The audited palette, one line, so the smoke record says which colours were checked.</summary>
    private static string DescribeColours(OgThemeColours colours) =>
        string.Join(
            " ",
            new[]
            {
                $"bg={colours.Background}",
                $"surface={colours.Surface}",
                $"alt={colours.SurfaceAlt}",
                $"text={colours.TextPrimary}",
                $"muted={colours.TextSecondary}",
                $"accent={colours.Accent}",
                $"danger={colours.Danger}",
                $"success={colours.Success}",
                $"warning={colours.Warning}",
            });

    private static byte[] RenderLithology()    {
        using var renderer = new WellTools.Hosting.ViewSurfaceRenderer
        {
            Surface = new TheTechIdea.OilGas.WellLogs.WellLogView
            {
                Document = TheTechIdea.OilGas.WellLogs.WellLogTemplates.CreateTripleComboDemo(),
            },
        };

        renderer.Render(480, 360);
        return renderer.Frame ?? [];
    }

    /// <summary>A reporter that keeps the smoke run's own probe failures out of the real store.</summary>
    private sealed class SmokeFailureReporter : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) =>
            FailureReferenceFormat.New();
    }

    private sealed class SmokeThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException("probe: the store did not answer");
    }

    private sealed class SmokeStatusHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _status;

        public SmokeStatusHandler(HttpStatusCode status) => _status = status;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(_status));
    }
}
