using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// LSE-03 against a real SQLite database: the NRI refusal, the 30/60/90 calendar, an overdue obligation cleared
/// by a payment, a renewal that retains the old term, a release of a lost acknowledgement, and a lifecycle event.
/// Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class LeaseFinancialGatewayTests
{
    private const string ConnectionName = "lse03-financials";

    [IntegrationFact]
    public async Task The_financial_register_refuses_unrepresentable_terms_and_renews_without_losing_the_old_term()
    {        var root = Path.Combine(OgPaths.DataRoot, "lse03-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasLse03"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "financials.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "financials.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var financials = new OgLeaseFinancialService(() => source, reporter);
        var asOf = new DateTime(2026, 6, 1);

        // The register: one lease for F-1, expiring in 44 days, with one overdue obligation.
        var loaded = await financials.LoadAsync("F-1", "Alpha field", asOf);

        Assert.True(loaded.Success, loaded.Message);
        var register = loaded.Value!;

        Assert.Single(register.Leases);
        Assert.Equal(OgLeaseFinancialSummary.Expiring, register.Leases[0].ExpiryState);
        Assert.Equal(1, register.OverdueObligations);
        Assert.Equal("L-1", Assert.Single(register.Expiry[1].Leases).Id);

        // The detail: the three rate fields, the burden, the overdue days, the payment and the history.
        var detail = await financials.LoadDetailAsync("L-1", asOf);

        Assert.True(detail.Success, detail.Message);
        var first = detail.Value!;

        Assert.Equal(0.125m, first.Terms.RoyaltyRate);
        Assert.Equal(0.775m, first.Terms.NetRevenueInterest);
        Assert.Equal(0.05m, first.Terms.OverridingRoyalty);
        Assert.Single(first.Terms.Burdens);
        Assert.Equal(2, first.Obligations.Count);
        Assert.Equal(31, first.Obligations.Single(obligation => obligation.Id == "O-1").DaysOverdue);
        Assert.Single(first.Payments);
        Assert.Single(first.TermsHistory);

        // AC-1: a supplied NRI that does not match the deductions is refused and nothing is written.
        var refused = await financials.SaveTermsAsync(
            "L-1",
            new OgLeaseTerms(1000m, "USD", 0.125m, 0.9m, 0.05m, [new OgLeaseBurden("landowner", 0.05m)],
                new DateTime(2025, 1, 1), new DateTime(2026, 7, 15), "edit"),
            "sara");

        Assert.False(refused.Success);
        Assert.Equal(OgLeaseNri.CodeUnrepresentable, refused.Reference);

        // The representable terms are saved as a new active term; the old one is retained.
        var saved = await financials.SaveTermsAsync(
            "L-1",
            new OgLeaseTerms(1200m, "USD", 0.125m, 0.775m, 0.05m, [new OgLeaseBurden("landowner", 0.05m)],
                new DateTime(2026, 1, 1), new DateTime(2026, 7, 15), "indexed"),
            "sara");

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(2, saved.Value!.TermsHistory.Count);
        Assert.Equal(1200m, saved.Value.Terms.Bonus);

        // AC-5: a payment that references the obligation clears the overdue state, which is never stored.
        var paid = await financials.RecordPaymentAsync("L-1", 500m, "USD", "RENTAL", "RENT-1", "O-1", "sara");

        Assert.True(paid.Success, paid.Message);
        Assert.True(paid.Value!.Obligations.Single(obligation => obligation.Id == "O-1").Paid);

        // The write result is computed at today's date; the deterministic assertion re-reads the detail at asOf.
        var afterPayment = await financials.LoadDetailAsync("L-1", asOf);

        Assert.True(afterPayment.Success, afterPayment.Message);
        Assert.Equal(0, afterPayment.Value!.OverdueCount);

        var added = await financials.RecordObligationAsync(
            "L-1", OgLeaseObligation.MinimumSpendType, "Year-3 spend", new DateTime(2026, 8, 1), 2000m, "USD", "sara");

        Assert.True(added.Success, added.Message);
        Assert.Equal(3, added.Value!.Obligations.Count);

        // AC-3: the expiring set is exposed for the task inbox (WFL-05), which is not built in this stage.
        var tasks = await financials.ExpiringTasksAsync(asOf);

        Assert.True(tasks.Success, tasks.Message);
        var task = Assert.Single(tasks.Value!);
        Assert.Equal("L-1", task.LeaseId);
        Assert.Equal("31-60 days", task.Bucket);
        Assert.Equal(OgLeaseExpiryTask.LeaseRoute, task.Route);

        // AC-4: a renewal appends a term and keeps the old one; the acquisition row follows the active term.
        var renewed = await financials.RenewAsync("L-1", new DateTime(2027, 7, 15), "extend by one year", "sara");

        Assert.True(renewed.Success, renewed.Message);
        Assert.Equal(3, renewed.Value!.TermsHistory.Count);
        Assert.Contains(renewed.Value.TermsHistory, term => term.Expiry == new DateTime(2026, 7, 15));
        Assert.Equal(new DateTime(2027, 7, 15), renewed.Value.Terms.Expiry);
        Assert.Contains(renewed.Value.Lifecycle, entry => entry.Event == OgLeaseLifecycleEntry.Renewed);

        using (var connection = new SqliteConnection($"Data Source={database}"))
        {
            connection.Open();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT LEASE_EXPIRATION FROM LEASE_ACQUISITION WHERE LEASE_ACQUISITION_ID = 'L-1';";
            Assert.StartsWith("2027-07-15", (string)command.ExecuteScalar()!);
        }

        // A lost acknowledgement retains the renewal and a retry sends it unchanged.
        ForceTermInsertFailure(database, "L-1");
        var lost = await financials.RenewAsync("L-1", new DateTime(2028, 7, 15), "second extension", "sara");

        Assert.False(lost.Success);
        Assert.NotNull(financials.Pending);
        Assert.Equal("second extension", financials.Pending!.Rationale);
        Assert.Equal(3, TermCount(database, "L-1"));

        DropTermFailureTrigger(database);
        var retried = await financials.RetryAsync(asOf);

        Assert.True(retried.Success, retried.Message);
        Assert.Equal(4, retried.Value!.TermsHistory.Count);
        Assert.Equal(new DateTime(2028, 7, 15), retried.Value.Terms.Expiry);
        Assert.Equal(4, TermCount(database, "L-1"));

        // A lifecycle event is append-only and keeps the terms untouched.
        var relinquished = await financials.RelinquishAsync(
            "L-1", new DateTime(2028, 7, 16), "no further drilling", "sara");

        Assert.True(relinquished.Success, relinquished.Message);
        Assert.Contains(relinquished.Value!.Lifecycle, entry => entry.Event == OgLeaseLifecycleEntry.Relinquished);
        Assert.Equal(4, relinquished.Value.TermsHistory.Count);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE LEASE_ACQUISITION (
                LEASE_ACQUISITION_ID TEXT NOT NULL PRIMARY KEY,
                FIELD_ID TEXT NULL,
                LEASE_NAME TEXT NULL,
                STATE_ID TEXT NULL,
                GROSS_ACREAGE REAL NOT NULL,
                ACREAGE_OUOM TEXT NULL,
                BONUS_AMOUNT REAL NOT NULL,
                BONUS_AMOUNT_OUOM TEXT NULL,
                ROYALTY_RATE REAL NOT NULL,
                NET_REVENUE_INTEREST REAL NOT NULL,
                ACQUISITION_DATE TEXT NULL,
                LEASE_EXPIRATION TEXT NULL,
                LEASE_TYPE TEXT NULL,
                ACQUISITION_STATUS TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE LEASE_TERM (
                TERM_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_ID TEXT NOT NULL,
                TERM_TYPE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                EXPIRY_DATE TEXT NULL,
                BONUS_AMOUNT REAL NOT NULL,
                BONUS_CURRENCY TEXT NULL,
                ROYALTY_RATE REAL NOT NULL,
                NET_REVENUE_INTEREST REAL NOT NULL,
                OVERRIDING_ROYALTY REAL NOT NULL,
                BURDENS_TEXT TEXT NULL,
                RATIONALE TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL,
                ACTIVE_IND TEXT NULL,
                ROW_CHANGED_BY TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE LEASE_OBLIGATION (
                OBLIGATION_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_ID TEXT NOT NULL,
                OBLIGATION_TYPE TEXT NULL,
                DESCRIPTION TEXT NULL,
                DUE_DATE TEXT NULL,
                MINIMUM_SPEND REAL NOT NULL,
                CURRENCY TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL);

            CREATE TABLE LEASE_PAYMENT (
                PAYMENT_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_ID TEXT NOT NULL,
                OBLIGATION_ID TEXT NULL,
                PAID_AT TEXT NULL,
                AMOUNT REAL NOT NULL,
                CURRENCY TEXT NULL,
                KIND TEXT NULL,
                REFERENCE TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL);

            CREATE TABLE LEASE_LIFECYCLE (
                EVENT_ID TEXT NOT NULL PRIMARY KEY,
                LEASE_ID TEXT NOT NULL,
                EVENT TEXT NULL,
                EFFECTIVE_DATE TEXT NULL,
                RATIONALE TEXT NULL,
                ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL);

            INSERT INTO LEASE_ACQUISITION
                (LEASE_ACQUISITION_ID, FIELD_ID, LEASE_NAME, STATE_ID, GROSS_ACREAGE, ACREAGE_OUOM,
                 BONUS_AMOUNT, BONUS_AMOUNT_OUOM, ROYALTY_RATE, NET_REVENUE_INTEREST, ACQUISITION_DATE,
                 LEASE_EXPIRATION, LEASE_TYPE, ACQUISITION_STATUS)
            VALUES
                ('L-1', 'F-1', 'Alpha', 'NORTH', 100, 'acres', 1000, 'USD', 0.125, 0.775, '2025-01-01',
                 '2026-07-15', 'LEASE', 'EXECUTED'),
                ('L-2', 'F-2', 'Bravo', 'SOUTH', 200, 'acres', 2000, 'USD', 0.2, 0.8, '2025-01-01',
                 '2027-01-01', 'LEASE', 'EXECUTED');

            INSERT INTO LEASE_TERM
                (TERM_ID, LEASE_ID, TERM_TYPE, EFFECTIVE_DATE, EXPIRY_DATE, BONUS_AMOUNT, BONUS_CURRENCY,
                 ROYALTY_RATE, NET_REVENUE_INTEREST, OVERRIDING_ROYALTY, BURDENS_TEXT, RATIONALE, ACTOR,
                 RECORDED_AT, ACTIVE_IND)
            VALUES
                ('T-1', 'L-1', 'ORIGINAL', '2025-01-01', '2026-07-15', 1000, 'USD', 0.125, 0.775, 0.05,
                 'landowner=0.05', 'original', 'system', '2025-01-01', 'Y');

            INSERT INTO LEASE_OBLIGATION
                (OBLIGATION_ID, LEASE_ID, OBLIGATION_TYPE, DESCRIPTION, DUE_DATE, MINIMUM_SPEND, CURRENCY, ACTOR, RECORDED_AT)
            VALUES
                ('O-1', 'L-1', 'RENTAL', 'Annual rental', '2026-05-01', 500, 'USD', 'system', '2025-01-01'),
                ('O-2', 'L-1', 'MINIMUM_SPEND', 'Year-2 spend', '2026-07-01', 1000, 'USD', 'system', '2025-01-01');

            INSERT INTO LEASE_PAYMENT
                (PAYMENT_ID, LEASE_ID, OBLIGATION_ID, PAID_AT, AMOUNT, CURRENCY, KIND, REFERENCE, ACTOR, RECORDED_AT)
            VALUES ('P-1', 'L-1', NULL, '2026-02-01', 500, 'USD', 'RENTAL', 'OLD-RENT', 'system', '2026-02-01');
            """;
        command.ExecuteNonQuery();
    }

    private static void ForceTermInsertFailure(string database, string leaseId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TRIGGER BLOCK_TERM BEFORE INSERT ON LEASE_TERM
             WHEN NEW.LEASE_ID = '{leaseId}'
             BEGIN SELECT RAISE(ABORT, 'forced'); END;
             """;
        command.ExecuteNonQuery();
    }

    private static void DropTermFailureTrigger(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DROP TRIGGER BLOCK_TERM;";
        command.ExecuteNonQuery();
    }

    private static long TermCount(string database, string leaseId)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM LEASE_TERM WHERE LEASE_ID = '{leaseId}';";
        return (long)command.ExecuteScalar()!;
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
