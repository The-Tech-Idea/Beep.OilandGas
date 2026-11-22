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
/// PRD-01 against a real SQLite database over the PPDM tables: the day total takes the latest amendment, injection and
/// undated rows are excluded and counted, a row in another unit is named and never converted, the field's inactive
/// wells are not expected to report, the completeness count names the wells that did not, top movers need seven days of
/// history, and a failed read is a failure. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class OperationsCentreGatewayTests
{
    private const string ConnectionName = "prd01-operations";
    private static readonly DateTime Day = new(2026, 10, 1);

    [IntegrationFact]
    public async Task The_day_reads_the_latest_amendment_excludes_what_it_cannot_compare_and_counts_what_is_missing()
    {        var root = Path.Combine(OgPaths.DataRoot, "prd01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPrd01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "operations.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "operations.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgOperationsCentreService(() => source, reporter);

        var loaded = await service.LoadAsync("F-1", "Alpha field", Day);

        Assert.True(loaded.Success, loaded.Message);

        var centre = loaded.Value!;

        // The field, its wells and the window.
        Assert.Equal("Alpha field", centre.FieldName);
        Assert.Equal("F-1", centre.FieldId);
        Assert.Equal(Day, centre.Day);
        Assert.Equal(30, centre.Days.Count);
        Assert.Equal(Day.AddDays(-29), centre.Days[0].Date);
        Assert.Equal(Day, centre.Days[^1].Date);

        // AC-2 / the day state.
        Assert.True(centre.IsReported);
        Assert.StartsWith("reported for 2026-10-01", centre.DayStateText, StringComparison.Ordinal);

        // The amendment supersedes the original row: 120 (amended) + 50 (other well) = 170 bbl.
        Assert.Equal(170m, centre.Today.Oil);
        Assert.Equal("bbl", centre.Today.OilUnit);
        Assert.Equal("170 bbl", centre.OilText);

        Assert.Equal(5m, centre.Today.Gas);
        Assert.Equal("MMscf", centre.Today.GasUnit);
        Assert.Equal("5 MMscf", centre.GasText);

        Assert.Equal(75m, centre.Today.Water);
        Assert.Equal("75 bbl", centre.WaterText);

        // AC-3: the water cut is computed because oil and water are both in bbl.
        Assert.Equal(75m / 245m, centre.WaterCut);
        Assert.Equal("30.61%", centre.WaterCutText);

        // The other field's well is not part of this field's day.
        Assert.DoesNotContain(centre.Wells, well => well.WellId == "WELL-9");
        Assert.Equal(2, centre.Wells.Count);

        // Rows the screen excludes, and why.
        Assert.Equal(1, centre.InjectionRowsExcluded);
        Assert.True(centre.SupersededRowsExcluded >= 1);
        Assert.Contains(centre.NotComparable, item => item.Contains("not in bbl", StringComparison.Ordinal));
        Assert.Contains("1 oil row(s) not in bbl", centre.NotComparableText, StringComparison.Ordinal);
        Assert.Contains("1 injection", centre.ExcludedText, StringComparison.Ordinal);
        Assert.Contains("without a volume date", centre.ExcludedText, StringComparison.Ordinal);

        // AC-4: the field has three active wells and WELL-3 did not report.
        Assert.Equal(3, centre.ActiveWells);
        Assert.Equal(3, centre.TotalWells);
        Assert.Equal(1, centre.MissingWellCount);
        Assert.Equal("WELL-3", Assert.Single(centre.Alerts).WellId);
        Assert.Contains("1 of 3 active well(s) have no measurement", centre.CompletenessText, StringComparison.Ordinal);
        Assert.Contains("no production measurement", centre.Alerts[0].Describe(), StringComparison.Ordinal);

        // AC-5: WELL-1 has seven days of history behind the day, so its change is shown with its unit.
        Assert.True(centre.TopMoversAvailable);
        Assert.Contains("previous 7 day(s)", centre.TopMoversReason, StringComparison.Ordinal);

        var mover = Assert.Single(centre.Gainers);

        Assert.Equal("WELL-1", mover.WellId);
        Assert.Equal(120m, mover.Today);
        Assert.Equal(100m, mover.Average);
        Assert.Equal(20m, mover.Change);
        Assert.Equal(7, mover.DaysOfHistory);
        Assert.Equal("+20 bbl", mover.ChangeText);
        Assert.Empty(centre.Decliners);

        // The day before is reported as well, and the strip names each day.
        Assert.True(centre.Days.Single(day => day.Date == Day.AddDays(-1)).Reported);
        Assert.Contains("100 bbl", centre.StripText, StringComparison.Ordinal);

        // Deferment and allocation are labelled, not zero.
        Assert.False(centre.DeferredAvailable);
        Assert.Contains("PRD-03", centre.DeferredText, StringComparison.Ordinal);
        Assert.Contains("PRD-04", centre.AllocationText, StringComparison.Ordinal);

        // A day with no measurement is "not yet reported", not zero.
        var quiet = await service.LoadAsync("F-1", "Alpha field", Day.AddDays(-60));

        Assert.True(quiet.Success, quiet.Message);
        Assert.False(quiet.Value!.IsReported);
        Assert.False(quiet.Value.HasData);
        Assert.Contains("no production is recorded", quiet.Value.DayStateText, StringComparison.Ordinal);
        Assert.Equal("not yet reported", quiet.Value.OilText);
        Assert.Equal("no data for the day, so completeness is not known", quiet.Value.CompletenessText);

        // AC-1: a failed read is a failure, not a page of zero cards.
        Drop(database, "PDEN_VOL_SUMMARY");

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
            CREATE TABLE FIELD (
                FIELD_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_NAME TEXT NULL,
                ACTIVE_IND TEXT NULL);

            CREATE TABLE WELL (
                UWI TEXT NOT NULL PRIMARY KEY,
                WELL_NAME TEXT NULL,
                ASSIGNED_FIELD TEXT NULL,
                ACTIVE_IND TEXT NULL,
                CURRENT_STATUS TEXT NULL);

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
                PERIOD_ID TEXT NULL,
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

            INSERT INTO FIELD (FIELD_ID, FIELD_NAME, ACTIVE_IND) VALUES
                ('F-1', 'Alpha field', 'Y'),
                ('F-2', 'Other field', 'Y');

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, ACTIVE_IND, CURRENT_STATUS) VALUES
                ('WELL-1', 'Well one', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-2', 'Well two', 'F-1', 'Y', 'PRODUCING'),
                ('WELL-3', 'Well three', 'F-1', 'Y', 'SUSPENDED'),
                ('WELL-9', 'Other field well', 'F-2', 'Y', 'PRODUCING');

            INSERT INTO PDEN_WELL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PRIMARY_UWI, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'WELL-1', 'Y'),
                ('PD-2', 'WELL', 'PPDM', 'WELL-2', 'Y'),
                ('PD-9', 'WELL', 'PPDM', 'WELL-9', 'Y');

            -- WELL-1: the original measurement and its amendment on the day, plus seven days of history.
            INSERT INTO PDEN_VOL_SUMMARY
                (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PERIOD_ID, ACTIVITY_TYPE, AMENDMENT_SEQ_NO, ACTIVE_IND,
                 VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME, WATER_VOLUME_OUOM)
            VALUES
                ('PD-1', 'WELL', 'PPDM', '2026-10-01', 'PRODUCTION', 0, 'Y', '2026-10-01', 100, 'bbl', 5, 'MMscf', 25, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', '2026-10-01', 'PRODUCTION', 1, 'Y', '2026-10-01', 120, 'bbl', 5, 'MMscf', 25, 'bbl'),
                -- an injection row on the same day is not a production row
                ('PD-1', 'WELL', 'PPDM', '2026-10-01', 'INJECTION', 0, 'Y', '2026-10-01', 0, 'bbl', 0, 'MMscf', 400, 'bbl'),
                -- a row with no volume date is not a calendar day
                ('PD-1', 'WELL', 'PPDM', NULL, 'PRODUCTION', 0, 'Y', NULL, 10, 'bbl', 0, 'MMscf', 0, 'bbl'),
                ('PD-2', 'WELL', 'PPDM', '2026-10-01', 'PRODUCTION', 0, 'Y', '2026-10-01', 50, 'bbl', 0, 'MMscf', 50, 'bbl'),
                -- a row stated in another unit is named rather than added to that day's oil
                ('PD-2', 'WELL', 'PPDM', '2026-09-29', 'PRODUCTION', 0, 'Y', '2026-09-29', 10, 'm3', 0, 'MMscf', 0, 'bbl'),
                -- the other field's well must not reach this field's day
                ('PD-9', 'WELL', 'PPDM', '2026-10-01', 'PRODUCTION', 0, 'Y', '2026-10-01', 900, 'bbl', 9, 'MMscf', 9, 'bbl');

            -- Seven days of history for WELL-1 before the day.
            INSERT INTO PDEN_VOL_SUMMARY
                (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PERIOD_ID, ACTIVITY_TYPE, AMENDMENT_SEQ_NO, ACTIVE_IND,
                 VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME, WATER_VOLUME_OUOM)
            VALUES
                ('PD-1', 'WELL', 'PPDM', '2026-09-24', 'PRODUCTION', 0, 'Y', '2026-09-24', 100, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', '2026-09-25', 'PRODUCTION', 0, 'Y', '2026-09-25', 100, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', '2026-09-26', 'PRODUCTION', 0, 'Y', '2026-09-26', 100, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', '2026-09-27', 'PRODUCTION', 0, 'Y', '2026-09-27', 100, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', '2026-09-28', 'PRODUCTION', 0, 'Y', '2026-09-28', 100, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', '2026-09-29', 'PRODUCTION', 0, 'Y', '2026-09-29', 100, 'bbl', 5, 'MMscf', 20, 'bbl'),
                ('PD-1', 'WELL', 'PPDM', '2026-09-30', 'PRODUCTION', 0, 'Y', '2026-09-30', 100, 'bbl', 5, 'MMscf', 20, 'bbl');
            """;
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
