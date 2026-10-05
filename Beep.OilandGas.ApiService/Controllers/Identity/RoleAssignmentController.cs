using Beep.OilandGas.ApiService.Data;
using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheTechIdeaWeb.Diagnostics;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/identity/roles")]
[Authorize(Roles = "Administrator")]
public class RoleAssignmentController : ControllerBase
{
    private readonly RepositoryRoleAssignmentService _roleService;
    private readonly ILogger<RoleAssignmentController> _logger;
    private readonly IFailureReporter _failures;

    public RoleAssignmentController(
        RepositoryRoleAssignmentService roleService,
        ILogger<RoleAssignmentController> logger,
        IFailureReporter failures)
    {
        _roleService = roleService;
        _logger = logger;
        _failures = failures;
    }

    // ── Catalog ─────────────────────────────────────────────────────────────

    /// <summary>Get the full role catalog.</summary>
    [HttpGet("catalog")]
    public async Task<IActionResult> GetRoleCatalog()
    {
        return Ok(await _roleService.GetRoleCatalogAsync()); 
    }

    /// <summary>Get the full permission catalog.</summary>
    [HttpGet("permissions/catalog")]
    public async Task<IActionResult> GetPermissionCatalog()
    {
        return Ok(await _roleService.GetPermissionCatalogAsync()); 
    }

    // ── Role assignments ─────────────────────────────────────────────────────

    /// <summary>Get all active role assignments for a user (Administrator; the route id names the subject).</summary>
    [HttpGet("users/{targetUserId}/assignments")]
    public async Task<IActionResult> GetUserRoleAssignments(string targetUserId)
    {
        if (string.IsNullOrWhiteSpace(targetUserId))
            return BadRequest(new { error = "User ID is required." });
        return Ok(await _roleService.GetUserRoleAssignmentsAsync(targetUserId)); 
    }

    /// <summary>Assign a role to a user (Administrator; the route id names the subject, the actor is the caller).</summary>
    [HttpPost("users/{targetUserId}/assignments")]
    public async Task<IActionResult> AssignRole(string targetUserId, [FromBody] AssignRoleRequest request)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(targetUserId) || string.IsNullOrWhiteSpace(request?.RoleId))
            return BadRequest(new { error = "User ID and RoleId are required." });
        try
        {
            var assignment = await _roleService.AssignRoleAsync(
                targetUserId, request.RoleId, actor, request.Reason);
            return Ok(assignment);
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "assigning a role"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "assigning a role"); }
    }

    /// <summary>Revoke a role assignment by its record ID.</summary>
    [HttpDelete("assignments/{userRoleId}")]
    public async Task<IActionResult> RevokeRole(string userRoleId)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(userRoleId))
            return BadRequest(new { error = "UserRoleId is required." });
        try
        {
            var ok = await _roleService.RevokeRoleAsync(userRoleId, actor);
            if (!ok) return NotFound(new { message = $"Assignment {userRoleId} not found." });
            return Ok(new { message = "Role assignment revoked." });
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "revoking a role assignment"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "revoking a role assignment"); }
    }

    // ── Role permissions ─────────────────────────────────────────────────────

    /// <summary>Get all active permissions for a role.</summary>
    [HttpGet("{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return BadRequest(new { error = "Role ID is required." });
        return Ok(await _roleService.GetRolePermissionsAsync(roleId)); 
    }

    /// <summary>Grant a permission to a role.</summary>
    [HttpPost("{roleId}/permissions")]
    public async Task<IActionResult> GrantPermission(string roleId, [FromBody] GrantPermissionRequest request)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(request?.PermissionId))
            return BadRequest(new { error = "Role ID and PermissionId are required." });
        try
        {
            var grant = await _roleService.GrantPermissionToRoleAsync(
                roleId, request.PermissionId, actor);
            return Ok(grant);
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "granting a permission to a role"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "granting a permission to a role"); }
    }

    /// <summary>Revoke a permission from a role by grant record ID.</summary>
    [HttpDelete("permissions/{rolePermissionId}")]
    public async Task<IActionResult> RevokePermission(string rolePermissionId)
    {
        var actor = User.ActingUserId();
        if (string.IsNullOrWhiteSpace(rolePermissionId))
            return BadRequest(new { error = "RolePermissionId is required." });
        try
        {
            var ok = await _roleService.RevokePermissionFromRoleAsync(rolePermissionId, actor);
            if (!ok) return NotFound(new { message = $"Grant {rolePermissionId} not found." });
            return Ok(new { message = "Permission grant revoked." });
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "revoking a permission from a role"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "revoking a permission from a role"); }
    }

    // A grant somebody else changed first — a stale version, or the same row written twice — is the administrator's to
    // reload. Any other refused save is a failure and goes on to the API's handler (OILGAS-CATCH-01: every refused save was
    // answered "the grant changed", unreported).
    private ConflictObjectResult GrantChanged(DbUpdateException refused, string operation)
    {
        _failures.ReportHandled(refused, operation, "nothing is changed; the administrator is told to reload", FailureSeverity.Degraded);
        return Conflict(new { error = "The grant changed. Reload before retrying." });
    }
}
