using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Engineering;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ENG-06 against a real SQLite database: a saved plan records its design method and its outcome (AC-6); the stations are
/// reproduced from the inputs when the plan is opened, so a save is not a copy of the geometry; the handoff records the actor and
/// passes the computed trajectory (AC-5); and the newest plan is the one read. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WellPlanGatewayTests
{
    private const string ConnectionName = "eng06-well-plan";

    private static OgPlanRequest BuildAndHold(string name = "Plan 1") =>
        new("W-1", "North", name, WellPlanRules.BuildAndHold, "m",
            0, 0, 0, 0, 0, 0, 800, 0, 2000, 30, 3, []);

    private static OgPlanRequest Explicit(string name = "Explicit 1") =>
        new("W-1", "North", name, WellPlanRules.Explicit, "m",
            0, 0, 0, 0, 0, 0, null, null, null, 0, 0,
            [new OgPlanSection(1000, 0, 0), new OgPlanSection(900, 3, 0), new OgPlanSection(2000, 0, 0)]);

    [IntegrationFact]
    public async Task A_plan_is_saved_reproduced_and_handed_off()
    {        var root = Path.Combine(OgPaths.DataRoot, "eng06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEng06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "well-plan.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "well-plan.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellPlanService(() => source, reporter);

        // ---- AC-6: a saved plan records the design method and the outcome ----
        var saved = await service.SaveAsync(BuildAndHold(), "sara");

        Assert.True(saved.Success, saved.Message + " :: " + string.Join(" | ", reporter.Failures));

        var first = saved.Value!;

        Assert.NotNull(first.PlanId);
        Assert.Equal("sara", first.SavedBy);
        Assert.NotNull(first.SavedAt);

        Assert.Equal(1, Count(database,
            "WELL_PLAN WHERE DESIGN_METHOD = 'BUILD_AND_HOLD' AND DEPTH_OUOM = 'm' AND TARGET_HIT = 'Y' AND " +
            "SAVED_BY = 'sara' AND ABS(MISS_DISTANCE) < 1e-4 AND TORTUOSITY > 0 AND TORTUOSITY_BASIS IS NOT NULL"));
        Assert.Equal(1, Count(database,
            "WELL_PLAN WHERE ABS(FINAL_NORTH - 800) < 0.01 AND ABS(FINAL_TVD - 2000) < 0.01 AND SECTIONS_TEXT IS NULL"));

        Assert.Equal(first.Stations.Count,
            Convert.ToInt32(Scalar(database, "SELECT STATION_COUNT FROM WELL_PLAN WHERE DESIGN_METHOD = 'BUILD_AND_HOLD';")));

        // ---- the stations are reproduced from the inputs, not read back ----
        var loaded = await service.LoadAsync("W-1");

        Assert.True(loaded.Success, loaded.Message + " :: " + string.Join(" | ", reporter.Failures));

        var reproduced = loaded.Value!;

        Assert.Equal(first.PlanId, reproduced.PlanId);
        Assert.Equal(WellPlanRules.BuildAndHold, reproduced.Method);
        Assert.Equal(first.Stations.Count, reproduced.Stations.Count);
        Assert.Equal(first.FinalMd!.Value, reproduced.FinalMd!.Value, 6);
        Assert.Equal(first.FinalNorth!.Value, reproduced.FinalNorth!.Value, 6);
        Assert.Equal(first.FinalTvd!.Value, reproduced.FinalTvd!.Value, 6);
        Assert.True(reproduced.TargetHit);

        // ---- AC-5: the handoff records the actor and passes the trajectory ----
        var handed = await service.HandoffAsync(first.PlanId!, "nadia");

        Assert.True(handed.Success, handed.Message + " :: " + string.Join(" | ", reporter.Failures));

        Assert.NotNull(handed.Value!.HandedOffAt);
        Assert.Equal("nadia", handed.Value.HandedOffBy);
        Assert.Contains("handed off", handed.Value.SavedText, StringComparison.Ordinal);

        Assert.NotNull(handed.Value.Trajectory);
        Assert.Equal(handed.Value.Stations.Count, handed.Value.Trajectory!.Survey.Count);

        Assert.Equal(1, Count(database,
            "WELL_PLAN WHERE HANDED_OFF_BY = 'nadia' AND HANDED_OFF_AT_UTC IS NOT NULL AND " +
            "ABS(HANDED_OFF_AT_UTC - datetime('now')) < 1"));

        // ---- an explicit plan is recorded with its sections, and it becomes the newest ----
        Execute(database,
            "UPDATE WELL_PLAN SET SAVED_AT_UTC = datetime('now', '-1 day') WHERE DESIGN_METHOD = 'BUILD_AND_HOLD';");

        var explicitPlan = await service.SaveAsync(Explicit(), "omar");

        Assert.True(explicitPlan.Success, explicitPlan.Message + " :: " + string.Join(" | ", reporter.Failures));

        Assert.Equal(WellPlanRules.Explicit, explicitPlan.Value!.Method);
        Assert.Equal(3900d, explicitPlan.Value.FinalMd!.Value, 3);
        Assert.Equal(3, explicitPlan.Value.Sections.Count);
        Assert.Null(explicitPlan.Value.Miss);
        Assert.Contains("nothing to hit", explicitPlan.Value.TargetText, StringComparison.Ordinal);

        Assert.Equal(1, Count(database,
            "WELL_PLAN WHERE DESIGN_METHOD = 'EXPLICIT' AND SECTIONS_TEXT LIKE '%hold%' AND TARGET_HIT IS NULL AND " +
            "SAVED_BY = 'omar'"));

        var newest = await service.LoadAsync("W-1");

        Assert.True(newest.Success, newest.Message);
        Assert.Equal("Explicit 1", newest.Value!.PlanName);
        Assert.Equal(WellPlanRules.Explicit, newest.Value.Method);

        // ---- the refusals ----
        var unknownPlan = await service.HandoffAsync("no-such-plan", "sara");

        Assert.False(unknownPlan.Success);
        Assert.Equal(WellPlanRules.CodeHandoffBeforeSave, unknownPlan.Reference);

        var noPlan = await service.LoadAsync("W-404");

        Assert.False(noPlan.Success);
        Assert.Equal(WellPlanRules.CodeUnknownPlan, noPlan.Reference);

        var noWell = await service.LoadAsync(" ");

        Assert.False(noWell.Success);
        Assert.Equal(WellPlanRules.CodeWellRequired, noWell.Reference);

        var offline = new OgWellPlanService(() => null, reporter);
        var noConnection = await offline.SaveAsync(BuildAndHold(), "sara");

        Assert.False(noConnection.Success);
        Assert.Equal(WellPlanRules.CodeNoConnection, noConnection.Reference);

        // ---- a failed read is a failure, not an empty plan ----
        Execute(database, "DROP TABLE WELL_PLAN;");

        var failed = await service.LoadAsync("W-1");

        Assert.False(failed.Success);
        Assert.Null(failed.Value);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database) => Execute(database, """
            CREATE TABLE WELL_PLAN (
                PLAN_ID TEXT,
                UWI TEXT,
                FIELD_NAME TEXT,
                PLAN_NAME TEXT,
                DESIGN_METHOD TEXT,
                DEPTH_OUOM TEXT,
                TIE_ON_MD REAL,
                TIE_ON_NORTH REAL,
                TIE_ON_EAST REAL,
                TIE_ON_TVD REAL,
                TIE_ON_INCLINATION REAL,
                TIE_ON_AZIMUTH REAL,
                TARGET_NORTH REAL,
                TARGET_EAST REAL,
                TARGET_TVD REAL,
                TARGET_TOLERANCE REAL,
                DESIGN_DOGLEG REAL,
                MAX_DOGLEG REAL,
                SECTIONS_TEXT TEXT,
                STATION_COUNT INTEGER,
                FINAL_MD REAL,
                FINAL_NORTH REAL,
                FINAL_EAST REAL,
                FINAL_TVD REAL,
                FINAL_INCLINATION REAL,
                FINAL_AZIMUTH REAL,
                MISS_DISTANCE REAL,
                TARGET_HIT TEXT,
                MAX_STATION_DOGLEG REAL,
                TORTUOSITY_BASIS TEXT,
                TORTUOSITY REAL,
                HANDED_OFF_AT_UTC TEXT,
                HANDED_OFF_BY TEXT,
                SAVED_BY TEXT,
                SAVED_AT_UTC TEXT,
                ACTIVE_IND TEXT,
                ROW_CREATED_BY TEXT,
                ROW_CREATED_DATE TEXT,
                ROW_CHANGED_BY TEXT,
                ROW_CHANGED_DATE TEXT,
                ROW_EFFECTIVE_DATE TEXT,
                ROW_EXPIRY_DATE TEXT,
                ROW_QUALITY TEXT,
                PPDM_GUID TEXT,
                PRIMARY KEY (PLAN_ID));
            """);

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static string Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToString(command.ExecuteScalar()) ?? string.Empty;
    }

    private static long Count(string database, string fromAndWhere)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM " + fromAndWhere + ";";

        return Convert.ToInt64(command.ExecuteScalar());
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
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
