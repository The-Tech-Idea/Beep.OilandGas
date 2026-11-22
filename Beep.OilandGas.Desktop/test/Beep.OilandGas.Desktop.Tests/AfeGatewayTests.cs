using Beep.OilandGas.ProductionAccounting.Constants;
using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ECO-02 against a real SQLite database: the whole AFE journey - raise, submit, approve (with the author refused), commit,
/// supplement, approve the supplement and close - and the four things that must not happen: a commitment above the
/// remaining amount (AC-2), an approval by the author (AC-3), a supplement that mutates the approved version (AC-1,
/// SG-09-4) and a write to a closed AFE (AC-5). The supplement is inert until approved (AC-6).
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class AfeGatewayTests
{
    private const string ConnectionName = "eco02-afe";

    [IntegrationFact]
    public async Task The_afe_journey_and_the_immutability_of_the_approved_revision()
    {        var root = Path.Combine(OgPaths.DataRoot, "eco02-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEco02"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "afe.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "afe.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgAfeService(() => source, reporter);

        // ---- the workspace ---------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Afes);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Equal(AfeLifecycle.All, loaded.Value.States);
        Assert.Contains("supplement raises a new revision", loaded.Value.LifecycleText, StringComparison.Ordinal);

        // ---- raise ------------------------------------------------------------
        var raised = await service.RaiseAsync(Entry(), "sara");

        Assert.True(raised.Success, raised.Message + " :: " + string.Join(" | ", reporter.Failures));

        var draft = raised.Value!.Selected!;

        Assert.Equal(AfeLifecycle.Draft, draft.Status);
        Assert.Equal("sara", draft.Author);
        Assert.StartsWith("AFE-", draft.Number, StringComparison.Ordinal);
        Assert.EndsWith("-001", draft.Number, StringComparison.Ordinal);
        Assert.Equal(1, draft.RevisionNo);

        // ---- a draft cannot jump straight to approved -------------------------
        var premature = await service.ApproveAsync(draft.Id, "omar");

        Assert.False(premature.Success);
        Assert.Equal(AfeRules.CodeTransitionRefused, premature.Reference);

        // ---- submit ----------------------------------------------------------
        var submitted = await service.SubmitAsync(draft.Id, "sara");

        Assert.True(submitted.Success, submitted.Message);
        Assert.Equal(AfeLifecycle.Submitted, submitted.Value!.Selected!.Status);

        // ---- AC-3: the author cannot approve --------------------------------
        var byAuthor = await service.ApproveAsync(draft.Id, "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(AfeRules.CodeAuthorCannotApprove, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);

        var approved = await service.ApproveAsync(draft.Id, "omar");

        Assert.True(approved.Success, approved.Message);

        var approvedAfe = approved.Value!.Selected!;

        Assert.Equal(AfeLifecycle.Approved, approvedAfe.Status);
        Assert.Equal("omar", approvedAfe.Approver);
        Assert.True(approvedAfe.IsEffective);
        Assert.Equal(1_000m, approvedAfe.Amount);

        // ---- AC-2: a commitment above the remaining amount is refused --------
        var first = await service.CommitAsync(draft.Id, "PO-1001", 400m, "sara");

        Assert.True(first.Success, first.Message);
        Assert.Equal(400m, first.Value!.Selected!.Committed);
        Assert.Equal(600m, first.Value.Selected.Remaining);

        var tooMuch = await service.CommitAsync(draft.Id, "PO-1002", 700m, "sara");

        Assert.False(tooMuch.Success);
        Assert.Equal(AfeRules.CodeCommitmentExceedsRemaining, tooMuch.Reference);
        Assert.Contains("600.00", tooMuch.Message!, StringComparison.Ordinal);

        // Exactly the remaining amount is allowed.
        var toTheLimit = await service.CommitAsync(draft.Id, "PO-1003", 600m, "sara");

        Assert.True(toTheLimit.Success, toTheLimit.Message);
        Assert.Equal(1_000m, toTheLimit.Value!.Selected!.Committed);

        // AC-4: remaining is the amount less committed, exactly.
        Assert.Equal(0m, toTheLimit.Value.Selected.Remaining);
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM AFE WHERE AFE_ID = '" + draft.Id + "' AND ESTIMATED_COST = 1000 " +
            "AND APPROVED_AMOUNT = 1000 AND STATUS = 'APPROVED' AND APPROVER = 'omar';"));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM (SELECT SUM(AMOUNT) AS TOTAL FROM AFE_COMMITMENT WHERE AFE_ID = '" + draft.Id +
            "') WHERE TOTAL = 1000;"));

        // ---- AC-1 and SG-09-4: the supplement does not touch the approved row ----
        var before = Snapshot(database, draft.Id);

        var supplement = await service.SupplementAsync(draft.Id, 1_500m, "Third well added to the pad", "sara");

        Assert.True(supplement.Success, supplement.Message);

        var revision = supplement.Value!.Selected!;

        Assert.Equal(draft.Number, revision.Number);
        Assert.Equal(2, revision.RevisionNo);
        Assert.Equal(draft.Id, revision.SupersedesAfeId);
        Assert.Equal(draft.Id, revision.RootAfeId);
        Assert.Equal(AfeLifecycle.Draft, revision.Status);
        Assert.Equal(1_500m, revision.Amount);
        Assert.Equal("Third well added to the pad", revision.RevisionReason);

        // The approved row is byte-identical: no amount, status, approver or change stamp moved.
        Assert.Equal(before, Snapshot(database, draft.Id));

        // Supersession is derived from the new row, and the approved revision is no longer the effective one.
        var afterSupplement = supplement.Value.Afes.Single(afe => afe.Id == draft.Id);

        Assert.True(afterSupplement.IsSuperseded);
        Assert.False(afterSupplement.IsEffective);
        Assert.Equal(2, afterSupplement.Revisions.Count);
        Assert.Contains(afterSupplement.Revisions,
            chainItem => chainItem.RevisionNo == 2 && !chainItem.Effective);

        // ---- AC-6: the supplement is inert until it is approved --------------
        var onDraft = await service.CommitAsync(revision.Id, "PO-2001", 100m, "sara");

        Assert.False(onDraft.Success);
        Assert.Equal(AfeRules.CodeNotEffective, onDraft.Reference);

        Assert.True((await service.SubmitAsync(revision.Id, "sara")).Success);
        Assert.True((await service.ApproveAsync(revision.Id, "omar")).Success);

        var effective = (await service.CommitAsync(revision.Id, "PO-2001", 1_500m, "sara")).Value!.Selected!;

        Assert.True(effective.IsEffective);
        Assert.Equal(1_500m, effective.Committed);

        // ---- AC-5: a closed AFE is immutable ---------------------------------
        var closed = await service.CloseAsync(revision.Id, "Work complete", "sara");

        Assert.True(closed.Success, closed.Message);

        var closedAfe = closed.Value!.Selected!;

        Assert.True(closedAfe.IsClosed);
        Assert.Equal(AfeLifecycle.Closed, closedAfe.Status);
        Assert.Contains("closed by sara", closedAfe.ClosureText, StringComparison.Ordinal);

        Assert.Equal(
            AfeRules.CodeClosedImmutable,
            (await service.CommitAsync(revision.Id, "PO-2002", 10m, "sara")).Reference);
        Assert.Equal(
            AfeRules.CodeClosedImmutable,
            (await service.CloseAsync(revision.Id, "again", "sara")).Reference);
        Assert.Equal(
            AfeRules.CodeClosedImmutable,
            (await service.SupplementAsync(revision.Id, 100m, "again", "sara")).Reference);

        // ---- the reload keeps the whole chain --------------------------------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);
        Assert.Equal(2, reloaded.Value!.Afes.Count);

        var original = reloaded.Value.Afes.Single(afe => afe.Id == draft.Id);
        var closedRevision = reloaded.Value.Afes.Single(afe => afe.Id == revision.Id);

        Assert.Equal(AfeLifecycle.Approved, original.Status);
        Assert.True(original.IsSuperseded);
        Assert.Equal(1_000m, original.Committed);
        Assert.Equal(AfeLifecycle.Closed, closedRevision.Status);
        Assert.Equal(1_500m, closedRevision.Amount);
        Assert.Equal("omar", closedRevision.Approver);

        // ---- an unavailable register is a state -------------------------------
        Drop(database);
        DropCommitments(database);

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Afes);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database)
    {
        CreateAfeTable(database);
        CreateCommitmentTable(database);
    }

    private static void CreateAfeTable(string database) =>
        Execute(database,
            """
            CREATE TABLE AFE (
                AFE_ID TEXT NOT NULL PRIMARY KEY,
                AFE_NUMBER TEXT NOT NULL,
                AFE_NAME TEXT NOT NULL,
                PROPERTY_ID TEXT NULL,
                FIELD_ID TEXT NULL,
                ESTIMATED_COST REAL NULL,
                ACTUAL_COST REAL NULL,
                STATUS TEXT NULL,
                APPROVAL_DATE TEXT NULL,
                DESCRIPTION TEXT NULL,
                ROW_ID TEXT NULL,
                OWNER_TENANT_ID TEXT NULL,
                OWNER_COMPANY_BA_ID TEXT NULL,
                WORK_ORDER_ID TEXT NULL,
                BOOK_ID TEXT NULL,
                CURRENCY_CODE TEXT NULL,
                WORK_ORDER_BUDGET_JSON TEXT NULL,
                WORK_ORDER_BUDGET_REVISION TEXT NULL,
                FDP_BASELINE_ID TEXT NULL,
                DEVELOPMENT_BUDGET_JSON TEXT NULL,
                DEVELOPMENT_BUDGET_REVISION TEXT NULL,
                AFE_KIND TEXT NULL,
                ENTITY_ID TEXT NULL,
                AUTHOR TEXT NULL,
                APPROVER TEXT NULL,
                APPROVED_AMOUNT REAL NULL,
                REVISION_NO INTEGER NOT NULL,
                SUPERSEDES_AFE_ID TEXT NULL,
                ROOT_AFE_ID TEXT NULL,
                REVISION_REASON TEXT NULL,
                CLOSED_AT TEXT NULL,
                CLOSED_BY TEXT NULL,
                CLOSE_NOTE TEXT NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                ROW_EFFECTIVE_DATE TEXT NULL,
                ROW_EXPIRY_DATE TEXT NULL,
                ROW_QUALITY TEXT NULL,
                PPDM_GUID TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                REMARK TEXT NULL,
                SOURCE TEXT NULL);
            """);

    private static void CreateCommitmentTable(string database) =>
        Execute(database,
            """
            CREATE TABLE AFE_COMMITMENT (
                AFE_COMMITMENT_ID TEXT NOT NULL PRIMARY KEY,
                AFE_ID TEXT NOT NULL,
                REFERENCE TEXT NULL,
                AMOUNT REAL NOT NULL,
                CURRENCY_CODE TEXT NULL,
                COMMITTED_AT TEXT NULL,
                ACTOR TEXT NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CREATED_BY TEXT NULL,
                ROW_CREATED_DATE TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL,
                ROW_EFFECTIVE_DATE TEXT NULL,
                ROW_EXPIRY_DATE TEXT NULL,
                ROW_QUALITY TEXT NULL,
                PPDM_GUID TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                REMARK TEXT NULL,
                SOURCE TEXT NULL);
            """);

    /// <summary>Every column a later write could disturb, as one string, so a changed byte shows up.</summary>
    private static string Snapshot(string database, string afeId) =>
        Query(database,
            "SELECT COALESCE(AFE_NUMBER,'') || '|' || COALESCE(AFE_NAME,'') || '|' || " +
            "COALESCE(CAST(ESTIMATED_COST AS TEXT),'') || '|' || COALESCE(STATUS,'') || '|' || " +
            "COALESCE(APPROVER,'') || '|' || COALESCE(CAST(APPROVAL_DATE AS TEXT),'') || '|' || " +
            "COALESCE(CAST(APPROVED_AMOUNT AS TEXT),'') || '|' || COALESCE(CAST(REVISION_NO AS TEXT),'') || '|' || " +
            "COALESCE(REVISION_REASON,'') || '|' || COALESCE(CAST(ROW_CHANGED_DATE AS TEXT),'') || '|' || " +
            "COALESCE(CLOSED_AT,'') " +
            "FROM AFE WHERE AFE_ID = '" + afeId + "';");

    private static void Drop(string database) => Execute(database, "DROP TABLE AFE;");

    private static void DropCommitments(string database) => Execute(database, "DROP TABLE AFE_COMMITMENT;");

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static string Query(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return command.ExecuteScalar()?.ToString() ?? string.Empty;
    }

    private static long Scalar(string database, string sql) => Convert.ToInt64(Query(database, sql));

    // ------------------------------------------------------------------ the inputs

    private static OgAfeEntry Entry() =>
        new("Woodford infill AFE", "DRILL", "WELL-1", 1_000m, "USD", "Two wells, 12 months");

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
