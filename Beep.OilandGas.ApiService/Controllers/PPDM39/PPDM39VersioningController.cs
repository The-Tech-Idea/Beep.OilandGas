using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 data versioning operations
    /// </summary>
    [Authorize]
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
            [FromBody] VersioningRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required." });
            if (string.IsNullOrWhiteSpace(entityId))
                    return BadRequest(new { error = "Entity ID is required." });
            if (request == null)
            {
                    return BadRequest(new { error = "Request body is required." });
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

        /// <summary>
        /// Get version history for an entity
        /// </summary>
        [HttpGet("{tableName}/{entityId}/versions")]
        public async Task<ActionResult> GetVersionHistory(string tableName, string entityId, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            if (string.IsNullOrWhiteSpace(entityId))
                return BadRequest(new { error = "Entity ID is required." });
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

        /// <summary>
        /// Get a specific version of an entity
        /// </summary>
        [HttpGet("{tableName}/{entityId}/versions/{versionNumber}")]
        public async Task<ActionResult<VersionInfo>> GetVersion(string tableName, string entityId, int versionNumber, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest(new { error = "Table name is required." });
            if (string.IsNullOrWhiteSpace(entityId))
                return BadRequest(new { error = "Entity ID is required." });
            _logger.LogInformation("Getting version {VersionNumber} for entity {EntityId} in table {TableName}", versionNumber, entityId, tableName);
            var version = await _versioningService.GetVersionAsync(tableName, entityId, versionNumber);
            
            if (version == null)
            {
                return NotFound(new { error = "Version not found." });
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

        /// <summary>
        /// Restore an entity to a specific version
        /// </summary>
        [HttpPost("{tableName}/{entityId}/restore")]
        public async Task<ActionResult<VersioningResult>> RestoreVersion(
            string tableName,
            string entityId,
            [FromBody] RestoreVersionRequest request)
        {
            var userId = User.ActingUserId();
            if (string.IsNullOrWhiteSpace(tableName))
                    return BadRequest(new { error = "Table name is required." });
            if (string.IsNullOrWhiteSpace(entityId))
                    return BadRequest(new { error = "Entity ID is required." });
            if (request == null)
            {
                    return BadRequest(new { error = "Request body is required." });
                }
                if (string.IsNullOrWhiteSpace(request.VersionId))
                {
                    return BadRequest(new { error = "Version ID is required." });
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
        /// <summary>
        /// Create a table-level snapshot — not available.
        /// POST /api/ppdm39/versioning/snapshots
        /// </summary>
        [HttpPost("snapshots")]
        public IActionResult CreateTableSnapshot()
        {
            // Not available: the versioning service versions one record at a time (a DATA_VERSION_SNAPSHOT row per record,
            // read back by table and record key); nothing captures a table's rows as one snapshot. This passed the table's
            // name as the record to version, and the service, finding no key column on a string, failed every time with a
            // 500 (2026-10-10).
            return Problem(
                statusCode: StatusCodes.Status501NotImplemented,
                title: "Not available",
                detail: "Taking a snapshot of a table is not available yet.");
        }

        /// <summary>
        /// List a table's snapshots — not available.
        /// GET /api/ppdm39/versioning/snapshots?table=WELL
        /// </summary>
        [HttpGet("snapshots")]
        public IActionResult GetTableSnapshots()
        {
            // Not available, as taking one is not: there are no table snapshots to list. This listed the versions of a
            // record keyed by the table's name, of which there are none, with each version's number as its "row count"
            // (2026-10-10).
            return Problem(
                statusCode: StatusCodes.Status501NotImplemented,
                title: "Not available",
                detail: "Listing a table's snapshots is not available yet.");
        }
    }
}
