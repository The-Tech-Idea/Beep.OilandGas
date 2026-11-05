using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.ApiService.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/personas")]
[Authorize]
public sealed class PersonasController(RepositoryPersonaService personas, IFailureReporter failures) : ControllerBase
{
    // Self or Administrator: the route id names the subject whose personas are read or changed; the actor is always the
    // signed-in account.
    private bool CanAccess(string targetUserId) => User.Identity?.IsAuthenticated == true
        && !string.IsNullOrWhiteSpace(User.FindActingUserId())
        && (string.Equals(User.FindActingUserId(), targetUserId, StringComparison.Ordinal) || User.IsInRole("Administrator"));

    [HttpGet]
    public async Task<IActionResult> Catalog(CancellationToken token) => Ok(await personas.CatalogAsync(token));

    [HttpGet("workspaces/current")]
    public async Task<IActionResult> Workspaces([FromServices] RepositoryTenantContext tenant,
        [FromServices] IRepositoryAccessService access, CancellationToken token)
    {
        var actor = User.ActingUserId();
        if (tenant.Company is not { } company || company.UserId != actor) return Forbid();
        try { return Ok(await personas.ReviewWorkspacesAsync(actor, company.Scope, access, token)); }
        catch (UnauthorizedAccessException denied)
        {
            failures.ReportHandled(denied, "reviewing current company workspaces", "workspace access is refused", FailureSeverity.Degraded);
            return Forbid();
        }
        catch (InvalidOperationException changed)
        {
            failures.ReportHandled(changed, "reviewing current company workspaces", "workspace configuration must be reloaded", FailureSeverity.Degraded);
            return Conflict(new { Error = "Company access or workspace configuration changed. Reload company access." });
        }
    }

