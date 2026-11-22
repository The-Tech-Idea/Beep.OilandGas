using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>
/// DAT-05: takes a filled-in generated form, validates it, and only then writes it through the gateway.
/// </summary>
/// <remarks>
/// AC-1 is the point of this class: an invalid form never reaches the gateway, so a mandatory violation cannot
/// become a write attempt. The gateway is asked only after <see cref="OgEntityForm.Validate"/> returns nothing,
/// and its result is passed through unchanged so a conflict or a provider refusal is still visible.
/// </remarks>
public sealed class OgEntityFormService
{
    private readonly IOgPpdmTableGateway _gateway;

    public OgEntityFormService(IOgPpdmTableGateway gateway)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
    }

    /// <summary>How many times the gateway was asked to write; a blocked form leaves this at zero.</summary>
    public int WriteAttempts { get; private set; }

    /// <summary>Validates then inserts.</summary>
    public async Task<OgResult> InsertAsync(
        OgFormDefinition form,
        IReadOnlyDictionary<string, string?> values,
        OgFormLanguage language = OgFormLanguage.English,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(form);

        var problems = OgEntityForm.Validate(form, values, isNew: true, language);

        return problems.Count > 0
            ? Invalid(problems)
            : await WriteAsync(form, values, original: null, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Validates then updates, keeping the original values for the concurrency guard.</summary>
    public async Task<OgResult> UpdateAsync(
        OgFormDefinition form,
        IReadOnlyDictionary<string, string?> values,
        IReadOnlyDictionary<string, object?> original,
        OgFormLanguage language = OgFormLanguage.English,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(original);

        var problems = OgEntityForm.Validate(form, values, isNew: false, language);

        return problems.Count > 0
            ? Invalid(problems)
            : await WriteAsync(form, values, original, cancellationToken).ConfigureAwait(false);
    }

    private async Task<OgResult> WriteAsync(
        OgFormDefinition form,
        IReadOnlyDictionary<string, string?> values,
        IReadOnlyDictionary<string, object?>? original,
        CancellationToken cancellationToken)
    {
        var converted = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var field in form.EditableFields)
        {
            // A read-only field keeps the value it already had; the form never supplies it.
            if (values.TryGetValue(field.Column, out var text))
            {
                converted[field.Column] = OgEntityForm.Convert(field, text);
            }
        }

        foreach (var key in form.KeyColumns)
        {
            if (values.TryGetValue(key, out var text) && !string.IsNullOrWhiteSpace(text))
            {
                converted[key] = text.Trim();
            }
        }

        WriteAttempts++;

        return original is null
            ? await _gateway.InsertAsync(form.TableName, new OgRow(converted), cancellationToken).ConfigureAwait(false)
            : await _gateway.UpdateAsync(form.TableName, new OgRow(converted), new OgRow(original), cancellationToken).ConfigureAwait(false);
    }

    private static OgResult Invalid(IReadOnlyList<OgFormProblem> problems) =>
        OgResult.Failed(
            string.Join("  ·  ", problems.Select(problem => problem.Message)),
            "form-invalid");
}
