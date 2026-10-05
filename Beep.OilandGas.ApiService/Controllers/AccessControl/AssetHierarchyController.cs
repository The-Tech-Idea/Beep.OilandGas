using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Controllers.AccessControl
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class AssetHierarchyController : ControllerBase
    {
        private readonly IAssetHierarchyService _assetHierarchyService;
        private bool IsLocalUser => User.Identity?.IsAuthenticated == true
            && !string.IsNullOrWhiteSpace(User.FindActingUserId());
        private bool IsAdministrator => IsLocalUser && User.IsInRole("Administrator");
        // Self or Administrator: the route/body id names the subject whose hierarchy is read, never the actor.
        private bool CanAccess(string targetUserId) => IsLocalUser
            && (User.FindActingUserId() == targetUserId || IsAdministrator);

        public AssetHierarchyController(IAssetHierarchyService assetHierarchyService)
        {
            _assetHierarchyService = assetHierarchyService;
        }

        /// <summary>
        /// Get the asset hierarchy for an organization
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<AssetHierarchyNode>> GetAssetHierarchy(
            string organizationId,
            [FromQuery] string? rootAssetId = null,
            [FromQuery] string? rootAssetType = null)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(organizationId))
                return BadRequest(new { error = "Organization ID is required." });
            var hierarchy = await _assetHierarchyService.GetAssetHierarchyAsync(
                organizationId, rootAssetId, rootAssetType);
            
            if (hierarchy == null)
                return NotFound(new { message = "Hierarchy not found" });
            
            return Ok(hierarchy);
        }

        /// <summary>
        /// Get the asset hierarchy filtered by user access
        /// </summary>
        [HttpGet("user/{targetUserId}")]
        public async Task<ActionResult<AssetHierarchyNode>> GetAssetHierarchyForUser(
            string targetUserId,
            [FromQuery] string? organizationId = null,
            [FromQuery] string? rootAssetId = null,
            [FromQuery] string? rootAssetType = null)
        {
            if (!CanAccess(targetUserId)) return Forbid();
            if (string.IsNullOrWhiteSpace(targetUserId))
                return BadRequest(new { error = "User ID is required." });
            var hierarchy = await _assetHierarchyService.GetAssetHierarchyForUserAsync(
                targetUserId, organizationId, rootAssetId, rootAssetType);
            
            if (hierarchy == null)
                return NotFound(new { message = "Hierarchy not found" });
            
            return Ok(hierarchy);
        }

        /// <summary>
        /// Get child assets of a given asset
        /// </summary>
        [HttpGet("asset/{assetId}/{assetType}/children")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<AssetHierarchyNode>>> GetAssetChildren(
            string assetId,
            string assetType,
            [FromQuery] string? organizationId = null)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(assetId))
                return BadRequest(new { error = "Asset ID is required." });
            if (string.IsNullOrWhiteSpace(assetType))
                return BadRequest(new { error = "Asset type is required." });
            var children = await _assetHierarchyService.GetAssetChildrenAsync(assetId, assetType, organizationId);
            return Ok(children);
        }

        /// <summary>
        /// Get the full path from root to a given asset
        /// </summary>
        [HttpGet("asset/{assetId}/{assetType}/path")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<AssetHierarchyNode>>> GetAssetPath(
            string assetId,
            string assetType,
            [FromQuery] string? organizationId = null)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(assetId))
                return BadRequest(new { error = "Asset ID is required." });
            if (string.IsNullOrWhiteSpace(assetType))
                return BadRequest(new { error = "Asset type is required." });
            var path = await _assetHierarchyService.GetAssetPathAsync(assetId, assetType, organizationId);
            return Ok(path);
        }

        /// <summary>
        /// Validate if a user has access to an asset path
        /// </summary>
        [HttpPost("validate-access")]
        public async Task<ActionResult<bool>> ValidateAccess([FromBody] ValidateAccessRequest request)
        {
            if (!CanAccess(request.UserId)) return Forbid();
            var result = await _assetHierarchyService.ValidateAccessAsync(request.UserId, request.AssetPath);
            return Ok(result);
        }

        /// <summary>
        /// Get the hierarchy configuration for an organization
        /// </summary>
        [HttpGet("organization/{organizationId}/config")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<List<HierarchyConfig>>> GetHierarchyConfig(string organizationId)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(organizationId))
                return BadRequest(new { error = "Organization ID is required." });
            var config = await _assetHierarchyService.GetHierarchyConfigAsync(organizationId);
            return Ok(config);
        }

        /// <summary>
        /// Update the hierarchy configuration for an organization
        /// </summary>
        [HttpPut("organization/{organizationId}/config")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<bool>> UpdateHierarchyConfig(
            string organizationId,
            [FromBody] List<HierarchyConfig> config)
        {
            if (!IsAdministrator) return Forbid();
            if (string.IsNullOrWhiteSpace(organizationId))
                return BadRequest(new { error = "Organization ID is required." });
            var result = await _assetHierarchyService.UpdateHierarchyConfigAsync(organizationId, config);
            return Ok(result);
        }
    }
}
