using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

/// <summary>Builds the service graph in the one order that keeps failure reporting available from the first line.</summary>
public sealed class OgBootstrapper
{
    public OgApplication Create()
    {
        // REL-04: the first desktop code that runs starts the startup clock.
        OgStartupTimeline.Begin();

        OgPaths.EnsureDirectories();

        var services = new ServiceCollection();
        services.AddLogging(builder => builder
            .AddDebug()
            .AddProvider(new OgFileLoggerProvider())
            .SetMinimumLevel(LogLevel.Information));

        // Diagnostics first: everything after this can report.
        services.AddSingleton<OgRuntimeContext>();
        services.AddSingleton<IOgFailureStore>(_ => new SqliteFailureStore());
        services.AddSingleton<IFailureStoreAccessor>(sp =>
            new FailureStoreAccessor(sp.GetRequiredService<IOgFailureStore>()));
        services.AddSingleton<IOgAuditStore>(_ => new SqliteAuditStore());
        services.AddSingleton<OgDiagnosticsPipeline>();

        // One pipeline behind all three contracts: a caller asking for the reporter, the audit writer or
        // the concrete pipeline must reach the same instance, or a sign-in writes to a different store
        // than a failure. (The composition root's ValidateOnBuild caught this when OgSignInService was
        // registered by type and IOgAuditService had no registration.)
        services.AddSingleton<TheTechIdeaWeb.Diagnostics.IFailureReporter>(sp =>
            sp.GetRequiredService<OgDiagnosticsPipeline>());
        services.AddSingleton<IOgAuditService>(sp =>
            sp.GetRequiredService<OgDiagnosticsPipeline>());

        // The visible end of the pipeline.
        services.AddSingleton<ILocProvider, OgLoc>();
        services.AddSingleton<IOgToastService, OgToastService>();
        services.AddSingleton<OgNotificationHub>();

        // FND-06: forward failures to the API in CompanyApi mode, through the shared device outbox.
        // Registered as a subscriber rather than a pipeline dependency: the pipeline stays the one fan-in,
        // and there is no cycle (access mode -> audit trail -> pipeline).
        services.AddSingleton<IOgFailureForwarder>(sp =>
        {
            var access = sp.GetRequiredService<IOgServiceAccess>();
            var baseUrl = access.Settings.CompanyApiBaseUrl;

            if (string.IsNullOrWhiteSpace(baseUrl) || !access.Settings.HasUsableToken)
            {
                return new DisabledFailureForwarder();
            }

            var loggers = sp.GetRequiredService<ILoggerFactory>();
            var outbox = new TheTechIdeaWeb.Diagnostics.Device.FailureOutbox(
                OgPaths.FailureOutboxDirectory,
                loggers.CreateLogger<TheTechIdeaWeb.Diagnostics.Device.FailureOutbox>());

            var http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl, UriKind.Absolute),
                Timeout = TimeSpan.FromSeconds(30),
            };

            var delivery = new TheTechIdeaWeb.Diagnostics.Device.FailureDelivery(
                outbox,
                () => http,
                $"{OgPaths.AppRepoName} Desktop {OgPaths.ProductVersion}",
                loggers.CreateLogger<TheTechIdeaWeb.Diagnostics.Device.FailureDelivery>());

            var forwarder = new OgFailureForwarder(
                outbox,
                delivery,
                sp.GetRequiredService<IOgFailureStore>(),
                loggers.CreateLogger<OgFailureForwarder>());

            forwarder.Enable();
            return forwarder;
        });
        // FND-02 AC-3: the first business read. It reads the settings per call, so a mode switch is honoured.
        services.AddSingleton(sp => new Services.OgRemoteReader(
            () => sp.GetRequiredService<IOgServiceAccess>().Settings,
            sp.GetRequiredService<IFailureReporter>()));

        services.AddSingleton<OgFailureRelay>();

        // FND-07: sign-in authenticates against the installation's database; there is no local credential file.
        services.AddSingleton<TheTechIdea.Beep.OilandGas.Desktop.Security.OgSignInService>();

        // SETUP-12: the backup service over the installation's own database. A server database is refused inside it:
        // this machine cannot copy what it cannot see.
        services.AddSingleton<Data.Setup.IOgDatabaseBackup>(sp => new Data.Setup.OgDatabaseBackup(
            sp.GetRequiredService<OgBeepLifetime>().Host,
            sp.GetRequiredService<IFailureReporter>(),
            sp.GetRequiredService<IOgAuditService>(),
            OgPaths.BackupDirectory,
            OgPaths.ConnectionName));

        // FND-07 AC-2: the repair assessment is cheap; the catalog is passed to RepairAsync so that merely
        // asking "is repair needed?" never starts BeepDM.
        services.AddSingleton(_ => new OgRepairService());

        // Shell.
        // REL-02: the licence gate. One instance behind both contracts, so the shell's banner and the
        // navigation's second gate read the same state, and an import updates both.
        services.AddSingleton(sp => new Security.OgLicenceService(
            sp.GetRequiredService<IOgAuditService>(),
            sp.GetRequiredService<IFailureReporter>()));
        services.AddSingleton<IOgEntitlementService>(sp =>
            sp.GetRequiredService<Security.OgLicenceService>());
        services.AddSingleton<OgNavigationService>();

        // DAT-01: the connection catalog, over the BeepDM runtime created on first use.
        services.AddSingleton<OgBeepLifetime>();
        services.AddSingleton<IOgConnectionCatalog>(sp => new BeepDmConnectionCatalog(
            sp.GetRequiredService<OgBeepLifetime>().Host,
            sp.GetRequiredService<OgDiagnosticsPipeline>()));

        // WEL-01: the well index over WELL and WELL_XREF, the shared well context and the saved-search file.
        services.AddSingleton<Core.OgWellContext>();
        services.AddSingleton(sp => new Diagnostics.OgSavedWellSearchStore(
            sp.GetRequiredService<IFailureReporter>()));
        services.AddSingleton<Data.Wells.IOgWellSearch>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Wells.OgWellSearch(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WEL-02: the well header over WELL/WELL_XREF/WELL_BA_SERVICE and its local notes.
        services.AddSingleton(sp => new Diagnostics.OgWellHeaderNotesStore(
            sp.GetRequiredService<IFailureReporter>()));
        services.AddSingleton<Data.Wells.IOgWellHeader>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Wells.OgWellHeaderService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // EXP-05: the well program gate over the LifeCycle-owned WELL_PROGRAM and WELL_PROGRAM_DECISION tables.
        services.AddSingleton<Data.Exploration.IOgWellProgramApprovals>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Exploration.OgWellProgramApprovals(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // EXP-04: the seismic register over SEIS_ACQTN_SURVEY, SEIS_LINE, the status reference tables and
        // SEIS_PROC_SET.
        services.AddSingleton<Data.Exploration.IOgSeismicRegister>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Exploration.OgSeismicRegister(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LSE-01: the lease register over LEASE_ACQUISITION and the three typed lease tables.
        services.AddSingleton<Data.Lease.IOgLeaseRegister>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lease.OgLeaseRegisterService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LSE-02: the acquisition pipeline over LEASE_ACQUISITION and the pipeline/diligence/rights/negotiation
        // records the LeaseAcquisition module owns.
        services.AddSingleton<Data.Lease.IOgLeasePipeline>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lease.OgLeasePipelineService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LSE-03: the financial and lifecycle side of a lease over LEASE_ACQUISITION and the LeaseAcquisition-owned
        // term, obligation, payment and lifecycle records.
        services.AddSingleton<Data.Lease.IOgLeaseFinancials>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lease.OgLeaseFinancialService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WFL-05: the task inbox over the LifeCycle-owned CROSS_PERSONA_TASK table and the LSE-03 lease-expiry
        // projection. It is the one list of everything assigned to the signed-in persona.
        services.AddSingleton<Data.Workflow.IOgTaskInbox>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Workflow.OgTaskInboxService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<Data.Lease.IOgLeaseFinancials>(),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-01: the development persona's landing over the DevelopmentPlanning-owned plans, well schedules,
        // facilities and costs, with drilling programs read for the attention strip.
        services.AddSingleton<Data.Development.IOgDevelopmentWorkspace>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgDevelopmentWorkspace(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-02: the field development plan wizard, review and baseline history.
        services.AddSingleton<Data.Development.IOgFdpWizard>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgFdpWizardService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-03: the design of each planned development well, its casing outline and the recorded casing checks.
        services.AddSingleton<Data.Development.IOgWellDesign>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgWellDesignService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // REL-05: the release acceptance checklist - five items computed from artefacts that exist (the smoke result, the
        // accessibility counters in it, the budgets in it, the project files and the screen sources), the rest attested.
        services.AddSingleton<Data.Release.IOgReleaseAcceptance>(sp =>
            new Data.Release.OgReleaseAcceptanceService(
                sp.GetRequiredService<IFailureReporter>(),
                () => OgReleaseLocations.AcceptanceStorePath,
                () => OgPaths.SmokeResultPath,
                OgReleaseLocations.SourceRoot,
                OgReleaseLocations.PlanFolder,
                () => OgPaths.ProductVersion));

        // REL-01: the installation's health, its uninstall decision and its upgrade record. The probe names the two folders and the
        // version, so the rules never look anything up for themselves and a test can point them at a folder of its own.
        services.AddSingleton<Data.Release.IOgInstallation>(sp =>
            new Data.Release.OgInstallationService(
                () => new Data.Release.OgInstallProbe(
                    AppContext.BaseDirectory,
                    OgPaths.DataRoot,
                    OgPaths.ProductVersion,
                    Environment.ProcessPath),
                sp.GetRequiredService<IFailureReporter>()));

        // RPT-02: the print packs - one document per well plus a manifest, over the well view's own exporter and the reports above.
        services.AddSingleton<Data.Reporting.IOgPrintPacks>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reporting.OgPrintPacksService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>(),
                sp.GetRequiredService<Data.Reporting.IOgReports>());
        });

        // RPT-01: the report catalogue, its templates and its exports. The PDF is the drawing layer's, handed to the service as the
        // pages the preview shows, so the file and the screen cannot differ.
        services.AddSingleton<Data.Reporting.IOgReports>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reporting.OgReportsService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>(),
                () => System.IO.Path.Combine(OgPaths.DataRoot, "Reporting", "Templates"),
                new WellTools.Bridging.OgReportPdf());
        });

        // TRN-03: the sample pack and the optional local training server. The server is off until it is asked for and bound to
        // loopback unless a person confirms otherwise, because it has no sign-in.
        services.AddSingleton<Data.Training.IOgTrainingServer>(sp =>
            new Data.Training.OgTrainingServerService(
                sp.GetRequiredService<IFailureReporter>(),
                () =>
                {
                    TheTechIdea.OilGas.Training.TrainingToolbox.Register();

                    var folders = TheTechIdea.OilGas.Training.TrainingToolbox.Settings.ScenarioFolders;

                    return folders.Count == 0 ? null : folders[0];
                },
                () => System.IO.Path.Combine(OgPaths.DataRoot, "Training", "results.json"),
                () => System.IO.Path.Combine(OgPaths.DataRoot, "Training", "Packs")));

        // TRN-02: the courses, the results ledger and the cohort. It reads the same ledger and the same folder the runner uses, so
        // a course result cannot be computed from a different record than the attempt was written to.
        services.AddSingleton<Data.Training.IOgTrainingCourses>(sp =>
            new Data.Training.OgTrainingCoursesService(
                sp.GetRequiredService<IFailureReporter>(),
                () =>
                {
                    TheTechIdea.OilGas.Training.TrainingToolbox.Register();

                    var folders = TheTechIdea.OilGas.Training.TrainingToolbox.Settings.ScenarioFolders;

                    return folders.Count == 0 ? null : folders[0];
                },
                () => System.IO.Path.Combine(OgPaths.DataRoot, "Training", "results.json")));

        // TRN-01: the scenario runner. It is handed no data source at all: an exercise's well is a JSON file beside its
        // scenario file, so training cannot touch production data. The sample pack lives under the data root, not the
        // customer's environment folder.
        services.AddSingleton<Data.Training.IOgTrainingScenarios>(sp =>
            new Data.Training.OgTrainingScenariosService(
                sp.GetRequiredService<IFailureReporter>(),
                () =>
                {
                    TheTechIdea.OilGas.Training.TrainingToolbox.Register();

                    var folders = TheTechIdea.OilGas.Training.TrainingToolbox.Settings.ScenarioFolders;

                    return folders.Count == 0 ? null : folders[0];
                },
                () => System.IO.Path.Combine(OgPaths.DataRoot, "Training", "Samples"),
                () => System.IO.Path.Combine(OgPaths.DataRoot, "Training", "results.json")));

        // INT-08: the same field in three dimensions, with the glTF export. It takes INT-07's service so the projection list is
        // the map's own rather than a second answer to the same question.
        services.AddSingleton<Data.Interop.IOgScene>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Interop.OgSceneService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>(),
                sp.GetRequiredService<Data.Interop.IOgMap>());
        });

        // INT-07: the mapping engine - wells placed by the survey, gridded, contoured and exported. The wells are read from the
        // PPDM tables the application already opens.
        services.AddSingleton<Data.Interop.IOgMap>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Interop.OgMapService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // INT-06: the field QC run, the duplicate proposals and the mapping gaps - the setup engine is reused for the
        // acceptances, so a proposal has one path to the environment.
        services.AddSingleton<Data.Interop.IOgFieldQc>(sp =>
            new Data.Interop.OgFieldQcService(
                sp.GetRequiredService<IFailureReporter>(),
                sp.GetRequiredService<Data.Interop.IOgWellboreSetup>(),
                () => OgPaths.WellboreEnvironmentPath));

        // INT-05: the customer's wellbore environment, set up through the tools' own setup engine. It edits the file the
        // application itself loads, so a change is the running configuration rather than a second copy of it.
        services.AddSingleton<Data.Interop.IOgWellboreSetup>(sp =>
            new Data.Interop.OgWellboreSetupService(
                sp.GetRequiredService<IFailureReporter>(),
                () => OgPaths.WellboreEnvironmentPath));

        // INT-04: the mapping profiles, their test panel and the write-back. The profiles are customer data, so they live as
        // files under the application's data root rather than inside the build.
        services.AddSingleton<Data.Interop.IOgMappingProfiles>(sp =>
            new Data.Interop.OgMappingProfilesService(
                sp.GetRequiredService<IFailureReporter>(),
                () => System.IO.Path.Combine(OgPaths.DataRoot, "MappingProfiles")));

        // ENG-09: the drilling performance of a well - the time-depth curve, non-productive time and rate of penetration.
        services.AddSingleton<Data.Engineering.IOgDrillingPerformance>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgDrillingPerformanceService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ENG-08: the circulating system, the ECD profile and the rate that reaches a target ECD.
        services.AddSingleton<Data.Engineering.IOgHydraulics>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgHydraulicsService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>(),
                sp.GetRequiredService<Data.Engineering.IOgWellPlan>());
        });

        // ENG-07: torque and drag along the saved well plan, with the string's own limits and the recorded run.
        services.AddSingleton<Data.Engineering.IOgTorqueDrag>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgTorqueDragService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>(),
                sp.GetRequiredService<Data.Engineering.IOgWellPlan>());
        });

        // ENG-06: the directional well plan - its stations, its checks and its handoff trajectory.
        services.AddSingleton<Data.Engineering.IOgWellPlan>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgWellPlanService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ENG-05: the field integrity board over the barrier set, the annulus tests and the joint re-ratings.
        services.AddSingleton<Data.Engineering.IOgIntegrityBoard>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgIntegrityBoardService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ENG-04: the annuli, their MAWOP and MAASP, and the API RP 90 classification of a recorded pressure test.
        services.AddSingleton<Data.Engineering.IOgCasingPressure>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgCasingPressureService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ENG-03: the multi-finger caliper analysis and the re-rating at the wall it leaves, with the result recorded.
        services.AddSingleton<Data.Engineering.IOgMetalLoss>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgMetalLossService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ENG-02: the casing design - API 5C3 ratings per section against the simple load cases, with the run recorded.
        services.AddSingleton<Data.Engineering.IOgCasingDesign>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgCasingDesignService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ENG-01: the anti-collision scan over the recorded directional surveys, with the run recorded as evidence.
        services.AddSingleton<Data.Engineering.IOgAntiCollision>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Engineering.OgAntiCollision(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // VSB-03: a well from the database drawn as a wellbore document (depth track plus its completion intervals).
        services.AddSingleton<Data.Wells.IOgWellboreSource>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Wells.OgWellboreSource(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WFL-06: workflow administration - versions, dependencies, triggers, segregation rules and delegations.
        services.AddSingleton<Data.Workflow.IOgWorkflowAdmin>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Workflow.OgWorkflowAdminService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WFL-04: the gate reviews - evidence, decision, rationale and conditions, with SoD and delegation.
        services.AddSingleton<Data.Workflow.IOgGateReviews>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Workflow.OgGateReviewsService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WFL-03: the process instances with their timeline and history, selected by exact identity.
        services.AddSingleton<Data.Workflow.IOgProcessInstances>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Workflow.OgProcessInstancesService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WFL-02: the definition catalogue - steps, versions, dependencies and cycles - read-only.
        services.AddSingleton<Data.Workflow.IOgDefinitionCatalogue>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Workflow.OgDefinitionCatalogueService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WFL-01: the process dashboard over the owner's definition, instance, step-instance and history tables.
        services.AddSingleton<Data.Workflow.IOgProcessDashboard>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Workflow.OgProcessDashboardService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WRK-04: the contractor register, the permit request, its issue and the execution-start gate.
        services.AddSingleton<Data.Work.IOgPermitsToWork>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Work.OgPermitsToWorkService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WRK-03: maintenance plans, the jobs generated from them and the inspections against their assets.
        services.AddSingleton<Data.Work.IOgMaintenance>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Work.OgMaintenanceService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WRK-02: one work order with its labour and material lines, its rolled-up cost, its transitions and its gates.
        services.AddSingleton<Data.Work.IOgWorkOrderDetail>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Work.OgWorkOrderDetailService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WRK-01: the work-order dashboard over the owner's work-order header, its components and the linked project's
        // recorded state.
        services.AddSingleton<Data.Work.IOgWorkOrders>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Work.OgWorkOrders(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEC-03: facility decommissioning, its versioned cost estimate and its environmental restoration.
        services.AddSingleton<Data.Decommissioning.IOgFacilityCost>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Decommissioning.OgFacilityCostService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEC-02: the well plug-and-abandonment programme, its barrier evidence and its leak paths.
        services.AddSingleton<Data.Decommissioning.IOgWellPa>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Decommissioning.OgWellPaService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEC-01: the decommissioning workspace over the owner's status, restoration and reference tables.
        services.AddSingleton<Data.Decommissioning.IOgDecommissioning>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Decommissioning.OgDecommissioning(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<Data.Workflow.IOgTaskInbox>(),
                sp.GetRequiredService<IFailureReporter>());
        });

        // HSE-07: the permit inbox, the application command and the decision.
        services.AddSingleton<Data.Hse.IOgPermits>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Hse.OgPermits(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // HSE-06: the GHG factor catalog, its review state and the emissions report.
        services.AddSingleton<Data.Hse.IOgGhg>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Hse.OgGhg(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // HSE-05: the compliance calendar and obligation register.
        services.AddSingleton<Data.Hse.IOgCompliance>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Hse.OgCompliance(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // HSE-04: the HAZOP study - nodes, deviations, actions and legal transitions.
        services.AddSingleton<Data.Hse.IOgHazop>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Hse.OgHazop(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // HSE-03: the investigation - assignment, cause chain, corrective actions and closure.
        services.AddSingleton<Data.Hse.IOgInvestigation>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Hse.OgInvestigation(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // HSE-02: the incident register and the report command.
        services.AddSingleton<Data.Hse.IOgIncidents>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Hse.OgIncidents(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // HSE-01: the HSE workspace over the incident, obligation, project and permit-history registers, composed with the
        // WFL-05 inbox.
        services.AddSingleton<Data.Hse.IOgHseWorkspace>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Hse.OgHseWorkspace(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<Data.Workflow.IOgTaskInbox>(),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-01: the dashboard, composed from the seven accounting services above so a count cannot disagree with its
        // own page.
        services.AddSingleton<Data.Accounting.IOgAccountingDashboard>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgAccountingDashboardService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<Data.Accounting.IOgRevenue>(),
                sp.GetRequiredService<Data.Accounting.IOgReceivable>(),
                sp.GetRequiredService<Data.Accounting.IOgPayables>(),
                sp.GetRequiredService<Data.Accounting.IOgLedger>(),
                sp.GetRequiredService<Data.Accounting.IOgTax>(),
                sp.GetRequiredService<Data.Accounting.IOgRoyalties>(),
                sp.GetRequiredService<Data.Accounting.IOgWarehouse>(),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-08: the tax workbench over the configured rates, the tax computations and the deferred balances.
        services.AddSingleton<Data.Accounting.IOgTax>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgTaxService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-07: royalties, joint-interest billing and cost allocation over the owner tables and the ledger.
        services.AddSingleton<Data.Accounting.IOgRoyalties>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgRoyaltiesService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-06: the warehouse over the inventory items, their movements, holds and the stock ledger entries.
        services.AddSingleton<Data.Accounting.IOgWarehouse>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgWarehouseService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-05: payables over the orders, receipts, supplier invoices and payment runs, and the AP matching policy.
        services.AddSingleton<Data.Accounting.IOgPayables>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgPayablesService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-04: receivables over the AR receipts, their allocations and the cash journal.
        services.AddSingleton<Data.Accounting.IOgReceivable>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgReceivableService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-03: revenue over the AR invoices, their lines and credit notes, and the invoice journal.
        services.AddSingleton<Data.Accounting.IOgRevenue>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgRevenueService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ACC-02: the ledger over the ProductionAccounting journals and the accounting period lock.
        services.AddSingleton<Data.Accounting.IOgLedger>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Accounting.OgLedgerService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ECO-03: the investment review queue over LifeCycle's approval workflow rows and the AFE handoff.
        services.AddSingleton<Data.Economics.IOgInvestmentReview>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Economics.OgInvestmentReviewService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ECO-02: AFE management over the ProductionAccounting AFE rows, its lifecycle and its commitments.
        services.AddSingleton<Data.Economics.IOgAfe>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Economics.OgAfeService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // ECO-01: economic evaluation over the EconomicAnalysis decimal money arithmetic and its fiscal regime.
        services.AddSingleton<Data.Economics.IOgEconomics>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Economics.OgEconomicsService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WTA-03: pipeline capacity, regime and erosion over the PipelineAnalysis calculators and its standards.
        services.AddSingleton<Data.WellTest.IOgPipeline>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.WellTest.OgPipelineService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WTA-02: the well test register over the LifeCycle register row, the PPDM reference tables and the analysis link.
        services.AddSingleton<Data.WellTest.IOgWellTestRegister>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.WellTest.OgWellTestRegisterService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WTA-01: well test analysis over the WellTestAnalysis build-up, drawdown and derivative calculators.
        services.AddSingleton<Data.WellTest.IOgWellTest>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.WellTest.OgWellTestService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-08: compressor analysis over the CompressorAnalysis centrifugal, reciprocating and staging calculators.
        services.AddSingleton<Data.Lift.IOgCompressor>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgCompressorService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-07: choke analysis over the ChokeAnalysis gas, multiphase and erosion calculators.
        services.AddSingleton<Data.Lift.IOgChoke>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgChokeService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-06: hydraulic pumps over the HydraulicPumps jet/piston calculators and its findings catalog.
        services.AddSingleton<Data.Lift.IOgHydraulicPump>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgHydraulicPumpService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-05: plunger lift over the PlungerLift velocities and the module's feasibility constants.
        services.AddSingleton<Data.Lift.IOgPlunger>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgPlungerService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-04: sucker rod design over the SuckerRodPumping load, fatigue and card calculations.
        services.AddSingleton<Data.Lift.IOgSuckerRod>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgSuckerRodService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-03: pump performance over the PumpPerformance curve, NPSH and affinity calculations.
        services.AddSingleton<Data.Lift.IOgPump>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgPumpService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-02: gas lift design over the GasLift calculators, spacing and limit catalog.
        services.AddSingleton<Data.Lift.IOgGasLift>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgGasLiftService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // LFT-01: nodal analysis over the NodalAnalysis calculators and the tools cross-check.
        services.AddSingleton<Data.Lift.IOgNodal>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Lift.OgNodalService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // RES-07: production forecasting over the saved decline fits and the forecast records.
        services.AddSingleton<Data.Reservoir.IOgForecast>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reservoir.OgForecastService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // RES-06: decline curve analysis over the ProductionForecasting fit and the tools cross-check.
        services.AddSingleton<Data.Reservoir.IOgDeclineCurve>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reservoir.OgDeclineCurveService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // RES-05: reserves classification, characterization and the disclosure record.
        services.AddSingleton<Data.Reservoir.IOgReserves>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reservoir.OgReservesService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // RES-04: EOR screening over the EnhancedRecovery module constants and its screening rows.
        services.AddSingleton<Data.Reservoir.IOgEorScreening>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reservoir.OgEorScreeningService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // RES-03: the isothermal flash and phase envelope over the FlashCalculations engine.
        services.AddSingleton<Data.Reservoir.IOgFlash>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reservoir.OgFlashService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // RES-02: oil properties over the OilProperties-owned calculators and the OIL_PROPERTY_RESULT row.
        services.AddSingleton<Data.Reservoir.IOgOilProperties>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reservoir.OgOilPropertiesService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // RES-01: gas properties over the GasProperties-owned calculators and the GAS_PROPERTY_RUN row.
        services.AddSingleton<Data.Reservoir.IOgGasProperties>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Reservoir.OgGasPropertiesService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // PRD-06: intervention decisions and the engineer workspace over the PPDM wells and the recorded evidence.
        services.AddSingleton<Data.Production.IOgInterventions>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Production.OgInterventionService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // PRD-05: the volume reconciliation over the measurements, run tickets, booked volumes and PRD-04 runs.
        services.AddSingleton<Data.Production.IOgReconciliation>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Production.OgReconciliationService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // PRD-04: the allocation workbench over the PPDM measurements and the ProductionAccounting allocation rows.
        services.AddSingleton<Data.Production.IOgAllocationWorkbench>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Production.OgAllocationWorkbenchService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // PRD-03: the deferment log over the ProductionOperations PRODUCTION_DEFERMENT and R_DEFERMENT_REASON rows.
        services.AddSingleton<Data.Production.IOgWellStatusDeferment>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Production.OgWellStatusDefermentService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // PRD-02: the register of producing fields and pools over the PPDM FIELD/POOL/PDEN rows.
        services.AddSingleton<Data.Production.IOgFieldPoolRegister>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Production.OgFieldPoolRegisterService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // PRD-01: the daily operations centre over the PPDM measured volumes.
        services.AddSingleton<Data.Production.IOgOperationsCentre>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Production.OgOperationsCentreService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-08: facility construction progress, milestone evidence and the handover to operations.
        services.AddSingleton<Data.Development.IOgConstruction>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgConstructionService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-07: completion tests, the submission and the independent acceptance/handover.
        services.AddSingleton<Data.Development.IOgWellCompletion>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgWellCompletionService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-06: the execution of an approved program, its daily reports and reporting responsibility.
        services.AddSingleton<Data.Development.IOgDrillingExecution>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgDrillingExecutionService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-05: drilling program authoring and its independent review.
        services.AddSingleton<Data.Development.IOgDrillingPrograms>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgDrillProgramService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DEV-04: pools, facility capacity and the facility investment the plan depends on.
        services.AddSingleton<Data.Development.IOgPoolsFacilities>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Development.OgPoolsFacilitiesService(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // EXP-03: the prospect editor over PROSPECT and its trap/reservoir/source/volume/risk/economic tables.
        services.AddSingleton<Data.Exploration.IOgProspectEditor>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Exploration.OgProspectEditor(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // EXP-02: the prospect maturation board over PROSPECT/PLAY/PROSPECT_WORKFLOW_STAGE.
        services.AddSingleton<Core.OgProspectContext>();
        services.AddSingleton<Data.Exploration.IOgProspectBoard>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Exploration.OgProspectBoard(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // EXP-01: the exploration workspace over PROSPECT/PLAY/LEAD/WELL.
        services.AddSingleton<Data.Exploration.IOgExplorationWorkspace>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Exploration.OgExplorationWorkspace(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WEL-06: the comparison over WELL and the well's picks.
        services.AddSingleton<Data.Wells.IOgWellComparison>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Wells.OgWellComparison(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WEL-05: the stratigraphy over STRAT_UNIT/STRAT_COLUMN/STRAT_WELL_SECTION.
        services.AddSingleton<Data.Wells.IOgStratigraphy>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Wells.OgStratigraphy(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // WEL-04: the well status over WELL_STATUS/R_WELL_STATUS, audited through the application audit service.
        services.AddSingleton<Data.Wells.IOgWellStatus>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;
            var audit = sp.GetRequiredService<IOgAuditService>();

            return new Data.Wells.OgWellStatus(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>(),
                (verb, id, outcome, detail) => audit.Add(verb, "well", id, outcome, detail));
        });

        // WEL-03: the wellbores over the LifeCycle-owned WELLBORE and WELLBORE_SURVEY tables.
        services.AddSingleton<Data.Wells.IOgWellbores>(sp =>
        {
            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Wells.OgWellbores(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                sp.GetRequiredService<IFailureReporter>());
        });

        // DAT-08: row history, read audit and the tamper-evident chain live in the local SQLite file beside the
        // failure and audit stores, so the trail outlives the process. One instance, so a page that records a
        // version and a page that lists history see the same history.
        services.AddSingleton<Diagnostics.SqliteGovernanceStore>();
        services.AddSingleton<Data.Ppdm.IOgVersionStore>(sp =>
            sp.GetRequiredService<Diagnostics.SqliteGovernanceStore>());
        services.AddSingleton<Data.Ppdm.IOgAccessAudit>(sp =>
            sp.GetRequiredService<Diagnostics.SqliteGovernanceStore>());
        services.AddSingleton<Data.Ppdm.IOgAuditChain>(sp =>
            sp.GetRequiredService<Diagnostics.SqliteGovernanceStore>());
        services.AddSingleton<Data.Ppdm.OgChainGate>();
        services.AddSingleton<Data.Ppdm.IOgQualityJournal>(sp =>
            sp.GetRequiredService<Diagnostics.SqliteGovernanceStore>());

        // DAT-05 AC-3/AC-5: reference pickers and defaults come from the owning services, not from the form.
        // The construction lives in .Data, which is the only project allowed to hold BeepDM types.
        services.AddSingleton<Data.Ppdm.IOgFormLookups>(sp => Data.Ppdm.OgFormSourceFactory.CreateLookups(
            sp.GetRequiredService<OgBeepLifetime>().Host,
            MetadataService(sp),
            OgPaths.ConnectionName));

        services.AddSingleton<Data.Ppdm.IOgFormDefaults>(sp => Data.Ppdm.OgFormSourceFactory.CreateDefaults(
            sp.GetRequiredService<OgBeepLifetime>().Host,
            MetadataService(sp),
            OgPaths.ConnectionName));
        // DAT-04: the table workbench gateway, over the connection's datasource and the metadata registry.
        services.AddSingleton<Core.OgPageNavigator>();
        services.AddSingleton<Core.OgWorkbenchNavigation>();
        services.AddSingleton<Data.Ppdm.IOgPpdmTableGateway>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger<global::Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService>();

            var metadata = new global::Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService(
                logger,
                contributors: []);

            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Ppdm.OgPpdmTableGateway(
                () => host.Editor.GetDataSource(OgPaths.ConnectionName),
                metadata,
                sp.GetRequiredService<IFailureReporter>(),
                lookups: sp.GetRequiredService<Data.Ppdm.IOgFormLookups>(),
                defaults: sp.GetRequiredService<Data.Ppdm.IOgFormDefaults>());
        });

        // DAT-03: the PPDM tree, over the metadata registry. The row count is read only when the page asks for
        // one table, and the table name is validated before it reaches SQL.
        services.AddSingleton<Data.Ppdm.IOgPpdmTreeBuilder>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger<global::Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService>();

            var metadata = new global::Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService(
                logger,
                contributors: []);

            var host = sp.GetRequiredService<OgBeepLifetime>().Host;

            return new Data.Ppdm.OgPpdmTreeBuilder(
                metadata,
                sp.GetRequiredService<IFailureReporter>(),
                rowCount: (table, _) => Task.FromResult(RowCount(host, table)));
        });

        // DAT-02: the module-setup runner over the same BeepDM host the connections use. One instance, so a
        // plan and an apply see the same connection and the same state file.
        services.AddSingleton<Data.Setup.IOgModuleSetupRunner>(sp => new Data.Setup.OgModuleSetupRunner(
            new Data.Setup.OgSetupComposition(
                sp.GetRequiredService<OgBeepLifetime>().Host,
                sp.GetRequiredService<IFailureReporter>(),
                OgPaths.ConnectionName),
            new Data.Setup.OgSetupStateStore(OgPaths.ModuleSetupStatePath),
            sp.GetRequiredService<IFailureReporter>(),
            "desktop",
            // The schema change takes a real copy of this file as its backup evidence.
            () => OgPaths.LocalDatabasePath));

        // RPT-03: one chart renderer for the whole desktop, behind the domain's IRenderingService.
        services.AddSingleton<WellTools.Reporting.OgSkiaChartRenderer>();

        // VSB-09: PDF / SVG / PNG export through the tools' own WellboreExport.
        services.AddSingleton(sp => new TheTechIdea.Beep.OilandGas.Desktop.WellTools.Export.OgExportService(
            sp.GetRequiredService<IFailureReporter>()));

        // VSB-02: the wellbore environment, created on first use, after the toolboxes are registered.
        services.AddSingleton<OgWellboreLifetime>();
        services.AddSingleton<TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment.OgEnvironmentStore>(sp =>
            new TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment.OgEnvironmentStore(
                sp.GetRequiredService<OgWellboreLifetime>().Environment));

        // FND-02: how this installation reaches its data. Auto by default, resolved every time it is asked.
        services.AddSingleton<IOgServiceAccess>(sp => new OgServiceAccess(
            new OgServiceAccessSettings(
                Preference: OgAccessPreference.Auto,
                CompanyApiBaseUrl: null,
                HasUsableToken: false,
                LocalConnectionName: OgPaths.ConnectionName,
                LocalDatabaseExists: File.Exists(OgPaths.LocalDatabasePath)),
            sp.GetRequiredService<IOgAuditService>(),
            sp.GetRequiredService<IFailureReporter>(),
            localDatabasePresent: _ => File.Exists(OgPaths.LocalDatabasePath)));

        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        var runtime = provider.GetRequiredService<OgRuntimeContext>();
        var permissions = CreatePermissions();
        var session = CreateSession(runtime, permissions);

        // Wire the hub to the pipeline exactly once.
        var pipeline = provider.GetRequiredService<OgDiagnosticsPipeline>();
        var hub = provider.GetRequiredService<OgNotificationHub>();
        pipeline.Published += (_, published) => hub.Publish(published);

        // Resolve the relay once so it subscribes; the provider keeps the singleton alive.
        _ = provider.GetRequiredService<OgFailureRelay>();

        // REL-04: the first number the startup budget needs. Marked once; the first mark wins.
        OgStartupTimeline.Mark("bootstrap");

        return new OgApplication(provider, session, permissions);
    }

    private static OgSession CreateSession(OgRuntimeContext runtime, OgPermissions permissions)
    {
        var session = new OgSession(
            connectionName: OgPaths.ConnectionName,
            companyId: OgPaths.DefaultCompanyId,
            companyName: OgPaths.DefaultCompanyName,
            fieldId: OgPaths.DefaultFieldId,
            fieldName: OgPaths.DefaultFieldName,
            userId: "stage0-user",
            userDisplayName: "Stage 0 Operator",
            cultureName: "en-US",
            timeZoneId: TimeZoneInfo.Local.Id,
            baseCurrencyCode: "USD",
            bookCode: OgPaths.DefaultBookCode,
            periodId: "PERIOD-0001",
            periodName: "Current period",
            mode: OgDeploymentMode.Personal,
            personaCode: OgPersonaLanding.PersonaFor(permissions.Roles));

        runtime.UserId = session.UserId;
        runtime.CompanyId = session.CompanyId;
        runtime.FieldId = session.FieldId;

        return session;
    }

    private static OgPermissions CreatePermissions() =>
        new(
            roles: [OgRoleNames.SystemAdmin, OgRoleNames.TenantAdmin],
            permissions: [],
            permissionSourceAvailable: true);
    /// <summary>One table's row count, or null when it cannot be read. The name is validated as an identifier.</summary>
    private static int? RowCount(Data.Bootstrapping.OgBeepHost host, string table)
    {
        if (string.IsNullOrWhiteSpace(table) || !System.Text.RegularExpressions.Regex.IsMatch(table, "^[A-Za-z_][A-Za-z0-9_]*$"))
        {
            return null;
        }

        var source = host.Editor.GetDataSource(OgPaths.ConnectionName);

        if (source is null)
        {
            return null;
        }

        // The rows are counted through the datasource's entity API, not a statement written here.
        var rows = source.GetEntity(table, new List<TheTechIdea.Beep.Report.AppFilter>());

        if (source.ErrorObject is { } error
            && !string.Equals(error.Flag.ToString(), "Ok", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return rows.Count();
    }
    private static global::Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository MetadataService(IServiceProvider sp) =>
        new global::Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService(
            sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger<global::Beep.OilandGas.PPDM39.DataManagement.Core.Metadata.PPDMMetadataService>(),
            contributors: []);


}
