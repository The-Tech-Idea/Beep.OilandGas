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
/// WTA-01 against a real SQLite database: a result saves with its window, method and cross-check (AC-6), a refused analysis
/// writes nothing, a save whose acknowledgement was lost is retained and retried, and an unavailable history is a state.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WellTestGatewayTests
{
    private const string ConnectionName = "wta01-welltest";

    [IntegrationFact]
    public async Task A_result_saves_with_its_window_method_and_cross_check()
    {        var root = Path.Combine(OgPaths.DataRoot, "wta01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWta01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "welltest.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "welltest.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellTestService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Results);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains("BUILDUP", loaded.Value.Types);
        Assert.Contains("HORNER", loaded.Value.Methods);
        Assert.Contains("type-curve matching", loaded.Value.SupportedText, StringComparison.Ordinal);
        Assert.Contains("HornerAnalysis", loaded.Value.ReferenceText, StringComparison.Ordinal);

        // ---- AC-3: a window with no reading is refused and writes nothing ----
        var noWindow = await service.AnalyseAsync(Inputs() with { WindowStartHours = 500m, WindowEndHours = 600m });

        Assert.False(noWindow.Success);
        Assert.Equal(WellTestRules.CodeNoWindow, noWindow.Reference);
        Assert.Equal(0, CountRows(database));

        // ---- analyse and save the reference build-up ------------------------
        var analysed = await service.AnalyseAsync(Inputs());

        Assert.True(analysed.Success, analysed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = analysed.Value!.Selected!;

        Assert.True(result.Converged);
        Assert.InRange(result.Permeability, 49.5m, 50.5m);
        Assert.True(result.CrossCheck.Agrees, result.CrossCheck.Text);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6: the window, the method and the cross-check are all on the row.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM WELL_TEST_ANALYSIS_RESULT WHERE ANALYSIS_TYPE = 'BUILDUP' " +
            "AND ANALYSIS_METHOD = 'HORNER' AND WINDOW_START_HOURS = 1 AND WINDOW_END_HOURS = 60 " +
            "AND POINTS_IN_WINDOW = 9 AND CONVERGED_IND = 'Y' " +
            "AND PERMEABILITY BETWEEN 49.5 AND 50.5 AND SKIN_FACTOR <> 0 " +
            "AND ANALYSIS_SLOPE > 0 AND CROSS_CHECK_SLOPE > 0 AND CROSS_CHECK_AGREES = 'Y' " +
            "AND CROSS_CHECK_SOURCE LIKE '%HornerAnalysis%' AND DERIVATIVE_USABLE_IND = 'Y' " +
            "AND DATA_UNITS_TEXT LIKE '%psia%' AND ACTOR = 'sara';"));

        // ---- the reload keeps the window, the method and the cross-check ----
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(WellTestAnalyses.BuildUp, again.AnalysisType);
        Assert.Equal(WellTestAnalyses.Horner, again.Method);
        Assert.Equal(1m, again.WindowStartHours);
        Assert.Equal(60m, again.WindowEndHours);
        Assert.Equal(9, again.PointsInWindow);
        Assert.InRange(Math.Abs(result.Permeability - again.Permeability), 0m, 0.001m);
        Assert.True(again.CrossCheck.Agrees, again.CrossCheck.Text);
        Assert.True(again.DerivativeUsable);
        Assert.Equal("sara", again.Actor);

        // ---- a drawdown saves with its own method ---------------------------
        var drawdown = (await service.AnalyseAsync(Inputs() with
        {
            AnalysisType = WellTestAnalyses.DrawDown,
            Method = WellTestAnalyses.Mdh,
        })).Value!.Selected!;

        Assert.Equal(WellTestAnalyses.DrawDown, drawdown.AnalysisType);

        Assert.True((await service.SaveAsync(drawdown, "sara")).Success);
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM WELL_TEST_ANALYSIS_RESULT WHERE ANALYSIS_TYPE = 'DRAWDOWN' " +
            "AND ANALYSIS_METHOD = 'MDH' AND CROSS_CHECK_AGREES IS NULL;"));

        // ---- the state table's "lost acknowledgement" ------------------------
        Drop(database);

        var lost = await service.SaveAsync(result, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal("WELL-1", service.Pending!.Result.WellId);

        CreateTable(database);

        var retried = await service.RetrySaveAsync("sara");

        Assert.True(retried.Success, retried.Message);
        Assert.Null(service.Pending);
        Assert.Equal(1, CountRows(database));

        // ---- an unavailable history is a state -------------------------------
        Drop(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Results);
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
            CREATE TABLE WELL_TEST_ANALYSIS_RESULT (
                WELL_TEST_ANALYSIS_RESULT_ID TEXT NOT NULL PRIMARY KEY,
                WELL_TEST_DATA_ID TEXT NULL,
                PERMEABILITY REAL NOT NULL,
                SKIN_FACTOR REAL NOT NULL,
                RESERVOIR_PRESSURE REAL NOT NULL,
                PRODUCTIVITY_INDEX REAL NOT NULL,
                FLOW_EFFICIENCY REAL NOT NULL,
                DAMAGE_RATIO REAL NOT NULL,
                RADIUS_OF_INVESTIGATION REAL NOT NULL,
                IDENTIFIED_MODEL TEXT NULL,
                R_SQUARED REAL NOT NULL,
                ANALYSIS_METHOD TEXT NULL,
                AREA_ID TEXT NULL,
                AREA_TYPE TEXT NULL,
                BUSINESS_ASSOCIATE_ID TEXT NULL,
                ANALYSIS_ID TEXT NULL,
                CALCULATION_ID TEXT NULL,
                WELL_ID TEXT NULL,
                WELL_UWI TEXT NULL,
                TEST_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                ANALYSIS_DATE TEXT NOT NULL,
                CALCULATION_DATE TEXT NOT NULL,
                ANALYSIS_BY_USER TEXT NULL,
                IDENTIFIED_MODEL_STRING TEXT NULL,
                ANALYSIS_TYPE TEXT NULL,
                FLOW_RATE REAL NOT NULL,
                WELLBORE_RADIUS REAL NOT NULL,
                FORMATION_THICKNESS REAL NOT NULL,
                POROSITY REAL NOT NULL,
                TOTAL_COMPRESSIBILITY REAL NOT NULL,
                OIL_VISCOSITY REAL NOT NULL,
                OIL_FORMATION_VOLUME_FACTOR REAL NOT NULL,
                PRODUCTION_TIME REAL NOT NULL,
                IS_GAS_WELL INTEGER NOT NULL,
                GAS_SPECIFIC_GRAVITY REAL NOT NULL,
                RESERVOIR_TEMPERATURE REAL NOT NULL,
                INITIAL_RESERVOIR_PRESSURE REAL NOT NULL,
                DIAGNOSTIC_DATA_JSON TEXT NULL,
                DERIVATIVE_DATA_JSON TEXT NULL,
                STATUS TEXT NULL,
                ERROR_MESSAGE TEXT NULL,
                USER_ID TEXT NULL,
                IS_SUCCESSFUL INTEGER NOT NULL,
                RSQUARED REAL NOT NULL,
                WINDOW_START_HOURS REAL NULL,
                WINDOW_END_HOURS REAL NULL,
                POINTS_IN_WINDOW INTEGER NULL,
                WINDOW_TEXT TEXT NULL,
                CONVERGED_IND TEXT NOT NULL,
                DERIVATIVE_USABLE_IND TEXT NOT NULL,
                DERIVATIVE_REASON TEXT NULL,
                DERIVATIVE_POINTS INTEGER NULL,
                CROSS_CHECK_SOURCE TEXT NULL,
                CROSS_CHECK_SLOPE REAL NULL,
                ANALYSIS_SLOPE REAL NULL,
                CROSS_CHECK_PERMEABILITY REAL NULL,
                CROSS_CHECK_AGREES TEXT NULL,
                CROSS_CHECK_TEXT TEXT NULL,
                DATA_UNITS_TEXT TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static OgWellTestInputs Inputs() =>
        new(
            "WELL-1",
            WellTestAnalyses.BuildUp,
            WellTestAnalyses.Horner,
            TimeUnit: "h",
            PressureUnit: "psia",
            RateUnit: "STB/d",
            Readings: Readings(),
            WindowStartHours: 1m,
            WindowEndHours: 60m,
            FlowRate: 500m,
            WellboreRadius: 0.35m,
            Thickness: 40m,
            Porosity: 0.2m,
            TotalCompressibility: 0.000012m,
            Viscosity: 1.5m,
            FormationVolumeFactor: 1.2m,
            ProductionTimeHours: 200m,
            InitialReservoirPressure: 3000m);

    /// <summary>Nine readings on the published Horner line for 50 md.</summary>
    private static IReadOnlyList<OgWellTestReading> Readings()
    {
        const decimal slope = 162.6m * 500m * 1.2m * 1.5m / (50m * 40m);
        const decimal producingTime = 200m;

        return new[] { 1m, 2m, 4m, 8m, 12m, 20m, 30m, 45m, 60m }
            .Select(dt =>
            {
                var horner = (producingTime + dt) / dt;
                var pressure = 3000m - slope * (decimal)Math.Log10((double)horner);

                return new OgWellTestReading(dt, pressure);
            })
            .ToList();
    }

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM WELL_TEST_ANALYSIS_RESULT;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE WELL_TEST_ANALYSIS_RESULT;";
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
