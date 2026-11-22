using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.WellTest;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WTA-02 against a real SQLite database: a test registers against one well and reloads (AC-1, SG-08-4), the type and
/// result codes are validated against the reference tables (AC-2), a duplicate well+date+type is refused (AC-3), a test
/// with no result code reads as pending rather than failed (AC-4), the units survive (AC-5), and a test with an analysis
/// linked to it cannot be deleted while one without can (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WellTestRegisterGatewayTests
{
    private const string ConnectionName = "wta02-register";

    [IntegrationFact]
    public async Task A_test_registers_links_to_one_well_and_cannot_be_deleted_while_analysed()
    {        var root = Path.Combine(OgPaths.DataRoot, "wta02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWta02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "register.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "register.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellTestRegisterService(() => source, reporter);

        // ---- AC-2: the codes come from the reference tables -------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Tests);
        Assert.True(loaded.Value.ReferenceFromTables);
        Assert.Equal(["BUILDUP", "DRAWDOWN"], loaded.Value.TestTypes);
        Assert.Equal(["FAILED", "OK"], loaded.Value.ResultCodes);
        Assert.Contains("R_WELL_TEST_TYPE", loaded.Value.ReferenceText, StringComparison.Ordinal);

        // ---- AC-3: an unknown code is refused before anything is written ------
        var unknownType = await service.SaveAsync(Entry(type: "ISOCHRONAL"), "sara");

        Assert.False(unknownType.Success);
        Assert.Equal(WellTestRegisterRules.CodeTypeUnknown, unknownType.Reference);
        Assert.Equal(0, CountRows(database));

        // ---- register a test -------------------------------------------------
        var saved = await service.SaveAsync(Entry(), "sara");

        Assert.True(saved.Success, saved.Message + " :: " + string.Join(" | ", reporter.Failures));

        var test = saved.Value!.Selected!;

        // AC-1 and SG-08-4: one well, and the natural key carries it.
        Assert.Equal("WELL-1", test.WellId);
        Assert.Equal("WELL-1|BUILDUP|1|1", test.TestKey);
        Assert.Equal(1, CountRows(database));
        // One predicate per assertion, so a stored shape that differs says which column it is.
        foreach (var (predicate, label) in new[]
                 {
                     ("WELL_UWI = 'WELL-1'", "well"),
                     ("TEST_TYPE = 'BUILDUP'", "type"),
                     ("TEST_DATE LIKE '2026-03-14%'", "date"),
                     ("TEST_KEY = 'WELL-1|BUILDUP|1|1'", "key"),
                     ("RUN_NUM = 1 AND TEST_NUM = 1", "run and test number"),
                     ("DURATION = 24 AND DURATION_OUOM = 'h'", "duration"),
                     ("RESULT_CODE IS NULL", "pending result"),
                     ("OIL_RATE = 1200 AND OIL_RATE_OUOM = 'bbl/d'", "oil rate and unit"),
                     ("GAS_RATE = 900 AND GAS_RATE_OUOM = 'Mscf/d'", "gas rate and unit"),
                     ("FLOWING_PRESSURE = 2400 AND FLOWING_PRESSURE_OUOM = 'psia'", "pressure and unit"),
                     ("ACTOR = 'sara'", "actor"),
                 })
        {
            Assert.True(
                Scalar(database, $"SELECT COUNT(*) FROM WELL_TEST_REGISTER WHERE {predicate};") == 1,
                "the stored row does not match on " + label);
        }

        // ---- AC-4 and AC-5 on the reload -------------------------------------
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.True(again.PendingResult);
        Assert.Equal("pending result", again.StatusText);
        Assert.Contains("1,200 bbl/d", again.RatesText, StringComparison.Ordinal);
        Assert.Contains("flowing 2,400 psia", again.RatesText, StringComparison.Ordinal);
        Assert.Contains("24 h", again.DurationText, StringComparison.Ordinal);
        Assert.Equal("Pressure build-up", again.TestTypeName);
        Assert.False(again.HasAnalysis);
        Assert.Contains("not analysed", again.AnalysisText, StringComparison.Ordinal);

        // ---- AC-3: the duplicate is refused against the real rows ------------
        var duplicate = await service.SaveAsync(Entry(well: "WELL-1", type: "BUILDUP"), "sara");

        Assert.False(duplicate.Success);
        Assert.Equal(WellTestRegisterRules.CodeDuplicate, duplicate.Reference);
        Assert.Contains("2026-03-14", duplicate.Message!, StringComparison.Ordinal);
        Assert.Equal(1, CountRows(database));

        // A different type on the same day is a different test, and is allowed.
        var drawdown = await service.SaveAsync(Entry(type: "DRAWDOWN", result: "OK"), "sara");

        Assert.True(drawdown.Success, drawdown.Message);
        Assert.Equal(2, CountRows(database));

        // ---- AC-6: a linked analysis blocks the delete -----------------------
        LinkAnalysis(database, test.TestKey);

        var afterLink = await service.LoadAsync();

        Assert.True(afterLink.Success, afterLink.Message);

        var linked = afterLink.Value!.Tests.Single(candidate => candidate.TestKey == test.TestKey);

        Assert.True(linked.HasAnalysis);
        Assert.Equal(1, linked.AnalysisCount);

        var refused = await service.DeleteAsync(test.RegisterId, "sara");

        Assert.False(refused.Success);
        Assert.Equal(WellTestRegisterRules.CodeAnalysisLinked, refused.Reference);
        Assert.Contains("analysis(es)", refused.Message!, StringComparison.Ordinal);

        // Both tests are still there: the refused delete changed nothing.
        Assert.Equal(2, CountRows(database));

        // The unanalysed test can be deleted, and the row is gone.
        var removed = await service.DeleteAsync(drawdown.Value!.Selected!.RegisterId, "sara");

        Assert.True(removed.Success, removed.Message);
        Assert.Equal(1, CountRows(database));
        Assert.DoesNotContain(removed.Value!.Tests, candidate => candidate.TestType == "DRAWDOWN");

        // ---- an unavailable register is a state -------------------------------
        Drop(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Tests);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    private static void Seed(string database) => CreateTable(database);

    private static void CreateTable(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE R_WELL_TEST_TYPE (
                WELL_TEST_TYPE TEXT NOT NULL PRIMARY KEY,
                ABBREVIATION TEXT NULL,
                ACTIVE_IND TEXT NULL,
                LONG_NAME TEXT NULL,
                SHORT_NAME TEXT NULL);

            CREATE TABLE R_TEST_RESULT (
                TEST_RESULT TEXT NOT NULL PRIMARY KEY,
                ABBREVIATION TEXT NULL,
                ACTIVE_IND TEXT NULL,
                LONG_NAME TEXT NULL,
                SHORT_NAME TEXT NULL);

            CREATE TABLE WELL_TEST_REGISTER (
                REGISTER_ID TEXT NOT NULL PRIMARY KEY,
                TEST_KEY TEXT NOT NULL,
                WELL_UWI TEXT NOT NULL,
                TEST_TYPE TEXT NOT NULL,
                RUN_NUM INTEGER NOT NULL,
                TEST_NUM INTEGER NOT NULL,
                TEST_DATE TEXT NOT NULL,
                DURATION REAL NULL,
                DURATION_OUOM TEXT NULL,
                RESULT_CODE TEXT NULL,
                PRODUCTION_METHOD TEXT NULL,
                REMARKS TEXT NULL,
                OIL_RATE REAL NULL,
                OIL_RATE_OUOM TEXT NULL,
                GAS_RATE REAL NULL,
                GAS_RATE_OUOM TEXT NULL,
                FLOWING_PRESSURE REAL NULL,
                FLOWING_PRESSURE_OUOM TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE WELL_TEST_ANALYSIS_RESULT (
                WELL_TEST_ANALYSIS_RESULT_ID TEXT NOT NULL PRIMARY KEY,
                TEST_ID TEXT NULL,
                WELL_UWI TEXT NULL);

            INSERT INTO R_WELL_TEST_TYPE (WELL_TEST_TYPE, ACTIVE_IND, LONG_NAME) VALUES
                ('BUILDUP', 'Y', 'Pressure build-up'),
                ('DRAWDOWN', 'Y', 'Drawdown');

            INSERT INTO R_TEST_RESULT (TEST_RESULT, ACTIVE_IND, LONG_NAME) VALUES
                ('OK', 'Y', 'Test completed normally'),
                ('FAILED', 'Y', 'Test failed');
            """;
        command.ExecuteNonQuery();
    }

    /// <summary>An analysis row whose TEST_ID is the test's natural key - the link AC-1 and AC-6 depend on.</summary>
    private static void LinkAnalysis(string database, string testKey)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            "INSERT INTO WELL_TEST_ANALYSIS_RESULT (WELL_TEST_ANALYSIS_RESULT_ID, TEST_ID, WELL_UWI) " +
            $"VALUES ('AN-1', '{testKey}', 'WELL-1');";
        command.ExecuteNonQuery();
    }

    private static OgWellTestEntry Entry(
        string well = "WELL-1",
        string type = "BUILDUP",
        string? result = null) =>
        new(
            null,
            well,
            new DateTime(2026, 3, 14),
            type,
            null,
            null,
            Duration: 24m,
            DurationUnit: "h",
            ResultCode: result,
            ProductionMethod: "Flowing",
            Remarks: "Reference build-up",
            OilRate: 1200m,
            OilRateUnit: "bbl/d",
            GasRate: 900m,
            GasRateUnit: "Mscf/d",
            FlowingPressure: 2400m,
            FlowingPressureUnit: "psia");

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM WELL_TEST_REGISTER;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE WELL_TEST_REGISTER;";
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
