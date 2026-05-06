using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.Models.Data.Security;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.Identity;

/// <summary>
/// User management controller for CRUD operations on users.
/// Implements Oil & Gas industry best practices for user lifecycle management.
/// </summary>
[ApiController]
[Route("api/identity/users")]
[Authorize]
public class UserManagementController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoleAssignmentService _roleService;
    private readonly ILogger<UserManagementController> _logger;

    public UserManagementController(
        IUserService userService,
        IRoleAssignmentService roleService,
        ILogger<UserManagementController> logger)
    {
        _userService = userService;
        _roleService = roleService;
        _logger = logger;
    }

    private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";

    // ── User CRUD ───────────────────────────────────────────────────────────

    /// <summary>Get all users.</summary>
    [HttpGet]
    [Authorize(Policy = "Admin.ManageUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users.Select(MapToUserResponse));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get all users");
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Get a user by ID.</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(string id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { error = "User not found." });

            var roles = await _userService.GetRolesAsync(id);
            return Ok(new
            {
                user.USER_ID,
                user.USER_NAME,
                user.EMAIL,
                user.FULL_NAME,
                user.TENANT_ID,
                user.BUSINESS_ASSOCIATE_ID,
                user.ACTIVE_IND,
                user.LAST_LOGIN_UTC,
                user.LAST_PASSWORD_CHANGE_UTC,
                user.FAILED_LOGIN_COUNT,
                user.LOCKED_IND,
                user.LOCKOUT_UNTIL_UTC,
                Roles = roles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user {UserId}", id);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Get a user by username.</summary>
    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        try
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null) return NotFound(new { error = "User not found." });

            return Ok(MapToUserResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get user by username {Username}", username);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Create a new user. Requires admin privileges.</summary>
    [HttpPost]
    [Authorize(Policy = "Admin.ManageUsers")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { error = "Username and password are required." });
        }

        try
        {
            var existing = await _userService.GetByUsernameAsync(request.Username);
            if (existing != null)
            {
                return Conflict(new { error = "Username already exists." });
            }

            var user = new USER
            {
                USER_ID = Guid.NewGuid().ToString(),
                USER_NAME = request.Username,
                EMAIL = request.Email,
                FULL_NAME = request.FullName,
                TENANT_ID = request.TenantId,
                BUSINESS_ASSOCIATE_ID = request.BusinessAssociateId,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = CurrentUserId,
                ROW_CREATED_DATE = DateTime.UtcNow,
                ROW_CHANGED_BY = CurrentUserId,
                ROW_CHANGED_DATE = DateTime.UtcNow,
                ROW_EFFECTIVE_DATE = DateTime.UtcNow
            };

            var created = await _userService.CreateAsync(user, request.Password);

            if (request.Roles?.Any() == true)
            {
                foreach (var role in request.Roles)
                {
                    await _userService.AddToRoleAsync(created.USER_ID, role);
                }
            }

            _logger.LogInformation("User created: {Username} by {Actor}", request.Username, CurrentUserId);
            return CreatedAtAction(nameof(GetUser), new { id = created.USER_ID }, MapToUserResponse(created));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Username}", request.Username);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Update a user. Requires admin privileges or own profile.</summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null) return NotFound(new { error = "User not found." });

            if (!IsAdmin() && CurrentUserId != id)
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(request.Email)) user.EMAIL = request.Email;
            if (!string.IsNullOrWhiteSpace(request.FullName)) user.FULL_NAME = request.FullName;
            if (!string.IsNullOrWhiteSpace(request.ActiveInd)) user.ACTIVE_IND = request.ActiveInd;

            user.ROW_CHANGED_BY = CurrentUserId;
            user.ROW_CHANGED_DATE = DateTime.UtcNow;

            await _userService.UpdateAsync(user);

            _logger.LogInformation("User updated: {UserId} by {Actor}", id, CurrentUserId);
            return Ok(MapToUserResponse(user));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user {UserId}", id);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Deactivate a user. Requires admin privileges.</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin.ManageUsers")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        try
        {
            var result = await _userService.DeleteAsync(id);
            if (!result) return NotFound(new { error = "User not found." });

            _logger.LogInformation("User deactivated: {UserId} by {Actor}", id, CurrentUserId);
            return Ok(new { message = "User deactivated successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate user {UserId}", id);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    // ── Role Assignment ─────────────────────────────────────────────────────

    /// <summary>Get roles for a user.</summary>
    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetUserRoles(string id)
    {
        try
        {
            var roles = await _userService.GetRolesAsync(id);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get roles for user {UserId}", id);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Add a role to a user. Requires admin privileges.</summary>
    [HttpPost("{id}/roles")]
    [Authorize(Policy = "Admin.AssignRoles")]
    public async Task<IActionResult> AddRole(string id, [FromBody] RoleAssignmentRequest request)
    {
        try
        {
            var result = await _userService.AddToRoleAsync(id, request.RoleName);
            if (!result) return BadRequest(new { error = "Failed to assign role. Role may not exist." });

            _logger.LogInformation("Role {Role} assigned to user {UserId} by {Actor}", request.RoleName, id, CurrentUserId);
            return Ok(new { message = "Role assigned successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign role to user {UserId}", id);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    /// <summary>Remove a role from a user. Requires admin privileges.</summary>
    [HttpDelete("{id}/roles/{roleName}")]
    [Authorize(Policy = "Admin.AssignRoles")]
    public async Task<IActionResult> RemoveRole(string id, string roleName)
    {
        try
        {
            var result = await _userService.RemoveFromRoleAsync(id, roleName);
            if (!result) return NotFound(new { error = "Role assignment not found." });

            _logger.LogInformation("Role {Role} removed from user {UserId} by {Actor}", roleName, id, CurrentUserId);
            return Ok(new { message = "Role removed successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove role from user {UserId}", id);
            return StatusCode(500, new { error = "An internal error occurred." });
        }
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private bool IsAdmin()
    {
        return User.IsInRole("Administrator") || User.IsInRole("Admin");
    }

    private static object MapToUserResponse(USER user)
    {
        return new
        {
            user.USER_ID,
            user.USER_NAME,
            user.EMAIL,
            user.FULL_NAME,
            user.TENANT_ID,
            user.BUSINESS_ASSOCIATE_ID,
            user.ACTIVE_IND,
            user.LAST_LOGIN_UTC,
            user.LAST_PASSWORD_CHANGE_UTC,
            user.FAILED_LOGIN_COUNT,
            user.LOCKED_IND,
            user.LOCKOUT_UNTIL_UTC,
            user.ROW_CREATED_DATE,
            user.ROW_CHANGED_DATE
        };
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record CreateUserRequest(
    string Username,
    string Password,
    string? Email,
    string? FullName,
    string? TenantId,
    string? BusinessAssociateId,
    string[]? Roles);

public record UpdateUserRequest(
    string? Email,
    string? FullName,
    string? ActiveInd);

public record RoleAssignmentRequest(string RoleName);
