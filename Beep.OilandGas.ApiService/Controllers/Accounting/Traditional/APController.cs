using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.DTOs.Accounting;
using Beep.OilandGas.ProductionAccounting.GeneralLedger;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>
    /// API controller for Accounts Payable operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/traditional/ap")]
    public class APController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<APController> _logger;

        public APController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<APController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Get AP invoice by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<object> GetAPInvoice(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var invoice = _service.TraditionalAccounting.AccountsPayable.GetAPInvoice(id);
                if (invoice == null)
                    return NotFound(new { error = $"AP invoice with ID {id} not found" });

                return Ok(new
                {
                    ApInvoiceId = invoice.AP_INVOICE_ID,
                    InvoiceNumber = invoice.INVOICE_NUMBER,
                    VendorBaId = invoice.VENDOR_BA_ID,
                    InvoiceDate = invoice.INVOICE_DATE,
                    DueDate = invoice.DUE_DATE,
                    TotalAmount = invoice.TOTAL_AMOUNT,
                    BalanceDue = invoice.BALANCE_DUE,
                    Status = invoice.STATUS
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AP invoice {InvoiceId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new AP invoice.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateAPInvoice(
            [FromBody] CreateAPInvoiceRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var invoice = _service.TraditionalAccounting.AccountsPayable.CreateAPInvoice(request, userId ?? "system");

                // Post to GL: Debit Expense, Credit AP
                var lines = new List<JournalEntryLineData>
                {
                    new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "5000")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = invoice.TOTAL_AMOUNT,
                        CreditAmount = null,
                        Description = $"AP Invoice {invoice.INVOICE_NUMBER}"
                    },
                    new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "2000")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = null,
                        CreditAmount = invoice.TOTAL_AMOUNT,
                        Description = $"AP Invoice {invoice.INVOICE_NUMBER}"
                    }
                };

                var journalEntryId = await _glIntegration.PostTraditionalAccountingToGL(
                    invoice.AP_INVOICE_ID,
                    "AP_Invoice",
                    lines,
                    invoice.INVOICE_DATE,
                    userId ?? "system");

                return Ok(new { ApInvoiceId = invoice.AP_INVOICE_ID, InvoiceNumber = invoice.INVOICE_NUMBER, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for AP invoice");
                return StatusCode(500, new { error = "AP invoice created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AP invoice");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

