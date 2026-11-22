using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

/// <summary>
/// The composition root. A view model receives this or a service interface — never the provider.
/// </summary>
public sealed class OgApplication : IDisposable
{
    private readonly ServiceProvider _provider;

    public OgApplication(ServiceProvider provider, OgSession session, OgPermissions permissions)
    {
        _provider = provider;
        Session = session;
        Permissions = permissions;
    }

    public OgSession Session { get; private set; }

    /// <summary>False until a sign-in succeeds; the shell is only created after it is true.</summary>
    public bool IsSignedIn { get; private set; }

    /// <summary>
    /// True when the account signed in with a password somebody else typed: the setup left a marker, and the shell
    /// is not created until its owner has replaced it (SETUP-03 AC-4).
    /// </summary>
    public bool MustChangePassword { get; private set; }

    /// <summary>Replaces the signed-in account's password and clears the setup's marker.</summary>
    public async Task<OgResult> ChangePasswordAsync(
        string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (!IsSignedIn)
        {
            return OgResult.Failed(
                "There is no signed-in account to change the password of.",
                Data.Security.OgDatabaseIdentityCodes.Refused);
        }

        var changed = await DatabaseIdentity()
            .ChangePasswordAsync(Session.UserId, newPassword, cancellationToken)
            .ConfigureAwait(false);

        if (!changed.Success)
        {
            return changed;
        }

        MustChangePassword = false;
        Notifications.Success("Settings.PasswordChanged");
        return changed;
    }

    /// <summary>
    /// The installed database and what it holds, probed from the database itself. Null until
    /// <see cref="ProbeInstallationAsync"/> has run.
    /// </summary>
    /// <remarks>
    /// Nothing about the installation is read from a settings file. The only fact on this machine is the connection
    /// that reaches the database (<c>OgPaths.ConnectionName</c>); whether the installation is personal or a company
    /// one, and whether it has a company record and accounts, is answered by opening it.
    /// </remarks>
    public Data.Setup.OgInstallation? InstalledDatabase { get; private set; }

    /// <summary>
    /// The installation as the in-memory shape the session, the wizard and the settings page read. It is assembled
    /// from <see cref="Installation"/> on each read and is never persisted: <see cref="Setup.OgSetupState"/> is not
    /// written to disk anywhere.
    /// </summary>
    public Setup.OgSetupState? SetupState => InstalledDatabase is null ? null : StateOf(InstalledDatabase.Company);

    /// <summary>
    /// Probes the installation: is the connection configured, does it open, does its database hold a company record
    /// and an account directory? A machine with no connection is <b>not</b> a failure — it is a first run.
    /// </summary>
    public async Task<OgResult<Data.Setup.OgInstallation>> ProbeInstallationAsync(
        CancellationToken cancellationToken = default)
    {
        var probe = new Data.Setup.OgInstallationProbe(
            Connections,
            SharedSettings,
            connection => DatabaseIdentity(connection));

        var result = await probe.ProbeAsync(OgPaths.ConnectionName, cancellationToken).ConfigureAwait(false);

        if (result.Success && result.Value is not null)
        {
            InstalledDatabase = result.Value;
        }

        return result;
    }

    /// <summary>
    /// Signs in for whichever deployment is in force: the company database's own account directory, or this
    /// workstation's credential file. The roles the company database grants replace the workstation's default, so
    /// a company user gets exactly the pages their RBAC row allows.
    /// </summary>
    public async Task<OgResult<OgSession>> SignInAsync(
        string userName,
        string password,
        CancellationToken cancellationToken = default)
    {
        var result = await _provider.GetRequiredService<Security.OgSignInService>()
            .SignInAsync(userName, password, DatabaseIdentity(), cancellationToken)
            .ConfigureAwait(false);

        if (!result.Success || result.Value is null)
        {
            return OgResult<OgSession>.Failed(
                result.Message ?? "Sign-in failed.",
                result.Reference ?? Data.Security.OgDatabaseIdentityCodes.Refused);
        }

        Session = result.Value.Session;
        IsSignedIn = true;

        if (InstalledDatabase is not null)
        {
            InstalledDatabase = InstalledDatabase with { AccountsExist = true };
        }

        MustChangePassword = result.Value.MustChangePassword;

        // The company database's grant replaces the workstation default: a page the RBAC row does not allow stays
        // closed even though the person is signed in.
        Permissions = new OgPermissions(
            MapRoles(result.Value.Roles),
            result.Value.GrantedPermissions,
            permissionSourceAvailable: true);

        return OgResult<OgSession>.Ok(Session);
    }

    /// <summary>
    /// The app's page gates use <see cref="OgRoleNames"/>; the database seeds <c>Administrator</c>/<c>Admin</c>.
    /// The mapping is explicit so a role that grants nothing in the database does not silently become admin.
    /// </summary>
    private static IReadOnlyList<string> MapRoles(IReadOnlyList<string> databaseRoles)
    {
        var mapped = new List<string>();

        foreach (var role in databaseRoles)
        {
            mapped.Add(role);

            if (string.Equals(role, "Administrator", StringComparison.OrdinalIgnoreCase)
                || string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                mapped.Add(OgRoleNames.SystemAdmin);
            }
        }

        return mapped.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>
    /// The account directory: the installed database. Both deployments authenticate there — a personal installation
    /// against its own local database, a company installation against the server's.
    /// </summary>
    public Data.Security.IOgDatabaseIdentity DatabaseIdentity(string? connectionName = null) =>
        new Data.Security.BeepDmDatabaseIdentity(
            _provider.GetRequiredService<Services.OgBeepLifetime>().Host,
            _provider.GetRequiredService<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),
            connectionName
            ?? InstalledDatabase?.ConnectionName
            ?? OgPaths.ConnectionName);

