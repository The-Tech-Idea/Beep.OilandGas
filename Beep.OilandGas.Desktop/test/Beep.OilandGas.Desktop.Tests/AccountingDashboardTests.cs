using Beep.OilandGas.Accounting.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Accounting;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// ACC-01: the permission gate on the queues, the nullable KPI text, the refresh stamp, the overdue rule, the period state
/// and the queue's own sentences - the parts that need no database. The composition against the seven services and the
/// numbers are proven in the gateway test.
/// </summary>
public sealed class AccountingDashboardTests
{
    private const string PageFileName = "AccountingDashboardPageView.cs";

    [Fact]
    public void A_queue_is_shown_only_to_an_account_that_holds_its_permission()
    {
        // AC-4: an administrator sees every queue.
        var admin = new OgPermissions([OgRoleNames.SystemAdmin], [], permissionSourceAvailable: true);

        Assert.Equal((true, (string?)null), DashboardRules.Access(admin, "accounting.tax.post", "Tax"));

        // A granted permission is enough on its own.
        var granted = new OgPermissions([OgRoleNames.Viewer], ["accounting.tax.post"], true);

        Assert.Equal((true, (string?)null), DashboardRules.Access(granted, "accounting.tax.post", "Tax"));

        // A valid permission set that simply does not hold the right names it.
        var denied = new OgPermissions([OgRoleNames.Viewer], [], true);
        var withheld = DashboardRules.Access(denied, "accounting.tax.post", "Tax");

        Assert.False(withheld.Shown);
        Assert.Contains("accounting.tax.post", withheld.Reason!, StringComparison.Ordinal);
        Assert.Contains("does not hold", withheld.Reason!, StringComparison.Ordinal);

        // AC-4: a failed lookup is unknown, not denied, and the reason says so rather than implying a denial.
        var unknown = DashboardRules.Access(OgPermissions.None, "accounting.tax.post", "Tax");

        Assert.False(unknown.Shown);
        Assert.Contains("unknown", unknown.Reason!, StringComparison.Ordinal);
        Assert.Contains("not denied", unknown.Reason!, StringComparison.Ordinal);

        // No permission set at all is also withheld rather than shown empty.
        Assert.False(DashboardRules.Access(null, "accounting.tax.post", "Tax").Shown);
    }

    [Fact]
    public void A_balance_that_could_not_be_read_prints_unavailable_never_zero()
    {
        // AC-1: null is a different answer from zero and is shown as one.
        var unread = new OgAccountingKpis(
            OpenAp: null,
            OpenAr: null,
            Overdue: null,
            UnpostedJournals: null,
            PeriodState: "open",
            Cash: null,
            Currency: "USD",
            CashOpening: null,
            CashMovement: null,
            Unavailable: ["the sales invoices could not be read"]);

        Assert.Contains("unavailable", unread.OpenArText, StringComparison.Ordinal);
        Assert.Contains("unavailable", unread.OpenApText, StringComparison.Ordinal);
        Assert.Contains("unavailable", unread.OverdueText, StringComparison.Ordinal);
        Assert.Contains("unavailable", unread.UnpostedJournalsText, StringComparison.Ordinal);
        Assert.Contains("unavailable", unread.CashText, StringComparison.Ordinal);
        Assert.Contains("the sales invoices could not be read", unread.OpenArText, StringComparison.Ordinal);
        Assert.DoesNotContain("0.00", unread.OpenArText, StringComparison.Ordinal);
        Assert.True(unread.AnyUnavailable);

        // A real zero is printed as a zero, which is why the two cannot be confused.
        var zero = unread with { OpenAr = 0m, Unavailable = [] };

        Assert.Contains("0.00", zero.OpenArText, StringComparison.Ordinal);
        Assert.DoesNotContain("unavailable", zero.OpenArText, StringComparison.Ordinal);

        // A cash position that could not be read does not break its breakdown into a zero sum either.
        Assert.Contains("unavailable", unread.CashBreakdownText, StringComparison.Ordinal);
        Assert.Contains("opening 5,000.00 + posted movement 250.00 USD",
            (unread with { Cash = 5_250m, CashOpening = 5_000m, CashMovement = 250m }).CashBreakdownText,
            StringComparison.Ordinal);
    }

