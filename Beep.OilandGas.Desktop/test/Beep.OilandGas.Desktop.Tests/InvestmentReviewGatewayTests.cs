using Beep.OilandGas.LifeCycle.Constants;
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
/// ECO-03 against a real SQLite database: the queue, a decision that records actor, date, rationale and conditions
/// (AC-1, SG-09-5), the author refused (AC-3), stale economics refused until re-reviewed (AC-2), the concurrent loser
/// refused (AC-3's neighbour), and a handoff that creates exactly one AFE carrying the conditions (AC-4, AC-5) and does
/// not duplicate it after a lost acknowledgement (AC-6). Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class InvestmentReviewGatewayTests
{
    private const string ConnectionName = "eco03-investment";

    [IntegrationFact]
    public async Task The_queue_decides_and_hands_off_exactly_one_afe()
    {        var root = Path.Combine(OgPaths.DataRoot, "eco03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasEco03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "investment.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "investment.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var service = new OgInvestmentReviewService(() => source, reporter);

        // ---- the queue -------------------------------------------------------
        var loaded = await service.LoadAsync();

        Assert.True(loaded.Success, loaded.Message);
        Assert.Empty(loaded.Value!.Queue);
        Assert.False(loaded.Value.HistoryFailed);
        Assert.Contains("APPROVED", loaded.Value.Decisions);

        // ---- submit ----------------------------------------------------------
        var submitted = await service.SubmitAsync(Entry(), "sara");

        Assert.True(submitted.Success, submitted.Message + " :: " + string.Join(" | ", reporter.Failures));

        var proposal = submitted.Value!.Selected!;

        Assert.Equal(InvestmentDecisionCodes.Queued, proposal.State);
        Assert.True(proposal.IsQueued);
        Assert.Equal("sara", proposal.SubmittedBy);
        Assert.Equal(2_000_000m, proposal.Amount);
        Assert.False(proposal.EconomicsStale);
        Assert.True(proposal.HasEconomics);

        // ---- the economics-missing state -------------------------------------
        var noEconomics = await service.SubmitAsync(Entry() with { EconomicsId = null, EconomicsNpv = null }, "sara");

        Assert.True(noEconomics.Success, noEconomics.Message);

        var bare = noEconomics.Value!.Selected!;

        Assert.False(bare.HasEconomics);

        var refusedForEconomics = await service.DecideAsync(
            bare.Id, "APPROVED", "looks fine", string.Empty, "omar");

        Assert.False(refusedForEconomics.Success);
        Assert.Equal(InvestmentReviewCodes.EconomicsMissing, refusedForEconomics.Reference);
        Assert.Contains("ECO-01", refusedForEconomics.Message!, StringComparison.Ordinal);

        // ---- AC-3: the submitter cannot decide -------------------------------
        var byAuthor = await service.DecideAsync(proposal.Id, "APPROVED", "my own proposal", string.Empty, "sara");

        Assert.False(byAuthor.Success);
        Assert.Equal(InvestmentReviewCodes.AuthorCannotDecide, byAuthor.Reference);
        Assert.Contains("sara", byAuthor.Message!, StringComparison.Ordinal);

        // ---- AC-2: an amount changed after review is stale --------------------
        var amended = await service.AmendAsync(proposal.Id, 3_000_000m, "sara");

        Assert.True(amended.Success, amended.Message);
        Assert.Equal(3_000_000m, amended.Value!.Selected!.Amount);
        Assert.True(amended.Value.Selected.EconomicsStale);

        var stale = await service.DecideAsync(proposal.Id, "CONDITIONAL", "approve", "audit first", "omar");

        Assert.False(stale.Success);
        Assert.Equal(InvestmentReviewCodes.StaleEconomics, stale.Reference);
        Assert.Contains("2,000,000.00", stale.Message!, StringComparison.Ordinal);

        // The re-review clears it, and the decision then goes through.
        Assert.True((await service.ReviewAsync(proposal.Id, "omar")).Success);

        // ---- AC-1 and SG-09-5: actor, date, rationale and conditions ----------
        var decided = await service.DecideAsync(
            proposal.Id,
            "CONDITIONAL",
            "Best value at 2.1 years payback; the water-disposal risk is manageable",
            "Third-party audit before spud\nMonitor overburden pressure monthly",
            "omar");

        Assert.True(decided.Success, decided.Message);

        var decidedProposal = decided.Value!.Selected!;

        Assert.Equal(InvestmentDecisionCodes.Decided, decidedProposal.State);
        Assert.Equal(InvestmentDecisionCodes.Conditional, decidedProposal.Decision);
        Assert.Equal("omar", decidedProposal.DecidedBy);
        Assert.NotNull(decidedProposal.DecidedAt);
        Assert.Contains("Best value at 2.1 years payback", decidedProposal.RationaleText, StringComparison.Ordinal);
        Assert.Equal(
            ["Third-party audit before spud", "Monitor overburden pressure monthly"],
            decidedProposal.Conditions);
        Assert.Equal(3_000_000m, decidedProposal.DecisionAmount);

        // The row itself: STATUS, DECISION, APPROVED_BY, APPROVAL_DATE, RATIONALE, CONDITIONS_TEXT, DECISION_AMOUNT.
        foreach (var (predicate, label) in new[]
                 {
                     ("STATUS = 'DECIDED'", "queue state"),
                     ("DECISION = 'CONDITIONAL'", "decision"),
                     ("APPROVED_BY = 'omar'", "decider"),
                     ("APPROVAL_DATE IS NOT NULL", "decision date"),
                     ("RATIONALE LIKE 'Best value%'", "rationale"),
                     ("CONDITIONS_TEXT LIKE '%Third-party audit%'", "first condition"),
                     ("CONDITIONS_TEXT LIKE '%Monitor overburden%'", "second condition"),
                     ("DECISION_AMOUNT = 3000000", "amount decided on"),
                     ("REVIEWED_AMOUNT = 3000000", "reviewed amount"),
                 })
        {
            Assert.True(
                Scalar(database,
                    $"SELECT COUNT(*) FROM APPROVAL_WORKFLOW WHERE APPROVAL_WORKFLOW_ID = '{proposal.Id}' AND " +
                    predicate + ";") == 1,
                "the stored proposal does not match on " + label);
        }

        // ---- the concurrent loser is refused and the winner is named ----------
        var second = await service.DecideAsync(proposal.Id, "REJECTED", "too expensive", string.Empty, "nadia");

        Assert.False(second.Success);
        Assert.Equal(InvestmentReviewCodes.AlreadyDecided, second.Reference);
        Assert.Contains("omar", second.Message!, StringComparison.Ordinal);

        // ---- AC-4 and AC-5: one AFE, with the reviewed amount and conditions --
        var handedOff = await service.HandoffAsync(proposal.Id, "omar");

        Assert.True(handedOff.Success, handedOff.Message);

        var handed = handedOff.Value!.Selected!;

        Assert.True(handed.IsHandedOff);
        Assert.False(string.IsNullOrWhiteSpace(handed.AfeId));
        Assert.Equal(1, CountAfes(database));

        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM AFE WHERE SOURCE_PROPOSAL_ID = '" + proposal.Id + "' " +
            "AND ESTIMATED_COST = 3000000 AND APPROVED_AMOUNT = 3000000 AND CURRENCY_CODE = 'USD' " +
            "AND CONDITIONS_TEXT LIKE '%Third-party audit%' AND CONDITIONS_TEXT LIKE '%Monitor overburden%' " +
            "AND STATUS = 'APPROVED' AND APPROVER = 'omar';"));

        // ---- AC-6: a second handoff does not create a second AFE --------------
        var again = await service.HandoffAsync(proposal.Id, "omar");

        Assert.True(again.Success, again.Message);
        Assert.Equal(1, CountAfes(database));

        // ---- AC-6: a lost acknowledgement is recovered, not duplicated --------
        Execute(database,
            "UPDATE APPROVAL_WORKFLOW SET AFE_ID = NULL, HANDOFF_AT = NULL, HANDOFF_BY = NULL, " +
            $"STATUS = '{InvestmentDecisionCodes.Decided}' WHERE APPROVAL_WORKFLOW_ID = '{proposal.Id}';");

        var recovered = await service.HandoffAsync(proposal.Id, "omar");

        Assert.True(recovered.Success, recovered.Message);
        Assert.Equal(1, CountAfes(database));
        Assert.Equal(1, Scalar(database,
            "SELECT COUNT(*) FROM APPROVAL_WORKFLOW WHERE APPROVAL_WORKFLOW_ID = '" + proposal.Id + "' " +
            "AND STATUS = 'HANDED_OFF' AND AFE_ID IS NOT NULL AND HANDOFF_BY = 'omar';"));

        // ---- a returned proposal cannot be handed off -------------------------
        var returned = (await service.SubmitAsync(Entry() with { Title = "Woodford workover" }, "sara"))
            .Value!.Selected!;

        Assert.True((await service.DecideAsync(returned.Id, "RETURNED", "rework the estimate", string.Empty, "omar"))
            .Success);

        var notAuthorised = await service.HandoffAsync(returned.Id, "omar");

        Assert.False(notAuthorised.Success);
        Assert.Equal(InvestmentReviewCodes.NotAuthorised, notAuthorised.Reference);
        Assert.Equal(1, CountAfes(database));

        // ---- the reload keeps the chain ---------------------------------------
        var reloaded = await service.LoadAsync();

        Assert.True(reloaded.Success, reloaded.Message);
        // Three proposals were submitted: the main one, the bare one without economics, and the returned one.
        Assert.Equal(3, reloaded.Value!.Queue.Count);

        var stored = reloaded.Value.Queue.Single(item => item.Id == proposal.Id);

        Assert.True(stored.IsHandedOff);
        Assert.Equal(handed.AfeId, stored.AfeId);
        Assert.Equal(2, stored.Conditions.Count);

        // ---- an unavailable queue is a state ----------------------------------
        Execute(database, "DROP TABLE APPROVAL_WORKFLOW;");
        Execute(database, "DROP TABLE AFE;");

        var unavailable = await service.LoadAsync();

        Assert.True(unavailable.Success, unavailable.Message);
        Assert.True(unavailable.Value!.HistoryFailed);
        Assert.Empty(unavailable.Value.Queue);
        Assert.Contains("could not be read", unavailable.Value.HistoryStateText, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the schema

    private static void Seed(string database)
    {
        Execute(database,
            """
            CREATE TABLE APPROVAL_WORKFLOW (
                APPROVAL_WORKFLOW_ID TEXT NOT NULL PRIMARY KEY,
                ENTITY_NAME TEXT NULL,
                ENTITY_ID TEXT NULL,
                AMOUNT REAL NOT NULL,
                STATUS TEXT NULL,
                REQUESTED_BY TEXT NULL,
                REQUESTED_DATE TEXT NULL,
                APPROVED_BY TEXT NULL,
                APPROVAL_DATE TEXT NULL,
                COMMENTS TEXT NULL,
                CURRENCY_CODE TEXT NULL,
                PROPOSAL_TITLE TEXT NULL,
                INVESTMENT_CLASS TEXT NULL,
                DECISION_DUE TEXT NULL,
                DECISION TEXT NULL,
                RATIONALE TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
                DECISION_AMOUNT REAL NULL,
                ECONOMICS_ID TEXT NULL,
                ECONOMICS_NPV REAL NULL,
                REVIEWED_AMOUNT REAL NULL,
                REVIEWED_AT TEXT NULL,
                RISKS_TEXT TEXT NULL,
                AFE_ID TEXT NULL,
                HANDOFF_AT TEXT NULL,
                HANDOFF_BY TEXT NULL,
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
                SOURCE_PROPOSAL_ID TEXT NULL,
                CONDITIONS_TEXT TEXT NULL,
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
    }

    private static void Execute(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }

    private static long Scalar(string database, string sql)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = sql;

        return Convert.ToInt64(command.ExecuteScalar());
    }

    private static long CountAfes(string database) =>
        Scalar(database, "SELECT COUNT(*) FROM AFE;");

    private static OgInvestmentEntry Entry() =>
        new(
            "Woodford infill programme",
            "WELL-1",
            2_000_000m,
            "USD",
            "DRILL",
            new DateTime(2026, 7, 1),
            "ECO-EVAL-1",
            4_500_000m,
            "Water disposal capacity is the main risk");

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
