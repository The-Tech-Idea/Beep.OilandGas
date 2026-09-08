using Beep.OilandGas.ApiService.Services;
using TheTechIdea.Data.OilGas;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

[ApiController]
[Route("api/identity/roles")]
[Authorize(Roles = "Administrator")]
public class RoleAssignmentController : ControllerBase
{
    private readonly RepositoryRoleAssignmentService _roleService;
    private readonly ILogger<RoleAssignmentController> _logger;

    public RoleAssignmentController(
        RepositoryRoleAssignmentService roleService,
        ILogger<RoleAssignmentController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    private string ActorUserId =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";

    // ── Catalog ─────────────────────────────────────────────────────────────

    /// <summary>Get the full role catalog.</summary>
    [HttpGet("catalog")]
    public async Task<IActionResult> GetRoleCatalog()
    {
        try { return Ok(await _roleService.GetRoleCatalogAsync()); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get role catalog");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Get the full permission catalog.</summary>
    [HttpGet("permissions/catalog")]
    public async Task<IActionResult> GetPermissionCatalog()
    {
        try { return Ok(await _roleService.GetPermissionCatalogAsync()); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get permission catalog");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    // ── Role assignments ─────────────────────────────────────────────────────

    /// <summary>Get all active role assignments for a user.</summary>
    [HttpGet("users/{userId}/assignments")]
    public async Task<IActionResult> GetUserRoleAssignments(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { error = "User ID is required." });
        try { return Ok(await _roleService.GetUserRoleAssignmentsAsync(userId)); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get role assignments for user {UserId}", userId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Assign a role to a user.</summary>
    [HttpPost("users/{userId}/assignments")]
    public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(ActorUserId)) return Forbid();
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(request?.RoleId))
            return BadRequest(new { error = "User ID and RoleId are required." });
        try
        {
            var assignment = await _roleService.AssignRoleAsync(
                userId, request.RoleId, ActorUserId, request.Reason);
            return Ok(assignment);
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { error = "The grant changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign role {RoleId} to user {UserId}", request?.RoleId, userId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Revoke a role assignment by its record ID.</summary>
    [HttpDelete("assignments/{userRoleId}")]
    public async Task<IActionResult> RevokeRole(string userRoleId)
    {
        if (string.IsNullOrWhiteSpace(ActorUserId)) return Forbid();
        if (string.IsNullOrWhiteSpace(userRoleId))
            return BadRequest(new { error = "UserRoleId is required." });
        try
        {
            var ok = await _roleService.RevokeRoleAsync(userRoleId, ActorUserId);
            if (!ok) return NotFound(new { message = $"Assignment {userRoleId} not found." });
            return Ok(new { message = "Role assignment revoked." });
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { error = "The grant changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke role assignment {UserRoleId}", userRoleId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    // ── Role permissions ─────────────────────────────────────────────────────

    /// <summary>Get all active permissions for a role.</summary>
    [HttpGet("{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return BadRequest(new { error = "Role ID is required." });
        try { return Ok(await _roleService.GetRolePermissionsAsync(roleId)); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get permissions for role {RoleId}", roleId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Grant a permission to a role.</summary>
    [HttpPost("{roleId}/permissions")]
    public async Task<IActionResult> GrantPermission(string roleId, [FromBody] GrantPermissionRequest request)
    {
        if (string.IsNullOrWhiteSpace(ActorUserId)) return Forbid();
        if (string.IsNullOrWhiteSpace(roleId) || string.IsNullOrWhiteSpace(request?.PermissionId))
            return BadRequest(new { error = "Role ID and PermissionId are required." });
        try
        {
            var grant = await _roleService.GrantPermissionToRoleAsync(
                roleId, request.PermissionId, ActorUserId);
            return Ok(grant);
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { error = "The grant changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant permission {PermId} to role {RoleId}",
                request?.PermissionId, roleId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Revoke a permission from a role by grant record ID.</summary>
    [HttpDelete("permissions/{rolePermissionId}")]
    public async Task<IActionResult> RevokePermission(string rolePermissionId)
    {
        if (string.IsNullOrWhiteSpace(ActorUserId)) return Forbid();
        if (string.IsNullOrWhiteSpace(rolePermissionId))
            return BadRequest(new { error = "RolePermissionId is required." });
        try
        {
            var ok = await _roleService.RevokePermissionFromRoleAsync(rolePermissionId, ActorUserId);
            if (!ok) return NotFound(new { message = $"Grant {rolePermissionId} not found." });
            return Ok(new { message = "Permission grant revoked." });
        }
        catch (ArgumentException exception) { return BadRequest(new { error = exception.Message }); }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException) { return Conflict(new { error = "The grant changed. Reload before retrying." }); }
        catch (InvalidOperationException exception) { return Conflict(new { error = exception.Message }); }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke permission grant {RolePermissionId}", rolePermissionId);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }
}
