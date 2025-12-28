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
    /// API controller for Accounts Receivable operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/traditional/ar")]
    public class ARController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<ARController> _logger;

        public ARController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<ARController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Get AR invoice by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<object> GetARInvoice(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var invoice = _service.TraditionalAccounting.AccountsReceivable.GetARInvoice(id);
                if (invoice == null)
                    return NotFound(new { error = $"AR invoice with ID {id} not found" });

                return Ok(new
                {
                    ArInvoiceId = invoice.AR_INVOICE_ID,
                    InvoiceNumber = invoice.INVOICE_NUMBER,
                    CustomerBaId = invoice.CUSTOMER_BA_ID,
                    InvoiceDate = invoice.INVOICE_DATE,
                    DueDate = invoice.DUE_DATE,
                    TotalAmount = invoice.TOTAL_AMOUNT,
                    BalanceDue = invoice.BALANCE_DUE,
                    Status = invoice.STATUS
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting AR invoice {InvoiceId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new AR invoice.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateARInvoice(
            [FromBody] CreateARInvoiceRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var invoice = _service.TraditionalAccounting.AccountsReceivable.CreateARInvoice(request, userId ?? "system");

                // Post to GL: Debit AR, Credit Revenue
                var lines = new List<JournalEntryLineData>
                {
                    new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "1200")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = invoice.TOTAL_AMOUNT,
                        CreditAmount = null,
                        Description = $"AR Invoice {invoice.INVOICE_NUMBER}"
                    },
                    new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "4000")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = null,
                        CreditAmount = invoice.TOTAL_AMOUNT,
                        Description = $"AR Invoice {invoice.INVOICE_NUMBER}"
                    }
                };

                var journalEntryId = await _glIntegration.PostTraditionalAccountingToGL(
                    invoice.AR_INVOICE_ID,
                    "AR_Invoice",
                    lines,
                    invoice.INVOICE_DATE,
                    userId ?? "system");

                return Ok(new { ArInvoiceId = invoice.AR_INVOICE_ID, InvoiceNumber = invoice.INVOICE_NUMBER, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for AR invoice");
                return StatusCode(500, new { error = "AR invoice created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AR invoice");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

