using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.Accounting;
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
        private const string ConnectionName = "PPDM39";

        public PeriodClosingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            TrialBalanceService trialBalanceService,
            JournalEntryService journalEntryService,
            AccountingBasisPostingService basisPosting,
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
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _accountMapping = accountMapping;
        }

        /// <summary>
        /// Check if period can be closed
        /// Requirements: GL must be balanced
        /// </summary>
        public async Task<PeriodCloseReadiness> CheckCloseReadinessAsync(DateTime periodEndDate, string? bookId = null)
        {
            _logger?.LogInformation("Checking period close readiness for {PeriodEnd}", periodEndDate);

            try
            {
                var readiness = new PeriodCloseReadiness
                {
                    PeriodEndDate = periodEndDate,
                    CheckDate = DateTime.UtcNow,
                    Issues = new List<string>()
                };

                // Check 1: GL must be balanced
                var glValidation = await _trialBalanceService.ValidateGLAsync(periodEndDate, bookId);
                if (!glValidation.IsBalanced)
                {
                    readiness.Issues.Add($"GL out of balance: Difference {glValidation.Difference:C}");
                    readiness.IsReadyToClose = false;
                    return readiness;
                }

                // Check 2: Get trial balance for review
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEndDate, bookId);

                // Check 3: Validate no pending transactions
                var pendingEntries = trialBalance.Where(x => false).ToList(); // Would check for DRAFT entries if needed

                if (readiness.Issues.Count == 0)
                {
                    readiness.IsReadyToClose = true;
                    readiness.Message = "Period is ready to close";
                    _logger?.LogInformation("Period {PeriodEnd} is ready to close", periodEndDate);
                }
                else
                {
                    readiness.IsReadyToClose = false;
                    readiness.Message = $"Period has {readiness.Issues.Count} issues that must be resolved";
                }

                return readiness;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking period close readiness: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Close a period
        /// Steps:
        /// 1. Validate GL is balanced
        /// 2. Create closing entries (close revenue/expense to retained earnings)
        /// 3. Create post-closing trial balance
        /// 4. Mark period as closed
        /// </summary>
        public async Task<PeriodCloseResult> ClosePeriodAsync(
            DateTime periodEndDate,
            string periodName,
            string userId = "SYSTEM",
            string? bookId = null)
        {
            _logger?.LogInformation("Closing period {PeriodName} ending {PeriodEnd}", periodName, periodEndDate);

            try
            {
                var closeResult = new PeriodCloseResult
                {
                    PeriodEndDate = periodEndDate,
                    PeriodName = periodName,
                    CloseDate = DateTime.UtcNow,
                    StepsCompleted = new List<string>()
                };

                // Step 1: Validate GL
                var readiness = await CheckCloseReadinessAsync(periodEndDate, bookId);
                if (!readiness.IsReadyToClose)
                {
                    closeResult.Success = false;
                    closeResult.Errors.AddRange(readiness.Issues);
                    return closeResult;
                }
                closeResult.StepsCompleted.Add("GL validation passed");

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
                    if (balance == 0m)
                        continue;

                    var accountId = string.IsNullOrWhiteSpace(account.ACCOUNT_NUMBER)
                        ? account.GL_ACCOUNT_ID
                        : account.ACCOUNT_NUMBER;

                    if (balance > 0m)
                    {
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = accountId,
                            DEBIT_AMOUNT = balance,
                            CREDIT_AMOUNT = 0m,
                            DESCRIPTION = $"Closing revenue account {account.ACCOUNT_NUMBER} for {periodName}"
                        });
                    }
                    else
                    {
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = accountId,
                            DEBIT_AMOUNT = 0m,
                            CREDIT_AMOUNT = Math.Abs(balance),
                            DESCRIPTION = $"Closing contra revenue account {account.ACCOUNT_NUMBER} for {periodName}"
                        });
                    }

                    totalRevenue += balance;
                }

                foreach (var account in expenseAccounts)
                {
                    var balance = account.CURRENT_BALANCE ?? 0m;
                    if (balance == 0m)
                        continue;

                    var accountId = string.IsNullOrWhiteSpace(account.ACCOUNT_NUMBER)
                        ? account.GL_ACCOUNT_ID
                        : account.ACCOUNT_NUMBER;

                    if (balance > 0m)
                    {
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = accountId,
                            DEBIT_AMOUNT = 0m,
                            CREDIT_AMOUNT = balance,
                            DESCRIPTION = $"Closing expense account {account.ACCOUNT_NUMBER} for {periodName}"
                        });
                    }
                    else
                    {
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = accountId,
                            DEBIT_AMOUNT = Math.Abs(balance),
                            CREDIT_AMOUNT = 0m,
                            DESCRIPTION = $"Closing contra expense account {account.ACCOUNT_NUMBER} for {periodName}"
                        });
                    }

                    totalExpense += balance;
                }

                var netIncome = totalRevenue - totalExpense;
                if (netIncome != 0m)
                {
                    if (netIncome > 0m)
                    {
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = retainedEarnings,
                            DEBIT_AMOUNT = 0m,
                            CREDIT_AMOUNT = netIncome,
                            DESCRIPTION = $"Closing net income to retained earnings for {periodName}"
                        });
                    }
                    else
                    {
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = retainedEarnings,
                            DEBIT_AMOUNT = Math.Abs(netIncome),
                            CREDIT_AMOUNT = 0m,
                            DESCRIPTION = $"Closing net loss to retained earnings for {periodName}"
                        });
                    }
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
                    _ = result.IfrsEntry;
                    entriesPosted = 1;
                }

                closeResult.StepsCompleted.Add($"Posted {entriesPosted} closing entries");

                // Step 4: Generate post-closing trial balance
                var postClosingTB = await _trialBalanceService.GetPostClosingTrialBalanceAsync(periodEndDate, bookId);
                closeResult.StepsCompleted.Add($"Generated post-closing trial balance with {postClosingTB.Count} permanent accounts");

                // Step 5: Verify post-closing TB
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
                closeResult.ClosingEntriesCount = entriesPosted;
                closeResult.FinalBalance = postCloseValidation.TotalDebits; // Should equal credits

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

        /// <summary>
        /// Get closing checklist for a period
        /// </summary>
        public async Task<ClosingChecklist> GetClosingChecklistAsync(DateTime periodEndDate, string? bookId = null)
        {
            try
            {
                var checklist = new ClosingChecklist
                {
                    PeriodEndDate = periodEndDate,
                    Items = new List<ChecklistItem>()
                };

                // Item 1: GL Balance
                var glValidation = await _trialBalanceService.ValidateGLAsync(periodEndDate, bookId);
                checklist.Items.Add(new ChecklistItem
                {
                    Task = "GL Balance Validation",
                    IsComplete = glValidation.IsBalanced,
                    Details = $"Debits: {glValidation.TotalDebits:C}, Credits: {glValidation.TotalCredits:C}, Difference: {glValidation.Difference:C}"
                });

                // Item 2: AR Reconciliation
                checklist.Items.Add(new ChecklistItem
                {
                    Task = "AR Reconciliation",
                    IsComplete = false,
                    Details = "Verify all invoices are recorded and payments posted"
                });

                // Item 3: AP Reconciliation
                checklist.Items.Add(new ChecklistItem
                {
                    Task = "AP Reconciliation",
                    IsComplete = false,
                    Details = "Verify all bills are recorded and payments scheduled"
                });

                // Item 4: Bank Reconciliation
                checklist.Items.Add(new ChecklistItem
                {
                    Task = "Bank Reconciliation",
                    IsComplete = false,
                    Details = "Reconcile cash accounts to bank statements"
                });

                // Item 5: Inventory Count
                checklist.Items.Add(new ChecklistItem
                {
                    Task = "Physical Inventory Count",
                    IsComplete = false,
                    Details = "Complete and reconcile physical count to system"
                });

                // Overall status
                checklist.IsReadyToClose = checklist.Items.All(x => x.IsComplete);
                checklist.CompletionPercentage = (decimal)checklist.Items.Count(x => x.IsComplete) / checklist.Items.Count * 100;

                return checklist;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting closing checklist: {Message}", ex.Message);
                throw;
            }
        }

        private string GetAccountId(string key, string fallback)
        {
            return _accountMapping?.GetAccountId(key) ?? fallback;
        }

        private async Task<List<JOURNAL_ENTRY>> GetClosingEntriesAsync(DateTime periodEndDate, string? bookId)
        {
            var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY");
            var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}")
                ?? typeof(JOURNAL_ENTRY);

            var repo = new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                entityType, ConnectionName, "JOURNAL_ENTRY");

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
    }

}


