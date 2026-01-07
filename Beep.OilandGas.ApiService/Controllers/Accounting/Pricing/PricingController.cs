using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Pricing;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.DTOs.Accounting.Pricing;
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
        private readonly ILogger<PricingController> _logger;

        public PricingController(
            ProductionAccountingService service,
            ILogger<PricingController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Get price indices.
        /// </summary>
        [HttpGet("indices")]
        public ActionResult<List<PriceIndexDto>> GetPriceIndices(
            [FromQuery] string? indexName = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var indexManager = _service.PricingManager.GetIndexManager();
                if (!string.IsNullOrEmpty(indexName))
                {
                    var index = indexManager.GetLatestPrice(indexName);
                    if (index == null)
                        return NotFound(new { error = $"Price index {indexName} not found" });
                    return Ok(new List<PriceIndexDto> { new PriceIndexDto
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
                    return idx != null ? new PriceIndexDto
                    {
                        IndexName = idx.IndexName,
                        IndexDate = idx.IndexDate,
                        Price = idx.Price,
                        Currency = idx.Currency
                    } : null;
                }).Where(i => i != null).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting price indices");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Add or update price index.
        /// </summary>
        [HttpPost("indices")]
        public ActionResult<PriceIndexDto> AddPriceIndex(
            [FromBody] PriceIndexRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
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
                return Ok(new PriceIndexDto
                {
                    IndexName = index.IndexName,
                    IndexDate = index.IndexDate,
                    Price = index.Price,
                    Currency = index.Currency
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding price index");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Value a run ticket.
        /// </summary>
        [HttpPost("valuateticket")]
        public ActionResult<RUN_TICKET_VALUATIONDto> ValueRunTicket(
            [FromBody] ValueRunTicketRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ticket = _service.ProductionManager.GetRunTicket(request.RunTicketNumber);
                if (ticket == null)
                    return NotFound(new { error = $"Run ticket {request.RunTicketNumber} not found" });

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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error valuing run ticket");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private RUN_TICKET_VALUATIONDto MapToRUN_TICKET_VALUATIONDto(RUN_TICKET_VALUATION valuation)
        {
            return new RUN_TICKET_VALUATIONDto
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