    /// <summary>Records the sign-out and drops the session.</summary>
    public void SignOut()
    {
        if (IsSignedIn)
        {
            _provider.GetRequiredService<Security.OgSignInService>().SignOut(Session);
        }

        IsSignedIn = false;
    }

    public OgPermissions Permissions { get; private set; }

    public IServiceProvider Services => _provider;

    public OgDiagnosticsPipeline Diagnostics => _provider.GetRequiredService<OgDiagnosticsPipeline>();

    public OgNotificationHub Notifications => _provider.GetRequiredService<OgNotificationHub>();

    public OgNavigationService Navigation => _provider.GetRequiredService<OgNavigationService>();

    public ILocProvider Loc => _provider.GetRequiredService<ILocProvider>();

    public OgRuntimeContext RuntimeContext => _provider.GetRequiredService<OgRuntimeContext>();

    public IOgFailureStore Failures => _provider.GetRequiredService<IOgFailureStore>();

    public IOgAuditStore Audit => _provider.GetRequiredService<IOgAuditStore>();

    /// <summary>The audited write path, which stamps the signed-in actor and company on every row.</summary>
    public IOgAuditService AuditLog => _provider.GetRequiredService<IOgAuditService>();

    /// <summary>DAT-01. Resolving this creates the BeepDM runtime on first use.</summary>
    public Data.Connections.IOgConnectionCatalog Connections =>
        _provider.GetRequiredService<Data.Connections.IOgConnectionCatalog>();

    /// <summary>VSB-02. Resolving this registers the toolboxes and loads the environment on first use.</summary>
    public WellTools.Environment.OgWellboreEnvironment Wellbore =>
        _provider.GetRequiredService<Services.OgWellboreLifetime>().Environment;

    /// <summary>The environment findings for the settings and diagnostics pages.</summary>
    public WellTools.Environment.OgEnvironmentStore Environment =>
        _provider.GetRequiredService<WellTools.Environment.OgEnvironmentStore>();

    /// <summary>Re-reads the configured wellbore environment file (defaults when there is none).</summary>
    public OgResult ReloadWellboreEnvironment() =>
        _provider.GetRequiredService<Services.OgWellboreLifetime>().Reload();

    /// <summary>The lifetime behind <see cref="Wellbore"/>; it announces reloads and preset changes.</summary>
    public Services.OgWellboreLifetime WellboreLifetime =>
        _provider.GetRequiredService<Services.OgWellboreLifetime>();

    /// <summary>FND-02: how this installation reaches its data, and its health.</summary>
    public IOgServiceAccess Access => _provider.GetRequiredService<IOgServiceAccess>();

    /// <summary>RPT-03: the desktop's chart renderer (the domain's IRenderingService).</summary>
    public WellTools.Reporting.OgSkiaChartRenderer Charts =>
        _provider.GetRequiredService<WellTools.Reporting.OgSkiaChartRenderer>();

    /// <summary>VSB-09: PDF / SVG / PNG export of a well visual.</summary>
    public WellTools.Export.OgExportService Export =>
        _provider.GetRequiredService<WellTools.Export.OgExportService>();

    /// <summary>SETUP-12: copies the installation's database to a dated file, and restores from one.</summary>
    public Data.Setup.IOgDatabaseBackup Backup =>
        _provider.GetRequiredService<Data.Setup.IOgDatabaseBackup>();

    /// <summary>FND-07 AC-2: detects and repairs a missing local database.</summary>
    public OgRepairService Repair => _provider.GetRequiredService<OgRepairService>();

    /// <summary>
    /// The BeepDM provider, for the shared data controls (the connection catalogue, its editor and the two
    /// downloaders) that resolve the editor themselves. Typed as <see cref="IServiceProvider"/> so the editor's
    /// own type stays inside the data layer.
    /// </summary>
    public IServiceProvider DataServices =>
        _provider.GetRequiredService<Services.OgBeepLifetime>().Host.BeepServices;

