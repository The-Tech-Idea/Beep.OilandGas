using System.Globalization;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>What kind of control a generated field needs.</summary>
public enum OgFormFieldKind
{
    Text,
    Integer,
    Decimal,
    Date,
    Boolean,
}

/// <summary>One field of a generated form.</summary>
/// <param name="Column">The database column.</param>
/// <param name="Label">What the person sees; defaults to a humanised column name.</param>
/// <param name="Kind">The control and the parse rule.</param>
/// <param name="IsKey">Part of the key; never editable, and required on insert.</param>
/// <param name="Required">The column does not allow NULL.</param>
/// <param name="MaxLength">Text length limit, when the schema records one.</param>
/// <param name="Scale">Decimal places, when the schema records them.</param>
/// <param name="ReadOnly">True for keys and audit/system columns.</param>
/// <param name="Note">Why a field is read-only, for the tooltip.</param>
public sealed record OgFormField(
    string Column,
    string Label,
    OgFormFieldKind Kind,
    bool IsKey,
    bool Required,
    int? MaxLength = null,
    int? Scale = null,
    bool ReadOnly = false,
    string? Note = null,
    IReadOnlyList<OgLookupValue>? Lookup = null,
    string? Default = null);

/// <summary>A form generated from one table's structure.</summary>
public sealed record OgFormDefinition(string TableName, IReadOnlyList<OgFormField> Fields)
{
    public IReadOnlyList<string> KeyColumns => Fields.Where(f => f.IsKey).Select(f => f.Column).ToList();

    public IReadOnlyList<OgFormField> EditableFields => Fields.Where(f => !f.ReadOnly).ToList();
}

/// <summary>One validation problem: which field, and a sentence that names it.</summary>
public sealed record OgFormProblem(string Column, string Code, string Message);

