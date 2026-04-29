using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Production
{
    /// <summary>
    /// API controller for Run Ticket operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/production/runtickets")]
    public class RunTicketController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IProductionAccountingService _productionAccountingService;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<RunTicketController> _logger;

        public RunTicketController(
            ProductionAccountingService service,
            IProductionAccountingService productionAccountingService,
            GLIntegrationService glIntegration,
            ILogger<RunTicketController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _productionAccountingService = productionAccountingService ?? throw new ArgumentNullException(nameof(productionAccountingService));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Get all run tickets.
        /// </summary>
        [HttpGet]
        public ActionResult<List<RUN_TICKET>> GetRunTickets(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var start = startDate ?? DateTime.Now.AddMonths(-1);
                var end = endDate ?? DateTime.Now;
                var tickets = _service.ProductionManager.GetRunTicketsByDateRange(start, end).ToList();
                var dtos = tickets.Select(MapToRunTicketDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting run tickets");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get run ticket by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<RUN_TICKET> GetRunTicket(string id, [FromQuery] string? connectionName = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Run ticket ID is required." });
            try
            {
                var ticket = _service.ProductionManager.GetRunTicket(id);
                if (ticket == null)
                        return NotFound(new { error = $"Run ticket with ID {id} not found." });

                return Ok(MapToRunTicketDto(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting run ticket {TicketId}", id);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Create a run ticket.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RUN_TICKET>> CreateRunTicket(
            [FromBody] CreateRunTicketRequest request,
            [FromQuery] decimal? revenueAmount = null,
            [FromQuery] bool isCash = false,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var measurement = new MEASUREMENT_RECORD
                {
                    MeasurementId = Guid.NewGuid().ToString(),
                    MeasurementDateTime = request.TICKET_DATE_TIME ?? DateTime.Now,
                    Method = MeasurementMethod.Manual,
                    Standard = MeasurementStandard.API,
                    GrossVolume = request.GROSS_VOLUME,
                    BSW = request.BSWPERCENTAGE,
                    Temperature = request.TEMPERATURE,
                    ApiGravity = request.API_GRAVITY
                };

                var ticket = _service.ProductionManager.CreateRunTicket(
                    request.LEASE_ID,
                    request.WELL_ID,
                    request.TANK_BATTERY_ID,
                    measurement,
                    request.DISPOSITION_TYPE,
                    request.PURCHASER);

                // Post to GL if revenue amount provided
                if (revenueAmount.HasValue && revenueAmount.Value > 0)
                {
                    var journalEntryId = await _glIntegration.PostProductionToGL(
                        ticket.RUN_TICKET_NUMBER,
                        revenueAmount.Value,
                        isCash: isCash,
                        transactionDate: ticket.TICKET_DATE_TIME,
                        userId: userId ?? "system");

                    return Ok(new { Ticket = MapToRunTicketDto(ticket), JournalEntryId = journalEntryId });
                }

                return Ok(MapToRunTicketDto(ticket));
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for run ticket");
                    return StatusCode(500, new { error = "Run ticket created but GL posting failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating run ticket");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed full production-accounting cycle for a ticket payload.</summary>
        [HttpPost("service/process-cycle")]
        public async Task<ActionResult> ProcessProductionCycleAsync(
            [FromBody] RUN_TICKET runTicket,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (runTicket == null)
                    return BadRequest(new { error = "Run ticket payload is required." });

                var processed = await _productionAccountingService.ProcessProductionCycleAsync(
                    runTicket,
                    ResolveUserId(),
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(new { Processed = processed });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing production cycle");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed accounting status lookup.</summary>
        [HttpGet("service/accounting-status/{fieldId}")]
        public async Task<ActionResult<AccountingStatusData>> GetAccountingStatusAsync(
            string fieldId,
            [FromQuery] DateTime? asOfDate = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "Field ID is required." });

                var status = await _productionAccountingService.GetAccountingStatusAsync(
                    fieldId,
                    asOfDate,
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accounting status for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed revenue transactions query for a field/date window.</summary>
        [HttpGet("service/revenue-transactions/{fieldId}")]
        public async Task<ActionResult<List<REVENUE_TRANSACTION>>> GetRevenueTransactionsAsync(
            string fieldId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fieldId))
                    return BadRequest(new { error = "Field ID is required." });
                if (endDate < startDate)
                    return BadRequest(new { error = "End date must be on or after start date." });

                var transactions = await _productionAccountingService.GetRevenueTransactionsAsync(
                    fieldId,
                    startDate,
                    endDate,
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue transactions for field {FieldId}", fieldId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "system";
        }

        private RUN_TICKET MapToRunTicketDto(RUN_TICKET ticket)
        {
                return new RUN_TICKET
                {
                    RunTicketNumber = ticket.RUN_TICKET_NUMBER,
                    TicketDateTime = ticket.TICKET_DATE_TIME,
                    LeaseId = ticket.LEASE_ID,
                    WellId = ticket.WELL_ID,
                    TankBatteryId = ticket.TANK_BATTERY_ID,
                    GrossVolume = ticket.GROSS_VOLUME,
                    BSWVolume = ticket.BSW_VOLUME,
                    BSWPercentage = ticket.BSW_PERCENTAGE,
                    NetVolume = ticket.NET_VOLUME,
                    Temperature = ticket.TEMPERATURE,
                    ApiGravity = ticket.API_GRAVITY,
                    DispositionType = ticket.DISPOSITION_TYPE,
                    Purchaser = ticket.PURCHASER
                };
        }
    }
}

