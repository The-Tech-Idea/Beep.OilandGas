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
/// DEV-06 against a real SQLite database: an execution starts only from an approved program; a duplicate date+shift is
/// refused and a second submission is refused (AC-1); a submitted report is read-only and a correction is a new
/// revision with its reason while the original stays submitted (AC-2); a lost acknowledgement leaves the report a draft
/// and retries (AC-3); a transfer records actor, receiver and reason and disables the previous holder (AC-4); an NPT
/// line needs a reference code (AC-5); and a break in the depth chain is flagged (AC-6). Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class DrillingExecutionGatewayTests
{
    private const string ConnectionName = "dev06-execution";
    private static readonly DateTime Day1 = new(2026, 8, 1);
    private static readonly DateTime Day2 = new(2026, 8, 2);

    [IntegrationFact]
    public async Task The_execution_records_reports_corrects_and_hands_over_responsibility()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev06-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev06"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "execution.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "execution.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgDrillingExecutionService(() => source, reporter);

        var loaded = await service.LoadAsync("F-1", "Alpha field");

        Assert.True(loaded.Success, loaded.Message);
        var workspace = loaded.Value!;

        // Only the approved program is offered to run.
        Assert.Equal("PR-1", Assert.Single(workspace.StartablePrograms).Id);
        Assert.Empty(workspace.Executions);

        // A draft program cannot be executed.
        var refused = await service.StartAsync("COMPANY-1", "PR-2", Day1, "sara", "COMPANY_MAN", "sara");

        Assert.False(refused.Success);
        Assert.Equal(OgDrillingExecutionService.CodeProgramNotApproved, refused.Reference);

        var started = await service.StartAsync("COMPANY-1", "PR-1", Day1, "sara", "COMPANY_MAN", "sara");

        Assert.True(started.Success, started.Message);
        var execution = Assert.Single(started.Value!.Executions);

        Assert.Equal("WELL-1", execution.WellId);
        Assert.Equal(1, execution.ProgramVersion);
        Assert.Equal("sara", execution.Responsible);
        Assert.Equal("COMPANY_MAN", execution.ResponsibleRole);

        // AC-1 (creation): one report per day and shift.
        var day1 = await service.CreateReportAsync(execution.Id, Values(Day1, "DAY", 0m, 500m), "sara");

        Assert.True(day1.Success, day1.Message);

        var duplicate = await service.CreateReportAsync(execution.Id, Values(Day1, "DAY", 500m, 900m), "sara");

        Assert.False(duplicate.Success);
        Assert.Equal(OgDrillingExecutionService.CodeDuplicatePeriod, duplicate.Reference);

        var day1Report = Assert.Single(day1.Value!.Reports);

        // AC-5: an NPT line needs a code the reference set holds.
        var noCode = await service.AddActivityAsync(
            day1Report.Id, new OgDailyActivityValues("NPT", 2m, "pump failure", true, null, 0m), "sara");

        Assert.False(noCode.Success);
        Assert.Equal(OgDrillingExecutionService.CodeNptCodeRequired, noCode.Reference);

        var unknownCode = await service.AddActivityAsync(
            day1Report.Id, new OgDailyActivityValues("NPT", 2m, "pump failure", true, "MADE-UP", 0m), "sara");

        Assert.False(unknownCode.Success);
        Assert.Equal(OgDrillingExecutionService.CodeNptCodeUnknown, unknownCode.Reference);

        Assert.True((await service.AddActivityAsync(
            day1Report.Id, new OgDailyActivityValues("DRILL", 8m, "drilling 12-1/4\" hole", false, null, 0m), "sara")).Success);
        Assert.True((await service.AddActivityAsync(
            day1Report.Id, new OgDailyActivityValues("NPT", 2m, "pump failure", true, "EQ-FAIL", 300m), "sara")).Success);

        var withLines = await service.LoadAsync("F-1", "Alpha field", execution.Id);
        var lineReport = Assert.Single(withLines.Value!.Reports);

        Assert.Equal(2, lineReport.ActivityCount);
        Assert.Equal(2m, lineReport.NptHours);
        Assert.Equal(500m, lineReport.MetresDrilled);

        // AC-6: the next report continues the depth; one that does not is flagged.
        Assert.True((await service.CreateReportAsync(execution.Id, Values(Day1, "NIGHT", 500m, 900m), "sara")).Success);

        var breakReport = await service.CreateReportAsync(execution.Id, Values(Day2, "DAY", 950m, 1200m), "sara");

        Assert.True(breakReport.Success, breakReport.Message);

        var ordered = breakReport.Value!.Reports
            .OrderBy(report => report.Date)
            .ThenBy(report => report.Shift == "NIGHT" ? 1 : 0)
            .ToList();

        Assert.Null(ordered[0].DepthFlag);   // day 1, day shift starts at 0
        Assert.Null(ordered[1].DepthFlag);   // night shift starts where the day shift ended
        Assert.True(ordered[2].DepthFlagged); // day 2 starts at 950, not 900
        Assert.Contains("900", ordered[2].DepthFlag!, StringComparison.Ordinal);

        // AC-2: submit, then the report is read-only.
        var submitted = await service.SubmitReportAsync(day1Report.Id, "sara");

        Assert.True(submitted.Success, submitted.Message);

        var submittedReport = submitted.Value!.Reports.Single(report => report.Id == day1Report.Id);

        Assert.True(submittedReport.IsSubmitted);
        Assert.Equal("sara", submittedReport.SubmittedBy);

        var locked = await service.SaveReportAsync(day1Report.Id, Values(Day1, "DAY", 0m, 600m), "sara");

        Assert.False(locked.Success);
        Assert.Equal(OgDrillingExecutionService.CodeReportLocked, locked.Reference);

        // AC-1 (submission): the same report cannot be submitted twice.
        var twice = await service.SubmitReportAsync(day1Report.Id, "sara");

        Assert.False(twice.Success);

        // AC-2: a correction is a new revision with a reason; the submitted one is untouched.
        var noReason = await service.ReviseReportAsync(day1Report.Id, " ", "sara");

        Assert.False(noReason.Success);
        Assert.Equal(OgDrillingExecutionService.CodeReasonRequired, noReason.Reference);

        var corrected = await service.ReviseReportAsync(day1Report.Id, "depth at end was mis-read", "sara");

        Assert.True(corrected.Success, corrected.Message);

        var revision = corrected.Value!.Reports.Single(report => report.Revision == 2 && report.Date == Day1 && report.Shift == "DAY");

        Assert.True(revision.IsDraft);
        Assert.Equal(day1Report.Id, revision.CorrectsReportId);
        Assert.Equal("depth at end was mis-read", revision.CorrectionReason);
        Assert.Equal(2, revision.ActivityCount);

        var original = corrected.Value.Reports.Single(report => report.Id == day1Report.Id);

        Assert.True(original.IsSubmitted);
        Assert.Equal(500m, original.EndDepth);

        // AC-4: a transfer records the actor, the receiver and the reason, and the previous holder cannot write.
        var noReceiver = await service.TransferResponsibilityAsync(execution.Id, " ", "COMPANY_MAN", "handover", "sara");

        Assert.False(noReceiver.Success);
        Assert.Equal(OgDrillingExecutionService.CodeActorRequired, noReceiver.Reference);

        var sameHolder = await service.TransferResponsibilityAsync(execution.Id, "sara", "COMPANY_MAN", "handover", "sara");

        Assert.False(sameHolder.Success);
        Assert.Equal(OgDrillingExecutionService.CodeSameHolder, sameHolder.Reference);

        var transferred = await service.TransferResponsibilityAsync(
            execution.Id, "omar", "TOOLPUSHER", "night shift handover", "sara");

        Assert.True(transferred.Success, transferred.Message);
        Assert.Equal("omar", transferred.Value!.Selected!.Responsible);
        Assert.Equal("TOOLPUSHER", transferred.Value.Selected.ResponsibleRole);
        Assert.Equal(2, transferred.Value.Responsibilities.Count);

        var closed = transferred.Value.Responsibilities.Single(row => row.AssignedTo == "sara");
        var open = transferred.Value.Responsibilities.Single(row => row.AssignedTo == "omar");

        Assert.False(closed.Current);
        Assert.NotNull(closed.To);
        Assert.True(open.Current);
        Assert.Null(open.To);
        Assert.Equal("sara", open.Actor);
        Assert.Equal("night shift handover", open.Reason);

        var previousHolder = await service.CreateReportAsync(execution.Id, Values(Day2, "NIGHT", 1200m, 1400m), "sara");

        Assert.False(previousHolder.Success);
        Assert.Equal(OgDrillingExecutionService.CodeNotResponsible, previousHolder.Reference);

        var newHolder = await service.CreateReportAsync(execution.Id, Values(Day2, "NIGHT", 1200m, 1400m), "omar");

        Assert.True(newHolder.Success, newHolder.Message);

        // AC-3: a lost acknowledgement leaves the report a draft and is retained for a retry.
        var pending = newHolder.Value!.Reports.Single(report => report.Date == Day2 && report.Shift == "NIGHT");

        ForceSubmitFailure(database, pending.Id);
        var lost = await service.SubmitReportAsync(pending.Id, "omar");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);

        var stillDraft = await service.LoadAsync("F-1", "Alpha field", execution.Id);

        Assert.True(stillDraft.Value!.Reports.Single(report => report.Id == pending.Id).IsDraft);

        DropFailureTrigger(database);
        var retried = await service.RetrySubmitAsync();

        Assert.True(retried.Success, retried.Message);
        Assert.True(retried.Value!.Reports.Single(report => report.Id == pending.Id).IsSubmitted);

        // With no NPT reference rows the workspace says so rather than accepting free text.
        ClearNptCodes(database);
        var withoutReference = await service.LoadAsync("F-1", "Alpha field", execution.Id);

        Assert.True(withoutReference.Value!.NptReferenceMissing);

        // The refusal is proved on a draft (a submitted report refuses any edit before it looks at the reference set).
        var draftReport = withoutReference.Value.Reports.Single(report => report.Date == Day2 && report.Shift == "DAY");

        Assert.True(draftReport.IsDraft);

        var noReference = await service.AddActivityAsync(
            draftReport.Id, new OgDailyActivityValues("NPT", 1m, "waiting", true, "EQ-FAIL", 1300m), "omar");

        Assert.False(noReference.Success);
        Assert.Equal(OgDrillingExecutionService.CodeNptReferenceMissing, noReference.Reference);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE DEVELOPMENT_DRILLING_PROGRAM (
                PROGRAM_ID TEXT NOT NULL PRIMARY KEY,
                LINEAGE_ID TEXT NOT NULL,
                VERSION INTEGER NOT NULL,
                FIELD_ID TEXT NULL,
                WELL_ID TEXT NOT NULL,
                PROGRAM_NAME TEXT NULL,
                STATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_DRILLING_EXECUTION (
                EXECUTION_ID TEXT NOT NULL PRIMARY KEY,
                PROGRAM_ID TEXT NOT NULL,
                FIELD_ID TEXT NULL,
                WELL_ID TEXT NOT NULL,
                COMPANY_ID TEXT NULL,
                STATUS TEXT NULL,
                PROGRAM_VERSION INTEGER NOT NULL,
                SPUD_DATE TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_DAILY_REPORT (
                REPORT_ID TEXT NOT NULL PRIMARY KEY,
                EXECUTION_ID TEXT NOT NULL,
                PROGRAM_ID TEXT NOT NULL,
                WELL_ID TEXT NOT NULL,
                REPORT_DATE TEXT NULL,
                SHIFT TEXT NULL,
                REVISION INTEGER NOT NULL,
                CORRECTS_REPORT_ID TEXT NULL,
                CORRECTION_REASON TEXT NULL,
                PERIOD_START TEXT NULL,
                PERIOD_END TEXT NULL,
                DEPTH_START REAL NOT NULL,
                DEPTH_END REAL NOT NULL,
                DEPTH_OUOM TEXT NULL,
                COST REAL NULL,
                CURRENCY TEXT NULL,
                HSE_EVENTS TEXT NULL,
                PERSONNEL TEXT NULL,
                EQUIPMENT TEXT NULL,
                STATE TEXT NULL,
                AUTHOR TEXT NULL,
                SUBMITTED_BY TEXT NULL,
                SUBMITTED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DRILLING_ACTIVITY (
                ACTIVITY_ID TEXT NOT NULL PRIMARY KEY,
                DDR_ID TEXT NOT NULL,
                WELL_ID TEXT NULL,
                START_TIME TEXT NULL,
                END_TIME TEXT NULL,
                DURATION_HOURS REAL NULL,
                ACTIVITY_CODE TEXT NULL,
                NPT_CODE_IADC TEXT NULL,
                NPT_IND TEXT NULL,
                DEPTH_AT_START_MD REAL NULL,
                DEPTH_OUOM TEXT NULL,
                ACTIVITY_DESCRIPTION TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_REPORT_RESPONSIBILITY (
                RESPONSIBILITY_ID TEXT NOT NULL PRIMARY KEY,
                EXECUTION_ID TEXT NOT NULL,
                ASSIGNED_TO TEXT NOT NULL,
                ROLE TEXT NULL,
                FROM_AT TEXT NULL,
                TO_AT TEXT NULL,
                REASON TEXT NULL,
                ACTOR TEXT NULL,
                CURRENT_IND TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE R_DRILLING_NPT_CODE (
                NPT_CODE TEXT NOT NULL PRIMARY KEY,
                NPT_NAME TEXT NULL,
                CATEGORY TEXT NULL,
                ACTIVE_IND TEXT NULL);

            INSERT INTO DEVELOPMENT_DRILLING_PROGRAM (PROGRAM_ID, LINEAGE_ID, VERSION, FIELD_ID, WELL_ID, PROGRAM_NAME, STATE) VALUES
                ('PR-1', 'PR-1', 1, 'F-1', 'WELL-1', 'Alpha drilling program', 'APPROVED'),
                ('PR-2', 'PR-2', 1, 'F-1', 'WELL-2', 'Draft program', 'DRAFT');

            INSERT INTO R_DRILLING_NPT_CODE (NPT_CODE, NPT_NAME, CATEGORY, ACTIVE_IND) VALUES
                ('EQ-FAIL', 'Equipment failure', 'EQUIPMENT', 'Y'),
                ('WEATHER', 'Waiting on weather', 'WEATHER', 'Y');
            """;
        command.ExecuteNonQuery();
    }

    private static OgDailyReportValues Values(DateTime date, string shift, decimal start, decimal end) =>
        new(date, shift, date, date.AddHours(12), start, end, "m", 250_000m, "USD", null, "12 crew", "Rig 3");

    private static void ForceSubmitFailure(string database, string reportId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER BLOCK_SUBMIT BEFORE UPDATE ON DEVELOPMENT_DAILY_REPORT
             WHEN NEW.REPORT_ID = '{reportId}' AND NEW.STATE = 'SUBMITTED'
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

    private static void ClearNptCodes(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM R_DRILLING_NPT_CODE;";
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
