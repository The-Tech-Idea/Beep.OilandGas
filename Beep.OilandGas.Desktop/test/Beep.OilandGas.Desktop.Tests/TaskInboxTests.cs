using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Lease;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-05: the tab predicates, the missing and unconfirmed deadline rules, the counts, the broken link and the
/// company-zone display - the parts that do not need a database. The merged process + lease inbox is proven in the
/// gateway test.
/// </summary>
public sealed class TaskInboxTests
{
    private static readonly DateTime AsOf = new(2026, 6, 1);
    private static readonly TimeZoneInfo CompanyZone =
        TimeZoneInfo.CreateCustomTimeZone("CompanyZone", TimeSpan.FromHours(3), "Company", "Company");

    private static readonly IReadOnlySet<string> Leases =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "L-1" };

    [Fact]
    public void A_missing_deadline_is_unscheduled_and_never_overdue()
    {
        var task = Map(Task("T-1", due: null));

        Assert.True(task.Unscheduled);
        Assert.False(task.Overdue);
        Assert.Equal("unscheduled", task.DueText);
        Assert.Contains(task, OgTaskInboxService.Filter([task], OgTaskTab.Unscheduled));
        Assert.Empty(OgTaskInboxService.Filter([task], OgTaskTab.Overdue));
    }

    [Fact]
    public void An_unconfirmed_timestamp_is_not_overdue_and_says_so()
    {
        var task = Map(Task("T-2", AsOf.AddDays(-3), confirmed: "N"));

        Assert.False(task.Overdue);
        Assert.True(task.Unconfirmed);
        Assert.Contains("time zone unconfirmed", task.DueText, StringComparison.Ordinal);
        Assert.Empty(OgTaskInboxService.Filter([task], OgTaskTab.Overdue));
        Assert.Contains(task, OgTaskInboxService.Filter([task], OgTaskTab.All));
    }

    [Fact]
    public void A_confirmed_past_deadline_is_overdue()
    {
        var task = Map(Task("T-3", AsOf.AddDays(-3), confirmed: "Y"));

        Assert.True(task.Overdue);
        Assert.False(task.Unconfirmed);
        Assert.Contains(task, OgTaskInboxService.Filter([task], OgTaskTab.Overdue));
    }

    [Fact]
    public void The_tab_counts_match_the_grid()
    {
        var tasks = new[]
        {
            Map(Task("T-today", AsOf)),
            Map(Task("T-late", AsOf.AddDays(-2))),
            Map(Task("T-unscheduled", null)),
            Map(Task("T-done", AsOf.AddDays(-1), status: "COMPLETED")),
            Map(Task("T-unconfirmed", AsOf.AddDays(-2), confirmed: "N")),
        };

        var counts = OgTaskInboxService.Count(tasks);

        Assert.Equal(1, counts.Today);
        Assert.Equal(1, counts.Overdue);
        Assert.Equal(1, counts.Unscheduled);
        Assert.Equal(5, counts.All);
        Assert.Equal(1, counts.CompletedRecently);
        Assert.Equal(0, counts.Broken);

        Assert.Equal(counts.Today, OgTaskInboxService.Filter(tasks, OgTaskTab.Today).Count);
        Assert.Equal(counts.Overdue, OgTaskInboxService.Filter(tasks, OgTaskTab.Overdue).Count);
        Assert.Equal(counts.Unscheduled, OgTaskInboxService.Filter(tasks, OgTaskTab.Unscheduled).Count);
        Assert.Equal(counts.All, OgTaskInboxService.Filter(tasks, OgTaskTab.All).Count);
        Assert.Equal(
            counts.CompletedRecently,
            OgTaskInboxService.Filter(tasks, OgTaskTab.CompletedRecently).Count);
    }

    [Fact]
    public void A_missing_source_record_is_a_broken_link()
    {
        var found = Map(Task("T-4", AsOf, entityId: "L-1"));
        var missing = Map(Task("T-5", AsOf, entityId: "MISSING"));

        Assert.True(found.SourceExists);
        Assert.Empty(found.BrokenText);

        Assert.False(missing.SourceExists);
        Assert.Contains("not found", missing.BrokenText, StringComparison.Ordinal);
        Assert.Equal(1, OgTaskInboxService.Count([found, missing]).Broken);
    }

    [Fact]
    public void A_newer_load_discards_the_earlier_one()
    {
        var inbox = new OgTaskInboxService(() => null, new StubLeases(), new Quiet());

        var first = inbox.BeginLoad();
        var second = inbox.BeginLoad();

        Assert.False(inbox.IsCurrent(first));
        Assert.True(inbox.IsCurrent(second));
    }

    [Fact]
    public void A_confirmed_deadline_shows_utc_and_the_company_zone()
    {
        var text = OgTaskInboxService.DueText(new DateTime(2026, 6, 1, 9, 0, 0), true, CompanyZone);

        Assert.Contains("2026-06-01 09:00 UTC", text, StringComparison.Ordinal);
        Assert.Contains("2026-06-01 12:00 CompanyZone", text, StringComparison.Ordinal);
    }

    private static OgTask Map(CROSS_PERSONA_TASK task) =>
        OgTaskInboxService.MapTask(task, AsOf, CompanyZone, Leases);

    private static CROSS_PERSONA_TASK Task(
        string id,
        DateTime? due,
        string confirmed = "Y",
        string status = "PENDING",
        string? entityType = "LEASE_ACQUISITION",
        string? entityId = "L-1") =>
        new()
        {
            CROSS_TASK_ID = id,
            PROCESS_INSTANCE_ID = "PI-1",
            TARGET_PERSONA_CODE = "Accountant",
            ASSIGNED_ROLE = "Accountant",
            TASK_TYPE = "REVIEW",
            TASK_STATUS = status,
            DUE_DATE = due,
            DUE_CONFIRMED_IND = confirmed,
            ENTITY_TYPE = entityType,
            ENTITY_ID = entityId,
            ENTITY_DESCRIPTION = "Lease review " + id,
            ROUTE = "leasefinancials",
            ROW_CHANGED_DATE = status == "COMPLETED" ? AsOf.AddDays(-1) : null,
        };

    private sealed class StubLeases : IOgLeaseFinancials
    {
        public OgLeaseRenewal? Pending => null;

        public Task<OgResult<OgLeaseFinancialSnapshot>> LoadAsync(
            string? fieldId, string fieldName, DateTime asOf, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> LoadDetailAsync(
            string leaseId, DateTime asOf, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> SaveTermsAsync(
            string leaseId, OgLeaseTerms terms, string actor, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> RecordObligationAsync(
            string leaseId, string type, string description, DateTime dueDate, decimal minimumSpend,
            string currency, string actor, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> RecordPaymentAsync(
            string leaseId, decimal amount, string currency, string kind, string? reference,
            string? obligationId, string actor, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> RenewAsync(
            string leaseId, DateTime newExpiry, string rationale, string actor,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> RelinquishAsync(
            string leaseId, DateTime effective, string rationale, string actor,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> ExpireAsync(
            string leaseId, DateTime effective, string rationale, string actor,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<IReadOnlyList<OgLeaseExpiryTask>>> ExpiringTasksAsync(
            DateTime asOf, int days = 90, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");

        public Task<OgResult<OgLeaseFinancialDetail>> RetryAsync(
            DateTime asOf, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not used by the unit test.");
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
