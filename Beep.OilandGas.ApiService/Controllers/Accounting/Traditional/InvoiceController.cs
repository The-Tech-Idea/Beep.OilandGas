using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Accounting.Services;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ApiService.Exceptions;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.ApiService.Services;

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
            [FromQuery] string connectionName = "PPDM39")
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

        /// <summary>
        /// Get invoice by ID.
        /// </summary>
        [HttpGet("{id}")]
        public ActionResult<object> GetInvoice(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest(new { error = "Invoice ID is required." });
            var invoice = _service.TraditionalAccounting.Invoice.GetInvoice(id);
            if (invoice == null)
                    return NotFound(new { error = $"Invoice with ID {id} not found." });

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

        /// <summary>
        /// Create a new invoice.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<object>> CreateInvoice([FromBody] CreateInvoiceRequest request)
        {
            var userId = User.ActingUserId();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var invoice = _service.TraditionalAccounting.Invoice.CreateInvoice(request, userId);

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

            var journalEntryId = await LedgerPosting.PostAsync(
                () => _glIntegration.PostTraditionalAccountingToGL(
                    invoice.INVOICE_ID,
                    "AR_Invoice",
                    lines,
                    invoice.INVOICE_DATE,
                    userId),
                $"Invoice {invoice.INVOICE_NUMBER}", invoice.INVOICE_ID, "AR_Invoice");

            return Ok(new { InvoiceId = invoice.INVOICE_ID, InvoiceNumber = invoice.INVOICE_NUMBER, JournalEntryId = journalEntryId });
        }
    }
}

