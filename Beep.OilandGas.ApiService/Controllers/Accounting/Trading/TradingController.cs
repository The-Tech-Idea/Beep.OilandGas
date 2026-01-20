using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting.Trading;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Trading
{
    /// <summary>
    /// API controller for Trading and Exchange operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/trading")]
    public class TradingController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly ILogger<TradingController> _logger;

        public TradingController(
            ProductionAccountingService service,
            ILogger<TradingController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Get exchange contract by ID.
        /// </summary>
        [HttpGet("exchanges")]
        public async Task<ActionResult<List<ExchangeContract>>> GetExchanges(
            [FromQuery] string? contractId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(contractId))
                    return BadRequest(new { error = "contractId parameter is required" });

                var contract = await _service.TradingService.GetContractAsync(contractId, connectionName);
                if (contract == null)
                    return NotFound(new { error = $"Exchange contract {contractId} not found" });
                
                return Ok(new List<ExchangeContract> { MapToExchangeContractDto(contract) });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting exchange contracts");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create exchange contract.
        /// </summary>
        [HttpPost("exchanges")]
        public async Task<ActionResult<ExchangeContract>> CreateExchangeContract(
            [FromBody] CreateExchangeContractRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Convert DTO to request for TradingService
                var tradingRequest = new Beep.OilandGas.Models.Data.ProductionAccounting.CreateExchangeContractRequest
                {
                    ContractId = request.ContractId,
                    ContractName = request.ContractName,
                    ContractType = request.ContractType,
                    EffectiveDate = request.EffectiveDate,
                    ExpirationDate = request.ExpirationDate
                };

                var contract = await _service.TradingService.RegisterContractAsync(tradingRequest, userId ?? "system", connectionName);
                return Ok(MapToExchangeContractDto(contract));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exchange contract");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private ExchangeContract MapToExchangeContractDto(Beep.OilandGas.Models.Data.ProductionAccounting.EXCHANGE_CONTRACT contract)
        {
            // Parse ContractType from string to enum
            ExchangeContractType contractType = ExchangeContractType.PhysicalExchange;
            if (!string.IsNullOrEmpty(contract.CONTRACT_TYPE))
            {
                Enum.TryParse<ExchangeContractType>(contract.CONTRACT_TYPE, true, out contractType);
            }

            return new ExchangeContract
            {
                ContractId = contract.CONTRACT_ID ?? string.Empty,
                ContractName = contract.CONTRACT_NAME ?? string.Empty,
                ContractType = contractType,
                EffectiveDate = contract.EFFECTIVE_DATE ?? DateTime.UtcNow,
                ExpirationDate = contract.EXPIRY_DATE
            };
        }
    }

}

