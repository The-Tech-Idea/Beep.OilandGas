using System.Globalization;
using Beep.OilandGas.Models.Core.Refusals;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reviews explicit WELL fields through the existing company and asset-scoped data owner.</summary>
public sealed class CompanyWellQualityReviewer(ICompanyEnvironmentWellReader reader)
{
    private static readonly string[] Fields = ["UWI", "WELL_NAME", "WELL_NUM"];

    public async Task<CompanyWellQualityBatchReview> ReviewBatchAsync(CompanyScope company, CompanyWellQualityBatchRequest request,
        Func<Task> authority, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.WellIds is null || request.WellIds.Length is < 1 or > 20 ||
            request.WellIds.Any(id => string.IsNullOrWhiteSpace(id) || id.Length > 128 || id != id.Trim() || id.Any(char.IsControl)) ||
            request.WellIds.Distinct(StringComparer.Ordinal).Count() != request.WellIds.Length)
            throw RefusalException.Invalid("Enter one to twenty distinct well identifiers.");
        ArgumentNullException.ThrowIfNull(company); ArgumentNullException.ThrowIfNull(authority);
        var first = new List<string?[]?>(request.WellIds.Length);
        foreach (var wellId in request.WellIds)
        {
            token.ThrowIfCancellationRequested();
            first.Add(await ReadAsync(company, wellId, authority, token));
        }
        var wells = new List<CompanyWellQualityReview>(request.WellIds.Length);
        for (var i = 0; i < request.WellIds.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            var second = await ReadAsync(company, request.WellIds[i], authority, token);
            wells.Add(Compare(company, request.WellIds[i], first[i], second));
        }
        await authority();
        return new(company, wells.ToArray());
    }

    public async Task<CompanyWellQualityReview> ReviewAsync(CompanyScope company, string wellId, Func<Task> authority, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(company); ArgumentNullException.ThrowIfNull(authority);
        if (string.IsNullOrWhiteSpace(wellId) || wellId.Length > 128 || wellId != wellId.Trim() || wellId.Any(char.IsControl))
            throw RefusalException.Invalid("Enter a valid well identifier.");
        var first = await ReadAsync(company, wellId, authority, token);
        var second = await ReadAsync(company, wellId, authority, token);
        await authority();
        return Compare(company, wellId, first, second);
    }

    private async Task<string?[]?> ReadAsync(CompanyScope company, string wellId, Func<Task> authority, CancellationToken token)
    {
        await authority();
        var filter = new Dictionary<string, string>(StringComparer.Ordinal) { ["UWI"] = wellId };
        var rows = await reader.ReadAsync(company, wellId, filter, authority, token);
        if (rows is null || rows.Count > 1) throw RefusalException.Conflict("The well source is ambiguous.");
        if (rows.Count == 0) return null;
        var row = rows[0];
        if (row is null || Fields.Any(name => !row.ContainsKey(name))) throw RefusalException.Conflict("The required well fields are unavailable.");
        var values = Fields.Select(name => row[name] is null or DBNull ? null : Convert.ToString(row[name], CultureInfo.InvariantCulture)).ToArray();
        if (values[0] != wellId) throw RefusalException.Conflict("The well source returned a different record.");
        return values;
    }

    private static CompanyWellQualityReview Compare(CompanyScope company, string wellId, string?[]? first, string?[]? second)
    {
        if ((first is null) != (second is null) || first is not null && second is not null && !first.SequenceEqual(second, StringComparer.Ordinal))
            throw RefusalException.Conflict("The well record changed during review. Try again.");
        return new(company, wellId, second is null ? "NotVisible" : "Found", second is null ? [] :
            Fields.Select((name, i) => new CompanyWellQualityField(name, !string.IsNullOrWhiteSpace(second[i]))).ToArray());
    }
}
