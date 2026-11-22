using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEC-01 against a real SQLite database: the scope is the latest recorded status of each asset (AC-1), an asset with no
/// estimate is counted and excluded from the totals in its own currency (AC-2, AC-4), the restoration records are counted
/// apart from the asset states (SG-12-4), and a failed read carries no workspace at all rather than zero counts.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DecommissioningGatewayTests
{
    private const string ConnectionName = "dec01-workspace";

    private static readonly DateTime AsOf = new(2026, 11, 8);

    [IntegrationFact]
    public async Task The_scope_costs_and_restoration_come_from_the_records()
    {        var root = Path.Combine(OgPaths.DataRoot, "dec01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDec01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "decommissioning.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "decommissioning.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var workspace = new OgDecommissioning(() => source, new Inbox(), reporter);

        var loaded = await workspace.LoadAsync(
            "F-1", "DECOMMISSIONING_COORDINATOR", ["DecommissioningCoordinator"], AsOf, "UTC",
            route => route == "wells");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var snapshot = loaded.Value!;

        // ---- the scope: the latest state of each asset ----
        Assert.Equal(3, snapshot.Scopes.Count);
        Assert.Equal(2, snapshot.Wells);
        Assert.Equal(1, snapshot.Facilities);

        var well = snapshot.Scopes.Single(scope => scope.AssetId == "UWI-1");

        // The latest status wins: PLANNED was recorded after SUSPENDED.
        Assert.Equal("WELL", well.Kind);
        Assert.Equal("PLANNED", well.State);
        // The well's label comes from the ABANDONMENT_STATUS reference set, not the decommissioning one.
        Assert.Equal("Plug and abandonment planned", well.StateLabel);
        Assert.False(well.IsEstimated);
        Assert.Equal("not estimated", well.EstimatedText);

        // ---- AC-2 and AC-4: the totals exclude the unestimated asset and stay in their currency ----
        Assert.Equal(2, snapshot.Unestimated);

        var usd = Assert.Single(snapshot.Costs);

        Assert.Equal("USD", usd.Currency);
        Assert.Equal(2_500_000.55m, usd.Estimated);
        Assert.Equal(400_000.25m, usd.Actual);
        Assert.Equal(0, usd.Unestimated);

        // The two wells record no cost at all, so they are counted as not estimated rather than as a currency of zeros.
        Assert.Contains("2 not estimated", snapshot.CountsText, StringComparison.Ordinal);
        Assert.DoesNotContain("no currency", snapshot.CostText, StringComparison.Ordinal);

        // ---- AC-1: the state strip is the recorded states ----
        Assert.Equal(2, snapshot.States.Count);
        Assert.Equal(2, snapshot.States.Single(row => row.State == "PLANNED").Count);
        Assert.Equal(1, snapshot.States.Single(row => row.State == "SUSPENDED").Count);
        Assert.Contains("Planned for decommissioning", snapshot.StateStrip, StringComparison.Ordinal);

        // ---- AC-5: no planned date is recorded, so nothing is reported as overdue ----
        Assert.Equal(0, snapshot.Overdue);
        Assert.Contains("no planned year is recorded", snapshot.PlannedText, StringComparison.Ordinal);

        // ---- SG-12-4: the restoration is counted apart from the asset states ----
        Assert.Equal(2, snapshot.Restoration.Count);
        Assert.Equal(1, snapshot.RestorationOpen);
        Assert.Equal(1, snapshot.RestorationCertified);
        Assert.Contains("CERTIFIED", snapshot.RestorationText, StringComparison.OrdinalIgnoreCase);

        // ---- AC-3: the links are gated, and the pages this build lacks name their feature ----
        Assert.True(snapshot.Links.Single(link => link.Route == "wells").Available);
        // The well P&A page exists now, so it is refused by the account like the others.
        Assert.Contains("may not open", snapshot.Links.Single(link => link.Route == "wellpa").WhyNot!,
            StringComparison.Ordinal);

        // The facilities-and-cost page exists now, so it is refused by the account rather than by its absence.
        Assert.Contains("may not open", snapshot.Links.Single(link => link.Route == "facilitycost").WhyNot!,
            StringComparison.Ordinal);

        // ---- the composed inbox ----
        Assert.True(snapshot.Inbox.Available);
        Assert.Contains("overdue 2", snapshot.InboxText, StringComparison.Ordinal);

        Assert.Contains("last refreshed", "last refreshed " + snapshot.ReadText, StringComparison.Ordinal);

        // ---- AC-1: a failed read is a failure, not an empty portfolio ----
        Execute(database, "DROP TABLE DECOMMISSIONING_STATUS;");

        var broken = await workspace.LoadAsync(
            "F-1", "DECOMMISSIONING_COORDINATOR", ["DecommissioningCoordinator"], AsOf, "UTC", _ => true);

        Assert.False(broken.Success);
        Assert.Null(broken.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) =>
        Execute(database, """
            CREATE TABLE ABANDONMENT_STATUS (
                ABANDONMENT_STATUS_ID TEXT,
                WELL_ABANDONMENT_ID TEXT,
                STATUS TEXT,
                STATUS_DATE TEXT,
                STATUS_CHANGED_BY TEXT,
                NOTES TEXT,
                UWI TEXT,
                WELLBORE_ID TEXT,
                ABANDONMENT_TYPE TEXT,
                REGULATORY_PERMIT_NUMBER TEXT,
                PLUG_BACK_DEPTH_MD REAL,
                PLUG_BACK_DEPTH_OUOM TEXT,
                VERIFIED_BY TEXT,
                VERIFICATION_DATE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (ABANDONMENT_STATUS_ID));

            CREATE TABLE DECOMMISSIONING_STATUS (
                DECOMMISSIONING_STATUS_ID TEXT,
                FACILITY_DECOMMISSIONING_ID TEXT,
                STATUS TEXT,
                STATUS_DATE TEXT,
                STATUS_CHANGED_BY TEXT,
                NOTES TEXT,
                FIELD_ID TEXT,
                FACILITY_ID TEXT,
                DECOM_PHASE TEXT,
                REGULATORY_AUTHORITY TEXT,
                REGULATORY_APPROVAL_NUMBER TEXT,
                ESTIMATED_COST REAL,
                ACTUAL_COST REAL,
                COST_CURRENCY TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (DECOMMISSIONING_STATUS_ID));

            CREATE TABLE ENVIRONMENTAL_RESTORATION (
                RESTORATION_ID TEXT,
                SITE_ID TEXT,
                SITE_TYPE TEXT,
                RESTORATION_TYPE TEXT,
                START_DATE TEXT,
                COMPLETION_DATE TEXT,
                STATUS TEXT,
                CERTIFICATION_STATUS TEXT,
                CERTIFICATION_DATE TEXT,
                CERTIFYING_AUTHORITY TEXT,
                CONTRACTOR_ID TEXT,
                ENVIRONMENTAL_IMPACT TEXT,
                REMEDIATION_METHOD TEXT,
                REMARK TEXT,
                SOURCE TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (RESTORATION_ID));

            CREATE TABLE R_DECOMMISSIONING_REFERENCE_CODE (
                REFERENCE_SET TEXT,
                REFERENCE_CODE TEXT,
                LONG_NAME TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (REFERENCE_SET, REFERENCE_CODE));

            INSERT INTO R_DECOMMISSIONING_REFERENCE_CODE (REFERENCE_SET, REFERENCE_CODE, LONG_NAME) VALUES
                ('DECOMMISSIONING_STATUS', 'PLANNED', 'Planned for decommissioning'),
                ('ABANDONMENT_STATUS', 'PLANNED', 'Plug and abandonment planned');

            INSERT INTO ABANDONMENT_STATUS (ABANDONMENT_STATUS_ID, WELL_ABANDONMENT_ID, UWI, STATUS, STATUS_DATE,
                ACTIVE_IND) VALUES
                ('AS-1', 'WA-1', 'UWI-1', 'SUSPENDED', '2026-01-01', 'Y'),
                ('AS-2', 'WA-1', 'UWI-1', 'PLANNED', '2026-06-01', 'Y'),
                ('AS-3', 'WA-2', 'UWI-2', 'SUSPENDED', '2026-02-01', 'Y');

            INSERT INTO DECOMMISSIONING_STATUS (DECOMMISSIONING_STATUS_ID, FACILITY_DECOMMISSIONING_ID, FACILITY_ID,
                FIELD_ID, STATUS, STATUS_DATE, ESTIMATED_COST, ACTUAL_COST, COST_CURRENCY, ACTIVE_IND) VALUES
                ('DS-1', 'FD-1', 'FAC-1', 'F-1', 'PLANNED', '2026-03-01', 2500000.55, 400000.25, 'USD', 'Y');

            INSERT INTO ENVIRONMENTAL_RESTORATION (RESTORATION_ID, SITE_ID, SITE_TYPE, RESTORATION_TYPE, START_DATE,
                COMPLETION_DATE, STATUS, CERTIFICATION_STATUS, CERTIFYING_AUTHORITY) VALUES
                ('R-1', 'SITE-1', 'WELL PAD', 'SOIL', '2026-01-01', NULL, 'IN_PROGRESS', 'PENDING', 'RRC'),
                ('R-2', 'SITE-2', 'PLANT', 'GROUNDWATER', '2025-01-01', '2025-09-01', 'COMPLETE', 'CERTIFIED', 'TCEQ');
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private sealed class Inbox : IOgTaskInbox
    {
        public Task<TheTechIdea.Beep.OilandGas.Desktop.Core.OgResult<OgTaskInboxSnapshot>> LoadAsync(
            string personaCode,
            IReadOnlyCollection<string> roles,
            string? fieldId,
            string fieldName,
            OgTaskTab tab,
            DateTime asOf,
            string timeZoneId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(TheTechIdea.Beep.OilandGas.Desktop.Core.OgResult<OgTaskInboxSnapshot>.Ok(
                new OgTaskInboxSnapshot(
                    personaCode,
                    tab,
                    [],
                    new OgTaskCounts(1, 2, 0, 5, 0, 0),
                    asOf)));

        public long BeginLoad() => 1;

        public bool IsCurrent(long version) => version == 1;
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
