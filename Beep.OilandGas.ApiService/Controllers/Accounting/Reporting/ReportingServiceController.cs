using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Reporting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Reporting
{
    /// <summary>
    /// Service-backed reporting endpoints aligned with <see cref="IReportingService"/>.
    /// </summary>
    [ApiController]
    [Route("api/accounting/reporting/service")]
    public class ReportingServiceController : ControllerBase
    {
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportingServiceController> _logger;

        public ReportingServiceController(
            IReportingService reportingService,
            ILogger<ReportingServiceController> logger)
        {
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _logger = logger;
        }

        [HttpPost("operational")]
        public async Task<ActionResult<ReportResult>> GenerateOperationalAsync(
            [FromBody] GenerateOperationalReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.GenerateOperationalReportAsync(request, ResolveUserId(), connectionName),
                "Error generating operational report");
        }

        [HttpPost("financial")]
        public async Task<ActionResult<ReportResult>> GenerateFinancialAsync(
            [FromBody] GenerateFinancialReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.GenerateFinancialReportAsync(request, ResolveUserId(), connectionName),
                "Error generating financial report");
        }

        [HttpPost("royalty-statement")]
        public async Task<ActionResult<ReportResult>> GenerateRoyaltyStatementAsync(
            [FromBody] GenerateRoyaltyStatementRequest request,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.GenerateRoyaltyStatementAsync(request, ResolveUserId(), connectionName),
                "Error generating royalty statement");
        }

        [HttpPost("jib-statement")]
        public async Task<ActionResult<ReportResult>> GenerateJibStatementAsync(
            [FromBody] GenerateJIBStatementRequest request,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.GenerateJIBStatementAsync(request, ResolveUserId(), connectionName),
                "Error generating JIB statement");
        }

        [HttpPost("schedule")]
        public async Task<ActionResult<ReportSchedule>> ScheduleAsync(
            [FromBody] ScheduleReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.ScheduleReportAsync(request, ResolveUserId(), connectionName),
                "Error scheduling report");
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<List<ReportSchedule>>> GetSchedulesAsync([FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.GetScheduledReportsAsync(connectionName),
                "Error fetching report schedules");
        }

        [HttpPost("{reportId}/distribute")]
        public async Task<ActionResult<ReportDistributionResult>> DistributeAsync(
            string reportId,
            [FromBody] ReportDistributionRequest request,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.DistributeReportAsync(reportId, request, ResolveUserId(), connectionName),
                "Error distributing report");
        }

        [HttpGet("history")]
        public async Task<ActionResult<List<ReportHistory>>> GetHistoryAsync(
            [FromQuery] string? reportType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? connectionName = null)
        {
            return await ExecuteAsync(
                () => _reportingService.GetReportHistoryAsync(reportType, startDate, endDate, connectionName),
                "Error fetching report history");
        }

        private async Task<ActionResult<T>> ExecuteAsync<T>(Func<Task<T>> action, string logMessage)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await action();
                return Ok(result);
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogWarning(ex, "{Message}", logMessage);
                return BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "{Message}", logMessage);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", logMessage);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "system";
        }
    }
}
