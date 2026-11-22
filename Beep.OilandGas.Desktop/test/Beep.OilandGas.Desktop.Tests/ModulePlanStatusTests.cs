using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-06 AC-5: the wizard shows each module's status before anything is applied — what it will create, whether it
/// is already in the database, and whether it needs the demonstration-data confirmation.
/// </summary>
public sealed class ModulePlanStatusTests
{
    [Fact]
    public void A_module_to_be_applied_says_so_and_how_many_types_it_brings()
    {
        var text = new OgSetupModulePlan("WELL", "Well", Order: 1, EntityTypeCount: 12, AlreadyCompleted: false)
            .Describe();

        Assert.Equal("Well (WELL) - 12 type(s) - will be applied", text);
    }

    [Fact]
    public void A_module_already_in_the_database_says_already_applied_and_asks_for_no_confirmation()
    {
        var text = new OgSetupModulePlan("WELL", "Well", Order: 1, EntityTypeCount: 12, AlreadyCompleted: true)
            .Describe();

        Assert.Equal("Well (WELL) - 12 type(s) - already applied", text);
        Assert.DoesNotContain("needs confirmation", text, StringComparison.Ordinal);
    }

    [Fact]
    public void A_demonstration_module_says_it_needs_confirmation()
    {
        var text = new OgSetupModulePlan("DEMO_DATA", "Demonstration", Order: 2, EntityTypeCount: 40, AlreadyCompleted: false)
            .Describe();

        Assert.Contains("demonstration data, needs confirmation", text, StringComparison.Ordinal);
    }

    [Fact]
    public void The_row_never_claims_a_module_will_be_applied_when_it_is_demo_data()
    {
        // A demo module that is not confirmed must not read like an ordinary one: the plan is a promise, and the
        // confirmation is what makes it true.
        var text = new OgSetupModulePlan("DEMO_DATA", "Demonstration", Order: 2, EntityTypeCount: 40, AlreadyCompleted: false)
            .Describe();

        Assert.DoesNotContain("will be applied", text, StringComparison.Ordinal);
    }
}
