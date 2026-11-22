using Microsoft.Data.Sqlite;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-05 against a real SQLite database: the persona filter, the overdue/unscheduled/unconfirmed rules, the tab
/// counts, the broken source link, the UTC/company-zone text, and the LSE-03 expiring-lease task consumed by the
/// inbox. Opt-in (<c>OG_RUN_SETUP_INTEGRATION=1</c>).
/// </summary>
public sealed class TaskInboxGatewayTests
{
    private const string ConnectionName = "wfl05-inbox";

    [IntegrationFact]
    public async Task The_inbox_merges_process_and_lease_tasks_and_keeps_the_counts_honest()
    {        var root = Path.Combine(OgPaths.DataRoot, "wfl05-" + Guid.NewGuid().ToString("n")[..8]);
        var reporter = new Recording();

        using var host = OgBeepHost.Create(new OgBeepHostOptions(Path.Combine(root, "Beep"), "BeepOilGasWfl05"));
        OgDriverDiscovery.LoadFromAssembly(host.Beep, typeof(SQLiteDataSource).Assembly);

        var database = Path.Combine(root, "Data", "inbox.db");
        var catalog = new BeepDmConnectionCatalog(host, reporter);
        Assert.True((await catalog.EnsureLocalAsync(
            new OgLocalDatabaseRequest(ConnectionName, Path.Combine(root, "Data"), "inbox.db"))).Success);

        var source = host.Editor.GetDataSource(ConnectionName);
        Assert.NotNull(source);
        Seed(database);

        var leases = new OgLeaseFinancialService(() => source, reporter);
        var inbox = new OgTaskInboxService(() => source, leases, reporter);
        var asOf = new DateTime(2026, 6, 1);

        var loaded = await inbox.LoadAsync(
            "Accountant", ["Accountant"], null, string.Empty, OgTaskTab.All, asOf, "UTC");

        Assert.True(loaded.Success, loaded.Message);
        var all = loaded.Value!;

        // The other persona's task is absent; the four Accountant tasks and the lease-expiry task are present.
        Assert.Equal(5, all.Tasks.Count);
        Assert.DoesNotContain(all.Tasks, task => task.Id == "T-5");

        // AC-1/AC-2/AC-3: one overdue, one unscheduled, one unconfirmed (not overdue), one broken.
        Assert.Equal(1, all.Counts.Overdue);
        Assert.Equal(1, all.Counts.Unscheduled);
        Assert.Equal(1, all.Counts.Broken);
        Assert.Equal(5, all.Counts.All);

        var overdue = await inbox.LoadAsync(
            "Accountant", ["Accountant"], null, string.Empty, OgTaskTab.Overdue, asOf, "UTC");

        Assert.True(overdue.Success, overdue.Message);
        Assert.Equal("T-1", Assert.Single(overdue.Value!.Tasks).Id);
        Assert.Equal(overdue.Value.Counts.Overdue, overdue.Value.Tasks.Count);

        var unscheduled = await inbox.LoadAsync(
            "Accountant", ["Accountant"], null, string.Empty, OgTaskTab.Unscheduled, asOf, "UTC");

        Assert.Equal("T-2", Assert.Single(unscheduled.Value!.Tasks).Id);

        // The unconfirmed task is in All with the warning and never in Overdue.
        var unconfirmed = all.Tasks.Single(task => task.Id == "T-3");

        Assert.False(unconfirmed.Overdue);
        Assert.True(unconfirmed.Unconfirmed);
        Assert.Contains("time zone unconfirmed", unconfirmed.DueText, StringComparison.Ordinal);

        // AC-4: a process task pointing at a lease that does not exist is a broken link.
        var broken = all.Tasks.Single(task => task.Id == "T-4");

        Assert.False(broken.SourceExists);
        Assert.Contains("not found", broken.BrokenText, StringComparison.Ordinal);

        // AC-6: a confirmed deadline shows UTC.
        Assert.Contains("UTC", all.Tasks.Single(task => task.Id == "T-1").DueText, StringComparison.Ordinal);

        // LSE-03 AC-3: the expiring lease is consumed here as a task with the lease route.
        var expiry = Assert.Single(all.Tasks.Where(task => task.Kind == "LEASE_EXPIRY"));

        Assert.Equal("L-1", expiry.SubjectId);
        Assert.Equal("leasefinancials", expiry.SourceRoute);
        Assert.True(expiry.SourceExists);
    }

