using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.ProductionAccounting.GeneralLedger;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Beep.OilandGas.Models.Data.Accounting.Traditional;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>
    /// API controller for Inventory operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/traditional/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<InventoryController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Create an inventory transaction.
        /// </summary>
        [HttpPost("transactions")]
        public async Task<ActionResult<object>> CreateTransaction(
            [FromBody] CreateInventoryTransactionRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var transaction = _service.TraditionalAccounting.Inventory.CreateTransaction(
                    request.InventoryItemId,
                    request.TransactionType,
                    request.TransactionDate ?? DateTime.UtcNow,
                    request.Quantity,
                    request.UnitCost,
                    request.Description ?? "",
                    userId ?? "system");

                // Post to GL based on transaction type
                // Receipt: Debit Inventory, Credit AP/Cash
                // Issue: Debit Expense/COGS, Credit Inventory
                var lines = new List<JournalEntryLineData>();
                
                if (request.TransactionType == "Receipt")
                {
                    lines.Add(new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "1300")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = transaction.TOTAL_COST,
                        CreditAmount = null,
                        Description = $"Inventory Receipt - {request.Description}"
                    });
                    lines.Add(new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "2000")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = null,
                        CreditAmount = transaction.TOTAL_COST,
                        Description = $"Inventory Receipt - {request.Description}"
                    });
                }
                else if (request.TransactionType == "Issue")
                {
                    lines.Add(new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "5100")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = transaction.TOTAL_COST,
                        CreditAmount = null,
                        Description = $"Inventory Issue - {request.Description}"
                    });
                    lines.Add(new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "1300")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = null,
                        CreditAmount = transaction.TOTAL_COST,
                        Description = $"Inventory Issue - {request.Description}"
                    });
                }

                if (lines.Count > 0)
                {
                    var journalEntryId = await _glIntegration.PostTraditionalAccountingToGL(
                        transaction.INVENTORY_TRANSACTION_ID,
                        "Inventory",
                        lines,
                        transaction.TRANSACTION_DATE ?? DateTime.UtcNow,
                        userId ?? "system");

                    return Ok(new { TransactionId = transaction.INVENTORY_TRANSACTION_ID, JournalEntryId = journalEntryId });
                }

                return Ok(new { TransactionId = transaction.INVENTORY_TRANSACTION_ID });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for inventory transaction");
                return StatusCode(500, new { error = "Transaction created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory transaction");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory transaction by ID.
        /// </summary>
        [HttpGet("transactions/{id}")]
        public ActionResult<object> GetTransaction(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var transaction = _service.TraditionalAccounting.Inventory.GetTransaction(id);
                if (transaction == null)
                    return NotFound(new { error = $"Inventory transaction with ID {id} not found" });

                return Ok(new
                {
                    TransactionId = transaction.INVENTORY_TRANSACTION_ID,
                    InventoryItemId = transaction.INVENTORY_ITEM_ID,
                    TransactionType = transaction.TRANSACTION_TYPE,
                    TransactionDate = transaction.TRANSACTION_DATE,
                    Quantity = transaction.QUANTITY,
                    UnitCost = transaction.UNIT_COST,
                    TotalCost = transaction.TOTAL_COST
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory transaction {TransactionId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

}

