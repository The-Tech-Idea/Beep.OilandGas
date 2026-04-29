using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Beep.OilandGas.Models.Data.Accounting.Revenue;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Revenue
{
    /// <summary>
    /// API controller for Revenue Transaction operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/revenue/transactions")]
    public class RevenueTransactionController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly IRevenueService _revenueService;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<RevenueTransactionController> _logger;

        public RevenueTransactionController(
            ProductionAccountingService service,
            IRevenueService revenueService,
            GLIntegrationService glIntegration,
            ILogger<RevenueTransactionController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _revenueService = revenueService ?? throw new ArgumentNullException(nameof(revenueService));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Create a revenue transaction.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateRevenueTransaction(
            [FromBody] CreateRevenueTransactionRequest request,
            [FromQuery] bool isCash = false,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var connName = connectionName ?? _service.DefaultConnectionName;
                var repository = _service.GetRepository(typeof(REVENUE_TRANSACTION), connName, "REVENUE_TRANSACTION");

                var transaction = new REVENUE_TRANSACTION
                {
                    REVENUE_TRANSACTION_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = request.PropertyId,
                    TRANSACTION_DATE = request.TransactionDate ?? DateTime.UtcNow,
                    GROSS_REVENUE = request.RevenueAmount,
                    NET_REVENUE = request.RevenueAmount,
                    ACTIVE_IND = "Y",
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId ?? "system"
                };

                await repository.InsertAsync(transaction, userId ?? "system");

                var journalEntryId = await _glIntegration.PostRevenueToGL(
                    transaction.REVENUE_TRANSACTION_ID,
                    transaction.GROSS_REVENUE ?? 0m,
                    isCash: isCash,
                    transactionDate: transaction.TRANSACTION_DATE ?? DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { TransactionId = transaction.REVENUE_TRANSACTION_ID, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for revenue transaction");
                    return StatusCode(500, new { error = "Revenue transaction created but GL posting failed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating revenue transaction");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Service-backed revenue recognition from allocation detail.
        /// </summary>
        [HttpPost("service/recognize")]
        public async Task<ActionResult<REVENUE_ALLOCATION>> RecognizeRevenueAsync(
            [FromBody] ALLOCATION_DETAIL allocationDetail,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (allocationDetail == null)
                    return BadRequest(new { error = "Allocation detail payload is required." });

                var result = await _revenueService.RecognizeRevenueAsync(
                    allocationDetail,
                    ResolveUserId(),
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recognizing revenue via service endpoint");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        /// <summary>
        /// Service-backed revenue allocation validation.
        /// </summary>
        [HttpPost("service/validate")]
        public async Task<ActionResult<object>> ValidateRevenueAllocationAsync(
            [FromBody] REVENUE_ALLOCATION allocation,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                if (allocation == null)
                    return BadRequest(new { error = "Revenue allocation payload is required." });

                var isValid = await _revenueService.ValidateAsync(
                    allocation,
                    connectionName ?? _service.DefaultConnectionName);

                return Ok(new { IsValid = isValid });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating revenue allocation via service endpoint");
                return StatusCode(500, new { error = "An internal error occurred." });
            }
        }

        private string ResolveUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? "system";
        }
    }

}

