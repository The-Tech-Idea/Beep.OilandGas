using Beep.OilandGas.ApiService.Data;
using Beep.OilandGas.ApiService.Services;
using Beep.Foundation.IdentityServer.Shared.Authentication;
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
        await _roleService.RequireRepositoryAdministratorAsync(User.ActingUserId(), HttpContext.RequestAborted);
        return Ok(await _roleService.GetRoleCatalogAsync()); 
    }

    /// <summary>Get the full permission catalog.</summary>
    [HttpGet("permissions/catalog")]
    public async Task<IActionResult> GetPermissionCatalog()
    {
        await _roleService.RequireRepositoryAdministratorAsync(User.ActingUserId(), HttpContext.RequestAborted);
        return Ok(await _roleService.GetPermissionCatalogAsync()); 
    }

    // ── Role assignments ─────────────────────────────────────────────────────

    /// <summary>Get all active role assignments for a user (Administrator; the route id names the subject).</summary>
    [HttpGet("users/{targetUserId}/assignments")]
    public async Task<IActionResult> GetUserRoleAssignments(string targetUserId)
    {
        await _roleService.RequireRepositoryAdministratorAsync(User.ActingUserId(), HttpContext.RequestAborted);
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

    // Repository administration is independently checked by the service. Scope in the URL is a selector, not authority.
    [HttpGet("tenants/{tenantId}/companies/{businessAssociateId}/users/{targetUserId}/access")]
    public async Task<IActionResult> ReviewCompanyMemberAccess(string tenantId, string businessAssociateId,
        string targetUserId, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        return Ok(await _roleService.ReviewCompanyMemberAccessAsync(new(tenantId, businessAssociateId), targetUserId,
            User.ActingUserId(), cancellationToken));
    }
    [HttpPost("tenants/{tenantId}/companies/{businessAssociateId}/memberships")]
    public async Task<IActionResult> CreateCompanyMembership(string tenantId, string businessAssociateId,
        [FromBody] CompanyMembershipRequest request, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        if (request is null) return BadRequest();
        try
        {
            return Ok(await _roleService.CreateCompanyMembershipAsync(new(tenantId, businessAssociateId), request.UserId,
                request.RoleId, request.Reason, request.IntentId, User.ActingUserId(), cancellationToken));
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "creating a company membership"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "creating a company membership"); }
    }

    [HttpGet("tenants/{tenantId}/companies/{businessAssociateId}/users/{targetUserId}/assignments")]
    public async Task<IActionResult> GetCompanyAssignments(string tenantId, string businessAssociateId,
        string targetUserId, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        return Ok(await _roleService.GetCompanyRoleAssignmentsAsync(new(tenantId, businessAssociateId), targetUserId,
            User.ActingUserId(), cancellationToken));
    }

    [HttpPost("tenants/{tenantId}/companies/{businessAssociateId}/users/{targetUserId}/membership-retirements")]
    public async Task<IActionResult> RetireCompanyMembership(string tenantId, string businessAssociateId,
        string targetUserId, [FromBody] CompanyMembershipRetirementRequest request, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        try
        {
            return Ok(await _roleService.RetireCompanyMembershipAsync(new(tenantId, businessAssociateId), targetUserId,
                request, User.ActingUserId(), cancellationToken));
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "retiring a company membership"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "retiring a company membership"); }
    }

    [HttpGet("tenants/{tenantId}/companies/{businessAssociateId}/users/{targetUserId}/membership-retirements")]
    public async Task<IActionResult> ReviewMembershipRetirements(string tenantId, string businessAssociateId,
        string targetUserId, [FromQuery] int offset = 0, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        if (User.IsMachine()) return Forbid();
        return Ok(await _roleService.ReviewMembershipRetirementsAsync(new(tenantId, businessAssociateId), targetUserId,
            User.ActingUserId(), offset, limit, cancellationToken));
    }

    [HttpPost("tenants/{tenantId}/companies/{businessAssociateId}/users/{targetUserId}/assignments")]
    public async Task<IActionResult> AssignCompanyRole(string tenantId, string businessAssociateId,
        string targetUserId, [FromBody] AssignCompanyRoleRequest request, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        try
        {
            return Ok(await _roleService.AssignCompanyRoleAsync(new(tenantId, businessAssociateId), targetUserId,
                request, User.ActingUserId(), cancellationToken));
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "assigning a company role"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "assigning a company role"); }
    }

    [HttpDelete("tenants/{tenantId}/companies/{businessAssociateId}/assignments/{userRoleId}")]
    public async Task<IActionResult> RevokeCompanyRole(string tenantId, string businessAssociateId,
        string userRoleId, CancellationToken cancellationToken)
    {
        if (User.IsMachine()) return Forbid();
        try
        {
            return await _roleService.RevokeCompanyRoleAsync(new(tenantId, businessAssociateId), userRoleId,
                User.ActingUserId(), cancellationToken) ? NoContent() : NotFound();
        }
        catch (DbUpdateConcurrencyException changed) { return GrantChanged(changed, "revoking a company role"); }
        catch (DbUpdateException duplicate) when (UniqueKeyViolation.Is(duplicate)) { return GrantChanged(duplicate, "revoking a company role"); }
    }

    /// <summary>Get all active permissions for a role.</summary>
    [HttpGet("{roleId}/permissions")]
    public async Task<IActionResult> GetRolePermissions(string roleId)
    {
        await _roleService.RequireRepositoryAdministratorAsync(User.ActingUserId(), HttpContext.RequestAborted);
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