/// <summary>
/// DAT-05: builds a form from table metadata and validates a filled-in form before anything is written.
/// </summary>
/// <remarks>
/// Numbers are parsed as <see cref="decimal"/> and dates as <see cref="DateTime"/>, never through
/// <c>double</c>, so a precision that matters to the database is not silently changed on the way in (AC-2).
/// Audit and system columns are present in the schema and never offered for editing (AC-4).
/// </remarks>
public static class OgEntityForm
{
    /// <summary>Columns the database maintains; they are shown as read-only or not at all.</summary>
    private static readonly HashSet<string> AuditColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "ROW_CREATED_BY", "ROW_CREATED_DATE",
        "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY", "PPDM_GUID",
    };

    /// <summary>Builds the form from a table structure plus the key columns the registry records.</summary>
    public static OgFormDefinition Build(
        string tableName,
        IReadOnlyList<OgFormSourceField>? fields,
        IReadOnlyList<string>? keyColumns,
        IReadOnlyDictionary<string, IReadOnlyList<OgLookupValue>>? lookups = null,
        IReadOnlyDictionary<string, string>? defaults = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        var keys = new HashSet<string>(keyColumns ?? [], StringComparer.OrdinalIgnoreCase);
        var result = new List<OgFormField>();

        foreach (var field in fields ?? [])
        {
            if (string.IsNullOrWhiteSpace(field.Name))
            {
                continue;
            }

            var isKey = keys.Contains(field.Name);
            var isAudit = AuditColumns.Contains(field.Name);

            result.Add(new OgFormField(
                Column: field.Name,
                Label: Humanise(field.Name),
                Kind: KindOf(field.ClrType),
                IsKey: isKey,
                Required: !field.AllowNull,
                MaxLength: field.MaxLength,
                Scale: field.Scale,
                ReadOnly: isKey || isAudit,
                Note: isKey ? "Key column: fixed once the row exists."
                    : isAudit ? "Maintained by the database." : null,
                Lookup: !isKey && !isAudit && lookups is not null && lookups.TryGetValue(field.Name, out var values)
                    ? values
                    : null,
                Default: !isKey && !isAudit && defaults is not null && defaults.TryGetValue(field.Name, out var value)
                    ? value
                    : null));
        }

        return new OgFormDefinition(tableName, result);
    }

    /// <summary>
    /// Validates the values as typed. Runs before any gateway call, so a form that cannot be saved is never sent
    /// (AC-1). <paramref name="isNew"/> decides whether keys must be present.
    /// </summary>
    public static IReadOnlyList<OgFormProblem> Validate(
        OgFormDefinition form,
        IReadOnlyDictionary<string, string?> values,
        bool isNew,
        OgFormLanguage language = OgFormLanguage.English)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(values);

        var problems = new List<OgFormProblem>();

        foreach (var field in form.Fields)
        {
            var raw = values.TryGetValue(field.Column, out var value) ? value : null;
            var text = raw?.Trim() ?? string.Empty;

            // A read-only field is never validated as input; a key is required when the row is new.
            if (field.ReadOnly && !(field.IsKey && isNew))
            {
                continue;
            }

            if (text.Length == 0)
            {
                if (field.Required || field.IsKey)
                {
                    problems.Add(new OgFormProblem(
                        field.Column,
                        "required",
                        OgFormMessages.Required(field.Label, language)));
                }

                continue;
            }

            switch (field.Kind)
            {
                case OgFormFieldKind.Integer when !long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out _):
                    problems.Add(new OgFormProblem(field.Column, "not-an-integer", OgFormMessages.NotANumber(field.Label, language)));
                    break;

                case OgFormFieldKind.Decimal:
                    if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                    {
                        problems.Add(new OgFormProblem(field.Column, "not-a-number", OgFormMessages.NotANumber(field.Label, language)));
                        break;
                    }

                    if (field.Scale is { } scale && DecimalsOf(text) > scale)
                    {
                        problems.Add(new OgFormProblem(
                            field.Column,
                            "too-many-decimals",
                            OgFormMessages.TooManyDecimals(field.Label, scale, language)));
                    }

                    _ = number;
                    break;

                case OgFormFieldKind.Date when !DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _):
                    problems.Add(new OgFormProblem(field.Column, "not-a-date", OgFormMessages.NotADate(field.Label, language)));
                    break;

                case OgFormFieldKind.Boolean when !TryParseBoolean(text, out _):
                    problems.Add(new OgFormProblem(field.Column, "not-a-boolean", OgFormMessages.NotABoolean(field.Label, language)));
                    break;
            }

            if (field.MaxLength is { } max && text.Length > max)
            {
                problems.Add(new OgFormProblem(
                    field.Column,
                    "too-long",
                    OgFormMessages.TooLong(field.Label, max, language)));
            }
        }

        return problems;
    }

    /// <summary>
    /// The value as it will be written: a decimal stays a <see cref="decimal"/> (AC-2), a date a
    /// <see cref="DateTime"/>, a boolean a <see cref="bool"/>; everything else stays the text that was typed.
    /// </summary>
    public static object? Convert(OgFormField field, string? text)
    {
        ArgumentNullException.ThrowIfNull(field);

        var trimmed = text?.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return field.Kind switch
        {
            OgFormFieldKind.Integer => long.Parse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture),
            OgFormFieldKind.Decimal => decimal.Parse(trimmed, NumberStyles.Number, CultureInfo.InvariantCulture),
            OgFormFieldKind.Date => DateTime.Parse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.None),
            OgFormFieldKind.Boolean => TryParseBoolean(trimmed, out var flag) ? flag : null,
            OgFormFieldKind.Text => trimmed,
        };
    }

    /// <summary>True when the column is one the database maintains (AC-4).</summary>
    public static bool IsAuditColumn(string column) => AuditColumns.Contains(column);

    private static bool TryParseBoolean(string text, out bool value)
    {
        if (bool.TryParse(text, out value))
        {
            return true;
        }

        if (text is "1" or "0")
        {
            value = text == "1";
            return true;
        }

        if (string.Equals(text, "Y", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "N", StringComparison.OrdinalIgnoreCase))
        {
            value = string.Equals(text, "Y", StringComparison.OrdinalIgnoreCase);
            return true;
        }

        value = false;
        return false;
    }

    private static int DecimalsOf(string text)
    {
        var point = text.IndexOf('.');

        return point < 0 ? 0 : text.Length - point - 1;
    }

    private static OgFormFieldKind KindOf(Type? type)
    {
        var underlying = type is null
            ? typeof(string)
            : Nullable.GetUnderlyingType(type) ?? type;

        if (underlying == typeof(bool))
        {
            return OgFormFieldKind.Boolean;
        }

        if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset))
        {
            return OgFormFieldKind.Date;
        }

        if (underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short)
            || underlying == typeof(byte) || underlying == typeof(uint) || underlying == typeof(ulong))
        {
            return OgFormFieldKind.Integer;
        }

        if (underlying == typeof(decimal) || underlying == typeof(double) || underlying == typeof(float))
        {
            return OgFormFieldKind.Decimal;
        }

        return OgFormFieldKind.Text;
    }

    private static string Humanise(string column)
    {
        var words = column.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.Length == 1 ? word : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant());

        return string.Join(' ', words);
    }
}

/// <summary>One source column, so the form does not depend on a particular metadata type.</summary>
public sealed record OgFormSourceField(
    string Name,
    Type? ClrType,
    bool AllowNull,
    int? MaxLength = null,
    int? Scale = null);

/// <summary>The languages the form's messages exist in.</summary>
public enum OgFormLanguage
{
    English,
    Arabic,
}

/// <summary>DAT-05 AC-6: every validation sentence names the field, in both languages the desktop ships.</summary>
public static class OgFormMessages
{
    public static string Required(string label, OgFormLanguage language) =>
        language == OgFormLanguage.Arabic ? $"حقل {label} مطلوب." : $"{label} is required.";

    public static string NotANumber(string label, OgFormLanguage language) =>
        language == OgFormLanguage.Arabic ? $"حقل {label} يجب أن يكون رقماً." : $"{label} must be a number.";

    public static string NotADate(string label, OgFormLanguage language) =>
        language == OgFormLanguage.Arabic ? $"حقل {label} يجب أن يكون تاريخاً." : $"{label} must be a date.";

    public static string NotABoolean(string label, OgFormLanguage language) =>
        language == OgFormLanguage.Arabic ? $"حقل {label} يجب أن يكون نعم أو لا." : $"{label} must be yes or no.";

    public static string TooLong(string label, int max, OgFormLanguage language) =>
        language == OgFormLanguage.Arabic
            ? $"حقل {label} أطول من {max} حرفاً."
            : $"{label} is longer than {max} characters.";

    public static string TooManyDecimals(string label, int scale, OgFormLanguage language) =>
        language == OgFormLanguage.Arabic
            ? $"حقل {label} يقبل {scale} خانة عشرية فقط."
            : $"{label} accepts {scale} decimal place(s) only.";
}
