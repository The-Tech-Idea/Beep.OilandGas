using System;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 data versioning operations
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/versioning")]
    public class PPDM39VersioningController : ControllerBase
    {
        private readonly IPPDMDataVersioningService _versioningService;
        private readonly ILogger<PPDM39VersioningController> _logger;

        public PPDM39VersioningController(
            IPPDMDataVersioningService versioningService,
            ILogger<PPDM39VersioningController> logger)
        {
            _versioningService = versioningService ?? throw new ArgumentNullException(nameof(versioningService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a version snapshot of an entity
        /// </summary>
        [HttpPost("{tableName}/{entityId}/create-version")]
        public async Task<ActionResult<VersioningResult>> CreateVersion(
            string tableName,
            string entityId,
            [FromBody] VersioningRequest request,
            [FromQuery] string userId = "SYSTEM")
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new VersioningResult { Success = false, ErrorMessage = "Request is required" });
                }

                _logger.LogInformation("Creating version for entity {EntityId} in table {TableName}", entityId, tableName);

                // Retrieve the entity first - in a real implementation, this would use a repository
                // For now, we'll pass the entityId as the entity object
                // The versioning service will handle the entity retrieval internally
                var version = await _versioningService.CreateVersionAsync(
                    tableName,
                    entityId,
                    userId,
                    request.VersionLabel);

                return Ok(new VersioningResult
                {
                    Success = true,
                    VersionId = version.VersionNumber.ToString(),
                    Message = "Version created successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating version for entity {EntityId} in table {TableName}", entityId, tableName);
                return StatusCode(500, new VersioningResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        /// <summary>
        /// Get version history for an entity
        /// </summary>
        [HttpGet("{tableName}/{entityId}/versions")]
        public async Task<ActionResult> GetVersionHistory(string tableName, string entityId, [FromQuery] string? connectionName = null)
        {
            try
            {
                _logger.LogInformation("Getting version history for entity {EntityId} in table {TableName}", entityId, tableName);
                var versions = await _versioningService.GetVersionsAsync(tableName, entityId);
                
                var versionInfos = versions?.Select(v => new VersionInfo
                {
                    VersionId = v.VersionNumber.ToString(),
                    CreatedAt = v.CreatedDate,
                    CreatedBy = v.CreatedBy ?? string.Empty,
                    Description = v.VersionLabel ?? v.ChangeDescription ?? string.Empty,
                    EntityData = v.EntityData as System.Collections.Generic.Dictionary<string, object>
                }).ToList() ?? new System.Collections.Generic.List<VersionInfo>();

                return Ok(versionInfos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for entity {EntityId} in table {TableName}", entityId, tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific version of an entity
        /// </summary>
        [HttpGet("{tableName}/{entityId}/versions/{versionNumber}")]
        public async Task<ActionResult<VersionInfo>> GetVersion(string tableName, string entityId, int versionNumber, [FromQuery] string? connectionName = null)
        {
            try
            {
                _logger.LogInformation("Getting version {VersionNumber} for entity {EntityId} in table {TableName}", versionNumber, entityId, tableName);
                var version = await _versioningService.GetVersionAsync(tableName, entityId, versionNumber);
                
                if (version == null)
                {
                    return NotFound(new { error = "Version not found" });
                }

                var versionInfo = new VersionInfo
                {
                    VersionId = version.VersionNumber.ToString(),
                    CreatedAt = version.CreatedDate,
                    CreatedBy = version.CreatedBy ?? string.Empty,
                    Description = version.VersionLabel ?? version.ChangeDescription ?? string.Empty,
                    EntityData = version.EntityData as System.Collections.Generic.Dictionary<string, object>
                };

                return Ok(versionInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version {VersionNumber} for entity {EntityId} in table {TableName}", versionNumber, entityId, tableName);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Restore an entity to a specific version
        /// </summary>
        [HttpPost("{tableName}/{entityId}/restore")]
        public async Task<ActionResult<VersioningResult>> RestoreVersion(
            string tableName,
            string entityId,
            [FromBody] RestoreVersionRequest request,
            [FromQuery] string userId = "SYSTEM")
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.VersionId))
                {
                    return BadRequest(new VersioningResult { Success = false, ErrorMessage = "Version ID is required" });
                }

                _logger.LogInformation("Restoring entity {EntityId} in table {TableName} to version {VersionId}", entityId, tableName, request.VersionId);

                // Parse version ID (could be version number or GUID)
                if (int.TryParse(request.VersionId, out int versionNumber))
                {
                    var restoreResult = await _versioningService.RestoreToVersionAsync(tableName, entityId, versionNumber, userId);
                    if (!restoreResult.Success)
                    {
                        return BadRequest(new VersioningResult { Success = false, ErrorMessage = restoreResult.Message ?? "Failed to restore version" });
                    }
                }
                else
                {
                    // If version ID is not a number, try to find version by ID
                    var versions = await _versioningService.GetVersionsAsync(tableName, entityId);
                    var version = versions?.FirstOrDefault(v => v.VersionNumber.ToString() == request.VersionId);
                    if (version != null)
                    {
                        var restoreResult = await _versioningService.RestoreToVersionAsync(tableName, entityId, version.VersionNumber, userId);
                        if (!restoreResult.Success)
                        {
                            return BadRequest(new VersioningResult { Success = false, ErrorMessage = restoreResult.Message ?? "Failed to restore version" });
                        }
                    }
                    else
                    {
                        return NotFound(new VersioningResult { Success = false, ErrorMessage = "Version not found" });
                    }
                }

                return Ok(new VersioningResult
                {
                    Success = true,
                    VersionId = request.VersionId,
                    Message = "Version restored successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring version for entity {EntityId} in table {TableName}", entityId, tableName);
                return StatusCode(500, new VersioningResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }
    }
}
