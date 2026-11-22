using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// DAT-05: the form is generated from column metadata, keys and audit columns are never editable, decimals keep
/// their precision, a mandatory violation blocks the save before the gateway is called, and every message names
/// its field in both languages.
/// </summary>
public sealed class EntityFormTests
{
    private static OgFormDefinition Form() => OgEntityForm.Build(
        "T_WELL",
        [
            new("WELL_ID", typeof(string), AllowNull: false, MaxLength: 20),
            new("WELL_NAME", typeof(string), AllowNull: false, MaxLength: 60),
            new("SPUD_DATE", typeof(DateTime?), AllowNull: true),
            new("DEPTH_MD", typeof(decimal?), AllowNull: true, Scale: 2),
            new("IS_ACTIVE", typeof(bool?), AllowNull: true),
            new("ROW_CHANGED_BY", typeof(string), AllowNull: true, MaxLength: 30),
            new("ROW_CHANGED_DATE", typeof(DateTime?), AllowNull: true),
        ],
        ["WELL_ID"]);

    [Fact]
    public void The_form_is_generated_from_the_column_metadata()
    {
        var form = Form();

        Assert.Equal("T_WELL", form.TableName);
        Assert.Equal(["WELL_ID"], form.KeyColumns);

        var name = Assert.Single(form.Fields, f => f.Column == "WELL_NAME");
        Assert.Equal("Well Name", name.Label);
        Assert.Equal(OgFormFieldKind.Text, name.Kind);
        Assert.True(name.Required);
        Assert.Equal(60, name.MaxLength);

        Assert.Equal(OgFormFieldKind.Date, Assert.Single(form.Fields, f => f.Column == "SPUD_DATE").Kind);
        Assert.Equal(OgFormFieldKind.Decimal, Assert.Single(form.Fields, f => f.Column == "DEPTH_MD").Kind);
        Assert.Equal(OgFormFieldKind.Boolean, Assert.Single(form.Fields, f => f.Column == "IS_ACTIVE").Kind);
    }

    [Fact]
    public void Keys_and_audit_columns_are_never_editable()
    {
        var form = Form();

        // AC-4
        Assert.True(Assert.Single(form.Fields, f => f.Column == "WELL_ID").ReadOnly);
        Assert.True(Assert.Single(form.Fields, f => f.Column == "ROW_CHANGED_BY").ReadOnly);
        Assert.True(Assert.Single(form.Fields, f => f.Column == "ROW_CHANGED_DATE").ReadOnly);

        var editable = form.EditableFields.Select(f => f.Column).ToList();

        Assert.Equal(["WELL_NAME", "SPUD_DATE", "DEPTH_MD", "IS_ACTIVE"], editable);
        Assert.True(OgEntityForm.IsAuditColumn("ROW_CREATED_DATE"));
        Assert.False(OgEntityForm.IsAuditColumn("WELL_NAME"));
    }

