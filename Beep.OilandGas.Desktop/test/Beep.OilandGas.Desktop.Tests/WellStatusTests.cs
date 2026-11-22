using Beep.OilandGas.LifeCycle.Services.WellLifecycle;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// WEL-04: the rules that hold without a database - the legal set is the owning project's state machine, an empty
/// reference table is a state, an actor is required and a retry with nothing pending is refused.
/// </summary>
public sealed class WellStatusTests
{
    [Theory]
    [InlineData("PLANNED", "DRILLING")]
    [InlineData("DRILLING", "COMPLETED")]
    [InlineData("PRODUCING", "SUSPENDED")]
    [InlineData("ABANDONED", null)]
    public void The_legal_set_is_the_owning_state_machine(string from, string? expected)
    {
        var view = new OgWellStatusView(
            new OgWellStatusCurrent(from, "Well Status", from, null, null),
            [new OgWellStatusOption("Well Status", "DRILLING", "Drilling", true),
             new OgWellStatusOption("Well Status", "COMPLETED", "Completed", true),
             new OgWellStatusOption("Well Status", "SUSPENDED", "Suspended", true)],
            [],
            []);

        var legal = OgWellStatus.LegalNext(view);

        if (expected is null)
        {
            Assert.Empty(legal);
        }
        else
        {
            Assert.Contains(expected, legal);
        }

        Assert.Equal(
            WellLifecycleService.AllowedTransitions[from].Count,
            WellLifecycleService.AllowedTransitions[from].Distinct().Count());
    }

    [Fact]
    public void An_illegal_choice_lists_the_legal_ones()
    {
        var view = new OgWellStatusView(
            new OgWellStatusCurrent("PLANNED", "Well Status", "Planned", null, null),
            [new OgWellStatusOption("Well Status", "DRILLING", "Drilling", true),
             new OgWellStatusOption("Well Status", "PRODUCING", "Producing", true)],
            [],
            ["DRILLING"]);

        Assert.Contains("DRILLING", view.LegalText!, StringComparison.Ordinal);
        Assert.DoesNotContain("PRODUCING", view.LegalText!, StringComparison.Ordinal);
    }

    [Fact]
    public void An_empty_reference_table_is_its_own_state()
    {
        var view = new OgWellStatusView(null, [], [], []);

        Assert.True(view.ReferenceDataMissing);
    }

    [Fact]
    public async Task A_change_must_record_who_made_it()
    {
        var status = new OgWellStatus(() => null!, new Quiet());

        var changed = await status.ChangeAsync(
            new OgWellStatusChange("UWI-1", "PLANNED", "DRILLING", null, "SPUD", null, " "));

        Assert.False(changed.Success);
        Assert.Equal(OgWellStatus.CodeActorRequired, changed.Reference);
    }

    [Fact]
    public async Task A_retry_with_nothing_pending_is_refused()
    {
        var status = new OgWellStatus(() => null!, new Quiet());

        var retried = await status.RetryAsync();

        Assert.False(retried.Success);
        Assert.Equal(OgWellStatus.CodeNothingPending, retried.Reference);
        Assert.Null(status.Pending);
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
