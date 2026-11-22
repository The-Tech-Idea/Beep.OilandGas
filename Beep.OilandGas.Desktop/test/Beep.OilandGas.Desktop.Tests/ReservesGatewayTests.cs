using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reservoir;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RES-05 against a real SQLite database: the totals exclude the unclassified entry and say so (AC-1); a revision is an
/// append that supersedes without overwriting (AC-3); every characterization fact names its source, including the two
/// nothing records (AC-4); the disclosure is blocked while an entry has no classifier and needs a second person to
/// approve it (AC-5). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class ReservesGatewayTests
{
    private const string ConnectionName = "res05-reserves";

    [IntegrationFact]
    public async Task Reserves_classify_revise_and_disclose_with_the_exclusions_stated()
    {        var root = Path.Combine(OgPaths.DataRoot, "res05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRes05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "reserves.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "reserves.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgReservesService(() => source, reporter);

        // ---- the seeded state ------------------------------------------------
        var loaded = await service.LoadAsync("F-1", "Alpha field");

        Assert.True(loaded.Success, loaded.Message);

        var workspace = loaded.Value!;

        // Four rows: three current entries and the proved revision they superseded.
        Assert.Equal(4, workspace.Entries.Count);
        Assert.Equal(3, workspace.Entries.Select(entry => entry.PoolId + entry.CategoryText).Distinct().Count());

        // AC-1: proved totals exclude the unclassified entry, and the exclusion is stated with its volumes.
        var proved = Assert.Single(workspace.Totals, total => total.Category == "PROVED");

        Assert.Equal(1, proved.Count);                       // only the current revision
        Assert.Equal(150m, proved.Gross);                    // the superseded 100 is not counted
        Assert.Equal(120m, proved.Net);

        var probable = Assert.Single(workspace.Totals, total => total.Category == "PROBABLE");

        Assert.Equal(40m, probable.Gross);
        Assert.Equal(1, probable.Count);

        Assert.DoesNotContain(workspace.Totals, total => total.Gross == 900m);
        Assert.Equal(1, workspace.UnclassifiedCount);
        Assert.Equal(900m, workspace.UnclassifiedGross);
        Assert.Contains("1 entry/entries are unclassified and excluded", workspace.UnclassifiedText,
            StringComparison.Ordinal);

        // AC-3: the superseded revision is still there, and it points forward through the newer row's supersede link.
        Assert.Contains(workspace.Entries, entry => entry.Revision == 1 && entry.Gross == 100m);
        Assert.Contains(workspace.Entries, entry => entry.Revision == 2 && entry.Gross == 150m &&
                                                   entry.SupersedesEntryId is not null);

        // AC-4: every fact names its source, and the two nothing records are still listed.
        Assert.All(workspace.Characterization, fact => Assert.False(string.IsNullOrWhiteSpace(fact.Source)));
        Assert.Contains(workspace.Characterization, fact =>
            fact.Fact == "Drive mechanism" && fact.Value == "water drive" &&
            fact.Source == "DEVELOPMENT_POOL_PLAN.DRIVE_MECHANISM");
        Assert.Contains(workspace.Characterization, fact =>
            fact.Fact == "Reservoir pressure" && fact.Value == "not recorded");
        Assert.Contains(workspace.Characterization, fact => fact.Fact == "Reservoir state");

        // The maturation funnel reads the prospect estimate.
        Assert.NotEmpty(workspace.Maturation);
        Assert.Contains(workspace.Maturation, row => row.ProspectId == "PR-1" && row.OilP50 == 12m);

        // The preview states the exclusion and the entry count.
        Assert.Contains("excluded (unclassified): 1 entry/entries", workspace.Preview, StringComparison.Ordinal);

        // ---- AC-5 and AC-3: a revision with no classifier blocks the disclosure ----
        // Every seeded entry carries a classifier, so nothing is blocked yet.
        Assert.False(workspace.DisclosureBlocked);
        Assert.Contains("disclosure available", workspace.DisclosureStateText, StringComparison.Ordinal);

        var revised = await service.AddRevisionAsync(
            new OgReserveEntryValues("P-2", "PROVED", 60m, 48m, "bbl", new DateTime(2026, 10, 1), null, "no classifier yet"),
            "sara",
            "Alpha field");

        Assert.True(revised.Success, revised.Message);

        var afterRevision = revised.Value!;

        // The new row is present, the one it supersedes is still present, and the revision number advanced.
        Assert.Equal(5, CountRows(database, "RESERVE_ENTRY"));
        // P-2 had no proved entry, so this is revision 1 and supersedes nothing - a first entry, not a revision.
        Assert.Contains(afterRevision.Entries, entry =>
            entry.PoolId == "P-2" && entry.Category == "PROVED" && entry.Revision == 1 &&
            entry.Gross == 60m && entry.SupersedesEntryId is null);

        // The unclassified-by-a-person entry blocks the disclosure, and the reason names it.
        Assert.Single(afterRevision.MissingClassifier);
        Assert.True(afterRevision.DisclosureBlocked);
        Assert.Contains("carry no classifier", afterRevision.DisclosureBlockedReason, StringComparison.Ordinal);

        var stillBlocked = await service.ApproveDisclosureAsync("F-1", new DateTime(2026, 10, 1), "reviewer");

        Assert.False(stillBlocked.Success);
        Assert.Equal(ReserveRules.CodeDisclosureBlocked, stillBlocked.Reference);
        Assert.Contains("carry no classifier", stillBlocked.Message!, StringComparison.Ordinal);
        Assert.Equal(0, CountRows(database, "RESERVE_DISCLOSURE"));

        // Classify it properly with a further revision; nothing was overwritten to get there.
        var classified = await service.AddRevisionAsync(
            new OgReserveEntryValues("P-2", "PROVED", 60m, 48m, "bbl", new DateTime(2026, 10, 1), "sara", "classified"),
            "sara",
            "Alpha field");

        Assert.True(classified.Success, classified.Message);
        Assert.False(classified.Value!.DisclosureBlocked);
        Assert.Equal(6, CountRows(database, "RESERVE_ENTRY"));
        // Two P-2 proved rows now exist - the unclassified one and the classified revision that supersedes it - so
        // the revision added a row rather than rewriting one (AC-3).
        Assert.Equal(2, Scalar(database,
            "SELECT COUNT(*) FROM RESERVE_ENTRY WHERE POOL_ID = 'P-2' AND CATEGORY = 'PROVED';"));

        var superseding = classified.Value.Entries.Single(entry =>
            entry.PoolId == "P-2" && entry.Category == "PROVED" && entry.Revision == 2);

        Assert.NotNull(superseding.SupersedesEntryId);
        Assert.Equal("sara", superseding.Classifier);

        // Separation of duties: the classifier cannot approve their own disclosure.
        var selfApproval = await service.ApproveDisclosureAsync("F-1", new DateTime(2026, 10, 1), "sara");

        Assert.False(selfApproval.Success);
        Assert.Equal(ReserveRules.CodeApproverIsPreparer, selfApproval.Reference);

        // A second person approves, and the record carries the totals it was approved under.
        var approved = await service.ApproveDisclosureAsync("F-1", new DateTime(2026, 10, 1), "reviewer", "Alpha field");

        Assert.True(approved.Success, approved.Message);

        var afterApproval = approved.Value!;

        var disclosure = Assert.Single(afterApproval.Disclosures);

        Assert.Equal("reviewer", disclosure.ApprovedBy);
        Assert.Equal(new DateTime(2026, 10, 1), disclosure.Period);
        Assert.Equal(1, disclosure.ExcludedCount);
        Assert.Equal(250m, disclosure.GrossTotal);            // proved 150 + 60, plus probable 40
        Assert.NotNull(disclosure.ApprovedAt);

        Assert.Equal(1, CountRows(database, "RESERVE_DISCLOSURE"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM RESERVE_DISCLOSURE WHERE APPROVED_BY = 'reviewer' AND EXCLUDED_COUNT = 1;"));

        // ---- a failed read is a failure --------------------------------------
        Drop(database, "RESERVE_ENTRY");

        var failed = await service.LoadAsync("F-1", "Alpha field");

        Assert.False(failed.Success);
        Assert.NotNull(failed.Reference);
        Assert.Null(failed.Value);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE FIELD (
                FIELD_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_NAME TEXT NULL,
                FIELD_TYPE TEXT NULL,
                AREA_ID TEXT NULL,
                DISCOVERY_DATE TEXT NULL,
                ACTIVE_IND TEXT NULL);

            CREATE TABLE POOL (
                POOL_ID TEXT NOT NULL,
                FIELD_ID TEXT NULL,
                POOL_NAME TEXT NULL,
                POOL_STATUS TEXT NULL,
                POOL_TYPE TEXT NULL,
                STRAT_UNIT_ID TEXT NULL,
                DISCOVERY_DATE TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (POOL_ID, FIELD_ID));

            CREATE TABLE RESERVE_ENTRY (
                ENTRY_ID TEXT NOT NULL PRIMARY KEY,
                POOL_ID TEXT NOT NULL,
                FIELD_ID TEXT NULL,
                CATEGORY TEXT NULL,
                GROSS_VOLUME REAL NULL,
                NET_VOLUME REAL NULL,
                VOLUME_OUOM TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                REVISION INTEGER NOT NULL,
                CLASSIFIER TEXT NULL,
                SUPERSEDES_ENTRY_ID TEXT NULL,
                BASIS_TEXT TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE RESERVE_DISCLOSURE (
                DISCLOSURE_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                PERIOD TEXT NOT NULL,
                GROSS_TOTAL REAL NULL,
                NET_TOTAL REAL NULL,
                VOLUME_OUOM TEXT NULL,
                ENTRY_COUNT INTEGER NOT NULL,
                EXCLUDED_COUNT INTEGER NOT NULL,
                PREPARED_BY TEXT NULL,
                APPROVED_BY TEXT NULL,
                APPROVED_AT TEXT NULL,
                STATE TEXT NOT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE RESERVOIR_STATUS (
                RESERVOIR_STATUS_ID TEXT NOT NULL PRIMARY KEY,
                RESERVE_ENTITY_ID TEXT NULL,
                POOL_ID TEXT NULL,
                STATUS TEXT NULL,
                STATUS_DATE TEXT NULL,
                STATUS_CHANGED_BY TEXT NULL,
                NOTES TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_POOL_PLAN (
                POOL_PLAN_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NOT NULL,
                PLAN_VERSION INTEGER NOT NULL,
                FIELD_ID TEXT NOT NULL,
                POOL_ID TEXT NULL,
                POOL_NAME TEXT NOT NULL,
                RESERVOIR TEXT NULL,
                DRIVE_MECHANISM TEXT NULL,
                OOIP REAL NOT NULL,
                OOIP_OUOM TEXT NULL,
                RECOVERY_FACTOR REAL NOT NULL,
                PLANNED_WELLS INTEGER NOT NULL,
                FACILITY_ID TEXT NULL);

            CREATE TABLE PROSPECT_VOLUME_ESTIMATE (
                PROSPECT_ID TEXT NOT NULL,
                ESTIMATE_ID TEXT NOT NULL,
                OIL_VOLUME_P10 REAL NULL,
                OIL_VOLUME_P50 REAL NULL,
                OIL_VOLUME_P90 REAL NULL,
                GAS_VOLUME_P10 REAL NULL,
                GAS_VOLUME_P50 REAL NULL,
                GAS_VOLUME_P90 REAL NULL,
                VOLUME_OUOM TEXT NULL,
                UNRISKED_OIL_VOLUME REAL NULL,
                UNRISKED_GAS_VOLUME REAL NULL,
                RISKED_OIL_VOLUME REAL NULL,
                RISKED_GAS_VOLUME REAL NULL,
                RECOVERY_FACTOR REAL NULL,
                FORMATION_VOLUME_FACTOR REAL NULL,
                ESTIMATE_METHOD TEXT NULL,
                ESTIMATE_DATE TEXT NULL,
                ESTIMATOR TEXT NULL,
                DESCRIPTION TEXT NULL,
                ESTIMATE_TYPE TEXT NULL,
                FLUID_TYPE TEXT NULL,
                OIL_VOLUME_MEAN REAL NULL,
                GAS_VOLUME_MEAN REAL NULL,
                CONDENSATE_VOLUME_P50 REAL NULL,
                BOE_EQUIVALENT_P50 REAL NULL,
                PRIMARY KEY (PROSPECT_ID, ESTIMATE_ID));

            INSERT INTO FIELD (FIELD_ID, FIELD_NAME, FIELD_TYPE, AREA_ID, DISCOVERY_DATE, ACTIVE_IND) VALUES
                ('F-1', 'Alpha field', 'OIL', 'AREA-1', '2005-03-01', 'Y'),
                ('F-2', 'Other field', 'GAS', 'AREA-2', '2010-07-01', 'Y');

            INSERT INTO POOL (POOL_ID, FIELD_ID, POOL_NAME, POOL_STATUS, POOL_TYPE, STRAT_UNIT_ID, DISCOVERY_DATE, ACTIVE_IND) VALUES
                ('P-1', 'F-1', 'Upper sand', 'PRODUCING', 'OIL', 'STRAT-A', '2005-03-01', 'Y'),
                ('P-2', 'F-1', 'Lower sand', 'PRODUCING', 'OIL', 'STRAT-B', '2006-05-01', 'Y'),
                ('P-9', 'F-2', 'Other sand', 'PRODUCING', 'GAS', 'STRAT-Z', '2010-07-01', 'Y');

            INSERT INTO RESERVE_ENTRY (ENTRY_ID, POOL_ID, FIELD_ID, CATEGORY, GROSS_VOLUME, NET_VOLUME, VOLUME_OUOM,
                EFFECTIVE_DATE, REVISION, CLASSIFIER, SUPERSEDES_ENTRY_ID, BASIS_TEXT, ACTOR, RECORDED_AT) VALUES
                ('E-1', 'P-1', 'F-1', 'PROVED', 100, 80, 'bbl', '2026-01-01', 1, 'sara', NULL, 'initial', 'sara', '2026-01-02'),
                ('E-2', 'P-1', 'F-1', 'PROVED', 150, 120, 'bbl', '2026-10-01', 2, 'sara', 'E-1', 'revised', 'sara', '2026-10-02'),
                ('E-3', 'P-2', 'F-1', 'PROBABLE', 40, 32, 'bbl', '2026-10-01', 1, 'sara', NULL, 'initial', 'sara', '2026-10-02'),
                ('E-4', 'P-2', 'F-1', NULL, 900, 900, 'bbl', '2026-10-01', 1, 'sara', NULL, 'unclassified', 'sara', '2026-10-02');

            INSERT INTO RESERVOIR_STATUS (RESERVOIR_STATUS_ID, POOL_ID, STATUS, STATUS_DATE) VALUES
                ('RS-1', 'P-1', 'ON_PRODUCTION', '2026-09-01');

            INSERT INTO DEVELOPMENT_POOL_PLAN (POOL_PLAN_ID, FDP_ID, PLAN_VERSION, FIELD_ID, POOL_ID, POOL_NAME,
                RESERVOIR, DRIVE_MECHANISM, OOIP, OOIP_OUOM, RECOVERY_FACTOR, PLANNED_WELLS, FACILITY_ID) VALUES
                ('PP-1', 'FDP-1', 3, 'F-1', 'P-1', 'Upper sand', 'STRAT-A', 'water drive', 100, 'MMbbl', 0.35, 4, 'FAC-1');

            INSERT INTO PROSPECT_VOLUME_ESTIMATE (PROSPECT_ID, ESTIMATE_ID, OIL_VOLUME_P50, GAS_VOLUME_P50,
                VOLUME_OUOM, RISKED_OIL_VOLUME, ESTIMATE_METHOD, ESTIMATE_DATE, ESTIMATOR) VALUES
                ('PR-1', 'PE-1', 12, 0, 'MMbbl', 6, 'analogue', '2026-08-01', 'sara');
            """;
        command.ExecuteNonQuery();
    }

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database, string table) =>
        Scalar(database, $"SELECT COUNT(*) FROM {table};");

    private static void Drop(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE {table};";
        command.ExecuteNonQuery();
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
            Failures.Add($"{operation} | {consequence} | {exception.Message}");
            return FailureReferenceFormat.New();
        }
    }
}
