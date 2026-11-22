using TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// SETUP-06 AC-5: the plan names the reference ("code") tables it will seed, so the wizard can show the LOV set for
/// review before anything is applied. The selection is the PPDM <c>R_*</c> convention, taken from module metadata.
/// </summary>
public sealed class ReferenceListTests
{
    [Fact]
    public void Only_the_reference_tables_are_selected()
    {
        var selected = OgSetupReferenceTables.From(
            ["R_WELL_STATUS_TYPE", "WELL", "R_WELL_TYPE", "R_WELL_STATUS_TYPE"]);

        Assert.Equal(["R_WELL_STATUS_TYPE", "R_WELL_TYPE"], selected);
    }

    [Fact]
    public void The_selection_is_case_insensitive_and_deduplicated()
    {
        var selected = OgSetupReferenceTables.From(
            ["r_well_status_type", "R_WELL_STATUS_TYPE", "R_Well_Status_Type"]);

        Assert.Single(selected);
        Assert.Equal("r_well_status_type", selected[0], ignoreCase: true);
    }

    [Fact]
    public void The_selection_ignores_blanks_and_drops_ordinary_tables()
    {
        var selected = OgSetupReferenceTables.From(
            [null, "", "   ", "WELL", "WELLBORE", "R_COUNTRY"]);

        Assert.Equal(["R_COUNTRY"], selected);
    }

    [Fact]
    public void The_selection_is_ordered_so_two_runs_show_the_same_list()
    {
        var first = OgSetupReferenceTables.From(["R_WELL_TYPE", "R_BASIN", "R_COUNTRY"]);
        var second = OgSetupReferenceTables.From(["R_COUNTRY", "R_WELL_TYPE", "R_BASIN"]);

        Assert.Equal(["R_BASIN", "R_COUNTRY", "R_WELL_TYPE"], first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void An_empty_module_names_no_reference_lists_and_never_returns_null()
    {
        var module = new OgSetupModulePlan("WELL", "Well", 1, 12, AlreadyCompleted: false);

        Assert.Empty(module.ReferenceLists);

        var plan = new OgSetupPlan("probe", [module], 12, IsComplete: false);

        Assert.Empty(plan.ReferenceLists);
    }

    [Fact]
    public void The_plan_carries_the_union_of_its_modules_reference_tables()
    {
        var modules = new[]
        {
            new OgSetupModulePlan("A", "A", 1, 3, false, ["R_BASIN"]),
            new OgSetupModulePlan("B", "B", 2, 4, false, ["R_COUNTRY", "R_WELL_TYPE"]),
        };

        var plan = new OgSetupPlan(
            "probe",
            modules,
            7,
            IsComplete: false,
            ["R_BASIN", "R_COUNTRY", "R_WELL_TYPE"]);

        Assert.Equal(["R_BASIN", "R_COUNTRY", "R_WELL_TYPE"], plan.ReferenceLists);
        Assert.All(plan.ReferenceLists, name => Assert.StartsWith("R_", name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_module_row_still_states_its_size_and_state()
    {
        // The reference lists were added to the row; they must not have displaced what the wizard already promised.
        var text = new OgSetupModulePlan("WELL", "Well", 1, 12, false, ["R_BASIN"]).Describe();

        Assert.Equal("Well (WELL) - 12 type(s) - will be applied", text);
    }
}
