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
    private bool IsLocalUser => !string.IsNullOrWhiteSpace(User.FindActingUserId());
    private bool IsAdministrator => User.IsInRole("Administrator");
    // Self or Administrator: the route id names the subject being read or changed; the actor is always the signed-in
    // account (RepositoryUserService records it).
    private bool CanAccess(string targetUserId) => IsLocalUser && (User.FindActingUserId() == targetUserId || IsAdministrator);

    [HttpGet]
    [Authorize(Policy = "Admin.ManageUsers")]
    public async Task<IActionResult> GetAllUsers() => Ok(await users.GetAllAsync());

    [HttpGet("{targetUserId}")]
    public async Task<IActionResult> GetUser(string targetUserId)
    {
        if (!CanAccess(targetUserId)) return Forbid();
        var user = await users.GetByIdAsync(targetUserId);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPut("{targetUserId}")]
    public async Task<IActionResult> UpdateUser(string targetUserId, [FromBody] RepositoryUserUpdate request)
    {
        if (!CanAccess(targetUserId) || (request.IsActive.HasValue && !IsAdministrator)) return Forbid();
        try
        {
            var user = await users.UpdateAsync(targetUserId, request);
            return user is null ? NotFound() : Ok(user);
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (DbUpdateConcurrencyException) { return Conflict(new { error = "The user changed. Reload before saving." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpGet("{targetUserId}/roles")]
    public async Task<IActionResult> GetUserRoles(string targetUserId)
    {
        if (!CanAccess(targetUserId)) return Forbid();
        return Ok(await users.GetRolesAsync(targetUserId));
    }

    [HttpPost("{targetUserId}/roles")]
    [Authorize(Policy = "Admin.AssignRoles")]
    public async Task<IActionResult> AddRole(string targetUserId, [FromBody] UserRoleChangeRequest request)
    {
        if (!IsLocalUser) return Forbid();
        if (string.IsNullOrWhiteSpace(request.RoleName)) return BadRequest();
        try
        {
            return await users.AddToRoleAsync(targetUserId, request.RoleName) ? NoContent() : NotFound();
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (DbUpdateException) { return Conflict(new { error = "The assignment changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }

    [HttpDelete("{targetUserId}/roles/{roleName}")]
    [Authorize(Policy = "Admin.AssignRoles")]
    public async Task<IActionResult> RemoveRole(string targetUserId, string roleName)
    {
        if (!IsLocalUser) return Forbid();
        try
        {
            return await users.RemoveFromRoleAsync(targetUserId, roleName) ? NoContent() : NotFound();
        }
        catch (DbUpdateException) { return Conflict(new { error = "The assignment changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
    }
}
