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
/// RES-04 against a real SQLite database: a saved scheme links to the pool and the screening inputs it was made with
/// (AC-5), the run and every candidate keep their state and deciding rule, and an unavailable history is a state.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class EorScreeningGatewayTests
{
    private const string ConnectionName = "res04-eor";

    [IntegrationFact]
    public async Task A_saved_scheme_links_the_pool_to_its_inputs_and_keeps_every_rule()
    {        var root = Path.Combine(OgPaths.DataRoot, "res04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRes04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "eor.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "eor.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgEorScreeningService(() => source, reporter);

        // ---- an empty history ------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.History);
        Assert.False(loaded.Value.HistoryFailed);

        // ---- screen and save --------------------------------------------------
        var inputs = new OgEorInputs("F-1", "P-1", 35m, 2m, 8000m, 180m);

        var screened = await service.ScreenAsync(inputs);

        Assert.True(screened.Success, screened.Message);
        Assert.Equal(9, screened.Value!.Candidates.Count);

        var saved = await service.SaveAsync(screened.Value, "CO2_MISCIBLE", "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.NotNull(saved.Value!.RunId);

        // AC-5: the scheme links the pool to the inputs it was screened with.
        Assert.Equal(1, CountRows(database, "EOR_SCREENING_RUN"));
        Assert.Equal(9, CountRows(database, "EOR_SCREENING_CANDIDATE"));

        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM EOR_SCREENING_RUN WHERE POOL_ID = 'P-1' AND FIELD_ID = 'F-1' AND API_GRAVITY = 35 " +
            "AND VISCOSITY_CP = 2 AND DEPTH_FT = 8000 AND CHOSEN_METHOD = 'CO2_MISCIBLE';"));

        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM EOR_SCREENING_CANDIDATE WHERE CHOSEN_IND = 'Y' AND METHOD_CODE = 'CO2_MISCIBLE';"));

        // Every candidate kept the rule that decided it (AC-1) and its state (AC-2 is the distinct state).
        Assert.Equal(0, Scalar(database,
            "SELECT COUNT(*) FROM EOR_SCREENING_CANDIDATE WHERE DECIDING_RULE IS NULL OR DECIDING_RULE = '';"));

        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM EOR_SCREENING_CANDIDATE WHERE METHOD_CODE = 'STEAM' AND STATE = 'INELIGIBLE';"));

        // The chemical cost is stored with its basis, and the method without one stores nothing.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM EOR_SCREENING_CANDIDATE WHERE METHOD_CODE = 'POLYMER' AND COST_VALUE IS NOT NULL " +
            "AND COST_BASIS LIKE '%not a cost per incremental barrel%';"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM EOR_SCREENING_CANDIDATE WHERE METHOD_CODE = 'STEAM' AND COST_VALUE IS NULL;"));

        // ---- the history reads the run ---------------------------------------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);

        var scheme = Assert.Single(reloaded.Value!.History);

        Assert.Equal("P-1", scheme.PoolId);
        Assert.Equal("F-1", scheme.FieldId);
        Assert.Equal(35m, scheme.ApiGravity);
        Assert.Equal(2m, scheme.ViscosityCp);
        Assert.Equal(8000m, scheme.DepthFt);
        Assert.Equal("CO2_MISCIBLE", scheme.ChosenMethod);
        Assert.Equal("sara", scheme.Actor);
        Assert.True(scheme.EligibleCount > 0);
        Assert.Contains("pool P-1", scheme.Describe(), StringComparison.Ordinal);

        // ---- a second run for another pool keeps both ------------------------
        var heavy = await service.ScreenAsync(new OgEorInputs("F-1", "P-2", 12m, 3000m, 2000m, 120m));

        Assert.True(heavy.Success, heavy.Message);

        var heavySaved = await service.SaveAsync(heavy.Value!, "STEAM", "sara");

        Assert.True(heavySaved.Success, heavySaved.Message);
        Assert.Equal(2, CountRows(database, "EOR_SCREENING_RUN"));
        Assert.Equal(18, CountRows(database, "EOR_SCREENING_CANDIDATE"));
        Assert.Equal(2, (await service.LoadAsync()).Value!.History.Count);

        // ---- an ineligible method cannot be saved as the scheme --------------
        var refused = await service.SaveAsync(heavy.Value!, "CO2_MISCIBLE", "sara");

        Assert.False(refused.Success);
        Assert.Equal(EorRules.CodeChosenIneligible, refused.Reference);
        Assert.Equal(2, CountRows(database, "EOR_SCREENING_RUN"));

        // ---- an unavailable history is a state, not a dead page --------------
        Drop(database, "EOR_SCREENING_CANDIDATE");
        Drop(database, "EOR_SCREENING_RUN");

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.History);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);

        // A new screen still works with the history gone.
        Assert.True((await service.ScreenAsync(inputs)).Success);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE EOR_SCREENING_RUN (
                RUN_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                POOL_ID TEXT NULL,
                API_GRAVITY REAL NULL,
                VISCOSITY_CP REAL NULL,
                DEPTH_FT REAL NULL,
                TEMPERATURE_F REAL NULL,
                CANDIDATE_COUNT INTEGER NOT NULL,
                ELIGIBLE_COUNT INTEGER NOT NULL,
                INSUFFICIENT_COUNT INTEGER NOT NULL,
                CHOSEN_METHOD TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE EOR_SCREENING_CANDIDATE (
                CANDIDATE_ID TEXT NOT NULL PRIMARY KEY,
                RUN_ID TEXT NOT NULL,
                METHOD_CODE TEXT NOT NULL,
                STATE TEXT NOT NULL,
                DECIDING_RULE TEXT NOT NULL,
                SCORE REAL NULL,
                INCREMENTAL_RECOVERY REAL NULL,
                COST_VALUE REAL NULL,
                COST_BASIS TEXT NULL,
                CHOSEN_IND TEXT NOT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
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
