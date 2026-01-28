using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.ProductionAccounting.Royalty;
using Beep.OilandGas.ProductionAccounting.Accounting;
using Beep.OilandGas.ProductionAccounting.Models;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Royalty
{
    /// <summary>
    /// API controller for Royalty operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/royalty")]
    public class RoyaltyController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IAccountingService _accountingService;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<RoyaltyController> _logger;

        public RoyaltyController(
            ProductionAccountingService service,
            IAccountingService accountingService,
            GLIntegrationService glIntegration,
            ILogger<RoyaltyController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _accountingService = accountingService ?? throw new ArgumentNullException(nameof(accountingService));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Calculate and create royalty payment.
        /// </summary>
        [HttpPost("payments")]
        public async Task<ActionResult<ROYALTY_PAYMENT>> CreateRoyaltyPayment(
            [FromBody] CreateRoyaltyPaymentRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var tempTransaction = new SalesTransaction
                {
                    TransactionId = Guid.NewGuid().ToString(),
                    TransactionDate = request.PAYMENT_DATE ?? DateTime.UtcNow,
                    NetVolume = 0,
                    PricePerBarrel = request.ROYALTY_AMOUNT,
                    Purchaser = ""
                };

                var payment = _service.RoyaltyManager.CalculateAndCreatePayment(
                    tempTransaction,
                    request.ROYALTY_OWNER_ID,
                    request.ROYALTY_INTEREST ?? 1.0m,
                    request.PAYMENT_DATE ?? DateTime.UtcNow);

                // Post to GL: Debit Royalty Expense, Credit Cash
                var journalEntryId = await _glIntegration.PostRoyaltyToGL(
                    payment.PAYMENT_ID,
                    payment.ROYALTY_AMOUNT,
                    transactionDate: payment.PAYMENT_DATE,
                    userId: userId ?? "system");

                return Ok(new { Payment = MapToRoyaltyPaymentDto(payment), JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for royalty payment");
                return StatusCode(500, new { error = "Royalty payment created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating royalty payment");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Calculate royalties for production.
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ActionResult<object>> CalculateRoyalties(
            [FromBody] CalculateRoyaltyRequest request,
            [FromQuery] string? connectionName = null,
            [FromQuery] string? userId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (string.IsNullOrEmpty(request.FieldId))
                    return BadRequest(new { error = "FieldId is required" });

                var startDate = request.CALCULATION_DATE.Date;
                var endDate = request.CALCULATION_DATE.Date;

                var result = await _accountingService.CalculateRoyaltiesAsync(
                    request.FieldId,
                    startDate,
                    endDate,
                    request.PoolId,
                    null);

                var oilRoyaltyRate = request.OilRoyaltyRate ?? result.OilRoyaltyRate ?? 12.5m;
                var gasRoyaltyRate = request.GasRoyaltyRate ?? result.GasRoyaltyRate ?? 12.5m;
                var oilPrice = request.OilPrice ?? 0m;
                var gasPrice = request.GasPrice ?? 0m;

                var royaltyOilValue = result.RoyaltyOilVolume * oilPrice;
                var royaltyGasValue = result.RoyaltyGasVolume * gasPrice;
                var totalRoyaltyValue = royaltyOilValue + royaltyGasValue;

                var ROYALTY_CALCULATION = new ROYALTY_CALCULATION
                {
                    ROYALTY_CALCULATION_ID = Guid.NewGuid().ToString(),
                    FIELD_ID = request.FieldId,
                    POOL_ID = request.PoolId,
                    CALCULATION_DATE = request.CALCULATION_DATE,
                    GROSS_OIL_VOLUME = result.GrossOilVolume,
                    GROSS_GAS_VOLUME = result.GrossGasVolume,
                    OIL_ROYALTY_RATE = oilRoyaltyRate,
                    GAS_ROYALTY_RATE = gasRoyaltyRate,
                    OIL_PRICE = oilPrice,
                    GAS_PRICE = gasPrice,
                    ACTIVE_IND = "Y",
                    CREATE_DATE = DateTime.UtcNow,
                    CREATE_USER = userId ?? "system"
                };

                await _accountingService.SaveRoyaltyCalculationAsync(ROYALTY_CALCULATION, userId ?? "system");

                return Ok(new
                {
                    OperationId = Guid.NewGuid().ToString(),
                    Result = new
                    {
                        GrossOilVolume = result.GrossOilVolume,
                        GrossGasVolume = result.GrossGasVolume,
                        RoyaltyOilVolume = result.RoyaltyOilVolume,
                        RoyaltyGasVolume = result.RoyaltyGasVolume,
                        RoyaltyOilValue = royaltyOilValue,
                        RoyaltyGasValue = royaltyGasValue,
                        TotalRoyaltyValue = totalRoyaltyValue
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating royalties");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get royalty calculation records.
        /// </summary>
        [HttpGet("calculations")]
        public async Task<ActionResult<List<Beep.OilandGas.Models.Data.ROYALTY_CALCULATION>>> GetRoyaltyCalculations(
            [FromQuery] string? fieldId = null,
            [FromQuery] string? poolId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var results = await _accountingService.GetRoyaltyCalculationsAsync(
                    fieldId, poolId, startDate, endDate, null);

                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting royalty calculations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get royalty payments by owner.
        /// </summary>
        [HttpGet("payments")]
        public ActionResult<List<ROYALTY_PAYMENT>> GetRoyaltyPayments([FromQuery] string? ownerId = null, [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrEmpty(ownerId))
                    return BadRequest(new { error = "ownerId parameter is required" });

                var payments = _service.RoyaltyManager.GetPaymentsByOwner(ownerId).ToList();
                var dtos = payments.Select(MapToRoyaltyPaymentDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting royalty payments");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private ROYALTY_PAYMENT MapToRoyaltyPaymentDto(ROYALTY_PAYMENT payment)
        {
            return new ROYALTY_PAYMENT
            {
                PaymentId = payment.PAYMENT_ID,
                RoyaltyOwnerId = payment.ROYALTY_OWNER_ID,
                OwnerName = payment.OWNER_NAME,
                PaymentDate = payment.PAYMENT_DATE,
                RoyaltyAmount = payment.ROYALTY_AMOUNT,
                NetPayment = payment.NET_PAYMENT,
                Status = payment.Status.ToString()
            };
        }
    }

}

