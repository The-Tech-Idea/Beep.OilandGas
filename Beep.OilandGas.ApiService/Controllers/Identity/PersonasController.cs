using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/personas")]
[Authorize]
public sealed class PersonasController(RepositoryPersonaService personas) : ControllerBase
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

    private async Task<IActionResult> Write(Func<Task<object>> save)
    {
        try { return Ok(await save()); }
        catch (DbUpdateException) { return Conflict(new { Error = "Settings changed or could not be saved. Reload before retrying." }); }
        catch (ValidationException) { return BadRequest(new { Error = "Invalid persona settings." }); }
        catch (ArgumentException exception) { return BadRequest(new { Error = exception.Message }); }
    }
}
