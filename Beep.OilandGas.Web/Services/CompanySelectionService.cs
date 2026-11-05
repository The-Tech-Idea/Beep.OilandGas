using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Microsoft.AspNetCore.Components.Authorization;

namespace Beep.OilandGas.Web.Services;

/// <summary>Resolves selection through the existing API gateway; never infers membership from identifiers.</summary>
public sealed class CompanySelectionService(ICompanyAccessClient api, AuthenticationStateProvider authentication,
    CompanyClientContext company, Func<ClaimsPrincipal, string?> resolveAccount)
{
    public async Task<CompanyScope[]> ListAsync(CancellationToken token = default)
    {
        var user = (await authentication.GetAuthenticationStateAsync()).User;
        if (user.Identity?.IsAuthenticated != true || resolveAccount(user) is null) return [];
        var version = company.Version;
        var account = resolveAccount(user);
        var scopes = await api.ListAsync(token);
        if (version != company.Version || resolveAccount((await authentication.GetAuthenticationStateAsync()).User) != account)
            throw new OperationCanceledException("The signed-in account or selected company changed.");
        return scopes.Distinct().OrderBy(x => x.TenantId, StringComparer.Ordinal)
            .ThenBy(x => x.BusinessAssociateId, StringComparer.Ordinal).ToArray();
    }

    public async Task SelectAsync(CompanyScope scope, CancellationToken token = default)
    {
        var version = company.Clear();
        var user = (await authentication.GetAuthenticationStateAsync()).User;
        var account = user.Identity?.IsAuthenticated == true ? resolveAccount(user) : null;
        if (account is null) throw new InvalidOperationException("Sign in with an active OilGas account before choosing a company.");
        var access = await api.GetAccessAsync(scope, token);
        if (access.Scope != scope) throw new InvalidOperationException("The company access response does not match the selected company.");
        var current = (await authentication.GetAuthenticationStateAsync()).User;
        if (current.Identity?.IsAuthenticated != true || resolveAccount(current) != account ||
            !company.TrySelect(version, account, access))
            throw new OperationCanceledException("The signed-in account or selected company changed.");
    }
}
