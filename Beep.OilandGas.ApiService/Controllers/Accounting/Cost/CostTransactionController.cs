using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Beep.OilandGas.Models.DTOs.Accounting.Cost;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Cost
{
    /// <summary>
    /// API controller for Cost Transaction operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/cost/transactions")]
    public class CostTransactionController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<CostTransactionController> _logger;

        public CostTransactionController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<CostTransactionController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Create a cost transaction.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateCostTransaction(
            [FromBody] CreateCostTransactionRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var connName = connectionName ?? _service.DefaultConnectionName;
                var repository = _service.GetRepository(typeof(COST_TRANSACTION), connName, "COST_TRANSACTION");

                var transaction = new COST_TRANSACTION
                {
                    COST_TRANSACTION_ID = Guid.NewGuid().ToString(),
                    PROPERTY_ID = request.PropertyId,
                    TRANSACTION_DATE = request.TransactionDate ?? DateTime.UtcNow,
                    AMOUNT = request.CostAmount,
                    IS_CAPITALIZED = request.IsCapitalized ? "Y" : "N",
                    IS_EXPENSED = request.IsCapitalized ? "N" : "Y",
                    COST_TYPE = request.IsCapitalized ? "Capital" : "Operating",
                    ACTIVE_IND = "Y",
                    ROW_CREATED_DATE = DateTime.UtcNow,
                    ROW_CREATED_BY = userId ?? "system"
                };

                if (!string.IsNullOrEmpty(request.Description))
                {
                    transaction.REMARK = request.Description;
                }

                await repository.InsertAsync(transaction, userId ?? "system");

                var journalEntryId = await _glIntegration.PostCostToGL(
                    transaction.COST_TRANSACTION_ID,
                    transaction.AMOUNT ?? 0m,
                    isCapitalized: request.IsCapitalized,
                    isCash: request.IsCash,
                    transactionDate: transaction.TRANSACTION_DATE ?? DateTime.UtcNow,
                    userId: userId ?? "system");

                return Ok(new { TransactionId = transaction.COST_TRANSACTION_ID, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for cost transaction");
                return StatusCode(500, new { error = "Cost transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cost transaction");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

