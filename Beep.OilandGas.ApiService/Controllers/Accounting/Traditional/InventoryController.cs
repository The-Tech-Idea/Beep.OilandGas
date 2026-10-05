using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Inventory;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.ApiService.Services;

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
        private readonly IInventoryService _inventoryService;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(
            ProductionAccountingService service,
            IInventoryService inventoryService,
            GLIntegrationService glIntegration,
            ILogger<InventoryController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Create an inventory transaction.
        /// </summary>
        [HttpPost("transactions")]
        public async Task<ActionResult<object>> CreateTransaction(
            [FromBody] CreateInventoryTransactionRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var transaction = _service.TraditionalAccounting.Inventory.CreateTransaction(
                request.InventoryItemId,
                request.TransactionType,
                request.TransactionDate,
                request.Quantity,
                request.UnitCost,
                request.Description ?? "",
                userId);

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
                var journalEntryId = await LedgerPosting.PostAsync(
                    () => _glIntegration.PostTraditionalAccountingToGL(
                        transaction.INVENTORY_TRANSACTION_ID,
                        "Inventory",
                        lines,
                        transaction.TRANSACTION_DATE ?? DateTime.UtcNow,
                        userId),
                    $"Inventory transaction {transaction.INVENTORY_TRANSACTION_ID}", transaction.INVENTORY_TRANSACTION_ID, "Inventory");

                return Ok(new { TransactionId = transaction.INVENTORY_TRANSACTION_ID, JournalEntryId = journalEntryId });
            }

            return Ok(new { TransactionId = transaction.INVENTORY_TRANSACTION_ID });
        }

        /// <summary>
        /// Get inventory transaction by ID.
        /// </summary>
        [HttpGet("transactions/{id}")]
        public ActionResult<object> GetTransaction(
            string id,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Transaction ID is required." });
            var transaction = _service.TraditionalAccounting.Inventory.GetTransaction(id);
            if (transaction == null)
                    return NotFound(new { error = $"Inventory transaction with ID {id} not found." });

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

        /// <summary>Service-backed tank inventory update (delta volume).</summary>
        [HttpPost("service/{tankId}/update")]
        public async Task<ActionResult<TANK_INVENTORY>> UpdateTankInventoryAsync(
            string tankId,
            [FromBody] UpdateTankInventoryRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(tankId))
                return BadRequest(new { error = "Tank ID is required." });

            var inventory = await _inventoryService.UpdateInventoryAsync(
                tankId,
                request.VolumeDelta,
                userId,
                connectionName ?? _service.DefaultConnectionName);

            return Ok(inventory);
        }

        /// <summary>Service-backed tank inventory lookup.</summary>
        [HttpGet("service/{tankId}")]
        public async Task<ActionResult<TANK_INVENTORY>> GetTankInventoryAsync(
            string tankId,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(tankId))
                return BadRequest(new { error = "Tank ID is required." });

            var inventory = await _inventoryService.GetInventoryAsync(
                tankId,
                connectionName ?? _service.DefaultConnectionName);

            if (inventory == null)
                return NotFound(new { error = $"Tank inventory {tankId} not found." });

            return Ok(inventory);
        }

        /// <summary>Service-backed inventory validation.</summary>
        [HttpPost("service/validate")]
        public async Task<ActionResult<object>> ValidateInventoryAsync(
            [FromBody] TANK_INVENTORY inventory,
            [FromQuery] string connectionName = "PPDM39")
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (inventory == null)
                return BadRequest(new { error = "Inventory payload is required." });

            var isValid = await _inventoryService.ValidateAsync(
                inventory,
                connectionName ?? _service.DefaultConnectionName);

            return Ok(new { IsValid = isValid });
        }

        /// <summary>Service-backed valuation calculation for an inventory item.</summary>
        [HttpPost("service/{inventoryItemId}/valuation")]
        public async Task<ActionResult<INVENTORY_VALUATION>> CalculateValuationAsync(
            string inventoryItemId,
            [FromBody] CalculateInventoryValuationRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                return BadRequest(new { error = "Inventory item ID is required." });

            var valuation = await _inventoryService.CalculateValuationAsync(
                inventoryItemId,
                request.ValuationDate,
                request.Method,
                userId,
                connectionName ?? _service.DefaultConnectionName);

            return Ok(valuation);
        }

        /// <summary>Service-backed inventory reconciliation summary.</summary>
        [HttpPost("service/{inventoryItemId}/reconciliation-report")]
        public async Task<ActionResult<INVENTORY_REPORT_SUMMARY>> GenerateReconciliationReportAsync(
            string inventoryItemId,
            [FromBody] GenerateInventoryReconciliationReportRequest request,
            [FromQuery] string connectionName = "PPDM39")
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (string.IsNullOrWhiteSpace(inventoryItemId))
                return BadRequest(new { error = "Inventory item ID is required." });

            var report = await _inventoryService.GenerateReconciliationReportAsync(
                inventoryItemId,
                request.PeriodStart,
                request.PeriodEnd,
                userId,
                connectionName ?? _service.DefaultConnectionName);

            return Ok(report);
        }
    }

}

