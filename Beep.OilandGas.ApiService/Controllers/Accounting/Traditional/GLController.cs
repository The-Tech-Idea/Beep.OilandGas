using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.GeneralLedger;
using Beep.OilandGas.ProductionAccounting.Services;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Traditional
{
    /// <summary>
    /// API controller for General Ledger operations.
    /// </summary>
    [ApiController]
    [Route("api/accounting/traditional/gl")]
    public class GLController : ControllerBase
    {
        private readonly ProductionAccountingService _service;
        private readonly ILogger<GLController> _logger;

        public GLController(
            ProductionAccountingService service,
            ILogger<GLController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
        }

        /// <summary>
        /// Get all GL accounts.
        /// </summary>
        [HttpGet("accounts")]
        public ActionResult<List<GLAccount>> GetAccounts([FromQuery] string? connectionName = null)
        {
            try
            {
                var accounts = _service.TraditionalAccounting.GeneralLedger.GetAllAccounts()
                    .Select(a => new GLAccount
                    {
                        GlAccountId = a.GL_ACCOUNT_ID,
                        AccountNumber = a.ACCOUNT_NUMBER,
                        AccountName = a.ACCOUNT_NAME,
                        AccountType = a.ACCOUNT_TYPE,
                        ParentAccountId = a.PARENT_ACCOUNT_ID,
                        NormalBalance = a.NORMAL_BALANCE,
                        OpeningBalance = a.OPENING_BALANCE,
                        CurrentBalance = a.CURRENT_BALANCE,
                        Description = a.DESCRIPTION,
                        ActiveInd = a.ACTIVE_IND
                    }).ToList();

                return Ok(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GL accounts");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get GL account by ID.
        /// </summary>
        [HttpGet("accounts/{id}")]
        public ActionResult<GLAccount> GetAccount(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var account = _service.TraditionalAccounting.GeneralLedger.GetAccount(id);
                if (account == null)
                    return NotFound(new { error = $"GL account with ID {id} not found" });

                var dto = new GLAccount
                {
                    GlAccountId = account.GL_ACCOUNT_ID,
                    AccountNumber = account.ACCOUNT_NUMBER,
                    AccountName = account.ACCOUNT_NAME,
                    AccountType = account.ACCOUNT_TYPE,
                    ParentAccountId = account.PARENT_ACCOUNT_ID,
                    NormalBalance = account.NORMAL_BALANCE,
                    OpeningBalance = account.OPENING_BALANCE,
                    CurrentBalance = account.CURRENT_BALANCE,
                    Description = account.DESCRIPTION,
                    ActiveInd = account.ACTIVE_IND
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting GL account {AccountId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new GL account.
        /// </summary>
        [HttpPost("accounts")]
        public ActionResult<GLAccount> CreateAccount(
            [FromBody] CreateGLAccountRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var account = _service.TraditionalAccounting.GeneralLedger.CreateAccount(request, userId ?? "system");

                var dto = new GLAccount
                {
                    GlAccountId = account.GL_ACCOUNT_ID,
                    AccountNumber = account.ACCOUNT_NUMBER,
                    AccountName = account.ACCOUNT_NAME,
                    AccountType = account.ACCOUNT_TYPE,
                    ParentAccountId = account.PARENT_ACCOUNT_ID,
                    NormalBalance = account.NORMAL_BALANCE,
                    OpeningBalance = account.OPENING_BALANCE,
                    CurrentBalance = account.CURRENT_BALANCE,
                    Description = account.DESCRIPTION,
                    ActiveInd = account.ACTIVE_IND
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating GL account");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a journal entry.
        /// </summary>
        [HttpPost("journal-entries")]
        public ActionResult<object> CreateJournalEntry(
            [FromBody] CreateJournalEntryRequest request,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var lines = request.Lines.Select(l => new JournalEntryLineData
                {
                    GlAccountId = l.GlAccountId,
                    DebitAmount = l.DebitAmount,
                    CreditAmount = l.CreditAmount,
                    Description = l.Description
                }).ToList();

                var entry = _service.TraditionalAccounting.JournalEntry.CreateJournalEntry(
                    request.EntryNumber ?? GenerateEntryNumber(),
                    request.EntryDate ?? DateTime.UtcNow,
                    request.EntryType ?? "Manual",
                    request.Description ?? "",
                    lines,
                    userId ?? "system");

                return Ok(new { JournalEntryId = entry.JOURNAL_ENTRY_ID, EntryNumber = entry.ENTRY_NUMBER });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating journal entry");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Post a journal entry.
        /// </summary>
        [HttpPost("journal-entries/{id}/post")]
        public ActionResult PostJournalEntry(
            string id,
            [FromQuery] string? userId = null,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                _service.TraditionalAccounting.JournalEntry.PostJournalEntry(id, userId ?? "system");
                return Ok(new { message = "Journal entry posted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error posting journal entry {EntryId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get journal entry by ID.
        /// </summary>
        [HttpGet("journal-entries/{id}")]
        public ActionResult<object> GetJournalEntry(
            string id,
            [FromQuery] string? connectionName = null)
        {
            try
            {
                var entry = _service.TraditionalAccounting.JournalEntry.GetJournalEntry(id);
                if (entry == null)
                    return NotFound(new { error = $"Journal entry with ID {id} not found" });

                var lines = _service.TraditionalAccounting.JournalEntry.GetJournalEntryLines(id).ToList();

                return Ok(new
                {
                    JournalEntryId = entry.JOURNAL_ENTRY_ID,
                    EntryNumber = entry.ENTRY_NUMBER,
                    EntryDate = entry.ENTRY_DATE,
                    EntryType = entry.ENTRY_TYPE,
                    Status = entry.STATUS,
                    Description = entry.DESCRIPTION,
                    TotalDebit = entry.TOTAL_DEBIT,
                    TotalCredit = entry.TOTAL_CREDIT,
                    Lines = lines.Select(l => new
                    {
                        LineId = l.JOURNAL_ENTRY_LINE_ID,
                        GlAccountId = l.GL_ACCOUNT_ID,
                        LineNumber = l.LINE_NUMBER,
                        DebitAmount = l.DEBIT_AMOUNT,
                        CreditAmount = l.CREDIT_AMOUNT,
                        Description = l.DESCRIPTION
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting journal entry {EntryId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        private string GenerateEntryNumber()
        {
            return $"JE-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
        }
    }
}

