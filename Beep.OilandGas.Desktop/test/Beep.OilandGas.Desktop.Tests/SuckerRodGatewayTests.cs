using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lift;
using Beep.OilandGas.SuckerRodPumping.Constants;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LFT-04 against a real SQLite database: a safe design saves with its grade, taper, safety factor and units (AC-6, AC-5),
/// an unsafe string is refused and writes nothing (AC-1), a save whose acknowledgement was lost is retained and retried,
/// and an unavailable history is a state. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class SuckerRodGatewayTests
{
    private const string ConnectionName = "lft04-srp";

    [IntegrationFact]
    public async Task A_safe_string_saves_with_its_grade_taper_and_safety_factor()
    {        var root = Path.Combine(OgPaths.DataRoot, "lft04-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLft04"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "srp.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "srp.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgSuckerRodService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Designs);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains(SuckerRodConstants.RodGradeD, loaded.Value.Grades);
        Assert.Equal((decimal)SuckerRodConstants.MinimumSafetyFactor, loaded.Value.RequiredSafetyFactor);
        Assert.Equal(SuckerRodConstants.StandardRodDiameters, loaded.Value.StandardRodDiameters);
        Assert.Contains("required safety factor", loaded.Value.ReferenceText, StringComparison.Ordinal);

        // ---- AC-1: an unsafe string is refused and writes nothing ------------
        var unsafeRun = await service.DesignAsync(Inputs() with
        {
            Sections = [new OgRodSection(0.625m, 2500m), new OgRodSection(0.625m, 2500m)],
        });

        Assert.False(unsafeRun.Success);
        Assert.Equal(SrpRules.CodeUnsafeString, unsafeRun.Reference);
        Assert.Equal(0, CountRows(database));

        // ---- design and save -------------------------------------------------
        var designed = await service.DesignAsync(Inputs());

        Assert.True(designed.Success, designed.Message + " :: " + string.Join(" | ", reporter.Failures));

        var result = designed.Value!.Selected!;

        Assert.False(result.IsUnsafe);
        Assert.Equal(3, result.Sections.Count);

        var saved = await service.SaveAsync(result, "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, CountRows(database));

        // AC-6 and AC-5 are on the stored row.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM SRP_DESIGN WHERE GRADE = 'D' AND SECTION_COUNT = 3 " +
            "AND TOTAL_LENGTH_FT = 5000 AND MIN_SAFETY_FACTOR > REQUIRED_SAFETY_FACTOR " +
            "AND REQUIRED_SAFETY_FACTOR = " +
            SuckerRodConstants.MinimumSafetyFactor.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            " AND TAPER LIKE '%,%' AND UNITS_TEXT LIKE '%psi%' AND ACTOR = 'sara' " +
            "AND DENSITY_ASSUMED_IND = 'Y';"));

        // The pump card was produced and its extremes are stored.
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM SRP_DESIGN WHERE CARD_PEAK_LB > 0 AND CARD_MIN_LB > 0 AND CARD_POINTS > 1;"));

        // ---- the reload keeps the string, the grade and the safety factor ----
        var reloaded = await service.LoadAsync("WELL-1");

        Assert.True(reloaded.Success, reloaded.Message);

        var again = reloaded.Value!.Selected!;

        Assert.Equal(SuckerRodConstants.RodGradeD, again.Grade);
        Assert.Equal(3, again.Sections.Count);
        Assert.Equal(result.Sections.Select(section => section.DiameterIn),
            again.Sections.Select(section => section.DiameterIn));
        Assert.Equal(result.TotalLengthFt, again.TotalLengthFt);
        // SQLite stores these as REAL, so the round trip is compared to 1e-3 rather than bit-exactly.
        Assert.InRange(Math.Abs(result.MinSafetyFactor - again.MinSafetyFactor), 0m, 0.001m);
        Assert.InRange(Math.Abs(result.RequiredSafetyFactor - again.RequiredSafetyFactor), 0m, 0.001m);
        Assert.InRange(Math.Abs(result.PeakLoad - again.PeakLoad), 0m, 0.001m);
        Assert.InRange(Math.Abs(result.MaxStress - again.MaxStress), 0m, 0.001m);
        Assert.Equal("sara", again.Actor);
        Assert.True(again.DensityAssumed);

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
        Assert.Empty(unavailable.Value.Designs);
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
            CREATE TABLE SRP_DESIGN (
                DESIGN_ID TEXT NOT NULL PRIMARY KEY,
                WELL_UWI TEXT NULL,
                DESIGN_DATE TEXT NULL,
                PUMP_DIAMETER_IN REAL NOT NULL,
                STROKE_LENGTH_IN REAL NOT NULL,
                STROKES_PER_MINUTE REAL NOT NULL,
                PUMP_DEPTH_FT REAL NOT NULL,
                GRADE TEXT NOT NULL,
                TAPER TEXT NULL,
                SECTION_COUNT INTEGER NOT NULL,
                TOTAL_LENGTH_FT REAL NOT NULL,
                PEAK_LOAD_LB REAL NOT NULL,
                MIN_LOAD_LB REAL NOT NULL,
                FLUID_LOAD_LB REAL NOT NULL,
                MAX_STRESS_PSI REAL NOT NULL,
                STRESS_RANGE_PSI REAL NOT NULL,
                MIN_SAFETY_FACTOR REAL NOT NULL,
                REQUIRED_SAFETY_FACTOR REAL NOT NULL,
                GOODMAN_RATIO REAL NOT NULL,
                CRITICAL_SECTION INTEGER NOT NULL,
                RISK TEXT NULL,
                PRODUCTION_RATE_BPD REAL NOT NULL,
                MOTOR_HORSEPOWER REAL NOT NULL,
                CARD_PEAK_LB REAL NOT NULL,
                CARD_MIN_LB REAL NOT NULL,
                CARD_POINTS INTEGER NOT NULL,
                UNITS_TEXT TEXT NULL,
                DENSITY_ASSUMED_IND TEXT NOT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);
            """;
        command.ExecuteNonQuery();
    }

    private static OgSrpInputs Inputs() =>
        new(
            "WELL-1", 2.0m, 48m, 10m, 5000m, 4800m, 2.875m, 35m, 0.3m, 300m, 0.65m,
            RodDensity: null,
            AcceptDefaultDensity: true,
            Grade: SuckerRodConstants.RodGradeD,
            Sections:
            [
                new OgRodSection(1.0m, 2500m),
                new OgRodSection(0.875m, 1500m),
                new OgRodSection(0.75m, 1000m),
            ]);

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountRows(string database) => Scalar(database, "SELECT COUNT(*) FROM SRP_DESIGN;");

    private static void Drop(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TABLE SRP_DESIGN;";
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
