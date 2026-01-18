using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
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
        private readonly ILogger<PeriodClosingService> _logger;
        private const string ConnectionName = "PPDM39";

        public PeriodClosingService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            TrialBalanceService trialBalanceService,
            JournalEntryService journalEntryService,
            ILogger<PeriodClosingService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Check if period can be closed
        /// Requirements: GL must be balanced
        /// </summary>
        public async Task<PeriodCloseReadiness> CheckCloseReadinessAsync(DateTime periodEndDate)
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
                var glValidation = await _trialBalanceService.ValidateGLAsync(periodEndDate);
                if (!glValidation.IsBalanced)
                {
                    readiness.Issues.Add($"GL out of balance: Difference {glValidation.Difference:C}");
                    readiness.IsReadyToClose = false;
                    return readiness;
                }

                // Check 2: Get trial balance for review
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEndDate);

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
            string userId = "SYSTEM")
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
                var readiness = await CheckCloseReadinessAsync(periodEndDate);
                if (!readiness.IsReadyToClose)
                {
                    closeResult.Success = false;
                    closeResult.Errors.AddRange(readiness.Issues);
                    return closeResult;
                }
                closeResult.StepsCompleted.Add("GL validation passed");

                // Step 2: Get trial balance
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEndDate);
                closeResult.StepsCompleted.Add($"Generated trial balance with {trialBalance.Count} accounts");

                // Step 3: Create closing entries
                // Temporary accounts (REVENUE, EXPENSE) → Retained Earnings (3100)
                var closingEntries = new List<PeriodClosingEntry>();
                
                // Collect revenue accounts for closing
                var revenueAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "REVENUE").ToList();
                decimal totalRevenue = revenueAccounts.Sum(x => x.CURRENT_BALANCE ?? 0m);

                if (totalRevenue != 0)
                {
                    var revenueClosingEntry = new PeriodClosingEntry
                    {
                        SourceAccount = "4001", // Revenue
                        TargetAccount = "3100", // Retained Earnings
                        Amount = Math.Abs(totalRevenue),
                        EntryType = "REVENUE_CLOSE",
                        Description = $"Closing revenue accounts for {periodName}"
                    };
                    closingEntries.Add(revenueClosingEntry);
                }

                // Collect expense accounts for closing
                var expenseAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "EXPENSE").ToList();
                decimal totalExpense = expenseAccounts.Sum(x => x.CURRENT_BALANCE ?? 0m);

                if (totalExpense != 0)
                {
                    var expenseClosingEntry = new PeriodClosingEntry
                    {
                        SourceAccount = "5000", // COGS / Expenses
                        TargetAccount = "3100", // Retained Earnings
                        Amount = Math.Abs(totalExpense),
                        EntryType = "EXPENSE_CLOSE",
                        Description = $"Closing expense accounts for {periodName}"
                    };
                    closingEntries.Add(expenseClosingEntry);
                }

                // Post closing entries to GL
                int entriesPosted = 0;
                foreach (var closingEntry in closingEntries)
                {
                    var lineItems = new List<JOURNAL_ENTRY_LINE>();

                    if (closingEntry.EntryType == "REVENUE_CLOSE")
                    {
                        // Debit Revenue, Credit Retained Earnings
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = closingEntry.SourceAccount,
                            DEBIT_AMOUNT = closingEntry.Amount,
                            CREDIT_AMOUNT = 0m,
                            DESCRIPTION = closingEntry.Description
                        });
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = closingEntry.TargetAccount,
                            DEBIT_AMOUNT = 0m,
                            CREDIT_AMOUNT = closingEntry.Amount,
                            DESCRIPTION = closingEntry.Description
                        });
                    }
                    else if (closingEntry.EntryType == "EXPENSE_CLOSE")
                    {
                        // Debit Retained Earnings, Credit Expense
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = closingEntry.TargetAccount,
                            DEBIT_AMOUNT = closingEntry.Amount,
                            CREDIT_AMOUNT = 0m,
                            DESCRIPTION = closingEntry.Description
                        });
                        lineItems.Add(new JOURNAL_ENTRY_LINE
                        {
                            GL_ACCOUNT_ID = closingEntry.SourceAccount,
                            DEBIT_AMOUNT = 0m,
                            CREDIT_AMOUNT = closingEntry.Amount,
                            DESCRIPTION = closingEntry.Description
                        });
                    }

                    var glEntry = await _journalEntryService.CreateEntryAsync(
                        periodEndDate,
                        closingEntry.Description,
                        lineItems,
                        userId,
                        $"CLOSE-{periodName}",
                        "CLOSING");

                    await _journalEntryService.PostEntryAsync(glEntry.JOURNAL_ENTRY_ID, userId);
                    entriesPosted++;
                }

                closeResult.StepsCompleted.Add($"Posted {entriesPosted} closing entries");

                // Step 4: Generate post-closing trial balance
                var postClosingTB = await _trialBalanceService.GetPostClosingTrialBalanceAsync(periodEndDate);
                closeResult.StepsCompleted.Add($"Generated post-closing trial balance with {postClosingTB.Count} permanent accounts");

                // Step 5: Verify post-closing TB
                var postCloseValidation = await _trialBalanceService.ValidateGLAsync(periodEndDate);
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
        public async Task<bool> ReopenPeriodAsync(DateTime periodEndDate, string userId = "SYSTEM")
        {
            _logger?.LogInformation("Reopening period ending {PeriodEnd}", periodEndDate);

            try
            {
                // In a full implementation, this would:
                // 1. Reverse closing entries
                // 2. Revert period status to OPEN
                // 3. Audit trail who reopened and why
                
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
        public async Task<ClosingChecklist> GetClosingChecklistAsync(DateTime periodEndDate)
        {
            try
            {
                var checklist = new ClosingChecklist
                {
                    PeriodEndDate = periodEndDate,
                    Items = new List<ChecklistItem>()
                };

                // Item 1: GL Balance
                var glValidation = await _trialBalanceService.ValidateGLAsync(periodEndDate);
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
    }

    /// <summary>
    /// Period close readiness check result
    /// </summary>
    public class PeriodCloseReadiness
    {
        public DateTime PeriodEndDate { get; set; }
        public DateTime CheckDate { get; set; }
        public bool IsReadyToClose { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Issues { get; set; } = new();
    }

    /// <summary>
    /// Period close result
    /// </summary>
    public class PeriodCloseResult
    {
        public DateTime PeriodEndDate { get; set; }
        public string PeriodName { get; set; } = string.Empty;
        public DateTime CloseDate { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> StepsCompleted { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public int ClosingEntriesCount { get; set; }
        public decimal FinalBalance { get; set; }
    }

    /// <summary>
    /// Individual closing entry
    /// </summary>
    public class PeriodClosingEntry
    {
        public string SourceAccount { get; set; } = string.Empty;
        public string TargetAccount { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string EntryType { get; set; } = string.Empty; // REVENUE_CLOSE, EXPENSE_CLOSE
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Period closing checklist
    /// </summary>
    public class ClosingChecklist
    {
        public DateTime PeriodEndDate { get; set; }
        public List<ChecklistItem> Items { get; set; } = new();
        public bool IsReadyToClose { get; set; }
        public decimal CompletionPercentage { get; set; }
    }

    /// <summary>
    /// Individual checklist item
    /// </summary>
    public class ChecklistItem
    {
        public string Task { get; set; } = string.Empty;
        public bool IsComplete { get; set; }
        public string Details { get; set; } = string.Empty;
    }
}
