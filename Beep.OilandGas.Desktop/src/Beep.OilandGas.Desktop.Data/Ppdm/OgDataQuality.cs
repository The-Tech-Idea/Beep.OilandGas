using System.Globalization;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>
/// How bad a finding is. The numbers are the sort order and are part of the contract: Critical (0) sorts before
/// High (1), then Medium (2), then Low (3), so two runs always list the worst first (AC-5).
/// </summary>
public enum OgQualitySeverity
{
    Critical = 0,
    High = 1,
    Medium = 2,
    Low = 3,
}

/// <summary>What a rule looks at.</summary>
public enum OgQualityRuleKind
{
    Required,
    DuplicateKey,
    ValueTooLong,
    NotANumber,
    NotADate,
    OutOfRange,
}

/// <summary>One check over a table.</summary>
/// <param name="Code">Stable identifier, used in the finding and in the report.</param>
/// <param name="Column">The column checked; null for a table-wide rule such as a duplicate key.</param>
/// <param name="Kind">What is checked.</param>
/// <param name="Severity">How bad a violation is.</param>
/// <param name="Minimum">Lower bound for <see cref="OgQualityRuleKind.OutOfRange"/>.</param>
/// <param name="Maximum">Upper bound for <see cref="OgQualityRuleKind.OutOfRange"/>.</param>
public sealed record OgQualityRule(
    string Code,
    string? Column,
    OgQualityRuleKind Kind,
    OgQualitySeverity Severity,
    decimal? Minimum = null,
    decimal? Maximum = null)
{
    public string Describe() => Kind switch
    {
        OgQualityRuleKind.Required => $"{Column} must have a value",
        OgQualityRuleKind.DuplicateKey => "the key must be unique",
        OgQualityRuleKind.ValueTooLong => $"{Column} must fit the column's length",
        OgQualityRuleKind.NotANumber => $"{Column} must be a number",
        OgQualityRuleKind.NotADate => $"{Column} must be a date",
        OgQualityRuleKind.OutOfRange => $"{Column} must be between {Minimum} and {Maximum}",
    };
}

/// <summary>One defect. Carries the table, the row's key, the column and the rule that found it (AC-1).</summary>
public sealed record OgQualityFinding(
    string Id,
    string Table,
    string Rule,
    string? Column,
    string Key,
    OgQualitySeverity Severity,
    string Message,
    DateTimeOffset FoundAtUtc);

/// <summary>What one run did.</summary>
public sealed record OgQualityRun(
    string Table,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    int RowsScanned,
    int RulesApplied,
    bool Succeeded,
    IReadOnlyList<OgQualityFinding> Findings,
    string? Error = null)
{
    /// <summary>False when no rule applied to this table - different from a rule that found nothing (AC-2).</summary>
    public bool HasRules => RulesApplied > 0;

    public bool HasFindings => Findings.Count > 0;

    /// <summary>
    /// One sentence that keeps "nothing to check" and "nothing wrong" apart, because an empty grid for the wrong
    /// reason is how a quality screen loses trust.
    /// </summary>
    public string Describe() =>
        !Succeeded ? $"The run failed: {Error}"
        : !HasRules ? "No quality rules apply to this table."
        : !HasFindings ? $"{RulesApplied} rule(s) ran over {RowsScanned} row(s) with no findings."
        : $"{Findings.Count} finding(s) from {RulesApplied} rule(s) over {RowsScanned} row(s): " +
          string.Join(", ", Findings.GroupBy(f => f.Severity).OrderBy(g => (int)g.Key)
              .Select(g => $"{g.Count()} {g.Key}"));
}

/// <summary>A resolution: who closed which finding and why (AC-4).</summary>
public sealed record OgQualityResolution(
    string FindingId,
    string Actor,
    string Note,
    DateTimeOffset ResolvedAtUtc);

/// <summary>DAT-07 AC-3: the last successful run per table, held in memory or in the diagnostics database.</summary>
public interface IOgQualityJournal
{
    void Record(OgQualityRun run);

    DateTimeOffset? LastSuccess(string table);
}

/// <summary>
/// Remembers the last successful run per table, so a later failed run cannot erase the timestamp of the last run
/// that actually looked at the data (AC-3).
/// </summary>
public sealed class OgQualityJournal : IOgQualityJournal
{
    private readonly Dictionary<string, DateTimeOffset> _lastSuccess = new(StringComparer.OrdinalIgnoreCase);

    public void Record(OgQualityRun run)
    {
        ArgumentNullException.ThrowIfNull(run);

        if (run.Succeeded)
        {
            _lastSuccess[run.Table] = run.CompletedAtUtc;
        }
    }

    /// <summary>The last successful run for the table, or null when it has never succeeded.</summary>
    public DateTimeOffset? LastSuccess(string table) =>
        _lastSuccess.TryGetValue(table, out var when) ? when : null;
}

