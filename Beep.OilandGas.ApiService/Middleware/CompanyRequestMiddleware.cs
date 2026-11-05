using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Middleware;

/// <summary>Validates company selectors before domain authorization/data access. Account resolution remains the existing auth owner's.</summary>
public sealed class CompanyRequestMiddleware(RequestDelegate next, Func<ClaimsPrincipal, string?> resolveAccount)
{
    public async Task InvokeAsync(HttpContext http)
    {
        var path = http.Request.Path;
        if (!path.StartsWithSegments("/api") && !path.StartsWithSegments("/hubs") && path != "/progressHub")
        { await next(http); return; }
        if (SetupBootstrapPaths.IsKeyProtected(path))
        { await next(http); return; }
        if (http.User.Identity?.IsAuthenticated != true) { await next(http); return; }
        var tenant = http.RequestServices.GetRequiredService<RepositoryTenantContext>();
        var companies = http.RequestServices.GetRequiredService<RepositoryCompanyContextResolver>();
        var accounts = http.RequestServices.GetRequiredService<IRepositoryAccessService>();
        var bindings = http.RequestServices.GetRequiredService<ModuleBindingService>();
        var control = CompanyRequestPolicy.IsControlPlane(path.Value!);
        var tenantValues = http.Request.Headers[CompanyRequestHeaders.Tenant];
        var companyValues = http.Request.Headers[CompanyRequestHeaders.Company];
        if (control && tenantValues.Count == 0) tenantValues = http.Request.Query["tenantId"];
        if (tenantValues.Count > 1 || companyValues.Count > 1)
        { await Refuse(http, 400, "Select exactly one tenant and company."); return; }
        var tenantId = tenantValues.Count == 1 ? tenantValues[0] : null;
        var companyId = companyValues.Count == 1 ? companyValues[0] : null;
        var userId = resolveAccount(http.User);
        if (userId is null) { await Refuse(http, 403, "An active OilGas account is required."); return; }
        if (control)
        {
            // Current-account selectors are validated by that controller, not elevated into administration authority.
            if (path.StartsWithSegments("/api/auth/repository")) { await next(http); return; }
            if (tenantId is not null)
            {
                if (string.IsNullOrWhiteSpace(tenantId) || tenantId.Length > 64)
                { await Refuse(http, 400, "Select a valid tenant."); return; }
                var queryTenant = http.Request.Query["tenantId"];
                if (queryTenant.Count > 1 || (queryTenant.Count == 1 && queryTenant[0] != tenantId))
                { await Refuse(http, 400, "The tenant selectors disagree."); return; }
                var account = await accounts.GetAccessAsync(userId, http.RequestAborted);
                if (account is not { IsActive: true } || !account.Roles.Contains(RepositoryBootstrapService.AdministratorRole, StringComparer.Ordinal))
                { await Refuse(http, 403, "A repository administrator is required."); return; }
                await bindings.RequireActiveTenantAsync(tenantId, http.RequestAborted);
                tenant.SelectTenant(tenantId);
            }
            await next(http);
            return;
        }
        if (string.IsNullOrWhiteSpace(tenantId) || tenantId.Length > 64 ||
            string.IsNullOrWhiteSpace(companyId) || companyId.Length > 128)
        { await Refuse(http, 400, "Select a tenant and company before accessing business data."); return; }
        var company = await companies.ResolveAsync(userId, new(tenantId, companyId), http.RequestAborted);
        if (company is null) { await Refuse(http, 403, "This company is unavailable to your account."); return; }
        tenant.SelectCompany(company);
        http.Features.Set(company);
        http.User = CompanyPrincipal(http.User, company);
        await next(http);
    }

    private static ClaimsPrincipal CompanyPrincipal(ClaimsPrincipal principal, RepositoryCompanyContext company)
    {
        var result = new ClaimsPrincipal();
        var first = true;
        foreach (var identity in principal.Identities)
        {
            var claims = identity.Claims.Where(x => x.Type is not "role" and not "roles" and not "permission" and not "permissions" and
                not "elevated_permissions" and not "field_scope" && x.Type != ClaimTypes.Role && x.Type != identity.RoleClaimType).ToList();
            if (first)
            {
                claims.AddRange(company.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
                claims.AddRange(company.Permissions.Select(permission => new Claim("permission", permission)));
            }
            result.AddIdentity(new ClaimsIdentity(claims, identity.AuthenticationType, identity.NameClaimType, ClaimTypes.Role));
            first = false;
        }
        return result;
    }

    private static Task Refuse(HttpContext http, int status, string detail)
    {
        http.Response.StatusCode = status;
        return http.Response.WriteAsJsonAsync(new ProblemDetails { Status = status, Title = "Company selection refused", Detail = detail },
            http.RequestAborted);
    }
}
