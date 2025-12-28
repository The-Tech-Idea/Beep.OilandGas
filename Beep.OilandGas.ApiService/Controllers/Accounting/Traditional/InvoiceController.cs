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
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>
    /// API controller for Invoice operations (Customer Invoices).
    /// </summary>
    [ApiController]
    [Route("api/accounting/traditional/invoice")]
    public class InvoiceController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly GLIntegrationService _glIntegration;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            ProductionAccountingService service,
            GLIntegrationService glIntegration,
            ILogger<InvoiceController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _glIntegration = glIntegration ?? throw new ArgumentNullException(nameof(glIntegration));
            _logger = logger;
        }

        /// <summary>
        /// Get all invoices.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<object>>> GetInvoices(
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var connName = connectionName ?? _service.DefaultConnectionName;
                var repository = _service.GetRepository(typeof(INVOICE), connName, "INVOICE");
                var invoices = await repository.GetAsync(new List<AppFilter>());

                var result = invoices.Cast<INVOICE>().Select(invoice => new
                {
                    InvoiceId = invoice.INVOICE_ID,
                    InvoiceNumber = invoice.INVOICE_NUMBER,
                    CustomerBaId = invoice.CUSTOMER_BA_ID,
                    InvoiceDate = invoice.INVOICE_DATE,
                    DueDate = invoice.DUE_DATE,
                    Subtotal = invoice.SUBTOTAL,
                    TaxAmount = invoice.TAX_AMOUNT,
                    TotalAmount = invoice.TOTAL_AMOUNT,
                    BalanceDue = invoice.BALANCE_DUE,
                    Status = invoice.STATUS
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get invoice by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<object> GetInvoice(string id)
        {
            try
            {
                var invoice = _service.TraditionalAccounting.Invoice.GetInvoice(id);
                if (invoice == null)
                    return NotFound(new { error = $"Invoice with ID {id} not found" });

                return Ok(new
                {
                    InvoiceId = invoice.INVOICE_ID,
                    InvoiceNumber = invoice.INVOICE_NUMBER,
                    CustomerBaId = invoice.CUSTOMER_BA_ID,
                    InvoiceDate = invoice.INVOICE_DATE,
                    DueDate = invoice.DUE_DATE,
                    Subtotal = invoice.SUBTOTAL,
                    TaxAmount = invoice.TAX_AMOUNT,
                    TotalAmount = invoice.TOTAL_AMOUNT,
                    BalanceDue = invoice.BALANCE_DUE,
                    Status = invoice.STATUS
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice {InvoiceId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new invoice.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateInvoice([FromBody] CreateInvoiceRequest request, [FromQuery] string? userId = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var invoice = _service.TraditionalAccounting.Invoice.CreateInvoice(request, userId ?? "system");

                // Post to GL: Debit AR, Credit Revenue
                var lines = new List<JournalEntryLineData>
                {
                    new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "1200")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = invoice.TOTAL_AMOUNT,
                        CreditAmount = null,
                        Description = $"Invoice {invoice.INVOICE_NUMBER}"
                    },
                    new JournalEntryLineData
                    {
                        GlAccountId = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                            .FirstOrDefault(a => a.ACCOUNT_NUMBER == "4000")?.GL_ACCOUNT_ID ?? "",
                        DebitAmount = null,
                        CreditAmount = invoice.TOTAL_AMOUNT,
                        Description = $"Invoice {invoice.INVOICE_NUMBER}"
                    }
                };

                var journalEntryId = await _glIntegration.PostTraditionalAccountingToGL(
                    invoice.INVOICE_ID,
                    "AR_Invoice",
                    lines,
                    invoice.INVOICE_DATE,
                    userId ?? "system");

                return Ok(new { InvoiceId = invoice.INVOICE_ID, InvoiceNumber = invoice.INVOICE_NUMBER, JournalEntryId = journalEntryId });
            }
            catch (GLPostingException ex)
            {
                _logger.LogError(ex, "GL posting failed for invoice");
                return StatusCode(500, new { error = "Invoice created but GL posting failed", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}

