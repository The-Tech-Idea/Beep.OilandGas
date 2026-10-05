using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// API controller for demo database operations
    /// </summary>
    [ApiController]
    [Route("api/demo")]
    [Authorize(Roles = "Admin,Administrator")]
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
        /// Create a demo database for the signed-in user
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<CreateDemoDatabaseResponse>> CreateDemoDatabase([FromBody] CreateDemoDatabaseRequest request)
        {
            var userId = User.ActingUserId();
            if (request == null)
            {
                    return BadRequest(new { error = "Request is required." });
            }

            request.UserId = userId;
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

        /// <summary>
        /// Get demo databases for current user
        /// </summary>
        [HttpGet("my-databases")]
        public ActionResult<List<DemoDatabaseMetadata>> GetMyDemoDatabases()
        {
            var userId = User.ActingUserId();
            var databases = _demoDatabaseService.GetUserDemoDatabases(userId);
            return Ok(databases);
        }

        /// <summary>
        /// List all demo databases (admin only)
        /// </summary>
        [HttpGet("list")]
        public ActionResult<ListDemoDatabasesResponse> ListAllDemoDatabases()
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

        /// <summary>
        /// Delete a specific demo database
        /// </summary>
        [HttpDelete("{connectionName}")]
        public async Task<ActionResult<DeleteDemoDatabaseResponse>> DeleteDemoDatabase(string connectionName)
        {
                if (string.IsNullOrWhiteSpace(connectionName))
                    return BadRequest(new { error = "Connection name is required." });
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

        /// <summary>
        /// Manually trigger cleanup of expired demo databases (admin only)
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<ActionResult<CleanupDemoDatabasesResponse>> CleanupExpiredDatabases()
        {
            var response = await _demoDatabaseService.CleanupExpiredDatabasesAsync();
            return Ok(response);
        }
    }
}
