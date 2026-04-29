using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Reporting;
using Beep.OilandGas.Models.Data.Accounting.Reporting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Reporting
{
    /// <summary>
    /// API controller for Reporting operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/reporting")]
    public class ReportingController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IReportingService _reportingService;
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(
            ProductionAccountingService service,
            IReportingService reportingService,
            ILogger<ReportingController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _reportingService = reportingService ?? throw new ArgumentNullException(nameof(reportingService));
            _logger = logger;
        }

        /// <summary>
        /// Generate operational report.
        /// </summary>
        [HttpPost("operational")]
        public async Task<ActionResult<ReportResult>> GenerateOperationalReport(
            [FromBody] GenerateOperationalReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                
                var result = await _reportingService.GenerateOperationalReportAsync(
                    request,
                    ResolveUserId(),
                    connectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating operational report");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Generate financial report through reporting service.</summary>
        [HttpPost("financial")]
        public async Task<ActionResult<ReportResult>> GenerateFinancialReport(
            [FromBody] GenerateFinancialReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _reportingService.GenerateFinancialReportAsync(
                    request,
                    ResolveUserId(),
                    connectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating financial report");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Generate royalty statement through reporting service.</summary>
        [HttpPost("royalty-statement")]
        public async Task<ActionResult<ReportResult>> GenerateRoyaltyStatement(
            [FromBody] GenerateRoyaltyStatementRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _reportingService.GenerateRoyaltyStatementAsync(
                    request,
                    ResolveUserId(),
                    connectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating royalty statement");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Generate JIB statement through reporting service.</summary>
        [HttpPost("jib-statement")]
        public async Task<ActionResult<ReportResult>> GenerateJibStatement(
            [FromBody] GenerateJIBStatementRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _reportingService.GenerateJIBStatementAsync(
                    request,
                    ResolveUserId(),
                    connectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JIB statement");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Schedule report through reporting service.</summary>
        [HttpPost("schedule")]
        public async Task<ActionResult<ReportSchedule>> ScheduleReport(
            [FromBody] ScheduleReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var schedule = await _reportingService.ScheduleReportAsync(
                    request,
                    ResolveUserId(),
                    connectionName);
                return Ok(schedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling report");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Get report schedules through reporting service.</summary>
        [HttpGet("schedule")]
        public async Task<ActionResult<List<ReportSchedule>>> GetSchedules([FromQuery] string? connectionName = null)
        {
            try
            {
                var schedules = await _reportingService.GetScheduledReportsAsync(connectionName);
                return Ok(schedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching report schedules");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Distribute report through reporting service.</summary>
        [HttpPost("{reportId}/distribute")]
        public async Task<ActionResult<ReportDistributionResult>> DistributeReport(
            string reportId,
            [FromBody] ReportDistributionRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var distribution = await _reportingService.DistributeReportAsync(
                    reportId,
                    request,
                    ResolveUserId(),
                    connectionName);
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error distributing report {ReportId}", reportId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Get report history through reporting service.</summary>
        [HttpGet("history")]
        public async Task<ActionResult<List<ReportHistory>>> GetReportHistory(
            [FromQuery] string? reportType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var history = await _reportingService.GetReportHistoryAsync(reportType, startDate, endDate, connectionName);
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching report history");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Generate lease report.
        /// </summary>
        [HttpPost("lease")]
        public ActionResult<LEASE_REPORT> GenerateLeaseReport(
            [FromBody] GenerateLeaseReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lease = _service.LeaseManager.GetLease(request.LEASE_ID);
                if (lease == null)
                    return NotFound(new { error = $"Lease {request.LEASE_ID} not found." });

                var runTickets = _service.ProductionManager.GetRunTicketsByLease(request.LEASE_ID)
                    .Where(t => t.TicketDateTime >= request.StartDate && t.TicketDateTime <= request.EndDate)
                    .ToList();

                var salesTransactions = new List<SalesTransaction>();

                var report = _service.ReportManager.GenerateLeaseReport(
                    request.LEASE_ID,
                    request.StartDate,
                    request.EndDate,
                    runTickets,
                    salesTransactions);

                return Ok(MapToLeaseReportDto(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating lease report");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private OPERATIONAL_REPORT MapToOperationalReportDto(OPERATIONAL_REPORT report)
        {
            return new OPERATIONAL_REPORT
            {
                ReportId = report.REPORT_ID,
                ReportPeriodStart = report.REPORT_PERIOD_START,
                ReportPeriodEnd = report.REPORT_PERIOD_END,
                GeneratedDate = report.GenerationDate
            };
        }

        private LEASE_REPORT MapToLeaseReportDto(LEASE_REPORT report)
        {
            var lease = _service.LeaseManager.GetLease(report.LEASE_ID);
            return new LEASE_REPORT
            {
                ReportId = report.REPORT_ID,
                LeaseId = report.LEASE_ID,
                LeaseName = lease?.LEASE_NAME ?? report.LEASE_ID,
                ReportPeriodStart = report.REPORT_PERIOD_START,
                ReportPeriodEnd = report.REPORT_PERIOD_END,
                GeneratedDate = report.GenerationDate
            };
        }

        private string ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "system";
        }
    }

}

