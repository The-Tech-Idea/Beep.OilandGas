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
/// DEV-02 against a real SQLite database: create, save, submit and lock; a return that names the step; an approval
/// that appends a baseline; a revision that starts a new draft and approval that appends a second baseline while the
/// first stays readable; the author cannot review; and a lost submit acknowledgement is retained for retry. Opt-in
/// (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class FdpWizardGatewayTests
{
    private const string ConnectionName = "dev02-fdp";

    [IntegrationFact]
    public async Task The_wizard_submits_reviews_revises_and_keeps_every_baseline()
    {        var root = Path.Combine(OgPaths.DataRoot, "dev02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasDev02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "fdp.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "fdp.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var wizard = new OgFdpWizardService(() => source, reporter);

        // A draft is created, saved across the wizard and submitted.
        var created = await wizard.CreateDraftAsync("F-1", "Alpha field", "Alpha FDP", "sara");

        Assert.True(created.Success, created.Message);
        var plan = created.Value!;
        var draftId = plan.Current!.Id;

        AddSchedule(database, plan.Id);

        var saved = await wizard.SaveDraftAsync(
            draftId,
            new OgFdpDraftValues("Alpha development", "Upper sand", 4, "Central facility", 1200m, "USD", "Water disposal", 7),
            "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Empty(OgFdpWizardService.MissingSteps(saved.Value!.Current!, hasSchedule: true));

        var submitted = await wizard.SubmitAsync(draftId, "sara");

        Assert.True(submitted.Success, submitted.Message);
        Assert.Equal(OgFdpDraft.Submitted, submitted.Value!.Current!.State);
        Assert.Equal(OgFdpDraft.Submitted, submitted.Value.PlanStatus);

        // AC-1: a submitted draft cannot be edited.
        var locked = await wizard.SaveDraftAsync(
            draftId,
            new OgFdpDraftValues("Changed", "Upper sand", 4, "Central", 1300m, "USD", "Risk", 7),
            "sara");

        Assert.False(locked.Success);
        Assert.Equal(OgFdpWizardService.CodeLocked, locked.Reference);

        // AC-4: the author cannot review their own plan.
        var selfReview = await wizard.ReviewAsync(
            draftId,
            new OgFdpDecision(OgFdpDecision.Approve, "approved", null, [], "sara"));

        Assert.False(selfReview.Success);
        Assert.Equal(OgFdpWizardService.CodeAuthorCannotApprove, selfReview.Reference);

        // AC-3: a return names the step to correct.
        var returned = await wizard.ReviewAsync(
            draftId,
            new OgFdpDecision(OgFdpDecision.Return, "capacity is not justified", 4, [], "reviewer"));

        Assert.True(returned.Success, returned.Message);
        Assert.Equal(OgFdpDraft.Returned, returned.Value!.Current!.State);
        Assert.Equal(4, returned.Value.Current.ReturnedStep);
        Assert.Contains("Facilities and capacity", returned.Value.Current.ReturnText!, StringComparison.Ordinal);

        // A returned draft is editable again and can be resubmitted.
        var corrected = await wizard.SaveDraftAsync(
            draftId,
            new OgFdpDraftValues("Alpha development", "Upper sand", 4, "Central facility 120 MM", 1200m, "USD", "Water disposal", 7),
            "sara");

        Assert.True(corrected.Success, corrected.Message);

        var resubmitted = await wizard.SubmitAsync(draftId, "sara");

        Assert.True(resubmitted.Success, resubmitted.Message);

        // AC-5: the approval appends a baseline carrying the decimal cost and currency.
        var approved = await wizard.ReviewAsync(
            draftId,
            new OgFdpDecision(OgFdpDecision.Approve, "approved after correction", null, [], "reviewer"));

        Assert.True(approved.Success, approved.Message);
        Assert.Equal(OgFdpDraft.Approved, approved.Value!.Current!.State);
        Assert.Single(approved.Value.Baselines);
        Assert.Equal(1200m, approved.Value.Baselines[0].CostAmount);
        Assert.Equal("USD", approved.Value.Baselines[0].CostCurrency);

        // AC-2/AC-6: a revision starts a new draft; approving it appends a second baseline and the first is intact.
        var revised = await wizard.ReviseAsync(draftId, "sara");

        Assert.True(revised.Success, revised.Message);
        var second = revised.Value!.Current!;

        Assert.Equal(2, second.Version);
        Assert.Equal(OgFdpDraft.Draft, second.State);
        Assert.Equal("Alpha development", second.Scope);

        var changed = await wizard.SaveDraftAsync(
            second.Id,
            new OgFdpDraftValues("Alpha development v2", "Upper and lower sand", 5, "Central facility 150 MM", 1500m, "USD", "Water disposal", 7),
            "sara");

        Assert.True(changed.Success, changed.Message);

        // A lost submit acknowledgement is retained and retried unchanged.
        ForceDraftUpdateFailure(database, second.Id);
        var lost = await wizard.SubmitAsync(second.Id, "sara");

        Assert.False(lost.Success);
        Assert.NotNull(wizard.Pending);

        DropFailureTrigger(database);
        var retried = await wizard.RetrySubmitAsync();

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(OgFdpDraft.Submitted, retried.Value!.Current!.State);

        var secondApproval = await wizard.ReviewAsync(
            second.Id,
            new OgFdpDecision(OgFdpDecision.ApproveWithConditions, "approved with conditions", null,
                ["Monthly pressure report", "No flaring above 5 MMscf/d"], "reviewer"));

        Assert.True(secondApproval.Success, secondApproval.Message);
        Assert.Equal(2, secondApproval.Value!.Baselines.Count);
        Assert.Equal(2, secondApproval.Value.Baselines[0].Version);
        Assert.Equal(1500m, secondApproval.Value.Baselines[0].CostAmount);
        Assert.Equal(2, secondApproval.Value.Baselines[0].Conditions.Count);

        var firstBaseline = secondApproval.Value.Baselines.Single(baseline => baseline.Version == 1);

        Assert.Equal(1200m, firstBaseline.CostAmount);
        Assert.Equal("Alpha development", firstBaseline.Scope);

        // The field filter excludes the other field's plan.
        var workspace = await wizard.LoadAsync("F-1", "Alpha field");

        Assert.True(workspace.Success, workspace.Message);
        var only = Assert.Single(workspace.Value!.Plans);

        Assert.Equal(plan.Id, only.Id);
        Assert.Equal(2, only.Baselines.Count);
        Assert.Equal(3, only.Reviews.Count);
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
                TENANT_ID TEXT NULL,
                BUSINESS_ASSOCIATE_ID TEXT NULL,
                DRAFT_INTENT_ID TEXT NULL,
                DRAFT_REQUEST_REVISION TEXT NULL,
                FIELD_ID TEXT NULL,
                FDP_NAME TEXT NULL,
                FDP_VERSION INTEGER NOT NULL,
                FDP_STATUS TEXT NULL,
                SUBMISSION_DATE TEXT NULL,
                TOTAL_WELLS_PLANNED REAL NULL,
                TOTAL_CAPEX_MM REAL NULL,
                CAPEX_CURRENCY TEXT NULL,
                DEVELOPMENT_CONCEPT_DESC TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FDP_DRAFT (
                DRAFT_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NOT NULL,
                FIELD_ID TEXT NOT NULL,
                VERSION INTEGER NOT NULL,
                STATE TEXT NULL,
                CURRENT_STEP INTEGER NOT NULL,
                AUTHOR TEXT NULL,
                SCOPE_TEXT TEXT NULL,
                RESERVOIR_TEXT TEXT NULL,
                WELL_COUNT INTEGER NOT NULL,
                FACILITY_TEXT TEXT NULL,
                COST_AMOUNT REAL NOT NULL,
                COST_CURRENCY TEXT NULL,
                RISK_TEXT TEXT NULL,
                BASELINE_ID TEXT NULL,
                RETURNED_STEP INTEGER NULL,
                RETURN_REASON TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE FDP_BASELINE (
                BASELINE_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NOT NULL,
                DRAFT_ID TEXT NOT NULL,
                VERSION INTEGER NOT NULL,
                APPROVED_AT TEXT NULL,
                APPROVED_BY TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
                SCOPE_TEXT TEXT NULL,
                RESERVOIR_TEXT TEXT NULL,
                WELL_COUNT INTEGER NOT NULL,
                FACILITY_TEXT TEXT NULL,
                COST_AMOUNT REAL NOT NULL,
                COST_CURRENCY TEXT NULL,
                RISK_TEXT TEXT NULL);

            CREATE TABLE FDP_REVIEW (
                REVIEW_ID TEXT NOT NULL PRIMARY KEY,
                DRAFT_ID TEXT NOT NULL,
                FDP_ID TEXT NOT NULL,
                DECISION TEXT NULL,
                RATIONALE TEXT NULL,
                RETURNED_STEP INTEGER NULL,
                CONDITIONS_TEXT TEXT NULL,
                REVIEWER TEXT NULL,
                REVIEWED_AT TEXT NULL);

            CREATE TABLE DEVELOPMENT_WELL_SCHEDULE (
                SCHEDULE_ID TEXT NOT NULL PRIMARY KEY,
                FDP_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                PLANNED_WELL_ID TEXT NULL,
                WELL_NAME TEXT NULL,
                SCHEDULE_STATUS TEXT NULL);

            INSERT INTO FIELD_DEVELOPMENT_PLAN
                (FDP_ID, FIELD_ID, FDP_NAME, FDP_VERSION, FDP_STATUS)
            VALUES ('FDP-OTHER', 'F-2', 'Other field plan', 1, 'DRAFT');
            """;
        command.ExecuteNonQuery();
    }

    private static void AddSchedule(string database, string fdpId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"INSERT INTO DEVELOPMENT_WELL_SCHEDULE (SCHEDULE_ID, FDP_ID, FIELD_ID, PLANNED_WELL_ID, WELL_NAME, SCHEDULE_STATUS) " +
            $"VALUES ('SCH-1', '{fdpId}', 'F-1', 'WELL-1', 'WELL-1', 'PLANNED');";
        command.ExecuteNonQuery();
    }

    private static void ForceDraftUpdateFailure(string database, string draftId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER BLOCK_DRAFT BEFORE UPDATE ON FDP_DRAFT
             WHEN NEW.DRAFT_ID = '{draftId}'
             BEGIN SELECT RAISE(ABORT, 'forced'); END;
             """;
        command.ExecuteNonQuery();
    }

    private static void DropFailureTrigger(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DROP TRIGGER BLOCK_DRAFT;";
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
