using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Models.DTOs;
using Beep.OilandGas.Models.DTOs.AccessControl;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using TheTechIdea.Beep.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Controllers.AccessControl
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessControlController : ControllerBase
    {
        private readonly IAccessControlService _accessControlService;

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
            try
            {
                var response = await _accessControlService.CheckAssetAccessAsync(
                    request.UserId, 
                    request.AssetId, 
                    request.AssetType, 
                    request.RequiredPermission);
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all assets a user can access
        /// </summary>
        [HttpGet("user/{userId}/assets")]
        public async Task<ActionResult<List<AssetAccessDTO>>> GetUserAccessibleAssets(
            string userId, 
            [FromQuery] string? assetType = null, 
            [FromQuery] string? organizationId = null,
            [FromQuery] bool includeInherited = true)
        {
            try
            {
                var assets = await _accessControlService.GetUserAccessibleAssetsAsync(
                    userId, assetType, organizationId, includeInherited);
                return Ok(assets);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all roles for a user
        /// </summary>
        [HttpGet("user/{userId}/roles")]
        public async Task<ActionResult<List<string>>> GetUserRoles(
            string userId, 
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var roles = await _accessControlService.GetUserRolesAsync(userId, organizationId);
                return Ok(roles);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        [HttpGet("user/{userId}/permission/{permissionId}")]
        public async Task<ActionResult<bool>> HasPermission(
            string userId, 
            string permissionId, 
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var hasPermission = await _accessControlService.HasPermissionAsync(userId, permissionId, organizationId);
                return Ok(hasPermission);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Grant access to an asset for a user
        /// </summary>
        [HttpPost("grant-access")]
        public async Task<ActionResult<bool>> GrantAssetAccess([FromBody] GrantAccessRequest request)
        {
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
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Revoke access to an asset for a user
        /// </summary>
        [HttpPost("revoke-access")]
        public async Task<ActionResult<bool>> RevokeAssetAccess([FromBody] RevokeAccessRequest request)
        {
            try
            {
                var result = await _accessControlService.RevokeAssetAccessAsync(
                    request.UserId, 
                    request.AssetId, 
                    request.AssetType);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all permissions for a role
        /// </summary>
        [HttpGet("role/{roleId}/permissions")]
        public async Task<ActionResult<List<string>>> GetRolePermissions(
            string roleId, 
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var permissions = await _accessControlService.GetRolePermissionsAsync(roleId, organizationId);
                return Ok(permissions);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Assign a permission to a role
        /// </summary>
        [HttpPost("role/{roleId}/permission/{permissionId}")]
        public async Task<ActionResult<bool>> AssignPermissionToRole(
            string roleId, 
            string permissionId, 
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var result = await _accessControlService.AssignPermissionToRoleAsync(roleId, permissionId, organizationId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Remove a permission from a role
        /// </summary>
        [HttpDelete("role/{roleId}/permission/{permissionId}")]
        public async Task<ActionResult<bool>> RemovePermissionFromRole(
            string roleId, 
            string permissionId, 
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var result = await _accessControlService.RemovePermissionFromRoleAsync(roleId, permissionId, organizationId);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
