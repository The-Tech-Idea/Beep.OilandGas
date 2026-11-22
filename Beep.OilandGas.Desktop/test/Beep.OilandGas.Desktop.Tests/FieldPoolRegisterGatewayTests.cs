using System.Text;
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
/// PRD-02 against a real SQLite database over the PPDM tables: the field figures come from the records, an unreported
/// field is pre-production rather than zero, an inactive field is named, aggregates exclude the rows whose unit is not
/// recorded and count them (AC-3), the pools carry their recorded rate and cumulative with DEV-04's drive mechanism, the
/// grid pages beyond five hundred fields (AC-4) and a failed read is a failure (AC-2). Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class FieldPoolRegisterGatewayTests
{
    private const string ConnectionName = "prd02-register";
    private static readonly DateTime Day = new(2026, 10, 1);

    [IntegrationFact]
    public async Task The_register_reads_every_figure_from_a_record_and_pages_beyond_five_hundred_fields()
    {        var root = Path.Combine(OgPaths.DataRoot, "prd02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasPrd02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "register.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "register.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgFieldPoolRegisterService(() => source, reporter);

        // ---- paging beyond 500 fields (AC-4) ---------------------------------
        var first = await service.LoadAsync("F-1", "Alpha field", null, 0, 100, Day);

        Assert.True(first.Success, first.Message);

        var register = first.Value!;

        Assert.Equal(521, register.TotalFields);
        Assert.Equal(6, register.PageCount);
        Assert.Equal(100, register.Fields.Count);
        Assert.Equal(0, register.PageIndex);
        Assert.False(register.HasPrevious);
        Assert.True(register.HasNext);
        Assert.Contains("fields 1–100 of 521", register.PagingText, StringComparison.Ordinal);

        var last = (await service.LoadAsync("F-1", "Alpha field", null, 5, 100, Day)).Value!;

        Assert.Equal(21, last.Fields.Count);
        Assert.Equal(5, last.PageIndex);
        Assert.True(last.HasPrevious);
        Assert.False(last.HasNext);

        // An index past the end is clamped rather than honoured.
        Assert.Equal(5, (await service.LoadAsync("F-1", "Alpha field", null, 99, 100, Day)).Value!.PageIndex);

        // ---- the field figures come from the records (AC-1) -------------------
        var alpha = register.Selected!;

        Assert.Equal("F-1", alpha.Id);
        Assert.Equal("Alpha field", alpha.NameText);
        Assert.Equal("AREA-1", alpha.BasinText);
        Assert.Equal(new DateTime(2005, 3, 1), alpha.DiscoveryDate);
        Assert.Equal(new DateTime(2026, 9, 17), alpha.FirstProduction);
        Assert.Equal("producing", alpha.StatusText);
        Assert.Equal(3, alpha.WellCount);
        Assert.Equal(2, alpha.PoolCount);

        // The field figures are the window's totals: fifteen days of 100 bbl for PD-1 (1500) plus PD-2's 50 bbl.
        Assert.Equal(1550m, alpha.Oil);
        Assert.Equal("bbl", alpha.OilUnit);
        Assert.Equal("1,550 bbl", alpha.OilText);

        // Water is 305 bbl for PD-1 plus 25 bbl for PD-2, so the water cut is 330 / 1880.
        Assert.Equal(330m, alpha.Water);
        Assert.Equal(330m / 1880m, alpha.WaterCut);
        Assert.Equal("17.55%", alpha.WaterCutText);

        // 15 of the 30 window days carry a measurement.
        Assert.Equal(0.5m, alpha.Uptime);
        Assert.Equal("50%", alpha.UptimeText);

        // AC-3: the row in m3 and the row with no unit are excluded and counted.
        Assert.Equal(2, alpha.ExcludedUnitRows);
        Assert.Contains("2 row(s) excluded", alpha.ExcludedText, StringComparison.Ordinal);
        Assert.Equal(2, register.ExcludedUnitRows);

        // ---- an unreported field and an inactive one -------------------------
        var bravo = register.Fields.Single(item => item.Id == "F-2");

        Assert.True(bravo.IsPreProduction);
        Assert.Equal("pre_production", bravo.StatusText);
        Assert.Equal("—", bravo.OilText);
        Assert.Equal("—", bravo.FirstProductionText);
        Assert.Equal("—", bravo.WaterCutText);

        var charlie = register.Fields.Single(item => item.Id == "F-3");

        Assert.False(charlie.IsPreProduction);
        Assert.Equal("inactive", charlie.StatusText);
        Assert.Equal("20 bbl", charlie.OilText);

        // ---- the pools of the selected field --------------------------------
        Assert.Equal(2, register.Pools.Count);

        var upper = register.Pools.Single(pool => pool.Id == "P-1");

        Assert.Equal("Upper sand", upper.NameText);
        Assert.Equal("STRAT-A", upper.ReservoirText);
        Assert.Equal("water drive", upper.DriveText);   // DEV-04's plan row, not PPDM
        Assert.Equal("producing", upper.StatusText);
        Assert.Equal(1, upper.WellCount);
        Assert.Equal("100 bbl", upper.RateText);
        Assert.Equal("5,000 bbl", upper.CumulativeText);

        var lower = register.Pools.Single(pool => pool.Id == "P-2");

        Assert.Equal("not recorded", lower.DriveText);   // no plan row: said, not invented
        Assert.Equal("50 bbl", lower.RateText);
        Assert.Equal("2,000 bbl", lower.CumulativeText);

        // ---- the trend -------------------------------------------------------
        Assert.Equal(FieldRegisterRules.TrendMonths, register.Trend.Count);
        Assert.Contains("monthly oil", register.TrendText, StringComparison.Ordinal);

        var september = register.Trend.Single(point => point.Label == "2026-09");
        var october = register.Trend.Single(point => point.Label == "2026-10");

        Assert.Equal(1400m, september.Oil);       // 14 days of 100 bbl
        Assert.Equal(150m, october.Oil);          // 100 bbl + PD-2's 50 bbl
        Assert.Equal("bbl", september.Unit);

        // A pre-production field has no trend to draw, and says so.
        var bravoRegister = (await service.LoadAsync("F-2", "Bravo field", null, 0, 100, Day)).Value!;

        Assert.Contains(
            "no production is recorded for Bravo field",
            bravoRegister.TrendText,
            StringComparison.Ordinal);

        // Selecting a pool narrows the trend to it.
        var poolRegister = (await service.LoadAsync("F-1", "Alpha field", "P-2", 0, 100, Day)).Value!;

        Assert.Equal("P-2", poolRegister.SelectedPool!.Id);
        Assert.Contains("Alpha field / Lower sand", poolRegister.TrendText, StringComparison.Ordinal);

        // ---- AC-2: a failed read is a failure, not zeroed figures ------------
        Drop(database, "PDEN_VOL_SUMMARY");

        var failed = await service.LoadAsync("F-1", "Alpha field", null, 0, 100, Day);

        Assert.False(failed.Success);
        Assert.NotNull(failed.Reference);
        Assert.Null(failed.Value);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        var filler = new StringBuilder();

        // 518 further fields so the register has to page beyond five hundred (AC-4).
        for (var index = 0; index < 518; index++)
        {
            filler.AppendLine(
                $"INSERT INTO FIELD (FIELD_ID, FIELD_NAME, AREA_ID, FIELD_TYPE, ACTIVE_IND) " +
                $"VALUES ('F-{1000 + index}', 'Field {1000 + index}', 'AREA-1', 'OIL', 'Y');");
        }

        var days = new StringBuilder();

        // Fifteen days of 100 bbl for PD-1 (2026-09-17 .. 2026-10-01), so the window's uptime is 50%.
        for (var index = 0; index < 15; index++)
        {
            var date = Day.AddDays(-index);
            var water = date == Day ? 25 : 20;

            days.AppendLine(
                "INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO, " +
                "ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, OIL_CUM_VOLUME, GAS_VOLUME, GAS_VOLUME_OUOM, " +
                "WATER_VOLUME, WATER_VOLUME_OUOM) VALUES " +
                $"('PD-1', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '{date:yyyy-MM-dd}', 100, 'bbl', 5000, 10, 'MMscf', " +
                $"{water}, 'bbl');");
        }

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE FIELD (
                FIELD_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_NAME TEXT NULL,
                AREA_ID TEXT NULL,
                FIELD_TYPE TEXT NULL,
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

            CREATE TABLE PDEN_FIELD (
                PDEN_ID TEXT NOT NULL,
                PDEN_SUBTYPE TEXT NULL,
                PDEN_SOURCE TEXT NULL,
                FIELD_ID TEXT NULL,
                NO_OF_OIL_WELLS REAL NOT NULL,
                NO_OF_GAS_WELLS REAL NOT NULL,
                NO_OF_INJECTION_WELLS REAL NOT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (PDEN_ID, FIELD_ID));

            CREATE TABLE PDEN_POOL (
                PDEN_ID TEXT NOT NULL,
                PDEN_SUBTYPE TEXT NULL,
                PDEN_SOURCE TEXT NULL,
                POOL_ID TEXT NULL,
                NO_OF_OIL_WELLS REAL NOT NULL,
                NO_OF_GAS_WELLS REAL NOT NULL,
                NO_OF_INJECTION_WELLS REAL NOT NULL,
                ACTIVE_IND TEXT NULL,
                PRIMARY KEY (PDEN_ID, POOL_ID));

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
                OIL_CUM_VOLUME REAL NOT NULL,
                GAS_VOLUME REAL NOT NULL,
                GAS_VOLUME_OUOM TEXT NULL,
                WATER_VOLUME REAL NOT NULL,
                WATER_VOLUME_OUOM TEXT NULL,
                PRIMARY KEY (PDEN_ID, VOLUME_DATE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO));

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

            INSERT INTO FIELD (FIELD_ID, FIELD_NAME, AREA_ID, FIELD_TYPE, DISCOVERY_DATE, ACTIVE_IND) VALUES
                ('F-1', 'Alpha field', 'AREA-1', 'OIL', '2005-03-01', 'Y'),
                ('F-2', 'Bravo field', 'AREA-2', 'GAS', '2010-07-01', 'Y'),
                ('F-3', 'Charlie field', 'AREA-1', 'OIL', '1998-01-15', 'N');

            INSERT INTO POOL (POOL_ID, FIELD_ID, POOL_NAME, POOL_STATUS, POOL_TYPE, STRAT_UNIT_ID, DISCOVERY_DATE, ACTIVE_IND) VALUES
                ('P-1', 'F-1', 'Upper sand', 'PRODUCING', 'OIL', 'STRAT-A', '2005-03-01', 'Y'),
                ('P-2', 'F-1', 'Lower sand', 'PRODUCING', 'OIL', 'STRAT-B', '2006-05-01', 'Y'),
                ('P-9', 'F-2', 'Bravo sand', 'UNDEVELOPED', 'GAS', 'STRAT-Z', '2010-07-01', 'Y');

            INSERT INTO PDEN_FIELD (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, FIELD_ID, NO_OF_OIL_WELLS, NO_OF_GAS_WELLS,
                NO_OF_INJECTION_WELLS, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'F-1', 2, 0, 0, 'Y'),
                ('PD-2', 'WELL', 'PPDM', 'F-1', 1, 0, 0, 'Y'),
                ('PD-3', 'WELL', 'PPDM', 'F-3', 1, 0, 0, 'Y');

            INSERT INTO PDEN_POOL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, POOL_ID, NO_OF_OIL_WELLS, NO_OF_GAS_WELLS,
                NO_OF_INJECTION_WELLS, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'P-1', 2, 0, 0, 'Y'),
                ('PD-2', 'WELL', 'PPDM', 'P-2', 1, 0, 0, 'Y');

            INSERT INTO PDEN_WELL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PRIMARY_UWI, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'WELL-1', 'Y'),
                ('PD-2', 'WELL', 'PPDM', 'WELL-2', 'Y');

            INSERT INTO DEVELOPMENT_POOL_PLAN (POOL_PLAN_ID, FDP_ID, PLAN_VERSION, FIELD_ID, POOL_ID, POOL_NAME,
                RESERVOIR, DRIVE_MECHANISM, OOIP, OOIP_OUOM, RECOVERY_FACTOR, PLANNED_WELLS, FACILITY_ID) VALUES
                ('PP-1', 'FDP-1', 3, 'F-1', 'P-1', 'Upper sand', 'STRAT-A', 'water drive', 100, 'MMbbl', 0.35, 4, 'FAC-1');
            """;

        command.ExecuteNonQuery();

        using var fields = connection.CreateCommand();
        fields.CommandText = filler.ToString();
        fields.ExecuteNonQuery();

        using var volumes = connection.CreateCommand();
        volumes.CommandText = days.ToString();
        volumes.ExecuteNonQuery();

        using var more = connection.CreateCommand();
        more.CommandText =
            """
            -- PD-2 reported on the day only: 50 bbl oil, 25 bbl water, cumulative 2000.
            INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO,
                ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, OIL_CUM_VOLUME, GAS_VOLUME, GAS_VOLUME_OUOM,
                WATER_VOLUME, WATER_VOLUME_OUOM) VALUES
                ('PD-2', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 50, 'bbl', 2000, 0, 'MMscf', 25, 'bbl');

            -- an inactive field's measurement still reads, and names the field as inactive
            INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO,
                ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, OIL_CUM_VOLUME, GAS_VOLUME, GAS_VOLUME_OUOM,
                WATER_VOLUME, WATER_VOLUME_OUOM) VALUES
                ('PD-3', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-10-01', 20, 'bbl', 900, 0, 'MMscf', 0, 'bbl');

            -- AC-3: a row in another unit and a row with no unit, both excluded from the totals
            INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO,
                ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, OIL_CUM_VOLUME, GAS_VOLUME, GAS_VOLUME_OUOM,
                WATER_VOLUME, WATER_VOLUME_OUOM) VALUES
                ('PD-2', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '2026-09-28', 30, 'm3', 0, 0, 'MMscf', 0, 'bbl');

            INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO,
                ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, OIL_CUM_VOLUME, GAS_VOLUME, GAS_VOLUME_OUOM,
                WATER_VOLUME, WATER_VOLUME_OUOM) VALUES
                ('PD-2', 'WELL', 'PPDM', 'PRODUCTION', 1, 'Y', '2026-09-29', 40, NULL, 1800, 0, 'MMscf', 0, 'bbl');
            """;
        more.ExecuteNonQuery();
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