/// <summary>
/// DAT-07: runs quality rules over rows that were read (bounded by the caller, as the workbench reads pages) and
/// sorts the findings worst-first.
/// </summary>
public static class OgQualityReviewer
{
    /// <summary>The default rules for a table: its keys, its required columns and the shape of its numbers and dates.</summary>
    public static IReadOnlyList<OgQualityRule> RulesFor(OgFormDefinition form)
    {
        ArgumentNullException.ThrowIfNull(form);

        var rules = new List<OgQualityRule>();

        if (form.KeyColumns.Count > 0)
        {
            rules.Add(new OgQualityRule("duplicate-key", null, OgQualityRuleKind.DuplicateKey, OgQualitySeverity.Critical));
        }

        foreach (var field in form.Fields.Where(f => !f.ReadOnly))
        {
            if (field.Required)
            {
                rules.Add(new OgQualityRule("required", field.Column, OgQualityRuleKind.Required, OgQualitySeverity.High));
            }

            switch (field.Kind)
            {
                case OgFormFieldKind.Decimal:
                case OgFormFieldKind.Integer:
                    rules.Add(new OgQualityRule("not-a-number", field.Column, OgQualityRuleKind.NotANumber, OgQualitySeverity.Medium));
                    break;

                case OgFormFieldKind.Date:
                    rules.Add(new OgQualityRule("not-a-date", field.Column, OgQualityRuleKind.NotADate, OgQualitySeverity.Medium));
                    break;
            }

            if (field.MaxLength is { } max)
            {
                rules.Add(new OgQualityRule("too-long", field.Column, OgQualityRuleKind.ValueTooLong, OgQualitySeverity.Low, Maximum: max));
            }
        }

        return rules;
    }

    /// <summary>Runs the rules over the rows and returns the findings, worst first.</summary>
    public static OgQualityRun Run(
        OgFormDefinition form,
        IReadOnlyList<OgRow> rows,
        IReadOnlyList<OgQualityRule> rules,
        DateTimeOffset? startedAt = null)
    {
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(rules);

        var started = startedAt ?? DateTimeOffset.UtcNow;
        var findings = new List<OgQualityFinding>();
        var keysSeen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var rule in rules)
        {
            if (rule.Kind == OgQualityRuleKind.DuplicateKey)
            {
                var duplicates = rows
                    .Select(row => (Row: row, Key: KeyOf(form, row)))
                    .Where(item => item.Key.Length > 0)
                    .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
                    .Where(group => group.Count() > 1);

                foreach (var group in duplicates)
                {
                    findings.Add(new OgQualityFinding(
                        Guid.NewGuid().ToString("n")[..12],
                        form.TableName,
                        rule.Code,
                        null,
                        group.Key,
                        rule.Severity,
                        $"{group.Count()} rows share the key {group.Key}.",
                        started));

                    keysSeen[group.Key] = group.Count();
                }

                continue;
            }

            foreach (var row in rows)
            {
                var key = KeyOf(form, row);

                if (Violates(rule, row.Values[form.Fields.First(f => f.Column == rule.Column!).Column], out var reason))
                {
                    findings.Add(new OgQualityFinding(
                        Guid.NewGuid().ToString("n")[..12],
                        form.TableName,
                        rule.Code,
                        rule.Column,
                        key,
                        rule.Severity,
                        reason!,
                        started));
                }
            }
        }

        return new OgQualityRun(
            form.TableName,
            started,
            DateTimeOffset.UtcNow,
            rows.Count,
            rules.Count,
            Succeeded: true,
            Ordered(findings));
    }

    /// <summary>Severity first, then column, rule and key - a stable order for two runs to be compared (AC-5).</summary>
    public static IReadOnlyList<OgQualityFinding> Ordered(IEnumerable<OgQualityFinding> findings) =>
        findings
            .OrderBy(finding => (int)finding.Severity)
            .ThenBy(finding => finding.Column ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(finding => finding.Rule, StringComparer.Ordinal)
            .ThenBy(finding => finding.Key, StringComparer.Ordinal)
            .ToList();

    /// <summary>Records who resolved a finding; an empty actor is refused, because an unattributed resolution is not one.</summary>
    public static OgResult<OgQualityResolution> Resolve(
        OgQualityFinding finding,
        string actor,
        string note,
        DateTimeOffset? at = null)
    {
        ArgumentNullException.ThrowIfNull(finding);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgQualityResolution>.Failed(
                "A resolution must record who resolved it.", "quality-actor-required");
        }

        return OgResult<OgQualityResolution>.Ok(new OgQualityResolution(
            finding.Id,
            actor.Trim(),
            note?.Trim() ?? string.Empty,
            at ?? DateTimeOffset.UtcNow));
    }

    private static bool Violates(OgQualityRule rule, object? value, out string? reason)
    {
        var text = Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() ?? string.Empty;
        reason = null;

        switch (rule.Kind)
        {
            case OgQualityRuleKind.Required when text.Length == 0:
                reason = $"{rule.Column} is empty.";
                return true;

            case OgQualityRuleKind.ValueTooLong when rule.Maximum is { } max && text.Length > max:
                reason = $"{rule.Column} is {text.Length} characters; the maximum is {max:0}.";
                return true;

            case OgQualityRuleKind.NotANumber when text.Length > 0
                                                  && !decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out _):
                reason = $"{rule.Column} is '{text}', which is not a number.";
                return true;

            case OgQualityRuleKind.NotADate when text.Length > 0
                                                 && !DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out _):
                reason = $"{rule.Column} is '{text}', which is not a date.";
                return true;

            case OgQualityRuleKind.OutOfRange when text.Length > 0:
                if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
                {
                    reason = $"{rule.Column} is '{text}', which is not a number.";
                    return true;
                }

                if (rule.Minimum is { } min && number < min)
                {
                    reason = $"{rule.Column} is {number}, below the minimum {min}.";
                    return true;
                }

                if (rule.Maximum is { } maxValue && number > maxValue)
                {
                    reason = $"{rule.Column} is {number}, above the maximum {maxValue}.";
                    return true;
                }

                return false;

            default:
                return false;
        }
    }

    private static string KeyOf(OgFormDefinition form, OgRow row) =>
        string.Join(
            "+",
            form.KeyColumns.Select(key =>
                Convert.ToString(row.Values.TryGetValue(key, out var value) ? value : null, CultureInfo.InvariantCulture) ?? string.Empty));
}
