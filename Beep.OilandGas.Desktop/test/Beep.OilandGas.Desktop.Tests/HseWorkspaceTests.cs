using System.Text.RegularExpressions;
using Beep.OilandGas.Models.Constants;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PermitsAndApplications.Constants;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// HSE-01: the state classification, the due/overdue/open rules, the link gating, the persona landing and the guard that no
/// hardcoded alert survives - the parts that need no database. The counts over real records are proven in the gateway test.
/// </summary>
public sealed class HseWorkspaceTests
{
    private const string PageFileName = "HseWorkspacePageView.cs";

    [Fact]
    public void Open_incidents_are_the_five_active_states_and_nothing_else()
    {
        // AC-1 and AC-2: the five active states, then closed, cancelled and other as their own rows.
        var states = HseRules.Classify(
        [
            IncidentState.Reported,
            IncidentState.Reported,
            IncidentState.UnderInvestigation,
            IncidentState.PendingRCA,
            IncidentState.PendingCorrectiveAction,
            IncidentState.PendingCloseOut,
            IncidentState.Closed,
            IncidentState.Cancelled,
            "SOMETHING_NEW",
            null,
        ]);

        Assert.Equal(8, states.Count);
        Assert.Equal(6, HseRules.OpenOf(states));
        Assert.Equal(6, states.Where(row => row.Active).Sum(row => row.Count));

        Assert.Equal(2, states.Single(row => row.State == IncidentState.Reported).Count);
        Assert.Equal(1, states.Single(row => row.State == IncidentState.Closed).Count);
        Assert.Equal(1, states.Single(row => row.State == IncidentState.Cancelled).Count);

        // The unrecognised state and the missing one land in the same "other" row, not in open.
        Assert.Equal(2, states.Single(row => row.State == HseRules.OtherState).Count);
        Assert.False(states.Single(row => row.State == HseRules.OtherState).Active);

        // A closed incident is never open, whatever its earlier state history said.
        Assert.False(HseRules.IsActive(IncidentState.Closed));
        Assert.False(HseRules.IsActive(IncidentState.Cancelled));
        Assert.True(HseRules.IsActive(IncidentState.PendingCloseOut));
        Assert.True(HseRules.IsActive(IncidentState.Reported.ToLowerInvariant()));
    }

    [Fact]
    public void An_obligation_is_due_only_when_its_recorded_date_has_arrived()
    {
        var asOf = new DateTime(2026, 11, 8);

        // Unfulfilled and its critical date has arrived (or passed).
        Assert.True(HseRules.IsDue(false, new DateTime(2026, 11, 8), null, asOf));
        Assert.True(HseRules.IsDue(false, new DateTime(2026, 1, 1), null, asOf));

        // Its expiry date is used only when no critical date is recorded.
        Assert.True(HseRules.IsDue(false, null, new DateTime(2026, 11, 1), asOf));
        Assert.False(HseRules.IsDue(false, null, new DateTime(2026, 12, 1), asOf));
        Assert.False(HseRules.IsDue(false, new DateTime(2026, 12, 1), new DateTime(2026, 1, 1), asOf));

        // Fulfilled is never due, and no recorded date means nothing says when it is.
        Assert.False(HseRules.IsDue(true, new DateTime(2026, 1, 1), null, asOf));
        Assert.False(HseRules.IsDue(false, null, null, asOf));
    }

    [Fact]
    public void An_action_is_overdue_when_its_due_date_passed_and_it_is_not_completed()
    {
        var asOf = new DateTime(2026, 11, 8);

        Assert.True(HseRules.IsOverdueAction(new DateTime(2026, 11, 7), HseReferenceCodes.CorrectiveAction.Open, asOf));

        // A step with no status row is open, which is the owner's default.
        Assert.True(HseRules.IsOverdueAction(new DateTime(2026, 11, 7), null, asOf));

        // Completed is not overdue, and a future or absent due date is not overdue.
        Assert.False(HseRules.IsOverdueAction(new DateTime(2026, 11, 7),
            HseReferenceCodes.CorrectiveAction.Completed, asOf));
        Assert.False(HseRules.IsOverdueAction(new DateTime(2026, 12, 7), null, asOf));
        Assert.False(HseRules.IsOverdueAction(null, null, asOf));
    }

    [Fact]
    public void Hazop_and_permit_states_come_from_the_owners_codes()
    {
        // The open HAZOP states are the owner's own OPEN and ACTION_REQUIRED.
        Assert.True(HseRules.IsOpenHazopAction(HseReferenceCodes.Hazop.Open));
        Assert.True(HseRules.IsOpenHazopAction(HseReferenceCodes.Hazop.ActionRequired));
        Assert.False(HseRules.IsOpenHazopAction(HseReferenceCodes.Hazop.Closed));
        Assert.False(HseRules.IsOpenHazopAction(null));

        // A permit is pending while it is submitted, under review or awaiting more information.
        Assert.True(HseRules.IsPendingPermit(PermitApplicationStatusCodes.Submitted));
        Assert.True(HseRules.IsPendingPermit(PermitApplicationStatusCodes.UnderReview));
        Assert.True(HseRules.IsPendingPermit(PermitApplicationStatusCodes.AdditionalInformationRequired));
        Assert.False(HseRules.IsPendingPermit(PermitApplicationStatusCodes.Approved));
        Assert.False(HseRules.IsPendingPermit(PermitApplicationStatusCodes.Draft));
        Assert.False(HseRules.IsPendingPermit(null));
    }

