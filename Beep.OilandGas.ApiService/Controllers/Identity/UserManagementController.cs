using System.Security.Claims;
using Beep.OilandGas.ApiService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/identity/users")]
[Authorize]
public sealed class UserManagementController(RepositoryUserService users) : ControllerBase
{
    private string? Actor => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private bool IsAdministrator => User.IsInRole("Administrator");
    private bool CanAccess(string id) => !string.IsNullOrWhiteSpace(Actor) && (Actor == id || IsAdministrator);

    [HttpGet]
    [Authorize(Policy = "Admin.ManageUsers")]
    public async Task<IActionResult> GetAllUsers() => Ok(await users.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        if (!CanAccess(id)) return Forbid();
        var user = await users.GetByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] RepositoryUserUpdate request)
    {
        if (!CanAccess(id) || (request.IsActive.HasValue && !IsAdministrator)) return Forbid();
        try
        {
            var user = await users.UpdateAsync(id, request);
            return user is null ? NotFound() : Ok(user);
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { error = "The user changed. Reload before saving." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetUserRoles(string id)
    {
        if (!CanAccess(id)) return Forbid();
        return Ok(await users.GetRolesAsync(id));
    }

    [HttpPost("{id}/roles")]
    [Authorize(Policy = "Admin.AssignRoles")]
    public async Task<IActionResult> AddRole(string id, [FromBody] UserRoleChangeRequest request)
    {
        if (string.IsNullOrWhiteSpace(Actor)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.RoleName)) return BadRequest();
        try
        {
            return await users.AddToRoleAsync(id, request.RoleName) ? NoContent() : NotFound();
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (DbUpdateException) { return Conflict(new { error = "The assignment changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpDelete("{id}/roles/{roleName}")]
    [Authorize(Policy = "Admin.AssignRoles")]
    public async Task<IActionResult> RemoveRole(string id, string roleName)
    {
        if (string.IsNullOrWhiteSpace(Actor)) return Forbid();
        try
        {
            return await users.RemoveFromRoleAsync(id, roleName) ? NoContent() : NotFound();
        }
        catch (DbUpdateException) { return Conflict(new { error = "The assignment changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }
}
