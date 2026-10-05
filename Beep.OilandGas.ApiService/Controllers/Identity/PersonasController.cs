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

    [HttpPut("{code}")]
    [Authorize(Roles = "Administrator")]
    public Task<IActionResult> SaveCatalog(string code, PersonaCatalogUpdate request, CancellationToken token)
    {
        var actor = User.ActingUserId();
        return Write(async () => await personas.SaveCatalogAsync(code, request, actor, token));
    }

    [HttpGet("users/{targetUserId}")]
    public async Task<IActionResult> Profile(string targetUserId, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Forbid();
        return Ok(new PersonaProfileResult(await personas.GetAsync(targetUserId, token)));
    }

    [HttpPut("users/{targetUserId}")]
    public Task<IActionResult> SaveProfile(string targetUserId, PersonaProfileUpdate request, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Task.FromResult<IActionResult>(Forbid());
        var actor = User.ActingUserId();
        return Write(async () => await personas.SaveAsync(targetUserId, request, actor, token));
    }

    [HttpGet("users/{targetUserId}/preferences/{code}")]
    public async Task<IActionResult> Preferences(string targetUserId, string code, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Forbid();
        return Ok(await personas.PreferencesAsync(targetUserId, code, token));
    }

    [HttpPut("users/{targetUserId}/preferences/{code}/{viewKey}")]
    public Task<IActionResult> SavePreference(string targetUserId, string code, string viewKey, PersonaPreferenceUpdate request, CancellationToken token)
    {
        if (!CanAccess(targetUserId)) return Task.FromResult<IActionResult>(Forbid());
        var actor = User.ActingUserId();
        return Write(async () => await personas.SavePreferenceAsync(targetUserId, code, viewKey, request, actor, token));
    }

    // A stale version, or a row somebody else created first, is the one answer here: the person reloads. Any other refused
    // save is a failure and goes on to the API's handler (OILGAS-CATCH-01: every refused save was answered "changed or could
    // not be saved", unreported). The settings' own rules are checked with DataAnnotations, whose refusal is the BCL's
    // ValidationException; it is refused here in this API's words.
    private async Task<IActionResult> Write(Func<Task<object>> save)
    {
        try { return Ok(await save()); }
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
