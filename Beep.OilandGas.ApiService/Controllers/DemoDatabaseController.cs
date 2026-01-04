using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for demo database operations
    /// </summary>
    [ApiController]
    [Route("api/demo")]
    public class DemoDatabaseController : ControllerBase
    {
        private readonly DemoDatabaseService _demoDatabaseService;
        private readonly ILogger<DemoDatabaseController> _logger;

        public DemoDatabaseController(
            DemoDatabaseService demoDatabaseService,
            ILogger<DemoDatabaseController> logger)
        {
            _demoDatabaseService = demoDatabaseService ?? throw new ArgumentNullException(nameof(demoDatabaseService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create a demo database for a user
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<CreateDemoDatabaseResponse>> CreateDemoDatabase([FromBody] CreateDemoDatabaseRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var response = await _demoDatabaseService.CreateDemoDatabaseAsync(request);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating demo database for user {UserId}", request?.UserId);
                return StatusCode(500, new CreateDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to create demo database",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Get demo databases for current user
        /// </summary>
        [HttpGet("my-databases")]
        public ActionResult<List<DemoDatabaseMetadata>> GetMyDemoDatabases([FromQuery] string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "UserId is required" });
                }

                var databases = _demoDatabaseService.GetUserDemoDatabases(userId);
                return Ok(databases);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting demo databases for user {UserId}", userId);
                return StatusCode(500, new { error = "Failed to get demo databases", details = ex.Message });
            }
        }

        /// <summary>
        /// List all demo databases (admin only)
        /// </summary>
        [HttpGet("list")]
        public ActionResult<ListDemoDatabasesResponse> ListAllDemoDatabases()
        {
            try
            {
                var allDatabases = _demoDatabaseService.GetAllDemoDatabases();
                var expiredCount = allDatabases.Count(d => d.IsExpired);

                return Ok(new ListDemoDatabasesResponse
                {
                    Databases = allDatabases,
                    TotalCount = allDatabases.Count,
                    ExpiredCount = expiredCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing all demo databases");
                return StatusCode(500, new { error = "Failed to list demo databases", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete a specific demo database
        /// </summary>
        [HttpDelete("{connectionName}")]
        public async Task<ActionResult<DeleteDemoDatabaseResponse>> DeleteDemoDatabase(string connectionName)
        {
            try
            {
                if (string.IsNullOrEmpty(connectionName))
                {
                    return BadRequest(new { error = "Connection name is required" });
                }

                var response = await _demoDatabaseService.DeleteDemoDatabaseAsync(connectionName);
                
                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting demo database {ConnectionName}", connectionName);
                return StatusCode(500, new DeleteDemoDatabaseResponse
                {
                    Success = false,
                    Message = "Failed to delete demo database",
                    ErrorDetails = ex.Message
                });
            }
        }

        /// <summary>
        /// Manually trigger cleanup of expired demo databases (admin only)
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<ActionResult<CleanupDemoDatabasesResponse>> CleanupExpiredDatabases()
        {
            try
            {
                var response = await _demoDatabaseService.CleanupExpiredDatabasesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up expired demo databases");
                return StatusCode(500, new CleanupDemoDatabasesResponse
                {
                    Success = false,
                    Message = "Failed to cleanup expired demo databases",
                    ErrorDetails = ex.Message
                });
            }
        }
    }
}

