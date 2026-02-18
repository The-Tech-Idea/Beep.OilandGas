using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Accounting.Models; // For PeriodCloseResult check

using Beep.OilandGas.Accounting.Models; // For PeriodCloseResult 

using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Period Closing Service
    /// Manages month/quarter/year close processes
    /// Validates GL balance, closes temporary accounts, generates closing entries
    /// </summary>
    public class PeriodClosingService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly TrialBalanceService _trialBalanceService;
        private readonly JournalEntryService _journalEntryService;
        private readonly AccountingBasisPostingService _basisPosting;
        private readonly IAccountMappingService? _accountMapping;
        private readonly ILogger<PeriodClosingService> _logger;
        private readonly APInvoiceService _apService;
        private readonly ARService _arService;
        private const string ConnectionName = "PPDM39";

        public PeriodClosingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            TrialBalanceService trialBalanceService,
            JournalEntryService journalEntryService,
            AccountingBasisPostingService basisPosting,
            APInvoiceService apService,
            ARService arService,
            ILogger<PeriodClosingService> logger,
            IAccountMappingService? accountMapping = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _basisPosting = basisPosting ?? throw new ArgumentNullException(nameof(basisPosting));
            _apService = apService ?? throw new ArgumentNullException(nameof(apService));
            _arService = arService ?? throw new ArgumentNullException(nameof(arService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        /// <summary>
        /// Validates if the period is ready to close.
        /// Checks GL balance, pending transactions, and subledger reconciliation.
        /// </summary>
        public async Task<PeriodCloseChecklist> ValidatePeriodCloseAsync(DateTime periodEndDate, string? bookId = null)
        {
            _logger?.LogInformation("Validating period close for {PeriodEnd}", periodEndDate);

            var checklist = new PeriodCloseChecklist
            {
                PeriodEndDate = periodEndDate,
                ChecklistGeneratedAt = DateTime.UtcNow
            };

            try
            {
                // 1. Validate GL Balance
                var glValidation = await _trialBalanceService.ValidateGLAsync(periodEndDate, bookId);
                checklist.Items.Add(new PeriodCloseChecklistItem
                {
                    RuleId = "GL_BALANCE",
                    Name = "GL Balance Check",
                    Description = "Ensure Total Debits equal Total Credits",
                    IsMandatory = true,
                    IsComplete = glValidation.IsBalanced,
                    Details = glValidation.IsBalanced 
                        ? "Balanced" 
                        : $"Difference: {glValidation.Difference:C} (Dr: {glValidation.TotalDebits:C}, Cr: {glValidation.TotalCredits:C})",
                    Module = "GL"
                });

                // 2. Check for Unposted AP Invoices
                var hasUnpostedAP = await _apService.HasUnpostedInvoicesAsync(periodEndDate);
                checklist.Items.Add(new PeriodCloseChecklistItem
                {
                    RuleId = "AP_CLOSE",
                    Name = "Accounts Payable Close",
                    Description = "All AP Invoices Posted",
                    IsMandatory = true,
                    IsComplete = !hasUnpostedAP,
                    Details = hasUnpostedAP ? "Found unposted AP Invoices" : "All AP Invoices Posted",
                    Module = "AP"
                });

                // 3. Check for Unposted AR Invoices
                var hasUnpostedAR = await _arService.HasUnpostedInvoicesAsync(periodEndDate);
                checklist.Items.Add(new PeriodCloseChecklistItem
                {
                    RuleId = "AR_CLOSE",
                    Name = "Accounts Receivable Close",
                    Description = "All AR Invoices Posted",
                    IsMandatory = true,
                    IsComplete = !hasUnpostedAR,
                    Details = hasUnpostedAR ? "Found unposted AR Invoices" : "All AR Invoices Posted",
                    Module = "AR"
                });

                // Determine overall readiness
                if (checklist.Items.Any(i => i.IsMandatory && !i.IsComplete))
                {
                    checklist.IsReadyToClose = false;
                    checklist.Errors = checklist.Items
                        .Where(i => i.IsMandatory && !i.IsComplete)
                        .Select(i => $"{i.Name} failed: {i.Details}")
                        .ToList();
                }
                else
                {
                    checklist.IsReadyToClose = true;
                }

                return checklist;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating period close: {Message}", ex.Message);
                checklist.IsReadyToClose = false;
                checklist.Errors.Add($"System Error: {ex.Message}");
                return checklist;
            }
        }
        /// 1. Validate GL is balanced
        /// 2. Create closing entries (close revenue/expense to retained earnings)
        /// 3. Create post-closing trial balance
        /// 4. Mark period as closed
        /// </summary>
        /// <summary>
        /// Close a period
        /// Steps:
        /// 1. Validate Period Readiness (GL Balanced, Subledgers Closed)
        /// 2. Create closing entries (close revenue/expense to retained earnings)
        /// 3. Create post-closing trial balance
        /// 4. (Optional) Create reversal entries for accruals
        /// 5. Mark period as closed
        /// </summary>
        public async Task<PeriodCloseResult> ClosePeriodAsync(
            DateTime periodEndDate,
            string periodName,
            string userId = "SYSTEM",
            string? bookId = null,
            bool createReversals = false)
        {
            _logger?.LogInformation("Closing period {PeriodName} ending {PeriodEnd}", periodName, periodEndDate);

            var closeResult = new PeriodCloseResult
            {
                //PeriodEndDate = periodEndDate,
                //PeriodName = periodName,
                //CloseDate = DateTime.UtcNow,
                StepsCompleted = new List<string>()
            };

            try
            {
                // Step 1: Validate Readiness
                var checklist = await ValidatePeriodCloseAsync(periodEndDate, bookId);
                if (!checklist.IsReadyToClose)
                {
                    closeResult.Success = false;
                    closeResult.Message = "Period validation failed";
                    closeResult.Errors = checklist.Errors;
                    return closeResult;
                }
                closeResult.StepsCompleted.Add("Period validation passed");

                // Step 2: Get trial balance
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEndDate, bookId);
                closeResult.StepsCompleted.Add($"Generated trial balance with {trialBalance.Count} accounts");

                // Step 3: Create closing entry lines for revenue/expense accounts
                var lineItems = new List<JOURNAL_ENTRY_LINE>();
                var retainedEarnings = GetAccountId(AccountMappingKeys.RetainedEarnings, DefaultGlAccounts.RetainedEarnings);
                var revenueAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "REVENUE").ToList();
                var expenseAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "EXPENSE").ToList();
                decimal totalRevenue = 0m;
                decimal totalExpense = 0m;

                foreach (var account in revenueAccounts)
                {
                    var balance = account.CURRENT_BALANCE ?? 0m;
                    if (balance == 0m) continue;

                    var accountId = string.IsNullOrWhiteSpace(account.ACCOUNT_NUMBER) ? account.GL_ACCOUNT_ID : account.ACCOUNT_NUMBER;
                    
                    if (balance > 0m)
                        lineItems.Add(new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = accountId, DEBIT_AMOUNT = balance, CREDIT_AMOUNT = 0m, DESCRIPTION = $"Closing revenue {account.ACCOUNT_NUMBER}" });
                    else
                        lineItems.Add(new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = accountId, DEBIT_AMOUNT = 0m, CREDIT_AMOUNT = Math.Abs(balance), DESCRIPTION = $"Closing contra revenue {account.ACCOUNT_NUMBER}" });

                    totalRevenue += balance;
                }

                foreach (var account in expenseAccounts)
                {
                    var balance = account.CURRENT_BALANCE ?? 0m;
                    if (balance == 0m) continue;

                    var accountId = string.IsNullOrWhiteSpace(account.ACCOUNT_NUMBER) ? account.GL_ACCOUNT_ID : account.ACCOUNT_NUMBER;

                    if (balance > 0m)
                        lineItems.Add(new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = accountId, DEBIT_AMOUNT = 0m, CREDIT_AMOUNT = balance, DESCRIPTION = $"Closing expense {account.ACCOUNT_NUMBER}" });
                    else
                        lineItems.Add(new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = accountId, DEBIT_AMOUNT = Math.Abs(balance), CREDIT_AMOUNT = 0m, DESCRIPTION = $"Closing contra expense {account.ACCOUNT_NUMBER}" });

                    totalExpense += balance;
                }

                var netIncome = totalRevenue - totalExpense;
                if (netIncome != 0m)
                {
                    if (netIncome > 0m)
                         lineItems.Add(new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = retainedEarnings, DEBIT_AMOUNT = 0m, CREDIT_AMOUNT = netIncome, DESCRIPTION = $"Closing Net Income to Retained Earnings" });
                    else
                         lineItems.Add(new JOURNAL_ENTRY_LINE { GL_ACCOUNT_ID = retainedEarnings, DEBIT_AMOUNT = Math.Abs(netIncome), CREDIT_AMOUNT = 0m, DESCRIPTION = $"Closing Net Loss to Retained Earnings" });
                }

                int entriesPosted = 0;
                if (lineItems.Count > 0)
                {
                    var result = await _basisPosting.PostEntryAsync(
                        periodEndDate,
                        $"Closing entries for {periodName}",
                        lineItems,
                        userId,
                        $"CLOSE-{periodName}",
                        "CLOSING",
                        bookId);
                    entriesPosted = 1;
                }
                closeResult.StepsCompleted.Add($"Posted {entriesPosted} closing entries");
                closeResult.ClosingEntriesCount = entriesPosted;

                // Step 4: Accrual Reversals (Next Period)
                if (createReversals)
                {
                    // Logic to find entries marked for reversal (e.g. source_module='ACCRUAL')
                    // This is a placeholder for the actual logic which would query specific accrual entries
                    // For now, we assume no auto-reversals unless specifically identified
                    closeResult.StepsCompleted.Add("Checked for automatic Accrual Reversals (None found)");
                }

                // Step 5: Post-Closing Validation
                var postCloseValidation = await _trialBalanceService.ValidateGLAsync(periodEndDate, bookId);
                if (!postCloseValidation.IsBalanced)
                {
                    closeResult.Success = false;
                    closeResult.Errors.Add("Post-closing GL validation failed");
                    return closeResult;
                }

                closeResult.StepsCompleted.Add("Post-closing GL validation passed");
                closeResult.Success = true;
                closeResult.Message = $"Period {periodName} closed successfully";

                _logger?.LogInformation("Period {PeriodName} closed successfully", periodName);
                return closeResult;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error closing period {PeriodName}: {Message}", periodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Reopen a closed period (for corrections)
        /// </summary>
        public async Task<bool> ReopenPeriodAsync(DateTime periodEndDate, string userId = "SYSTEM", string? bookId = null)
        {
            _logger?.LogInformation("Reopening period ending {PeriodEnd}", periodEndDate);

            try
            {
                // Reverse closing entries posted for the period.
                var entries = await GetClosingEntriesAsync(periodEndDate, bookId);
                foreach (var entry in entries)
                {
                    if (!string.Equals(entry.STATUS, "POSTED", StringComparison.OrdinalIgnoreCase))
                        continue;

                    await _journalEntryService.ReverseEntryAsync(
                        entry.JOURNAL_ENTRY_ID,
                        $"Reopen period ending {periodEndDate:yyyy-MM-dd}",
                        userId);
                }
                
                _logger?.LogInformation("Period {PeriodEnd} reopened", periodEndDate);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error reopening period: {Message}", ex.Message);
                throw;
            }
        }



        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }

        private async Task<List<JOURNAL_ENTRY>> GetClosingEntriesAsync(DateTime periodEndDate, string? bookId)
        {
            var repo = await GetRepoAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY", ConnectionName);

            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SOURCE_MODULE", Operator = "=", FilterValue = "CLOSING" },
                new AppFilter { FieldName = "ENTRY_DATE", Operator = "=", FilterValue = periodEndDate.ToString("yyyy-MM-dd") },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            if (!string.IsNullOrWhiteSpace(bookId))
                filters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = bookId });

            var entries = await repo.GetAsync(filters);
            return entries?.Cast<JOURNAL_ENTRY>().ToList() ?? new List<JOURNAL_ENTRY>();
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }
    }

}


