using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Beep.OilandGas.ApiService.Services;
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
        private readonly Beep.OilandGas.ApiService.Services.RunTicketStore _tickets;
        private readonly IProductionAccountingService _productionAccountingService;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<RunTicketController> _logger;

        public RunTicketController(
            Beep.OilandGas.ApiService.Services.RunTicketStore tickets,
            IProductionAccountingService productionAccountingService,
            GLIntegrationService glIntegration,
            ILogger<RunTicketController> logger)
        {
            _tickets = tickets ?? throw new ArgumentNullException(nameof(tickets));
            _productionAccountingService = productionAccountingService ?? throw new ArgumentNullException(nameof(productionAccountingService));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Get all run tickets.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<RUN_TICKET>>> GetRunTickets(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string connectionName = "PPDM39")
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;
            if (end < start) return BadRequest(new { error = "End date must not precede start date." });
            var tickets = await _tickets.ListAsync(start, end);
            var dtos = tickets.Select(MapToRunTicketDto).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Get run ticket by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RUN_TICKET>> GetRunTicket(string id, [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Run ticket ID is required." });
            var ticket = await _tickets.GetAsync(id);
            if (ticket == null)
                    return NotFound(new { error = $"Run ticket with ID {id} not found." });

            return Ok(MapToRunTicketDto(ticket));
        }

        /// <summary>
        /// Create a run ticket.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RUN_TICKET>> CreateRunTicket(
            [FromBody] CreateRunTicketRequest request,
            [FromQuery] decimal? revenueAmount = null,
            [FromQuery] bool isCash = false,
            [FromQuery] string connectionName = "PPDM39")
        {
            var actor = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _tickets.CreateAsync(request, actor);

            // Post to GL if revenue amount provided
            if (revenueAmount.HasValue && revenueAmount.Value > 0)
            {
                var journalEntryId = await LedgerPosting.PostAsync(
                    () => _glIntegration.PostProductionToGL(
                        ticket.RUN_TICKET_NUMBER,
                        revenueAmount.Value,
                        isCash: isCash,
                        transactionDate: ticket.TICKET_DATE_TIME,
                        userId: actor),
                    $"Run ticket {ticket.RUN_TICKET_NUMBER}", ticket.RUN_TICKET_ID, "PRODUCTION");

                return Ok(new { Ticket = MapToRunTicketDto(ticket), JournalEntryId = journalEntryId });
            }

            return Ok(MapToRunTicketDto(ticket));
        }

        /// <summary>Service-backed full production-accounting cycle for a ticket payload.</summary>
        [HttpPost("service/process-cycle")]
        public async Task<ActionResult> ProcessProductionCycleAsync(
            [FromBody] RUN_TICKET runTicket,
            [FromQuery] string connectionName = "PPDM39")
        {
            var actor = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (runTicket == null)
                return BadRequest(new { error = "Run ticket payload is required." });

            var processed = await _productionAccountingService.ProcessProductionCycleAsync(
                runTicket,
                actor,
                connectionName ?? "PPDM39");

            return Ok(new { Processed = processed });
        }

        /// <summary>Service-backed accounting status lookup.</summary>
        [HttpGet("service/accounting-status/{fieldId}")]
        public async Task<ActionResult<AccountingStatusData>> GetAccountingStatusAsync(
            string fieldId,
            [FromQuery] DateTime? asOfDate = null,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "Field ID is required." });

            var status = await _productionAccountingService.GetAccountingStatusAsync(
                fieldId,
                asOfDate,
                connectionName ?? "PPDM39");

            return Ok(status);
        }

        /// <summary>Service-backed revenue transactions query for a field/date window.</summary>
        [HttpGet("service/revenue-transactions/{fieldId}")]
        public async Task<ActionResult<List<REVENUE_TRANSACTION>>> GetRevenueTransactionsAsync(
            string fieldId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                return BadRequest(new { error = "Field ID is required." });
            if (endDate < startDate)
                return BadRequest(new { error = "End date must be on or after start date." });

            var transactions = await _productionAccountingService.GetRevenueTransactionsAsync(
                fieldId,
                startDate,
                endDate,
                connectionName ?? "PPDM39");

            return Ok(transactions);
        }

        private RUN_TICKET MapToRunTicketDto(RUN_TICKET ticket)
        {
                return new RUN_TICKET
                {
                    RUN_TICKET_ID = ticket.RUN_TICKET_ID,
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