    /// <summary>
    /// The installation-wide settings held in the company database, over <paramref name="connectionName"/>. The
    /// store is built per call because the connection is chosen during setup, not at composition time.
    /// </summary>
    public Data.Settings.OgSharedSetupService SharedSettings(string connectionName) =>
        new(
            new Data.Settings.BeepDmSharedSetupStore(
                _provider.GetRequiredService<Services.OgBeepLifetime>().Host,
                _provider.GetRequiredService<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),
                connectionName),
            _provider.GetRequiredService<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),
            AuditLog);

    /// <summary>
    /// The outcome of the last <see cref="LoadSettingsAsync"/>. The shell shows why the company settings are
    /// unavailable without pretending the installation has none.
    /// </summary>
    public OgResult<Data.Settings.OgSharedSettingsSnapshot>? Settings { get; private set; }

    /// <summary>
    /// Reads the installation's settings from its database and applies them: the company, region, calendar and
    /// currency go onto the session, the wellbore environment comes from the database, and the culture is applied to
    /// the interface.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Called <b>after</b> sign-in, for both deployments: the settings are rows in the installation's database and are
    /// the signed-in person's to read. There is no local copy to fall back on, which is the point — a setting that
    /// existed on one workstation would be a second truth.
    /// </para>
    /// <para>
    /// A failure does not stop the application. The shell opens, a notice carries the reference, and the settings
    /// page says what is unavailable rather than showing values nobody recorded.
    /// </para>
    /// </remarks>
    public async Task<OgResult<Data.Settings.OgSharedSettingsSnapshot>> LoadSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        if (InstalledDatabase is null)
        {
            Settings = OgResult<Data.Settings.OgSharedSettingsSnapshot>.Failed(
                "The installation has not been probed, so there is no database to read the settings from.",
                "settings-not-probed");

            return Settings;
        }

        if (!IsSignedIn)
        {
            // Settings are the signed-in person's to read. Asking the database for them before anybody has
            // authenticated would read a company's configuration for an anonymous caller.
            Settings = OgResult<Data.Settings.OgSharedSettingsSnapshot>.Failed(
                "The settings are read after sign-in; nobody is signed in yet.",
                "settings-not-authenticated");

            return Settings;
        }

        var connectionName = InstalledDatabase.ConnectionName;

        try
        {
            // A database written by a newer build is refused before anything is read: an older build must not write
            // rows it does not understand (SETUP-11 AC-3). A database written by an older one is migrated first, so
            // what follows reads the shape this build expects.
            var version = await SharedSettings(connectionName)
                .ReadSchemaVersionAsync(cancellationToken)
                .ConfigureAwait(false);

            if (version.Success
                && OgSetupCompatibility.Verdict(version.Value) == OgSetupCompatibilityVerdict.WrittenByNewerBuild)
            {
                Settings = OgResult<Data.Settings.OgSharedSettingsSnapshot>.Failed(
                    $"This database was set up by a newer build (settings version {version.Value}; this build understands {OgSetupCompatibility.CurrentSchemaVersion}). Install the newer build.",
                    OgSetupCompatibility.CodeDatabaseNewer);

                return Settings;
            }

            if (version.Success
                && OgSetupCompatibility.Verdict(version.Value) == OgSetupCompatibilityVerdict.NeedsMigration)
            {
                var migrated = await MigrateSettingsAsync(cancellationToken).ConfigureAwait(false);

                if (!migrated.Success)
                {
                    // The previous shape is still readable by this build in practice, so a failed migration is
                    // reported and the settings below are read as they are - but the person is told.
                    Notifications.Publish(new Diagnostics.OgDiagnosticEvent(
                        migrated.Reference ?? "setup-migration-failed",
                        FailureSeverity.Degraded,
                        migrated.Message ?? "The installation's settings could not be migrated."));
                }
            }

            var read = await SharedSettings(connectionName).ReadAsync(cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                Settings = OgResult<Data.Settings.OgSharedSettingsSnapshot>.Failed(
                    read.Message ?? "The settings could not be read.",
                    read.Reference ?? Data.Settings.OgSharedSetupCodes.ReadFailed);

                return Settings;
            }

            var snapshot = read.Value;

            if (snapshot.Installation is { } company)
            {
                AdoptCompany(company);
            }

            // The environment is the database's document for both deployments: that is what makes every user's
            // units, colours and printing the same.
            var environment = WellboreLifetime.LoadSharedDocument(snapshot.ToolsEnvironmentJson);

            if (!environment.Success)
            {
                Notifications.Publish(new Diagnostics.OgDiagnosticEvent(
                    environment.Reference ?? "environment-shared-failed",
                    FailureSeverity.Degraded,
                    "the shared wellbore environment could not be applied"));
            }

            Settings = read;
            return Settings;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            var reference = Diagnostics.ReportHandled(
                ex,
                $"loading the settings from '{connectionName}'",
                "the application opened with what it already had",
                FailureSeverity.Degraded);

            Settings = OgResult<Data.Settings.OgSharedSettingsSnapshot>.Failed(
                $"The settings could not be read. Reference: {reference}",
                Data.Settings.OgSharedSetupCodes.ReadFailed);

            return Settings;
        }
    }

    /// <summary>
    /// Brings an installation created by an earlier build to this build's settings shape. Runs at startup from
    /// <see cref="LoadSettingsAsync"/>; public so the settings page and tests can ask for it directly.
    /// </summary>
    public async Task<OgResult<Data.Setup.OgSetupMigrationOutcome>> MigrateSettingsAsync(
        CancellationToken cancellationToken = default)
    {
        if (InstalledDatabase is null)
        {
            return OgResult<Data.Setup.OgSetupMigrationOutcome>.Failed(
                "The installation has not been probed, so there is no database to migrate.",
                "settings-not-probed");
        }

        if (!IsSignedIn)
        {
            return OgResult<Data.Setup.OgSetupMigrationOutcome>.Failed(
                "The settings are migrated after sign-in; nobody is signed in yet.",
                "settings-not-authenticated");
        }

        var migration = new Data.Setup.OgSetupMigration(
            SharedSettings(InstalledDatabase.ConnectionName),
            Diagnostics,
            AuditLog);

        return await migration
            .MigrateAsync(Session.UserId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Makes a company record current for what is already running: the installation keeps it in memory, the session
    /// keeps the signed-in person but takes the company, field, culture, currency and data path, and the interface
    /// language follows.
    /// </summary>
    /// <remarks>
    /// Nothing is written to disk. A company record that came from the database or from the settings page is already
    /// stored where it belongs; this is the running application catching up with it.
    /// </remarks>
    public void AdoptCompany(Data.Settings.OgInstallationSettings company)
    {
        ArgumentNullException.ThrowIfNull(company);

        if (InstalledDatabase is null)
        {
            return;
        }

        InstalledDatabase = InstalledDatabase with { Company = company, CompanyRecorded = true };

        if (IsSignedIn)
        {
            Session = Security.OgSignInService.Adopt(Session, StateOf(company));
        }

        ApplyCulture(company);
    }

    /// <summary>The installation's machine facts and its company record as the in-memory setup shape.</summary>
    private Setup.OgSetupState StateOf(Data.Settings.OgInstallationSettings? company)
    {
        var installation = InstalledDatabase!;

        return new Setup.OgSetupState(
            Completed: installation.CanSignIn,
            CompanyName: company?.CompanyName ?? string.Empty,
            DataRoot: OgPaths.DataRoot,
            AdministratorUserName: Session.UserId,
            CompletedAtUtc: DateTimeOffset.UtcNow,
            CompanyCode: company?.CompanyCode ?? string.Empty,
            CountryCode: company?.CountryCode ?? string.Empty,
            CultureName: company?.CultureName ?? string.Empty,
            TimeZoneId: company?.TimeZoneId ?? string.Empty,
            BaseCurrencyCode: company?.BaseCurrencyCode ?? string.Empty,
            FiscalYearStartMonth: company?.FiscalYearStartMonth ?? 0,
            CompanyTypeCode: company?.CompanyTypeCode ?? string.Empty,
            UnitSystemCode: company?.UnitSystemCode ?? string.Empty,
            DefaultFieldName: company?.DefaultFieldName ?? string.Empty,
            Deployment: installation.Deployment,
            DataConnectionName: installation.ConnectionName,
            SharedSettingsLoadedAtUtc: company is null ? null : DateTimeOffset.UtcNow);
    }
    /// <summary>DAT-08: row history (append-only versions).</summary>
    public Data.Ppdm.IOgVersionStore Versions => _provider.GetRequiredService<Data.Ppdm.IOgVersionStore>();

    /// <summary>DAT-08: who read what.</summary>
    public Data.Ppdm.IOgAccessAudit AccessAudit => _provider.GetRequiredService<Data.Ppdm.IOgAccessAudit>();

    /// <summary>DAT-08: the tamper-evident audit chain.</summary>
    public Data.Ppdm.IOgAuditChain AuditChain => _provider.GetRequiredService<Data.Ppdm.IOgAuditChain>();

    /// <summary>WEL-01: the well index.</summary>
    public Data.Wells.IOgWellSearch WellSearch =>
        _provider.GetRequiredService<Data.Wells.IOgWellSearch>();

    /// <summary>WEL-02: one well's header, identifiers and participants.</summary>
    public Data.Wells.IOgWellHeader WellHeader =>
        _provider.GetRequiredService<Data.Wells.IOgWellHeader>();

    /// <summary>EXP-05: the well program gate - the queue and the decision on each program.</summary>
    public Data.Exploration.IOgWellProgramApprovals WellProgramGate =>
        _provider.GetRequiredService<Data.Exploration.IOgWellProgramApprovals>();

    /// <summary>LSE-01: the lease register over the four lease tables.</summary>
    public Data.Lease.IOgLeaseRegister LeaseRegister =>
        _provider.GetRequiredService<Data.Lease.IOgLeaseRegister>();

    /// <summary>LSE-02: the acquisition pipeline, diligence, rights and negotiation.</summary>
    public Data.Lease.IOgLeasePipeline LeasePipeline =>
        _provider.GetRequiredService<Data.Lease.IOgLeasePipeline>();

    /// <summary>LSE-03: the financial terms, obligations, payments and lifecycle of a lease.</summary>
    public Data.Lease.IOgLeaseFinancials LeaseFinancials =>
        _provider.GetRequiredService<Data.Lease.IOgLeaseFinancials>();

    /// <summary>WFL-05: the signed-in person's tasks across modules.</summary>
    public Data.Workflow.IOgTaskInbox TaskInbox =>
        _provider.GetRequiredService<Data.Workflow.IOgTaskInbox>();

    /// <summary>DEV-01: the development persona's landing.</summary>
    public Data.Development.IOgDevelopmentWorkspace DevelopmentWorkspace =>
        _provider.GetRequiredService<Data.Development.IOgDevelopmentWorkspace>();

    /// <summary>DEV-02: the field development plan wizard, review and baselines.</summary>
    public Data.Development.IOgFdpWizard FdpWizard =>
        _provider.GetRequiredService<Data.Development.IOgFdpWizard>();

    /// <summary>DEV-03: the design of each planned development well.</summary>
    public Data.Development.IOgWellDesign WellDesign =>
        _provider.GetRequiredService<Data.Development.IOgWellDesign>();

    /// <summary>REL-05: the release acceptance checklist and its evidence pack.</summary>
    public Data.Release.IOgReleaseAcceptance ReleaseAcceptance =>
        _provider.GetRequiredService<Data.Release.IOgReleaseAcceptance>();

    /// <summary>REL-01: the installation check, the uninstall decision and the upgrade record.</summary>
    public Data.Release.IOgInstallation Installation =>
        _provider.GetRequiredService<Data.Release.IOgInstallation>();

    /// <summary>RPT-02: the print packs.</summary>
    public Data.Reporting.IOgPrintPacks PrintPacks =>
        _provider.GetRequiredService<Data.Reporting.IOgPrintPacks>();

    /// <summary>RPT-01: the report catalogue, its templates and its exports.</summary>
    public Data.Reporting.IOgReports Reports =>
        _provider.GetRequiredService<Data.Reporting.IOgReports>();

    /// <summary>TRN-03: the sample pack and the local training server.</summary>
    public Data.Training.IOgTrainingServer TrainingServer =>
        _provider.GetRequiredService<Data.Training.IOgTrainingServer>();

    /// <summary>TRN-02: the courses, the results ledger and the cohort.</summary>
    public Data.Training.IOgTrainingCourses TrainingCourses =>
        _provider.GetRequiredService<Data.Training.IOgTrainingCourses>();

    /// <summary>TRN-01: the scenario catalogue, the runner and the results ledger.</summary>
    public Data.Training.IOgTrainingScenarios TrainingScenarios =>
        _provider.GetRequiredService<Data.Training.IOgTrainingScenarios>();

    /// <summary>INT-08: the field in three dimensions.</summary>
    public Data.Interop.IOgScene Scenes =>
        _provider.GetRequiredService<Data.Interop.IOgScene>();

    /// <summary>INT-07: the map of a value across the field.</summary>
    public Data.Interop.IOgMap Map =>
        _provider.GetRequiredService<Data.Interop.IOgMap>();

    /// <summary>INT-06: the field QC run and its proposals.</summary>
    public Data.Interop.IOgFieldQc FieldQc =>
        _provider.GetRequiredService<Data.Interop.IOgFieldQc>();

    /// <summary>INT-05: the customer's wellbore environment and its setup steps.</summary>
    public Data.Interop.IOgWellboreSetup WellboreSetup =>
        _provider.GetRequiredService<Data.Interop.IOgWellboreSetup>();

    /// <summary>INT-04: the mapping profiles and the well data they bring in.</summary>
    public Data.Interop.IOgMappingProfiles MappingProfiles =>
        _provider.GetRequiredService<Data.Interop.IOgMappingProfiles>();

    /// <summary>ENG-09: the drilling performance analysis.</summary>
    public Data.Engineering.IOgDrillingPerformance DrillingPerformance =>
        _provider.GetRequiredService<Data.Engineering.IOgDrillingPerformance>();

    /// <summary>ENG-08: the circulating hydraulics and the ECD profile.</summary>
    public Data.Engineering.IOgHydraulics Hydraulics =>
        _provider.GetRequiredService<Data.Engineering.IOgHydraulics>();

    /// <summary>ENG-07: torque and drag along the saved plan.</summary>
    public Data.Engineering.IOgTorqueDrag TorqueDrag =>
        _provider.GetRequiredService<Data.Engineering.IOgTorqueDrag>();

    /// <summary>ENG-06: the directional well plan.</summary>
    public Data.Engineering.IOgWellPlan WellPlan =>
        _provider.GetRequiredService<Data.Engineering.IOgWellPlan>();

    /// <summary>ENG-05: the field integrity board.</summary>
    public Data.Engineering.IOgIntegrityBoard IntegrityBoard =>
        _provider.GetRequiredService<Data.Engineering.IOgIntegrityBoard>();

    /// <summary>ENG-04: the annuli, their limits and the casing-pressure classification.</summary>
    public Data.Engineering.IOgCasingPressure CasingPressure =>
        _provider.GetRequiredService<Data.Engineering.IOgCasingPressure>();

    /// <summary>ENG-03: the caliper metal-loss analysis and re-rating.</summary>
    public Data.Engineering.IOgMetalLoss MetalLoss =>
        _provider.GetRequiredService<Data.Engineering.IOgMetalLoss>();

    /// <summary>ENG-02: the casing design and its check.</summary>
    public Data.Engineering.IOgCasingDesign CasingDesign =>
        _provider.GetRequiredService<Data.Engineering.IOgCasingDesign>();

    /// <summary>ENG-01: the anti-collision scan.</summary>
    public Data.Engineering.IOgAntiCollision AntiCollision =>
        _provider.GetRequiredService<Data.Engineering.IOgAntiCollision>();

    /// <summary>VSB-03: a well from the database as a wellbore document.</summary>
    public Data.Wells.IOgWellboreSource WellboreSource =>
        _provider.GetRequiredService<Data.Wells.IOgWellboreSource>();

    /// <summary>WFL-06: workflow administration.</summary>
    public Data.Workflow.IOgWorkflowAdmin WorkflowAdmin =>
        _provider.GetRequiredService<Data.Workflow.IOgWorkflowAdmin>();

    /// <summary>WFL-04: the gate reviews and their decisions.</summary>
    public Data.Workflow.IOgGateReviews GateReviews =>
        _provider.GetRequiredService<Data.Workflow.IOgGateReviews>();

    /// <summary>WFL-03: the process instances, their timeline and their history.</summary>
    public Data.Workflow.IOgProcessInstances ProcessInstances =>
        _provider.GetRequiredService<Data.Workflow.IOgProcessInstances>();

    /// <summary>WFL-02: the definition catalogue.</summary>
    public Data.Workflow.IOgDefinitionCatalogue DefinitionCatalogue =>
        _provider.GetRequiredService<Data.Workflow.IOgDefinitionCatalogue>();

    /// <summary>WFL-01: the process dashboard.</summary>
    public Data.Workflow.IOgProcessDashboard ProcessDashboard =>
        _provider.GetRequiredService<Data.Workflow.IOgProcessDashboard>();

    /// <summary>WRK-04: the contractor register and permit-to-work.</summary>
    public Data.Work.IOgPermitsToWork PermitsToWork =>
        _provider.GetRequiredService<Data.Work.IOgPermitsToWork>();

    /// <summary>WRK-03: maintenance plans, service jobs and inspections.</summary>
    public Data.Work.IOgMaintenance Maintenance =>
        _provider.GetRequiredService<Data.Work.IOgMaintenance>();

    /// <summary>WRK-02: one work order with its lines, cost and transitions.</summary>
    public Data.Work.IOgWorkOrderDetail WorkOrderDetail =>
        _provider.GetRequiredService<Data.Work.IOgWorkOrderDetail>();

    /// <summary>WRK-01: the work-order dashboard.</summary>
    public Data.Work.IOgWorkOrders WorkOrders =>
        _provider.GetRequiredService<Data.Work.IOgWorkOrders>();

    /// <summary>DEC-03: facility decommissioning, cost estimates and restoration.</summary>
    public Data.Decommissioning.IOgFacilityCost FacilityCost =>
        _provider.GetRequiredService<Data.Decommissioning.IOgFacilityCost>();

    /// <summary>DEC-02: a well's plug-and-abandonment programme.</summary>
    public Data.Decommissioning.IOgWellPa WellPa =>
        _provider.GetRequiredService<Data.Decommissioning.IOgWellPa>();

    /// <summary>DEC-01: the decommissioning workspace.</summary>
    public Data.Decommissioning.IOgDecommissioning Decommissioning =>
        _provider.GetRequiredService<Data.Decommissioning.IOgDecommissioning>();

    /// <summary>HSE-07: the permit inbox, its applications and its decisions.</summary>
    public Data.Hse.IOgPermits Permits =>
        _provider.GetRequiredService<Data.Hse.IOgPermits>();

    /// <summary>HSE-06: the GHG factor catalog and the emissions report.</summary>
    public Data.Hse.IOgGhg Ghg =>
        _provider.GetRequiredService<Data.Hse.IOgGhg>();

    /// <summary>HSE-05: the compliance calendar and obligation register.</summary>
    public Data.Hse.IOgCompliance Compliance =>
        _provider.GetRequiredService<Data.Hse.IOgCompliance>();

    /// <summary>HSE-04: the HAZOP study, its nodes, deviations and actions.</summary>
    public Data.Hse.IOgHazop Hazop =>
        _provider.GetRequiredService<Data.Hse.IOgHazop>();

    /// <summary>HSE-03: the investigation, its cause chain and its corrective actions.</summary>
    public Data.Hse.IOgInvestigation Investigation =>
        _provider.GetRequiredService<Data.Hse.IOgInvestigation>();

    /// <summary>HSE-02: the incident register and the report command.</summary>
    public Data.Hse.IOgIncidents Incidents =>
        _provider.GetRequiredService<Data.Hse.IOgIncidents>();

    /// <summary>HSE-01: the HSE workspace.</summary>
    public Data.Hse.IOgHseWorkspace Hse =>
        _provider.GetRequiredService<Data.Hse.IOgHseWorkspace>();

    /// <summary>ACC-01: the accounting dashboard.</summary>
    public Data.Accounting.IOgAccountingDashboard AccountingDashboard =>
        _provider.GetRequiredService<Data.Accounting.IOgAccountingDashboard>();

    /// <summary>ACC-08: the tax workbench.</summary>
    public Data.Accounting.IOgTax Tax =>
        _provider.GetRequiredService<Data.Accounting.IOgTax>();

    /// <summary>ACC-07: royalties, joint-interest billing and cost allocation.</summary>
    public Data.Accounting.IOgRoyalties Royalties =>
        _provider.GetRequiredService<Data.Accounting.IOgRoyalties>();

    /// <summary>ACC-06: the warehouse, its holds and its stock movements.</summary>
    public Data.Accounting.IOgWarehouse Warehouse =>
        _provider.GetRequiredService<Data.Accounting.IOgWarehouse>();

    /// <summary>ACC-05: payables, procurement and the payment run.</summary>
    public Data.Accounting.IOgPayables Payables =>
        _provider.GetRequiredService<Data.Accounting.IOgPayables>();

    /// <summary>ACC-04: receivables, receipts and the ageing.</summary>
    public Data.Accounting.IOgReceivable Receivables =>
        _provider.GetRequiredService<Data.Accounting.IOgReceivable>();

    /// <summary>ACC-03: revenue, sales invoices and credit notes.</summary>
    public Data.Accounting.IOgRevenue Revenue =>
        _provider.GetRequiredService<Data.Accounting.IOgRevenue>();

    /// <summary>ACC-02: the ledger, posting and the period close.</summary>
    public Data.Accounting.IOgLedger Ledger =>
        _provider.GetRequiredService<Data.Accounting.IOgLedger>();

    /// <summary>ECO-03: the investment review queue and handoff.</summary>
    public Data.Economics.IOgInvestmentReview InvestmentReview =>
        _provider.GetRequiredService<Data.Economics.IOgInvestmentReview>();

    /// <summary>ECO-02: AFE management.</summary>
    public Data.Economics.IOgAfe Afes =>
        _provider.GetRequiredService<Data.Economics.IOgAfe>();

    /// <summary>ECO-01: economic evaluation.</summary>
    public Data.Economics.IOgEconomics Economics =>
        _provider.GetRequiredService<Data.Economics.IOgEconomics>();

    /// <summary>WTA-03: pipeline analysis and integrity.</summary>
    public Data.WellTest.IOgPipeline Pipelines =>
        _provider.GetRequiredService<Data.WellTest.IOgPipeline>();

    /// <summary>WTA-02: the well test register.</summary>
    public Data.WellTest.IOgWellTestRegister WellTestRegister =>
        _provider.GetRequiredService<Data.WellTest.IOgWellTestRegister>();

    /// <summary>WTA-01: well test analysis.</summary>
    public Data.WellTest.IOgWellTest WellTests =>
        _provider.GetRequiredService<Data.WellTest.IOgWellTest>();

    /// <summary>LFT-08: compressor analysis.</summary>
    public Data.Lift.IOgCompressor Compressors =>
        _provider.GetRequiredService<Data.Lift.IOgCompressor>();

    /// <summary>LFT-07: choke analysis.</summary>
    public Data.Lift.IOgChoke Choke =>
        _provider.GetRequiredService<Data.Lift.IOgChoke>();

    /// <summary>LFT-06: hydraulic pumps.</summary>
    public Data.Lift.IOgHydraulicPump HydraulicPumps =>
        _provider.GetRequiredService<Data.Lift.IOgHydraulicPump>();

    /// <summary>LFT-05: plunger lift.</summary>
    public Data.Lift.IOgPlunger Plunger =>
        _provider.GetRequiredService<Data.Lift.IOgPlunger>();

    /// <summary>LFT-04: sucker rod pumping.</summary>
    public Data.Lift.IOgSuckerRod SuckerRod =>
        _provider.GetRequiredService<Data.Lift.IOgSuckerRod>();

    /// <summary>LFT-03: ESP and pump performance.</summary>
    public Data.Lift.IOgPump Pumps =>
        _provider.GetRequiredService<Data.Lift.IOgPump>();

    /// <summary>LFT-02: gas lift design.</summary>
    public Data.Lift.IOgGasLift GasLift =>
        _provider.GetRequiredService<Data.Lift.IOgGasLift>();

    /// <summary>LFT-01: nodal analysis.</summary>
    public Data.Lift.IOgNodal Nodal =>
        _provider.GetRequiredService<Data.Lift.IOgNodal>();

    /// <summary>RES-07: production forecasting.</summary>
    public Data.Reservoir.IOgForecast Forecasting =>
        _provider.GetRequiredService<Data.Reservoir.IOgForecast>();

    /// <summary>RES-06: decline curve analysis.</summary>
    public Data.Reservoir.IOgDeclineCurve DeclineCurve =>
        _provider.GetRequiredService<Data.Reservoir.IOgDeclineCurve>();

    /// <summary>RES-05: reserves classification and characterization.</summary>
    public Data.Reservoir.IOgReserves Reserves =>
        _provider.GetRequiredService<Data.Reservoir.IOgReserves>();

    /// <summary>RES-04: EOR screening.</summary>
    public Data.Reservoir.IOgEorScreening EorScreening =>
        _provider.GetRequiredService<Data.Reservoir.IOgEorScreening>();

    /// <summary>RES-03: flash calculations and the phase envelope.</summary>
    public Data.Reservoir.IOgFlash Flash =>
        _provider.GetRequiredService<Data.Reservoir.IOgFlash>();

    /// <summary>RES-02: oil properties (black-oil screening).</summary>
    public Data.Reservoir.IOgOilProperties OilProperties =>
        _provider.GetRequiredService<Data.Reservoir.IOgOilProperties>();

    /// <summary>RES-01: gas properties.</summary>
    public Data.Reservoir.IOgGasProperties GasProperties =>
        _provider.GetRequiredService<Data.Reservoir.IOgGasProperties>();

    /// <summary>PRD-06: intervention decisions and the engineer workspace.</summary>
    public Data.Production.IOgInterventions Interventions =>
        _provider.GetRequiredService<Data.Production.IOgInterventions>();

    /// <summary>PRD-05: volume reconciliation and reporting.</summary>
    public Data.Production.IOgReconciliation Reconciliation =>
        _provider.GetRequiredService<Data.Production.IOgReconciliation>();

    /// <summary>PRD-04: the allocation workbench.</summary>
    public Data.Production.IOgAllocationWorkbench AllocationWorkbench =>
        _provider.GetRequiredService<Data.Production.IOgAllocationWorkbench>();

    /// <summary>PRD-03: the well status board and the deferment log.</summary>
    public Data.Production.IOgWellStatusDeferment WellStatusDeferment =>
        _provider.GetRequiredService<Data.Production.IOgWellStatusDeferment>();

    /// <summary>PRD-02: the register of producing fields and pools.</summary>
    public Data.Production.IOgFieldPoolRegister FieldPoolRegister =>
        _provider.GetRequiredService<Data.Production.IOgFieldPoolRegister>();

    /// <summary>PRD-01: the daily operations centre.</summary>
    public Data.Production.IOgOperationsCentre OperationsCentre =>
        _provider.GetRequiredService<Data.Production.IOgOperationsCentre>();

    /// <summary>DEV-08: facility construction progress and the handover to operations.</summary>
    public Data.Development.IOgConstruction Construction =>
        _provider.GetRequiredService<Data.Development.IOgConstruction>();

    /// <summary>DEV-07: completion tests, the submission and the independent acceptance/handover.</summary>
    public Data.Development.IOgWellCompletion Completion =>
        _provider.GetRequiredService<Data.Development.IOgWellCompletion>();

    /// <summary>DEV-06: the execution of an approved program, its daily reports and responsibility.</summary>
    public Data.Development.IOgDrillingExecution DrillingExecution =>
        _provider.GetRequiredService<Data.Development.IOgDrillingExecution>();

    /// <summary>DEV-05: drilling program authoring and review.</summary>
    public Data.Development.IOgDrillingPrograms DrillingPrograms =>
        _provider.GetRequiredService<Data.Development.IOgDrillingPrograms>();

    /// <summary>DEV-04: pools, facility capacity and the facility investment a plan depends on.</summary>
    public Data.Development.IOgPoolsFacilities PoolsFacilities =>
        _provider.GetRequiredService<Data.Development.IOgPoolsFacilities>();

    /// <summary>EXP-04: the seismic register - surveys, lines and processing runs.</summary>
    public Data.Exploration.IOgSeismicRegister SeismicRegister =>
        _provider.GetRequiredService<Data.Exploration.IOgSeismicRegister>();

    /// <summary>EXP-03: the prospect editor and its risk assessment.</summary>
    public Data.Exploration.IOgProspectEditor ProspectEditor =>
        _provider.GetRequiredService<Data.Exploration.IOgProspectEditor>();

    /// <summary>EXP-02: the prospect maturation board.</summary>
    public Data.Exploration.IOgProspectBoard ProspectBoard =>
        _provider.GetRequiredService<Data.Exploration.IOgProspectBoard>();

    /// <summary>EXP-02: the prospect the board selected, for the editor that follows.</summary>
    public Core.OgProspectContext ProspectContext =>
        _provider.GetRequiredService<Core.OgProspectContext>();

    /// <summary>EXP-01: the exploration persona's workspace.</summary>
    public Data.Exploration.IOgExplorationWorkspace ExplorationWorkspace =>
        _provider.GetRequiredService<Data.Exploration.IOgExplorationWorkspace>();

    /// <summary>WEL-06: the well comparison matrix.</summary>
    public Data.Wells.IOgWellComparison WellComparison =>
        _provider.GetRequiredService<Data.Wells.IOgWellComparison>();

    /// <summary>WEL-05: the stratigraphic framework - units, columns and well picks.</summary>
    public Data.Wells.IOgStratigraphy Stratigraphy =>
        _provider.GetRequiredService<Data.Wells.IOgStratigraphy>();

    /// <summary>WEL-04: the well status and its append-only history.</summary>
    public Data.Wells.IOgWellStatus WellStatus => _provider.GetRequiredService<Data.Wells.IOgWellStatus>();

    /// <summary>WEL-03: the wellbores of one well.</summary>
    public Data.Wells.IOgWellbores Wellbores => _provider.GetRequiredService<Data.Wells.IOgWellbores>();

    /// <summary>WEL-02: coordinate-system records and explicit identifier choices.</summary>
    public Diagnostics.OgWellHeaderNotesStore WellHeaderNotes =>
        _provider.GetRequiredService<Diagnostics.OgWellHeaderNotesStore>();

    /// <summary>WEL-01: the well every page consumes, with cancellation of stale loads.</summary>
    public Core.OgWellContext WellContext => _provider.GetRequiredService<Core.OgWellContext>();

    /// <summary>WEL-01: saved well searches, persisted per user.</summary>
    public Diagnostics.OgSavedWellSearchStore SavedWellSearches =>
        _provider.GetRequiredService<Diagnostics.OgSavedWellSearchStore>();

    /// <summary>DAT-07: the last successful quality run per table.</summary>
    public Data.Ppdm.IOgQualityJournal QualityJournal =>
        _provider.GetRequiredService<Data.Ppdm.IOgQualityJournal>();

    /// <summary>DAT-08: re-opens governed writes after a named acknowledgement of a chain break.</summary>
    public Data.Ppdm.OgChainGate ChainGate => _provider.GetRequiredService<Data.Ppdm.OgChainGate>();

    /// <summary>DAT-04: the table workbench gateway.</summary>
    public Data.Ppdm.IOgPpdmTableGateway TableGateway =>
        _provider.GetRequiredService<Data.Ppdm.IOgPpdmTableGateway>();

    /// <summary>Lets a page ask the shell for another page (DAT-03 to DAT-04).</summary>
    public Core.OgPageNavigator Pages => _provider.GetRequiredService<Core.OgPageNavigator>();

    /// <summary>The table and filter the tree left for the workbench.</summary>
    public Core.OgWorkbenchNavigation Workbench => _provider.GetRequiredService<Core.OgWorkbenchNavigation>();

    /// <summary>DAT-03: the PPDM tree browser's data side.</summary>
    public Data.Ppdm.IOgPpdmTreeBuilder PpdmTree =>
        _provider.GetRequiredService<Data.Ppdm.IOgPpdmTreeBuilder>();

    /// <summary>DAT-02: the module-setup runner (plan, apply, resume).</summary>
    public Data.Setup.IOgModuleSetupRunner ModuleSetup =>
        _provider.GetRequiredService<Data.Setup.IOgModuleSetupRunner>();

    /// <summary>FND-02 AC-3: the first business read against the company API, with explicit failure states.</summary>
    public Services.OgRemoteReader RemoteReader =>
        _provider.GetRequiredService<Services.OgRemoteReader>();

    /// <summary>REL-02: the licence in force, and the operations that change it.</summary>
    public Security.OgLicenceService Licence =>
        _provider.GetRequiredService<Security.OgLicenceService>();

    /// <summary>REL-02: the commercial gate the navigation asks before a page is offered.</summary>
    public IOgEntitlementService Entitlements => _provider.GetRequiredService<IOgEntitlementService>();

    /// <summary>FND-06: forwards stored failures to the API when the installation is in CompanyApi mode.</summary>
    public OgFailureRelay FailureRelay => _provider.GetRequiredService<OgFailureRelay>();

    /// <summary>The shell status line: company · field · mode.</summary>
    public string StatusLine => Loc.Get(
        "Shell.Status",
        Session.CompanyName,
        Session.HasField ? Session.FieldName : "—",
        Session.Mode);

    /// <summary>Applies the recorded interface language when this build supports it.</summary>
    private void ApplyCulture(Data.Settings.OgInstallationSettings company)
    {
        if (!string.IsNullOrWhiteSpace(company.CultureName))
        {
            Loc.SetCulture(company.CultureName);
        }
    }

    public void Dispose() => _provider.Dispose();
}
