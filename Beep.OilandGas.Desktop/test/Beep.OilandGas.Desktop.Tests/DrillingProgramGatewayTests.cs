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
/// DEV-05 against a real SQLite database: the three checks start as not run (AC-2); an incomplete section or an
/// incomplete well design blocks the submit (AC-1, DEV-03); the author cannot review (AC-3); an approved version is
/// never mutated and a revision appends a new one (AC-4); the total is the sum of the section costs (AC-6); and a lost
/// submit acknowledgement is retained for retry. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DrillingProgramGatewayTests
{
    private const string ConnectionName = "dev05-program";

    [IntegrationFact]
    public async Task The_program_submits_reviews_and_keeps_the_approved_version()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "program.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "program.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgDrillProgramService(() => source, reporter);

        var workspace = await service.LoadAsync("F-1", "Alpha field");

        Assert.True(workspace.Success, workspace.Message);
        Assert.Equal(2, workspace.Value!.DesigndWells.Count);

        var completeDesign = workspace.Value.DesigndWells.Single(well => well.WellId == "WELL-1");
        var incompleteDesign = workspace.Value.DesigndWells.Single(well => well.WellId == "WELL-2");

        Assert.Empty(completeDesign.Missing);
        Assert.Contains("Casing scheme", incompleteDesign.Missing);

        var created = await service.CreateAsync(
            "COMPANY-1",
            completeDesign.DesignId,
            new OgDrillProgramValues("Alpha drilling program", "Drain the upper sand", 2600m, 2450m, "m", 34m, "USD"),
            "sara");

        Assert.True(created.Success, created.Message);
        var program = created.Value!;

        Assert.Equal(OgDrillProgram.Draft, program.State);
        Assert.Equal(1, program.Version);
        Assert.Equal("B-1", program.BaselineId);
        Assert.Equal("WELL-1", program.WellId);
        Assert.Equal("sara", program.PreparedBy);

        // AC-2: all three checks exist from creation and none of them is "passed".
        Assert.Equal(3, program.Checks.Count);
        Assert.All(program.Checks, check => Assert.False(check.IsRun));
        Assert.Contains(program.Checks, check => check.Name == "HYDRAULICS" && check.StatusText == "not run");
        Assert.Contains("not run", program.ChecksSummary, StringComparison.Ordinal);

        // AC-1: no section, then an incomplete section, both block the submit.
        var empty = await service.SubmitAsync(program.Id, "sara");

        Assert.False(empty.Success);
        Assert.Equal(OgDrillProgramService.CodeSectionRequired, empty.Reference);

        Assert.True((await service.SaveSectionAsync(
            program.Id,
            new OgDrillSectionValues(1, "Surface hole", 17.5m, "in", 0m, 400m, "m", 13.375m, "in", 9.5m, "ppg", null, null),
            "sara")).Success);

        var incomplete = await service.SubmitAsync(program.Id, "sara");

        Assert.False(incomplete.Success);
        Assert.Equal(OgDrillProgramService.CodeSectionIncomplete, incomplete.Reference);
        Assert.Contains("duration", incomplete.Message!, StringComparison.Ordinal);

        // A complete section, then a second one; the total is the sum (AC-6).
        Assert.True((await service.SaveSectionAsync(
            program.Id,
            new OgDrillSectionValues(1, "Surface hole", 17.5m, "in", 0m, 400m, "m", 13.375m, "in", 9.5m, "ppg", 6m, 400_000m),
            "sara")).Success);

        var second = await service.SaveSectionAsync(
            program.Id,
            new OgDrillSectionValues(2, "Intermediate hole", 12.25m, "in", 400m, 1600m, "m", 9.625m, "in", 10.5m, "ppg", 12m, 900_000m),
            "sara");

        Assert.True(second.Success, second.Message);
        Assert.Equal(2, second.Value!.Sections.Count);
        Assert.Equal(1_300_000m, second.Value.TotalCost);
        Assert.True(second.Value.TotalIsComplete);

        // A check recorded as not run stays "not run"; a pass is recorded with its reference.
        var notRun = await service.RecordCheckAsync(program.Id, "HYDRAULICS", OgDrillCheck.NotRun, "eng-hydraulics", null, "sara");

        Assert.True(notRun.Success, notRun.Message);
        Assert.All(notRun.Value!.Checks, check => Assert.False(check.IsRun));

        var passed = await service.RecordCheckAsync(program.Id, "HYDRAULICS", OgDrillCheck.Passed, "eng-hydraulics", "ECD 1.42", "sara");

        Assert.True(passed.Success, passed.Message);
        Assert.True(passed.Value!.Checks.Single(check => check.Name == "HYDRAULICS").IsPassed);
        Assert.False(passed.Value.Checks.Single(check => check.Name == "TORQUE_DRAG").IsRun);

        // A failing check is recorded and does not block the submit (section 5 of the feature doc).
        var failed = await service.RecordCheckAsync(program.Id, "TORQUE_DRAG", OgDrillCheck.Failed, "eng-td", "over limit at 1600 m", "sara");

        Assert.True(failed.Success, failed.Message);
        Assert.True(failed.Value!.AnyCheckFailed);

        var submitted = await service.SubmitAsync(program.Id, "sara");

        Assert.True(submitted.Success, submitted.Message);
        Assert.Equal(OgDrillProgram.Submitted, submitted.Value!.State);

        // A submitted program is read-only.
        var locked = await service.SaveHeaderAsync(
            program.Id,
            new OgDrillProgramValues("Changed", null, null, null, null, null, "USD"),
            "sara");

        Assert.False(locked.Success);
        Assert.Equal(OgDrillProgramService.CodeLocked, locked.Reference);

        // AC-3: the author cannot review their own program.
        var self = await service.ReviewAsync(
            program.Id,
            new OgDrillDecision(OgDrillDecision.Approve, null, "sara"));

        Assert.False(self.Success);
        Assert.Equal(OgDrillProgramService.CodeAuthorCannotApprove, self.Reference);

        var approved = await service.ReviewAsync(
            program.Id,
            new OgDrillDecision(OgDrillDecision.Approve, "approved for the AFE", "reviewer"));

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(OgDrillProgram.Approved, approved.Value!.State);
        Assert.Single(approved.Value.Reviews);
        Assert.Equal("reviewer", approved.Value.Reviews[0].Reviewer);

        // AC-4: a revision appends a new version; the approved one keeps its state, sections and checks.
        var revised = await service.ReviseAsync(program.Id, "sara");

        Assert.True(revised.Success, revised.Message);
        var secondVersion = revised.Value!;

        Assert.Equal(2, secondVersion.Version);
        Assert.Equal(OgDrillProgram.Draft, secondVersion.State);
        Assert.Equal(program.LineageId, secondVersion.LineageId);
        Assert.Equal(2, secondVersion.Sections.Count);

        // The checks of the new version start as not run, because they were run against version 1.
        Assert.All(secondVersion.Checks, check => Assert.False(check.IsRun));

        var history = await service.LoadVersionsAsync(program.LineageId);

        Assert.True(history.Success, history.Message);
        Assert.Equal(2, history.Value!.Count);

        var firstVersionNow = history.Value.Single(row => row.Id == program.Id);

        Assert.Equal(1, firstVersionNow.Version);
        Assert.Equal(OgDrillProgram.Approved, firstVersionNow.State);
        Assert.Equal(2, firstVersionNow.Sections.Count);
        Assert.True(firstVersionNow.Checks.Single(check => check.Name == "HYDRAULICS").IsPassed);

        // The workspace shows only the newest version of the lineage.
        var reloadedFirst = await service.LoadAsync("F-1", "Alpha field");

        Assert.Equal(secondVersion.Id, reloadedFirst.Value!.Programs.Single(row => row.LineageId == program.LineageId).Id);

        // DEV-03's gate: a program for an incomplete design cannot be submitted, and the refusal names the gap.
        var blocked = await service.CreateAsync(
            "COMPANY-1",
            incompleteDesign.DesignId,
            new OgDrillProgramValues("Bravo program", "Drain the lower sand", 2600m, 2450m, "m", 20m, "USD"),
            "sara");

        Assert.True(blocked.Success, blocked.Message);
        Assert.Contains("Casing scheme", blocked.Value!.DesignMissing);

        Assert.True((await service.SaveSectionAsync(
            blocked.Value.Id,
            new OgDrillSectionValues(1, "Production hole", 8.5m, "in", 0m, 2600m, "m", null, null, 10m, "ppg", 10m, 500_000m),
            "sara")).Success);

        var designGate = await service.SubmitAsync(blocked.Value.Id, "sara");

        Assert.False(designGate.Success);
        Assert.Equal(OgDrillProgramService.CodeDesignIncomplete, designGate.Reference);
        Assert.Contains("Casing scheme", designGate.Message!, StringComparison.Ordinal);

        // A lost acknowledgement retains the submit and a retry sends it unchanged.
        var pending = await service.CreateAsync(
            "COMPANY-1",
            completeDesign.DesignId,
            new OgDrillProgramValues("Charlie program", "Appraisal", 2600m, 2450m, "m", 15m, "USD"),
            "sara");

        Assert.True(pending.Success, pending.Message);

        Assert.True((await service.SaveSectionAsync(
            pending.Value!.Id,
            new OgDrillSectionValues(1, "Surface hole", 17.5m, "in", 0m, 300m, "m", 13.375m, "in", 9.5m, "ppg", 4m, 200_000m),
            "sara")).Success);

        ForceSubmitFailure(database, pending.Value.Id);
        var lost = await service.SubmitAsync(pending.Value.Id, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);

        // Nothing was written: the version is still a draft.
        var stillDraft = await service.LoadVersionsAsync(pending.Value!.LineageId);

        Assert.True(stillDraft.Success, stillDraft.Message);
        Assert.Equal(OgDrillProgram.Draft, stillDraft.Value!.Single().State);

        DropFailureTrigger(database);
        var retried = await service.RetrySubmitAsync();

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(OgDrillProgram.Submitted, retried.Value!.State);
        Assert.Equal(pending.Value.Id, retried.Value.Id);
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
                FDP_VERSION INTEGER NOT NULL);

            CREATE TABLE FDP_BASELINE (
                BASELINE_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NOT NULL,
                DRAFT_ID TEXT NOT NULL,
                VERSION INTEGER NOT NULL,
                APPROVED_AT TEXT NULL,
                APPROVED_BY TEXT NULL);

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
                SPUD_WINDOW_END TEXT NULL);

            CREATE TABLE DEVELOPMENT_CASING_STRING (
                CASING_STRING_ID TEXT NOT NULL PRIMARY KEY,
                DESIGN_ID TEXT NOT NULL,
                STRING_ORDER INTEGER NOT NULL,
                STRING_NAME TEXT NULL,
                OD REAL NOT NULL,
                WEIGHT REAL NOT NULL,
                GRADE TEXT NULL,
                SET_DEPTH REAL NOT NULL,
                DEPTH_OUOM TEXT NULL);

            CREATE TABLE DEVELOPMENT_CASING_CHECK (
                CHECK_ID TEXT NOT NULL PRIMARY KEY,
                DESIGN_ID TEXT NOT NULL,
                OUTCOME TEXT NULL,
                FINDINGS_TEXT TEXT NULL,
                CHECKED_BY TEXT NULL,
                CHECKED_AT TEXT NULL);

            CREATE TABLE DEVELOPMENT_DRILLING_PROGRAM (
                PROGRAM_ID TEXT NOT NULL PRIMARY KEY,
                LINEAGE_ID TEXT NOT NULL,
                VERSION INTEGER NOT NULL,
                COMPANY_ID TEXT NULL,
                FDP_ID TEXT NULL,
                FDP_BASELINE_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                WELL_ID TEXT NOT NULL,
                DESIGN_ID TEXT NULL,
                PROGRAM_NAME TEXT NOT NULL,
                DESCRIPTION_OBJECTIVE TEXT NULL,
                STATE TEXT NULL,
                PLANNED_MEASURED_DEPTH REAL NULL,
                PLANNED_TRUE_VERTICAL_DEPTH REAL NULL,
                DEPTH_OUOM TEXT NULL,
                PLANNED_DURATION_DAYS REAL NULL,
                CURRENCY TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DRILLING_PROGRAM_SECTION (
                SECTION_ID TEXT NOT NULL PRIMARY KEY,
                PROGRAM_ID TEXT NOT NULL,
                SECTION_SEQ INTEGER NOT NULL,
                SECTION_NAME TEXT NULL,
                HOLE_SIZE REAL NOT NULL,
                HOLE_SIZE_OUOM TEXT NULL,
                TOP_MD REAL NOT NULL,
                BOTTOM_MD REAL NOT NULL,
                DEPTH_OUOM TEXT NULL,
                CASING_OD REAL NOT NULL,
                CASING_OD_OUOM TEXT NULL,
                MUD_WEIGHT REAL NOT NULL,
                MUD_WEIGHT_OUOM TEXT NULL,
                DURATION_DAYS REAL NULL,
                COST REAL NULL,
                COST_CURRENCY TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DRILLING_PROGRAM_CHECK (
                CHECK_ID TEXT NOT NULL PRIMARY KEY,
                PROGRAM_ID TEXT NOT NULL,
                CHECK_NAME TEXT NOT NULL,
                STATUS TEXT NOT NULL,
                REFERENCE TEXT NULL,
                NOTE TEXT NULL,
                CHECKED_BY TEXT NULL,
                CHECKED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DRILLING_PROGRAM_REVIEW (
                REVIEW_ID TEXT NOT NULL PRIMARY KEY,
                PROGRAM_ID TEXT NOT NULL,
                PROGRAM_VERSION INTEGER NOT NULL,
                DECISION TEXT NULL,
                COMMENTS TEXT NULL,
                REVIEWER TEXT NULL,
                REVIEWED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO FIELD_DEVELOPMENT_PLAN (FDP_ID, FIELD_ID, FDP_NAME, FDP_VERSION) VALUES
                ('FDP-1', 'F-1', 'Alpha FDP', 2);

            INSERT INTO FDP_BASELINE (BASELINE_ID, FDP_ID, DRAFT_ID, VERSION, APPROVED_AT, APPROVED_BY) VALUES
                ('B-1', 'FDP-1', 'DRAFT-1', 1, '2026-01-10', 'reviewer');

            INSERT INTO DEVELOPMENT_WELL_DESIGN
                (DESIGN_ID, SCHEDULE_ID, FDP_ID, FIELD_ID, PLANNED_WELL_ID, WELL_NAME, VERSION, CURRENT_IND,
                 DESIGN_STATE, OBJECTIVE, TARGET_FORMATION, TARGET_DEPTH, DEPTH_OUOM, TRAJECTORY_INTENT,
                 COMPLETION_CONCEPT, ESTIMATED_COST, COST_CURRENCY, SPUD_WINDOW_START, SPUD_WINDOW_END) VALUES
                ('D-1', 'SCH-1', 'FDP-1', 'F-1', 'WELL-1', 'WELL-1', 1, 'Y', 'DEFINED', 'Drain the upper sand',
                 'Upper sand', 2500, 'm', 'Vertical', 'Perforated liner', 12.5, 'USD', '2026-08-01', '2026-09-01'),
                ('D-2', 'SCH-2', 'FDP-1', 'F-1', 'WELL-2', 'WELL-2', 1, 'Y', 'DEFINED', 'Drain the lower sand',
                 'Lower sand', 2600, 'm', 'Horizontal', 'Screens', 9.0, 'USD', '2026-10-01', '2026-11-01');

            INSERT INTO DEVELOPMENT_CASING_STRING
                (CASING_STRING_ID, DESIGN_ID, STRING_ORDER, STRING_NAME, OD, WEIGHT, GRADE, SET_DEPTH, DEPTH_OUOM) VALUES
                ('CS-1', 'D-1', 1, 'SURFACE', 20, 94, 'K55', 400, 'm'),
                ('CS-2', 'D-1', 2, 'PRODUCTION', 9.625, 47, 'P110', 2500, 'm');
            """;
        command.ExecuteNonQuery();
    }

    private static void ForceSubmitFailure(string database, string programId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER BLOCK_SUBMIT BEFORE UPDATE ON DEVELOPMENT_DRILLING_PROGRAM
             WHEN NEW.PROGRAM_ID = '{programId}' AND NEW.STATE = 'SUBMITTED'
             BEGIN SELECT RAISE(ABORT, 'forced'); END;
             """;
        command.ExecuteNonQuery();
    }

    private static void DropFailureTrigger(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DROP TRIGGER BLOCK_SUBMIT;";
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
