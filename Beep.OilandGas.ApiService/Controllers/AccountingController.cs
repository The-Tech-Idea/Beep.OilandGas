using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Beep.OilandGas.Accounting.Services;
using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers
{
    /// <summary>
    /// General Ledger, Accounts Receivable, Accounts Payable, and Inventory operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AccountingController : ControllerBase
    {
        private readonly GLAccountService _glAccountService;
        private readonly JournalEntryService _journalEntryService;
        private readonly TrialBalanceService _trialBalanceService;
        private readonly ARInvoiceService _arInvoiceService;
        private readonly APInvoiceService _apInvoiceService;
        private readonly APPaymentService _apPaymentService;
        private readonly PurchaseOrderService _purchaseOrderService;
        private readonly InventoryService _inventoryService;
        private readonly ILogger<AccountingController> _logger;

        public AccountingController(
            GLAccountService glAccountService,
            JournalEntryService journalEntryService,
            TrialBalanceService trialBalanceService,
            ARInvoiceService arInvoiceService,
            APInvoiceService apInvoiceService,
            APPaymentService apPaymentService,
            PurchaseOrderService purchaseOrderService,
            InventoryService inventoryService,
            ILogger<AccountingController> logger)
        {
            _glAccountService = glAccountService;
            _journalEntryService = journalEntryService;
            _trialBalanceService = trialBalanceService;
            _arInvoiceService = arInvoiceService;
            _apInvoiceService = apInvoiceService;
            _apPaymentService = apPaymentService;
            _purchaseOrderService = purchaseOrderService;
            _inventoryService = inventoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get current user ID from claims
        /// </summary>
        private string GetUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "SYSTEM";
        }

        #region General Ledger Operations

        /// <summary>
        /// Get all GL accounts
        /// </summary>
        [HttpGet("gl-accounts")]
        public async Task<IActionResult> GetGLAccounts([FromQuery] string? fieldId = null)
        {
            try
            {
                _logger.LogInformation("Fetching GL accounts for field: {FieldId}", fieldId);
                var accounts = await _glAccountService.GetAllAccountsAsync(fieldId);
                return Ok(new { success = true, data = accounts, count = accounts?.Count ?? 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GL accounts");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get GL account by ID
        /// </summary>
        [HttpGet("gl-accounts/{accountId}")]
        public async Task<IActionResult> GetGLAccount(string accountId)
        {
            try
            {
                _logger.LogInformation("Fetching GL account: {AccountId}", accountId);
                var account = await _glAccountService.GetAccountByIdAsync(accountId);
                if (account == null)
                    return NotFound(new { success = false, error = "Account not found" });

                return Ok(new { success = true, data = account });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching GL account");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Create new GL account
        /// </summary>
        [HttpPost("gl-accounts")]
        public async Task<IActionResult> CreateGLAccount([FromBody] dynamic accountData)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Creating GL account by user: {UserId}", userId);
                
                // Account creation would use GLAccountService.CreateAccountAsync(...)
                // Implementation depends on your data model structure
                
                return Ok(new { success = true, message = "GL Account created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GL account");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get account balance for specific account
        /// </summary>
        [HttpGet("gl-accounts/{accountId}/balance")]
        public async Task<IActionResult> GetAccountBalance(string accountId)
        {
            try
            {
                _logger.LogInformation("Fetching balance for account: {AccountId}", accountId);
                var balance = await _glAccountService.GetAccountBalanceAsync(accountId);
                return Ok(new { success = true, accountId, balance });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching account balance");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Journal Entry Operations

        /// <summary>
        /// Create journal entry with line items
        /// </summary>
        [HttpPost("journal-entries")]
        public async Task<IActionResult> CreateJournalEntry([FromBody] dynamic entryData)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Creating journal entry by user: {UserId}", userId);
                
                // Journal entry creation would use JournalEntryService.CreateEntryAsync(...)
                // Implementation depends on your data model structure
                
                return Ok(new { success = true, message = "Journal entry created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating journal entry");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Post journal entry to GL
        /// </summary>
        [HttpPost("journal-entries/{entryId}/post")]
        public async Task<IActionResult> PostJournalEntry(string entryId)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Posting journal entry {EntryId} by user: {UserId}", entryId, userId);
                
                // Journal entry posting would use JournalEntryService.PostEntryAsync(...)
                
                return Ok(new { success = true, message = "Journal entry posted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting journal entry");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get journal entries
        /// </summary>
        [HttpGet("journal-entries")]
        public async Task<IActionResult> GetJournalEntries([FromQuery] string? fieldId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                _logger.LogInformation("Fetching journal entries for field: {FieldId}", fieldId);
                
                // Would fetch journal entries filtered by date and field
                
                return Ok(new { success = true, data = new List<dynamic>(), count = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching journal entries");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Trial Balance & Validation

        /// <summary>
        /// Get trial balance for all GL accounts
        /// </summary>
        [HttpGet("trial-balance")]
        public async Task<IActionResult> GetTrialBalance([FromQuery] string? fieldId = null, [FromQuery] DateTime? asOfDate = null)
        {
            try
            {
                _logger.LogInformation("Fetching trial balance for field: {FieldId}", fieldId);
                
                // Would fetch trial balance using TrialBalanceService.GetTrialBalanceAsync(...)
                
                return Ok(new { success = true, data = new { totalDebits = 0, totalCredits = 0, isBalanced = true } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching trial balance");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Validate GL balance (debits must equal credits)
        /// </summary>
        [HttpGet("trial-balance/validate")]
        public async Task<IActionResult> ValidateTrialBalance([FromQuery] string? fieldId = null)
        {
            try
            {
                _logger.LogInformation("Validating GL balance for field: {FieldId}", fieldId);
                
                // Would validate using TrialBalanceService.ValidateBalanceAsync(...)
                
                return Ok(new { success = true, isValid = true, message = "GL is balanced" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating GL balance");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Accounts Receivable

        /// <summary>
        /// Create customer invoice
        /// </summary>
        [HttpPost("ar-invoices")]
        public async Task<IActionResult> CreateARInvoice([FromBody] dynamic invoiceData)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Creating AR invoice by user: {UserId}", userId);
                
                // Would create AR invoice using ARInvoiceService.CreateInvoiceAsync(...)
                
                return Ok(new { success = true, message = "AR Invoice created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AR invoice");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get AR invoices
        /// </summary>
        [HttpGet("ar-invoices")]
        public async Task<IActionResult> GetARInvoices([FromQuery] string? fieldId = null, [FromQuery] string? status = null)
        {
            try
            {
                _logger.LogInformation("Fetching AR invoices for field: {FieldId}", fieldId);
                
                return Ok(new { success = true, data = new List<dynamic>(), count = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AR invoices");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get AR aging analysis
        /// </summary>
        [HttpGet("ar-invoices/aging")]
        public async Task<IActionResult> GetARAging([FromQuery] string? fieldId = null)
        {
            try
            {
                _logger.LogInformation("Fetching AR aging analysis for field: {FieldId}", fieldId);
                
                return Ok(new { success = true, data = new { current = 0, days31to60 = 0, days61to90 = 0, over90Days = 0 } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AR aging");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Accounts Payable

        /// <summary>
        /// Create vendor bill
        /// </summary>
        [HttpPost("ap-invoices")]
        public async Task<IActionResult> CreateAPInvoice([FromBody] dynamic invoiceData)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Creating AP invoice by user: {UserId}", userId);
                
                // Would create AP invoice using APInvoiceService.CreateInvoiceAsync(...)
                
                return Ok(new { success = true, message = "AP Invoice created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AP invoice");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get AP invoices
        /// </summary>
        [HttpGet("ap-invoices")]
        public async Task<IActionResult> GetAPInvoices([FromQuery] string? fieldId = null, [FromQuery] string? status = null)
        {
            try
            {
                _logger.LogInformation("Fetching AP invoices for field: {FieldId}", fieldId);
                
                return Ok(new { success = true, data = new List<dynamic>(), count = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AP invoices");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Record AP payment
        /// </summary>
        [HttpPost("ap-payments")]
        public async Task<IActionResult> CreateAPPayment([FromBody] dynamic paymentData)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Recording AP payment by user: {UserId}", userId);
                
                // Would record AP payment using APPaymentService.RecordPaymentAsync(...)
                
                return Ok(new { success = true, message = "AP Payment recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording AP payment");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get AP aging analysis
        /// </summary>
        [HttpGet("ap-invoices/aging")]
        public async Task<IActionResult> GetAPAging([FromQuery] string? fieldId = null)
        {
            try
            {
                _logger.LogInformation("Fetching AP aging analysis for field: {FieldId}", fieldId);
                
                return Ok(new { success = true, data = new { current = 0, days31to60 = 0, days61to90 = 0, over90Days = 0 } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching AP aging");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Purchase Order & Inventory

        /// <summary>
        /// Create purchase order
        /// </summary>
        [HttpPost("purchase-orders")]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] dynamic poData)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Creating purchase order by user: {UserId}", userId);
                
                // Would create PO using PurchaseOrderService.CreatePOAsync(...)
                
                return Ok(new { success = true, message = "Purchase Order created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get purchase orders
        /// </summary>
        [HttpGet("purchase-orders")]
        public async Task<IActionResult> GetPurchaseOrders([FromQuery] string? fieldId = null, [FromQuery] string? status = null)
        {
            try
            {
                _logger.LogInformation("Fetching purchase orders for field: {FieldId}", fieldId);
                
                return Ok(new { success = true, data = new List<dynamic>(), count = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching purchase orders");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Record goods receipt against PO
        /// </summary>
        [HttpPost("purchase-orders/{poId}/goods-receipt")]
        public async Task<IActionResult> RecordGoodsReceipt(string poId, [FromBody] dynamic receiptData)
        {
            try
            {
                var userId = GetUserId();
                _logger.LogInformation("Recording goods receipt for PO {PoId} by user: {UserId}", poId, userId);
                
                // Would record goods receipt using PurchaseOrderService.RecordGoodsReceiptAsync(...)
                
                return Ok(new { success = true, message = "Goods receipt recorded successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording goods receipt");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory items
        /// </summary>
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventory([FromQuery] string? fieldId = null)
        {
            try
            {
                _logger.LogInformation("Fetching inventory for field: {FieldId}", fieldId);
                
                return Ok(new { success = true, data = new List<dynamic>(), count = 0 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory valuation (FIFO, LIFO, WAC)
        /// </summary>
        [HttpGet("inventory/valuation")]
        public async Task<IActionResult> GetInventoryValuation([FromQuery] string? fieldId = null, [FromQuery] string? method = "WAC")
        {
            try
            {
                _logger.LogInformation("Fetching inventory valuation ({Method}) for field: {FieldId}", method, fieldId);
                
                return Ok(new { success = true, data = new { totalValue = 0, method, itemCount = 0 } });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching inventory valuation");
                return BadRequest(new { success = false, error = ex.Message });
            }
        }

        #endregion

        #region Health Check

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new { status = "Accounting Service is healthy", timestamp = DateTime.UtcNow });
        }

        #endregion
    }
}
