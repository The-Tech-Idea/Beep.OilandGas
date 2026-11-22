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
/// RES-03 against a real SQLite database: the EOS list comes from the reference table when it has rows; a converged run
/// saves its feed, its model, its iteration count and its actor into the owner's result table; the history reloads the
/// feed and the phase split; and an unavailable history is a state. Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class FlashGatewayTests
{
    private const string ConnectionName = "res03-flash";

    [IntegrationFact]
    public async Task A_flash_run_saves_its_feed_model_and_actor_and_reloads()
    {        var root = Path.Combine(OgPaths.DataRoot, "res03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRes03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "flash.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "flash.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgFlashService(() => source, reporter);

        // ---- the EOS list comes from the reference table ----------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.History);
        Assert.True(loaded.Value.EosFromReference);
        Assert.Equal(["IDEAL_K", "PR", "SRK"], loaded.Value.EosCodes);
        Assert.Contains("R_FLASH_CALCULATION_REFERENCE_CODE", loaded.Value.EosText, StringComparison.Ordinal);

        // ---- a converged run saves its feed, model, iterations and actor -----
        var inputs = new OgFlashInputs(
            [
                new OgFlashComponent("C1", 0.50m, 343.0m, 667.8m, 0.0104m, 16.04m),
                new OgFlashComponent("C2", 0.20m, 549.8m, 707.8m, 0.0986m, 30.07m),
                new OgFlashComponent("C3", 0.15m, 665.7m, 616.3m, 0.1524m, 44.10m),
                new OgFlashComponent("NC4", 0.15m, 765.3m, 550.7m, 0.2002m, 58.12m),
            ],
            500m,
            600m,
            "IDEAL_K");

        var run = await service.RunAsync(inputs);

        Assert.True(run.Success, run.Message);
        Assert.False(run.Value!.IsFailed, run.Value.FailureText);
        Assert.Equal(FlashStatusCodes.Success, run.Value.StatusCode);

        var computed = run.Value;

        var saved = await service.SaveAsync(computed, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Single(saved.Value!.History);

        var stored = saved.Value.History[0];

        Assert.True(stored.IsSaved);
        Assert.Equal("IDEAL_K", stored.EosCode);
        Assert.Equal("sara", stored.Actor);
        Assert.NotNull(stored.RecordedAt);
        Assert.Equal(500m, stored.Pressure);
        Assert.Equal(600m, stored.Temperature);
        Assert.Equal(4, stored.Feed.Count);
        Assert.Equal(4, stored.Components.Count);
        Assert.InRange(Math.Abs(stored.VapourFraction - computed.VapourFraction), 0m, 0.000001m);
        Assert.InRange(Math.Abs(stored.FeedSum - 1m), 0m, 0.0001m);

        // The stored row carries the model, the iteration count and the status the feature names.
        Assert.Equal(1, CountRows(database));
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM FLASH_CALCULATION_RESULT WHERE EOS_CODE = 'IDEAL_K';"));
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM FLASH_CALCULATION_RESULT WHERE STATUS = 'SUCCESS';"));
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM FLASH_CALCULATION_RESULT WHERE CONVERGED = 'Y';"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM FLASH_CALCULATION_RESULT WHERE ITERATIONS > 0 AND FEED_COMPOSITION_JSON IS NOT NULL;"));

        // ---- the history reloads the feed and the phase split -----------------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);

        var again = Assert.Single(reloaded.Value!.History);

        Assert.Equal("C1", again.Feed[0].Name);
        Assert.Equal(0.50m, again.Feed[0].MoleFraction);
        Assert.Equal(343.0m, again.Feed[0].CriticalTemperature);
        Assert.Equal(4, again.Components.Count);
        Assert.Equal("C1", again.Components[0].Name);
        Assert.InRange(Math.Abs(again.Components[0].K - computed.Components[0].K), 0m, 0.000001m);
        Assert.Equal(FlashPhaseStates.TwoPhase, again.PhaseState);

        // Saving an already-saved run does not duplicate it.
        Assert.True((await service.SaveAsync(again, "sara")).Success);
        Assert.Equal(1, CountRows(database));

        // ---- the cubic path runs and is honest about a single phase -----------
        var cubic = await service.RunAsync(inputs with { EosCode = "PR" });

        Assert.True(cubic.Success, cubic.Message);
        Assert.False(cubic.Value!.IsFailed, cubic.Value.FailureText + " :: " + string.Join(" | ", reporter.Failures));
        Assert.Equal("PR", cubic.Value.EosCode);
        Assert.Contains("Peng-Robinson", cubic.Value.EosName, StringComparison.Ordinal);

        // At these conditions the stability test answers single-phase; that is reported, not turned into a phase split.
        if (string.Equals(cubic.Value.PhaseState, FlashPhaseStates.AllVapour, StringComparison.Ordinal))
        {
            Assert.NotNull(cubic.Value.StabilityText);
            Assert.Contains("single-phase", cubic.Value.StabilityText!, StringComparison.OrdinalIgnoreCase);
        }

        Assert.True((await service.SaveAsync(cubic.Value, "sara")).Success);
        Assert.Equal(2, CountRows(database));
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM FLASH_CALCULATION_RESULT WHERE EOS_CODE = 'PR';"));

        // ---- an empty reference set falls back to the mapping, and says so ----
        ClearReference(database);

        var withoutReference = await service.LoadAsync();

        Assert.True(withoutReference.Success);
        Assert.False(withoutReference.Value!.EosFromReference);
        Assert.Equal(FlashRules.MappingEosCodes, withoutReference.Value.EosCodes);
        Assert.Contains("no FLASH_EOS_MODEL row", withoutReference.Value.EosText, StringComparison.Ordinal);

        // ---- an unavailable history is a state, not a dead page ---------------
        Drop(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.History);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);

        // A new run still works with the history gone.
        var afterDrop = await service.RunAsync(inputs);

        Assert.True(afterDrop.Success, afterDrop.Message);
        Assert.False(afterDrop.Value!.IsFailed);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE R_FLASH_CALCULATION_REFERENCE_CODE (
                REFERENCE_SET TEXT NOT NULL,
                REFERENCE_CODE TEXT NOT NULL,
                LONG_NAME TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                PRIMARY KEY (REFERENCE_SET, REFERENCE_CODE));

            CREATE TABLE FLASH_CALCULATION_RESULT (
                FLASH_CALCULATION_RESULT_ID TEXT NOT NULL PRIMARY KEY,
                CALCULATION_ID TEXT NULL,
                FLASH_CALCULATION_ID TEXT NULL,
                COMPONENT_ID TEXT NULL,
                WELL_ID TEXT NULL,
                FACILITY_ID TEXT NULL,
                PRESSURE REAL NULL,
                TEMPERATURE REAL NULL,
                VAPOR_FRACTION REAL NULL,
                LIQUID_FRACTION REAL NULL,
                ITERATIONS INTEGER NULL,
                CONVERGED TEXT NULL,
                CONVERGENCE_ERROR REAL NULL,
                FEED_COMPOSITION_JSON TEXT NULL,
                VAPOR_COMPOSITION_JSON TEXT NULL,
                LIQUID_COMPOSITION_JSON TEXT NULL,
                K_VALUES_JSON TEXT NULL,
                CALCULATION_DATE TEXT NULL,
                STATUS TEXT NULL,
                ERROR_MESSAGE TEXT NULL,
                EOS_CODE TEXT NULL,
                STABILITY_TEXT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO R_FLASH_CALCULATION_REFERENCE_CODE (REFERENCE_SET, REFERENCE_CODE, LONG_NAME) VALUES
                ('FLASH_EOS_MODEL', 'PR', 'Peng-Robinson equation of state'),
                ('FLASH_EOS_MODEL', 'SRK', 'Soave-Redlich-Kwong equation of state'),
                ('FLASH_EOS_MODEL', 'IDEAL_K', 'Ideal K-value / simplified vapor-liquid split'),
                ('FLASH_STATUS_CODE', 'SUCCESS', 'The flash converged and its result is valid');
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

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM FLASH_CALCULATION_RESULT;");

    private static void ClearReference(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM R_FLASH_CALCULATION_REFERENCE_CODE;";
        command.ExecuteNonQuery();
    }

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE FLASH_CALCULATION_RESULT;";
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
