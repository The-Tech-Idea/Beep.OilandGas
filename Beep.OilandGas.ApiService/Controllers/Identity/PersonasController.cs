using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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
    private string? Actor => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool CanAccess(string userId) => User.Identity?.IsAuthenticated == true && !string.IsNullOrWhiteSpace(Actor) &&
        (string.Equals(Actor, userId, StringComparison.Ordinal) || User.IsInRole("Administrator"));

    [HttpGet]
    public async Task<IActionResult> Catalog(CancellationToken token) => Ok(await personas.CatalogAsync(token));

    [HttpPut("{code}")]
    [Authorize(Roles = "Administrator")]
    public Task<IActionResult> SaveCatalog(string code, PersonaCatalogUpdate request, CancellationToken token) =>
        string.IsNullOrWhiteSpace(Actor) ? Task.FromResult<IActionResult>(Forbid()) :
        Write(async () => await personas.SaveCatalogAsync(code, request, Actor!, token));

    [HttpGet("users/{userId}")]
    public async Task<IActionResult> Profile(string userId, CancellationToken token)
    {
        if (!CanAccess(userId)) return Forbid();
        return Ok(new PersonaProfileResult(await personas.GetAsync(userId, token)));
    }

    [HttpPut("users/{userId}")]
    public Task<IActionResult> SaveProfile(string userId, PersonaProfileUpdate request, CancellationToken token) =>
        !CanAccess(userId) ? Task.FromResult<IActionResult>(Forbid()) :
        Write(async () => await personas.SaveAsync(userId, request, Actor!, token));

    [HttpGet("users/{userId}/preferences/{code}")]
    public async Task<IActionResult> Preferences(string userId, string code, CancellationToken token)
    {
        if (!CanAccess(userId)) return Forbid();
        return Ok(await personas.PreferencesAsync(userId, code, token));
    }

    [HttpPut("users/{userId}/preferences/{code}/{viewKey}")]
    public Task<IActionResult> SavePreference(string userId, string code, string viewKey, PersonaPreferenceUpdate request, CancellationToken token) =>
        !CanAccess(userId) ? Task.FromResult<IActionResult>(Forbid()) :
        Write(async () => await personas.SavePreferenceAsync(userId, code, viewKey, request, Actor!, token));

    private async Task<IActionResult> Write(Func<Task<object>> save)
    {
        try { return Ok(await save()); }
        catch (DbUpdateException) { return Conflict(new { Error = "Settings changed or could not be saved. Reload before retrying." }); }
        catch (ValidationException) { return BadRequest(new { Error = "Invalid persona settings." }); }
        catch (ArgumentException exception) { return BadRequest(new { Error = exception.Message }); }
    }
}
