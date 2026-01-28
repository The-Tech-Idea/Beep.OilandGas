using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Reporting;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Allocation;
using Beep.OilandGas.ProductionAccounting.Measurement;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Inventory;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.Accounting.Reporting;
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
        private readonly ILogger<ReportingController> _logger;

        public ReportingController(
            ProductionAccountingService service,
            ILogger<ReportingController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Generate operational report.
        /// </summary>
        [HttpPost("operational")]
        public ActionResult<OPERATIONAL_REPORT> GenerateOperationalReport(
            [FromBody] GenerateOperationalReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var runTickets = _service.ProductionManager.GetRunTicketsByDateRange(request.StartDate, request.EndDate).ToList();
                var inventories = new List<Inventory.CrudeOilInventory>();
                var allocations = new List<ALLOCATION_RESULT>();
                var measurements = new List<MEASUREMENT_RECORD>();
                var salesTransactions = new List<SalesTransaction>();

                var report = _service.ReportManager.GenerateOperationalReport(
                    request.StartDate,
                    request.EndDate,
                    runTickets,
                    inventories,
                    allocations,
                    measurements,
                    salesTransactions);

                return Ok(MapToOperationalReportDto(report));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating operational report");
                return StatusCode(500, new { error = ex.Message });
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
                    return NotFound(new { error = $"Lease {request.LEASE_ID} not found" });

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
                return StatusCode(500, new { error = ex.Message });
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
    }

}

