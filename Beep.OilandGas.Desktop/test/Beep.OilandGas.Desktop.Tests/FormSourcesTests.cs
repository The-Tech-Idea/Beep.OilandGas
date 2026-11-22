using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-05 AC-3 and AC-5: reference pickers list only active codes from the owning service, and defaults come from
/// that service scoped to the table, never from the form itself.
/// </summary>
public sealed class FormSourcesTests
{
    [Fact]
    public async Task Only_active_reference_codes_are_offered()
    {
        var lookups = new OgLovFormLookups((_, _) => Task.FromResult<IReadOnlyList<(string, string, bool)>>(
        [
            ("COMPLETED", "Completed", true),
            ("RETIRED", "Retired", false),
            ("SPUD", "Spudded", true),
        ]));

        var values = await lookups.ValuesAsync("T_WELL", "WELL_STATUS");

        Assert.True(values.Success);
        Assert.Equal(["COMPLETED", "SPUD"], values.Value!.Select(value => value.Code));
        Assert.Equal("Completed", values.Value![0].Label);
        Assert.Equal(1, lookups.Reads);
    }

    [Fact]
    public async Task A_reference_value_with_no_active_code_is_not_offered_and_a_blank_label_falls_back_to_the_code()
    {
        var lookups = new OgLovFormLookups((_, _) => Task.FromResult<IReadOnlyList<(string, string, bool)>>(
        [
            ("", "no code", true),
            ("OK", "", true),
        ]));

        var values = await lookups.ValuesAsync("T_WELL", "WELL_STATUS");

        var only = Assert.Single(values.Value!);
        Assert.Equal("OK", only.Code);
        Assert.Equal("OK", only.Label);
    }

    [Fact]
    public async Task Defaults_are_scoped_to_the_table_that_asked()
    {
        var defaults = new OgDefaultValueFormDefaults(_ => Task.FromResult<IReadOnlyDictionary<string, string>>(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["T_WELL.WELL_STATUS"] = "SPUD",
                ["T_WELL.ACTIVE_IND"] = "Y",
                ["T_OTHER.COUNTRY"] = "SA",
            }));

        var values = await defaults.ValuesAsync("T_WELL");

        Assert.True(values.Success);
        Assert.Equal(2, values.Value!.Count);
        Assert.Equal("SPUD", values.Value["WELL_STATUS"]);
        Assert.Equal("Y", values.Value["ACTIVE_IND"]);
        Assert.False(values.Value.ContainsKey("COUNTRY"));
    }

    [Fact]
    public void A_generated_field_carries_its_picker_and_its_default()
    {
        var form = OgEntityForm.Build(
            "T_WELL",
            [
                new("WELL_ID", typeof(string), AllowNull: false),
                new("WELL_STATUS", typeof(string), AllowNull: false),
                new("ACTIVE_IND", typeof(string), AllowNull: true),
                new("ROW_CHANGED_BY", typeof(string), AllowNull: true),
            ],
            ["WELL_ID"],
            lookups: new Dictionary<string, IReadOnlyList<OgLookupValue>>(StringComparer.OrdinalIgnoreCase)
            {
                ["WELL_STATUS"] = [new OgLookupValue("SPUD", "Spudded"), new OgLookupValue("COMPLETED", "Completed")],
            },
            defaults: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["WELL_STATUS"] = "SPUD",
                ["ACTIVE_IND"] = "Y",
                ["ROW_CHANGED_BY"] = "SYSTEM",
            });

        var status = Assert.Single(form.Fields, f => f.Column == "WELL_STATUS");

        // AC-3
        Assert.Equal(2, status.Lookup!.Count);
        Assert.Equal("SPUD", status.Default);

        // AC-5: a default reaches an editable field, but never a key or an audit column.
        Assert.Equal("Y", Assert.Single(form.Fields, f => f.Column == "ACTIVE_IND").Default);
        Assert.Null(Assert.Single(form.Fields, f => f.Column == "WELL_ID").Default);
        Assert.Null(Assert.Single(form.Fields, f => f.Column == "ROW_CHANGED_BY").Default);
        Assert.Null(Assert.Single(form.Fields, f => f.Column == "WELL_ID").Lookup);
    }

    [Fact]
    public void A_field_with_no_reference_set_has_no_picker()
    {
        var form = OgEntityForm.Build(
            "T_WELL",
            [new("WELL_NAME", typeof(string), AllowNull: true)],
            []);

        Assert.Null(Assert.Single(form.Fields).Lookup);
        Assert.Null(Assert.Single(form.Fields).Default);
    }
}
