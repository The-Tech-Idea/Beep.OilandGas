using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.ProductionAccounting.Trading;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.DTOs.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Services;
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
        public ActionResult<List<ExchangeContractDto>> GetExchanges(
            [FromQuery] string? contractId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(contractId))
                    return BadRequest(new { error = "contractId parameter is required" });

                var contract = _service.TradingManager.GetContract(contractId);
                if (contract == null)
                    return NotFound(new { error = $"Exchange contract {contractId} not found" });
                
                return Ok(new List<ExchangeContractDto> { MapToExchangeContractDto(contract) });
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
        public ActionResult<ExchangeContractDto> CreateExchangeContract(
            [FromBody] CreateExchangeContractRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var contract = MapToExchangeContract(request);
                _service.TradingManager.RegisterContract(contract);
                return Ok(MapToExchangeContractDto(contract));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exchange contract");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private ExchangeContractDto MapToExchangeContractDto(ExchangeContract contract)
        {
            return new ExchangeContractDto
            {
                ContractId = contract.ContractId,
                ContractName = contract.ContractName,
                ContractType = contract.ContractType.ToString(),
                EffectiveDate = contract.EffectiveDate,
                ExpirationDate = contract.ExpirationDate
            };
        }

        private ExchangeContract MapToExchangeContract(CreateExchangeContractRequest request)
        {
            return new ExchangeContract
            {
                ContractId = request.ContractId,
                ContractName = request.ContractName,
                ContractType = request.ContractType,
                EffectiveDate = request.EffectiveDate,
                ExpirationDate = request.ExpirationDate
            };
        }
    }

    public class CreateExchangeContractRequest
    {
        public string ContractId { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;
        public ExchangeContractType ContractType { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}

