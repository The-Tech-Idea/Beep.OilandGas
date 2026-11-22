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
/// RES-06 against a real SQLite database: the fit reads the PPDM volume summary, recovers the parameters the history was
/// generated with, saves them with the exclusions and the cross-check, and a range with too few points is refused with
/// the minimum named. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DeclineCurveGatewayTests
{
    private const string ConnectionName = "res06-decline";
    private static readonly DateTime Start = new(2024, 1, 1);

    [IntegrationFact]
    public async Task A_history_is_fitted_from_the_ppdm_rows_and_the_fit_is_saved()
    {        var root = Path.Combine(OgPaths.DataRoot, "res06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasRes06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "decline.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "decline.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgDeclineCurveService(() => source, reporter);

        // ---- the saved fits start empty --------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.History);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains(DeclineModels.Hyperbolic, loaded.Value.Models);
        Assert.Equal(6, loaded.Value.MinimumPointsForFit);

        // ---- AC-1: the fit recovers the parameters the history was made with --
        var analyzed = await service.AnalyzeAsync(new OgDeclineInputs(
            "WELL-1", DeclineFluids.Oil, Start, Start.AddYears(3), DeclineModels.Hyperbolic, null, 10m));

        Assert.True(analyzed.Success, analyzed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var fit = analyzed.Value!.Fit!;

        Assert.False(fit.IsRefused, fit.RefusalText);
        Assert.True(fit.HasCurve);

        // The history is generated at qi 1000, Di 0.01/day, b 0.5.
        Assert.InRange(fit.Qi!.Value, 999m, 1001m);
        Assert.InRange(fit.DiPerDay!.Value, 0.0099m, 0.0101m);
        Assert.InRange(fit.B!.Value, 0.49m, 0.51m);
        Assert.Equal(22, fit.PointsUsed);
        Assert.Equal(2, fit.PointsExcluded);
        Assert.True(fit.Eur > 0m);
        Assert.Equal("bbl", fit.Unit);
        Assert.Equal("WELL-1", fit.WellId);

        // AC-4: the independent fitter agrees on real rows too.
        Assert.True(fit.CrossCheck.Available, fit.CrossCheck.Text);
        Assert.True(fit.CrossCheck.Agrees, fit.CrossCheck.Text);

        // AC-3: the two zero-volume rows are excluded, with their reasons.
        Assert.Equal(2, fit.PointsExcluded);
        Assert.Contains("log scale", fit.ExclusionsText, StringComparison.Ordinal);

        // ---- save records the parameters, exclusions and cross-check ---------
        var saved = await service.SaveAsync(fit, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Single(saved.Value!.History);
        Assert.Equal(1, CountRows(database, "DECLINE_FIT"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM DECLINE_FIT WHERE WELL_ID = 'WELL-1' AND FLUID = 'OIL' AND MODEL = 'HYPERBOLIC' " +
            "AND POINTS_USED = 22 AND POINTS_EXCLUDED = 2 AND BOUNDED_IND = 'Y' AND CROSS_CHECK_AGREES = 'Y';"));

        var history = saved.Value.History[0];

        Assert.Equal("sara", history.Actor);
        Assert.Equal(22, history.PointsUsed);
        Assert.Equal(2, history.PointsExcluded);
        Assert.Contains("agrees", history.CrossCheck.Text, StringComparison.Ordinal);
        Assert.NotNull(history.Eur);

        // ---- a range with too few points is refused with the minimum ---------
        var tooFew = await service.AnalyzeAsync(new OgDeclineInputs(
            "WELL-1", DeclineFluids.Oil, Start, Start.AddMonths(2), DeclineModels.Hyperbolic, null, 10m));

        Assert.True(tooFew.Success, tooFew.Message);
        Assert.True(tooFew.Value!.Fit!.IsRefused);
        Assert.Equal(DeclineRules.CodeTooFewPoints, tooFew.Value.Fit.RefusalCode);

        // ---- a well that has no history is refused, not zeroed ---------------
        var unknown = await service.AnalyzeAsync(new OgDeclineInputs(
            "WELL-404", DeclineFluids.Oil, Start, Start.AddYears(3), DeclineModels.Hyperbolic, null, 10m));

        Assert.True(unknown.Success, unknown.Message);
        Assert.Equal(DeclineRules.CodeTooFewPoints, unknown.Value!.Fit!.RefusalCode);

        // ---- an unavailable history is a state -------------------------------
        Drop(database, "DECLINE_FIT");

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.History);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        var volumes = new System.Text.StringBuilder();

        for (var month = 0; month < 24; month++)
        {
            var date = Start.AddMonths(month);
            var days = (date - Start).TotalDays;
            var rate = 1000.0 / Math.Pow(1.0 + 0.5 * 0.01 * days, 1.0 / 0.5);

            // Two of the months are recorded as zero, which a log-scale fit cannot use.
            if (month is 5 or 11)
            {
                rate = 0.0;
            }

            volumes.AppendLine(
                "INSERT INTO PDEN_VOL_SUMMARY (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, ACTIVITY_TYPE, AMENDMENT_SEQ_NO, " +
                "ACTIVE_IND, VOLUME_DATE, OIL_VOLUME, OIL_VOLUME_OUOM, GAS_VOLUME, GAS_VOLUME_OUOM, WATER_VOLUME, " +
                "WATER_VOLUME_OUOM) VALUES " +
                $"('PD-1', 'WELL', 'PPDM', 'PRODUCTION', 0, 'Y', '{date:yyyy-MM-dd}', " +
                $"{rate.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture)}, 'bbl', 0, 'MMscf', 0, 'bbl');");
        }

        using var command = connection.CreateCommand();
        command.CommandText =
            """
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

            CREATE TABLE DECLINE_FIT (
                FIT_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NULL,
                POOL_ID TEXT NULL,
                FLUID TEXT NULL,
                MODEL TEXT NOT NULL,
                OUTCOME TEXT NOT NULL,
                REFUSAL_CODE TEXT NULL,
                REFUSAL_MESSAGE TEXT NULL,
                QI REAL NULL,
                DI_PER_DAY REAL NULL,
                B REAL NULL,
                BOUNDED_IND TEXT NOT NULL,
                EUR REAL NULL,
                ECONOMIC_LIMIT REAL NULL,
                VOLUME_OUOM TEXT NULL,
                R_SQUARED REAL NULL,
                POINTS_USED INTEGER NOT NULL,
                POINTS_EXCLUDED INTEGER NOT NULL,
                EXCLUSIONS_TEXT TEXT NULL,
                HISTORY_FROM TEXT NULL,
                HISTORY_TO TEXT NULL,
                CROSS_CHECK_SOURCE TEXT NULL,
                CROSS_CHECK_QI REAL NULL,
                CROSS_CHECK_B REAL NULL,
                CROSS_CHECK_AGREES TEXT NULL,
                CROSS_CHECK_TEXT TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO WELL (UWI, WELL_NAME, ASSIGNED_FIELD, ACTIVE_IND, CURRENT_STATUS) VALUES
                ('WELL-1', 'Well one', 'F-1', 'Y', 'PRODUCING');

            INSERT INTO PDEN_WELL (PDEN_ID, PDEN_SUBTYPE, PDEN_SOURCE, PRIMARY_UWI, ACTIVE_IND) VALUES
                ('PD-1', 'WELL', 'PPDM', 'WELL-1', 'Y');
            """;
        command.ExecuteNonQuery();

        using var volumeCommand = connection.CreateCommand();
        volumeCommand.CommandText = volumes.ToString();
        volumeCommand.ExecuteNonQuery();
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
