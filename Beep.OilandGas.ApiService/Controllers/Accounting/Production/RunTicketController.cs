using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Production;
using Beep.OilandGas.ProductionAccounting.Measurement;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.Data.ProductionAccounting;
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
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<RunTicketController> _logger;

        public RunTicketController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<RunTicketController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
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
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get run ticket by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<RUN_TICKET> GetRunTicket(string id, [FromQuery] string? connectionName = null)
        {
            try
            {
                var ticket = _service.ProductionManager.GetRunTicket(id);
                if (ticket == null)
                    return NotFound(new { error = $"Run ticket with ID {id} not found" });

                return Ok(MapToRunTicketDto(ticket));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting run ticket {TicketId}", id);
                return StatusCode(500, new { error = ex.Message });
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
                return StatusCode(500, new { error = "Run ticket created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating run ticket");
                return StatusCode(500, new { error = ex.Message });
            }
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
                    BSWVolume = ticket.BSWVOLUME,
                    BSWPercentage = ticket.BSWPERCENTAGE,
                    NetVolume = ticket.NET_VOLUME,
                    Temperature = ticket.TEMPERATURE,
                    ApiGravity = ticket.API_GRAVITY,
                    DispositionType = ticket.DISPOSITION_TYPE,
                    Purchaser = ticket.PURCHASER
                };
        }
    }
}

