using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Production;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// PRD-03 against a real SQLite database over the PPDM status tables and the ProductionOperations deferment tables: the
/// board reads the status history, a deferment needs a reason code from the reference set (AC-1), an overlapping
/// deferment is saved and flagged rather than merged (AC-2), a deferred volume carries its unit (AC-3), the totals equal
/// the rows (AC-4), an ongoing deferment is distinct from a closed one and can be closed (AC-5), an empty reference set
/// refuses every deferment, and a failed read is a failure. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DefermentGatewayTests
{
    private const string ConnectionName = "prd03-deferment";
    private static readonly DateTime Day = new(2026, 10, 1);

    [IntegrationFact]
    public async Task The_board_reads_the_status_history_and_the_log_records_coded_deferments()
    {        var root = Path.Combine(OgPaths.DataRoot, "prd03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPrd03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "deferment.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "deferment.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellStatusDefermentService(() => source, reporter);

        // ---- the board -------------------------------------------------------
        var loaded = await service.LoadAsync("F-1", "Alpha field", Day);

        Assert.True(loaded.Success, loaded.Message);

        var board = loaded.Value!;

        Assert.Equal(3, board.WellCount);                        // WELL-9 is another field
        Assert.DoesNotContain(board.Wells, well => well.WellId == "WELL-9");

        // The statuses come from the PPI status history: WELL-1 producing, WELL-2 suspended, WELL-3 producing.
        Assert.Equal(2, board.Groups.Count);
        Assert.Contains(board.Groups, group => group.StatusText == "producing" && group.Count == 2);
        Assert.Contains(board.Groups, group => group.StatusText == "suspended" && group.Count == 1);
        Assert.Contains("producing (2)", board.GroupText, StringComparison.Ordinal);

        var well1 = board.Wells.Single(well => well.WellId == "WELL-1");

        Assert.Equal("Well one", well1.NameText);
        Assert.Equal("producing", well1.StatusText);
        Assert.Equal("120 bbl", well1.RateText);
        Assert.True(well1.IsDeferred);
        Assert.Equal("EQ-FAIL (Equipment failure)", well1.ReasonText);
        Assert.Equal(1, board.DeferredWells);

        // One deferment is open, one was closed: AC-5's distinction, and AC-4's total.
        Assert.Equal(2, board.Deferments.Count);
        Assert.Equal(1, board.OngoingCount);
        Assert.Equal(175m, board.TotalVolume);
        Assert.Equal("bbl", board.TotalUnit);
        Assert.Contains("175 bbl", board.TotalText, StringComparison.Ordinal);

        var byReason = board.Totals.Single(total => total.Code == "EQ-FAIL");

        Assert.Equal(2, byReason.Count);
        Assert.Equal(175m, byReason.Volume);
        Assert.Equal(board.Deferments.Sum(deferment => deferment.Volume ?? 0m), board.Totals.Sum(total => total.Volume));

        Assert.Equal(["EQ-FAIL", "WEATHER"], board.Reasons);      // the inactive code is not offered
        Assert.False(board.ReasonReferenceMissing);

        // ---- AC-1: no reason code, no deferment ------------------------------
        var noReason = await service.LogDefermentAsync(Values(reason: null), "sara");

        Assert.False(noReason.Success);
        Assert.Equal(DefermentRules.CodeReasonRequired, noReason.Reference);
        Assert.Contains("EQ-FAIL", noReason.Message!, StringComparison.Ordinal);

        var unknown = await service.LogDefermentAsync(Values(reason: "MADE-UP"), "sara");

        Assert.False(unknown.Success);
        Assert.Equal(DefermentRules.CodeReasonUnknown, unknown.Reference);

        var inactive = await service.LogDefermentAsync(Values(reason: "RETIRED"), "sara");

        Assert.False(inactive.Success);
        Assert.Equal(DefermentRules.CodeReasonUnknown, inactive.Reference);

        // ---- AC-3: a volume without its unit --------------------------------
        var noUnit = await service.LogDefermentAsync(
            Values(reason: "WEATHER", start: Day.AddDays(1), volume: 40m), "sara");

        Assert.False(noUnit.Success);
        Assert.Equal(DefermentRules.CodeVolumeUnitRequired, noUnit.Reference);

        // ---- AC-2: an overlap is saved and flagged --------------------------
        var overlapping = await service.LogDefermentAsync(
            Values(reason: "WEATHER", start: Day.AddHours(4), volume: 40m, unit: "bbl"), "sara");

        Assert.True(overlapping.Success, overlapping.Message);

        var afterOverlap = overlapping.Value!;

        Assert.Equal(3, afterOverlap.Deferments.Count);
        Assert.Equal(2, afterOverlap.OverlapCount);
        Assert.All(
            afterOverlap.Deferments.Where(deferment => deferment.Start.Date == Day),
            deferment => Assert.True(deferment.Overlaps, $"{deferment.Id} should be flagged as overlapping"));

        // AC-4: the weather total now includes the new row, and the whole total is still the rows' sum.
        Assert.Equal(40m, afterOverlap.Totals.Single(total => total.Code == "WEATHER").Volume);
        Assert.Equal(215m, afterOverlap.TotalVolume);
        Assert.Equal(
            afterOverlap.Deferments.Sum(deferment => deferment.Volume ?? 0m),
            afterOverlap.Totals.Sum(total => total.Volume));

        // ---- AC-5: an ongoing deferment is closed, and not twice ------------
        var open = afterOverlap.Deferments.Single(deferment => deferment.IsOngoing && deferment.ReasonCode == "WEATHER");

        Assert.Equal("ongoing", open.EndText);
        Assert.Contains("so far", open.DurationText(Day.AddHours(12)), StringComparison.Ordinal);

        var closed = await service.CloseDefermentAsync(open.Id, Day.AddHours(10), "sara");

        Assert.True(closed.Success, closed.Message);

        var afterClose = closed.Value!;
        var closedRow = afterClose.Deferments.Single(deferment => deferment.Id == open.Id);

        Assert.False(closedRow.IsOngoing);
        Assert.Equal(6m, closedRow.ElapsedHours(Day.AddHours(20)));
        Assert.Equal(1, afterClose.OngoingCount);                 // D-2 is still open
        Assert.True(afterClose.Wells.Single(well => well.WellId == "WELL-1").IsDeferred);

        var twice = await service.CloseDefermentAsync(open.Id, Day.AddHours(11), "sara");

        Assert.False(twice.Success);
        Assert.Equal(DefermentRules.CodeAlreadyClosed, twice.Reference);

        var beforeStart = await service.CloseDefermentAsync(
            afterClose.Deferments.Single(deferment => deferment.IsOngoing).Id,
            Day.AddDays(-10),
            "sara");

        Assert.False(beforeStart.Success);
        Assert.Equal(DefermentRules.CodeEndBeforeStart, beforeStart.Reference);

        // ---- an empty reference set refuses every deferment ------------------
        ClearReasons(database);

        var withoutReference = await service.LoadAsync("F-1", "Alpha field", Day);

        Assert.True(withoutReference.Value!.ReasonReferenceMissing);

        var refused = await service.LogDefermentAsync(Values(reason: "EQ-FAIL", start: Day.AddDays(2)), "sara");

        Assert.False(refused.Success);
        Assert.Equal(DefermentRules.CodeReasonReferenceMissing, refused.Reference);

        // ---- a failed read is a failure --------------------------------------
        Drop(database, "PRODUCTION_DEFERMENT");

        var failed = await service.LoadAsync("F-1", "Alpha field", Day);

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
            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                WELL_NAME TEXT NULL,
                ASSIGNED_FIELD TEXT NULL,
                ACTIVE_IND TEXT NULL,
                CURRENT_STATUS TEXT NULL);

            CREATE TABLE WELL_STATUS (
                UWI TEXT NOT NULL,
                STATUS_ID TEXT NOT NULL,
                STATUS_TYPE TEXT NULL,
                STATUS TEXT NULL,
                STATUS_DATE TEXT NULL,
                START_TIME TEXT NULL,
                END_TIME TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (UWI, STATUS_ID));

            CREATE TABLE R_WELL_STATUS (
                STATUS_TYPE TEXT NOT NULL,
                STATUS TEXT NOT NULL,
                SHORT_NAME TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (STATUS_TYPE, STATUS));

            CREATE TABLE PDEN_WELL (
                PDEN_ID TEXT NOT NULL,
                PDEN_SUBTYPE TEXT NULL,
                PDEN_SOURCE TEXT NULL,
                PRIMARY_UWI TEXT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (PDEN_ID, PRIMARY_UWI));

            CREATE TABLE PDEN_VOL_SUMMARY (
                PDEN_ID TEXT NOT NULL,
                PDEN_SUBTYPE TEXT NULL,
                PDEN_SOURCE TEXT NULL,
                ACTIVITY_TYPE TEXT NULL,
                AMENDMENT_SEQ_NO REAL NOT NULL,
                ACTIVE_IND TEXT NULL,
                VOLUME_DATE TEXT NULL,
                OIL_VOLUME REAL NOT NULL,
                OIL_VOLUME_OUOM TEXT NULL,
                GAS_VOLUME REAL NOT NULL,
                GAS_VOLUME_OUOM TEXT NULL,
                WATER_VOLUME REAL NOT NULL,
                WATER_VOLUME_OUOM TEXT NULL,
                PRIMARY KEY (PDEN_ID, VOLUME_DATE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO));

            CREATE TABLE R_DEFERMENT_REASON (
                REASON_CODE TEXT NOT NULL PRIMARY KEY,
                REASON_NAME TEXT NULL,
                CATEGORY TEXT NULL,
                ACTIVE_IND TEXT NULL);

            CREATE TABLE PRODUCTION_DEFERMENT (
                DEFERMENT_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NOT NULL,
                FIELD_ID TEXT NULL,
                START_AT TEXT NOT NULL,
                END_AT TEXT NULL,
                REASON_CODE TEXT NOT NULL,
                DEFERRED_VOLUME REAL NULL,
                VOLUME_OUOM TEXT NULL,
                COMMENT TEXT NULL,
                REPORTER TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, ACTIVE_IND, CURRENT_STATUS) VALUES
                ('WELL-1', 'Well one', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-2', 'Well two', 'F-1', 'Y', 'SUSPENDED'),
                ('WELL-3', 'Well three', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-9', 'Other field well', 'F-2', 'Y', 'PRODUCING');

            INSERT INTO R_WELL_STATUS (STATUS_TYPE, STATUS, SHORT_NAME, ACTIVE_IND) VALUES
                ('Well Status', 'PRODUCING', 'Producing', 'Y'),
                ('Well Status', 'SUSPENDED', 'Suspended', 'Y');

            INSERT INTO WELL_STATUS (UWI, STATUS_ID, STATUS_TYPE, STATUS, STATUS_DATE, START_TIME, END_TIME, ACTIVE_IND) VALUES
                ('WELL-1', 'WS-1', 'Well Status', 'PRODUCING', '2026-08-01', '2026-08-01', NULL, 'Y'),
                ('WELL-2', 'WS-2', 'Well Status', 'SUSPENDED', '2026-09-01', '2026-09-01', NULL, 'Y'),
                ('WELL-3', 'WS-3', 'Well Status', 'PRODUCING', '2026-07-01', '2026-07-01', NULL, 'Y'),
                ('WELL-9', 'WS-9', 'Well Status', 'PRODUCING', '2026-07-01', '2026-07-01', NULL, 'Y');

            INSERT INTO PDEN_WELL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PRIMARY_UWI, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'WELL-1', 'Y'),
                ('PD-2', 'WELL', 'PPDM', 'WELL-2', 'Y');

            INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO,
                ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME,
                WATER_VOLUME_OUOM) VALUES
                ('PD-1', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 120, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-2', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 0, 'bbl', 0, 'MMscf', 0, 'bbl');

            INSERT INTO R_DEFERMENT_REASON (REASON_CODE, REASON_NAME, CATEGORY, ACTIVE_IND) VALUES
                ('EQ-FAIL', 'Equipment failure', 'EQUIPMENT', 'Y'),
                ('WEATHER', 'Waiting on weather', 'WEATHER', 'Y'),
                ('RETIRED', 'Retired code', 'OTHER', 'N');

            -- One closed deferment and one ongoing, both for WELL-1.
            INSERT INTO PRODUCTION_DEFERMENT (DEFERMENT_ID, WELL_ID, FIELD_ID, START_AT, END_AT, REASON_CODE,
                DEFERRED_VOLUME, VOLUME_OUOM, COMMENT, REPORTER, ACTOR, RECORDED_AT) VALUES
                ('D-1', 'WELL-1', 'F-1', '2026-09-01 06:00', '2026-09-01 14:00', 'EQ-FAIL', 100, 'bbl',
                 'pump trip', 'sara', 'sara', '2026-09-01 15:00'),
                ('D-2', 'WELL-1', 'F-1', '2026-10-01 06:00', NULL, 'EQ-FAIL', 75, 'bbl',
                 'seal leak', 'sara', 'sara', '2026-10-01 07:00');
            """;
        command.ExecuteNonQuery();
    }

    private static void ClearReasons(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM R_DEFERMENT_REASON;";
        command.ExecuteNonQuery();
    }

    private static void Drop(string database, string table)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = $"DROP TABLE {table};";
        command.ExecuteNonQuery();
    }

    private static OgDefermentValues Values(
        string? reason,
        DateTime? start = null,
        decimal? volume = null,
        string? unit = null) =>
        new("WELL-1", start ?? Day.AddHours(2), null, reason, volume, unit, "test", "sara");

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