    [Fact]
    public void Each_source_link_is_gated_on_its_own()
    {
        // AC-5: a page this build has is offered only when the shell would open it; a future page names its feature.
        var links = OgHseWorkspace.LinksFor(route => route == "wells");

        var wells = links.Single(link => link.Route == "wells");
        var tasks = links.Single(link => link.Route == "tasks");
        var incidents = links.Single(link => link.Route == "incidents");
        var hazop = links.Single(link => link.Route == "hazop");
        var compliance = links.Single(link => link.Route == "compliance");
        var ghg = links.Single(link => link.Route == "ghg");
        var permits = links.Single(link => link.Route == "permits");

        Assert.True(wells.Available);
        Assert.Null(wells.WhyNot);

        // A page this build has is refused by the account, not by its absence.
        Assert.False(tasks.Available);
        Assert.Contains("may not open", tasks.WhyNot!, StringComparison.Ordinal);
        Assert.False(incidents.Available);
        Assert.Contains("may not open", incidents.WhyNot!, StringComparison.Ordinal);

        // HAZOP is a real page now, so it is refused by the account like the others.
        Assert.False(hazop.Available);
        Assert.Contains("may not open", hazop.WhyNot!, StringComparison.Ordinal);

        // Every HSE page HSE-01 links to now exists, so each is refused by the account rather than by its absence.
        foreach (var link in new[] { compliance, ghg, permits })
        {
            Assert.False(link.Available);
            Assert.Contains("may not open", link.WhyNot!, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task A_failed_read_returns_no_counts_at_all()
    {
        // AC-3: the failure path has no snapshot, so the page cannot show a zero for it.
        var workspace = new OgHseWorkspace(
            () => null,
            new Inbox(),
            new Quiet());

        var loaded = await workspace.LoadAsync(
            null, "Field", null, "HSE_OFFICER", [OgRoleNames.HseOfficer],
            new DateTime(2026, 11, 8), "UTC", _ => true);

        Assert.False(loaded.Success);
        Assert.Null(loaded.Value);
        Assert.Contains("No connection", loaded.Message!, StringComparison.Ordinal);
    }

    /// <summary>
    /// AC-6: the HSE officer's desktop route is the RBAC seed's <c>DEFAULT_LANDING_ROUTE</c>. If either side moves, this fails.
    /// </summary>
    [Fact]
    public void The_persona_route_matches_the_rbac_default_landing_route()
    {
        var seed = Path.Combine(
            RepositoryRoot(), "Beep.OilandGas.UserManagement", "Services", "DefaultSecuritySeedService.cs");

        Assert.True(File.Exists(seed), seed);

        var text = File.ReadAllText(seed);
        var match = Regex.Match(text, "Code\\s*=\\s*\"HSE_OFFICER\".*?Route\\s*=\\s*\"([^\"]+)\"");

        Assert.True(match.Success, "The HSE officer's RBAC route was not found in the seed.");

        var rbacRoute = match.Groups[1].Value;
        var persona = Assert.Single(OgPersonaLanding.All, row =>
            string.Equals(row.Role, OgRoleNames.HseOfficer, StringComparison.OrdinalIgnoreCase));

        Assert.Equal("hse", OgPersonaLanding.RouteFor(rbacRoute));
        Assert.Equal(OgPersonaLanding.RouteFor(rbacRoute), persona.Route);
        Assert.Contains(persona.Route, OgModuleCatalog.Pages.Select(page => page.Route));
    }

    [Fact]
    public void The_page_has_no_hardcoded_alert_and_no_zero_default()
    {
        // AC-4: the Web removed hardcoded incident/overdue/permit alerts; the desktop must not reintroduce one.
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // Nothing on the page is a count taken from a literal.
        Assert.DoesNotMatch(new Regex(@"Count\s*=\s*\d"), page);
        Assert.DoesNotMatch(new Regex(@"\b\d+\s+(incidents?|actions?|permits?)\b", RegexOptions.IgnoreCase), page);

        // The counts and the strip come from the workspace.
        Assert.Contains("Counts.Summary", page, StringComparison.Ordinal);
        Assert.Contains("IncidentStates", page, StringComparison.Ordinal);
        Assert.Contains("LinksText", page, StringComparison.Ordinal);
        Assert.Contains("AvailableLinks", page, StringComparison.Ordinal);

        // AC-3: the failed branch clears every figure rather than defaulting any of them.
        Assert.Contains("_counts.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.Contains("_strip.Text = string.Empty", page, StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? AppContext.BaseDirectory;
    }

    private sealed class Inbox : IOgTaskInbox
    {
        public Task<OgResult<OgTaskInboxSnapshot>> LoadAsync(
            string personaCode,
            IReadOnlyCollection<string> roles,
            string? fieldId,
            string fieldName,
            OgTaskTab tab,
            DateTime asOf,
            string timeZoneId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgTaskInboxSnapshot>.Ok(new OgTaskInboxSnapshot(
                personaCode,
                tab,
                [],
                new OgTaskCounts(1, 2, 0, 3, 0, 0),
                asOf)));

        public long BeginLoad() => 1;

        public bool IsCurrent(long version) => version == 1;
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