    [Fact]
    public void The_refresh_stamp_is_always_printable()
    {
        // AC-5: the time is shown whether the read worked or not.
        var refreshed = new DateTime(2026, 11, 8, 9, 30, 15, DateTimeKind.Utc);
        var good = Dashboard(Kpis(), refreshed);

        Assert.Contains("last refreshed", good.RefreshedText, StringComparison.Ordinal);
        Assert.Contains("2026-11-08", good.RefreshedText, StringComparison.Ordinal);
        Assert.DoesNotContain("failed", good.RefreshedText, StringComparison.Ordinal);

        var failed = Dashboard(Kpis(), refreshed, readFailed: true);

        Assert.False(string.IsNullOrWhiteSpace(failed.RefreshedText));
        Assert.Contains("failed", failed.RefreshedText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_queue_says_nothing_awaiting_you_rather_than_showing_a_bare_zero()
    {
        var empty = new OgDashboardQueue(
            "tax-unposted", "Tax computations not posted", "accounting.tax.post", 0, null,
            "computed tax has not reached the ledger");

        Assert.True(empty.IsShown);
        Assert.Equal("0", empty.CountText);
        Assert.Equal("nothing awaiting you", empty.StateText);
        Assert.DoesNotContain("withheld", empty.Describe(), StringComparison.Ordinal);

        var withheld = empty with { Count = null, WithheldReason = "requires accounting.tax.post" };

        Assert.True(withheld.IsWithheld);
        Assert.Equal("(withheld)", withheld.CountText);
        Assert.Equal("requires accounting.tax.post", withheld.StateText);
        Assert.Contains("withheld", withheld.Describe(), StringComparison.Ordinal);

        // A queue whose records could not be read says so on a shown panel, and is not reported as empty.
        var unread = empty with { Count = null, Detail = "the records could not be read" };

        Assert.Equal("the records could not be read", unread.StateText);
        Assert.Equal("(not read)", unread.CountText);
    }

    [Fact]
    public void An_invoice_is_overdue_only_when_it_still_owes_and_the_date_has_passed()
    {
        var today = new DateTime(2026, 11, 8);

        Assert.True(DashboardRules.IsOverdue(100m, new DateTime(2026, 11, 7), today));
        Assert.False(DashboardRules.IsOverdue(0m, new DateTime(2026, 11, 7), today));
        Assert.False(DashboardRules.IsOverdue(100m, today, today));
        Assert.False(DashboardRules.IsOverdue(100m, new DateTime(2026, 11, 9), today));
        Assert.False(DashboardRules.IsOverdue(100m, null, today));
    }

    [Fact]
    public void The_period_state_is_read_from_the_ledgers_own_states()
    {
        Assert.Equal("open", DashboardRules.PeriodStateOf(LedgerCodes.PeriodOpen));
        Assert.Equal("closed", DashboardRules.PeriodStateOf(LedgerCodes.PeriodClosed));
        Assert.Equal("locked", DashboardRules.PeriodStateOf(LedgerCodes.PeriodLocked));
        Assert.Equal("not opened", DashboardRules.PeriodStateOf(null));
        Assert.Equal("not opened", DashboardRules.PeriodStateOf("  "));

        // An unfamiliar value is shown as it is rather than forced into a known state.
        Assert.Equal("suspended", DashboardRules.PeriodStateOf("SUSPENDED"));

        // The cash position comes from the configured chart's own account, not a literal on the screen.
        Assert.Equal(DefaultGlAccounts.Cash, DashboardRules.CashAccount);
    }

    [Fact]
    public void The_period_strip_is_the_sessions_period_and_says_when_it_blocks_the_close()
    {
        var period = new OgDashboardPeriod(
            "2026-11", "November 2026", "open", 2, true,
            "November 2026 (2026-11) is open · 2 draft journal(s) block its close");

        Assert.Contains("November 2026", period.Describe(), StringComparison.Ordinal);
        Assert.Contains("2 draft journal(s)", period.Describe(), StringComparison.Ordinal);

        var missing = new OgDashboardPeriod(
            "2026-12", "December 2026", "not opened", 0, false,
            "December 2026 (2026-12): no ledger period exists for it");

        Assert.False(missing.Found);
        Assert.Contains("no ledger period exists", missing.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_workspace_separates_shown_and_withheld_queues()
    {
        var dashboard = Dashboard(Kpis(), DateTime.UtcNow, queues:
        [
            new OgDashboardQueue("a", "Shown", "p.a", 3, null, "detail"),
            new OgDashboardQueue("b", "Hidden", "p.b", null, "requires p.b, which your account does not hold", "detail"),
        ]);

        Assert.Single(dashboard.Shown);
        Assert.Single(dashboard.Withheld);
        Assert.Equal("Hidden", dashboard.Withheld[0].Title);
        Assert.Contains("Shown", dashboard.QueuesText, StringComparison.Ordinal);
        Assert.Contains("Hidden", dashboard.QueuesText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_prints_the_refresh_stamp_and_the_withheld_reason_and_infers_no_zero()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-5 and AC-4: the stamp and the reason are bound from the workspace.
        Assert.Contains("RefreshedText", page, StringComparison.Ordinal);
        Assert.Contains("WithheldReason", page, StringComparison.Ordinal);

        // AC-1: the balances come from the KPI text, and the failed branch clears them rather than printing zeros.
        Assert.Contains("Kpis.OpenArText", page, StringComparison.Ordinal);
        Assert.Contains("Kpis.OpenApText", page, StringComparison.Ordinal);
        Assert.Contains("Kpis.UnpostedJournalsText", page, StringComparison.Ordinal);
        Assert.Contains("_balances.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.DoesNotContain("\"0.00\"", page, StringComparison.Ordinal);

        // The period strip is the session's period state, not a literal.
        Assert.Contains("dashboard.Period.Describe", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ helpers

    private static OgAccountingKpis Kpis() =>
        new(4_000m, 2_500m, 1_000m, 3, "open", 5_250m, "USD", 5_000m, 250m, []);

    private static OgAccountingDashboard Dashboard(
        OgAccountingKpis kpis,
        DateTime refreshedAt,
        bool readFailed = false,
        IReadOnlyList<OgDashboardQueue>? queues = null) =>
        new(
            kpis,
            queues ?? [],
            [],
            new OgDashboardPeriod("2026-11", "November 2026", "open", 0, true,
                "November 2026 (2026-11) is open"),
            refreshedAt,
            readFailed,
            []);
}