    [Fact]
    public void A_missing_mandatory_field_is_a_problem_that_names_it()
    {
        var problems = OgEntityForm.Validate(
            Form(),
            new Dictionary<string, string?> { ["WELL_ID"] = "W-1", ["WELL_NAME"] = "  " },
            isNew: true);

        var problem = Assert.Single(problems);

        Assert.Equal("WELL_NAME", problem.Column);
        Assert.Equal("required", problem.Code);
        Assert.Contains("Well Name", problem.Message, StringComparison.Ordinal);
        Assert.Contains("required", problem.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Messages_are_localised_and_still_name_the_field()
    {
        var problems = OgEntityForm.Validate(
            Form(),
            new Dictionary<string, string?> { ["WELL_ID"] = "W-1" },
            isNew: true,
            OgFormLanguage.Arabic);

        var problem = Assert.Single(problems);

        Assert.Contains("Well Name", problem.Message, StringComparison.Ordinal);
        Assert.Contains("مطلوب", problem.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Each_kind_is_checked_and_reports_its_own_code()
    {
        var problems = OgEntityForm.Validate(
            Form(),
            new Dictionary<string, string?>
            {
                ["WELL_ID"] = "W-1",
                ["WELL_NAME"] = "Alpha",
                ["SPUD_DATE"] = "not a date",
                ["DEPTH_MD"] = "12.345",
                ["IS_ACTIVE"] = "maybe",
            },
            isNew: true);

        Assert.Contains(problems, p => p.Column == "SPUD_DATE" && p.Code == "not-a-date");
        Assert.Contains(problems, p => p.Column == "DEPTH_MD" && p.Code == "too-many-decimals");
        Assert.Contains(problems, p => p.Column == "IS_ACTIVE" && p.Code == "not-a-boolean");
    }

    [Fact]
    public void A_value_longer_than_the_column_is_refused()
    {
        var problems = OgEntityForm.Validate(
            Form(),
            new Dictionary<string, string?>
            {
                ["WELL_ID"] = "W-1",
                ["WELL_NAME"] = new string('x', 61),
            },
            isNew: true);

        var problem = Assert.Single(problems);

        Assert.Equal("too-long", problem.Code);
        Assert.Contains("60", problem.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void A_decimal_keeps_its_exact_precision_and_is_never_a_double()
    {
        var field = Assert.Single(Form().Fields, f => f.Column == "DEPTH_MD");

        var converted = OgEntityForm.Convert(field, "1234.5678901234567890123");

        // AC-2: the value stays a decimal; a double would have lost the tail of this number.
        var number = Assert.IsType<decimal>(converted);
        Assert.Equal(1234.5678901234567890123m, number);
        Assert.Equal("1234.5678901234567890123", number.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task A_mandatory_violation_blocks_the_save_before_the_gateway_is_called()
    {
        var gateway = new CountingGateway();
        var service = new OgEntityFormService(gateway);

        var saved = await service.InsertAsync(
            Form(),
            new Dictionary<string, string?> { ["WELL_ID"] = "W-1" });

        // AC-1: the form is refused and no write was attempted.
        Assert.False(saved.Success);
        Assert.Equal("form-invalid", saved.Reference);
        Assert.Contains("Well Name", saved.Message!, StringComparison.Ordinal);
        Assert.Equal(0, service.WriteAttempts);
        Assert.Equal(0, gateway.Calls);
    }

    [Fact]
    public async Task A_valid_form_reaches_the_gateway_with_converted_values()
    {
        var gateway = new CountingGateway();
        var service = new OgEntityFormService(gateway);

        var saved = await service.InsertAsync(
            Form(),
            new Dictionary<string, string?>
            {
                ["WELL_ID"] = "W-1",
                ["WELL_NAME"] = "Alpha",
                ["DEPTH_MD"] = "12.34",
                ["SPUD_DATE"] = "2026-01-31",
                ["IS_ACTIVE"] = "Y",
            });

        Assert.True(saved.Success, saved.Message);
        Assert.Equal(1, service.WriteAttempts);
        Assert.NotNull(gateway.LastRow);

        Assert.Equal("W-1", gateway.LastRow!["WELL_ID"]);
        Assert.Equal("Alpha", gateway.LastRow["WELL_NAME"]);
        Assert.Equal(12.34m, gateway.LastRow["DEPTH_MD"]);
        Assert.Equal(new DateTime(2026, 1, 31), gateway.LastRow["SPUD_DATE"]);
        Assert.Equal(true, gateway.LastRow["IS_ACTIVE"]);

        // The audit column was not supplied by the form and must not be written.
        Assert.DoesNotContain("ROW_CHANGED_BY", gateway.LastRow.Keys);
    }

    private sealed class CountingGateway : IOgPpdmTableGateway
    {
        public int Calls { get; private set; }

        public IReadOnlyDictionary<string, object?>? LastRow { get; private set; }

        public Task<OgResult> InsertAsync(string table, OgRow row, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastRow = row.Values;
            return Task.FromResult(OgResult.Ok("inserted"));
        }

        public Task<OgResult> UpdateAsync(string table, OgRow row, OgRow original, CancellationToken cancellationToken = default)
        {
            Calls++;
            LastRow = row.Values;
            return Task.FromResult(OgResult.Ok("updated"));
        }

        public Task<OgResult> DeleteAsync(string table, OgRow row, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult.Ok("deleted"));

        public Task<OgResult<OgPage<OgRow>>> QueryAsync(string table, OgQuery query, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<OgResult<IReadOnlyList<string>>> KeyColumnsAsync(string table, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<IReadOnlyList<string>>.Ok(["WELL_ID"]));

        public Task<OgResult<byte[]>> ExportAsync(string table, OgQuery query, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<OgResult<OgFormDefinition>> FormAsync(string table, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }
}
