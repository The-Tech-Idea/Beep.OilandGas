using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.PPDM39.Core.DTOs;
using Beep.OilandGas.LifeCycle.Services.AccessControl;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beep.OilandGas.ApiService.Controllers.AccessControl
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssetHierarchyController : ControllerBase
    {
        private readonly IAssetHierarchyService _assetHierarchyService;

        public AssetHierarchyController(IAssetHierarchyService assetHierarchyService)
        {
            _assetHierarchyService = assetHierarchyService;
        }

        /// <summary>
        /// Get the asset hierarchy for an organization
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<AssetHierarchyNode>> GetAssetHierarchy(
            string organizationId,
            [FromQuery] string? rootAssetId = null,
            [FromQuery] string? rootAssetType = null)
        {
            try
            {
                var hierarchy = await _assetHierarchyService.GetAssetHierarchyAsync(
                    organizationId, rootAssetId, rootAssetType);
                
                if (hierarchy == null)
                    return NotFound(new { message = "Hierarchy not found" });
                
                return Ok(hierarchy);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get the asset hierarchy filtered by user access
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<AssetHierarchyNode>> GetAssetHierarchyForUser(
            string userId,
            [FromQuery] string? organizationId = null,
            [FromQuery] string? rootAssetId = null,
            [FromQuery] string? rootAssetType = null)
        {
            try
            {
                var hierarchy = await _assetHierarchyService.GetAssetHierarchyForUserAsync(
                    userId, organizationId, rootAssetId, rootAssetType);
                
                if (hierarchy == null)
                    return NotFound(new { message = "Hierarchy not found" });
                
                return Ok(hierarchy);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get child assets of a given asset
        /// </summary>
        [HttpGet("asset/{assetId}/{assetType}/children")]
        public async Task<ActionResult<List<AssetHierarchyNode>>> GetAssetChildren(
            string assetId,
            string assetType,
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var children = await _assetHierarchyService.GetAssetChildrenAsync(assetId, assetType, organizationId);
                return Ok(children);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get the full path from root to a given asset
        /// </summary>
        [HttpGet("asset/{assetId}/{assetType}/path")]
        public async Task<ActionResult<List<AssetHierarchyNode>>> GetAssetPath(
            string assetId,
            string assetType,
            [FromQuery] string? organizationId = null)
        {
            try
            {
                var path = await _assetHierarchyService.GetAssetPathAsync(assetId, assetType, organizationId);
                return Ok(path);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Validate if a user has access to an asset path
        /// </summary>
        [HttpPost("validate-access")]
        public async Task<ActionResult<bool>> ValidateAccess([FromBody] ValidateAccessRequest request)
        {
            try
            {
                var result = await _assetHierarchyService.ValidateAccessAsync(request.UserId, request.AssetPath);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get the hierarchy configuration for an organization
        /// </summary>
        [HttpGet("organization/{organizationId}/config")]
        public async Task<ActionResult<List<HierarchyConfigDTO>>> GetHierarchyConfig(string organizationId)
        {
            try
            {
                var config = await _assetHierarchyService.GetHierarchyConfigAsync(organizationId);
                return Ok(config);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update the hierarchy configuration for an organization
        /// </summary>
        [HttpPut("organization/{organizationId}/config")]
        public async Task<ActionResult<bool>> UpdateHierarchyConfig(
            string organizationId,
            [FromBody] List<HierarchyConfigDTO> config)
        {
            try
            {
                var result = await _assetHierarchyService.UpdateHierarchyConfigAsync(organizationId, config);
                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // Request DTO for controller actions
    public class ValidateAccessRequest
    {
        public string UserId { get; set; } = string.Empty;
        public List<AssetHierarchyNode> AssetPath { get; set; } = new List<AssetHierarchyNode>();
    }
}
