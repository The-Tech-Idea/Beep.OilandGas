using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.PPDM39
{
    /// <summary>
    /// API controller for PPDM39 data access audit log operations.
    /// </summary>
    [ApiController]
    [Route("api/ppdm39/audit")]
    [Authorize]
    public class PPDM39DataAccessAuditController : ControllerBase
    {
        private readonly IPPDMDataAccessAuditService _auditService;
        private readonly ILogger<PPDM39DataAccessAuditController> _logger;

        public PPDM39DataAccessAuditController(
            IPPDMDataAccessAuditService auditService,
            ILogger<PPDM39DataAccessAuditController> logger)
        {
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// GET api/ppdm39/audit/statistics
        /// Returns aggregate access statistics for a rolling time window.
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<AccessStatistics>> GetStatistics(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? tableName = null)
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
                var toDate   = to   ?? DateTime.UtcNow;

                _logger.LogInformation("Fetching audit statistics from {From} to {To}", fromDate, toDate);

                var stats = await _auditService.GetAccessStatisticsAsync(tableName, fromDate, toDate);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit statistics");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// GET api/ppdm39/audit/recent
        /// Returns recent audit events for the requesting user or all users (admin only).
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult<List<DataAccessEvent>>> GetRecentEvents(
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            try
            {
                var fromDate = from ?? DateTime.UtcNow.AddDays(-1);
                var toDate   = to   ?? DateTime.UtcNow;
                var userId   = User.FindFirstValue(ClaimTypes.NameIdentifier);

                _logger.LogInformation("Fetching recent audit events from {From} to {To}", fromDate, toDate);

                var events = await _auditService.GetUserAccessHistoryAsync(userId ?? string.Empty, fromDate, toDate);
                return Ok(events ?? new List<DataAccessEvent>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent audit events");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// GET api/ppdm39/audit/entity/{tableName}
        /// Returns audit history for all records in a specific table.
        /// </summary>
        [HttpGet("entity/{tableName}")]
        public async Task<ActionResult<List<DataAccessEvent>>> GetEntityHistory(
            string tableName,
            [FromQuery] string? entityId = null)
        {
            if (string.IsNullOrWhiteSpace(tableName)) return BadRequest(new { error = "Table name is required." });
            try
            {
                _logger.LogInformation("Fetching audit history for table {TableName}", tableName);

                var events = await _auditService.GetAccessHistoryAsync(tableName, entityId ?? string.Empty);
                return Ok(events ?? new List<DataAccessEvent>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving entity audit history for {TableName}", tableName);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }
    }
}
