using TheTechIdea.Beep.OilandGas.Desktop.Data.Workflow;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WFL-03: the exact-identity rule that refuses ambiguity, the unscheduled due date, the unconfirmed-zone rule and the timeline's
/// ordering - the parts that need no database. The reads over real rows are proven in the gateway test.
/// </summary>
public sealed class ProcessInstanceTests
{
    private const string PageFileName = "InstancesPageView.cs";

    [Fact]
    public void An_instance_is_selected_by_its_exact_identity_and_ambiguity_is_refused()
    {
        // AC-1 and SG-14-2: the newest is never preferred, and two rows with one identity are a refusal.
        var instances = new List<OgProcessInstanceView>
        {
            Instance("PI-1", "WELL", "W-1"),
            Instance("PI-2", "WELL", "W-2"),
            Instance("PI-2", "WELL", "W-3"),
        };

        var (found, code, message) = ProcessInstanceRules.SelectExact(instances, "PI-1");

        Assert.NotNull(found);
        Assert.Equal("PI-1", found!.Id);
        Assert.Empty(code);

        var ambiguous = ProcessInstanceRules.SelectExact(instances, "PI-2");

        Assert.Null(ambiguous.Instance);
        Assert.Equal(ProcessInstanceRules.CodeAmbiguousInstance, ambiguous.Code);
        Assert.Contains("2 instances are recorded as 'PI-2'", ambiguous.Message, StringComparison.Ordinal);
        Assert.Contains("newest is deliberately not preferred", ambiguous.Message, StringComparison.Ordinal);

        var unknown = ProcessInstanceRules.SelectExact(instances, "PI-9");

        Assert.Null(unknown.Instance);
        Assert.Equal(ProcessInstanceRules.CodeUnknownInstance, unknown.Code);

        var noIdentity = ProcessInstanceRules.SelectExact(instances, " ");

        Assert.Null(noIdentity.Instance);
        Assert.Equal(ProcessInstanceRules.CodeIdentityRequired, noIdentity.Code);
    }

    [Fact]
    public void A_subject_with_two_instances_is_refused_naming_both()
    {
        // AC-1 for a document rather than an identity.
        var instances = new List<OgProcessInstanceView>
        {
            Instance("PI-1", "WELL", "W-1"),
            Instance("PI-3", "WELL", "W-1"),
            Instance("PI-4", "FIELD", "W-1"),
        };

        var ambiguous = ProcessInstanceRules.SelectForSubject(instances, "WELL", "W-1");

        Assert.Null(ambiguous.Instance);
        Assert.Equal(ProcessInstanceRules.CodeSubjectAmbiguous, ambiguous.Code);
        Assert.Contains("PI-1", ambiguous.Message, StringComparison.Ordinal);
        Assert.Contains("PI-3", ambiguous.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("PI-4", ambiguous.Message, StringComparison.Ordinal);

        // The kind decides: the same subject id under another kind is a different document.
        var single = ProcessInstanceRules.SelectForSubject(instances, "FIELD", "W-1");

        Assert.Equal("PI-4", single.Instance!.Id);

        Assert.Equal(ProcessInstanceRules.CodeSubjectMissing,
            ProcessInstanceRules.SelectForSubject(instances, "WELL", "W-9").Code);
    }

    [Fact]
    public void A_step_with_no_sla_is_unscheduled_rather_than_overdue()
    {
        // AC-2: the due date is the step's start plus its SLA, and nothing else.
        var start = new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);

        Assert.Equal(start.AddHours(48), ProcessInstanceRules.DueOf(start, 48));
        Assert.Null(ProcessInstanceRules.DueOf(start, null));
        Assert.Null(ProcessInstanceRules.DueOf(null, 48));

        var unscheduled = Instance("PI-1", "WELL", "W-1") with
        {
            DueAt = null,
            SlaState = ProcessDashboardRules.Unscheduled,
        };

        Assert.True(unscheduled.IsUnscheduled);
        Assert.Equal("unscheduled", unscheduled.DueText);

        // A step with no SLA configured is a different absence from a step that has not started, and both are unscheduled.
        var noSla = unscheduled with { SlaState = ProcessDashboardRules.NoSla };

        Assert.True(noSla.IsUnscheduled);
        Assert.Equal("unscheduled", noSla.DueText);

        // A real due date shows, and is not called unscheduled.
        var due = unscheduled with
        {
            DueAt = start.AddHours(48),
            SlaState = ProcessDashboardRules.Breached,
        };

        Assert.False(due.IsUnscheduled);
        Assert.Contains("2026-11-03", due.DueText, StringComparison.Ordinal);
    }