    private static void Seed(string database)
    {
        using var connection = new SqliteConnection($"Data Source={database}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE CROSS_PERSONA_TASK (
                CROSS_TASK_ID TEXT NOT NULL PRIMARY KEY,
                PROCESS_INSTANCE_ID TEXT NULL,
                PROCESS_STEP_INSTANCE_ID TEXT NULL,
                TARGET_PERSONA_CODE TEXT NULL,
                ASSIGNED_ROLE TEXT NULL,
                TASK_TYPE TEXT NULL,
                PRIORITY INTEGER NOT NULL,
                TASK_STATUS TEXT NULL,
                DUE_DATE TEXT NULL,
                DUE_CONFIRMED_IND TEXT NULL,
                ENTITY_TYPE TEXT NULL,
                ENTITY_ID TEXT NULL,
                ENTITY_DESCRIPTION TEXT NULL,
                ROUTE TEXT NULL,
                TASK_CONTEXT_JSON TEXT NULL,
                ROW_CHANGED_DATE TEXT NULL);

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
                TERM_ID TEXT NOT NULL PRIMARY KEY, LEASE_ID TEXT NOT NULL, TERM_TYPE TEXT NULL,
                EFFECTIVE_DATE TEXT NULL, EXPIRY_DATE TEXT NULL, BONUS_AMOUNT REAL NOT NULL,
                BONUS_CURRENCY TEXT NULL, ROYALTY_RATE REAL NOT NULL, NET_REVENUE_INTEREST REAL NOT NULL,
                OVERRIDING_ROYALTY REAL NOT NULL, BURDENS_TEXT TEXT NULL, RATIONALE TEXT NULL, ACTOR TEXT NULL,
                RECORDED_AT TEXT NULL, ACTIVE_IND TEXT NULL, ROW_CHANGED_BY TEXT NULL, ROW_CHANGED_DATE TEXT NULL);

            CREATE TABLE LEASE_OBLIGATION (
                OBLIGATION_ID TEXT NOT NULL PRIMARY KEY, LEASE_ID TEXT NOT NULL, OBLIGATION_TYPE TEXT NULL,
                DESCRIPTION TEXT NULL, DUE_DATE TEXT NULL, MINIMUM_SPEND REAL NOT NULL, CURRENCY TEXT NULL,
                ACTOR TEXT NULL, RECORDED_AT TEXT NULL);

            CREATE TABLE LEASE_PAYMENT (
                PAYMENT_ID TEXT NOT NULL PRIMARY KEY, LEASE_ID TEXT NOT NULL, OBLIGATION_ID TEXT NULL,
                PAID_AT TEXT NULL, AMOUNT REAL NOT NULL, CURRENCY TEXT NULL, KIND TEXT NULL, REFERENCE TEXT NULL,
                ACTOR TEXT NULL, RECORDED_AT TEXT NULL);

            CREATE TABLE LEASE_LIFECYCLE (
                EVENT_ID TEXT NOT NULL PRIMARY KEY, LEASE_ID TEXT NOT NULL, EVENT TEXT NULL,
                EFFECTIVE_DATE TEXT NULL, RATIONALE TEXT NULL, ACTOR TEXT NULL, RECORDED_AT TEXT NULL);

            INSERT INTO LEASE_ACQUISITION
                (LEASE_ACQUISITION_ID, FIELD_ID, LEASE_NAME, STATE_ID, GROSS_ACREAGE, ACREAGE_OUOM,
                 BONUS_AMOUNT, BONUS_AMOUNT_OUOM, ROYALTY_RATE, NET_REVENUE_INTEREST, ACQUISITION_DATE,
                 LEASE_EXPIRATION, LEASE_TYPE, ACQUISITION_STATUS)
            VALUES ('L-1', 'F-1', 'Alpha', 'NORTH', 100, 'acres', 1000, 'USD', 0.125, 0.775, '2025-01-01',
                    '2026-07-15', 'LEASE', 'EXECUTED');

            INSERT INTO CROSS_PERSONA_TASK
                (CROSS_TASK_ID, PROCESS_INSTANCE_ID, TARGET_PERSONA_CODE, ASSIGNED_ROLE, TASK_TYPE, PRIORITY,
                 TASK_STATUS, DUE_DATE, DUE_CONFIRMED_IND, ENTITY_TYPE, ENTITY_ID, ENTITY_DESCRIPTION, ROUTE,
                 ROW_CHANGED_DATE)
            VALUES
                ('T-1', 'PI-1', 'Accountant', 'Accountant', 'REVIEW', 2, 'PENDING', '2026-05-30', 'Y',
                 'LEASE_ACQUISITION', 'L-1', 'Review the lease payment', 'leasefinancials', NULL),
                ('T-2', 'PI-1', 'Accountant', 'Accountant', 'REVIEW', 3, 'PENDING', NULL, 'N',
                 'LEASE_ACQUISITION', 'L-1', 'No deadline yet', 'leasefinancials', NULL),
                ('T-3', 'PI-1', 'Accountant', 'Accountant', 'REVIEW', 3, 'PENDING', '2026-05-30', 'N',
                 'LEASE_ACQUISITION', 'L-1', 'Zone not confirmed', 'leasefinancials', NULL),
                ('T-4', 'PI-1', 'Accountant', 'Accountant', 'REVIEW', 3, 'PENDING', '2026-07-01', 'Y',
                 'LEASE_ACQUISITION', 'MISSING', 'Source removed', 'leasefinancials', NULL),
                ('T-5', 'PI-2', 'Driller', 'Driller', 'REVIEW', 3, 'PENDING', '2026-05-30', 'Y',
                 'LEASE_ACQUISITION', 'L-1', 'Another persona', 'leasefinancials', NULL);
            """;
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
