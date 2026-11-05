using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Uses the canonical PPDM service, which applies company and asset grants to every WELL read.</summary>
public sealed class CompanyEnvironmentWellReader(IPPDM39DataService data, CompanyModuleConnection scope) : ICompanyEnvironmentWellReader
{
    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> ReadAsync(CompanyScope company, string wellId,
        IReadOnlyDictionary<string, string> filters, Func<Task> authority, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); await authority();
        if (scope.Company.Scope != company || !filters.TryGetValue("UWI", out var selected) || selected != wellId)
            throw new UnauthorizedAccessException("The selected company or well changed.");
        var binding = await scope.ResolveAsync(token: token);
        var query = filters.Select(x => new AppFilter { FieldName = x.Key, Operator = "=", FilterValue = x.Value }).ToList();
        var result = await data.GetEntitiesAsync("WELL", query, binding.ConnectionName);
        token.ThrowIfCancellationRequested(); await authority();
        if (scope.Company.Scope != company || await scope.ResolveAsync(token: token) != binding)
            throw new UnauthorizedAccessException("The selected company or source changed.");
        if (result is null || !result.Success || result.Entities is null || result.Count != result.Entities.Count || result.Count > 1)
            throw new InvalidOperationException("The company WELL source did not return a confirmed result.");
        return result.Entities.Select(row => (IReadOnlyDictionary<string, object?>)row.ToDictionary(x => x.Key,
            x => x.Value is DBNull ? null : (object?)x.Value, StringComparer.OrdinalIgnoreCase)).ToArray();
    }
}
