using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting.Royalty;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
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
        private readonly IRoyaltyService _royaltyService;
        private readonly IAccountingService _accountingService;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<RoyaltyController> _logger;

        public RoyaltyController(
            ProductionAccountingService service,
            IRoyaltyService royaltyService,
            IAccountingService accountingService,
            GLIntegrationService glIntegration,
            ILogger<RoyaltyController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _royaltyService = royaltyService ?? throw new ArgumentNullException(nameof(royaltyService));
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
                    return StatusCode(500, new { error = "Royalty payment created but GL posting failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating royalty payment");
                return StatusCode(500, new { error = "An internal error occurred." });
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
                        return BadRequest(new { error = "Field ID is required." });

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
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Get royalty calculation records.
        /// </summary>
        [HttpGet("calculations")]
        public async Task<ActionResult<List<ROYALTY_CALCULATION>>> GetRoyaltyCalculations(
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
                return StatusCode(500, new { error = "An internal error occurred." });
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
                        return BadRequest(new { error = "Owner ID parameter is required." });

                var payments = _service.RoyaltyManager.GetPaymentsByOwner(ownerId).ToList();
                var dtos = payments.Select(MapToRoyaltyPaymentDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting royalty payments");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed royalty calculation from allocation detail.</summary>
        [HttpPost("service/calculate")]
        public async Task<ActionResult<ROYALTY_CALCULATION>> CalculateFromAllocationDetailAsync(
            [FromBody] ALLOCATION_DETAIL detail,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _royaltyService.CalculateAsync(
                    detail,
                    ResolveUserId(),
                    connectionName ?? _service.DefaultConnectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating royalty via service endpoint");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed lookup of one royalty calculation by id.</summary>
        [HttpGet("service/calculations/{royaltyId}")]
        public async Task<ActionResult<ROYALTY_CALCULATION>> GetRoyaltyCalculationByIdAsync(
            string royaltyId,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(royaltyId))
                    return BadRequest(new { error = "Royalty calculation ID is required." });

                var result = await _royaltyService.GetAsync(royaltyId, connectionName ?? _service.DefaultConnectionName);
                if (result == null)
                    return NotFound(new { error = $"Royalty calculation {royaltyId} not found." });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching royalty calculation {RoyaltyId}", royaltyId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed lookup of royalty calculations by allocation id.</summary>
        [HttpGet("service/calculations/by-allocation/{allocationId}")]
        public async Task<ActionResult<List<ROYALTY_CALCULATION>>> GetRoyaltyCalculationsByAllocationAsync(
            string allocationId,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(allocationId))
                    return BadRequest(new { error = "Allocation ID is required." });

                var result = await _royaltyService.GetByAllocationAsync(
                    allocationId,
                    connectionName ?? _service.DefaultConnectionName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching royalty calculations by allocation {AllocationId}", allocationId);
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>Service-backed royalty payment record from an existing royalty calculation.</summary>
        [HttpPost("service/payments")]
        public async Task<ActionResult<ROYALTY_PAYMENT>> RecordRoyaltyPaymentAsync(
            [FromBody] RecordRoyaltyPaymentFromCalculationRequest request,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (request == null || request.RoyaltyCalculation == null)
                    return BadRequest(new { error = "Royalty calculation payload is required." });

                var payment = await _royaltyService.RecordPaymentAsync(
                    request.RoyaltyCalculation,
                    request.PaymentAmount,
                    ResolveUserId(),
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording royalty payment via service endpoint");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "system";
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