    [Fact]
    public void A_timestamp_without_a_confirmed_zone_is_unconfirmed()
    {
        // AC-3: only a UTC instant carries a confirmed zone.
        Assert.True(ProcessInstanceRules.ZoneConfirmed(new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc)));
        Assert.False(ProcessInstanceRules.ZoneConfirmed(new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Unspecified)));
        Assert.False(ProcessInstanceRules.ZoneConfirmed(new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Local)));
        Assert.False(ProcessInstanceRules.ZoneConfirmed(null));

        var unconfirmed = new OgProcessEvent("EVENT", "STATE_CHANGED", "SUBMITTED", "sara",
            new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Unspecified), null, false, true);

        Assert.Contains("(unconfirmed)", unconfirmed.Describe(), StringComparison.Ordinal);

        var confirmed = unconfirmed with { ZoneConfirmed = true };

        Assert.Contains("(confirmed)", confirmed.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_timeline_holds_every_step_and_every_event_in_order()
    {
        // AC-4: neither half may be dropped, and the order is by time.
        var steps = new List<(int, OgProcessEvent)>
        {
            (2, Event("STEP", "Peer review", "completed", "nadia", new DateTime(2026, 11, 3, 9, 0, 0))),
            (1, Event("STEP", "Submit", "completed", "sara", new DateTime(2026, 11, 1, 9, 0, 0))),
        };

        var history = new List<OgProcessEvent>
        {
            Event("EVENT", "STATE_CHANGED", "PEER_REVIEW", "nadia", new DateTime(2026, 11, 2, 9, 0, 0)),
        };

        var timeline = ProcessInstanceRules.Timeline(steps, history);

        Assert.Equal(3, timeline.Count);
        Assert.Equal(["Submit", "STATE_CHANGED", "Peer review"], timeline.Select(entry => entry.StepKey));
        Assert.Equal(timeline.OrderBy(entry => entry.At), timeline);
    }

    [Fact]
    public void Every_event_states_its_actor_or_says_none_is_recorded()
    {
        // AC-5.
        var withActor = Event("STEP", "Submit", "completed", "sara", new DateTime(2026, 11, 1, 9, 0, 0));
        var withoutActor = withActor with { Actor = string.Empty, ActorRecorded = false };

        Assert.Contains("· sara", withActor.Describe(), StringComparison.Ordinal);
        Assert.Contains("no actor recorded", withoutActor.Describe(), StringComparison.Ordinal);

        var workspace = new OgProcessInstanceWorkspace(
            "F-1",
            [Instance("PI-1", "WELL", "W-1")],
            Instance("PI-1", "WELL", "W-1"),
            [withActor, withoutActor],
            [withActor],
            null,
            [],
            DateTime.UtcNow,
            DateTime.UtcNow);

        Assert.Equal(1, workspace.WithoutActor);
        Assert.Contains("1 without an actor", workspace.CountsText, StringComparison.Ordinal);
    }

    [Fact]
    public void The_page_opens_by_identity_and_shows_the_subject_warning()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-1: the row and the typed identity both go through the same identity path.
        Assert.Contains("OpenByIdentity", page, StringComparison.Ordinal);
        Assert.Contains("Read(selected.Id, null, null)", page, StringComparison.Ordinal);

        // AC-6: the subject text is the workspace's, so its warning cannot be lost.
        Assert.Contains("workspace.SubjectText", page, StringComparison.Ordinal);
        Assert.Contains("workspace.BlocksText", page, StringComparison.Ordinal);

        // A failed read clears the timeline rather than showing an empty one.
        Assert.Contains("_timelineGrid.ItemsSource = null", page, StringComparison.Ordinal);
    }

    private static OgProcessEvent Event(
        string kind,
        string step,
        string outcome,
        string actor,
        DateTime at) =>
        new(kind, step, outcome, actor, at, null, ProcessInstanceRules.ZoneConfirmed(at),
            !string.IsNullOrWhiteSpace(actor));

    private static OgProcessInstanceView Instance(string id, string subjectKind, string subjectId) =>
        new(
            id,
            "PD-1",
            "Drilling approval",
            subjectKind,
            subjectId,
            "IN_PROGRESS",
            "S-2",
            "Peer review",
            "nadia",
            new DateTime(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 11, 3, 8, 0, 0, DateTimeKind.Utc),
            7m,
            ProcessDashboardRules.Breached);
}
