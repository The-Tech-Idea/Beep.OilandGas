using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Development;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DEV-03 against a real SQLite database: a design takes its plan and field from the scheduled well (AC-1), a depth
/// without its unit is refused (AC-2), the casing check records a linked pass and a linked fail (AC-3), an incomplete
/// design cannot be approved (AC-4), and a cost revision appends a version and retains the old one (AC-5). Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class WellDesignGatewayTests
{
    private const string ConnectionName = "dev03-welldesign";

    [IntegrationFact]
    public async Task A_design_is_scoped_checked_and_versioned()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "welldesign.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "welldesign.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var designs = new OgWellDesignService(() => source, reporter);

        // The workspace offers the scheduled wells that still need a design.
        var workspace = await designs.LoadAsync("F-1", "Alpha field", null);

        Assert.True(workspace.Success, workspace.Message);
        Assert.Empty(workspace.Value!.Wells);
        Assert.Equal(2, workspace.Value.UndesignedSchedules.Count);
        Assert.DoesNotContain(workspace.Value.Schedules, schedule => schedule.Id == "SCH-OTHER");

        // AC-1: the design takes its plan and field from the scheduled well.
        var created = await designs.CreateAsync("SCH-1", "sara");

        Assert.True(created.Success, created.Message);
        var first = created.Value!.Current;

        Assert.Equal("FDP-1", first.PlanId);
        Assert.Equal("F-1", first.FieldId);
        Assert.Equal("WELL-1", first.WellId);
        Assert.Equal("Upper sand", first.TargetFormation);
        Assert.Equal(OgDevelopmentWell.Concept, first.DesignState);

        // One design per scheduled well.
        var duplicate = await designs.CreateAsync("SCH-1", "sara");

        Assert.False(duplicate.Success);
        Assert.Equal(OgWellDesignService.CodeAlreadyDesigned, duplicate.Reference);

        // A schedule whose plan belongs to another field is refused rather than silently split.
        var mismatch = await designs.CreateAsync("SCH-3", "sara");

        Assert.False(mismatch.Success);
        Assert.Equal(OgWellDesignService.CodeFieldMismatch, mismatch.Reference);

        // AC-2: a target depth without its unit is refused, and nothing is written.
        var noUnit = await designs.SaveAsync(
            first.Id,
            Values(depth: 2400m, unit: null, cost: 12.5m, currency: "USD"),
            "sara");

        Assert.False(noUnit.Success);
        Assert.Equal(OgWellDesignService.CodeDepthUnitRequired, noUnit.Reference);

        // AC-4: an incomplete design cannot be approved and the refusal names what is missing.
        var incomplete = await designs.ApproveAsync(first.Id, "reviewer");

        Assert.False(incomplete.Success);
        Assert.Equal(OgWellDesignService.CodeIncomplete, incomplete.Reference);
        Assert.Contains("Casing scheme", incomplete.Message!, StringComparison.Ordinal);

        var saved = await designs.SaveAsync(
            first.Id,
            Values(depth: 2400m, unit: "m", cost: 12.5m, currency: "USD"),
            "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(OgDevelopmentWell.Defined, saved.Value!.Current.DesignState);

        // Every value is recorded; only the casing outline is still missing at this point.
        Assert.Equal(new[] { "Casing scheme" }, saved.Value.MissingItems);

        // AC-3: a check needs a casing outline, then records a pass linked to the version.
        var empty = await designs.RunCasingCheckAsync(first.Id, "sara");

        Assert.False(empty.Success);
        Assert.Equal(OgWellDesignService.CodeCasingRequired, empty.Reference);

        Assert.True((await designs.AddCasingStringAsync(
            first.Id, new OgCasingStringRequest("SURFACE", 20m, 94m, "K55", 400m, "m"), "sara")).Success);
        Assert.True((await designs.AddCasingStringAsync(
            first.Id, new OgCasingStringRequest("INTERMEDIATE", 13.375m, 68m, "L80", 1600m, "m"), "sara")).Success);
        Assert.True((await designs.AddCasingStringAsync(
            first.Id, new OgCasingStringRequest("PRODUCTION", 9.625m, 47m, "P110", 2400m, "m"), "sara")).Success);

        var passed = await designs.RunCasingCheckAsync(first.Id, "sara");

        Assert.True(passed.Success, passed.Message);
        Assert.NotNull(passed.Value!.LastCheck);
        Assert.True(passed.Value.LastCheck!.IsPass);
        Assert.Equal(first.Id, passed.Value.LastCheck.DesignId);
        Assert.Contains("Inputs verified", passed.Value.LastCheck.FindingsText, StringComparison.Ordinal);

        // A fifth string set shallower than the fourth is a recorded failure, not an unrecorded operation.
        Assert.True((await designs.AddCasingStringAsync(
            first.Id, new OgCasingStringRequest("LINER", 7m, 29m, "P110", 1000m, "m"), "sara")).Success);

        var failed = await designs.RunCasingCheckAsync(first.Id, "sara");

        Assert.True(failed.Success, failed.Message);
        Assert.NotNull(failed.Value!.LastCheck);
        Assert.False(failed.Value.LastCheck!.IsPass);
        Assert.Contains("not deeper", failed.Value.LastCheck.FindingsText, StringComparison.Ordinal);

        // AC-5: the cost revision appends a version and retains the old one.
        var revised = await designs.ReviseAsync(
            first.Id,
            Values(depth: 2400m, unit: "m", cost: 14.25m, currency: "USD"),
            "AFE re-priced after the rig quote",
            "sara");

        Assert.True(revised.Success, revised.Message);
        var second = revised.Value!.Current;

        Assert.Equal(2, second.Version);
        Assert.Equal(14.25m, second.EstimatedCost);
        Assert.Equal(OgDevelopmentWell.Defined, second.DesignState);

        // The four strings recorded on version 1 are carried into version 2.
        Assert.Equal(4, second.Casing.Count);
        Assert.Equal(2, revised.Value.Versions.Count);

        var old = revised.Value.Versions.Single(version => version.Version == 1);

        Assert.False(old.IsCurrent);
        Assert.Equal(12.5m, old.EstimatedCost);
        Assert.Equal(first.Id, old.Id);

        // The approved version cannot be edited; the working version can.
        Assert.True((await designs.SaveAsync(
            second.Id,
            Values(depth: 2400m, unit: "m", cost: 14.25m, currency: "USD"),
            "sara")).Success);

        var approved = await designs.ApproveAsync(second.Id, "reviewer");

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(OgDevelopmentWell.Approved, approved.Value!.Current.DesignState);
        Assert.Equal("reviewer", approved.Value.Current.ApprovedBy);
        Assert.NotNull(approved.Value.Current.ApprovedAt);

        var locked = await designs.SaveAsync(
            second.Id,
            Values(depth: 2500m, unit: "m", cost: 14.25m, currency: "USD"),
            "sara");

        Assert.False(locked.Success);
        Assert.Equal(OgWellDesignService.CodeLocked, locked.Reference);

        // The workspace now shows one current design for the well, and no undesigned well for it.
        var after = await designs.LoadAsync("F-1", "Alpha field", null);

        Assert.True(after.Success, after.Message);
        var shown = Assert.Single(after.Value!.Wells);

        Assert.Equal(second.Id, shown.Id);
        Assert.Equal(2, shown.Version);
        Assert.True(shown.IsApproved);
        Assert.Equal(1, after.Value.UndesignedSchedules.Count);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE FIELD_DEVELOPMENT_PLAN (
                FDP_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                FDP_NAME TEXT NULL,
                FDP_STATUS TEXT NULL,
                FDP_VERSION INTEGER NOT NULL);

            CREATE TABLE DEVELOPMENT_WELL_SCHEDULE (
                SCHEDULE_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                PLANNED_WELL_ID TEXT NULL,
                WELL_NAME TEXT NULL,
                TARGET_RESERVOIR TEXT NULL,
                AFE_COST_MM REAL NULL,
                AFE_CURRENCY TEXT NULL,
                PLANNED_SPUD_DATE TEXT NULL,
                PLANNED_COMPLETION_DATE TEXT NULL,
                SCHEDULE_STATUS TEXT NULL);

            CREATE TABLE DEVELOPMENT_WELL_DESIGN (
                DESIGN_ID TEXT NOT NULL PRIMARY KEY,
                SCHEDULE_ID TEXT NOT NULL,
                FDP_ID TEXT NOT NULL,
                FIELD_ID TEXT NOT NULL,
                PLANNED_WELL_ID TEXT NOT NULL,
                WELL_NAME TEXT NULL,
                VERSION INTEGER NOT NULL,
                CURRENT_IND TEXT NULL,
                DESIGN_STATE TEXT NULL,
                OBJECTIVE TEXT NULL,
                TARGET_FORMATION TEXT NULL,
                TARGET_DEPTH REAL NOT NULL,
                DEPTH_OUOM TEXT NULL,
                TRAJECTORY_INTENT TEXT NULL,
                COMPLETION_CONCEPT TEXT NULL,
                RISK_TEXT TEXT NULL,
                ESTIMATED_COST REAL NOT NULL,
                COST_CURRENCY TEXT NULL,
                SPUD_WINDOW_START TEXT NULL,
                SPUD_WINDOW_END TEXT NULL,
                APPROVED_BY TEXT NULL,
                APPROVED_AT TEXT NULL,
                REVISION_REASON TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_CASING_STRING (
                CASING_STRING_ID TEXT NOT NULL PRIMARY KEY,
                DESIGN_ID TEXT NOT NULL,
                STRING_ORDER INTEGER NOT NULL,
                STRING_NAME TEXT NULL,
                OD REAL NOT NULL,
                WEIGHT REAL NOT NULL,
                GRADE TEXT NULL,
                SET_DEPTH REAL NOT NULL,
                DEPTH_OUOM TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_CASING_CHECK (
                CHECK_ID TEXT NOT NULL PRIMARY KEY,
                DESIGN_ID TEXT NOT NULL,
                OUTCOME TEXT NULL,
                FINDINGS_TEXT TEXT NULL,
                CHECKED_BY TEXT NULL,
                CHECKED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO FIELD_DEVELOPMENT_PLAN (FDP_ID, FIELD_ID, FDP_NAME, FDP_STATUS, FDP_VERSION) VALUES
                ('FDP-1', 'F-1', 'Alpha FDP', 'APPROVED', 1),
                ('FDP-2', 'F-2', 'Other field FDP', 'DRAFT', 1);

            INSERT INTO DEVELOPMENT_WELL_SCHEDULE
                (SCHEDULE_ID, FDP_ID, FIELD_ID, PLANNED_WELL_ID, WELL_NAME, TARGET_RESERVOIR, AFE_COST_MM,
                 AFE_CURRENCY, PLANNED_SPUD_DATE, PLANNED_COMPLETION_DATE, SCHEDULE_STATUS) VALUES
                ('SCH-1', 'FDP-1', 'F-1', 'WELL-1', 'WELL-1', 'Upper sand', 12.5, 'USD', '2026-08-01', '2026-09-01', 'PLANNED'),
                ('SCH-2', 'FDP-1', 'F-1', 'WELL-2', 'WELL-2', 'Lower sand', 9.0, 'USD', '2026-10-01', '2026-11-01', 'PLANNED'),
                ('SCH-3', 'FDP-2', 'F-1', 'WELL-3', 'WELL-3', 'Lower sand', 9.0, 'USD', '2026-12-01', '2027-01-01', 'PLANNED'),
                ('SCH-OTHER', 'FDP-2', 'F-2', 'WELL-9', 'WELL-9', 'Other sand', 5.0, 'USD', '2027-02-01', '2027-03-01', 'PLANNED');
            """;
        command.ExecuteNonQuery();
    }

    private static OgWellDesignValues Values(decimal? depth, string? unit, decimal? cost, string? currency) =>
        new(
            "Drain the upper sand",
            "Upper sand",
            depth,
            unit,
            "Vertical to 1200 m, then 35 degrees",
            "Perforated liner with sand screens",
            "Lost circulation in the intermediate section",
            cost,
            currency,
            new DateTime(2026, 8, 1),
            new DateTime(2026, 9, 1));

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
