using TheTechIdea.Beep.OilandGas.Desktop.Core;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-03 AC-7: the wizard enforces the order of its steps. The rule is pure so it is tested without a window.
/// </summary>
public sealed class SetupStepOrderTests
{
    [Fact]
    public void The_steps_are_the_six_named_stages_in_order()
    {
        Assert.Equal(
            ["Welcome", "Database", "Company", "Modules", "Administrator", "Review"],
            OgSetupSteps.Names);
    }

    [Fact]
    public void A_person_may_go_back_to_any_step_already_reached()
    {
        for (var target = 0; target <= 4; target++)
        {
            Assert.True(OgSetupSteps.MaySelect(furthest: 4, target), $"step {target} should be reachable");
        }
    }

    [Fact]
    public void A_person_may_move_one_step_forward_but_no_further()
    {
        // Reached step 0 (Welcome): Database (1) is offered, Company (2) is not.
        Assert.True(OgSetupSteps.MaySelect(furthest: 0, target: 1));
        Assert.False(OgSetupSteps.MaySelect(furthest: 0, target: 2));

        // Reached step 2: Modules (3) is offered, Administrator (4) is not.
        Assert.True(OgSetupSteps.MaySelect(furthest: 2, target: 3));
        Assert.False(OgSetupSteps.MaySelect(furthest: 2, target: 4));
    }

    [Fact]
    public void Review_can_never_be_reached_by_skipping_a_step()
    {
        for (var furthest = 0; furthest < OgSetupSteps.Count - 2; furthest++)
        {
            Assert.False(
                OgSetupSteps.MaySelect(furthest, OgSetupSteps.Count - 1),
                $"Review must not be reachable from step {furthest}");
        }
    }

    [Fact]
    public void An_index_outside_the_steps_is_refused()
    {
        Assert.False(OgSetupSteps.MaySelect(furthest: 5, target: -1));
        Assert.False(OgSetupSteps.MaySelect(furthest: 5, target: OgSetupSteps.Count));
    }

    [Fact]
    public void A_step_names_itself_for_the_next_button()
    {
        Assert.Equal("Database", OgSetupSteps.NameOf(1));
        Assert.Equal("Review", OgSetupSteps.NameOf(5));
        Assert.Equal(string.Empty, OgSetupSteps.NameOf(6));
    }

    [Fact]
    public void Reached_steps_are_marked_and_unreached_ones_are_not()
    {
        Assert.True(OgSetupSteps.IsReached(furthest: 3, index: 3));
        Assert.True(OgSetupSteps.IsReached(furthest: 3, index: 0));
        Assert.False(OgSetupSteps.IsReached(furthest: 3, index: 4));
    }
}
