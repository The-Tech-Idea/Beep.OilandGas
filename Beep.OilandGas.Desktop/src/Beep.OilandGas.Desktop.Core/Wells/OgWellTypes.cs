using System.Globalization;

namespace TheTechIdea.Beep.OilandGas.Desktop.Core.Wells;

/// <summary>One row of the well index (WEL-01).</summary>
public sealed record OgWellSummary(
    string Id,
    string Name,
    string? ApiNumber,
    string? Field,
    string? Status,
    string? WellType,
    DateTime? SpudDate,
    DateTime? LastActivity)
{
    /// <summary>The sentence the grid shows for this row, so a copied row still says what it is.</summary>
    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                Name,
                string.IsNullOrWhiteSpace(Id) ? null : $"UWI {Id}",
                string.IsNullOrWhiteSpace(ApiNumber) ? null : $"API {ApiNumber}",
                string.IsNullOrWhiteSpace(Field) ? null : Field,
                string.IsNullOrWhiteSpace(Status) ? null : Status,
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}

/// <summary>What the well index is asked for. Every filter is optional and every one that is set must match.</summary>
public sealed record OgWellQuery(
    string? Text = null,
    string? Field = null,
    string? Status = null,
    string? WellType = null,
    int? SpudYearFrom = null,
    int? SpudYearTo = null,
    int Page = 0,
    int PageSize = 0)
{
    public bool HasFilters =>
        !string.IsNullOrWhiteSpace(Text)
        || !string.IsNullOrWhiteSpace(Field)
        || !string.IsNullOrWhiteSpace(Status)
        || !string.IsNullOrWhiteSpace(WellType)
        || SpudYearFrom is not null
        || SpudYearTo is not null;

    /// <summary>The filter values as saved-search text, the same shape the store persists.</summary>
    public string Describe() =>
        string.Join(
            " · ",
            new[]
            {
                string.IsNullOrWhiteSpace(Text) ? null : $"text '{Text.Trim()}'",
                string.IsNullOrWhiteSpace(Field) ? null : $"field {Field.Trim()}",
                string.IsNullOrWhiteSpace(Status) ? null : $"status {Status.Trim()}",
                string.IsNullOrWhiteSpace(WellType) ? null : $"type {WellType.Trim()}",
                SpudYearFrom is null && SpudYearTo is null
                    ? null
                    : $"spud {SpudYearFrom?.ToString(CultureInfo.InvariantCulture) ?? "*"}-{SpudYearTo?.ToString(CultureInfo.InvariantCulture) ?? "*"}",
            }.Where(part => !string.IsNullOrWhiteSpace(part)));
}
