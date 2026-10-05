using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Pricing;
using Beep.OilandGas.Models.Data.Accounting.Pricing;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Pricing
{
    /// <summary>
    /// API controller for Pricing operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/pricing")]
    public class PricingController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly Beep.OilandGas.ApiService.Services.RunTicketStore _tickets;
        private readonly ILogger<PricingController> _logger;

        public PricingController(
            ProductionAccountingService service,
            Beep.OilandGas.ApiService.Services.RunTicketStore tickets,
            ILogger<PricingController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _tickets = tickets ?? throw new ArgumentNullException(nameof(tickets));
            _logger = logger;
        }

        /// <summary>
        /// Get price indices.
        /// </summary>
        [HttpGet("indices")]
        public ActionResult<List<PriceIndex>> GetPriceIndices(
            [FromQuery] string? indexName = null,
            [FromQuery] string connectionName = "PPDM39")
        {
            var indexManager = _service.PricingManager.GetIndexManager();
            if (!string.IsNullOrEmpty(indexName))
            {
                var index = indexManager.GetLatestPrice(indexName);
                if (index == null)
                        return NotFound(new { error = $"Price index {indexName} not found." });
                return Ok(new List<PriceIndex> { new PriceIndex
                {
                    IndexName = index.IndexName,
                    IndexDate = index.IndexDate,
                    Price = index.Price,
                    Currency = index.Currency
                }});
            }
            
            var standardIndices = new[] { "WTI", "Brent", "LLS", "WCS" };
            var dtos = standardIndices.Select(name =>
            {
                var idx = indexManager.GetLatestPrice(name);
                return idx != null ? new PriceIndex
                {
                    IndexName = idx.IndexName,
                    IndexDate = idx.IndexDate,
                    Price = idx.Price,
                    Currency = idx.Currency
                } : null;
            }).Where(i => i != null).ToList();
            return Ok(dtos);
        }

        /// <summary>
        /// Add or update price index.
        /// </summary>
        [HttpPost("indices")]
        public ActionResult<PriceIndex> AddPriceIndex(
            [FromBody] PriceIndexRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var indexManager = _service.PricingManager.GetIndexManager();
            var index = new PriceIndex
            {
                IndexName = request.IndexName,
                IndexDate = request.IndexDate,
                Price = request.Price,
                Currency = request.Currency ?? "USD"
            };

            indexManager.AddOrUpdatePriceIndex(index);
            return Ok(new PriceIndex
            {
                IndexName = index.IndexName,
                IndexDate = index.IndexDate,
                Price = index.Price,
                Currency = index.Currency
            });
        }

        /// <summary>
        /// Value a run ticket.
        /// </summary>
        [HttpPost("valuateticket")]
        public async Task<ActionResult<RUN_TICKET_VALUATION>> ValueRunTicket(
            [FromBody] ValueRunTicketRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var ticket = await _tickets.GetAsync(request.RunTicketNumber);
            if (ticket == null)
                return NotFound(new { error = $"Run ticket {request.RunTicketNumber} not found." });

            PricingMethod pricingMethod;
            if (!Enum.TryParse<PricingMethod>(request.PricingMethod, true, out pricingMethod))
                pricingMethod = PricingMethod.IndexBased;

            var valuation = _service.PricingManager.ValueRunTicket(
                ticket,
                pricingMethod,
                request.FixedPrice,
                request.IndexName,
                request.Differential,
                null);

            return Ok(MapToRUN_TICKET_VALUATIONDto(valuation));
        }

        private RUN_TICKET_VALUATION MapToRUN_TICKET_VALUATIONDto(RUN_TICKET_VALUATION valuation)
        {
            return new RUN_TICKET_VALUATION
            {
                ValuationId = valuation.ValuationId,
                RunTicketNumber = valuation.RunTicketNumber,
                ValuationDate = valuation.ValuationDate,
                BasePrice = valuation.BasePrice,
                TotalAdjustments = valuation.TotalAdjustments,
                AdjustedPrice = valuation.AdjustedPrice,
                NetVolume = valuation.NetVolume,
                TotalValue = valuation.TotalValue
            };
        }
    }

}
