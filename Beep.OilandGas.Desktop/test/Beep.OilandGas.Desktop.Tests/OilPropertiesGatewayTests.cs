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
/// RES-02 against a real SQLite database: a saved screening records its inputs and correlation (AC-5), including whether
/// the default gas gravity was accepted (AC-3) and whether a result was out of range (AC-2); the history reloads; and an
/// unavailable history is a state with the banner still required. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class OilPropertiesGatewayTests
{
    private const string ConnectionName = "res02-oil";

    [IntegrationFact]
    public async Task A_screening_is_saved_with_its_inputs_correlation_and_range_mark()
    {        var root = Path.Combine(OgPaths.DataRoot, "res02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRes02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "oil.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "oil.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgOilPropertiesService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.History);
        Assert.Equal(0.65m, loaded.Value.DefaultGasSpecificGravity);
        Assert.Equal(OilPropertyRules.ScreeningBanner, loaded.Value.ScreeningBanner);
        Assert.Contains(OilUnits.Rankine, loaded.Value.TemperatureUnits);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains("only when the defaults are accepted explicitly", loaded.Value.DefaultGasGravityText,
            StringComparison.Ordinal);

        // ---- AC-5: save keeps the inputs and the correlation ------------------
        var run = await service.RunAsync(new OgOilPropertyInputs(
            35m, 0.65m, false, 200m, OilUnits.Fahrenheit, 3000m, 600m, OilCorrelations.Combined));

        Assert.True(run.Success, run.Message);
        Assert.False(run.Value!.IsFailed, run.Value.FailureText);

        var computed = run.Value;

        var saved = await service.SaveAsync(computed, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Single(saved.Value!.History);

        var stored = saved.Value.History[0];

        Assert.True(stored.IsSaved);
        Assert.Equal(35m, stored.ApiGravity);
        Assert.Equal(0.65m, stored.GasSpecificGravity);
        Assert.False(stored.GasGravityAssumed);
        Assert.Equal(3000m, stored.Pressure);
        Assert.Equal(200m, stored.TemperatureF);
        Assert.Equal(OilCorrelations.Combined, stored.Correlation);
        Assert.Equal("sara", stored.Actor);
        Assert.InRange(Math.Abs(stored.BubblePoint!.Value - computed.BubblePoint!.Value), 0m, 0.01m);
        Assert.False(stored.OutsideRange);
        Assert.Equal(1, CountRows(database));

        // ---- AC-3: the accepted default is recorded as such -------------------
        var withDefault = await service.RunAsync(new OgOilPropertyInputs(
            35m, null, true, 200m, OilUnits.Fahrenheit, 3000m, 600m, OilCorrelations.Combined));

        Assert.True(withDefault.Success, withDefault.Message);
        Assert.True(withDefault.Value!.GasGravityAssumed);

        var defaultSaved = await service.SaveAsync(withDefault.Value, "sara");

        Assert.True(defaultSaved.Success, defaultSaved.Message);
        Assert.Equal(2, CountRows(database));

        var defaultStored = defaultSaved.Value!.History.Single(item => item.GasGravityAssumed);

        Assert.Equal(0.65m, defaultStored.GasSpecificGravity);
        Assert.Contains("accepted explicitly", defaultStored.GasGravityText, StringComparison.Ordinal);
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM OIL_PROPERTY_RESULT WHERE GAS_GRAVITY_ASSUMED_IND = 'Y';"));

        // ---- AC-2: the range mark is stored with the reason -------------------
        var wide = await service.RunAsync(new OgOilPropertyInputs(
            40m, 0.8m, false, 250m, OilUnits.Fahrenheit, 12000m, 4000m, OilCorrelations.Combined));

        Assert.True(wide.Success, wide.Message);
        Assert.True(wide.Value!.OutsideRange);

        var wideSaved = await service.SaveAsync(wide.Value, "sara");

        Assert.True(wideSaved.Success, wideSaved.Message);

        var wideStored = wideSaved.Value!.History.Single(item => item.OutsideRange);

        Assert.NotEmpty(wideStored.RangeNotes);
        Assert.Contains("outside", wideStored.RangeText, StringComparison.Ordinal);
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM OIL_PROPERTY_RESULT WHERE OUTSIDE_RANGE_IND = 'Y';"));

        // ---- the history reloads the same figures -----------------------------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);
        Assert.Equal(3, reloaded.Value!.History.Count);

        var reloadedRun = reloaded.Value.History.Single(item => !item.OutsideRange && !item.GasGravityAssumed);

        Assert.InRange(Math.Abs(reloadedRun.BubblePoint!.Value - computed.BubblePoint!.Value), 0m, 0.01m);
        Assert.InRange(Math.Abs(reloadedRun.FormationVolumeFactor!.Value - computed.FormationVolumeFactor!.Value),
            0m, 0.000001m);

        // Saving an already-saved result does not duplicate it.
        Assert.True((await service.SaveAsync(reloadedRun, "sara")).Success);
        Assert.Equal(3, CountRows(database));

        // ---- an unavailable history is a state, not a dead page ---------------
        Drop(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.History);
        Assert.Equal(OilPropertyRules.ScreeningBanner, unavailable.Value.ScreeningBanner);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);

        // A new run still works with the history gone.
        var afterDrop = await service.RunAsync(new OgOilPropertyInputs(
            35m, 0.65m, false, 200m, OilUnits.Fahrenheit, 3000m, 600m, OilCorrelations.Combined));

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
            CREATE TABLE OIL_PROPERTY_RESULT (
                CALCULATION_ID TEXT NOT NULL PRIMARY KEY,
                OIL_COMPOSITION_ID TEXT NULL,
                PRESSURE REAL NULL,
                TEMPERATURE REAL NULL,
                FORMATION_VOLUME_FACTOR REAL NULL,
                DENSITY REAL NULL,
                VISCOSITY REAL NULL,
                COMPRESSIBILITY REAL NULL,
                CALCULATION_DATE TEXT NULL,
                CORRELATION_METHOD TEXT NULL,
                API_GRAVITY REAL NULL,
                GAS_SPECIFIC_GRAVITY REAL NULL,
                GAS_GRAVITY_ASSUMED_IND TEXT NOT NULL,
                SOLUTION_GOR REAL NULL,
                BUBBLE_POINT_PRESSURE REAL NULL,
                DEAD_OIL_VISCOSITY REAL NULL,
                OUTSIDE_RANGE_IND TEXT NOT NULL,
                RANGE_NOTES TEXT NULL,
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

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM OIL_PROPERTY_RESULT;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE OIL_PROPERTY_RESULT;";
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
