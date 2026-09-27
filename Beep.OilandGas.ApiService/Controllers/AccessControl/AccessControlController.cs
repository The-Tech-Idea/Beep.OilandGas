using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.ApiService.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Controllers.AccessControl
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AccessControlController : ControllerBase
    {
        private readonly IAccessControlService _accessControlService;
        private bool IsLocalUser => User.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(User.FindActingUserId());
        private bool IsAdministrator => IsLocalUser && User.IsInRole("Administrator");
        // Self or Administrator: the route/body id names the subject whose access is read, never the actor.
        private bool CanAccess(string targetUserId) => IsLocalUser
            && (User.FindActingUserId() == targetUserId || IsAdministrator);

        public AccessControlController(IAccessControlService accessControlService)
        {
            _accessControlService = accessControlService;
        }

        /// <summary>
        /// Check if a user has access to a specific asset
        /// </summary>
        [HttpPost("check-access")]
        public async Task<ActionResult<AccessCheckResponse>> CheckAssetAccess([FromBody] AccessCheckRequest request)
        {
            if (!CanAccess(request.UserId)) return Forbid();
            try
            {
                var response = await _accessControlService.CheckAssetAccessAsync(
                    request.UserId, 
                    request.AssetId, 
                    request.AssetType, 
                    request.RequiredPermission);
                return Ok(response);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get all assets a user can access
        /// </summary>
        [HttpGet("user/{targetUserId}/assets")]
        public async Task<ActionResult<List<AssetAccess>>> GetUserAccessibleAssets(
            string targetUserId, 
            [FromQuery] string? assetType = null, 
            [FromQuery] string? organizationId = null,
            [FromQuery] bool includeInherited = true)
        {
            if (!CanAccess(targetUserId)) return Forbid();
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var assets = await _accessControlService.GetUserAccessibleAssetsAsync(
                    targetUserId, assetType, organizationId, includeInherited);
                return Ok(assets);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        [HttpGet("user/{targetUserId}/roles")]
        public async Task<ActionResult<List<string>>> GetUserRoles(
            string targetUserId, 
            [FromQuery] string? organizationId = null)
        {
            if (!CanAccess(targetUserId)) return Forbid();
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            try
            {
                var roles = await _accessControlService.GetUserRolesAsync(targetUserId, organizationId);
                return Ok(roles);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        [HttpGet("user/{targetUserId}/permission/{permissionId}")]
        public async Task<ActionResult<bool>> HasPermission(
            string targetUserId, 
            string permissionId, 
            [FromQuery] string? organizationId = null)
        {
            if (!CanAccess(targetUserId)) return Forbid();
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            if (string.IsNullOrWhiteSpace(permissionId))
                return BadRequest(new { error = "Permission ID is required." });
            try
            {
                var hasPermission = await _accessControlService.HasPermissionAsync(targetUserId, permissionId, organizationId);
                return Ok(hasPermission);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Grant access to an asset for a user
        /// </summary>
        [HttpPost("grant-access")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<bool>> GrantAssetAccess([FromBody] GrantAccessRequest request)
        {
            if (!IsAdministrator) return Forbid();
            try
            {
                var result = await _accessControlService.GrantAssetAccessAsync(
                    request.UserId, 
                    request.AssetId, 
                    request.AssetType, 
                    request.AccessLevel, 
                    request.Inherit, 
                    request.OrganizationId);
                return Ok(result);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Revoke access to an asset for a user
        /// </summary>
        [HttpPost("revoke-access")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<bool>> RevokeAssetAccess([FromBody] RevokeAccessRequest request)
        {
            if (!IsAdministrator) return Forbid();
            try
            {
                var result = await _accessControlService.RevokeAssetAccessAsync(
                    request.UserId, 
                    request.AssetId, 
                    request.AssetType);
                return Ok(result);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get all permissions for a role
        /// </summary>
        [HttpGet("role/{roleId}/permissions")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<string>>> GetRolePermissions(
            string roleId, 
            [FromQuery] string? organizationId = null)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(roleId))
                return BadRequest(new { error = "Role ID is required." });
            try
            {
                var permissions = await _accessControlService.GetRolePermissionsAsync(roleId, organizationId);
                return Ok(permissions);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Assign a permission to a role
        /// </summary>
        [HttpPost("role/{roleId}/permission/{permissionId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<bool>> AssignPermissionToRole(
            string roleId, 
            string permissionId, 
            [FromQuery] string? organizationId = null)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(roleId))
                return BadRequest(new { error = "Role ID is required." });
            if (string.IsNullOrWhiteSpace(permissionId))
                return BadRequest(new { error = "Permission ID is required." });
            try
            {
                var result = await _accessControlService.AssignPermissionToRoleAsync(roleId, permissionId, organizationId);
                return Ok(result);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Remove a permission from a role
        /// </summary>
        [HttpDelete("role/{roleId}/permission/{permissionId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<bool>> RemovePermissionFromRole(
            string roleId, 
            string permissionId, 
            [FromQuery] string? organizationId = null)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(roleId))
                return BadRequest(new { error = "Role ID is required." });
            if (string.IsNullOrWhiteSpace(permissionId))
                return BadRequest(new { error = "Permission ID is required." });
            try
            {
                var result = await _accessControlService.RemovePermissionFromRoleAsync(roleId, permissionId, organizationId);
                return Ok(result);
            }
            catch (System.Exception)
            {
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }
}
