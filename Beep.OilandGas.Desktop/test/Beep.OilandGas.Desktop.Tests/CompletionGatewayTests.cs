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
/// DEV-07 against a real SQLite database: a submission needs a test and evidence; evidence naming another well is
/// refused (AC-2); acceptance needs a passing test (AC-1); the author cannot decide their own completion (AC-3); an
/// acceptance with conditions carries them and the handover recipient onto the record production reads (AC-4); every
/// test states its result unit (AC-5); and a lost acknowledgement is retried without writing a second acceptance
/// (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class CompletionGatewayTests
{
    private const string ConnectionName = "dev07-completion";
    private static readonly DateTime Day = new(2026, 9, 1);

    [IntegrationFact]
    public async Task Completion_tests_are_proven_the_evidence_is_same_well_and_the_acceptance_is_decided_once()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev07-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev07"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "completion.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "completion.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgWellCompletionService(() => source, reporter);

        // The wells come from the drilling executions of the field.
        var loaded = await service.LoadAsync("F-1", "Alpha field");

        Assert.True(loaded.Success, loaded.Message);

        var wells = loaded.Value!.Wells;

        Assert.Equal(3, wells.Count);
        Assert.Equal("WELL-1", wells[0].WellId);
        Assert.Equal("P-1", wells[0].ProgramId);
        Assert.DoesNotContain(wells, well => well.WellId == "WELL-9"); // another field

        Assert.Null(loaded.Value.Selected);
        Assert.Empty(loaded.Value.Tests);
        Assert.Equal(2, loaded.Value.SubmissionGaps.Count);

        // A test on an unknown well is refused.
        var unknownWell = await service.RecordTestAsync("WELL-404", Values(), "sara");

        Assert.False(unknownWell.Success);
        Assert.Equal(OgWellCompletionService.CodeWellUnknown, unknownWell.Reference);

        // AC-5: a test without the unit of its result is refused.
        var noUnit = await service.RecordTestAsync("WELL-1", Values(resultUnit: null), "sara");

        Assert.False(noUnit.Success);
        Assert.Equal(CompletionRules.CodeUnitRequired, noUnit.Reference);

        var passed = await service.RecordTestAsync("WELL-1", Values(), "sara");

        Assert.True(passed.Success, passed.Message);
        var passingTest = Assert.Single(passed.Value!.Tests);

        Assert.Equal("PRESSURE_TEST", passingTest.TestType);
        Assert.True(passingTest.IsPassed);
        Assert.Equal("3500 psi", passingTest.ResultText);
        Assert.Equal(1, passed.Value.PassedTests);
        Assert.True(passed.Value.CanAccept);

        Assert.True((await service.RecordTestAsync(
            "WELL-1", Values(type: "INFLOW_TEST", status: "FAIL", result: "0", resultUnit: "bbl/d"), "sara")).Success);

        var created = await service.CreateSubmissionAsync("WELL-1", "WELL-1 completion", "sara");

        Assert.True(created.Success, created.Message);
        var submission = created.Value!.Selected!;

        Assert.Equal(OgCompletionSubmission.Draft, submission.State);
        Assert.Equal("sara", submission.Author);
        Assert.Equal("WELL-1", submission.WellId);

        // A second open submission for the same well is refused.
        var duplicate = await service.CreateSubmissionAsync("WELL-1", null, "sara");

        Assert.False(duplicate.Success);
        Assert.Equal(OgWellCompletionService.CodeAlreadySubmitted, duplicate.Reference);

        // AC-2: evidence naming another well is refused and nothing is filed.
        var foreign = await service.AddEvidenceAsync(
            submission.Id, "WELL-2", "CBL LOG", "cbl-w2.pdf", "sara");

        Assert.False(foreign.Success);
        Assert.Equal(OgWellCompletionService.CodeEvidenceForeignWell, foreign.Reference);
        Assert.Contains("WELL-2", foreign.Message!, StringComparison.Ordinal);

        var empty = await service.AddEvidenceAsync(submission.Id, "WELL-1", " ", "  ", "sara");

        Assert.False(empty.Success);
        Assert.Equal(OgWellCompletionService.CodeEvidenceRequired, empty.Reference);

        var evidence = await service.AddEvidenceAsync(
            submission.Id, "WELL-1", "CBL LOG", "cbl-2026-09-01.pdf", "sara");

        Assert.True(evidence.Success, evidence.Message);
        var item = Assert.Single(evidence.Value!.Evidence);

        Assert.Equal("WELL-1", item.WellId);
        Assert.Equal("cbl-2026-09-01.pdf", item.Reference);
        Assert.Empty(evidence.Value.SubmissionGaps);

        // A draft cannot be decided at all.
        var notSubmitted = await service.DecideAsync(
            submission.Id,
            new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"),
            "reviewer");

        Assert.False(notSubmitted.Success);
        Assert.Equal("completion-not-submitted", notSubmitted.Reference);

        var submitted = await service.SubmitAsync(submission.Id, "sara");

        Assert.True(submitted.Success, submitted.Message);
        Assert.Equal(OgCompletionSubmission.Submitted, submitted.Value!.Selected!.State);
        Assert.NotNull(submitted.Value.Selected.SubmittedAt);

        // AC-3: the author cannot decide their own submission, not even to accept it.
        var selfAccept = await service.DecideAsync(
            submission.Id,
            new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"),
            "sara");

        Assert.False(selfAccept.Success);
        Assert.Equal(CompletionRules.CodeAuthorCannotAccept, selfAccept.Reference);

        // AC-4: acceptance with conditions carries them and the handover recipient.
        var acceptWithConditions = await service.DecideAsync(
            submission.Id,
            new OgCompletionDecision(
                OgCompletionDecision.AcceptWithConditions,
                null,
                ["Install the flow meter within 30 days", "Run a second CBL in 6 months"],
                "production"),
            "reviewer");

        Assert.True(acceptWithConditions.Success, acceptWithConditions.Message);

        var accepted = acceptWithConditions.Value!.Selected!;

        Assert.Equal(OgCompletionSubmission.AcceptedWithConditions, accepted.State);
        Assert.True(accepted.IsAccepted);
        Assert.Equal("reviewer", accepted.Reviewer);
        Assert.Equal(2, accepted.Conditions.Count);

        var handover = acceptWithConditions.Value.Handover;

        Assert.NotNull(handover);
        Assert.Equal("WELL-1", handover!.WellId);
        Assert.Equal("reviewer", handover.AcceptedBy);
        Assert.Equal("production", handover.HandoverTo);
        Assert.Equal(2, handover.Conditions.Count);
        Assert.NotNull(handover.AcceptedAt);
        Assert.Contains("Install the flow meter within 30 days", handover.Describe(), StringComparison.Ordinal);

        // AC-6: the same acceptance replayed by the same reviewer is returned, not written a second time.
        var replay = await service.DecideAsync(
            submission.Id,
            new OgCompletionDecision(
                OgCompletionDecision.AcceptWithConditions,
                null,
                ["Install the flow meter within 30 days", "Run a second CBL in 6 months"],
                "production"),
            "reviewer");

        Assert.True(replay.Success, replay.Message);
        Assert.Equal(OgCompletionSubmission.AcceptedWithConditions, replay.Value!.Selected!.State);
        Assert.Equal(2, replay.Value.Selected.Conditions.Count);
        Assert.Equal(1, CountSubmissions(database, "WELL-1"));

        // AC-1: a well with no passing test cannot be accepted.
        Assert.True((await service.RecordTestAsync(
            "WELL-2", Values(status: "FAIL"), "sara")).Success);

        var well2 = await service.CreateSubmissionAsync("WELL-2", "WELL-2 completion", "sara");
        var well2Submission = well2.Value!.Selected!;

        Assert.True((await service.AddEvidenceAsync(
            well2Submission.Id, "WELL-2", "CBL LOG", "cbl-w2.pdf", "sara")).Success);
        Assert.True((await service.SubmitAsync(well2Submission.Id, "sara")).Success);

        var noPass = await service.DecideAsync(
            well2Submission.Id,
            new OgCompletionDecision(OgCompletionDecision.Accept, null, [], "production"),
            "reviewer");

        Assert.False(noPass.Success);
        Assert.Equal(CompletionRules.CodeNoPassingTest, noPass.Reference);
        Assert.Contains("PASS", noPass.Message!, StringComparison.Ordinal);

        // A return on the same failing well is allowed and names its reason.
        var returned = await service.DecideAsync(
            well2Submission.Id,
            new OgCompletionDecision(OgCompletionDecision.Return, "the CBL is missing", [], null),
            "reviewer");

        Assert.True(returned.Success, returned.Message);
        Assert.Equal(OgCompletionSubmission.Returned, returned.Value!.Selected!.State);
        Assert.True(returned.Value.Selected.IsEditable);
        Assert.Null(returned.Value.Handover);

        // A returned completion can be submitted again once the gap is filled.
        Assert.True((await service.RecordTestAsync(
            "WELL-2", Values(type: "LEAK_OFF", status: "PASS"), "sara")).Success);
        Assert.True((await service.SubmitAsync(well2Submission.Id, "sara")).Success);

        var well2Accepted = await service.DecideAsync(
            well2Submission.Id,
            new OgCompletionDecision(OgCompletionDecision.Accept, "tests now pass", [], "production"),
            "reviewer");

        Assert.True(well2Accepted.Success, well2Accepted.Message);
        Assert.Equal(OgCompletionSubmission.Accepted, well2Accepted.Value!.Selected!.State);
        Assert.Empty(well2Accepted.Value.Selected.Conditions);
        Assert.Equal("no conditions", well2Accepted.Value.Handover!.ConditionsText);

        // AC-6: a decision that was not acknowledged is retained, leaves the completion submitted, and is retried
        // without writing a second acceptance.
        Assert.True((await service.RecordTestAsync("WELL-3", Values(), "sara")).Success);

        var well3 = await service.CreateSubmissionAsync("WELL-3", "WELL-3 completion", "sara");
        var well3Submission = well3.Value!.Selected!;

        Assert.True((await service.AddEvidenceAsync(
            well3Submission.Id, "WELL-3", "ACCEPTANCE CERTIFICATE", "cert-w3.pdf", "sara")).Success);
        Assert.True((await service.SubmitAsync(well3Submission.Id, "sara")).Success);

        ForceAcceptFailure(database, well3Submission.Id);

        var lost = await service.DecideAsync(
            well3Submission.Id,
            new OgCompletionDecision(
                OgCompletionDecision.AcceptWithConditions,
                null,
                ["Flare line inspection within 14 days"],
                "production"),
            "reviewer");

        Assert.False(lost.Success);
        Assert.NotNull(service.Pending);
        Assert.Equal(well3Submission.Id, service.Pending!.SubmissionId);

        var stillSubmitted = await service.LoadAsync("F-1", "Alpha field", "WELL-3");

        Assert.True(stillSubmitted.Success, stillSubmitted.Message);
        Assert.Equal(OgCompletionSubmission.Submitted, stillSubmitted.Value!.Selected!.State);
        Assert.Null(stillSubmitted.Value.Handover);

        DropFailureTrigger(database);

        var retried = await service.RetryDecisionAsync();

        Assert.True(retried.Success, retried.Message);
        Assert.Null(service.Pending);

        var accepted3 = retried.Value!.Selected!;

        Assert.Equal(OgCompletionSubmission.AcceptedWithConditions, accepted3.State);
        Assert.Equal("reviewer", accepted3.Reviewer);
        Assert.Single(accepted3.Conditions);
        Assert.Equal("production", retried.Value.Handover!.HandoverTo);

        // The acceptance exists once, not twice, and a later re-read sees the same single record.
        Assert.Equal(1, CountSubmissions(database, "WELL-3"));
        Assert.Equal(1, CountAccepted(database, "WELL-3"));
    }

    private static void ForceAcceptFailure(string database, string submissionId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER BLOCK_ACCEPT BEFORE UPDATE ON DEVELOPMENT_COMPLETION_SUBMISSION
             WHEN NEW.SUBMISSION_ID = '{submissionId}' AND NEW.STATE LIKE 'ACCEPTED%'
             BEGIN SELECT RAISE(ABORT, 'forced'); END;
             """;
        command.ExecuteNonQuery();
    }

    private static void DropFailureTrigger(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "DROP TRIGGER BLOCK_ACCEPT;";
        command.ExecuteNonQuery();
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
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

            CREATE TABLE DEVELOPMENT_COMPLETION_TEST (
                TEST_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NOT NULL,
                PROGRAM_ID TEXT NULL,
                TEST_TYPE TEXT NOT NULL,
                TOP_DEPTH REAL NOT NULL,
                BASE_DEPTH REAL NOT NULL,
                DEPTH_OUOM TEXT NULL,
                STATUS TEXT NOT NULL,
                RESULT TEXT NULL,
                RESULT_OUOM TEXT NULL,
                PERFORMED_BY TEXT NULL,
                PERFORMED_AT TEXT NULL,
                EVIDENCE_REF TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_COMPLETION_SUBMISSION (
                SUBMISSION_ID TEXT NOT NULL PRIMARY KEY,
                WELL_ID TEXT NOT NULL,
                PROGRAM_ID TEXT NULL,
                SUMMARY TEXT NULL,
                STATE TEXT NULL,
                AUTHOR TEXT NULL,
                SUBMITTED_AT TEXT NULL,
                REVIEWER TEXT NULL,
                RATIONALE TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
                ACCEPTED_AT TEXT NULL,
                ACCEPTED_BY TEXT NULL,
                HANDOVER_TO TEXT NULL,
                HANDOVER_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE DEVELOPMENT_COMPLETION_EVIDENCE (
                EVIDENCE_ID TEXT NOT NULL PRIMARY KEY,
                SUBMISSION_ID TEXT NOT NULL,
                WELL_ID TEXT NOT NULL,
                DOCUMENT TEXT NULL,
                REFERENCE TEXT NULL,
                ADDED_BY TEXT NULL,
                ADDED_AT TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            INSERT INTO DEVELOPMENT_DRILLING_EXECUTION
                (EXECUTION_ID, PROGRAM_ID, FIELD_ID, WELL_ID, COMPANY_ID, STATUS, PROGRAM_VERSION, SPUD_DATE) VALUES
                ('EX-1', 'P-1', 'F-1', 'WELL-1', 'COMPANY-1', 'ACTIVE', 1, '2026-08-01'),
                ('EX-2', 'P-2', 'F-1', 'WELL-2', 'COMPANY-1', 'ACTIVE', 1, '2026-08-10'),
                ('EX-3', 'P-3', 'F-1', 'WELL-3', 'COMPANY-1', 'ACTIVE', 1, '2026-08-15'),
                ('EX-9', 'P-9', 'F-2', 'WELL-9', 'COMPANY-1', 'ACTIVE', 1, '2026-08-20');
            """;
        command.ExecuteNonQuery();
    }

    private static int CountSubmissions(string database, string wellId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM DEVELOPMENT_COMPLETION_SUBMISSION WHERE WELL_ID = $well;";
        command.Parameters.AddWithValue("$well", wellId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static int CountAccepted(string database, string wellId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT COUNT(*) FROM DEVELOPMENT_COMPLETION_SUBMISSION WHERE WELL_ID = $well AND STATE LIKE 'ACCEPTED%';";
        command.Parameters.AddWithValue("$well", wellId);

        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static OgCompletionTestValues Values(
        string type = "PRESSURE_TEST",
        decimal? top = 2400m,
        decimal? bottom = 2500m,
        string? depthUnit = "m",
        string status = "PASS",
        string? result = "3500",
        string? resultUnit = "psi") =>
        new(type, top, bottom, depthUnit, status, result, resultUnit, "sara", Day, "CBL-1");

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
