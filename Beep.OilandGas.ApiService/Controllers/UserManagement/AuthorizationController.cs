using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.Core.Interfaces.Security;
using Beep.OilandGas.UserManagement.Core.Authorization;

namespace Beep.OilandGas.ApiService.Controllers.UserManagement
{
    [ApiController]
    [Route("api/usermanagement/[controller]")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IPermissionEvaluator _permissionEvaluator;
        private readonly IUserManagementService _userManagement;

        public AuthorizationController(
            IAuthorizationService authorizationService,
            IPermissionEvaluator permissionEvaluator,
            IUserManagementService userManagement)
        {
            _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
            _permissionEvaluator = permissionEvaluator ?? throw new ArgumentNullException(nameof(permissionEvaluator));
            _userManagement = userManagement ?? throw new ArgumentNullException(nameof(userManagement));
        }

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        [HttpPost("check-permission")]
        public async Task<ActionResult<AuthorizationResult>> CheckPermission([FromBody] CheckPermissionRequest request)
        {
            try
            {
                var result = await _permissionEvaluator.EvaluateAsync(request.UserId, request.Permission);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user has any of the specified permissions
        /// </summary>
        [HttpPost("check-permission-any")]
        public async Task<ActionResult<AuthorizationResult>> CheckPermissionAny([FromBody] CheckPermissionAnyRequest request)
        {
            try
            {
                var result = await _permissionEvaluator.EvaluateAnyAsync(request.UserId, request.Permissions);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user has all of the specified permissions
        /// </summary>
        [HttpPost("check-permission-all")]
        public async Task<ActionResult<AuthorizationResult>> CheckPermissionAll([FromBody] CheckPermissionAllRequest request)
        {
            try
            {
                var result = await _permissionEvaluator.EvaluateAllAsync(request.UserId, request.Permissions);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user is in a specific role
        /// </summary>
        [HttpPost("check-role")]
        public async Task<ActionResult<bool>> CheckRole([FromBody] CheckRoleRequest request)
        {
            try
            {
                var result = await _authorizationService.UserIsInRoleAsync(request.UserId, request.RoleName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all permissions for a user
        /// </summary>
        [HttpGet("user/{userId}/permissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions(string userId)
        {
            try
            {
                var permissions = await _userManagement.GetUserPermissionsAsync(userId);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        [HttpGet("user/{userId}/roles")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _userManagement.GetUserRolesAsync(userId);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request DTOs
    public class CheckPermissionRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
    }

    public class CheckPermissionAnyRequest
    {
        public string UserId { get; set; } = string.Empty;
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    }

    public class CheckPermissionAllRequest
    {
        public string UserId { get; set; } = string.Empty;
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    }

    public class CheckRoleRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