    [HttpGet("roles/{roleId}/workspaces")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> RoleWorkspaces(string roleId, [FromServices] IRepositoryAccessService access, CancellationToken token)
    {
        try { return Ok(await personas.ReviewRoleWorkspaceAsync(roleId, User.ActingUserId(), access, token)); }
        catch (UnauthorizedAccessException denied)
        {
            failures.ReportHandled(denied, "reviewing role workspace navigation", "workspace mappings are refused", FailureSeverity.Degraded);
            return Forbid();
        }
        catch (ArgumentException invalid) { throw new RefusalException(RefusalKind.Invalid, "The app role is unavailable.", invalid); }
        catch (InvalidOperationException changed)
        {
            failures.ReportHandled(changed, "reviewing role workspace navigation", "workspace mappings need review", FailureSeverity.Degraded);
            return Conflict(new { Error = "Workspace mappings changed or require reconciliation. Reload before editing." });
        }
    }

    [HttpPut("roles/{roleId}/workspaces/{code}")]
    [Authorize(Roles = "Administrator")]
    public async Task<IActionResult> MapRoleWorkspace(string roleId, string code, RoleWorkspaceMappingRequest request,
        [FromServices] IRepositoryAccessService access, CancellationToken token)
    {
        try
        {
            await personas.SaveRoleWorkspaceAsync(roleId, code, request.Enabled, request.ExpectedRevision, User.ActingUserId(), access, token);
            return NoContent();
        }
        catch (UnauthorizedAccessException denied)
        {
            failures.ReportHandled(denied, "configuring role workspace navigation", "the mapping is not changed", FailureSeverity.Degraded);
            return Forbid();
        }
        catch (ArgumentException invalid) { throw new RefusalException(RefusalKind.Invalid, "The role workspace mapping is invalid.", invalid); }
        catch (InvalidOperationException changed)
        {
            failures.ReportHandled(changed, "configuring role workspace navigation", "the mapping is not confirmed; review configuration", FailureSeverity.Degraded);
            return Conflict(new { Error = "Workspace mappings require reconciliation before changing them." });
        }
    }

    [HttpPut("{code}")]
    [Authorize(Roles = "Administrator")]
    public Task<IActionResult> SaveCatalog(string code, PersonaCatalogUpdate request, CancellationToken token)
    {
        var actor = User.ActingUserId();
        return Write(async () => await personas.SaveCatalogAsync(code, request, actor, token));
    }

    [HttpGet("users/{targetUserId}")]
    public async Task<IActionResult> Profile(string targetUserId, [FromServices] RepositoryTenantContext tenant,
        [FromServices] IRepositoryAccessService access, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Forbid();
        if (!await CanReadCompanyProfile(targetUserId, tenant, access, token)) return Forbid();
        var company = tenant.Company!.Scope;
        var profile = await personas.GetAsync(targetUserId, company, token);
        return await CanReadCompanyProfile(targetUserId, tenant, access, token) ? Ok(new PersonaProfileResult(profile)) : Forbid();
    }

    [HttpPut("users/{targetUserId}")]
    public Task<IActionResult> SaveProfile(string targetUserId, PersonaProfileUpdate request,
        [FromServices] RepositoryTenantContext tenant, [FromServices] IRepositoryAccessService access, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Task.FromResult<IActionResult>(Forbid());
        var actor = User.ActingUserId();
        if (tenant.Company is not { } company || company.UserId != actor) return Task.FromResult<IActionResult>(Forbid());
        return Write(async () => await personas.SaveAsync(targetUserId, request, actor, company.Scope, access, token));
    }

    [HttpGet("users/{targetUserId}/preferences/{code}")]
    public async Task<IActionResult> Preferences(string targetUserId, string code, [FromServices] RepositoryTenantContext tenant,
        [FromServices] IRepositoryAccessService access, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Forbid();
        if (!await CanReadCompanyProfile(targetUserId, tenant, access, token)) return Forbid();
        var company = tenant.Company!.Scope;
        var result = await personas.PreferencesAsync(targetUserId, company, code, token);
        return await CanReadCompanyProfile(targetUserId, tenant, access, token) ? Ok(result) : Forbid();
    }

    [HttpPut("users/{targetUserId}/preferences/{code}/{viewKey}")]
    public Task<IActionResult> SavePreference(string targetUserId, string code, string viewKey, PersonaPreferenceUpdate request,
        [FromServices] RepositoryTenantContext tenant, [FromServices] IRepositoryAccessService access, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Task.FromResult<IActionResult>(Forbid());
        var actor = User.ActingUserId();
        if (tenant.Company is not { } company || company.UserId != actor) return Task.FromResult<IActionResult>(Forbid());
        return Write(async () => await personas.SavePreferenceAsync(targetUserId, company.Scope, code, viewKey, request, actor, access, token));
    }

    private async Task<bool> CanReadCompanyProfile(string target, RepositoryTenantContext tenant, IRepositoryAccessService access, CancellationToken token)
    {
        var actor = User.FindActingUserId();
        if (actor is null || tenant.Company is not { } selected || selected.UserId != actor) return false;
        var actorAccess = await access.GetCompanyAccessAsync(actor, selected.Scope, token);
        if (actorAccess is null || actorAccess.Scope != selected.Scope) return false;
        if (actor != target)
        {
            var administrator = await access.GetAccessAsync(actor, token);
            if (administrator is not { IsActive: true } || !administrator.Roles.Contains("Administrator", StringComparer.Ordinal)) return false;
        }
        var targetAccess = await access.GetCompanyAccessAsync(target, selected.Scope, token);
        return targetAccess is not null && targetAccess.Scope == selected.Scope;
    }

    // A stale version, or a row somebody else created first, is the one answer here: the person reloads. Any other refused
    // save is a failure and goes on to the API's handler (OILGAS-CATCH-01: every refused save was answered "changed or could
    // not be saved", unreported). The settings' own rules are checked with DataAnnotations, whose refusal is the BCL's
    // ValidationException; it is refused here in this API's words.
    private async Task<IActionResult> Write(Func<Task<object>> save)
    {
        try { return Ok(await save()); }
        catch (UnauthorizedAccessException denied)
        {
            failures.ReportHandled(denied, "saving persona settings", "the settings are not saved because current access was not confirmed", FailureSeverity.Degraded);
            return Forbid();
        }
        catch (InvalidOperationException changed)
        {
            failures.ReportHandled(changed, "saving persona settings", "the settings are not saved; company access must be reloaded", FailureSeverity.Degraded);
            return Conflict(new { Error = "Company access or workspace configuration changed. Reload before retrying." });
        }
        catch (DbUpdateConcurrencyException changed)
        {
            failures.ReportHandled(changed, "saving persona settings", "the settings are not saved; the person is told to reload", FailureSeverity.Degraded);
            return Conflict(new { Error = "Settings changed. Reload before retrying." });
        }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate))
        {
            failures.ReportHandled(duplicate, "saving persona settings", "the settings are not saved; the person is told to reload", FailureSeverity.Degraded);
            return Conflict(new { Error = "Settings changed. Reload before retrying." });
        }
        catch (ValidationException invalid)
        {
            throw new RefusalException(RefusalKind.Invalid, "Invalid persona settings.", invalid);
        }
    }
}
