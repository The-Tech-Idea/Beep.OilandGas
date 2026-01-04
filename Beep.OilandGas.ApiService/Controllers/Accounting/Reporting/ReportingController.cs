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
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.DTOs.Accounting.Reporting;
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
        public ActionResult<OperationalReportDto> GenerateOperationalReport(
            [FromBody] GenerateOperationalReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var runTickets = _service.ProductionManager.GetRunTicketsByDateRange(request.StartDate, request.EndDate).ToList();
                var inventories = new List<Inventory.CrudeOilInventory>();
                var allocations = new List<AllocationResult>();
                var measurements = new List<MeasurementRecord>();
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
        public ActionResult<LeaseReportDto> GenerateLeaseReport(
            [FromBody] GenerateLeaseReportRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lease = _service.LeaseManager.GetLease(request.LeaseId);
                if (lease == null)
                    return NotFound(new { error = $"Lease {request.LeaseId} not found" });

                var runTickets = _service.ProductionManager.GetRunTicketsByLease(request.LeaseId)
                    .Where(t => t.TicketDateTime >= request.StartDate && t.TicketDateTime <= request.EndDate)
                    .ToList();

                var salesTransactions = new List<SalesTransaction>();

                var report = _service.ReportManager.GenerateLeaseReport(
                    request.LeaseId,
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

        private OperationalReportDto MapToOperationalReportDto(OperationalReport report)
        {
            return new OperationalReportDto
            {
                ReportId = report.ReportId,
                ReportPeriodStart = report.ReportPeriodStart,
                ReportPeriodEnd = report.ReportPeriodEnd,
                GeneratedDate = report.GenerationDate
            };
        }

        private LeaseReportDto MapToLeaseReportDto(LeaseReport report)
        {
            var lease = _service.LeaseManager.GetLease(report.LeaseId);
            return new LeaseReportDto
            {
                ReportId = report.ReportId,
                LeaseId = report.LeaseId,
                LeaseName = lease?.LeaseName ?? report.LeaseId,
                ReportPeriodStart = report.ReportPeriodStart,
                ReportPeriodEnd = report.ReportPeriodEnd,
                GeneratedDate = report.GenerationDate
            };
        }
    }

}

