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
/// RES-01 against a real SQLite database: a saved result records its inputs, the correlations and the actor (AC-5); the
/// converted psia/Rankine pair is stored beside the typed values so the run can be reproduced (AC-4); a failed run is
/// stored as a failure with no result (AC-3); the history reloads identically and re-running the reloaded inputs
/// reproduces the saved Z (AC-6); and an unavailable history is a state, not a dead page.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class GasPropertiesGatewayTests
{
    private const string ConnectionName = "res01-gas";
    private static readonly decimal PressurePsia = 2010.387m;
    private static readonly decimal TemperatureR = 547.665m;

    [IntegrationFact]
    public async Task A_run_is_saved_with_its_inputs_correlations_and_actor_and_reloads_identically()
    {        var root = Path.Combine(OgPaths.DataRoot, "res01-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRes01"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "gas.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "gas.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgGasPropertiesService(() => source, reporter);

        // ---- the history starts empty, with the choices on offer -----------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.History);
        Assert.Equal(GasCorrelations.Z, loaded.Value.ZCorrelations);
        Assert.Equal(GasCorrelations.Viscosity, loaded.Value.ViscosityCorrelations);
        Assert.Contains(GasUnits.Kilopascal, loaded.Value.PressureUnits);
        Assert.False(loaded.Value.HistoryFailed);

        // ---- the run ------------------------------------------------------------
        var inputs = new OgGasPropertyInputs(
            PressurePsia, GasUnits.Psia, TemperatureR, GasUnits.Rankine, 0.65m,
            GasCorrelations.StandingKatz, GasCorrelations.CarrKobayashiBurrows);

        var run = await service.RunAsync(inputs);

        Assert.True(run.Success, run.Message);
        Assert.False(run.Value!.IsFailed, run.Value.FailureText);
        Assert.False(run.Value.IsSaved);

        var computedZ = run.Value.ZFactor!.Value;

        // ---- AC-5: saving records inputs, correlations and actor ----------------
        var saved = await service.SaveAsync(run.Value, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Single(saved.Value!.History);

        var stored = saved.Value.History[0];

        Assert.True(stored.IsSaved);
        Assert.Equal("sara", stored.Actor);
        Assert.NotNull(stored.RecordedAt);
        Assert.Equal(PressurePsia, stored.PressurePsia);
        Assert.Equal(TemperatureR, stored.TemperatureRankine);
        Assert.Equal(0.65m, stored.SpecificGravity);
        Assert.Equal(GasCorrelations.StandingKatz, stored.ZCorrelation);
        Assert.Equal(GasCorrelations.CarrKobayashiBurrows, stored.ViscosityCorrelation);
        Assert.Contains("saved by sara", stored.SavedText, StringComparison.Ordinal);
        Assert.Equal(1, CountRows(database));

        // AC-4: the typed value and the converted pair are both stored.
        var metric = await service.RunAsync(inputs with
        {
            Pressure = 1000m,
            PressureUnit = GasUnits.Kilopascal,
            Temperature = 15m,
            TemperatureUnit = GasUnits.Celsius,
        });

        Assert.True(metric.Success, metric.Message);

        var metricSaved = await service.SaveAsync(metric.Value!, "sara");

        Assert.True(metricSaved.Success, metricSaved.Message);
        Assert.Equal(2, CountRows(database));

        var metricStored = metricSaved.Value!.History.Single(item => item.Pressure == 1000m);

        Assert.Equal(GasUnits.Kilopascal, metricStored.PressureUnit);
        Assert.Equal(145.0377377m, metricStored.PressurePsia);
        Assert.Equal(GasUnits.Celsius, metricStored.TemperatureUnit);
        Assert.Equal(GasUnits.ToRankine(15m, GasUnits.Celsius), metricStored.TemperatureRankine);

        // ---- AC-6: the reloaded run reproduces the saved Z ----------------------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);
        Assert.Equal(2, reloaded.Value!.History.Count);

        var savedRun = reloaded.Value.History.Single(item => item.PressurePsia == PressurePsia);

        // SQLite stores the value as a REAL, so equality is to 1e-6 rather than bit-exact.
        Assert.InRange(Math.Abs(savedRun.ZFactor!.Value - computedZ), 0m, 0.000001m);

        var rerun = await service.RunAsync(new OgGasPropertyInputs(
            savedRun.Pressure, savedRun.PressureUnit, savedRun.Temperature, savedRun.TemperatureUnit,
            savedRun.SpecificGravity, savedRun.ZCorrelation, savedRun.ViscosityCorrelation));

        Assert.True(rerun.Success, rerun.Message);
        Assert.InRange(Math.Abs(rerun.Value!.ZFactor!.Value - savedRun.ZFactor!.Value), 0m, 0.000001m);

        // Saving the same run twice does not duplicate the record.
        var again = await service.SaveAsync(savedRun, "sara");

        Assert.True(again.Success, again.Message);
        Assert.Equal(2, CountRows(database));

        // ---- AC-3: a failed run is stored as a failure, not as a number ---------
        var refused = await service.RunAsync(inputs with
        {
            Temperature = 90m,
            TemperatureUnit = GasUnits.Rankine,
        });

        Assert.True(refused.Success, refused.Message);
        Assert.True(refused.Value!.IsFailed);
        Assert.Null(refused.Value.ZFactor);

        var failedSaved = await service.SaveAsync(refused.Value, "sara");

        Assert.True(failedSaved.Success, failedSaved.Message);
        Assert.Equal(3, CountRows(database));

        var failedStored = failedSaved.Value!.History.First(item => item.IsFailed);

        Assert.Equal(GasPropertyRules.CodeCalculationRefused, failedStored.FailureCode);
        Assert.Null(failedStored.ZFactor);
        Assert.Contains("outside the engine", failedStored.FailureMessage!, StringComparison.Ordinal);
        Assert.StartsWith("FAILED", failedStored.Describe(), StringComparison.Ordinal);

        // ---- the status column distinguishes the two kinds of record ------------
        Assert.Equal(2, Scalar(database, "SELECT COUNT(*) FROM GAS_PROPERTY_RUN WHERE STATUS = 'OK';"));
        Assert.Equal(1, Scalar(database, "SELECT COUNT(*) FROM GAS_PROPERTY_RUN WHERE STATUS = 'FAILED';"));

        // ---- the state table's "history unavailable": a state, not a dead page --
        Drop(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.History);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
        Assert.Contains("new runs are still allowed", unavailable.Value.HistoryStateText, StringComparison.Ordinal);

        // New runs still work with the history gone.
        Assert.True((await service.RunAsync(inputs)).Success);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE GAS_PROPERTY_RUN (
                RUN_ID TEXT NOT NULL PRIMARY KEY,
                STATUS TEXT NOT NULL,
                PRESSURE_INPUT REAL NULL,
                PRESSURE_UNIT TEXT NULL,
                PRESSURE_PSIA REAL NULL,
                TEMPERATURE_INPUT REAL NULL,
                TEMPERATURE_UNIT TEXT NULL,
                TEMPERATURE_R REAL NULL,
                SPECIFIC_GRAVITY REAL NULL,
                Z_CORRELATION TEXT NULL,
                VISCOSITY_CORRELATION TEXT NULL,
                Z_FACTOR REAL NULL,
                VISCOSITY_CP REAL NULL,
                DENSITY REAL NULL,
                FORMATION_VOLUME_FACTOR REAL NULL,
                PSEUDO_PRESSURE REAL NULL,
                APPLICABILITY_TEXT TEXT NULL,
                FAILURE_CODE TEXT NULL,
                FAILURE_MESSAGE TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static long CountRows(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM GAS_PROPERTY_RUN;");

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE GAS_PROPERTY_RUN;";
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
