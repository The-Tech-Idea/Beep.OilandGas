using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Trial Balance Service - GL validation and period-close reporting
    /// Critical for period closing: GL must be BALANCED before close
    /// Works directly with GL_ACCOUNT entities
    /// </summary>
    public class TrialBalanceService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<TrialBalanceService> _logger;
        private const string ConnectionName = "PPDM39";

        // Tolerance for balanced check (0.01%)
        private const decimal BalanceTolerance = 0.0001m;

        public TrialBalanceService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<TrialBalanceService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate full trial balance
        /// Returns list of GL accounts with calculated balances
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GenerateTrialBalanceAsync(DateTime? asOfDate = null, string? bookId = null)
        {
            _logger?.LogInformation("Generating trial balance as of {AsOfDate}", asOfDate?.Date ?? DateTime.Today);

            try
            {
                // Get all GL accounts
                var accounts = await _glAccountService.GetAllAccountsAsync();

                foreach (var account in accounts)
                {
                    var balance = await _glAccountService.GetAccountBalanceAsync(account.ACCOUNT_NUMBER, asOfDate, bookId);
                    account.CURRENT_BALANCE = balance;
                }

                return accounts;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating trial balance: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get trial balance filtered by account type
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GetTrialBalanceByTypeAsync(string accountType, DateTime? asOfDate = null, string? bookId = null)
        {
            _logger?.LogInformation("Getting trial balance for type {AccountType}", accountType);

            try
            {
                var fullTB = await GenerateTrialBalanceAsync(asOfDate, bookId);
                return fullTB.Where(x => x.ACCOUNT_TYPE == accountType).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting trial balance by type {AccountType}: {Message}", accountType, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Validate GL is balanced (Debits = Credits)
        /// </summary>
        public async Task<(bool IsBalanced, decimal TotalDebits, decimal TotalCredits, decimal Difference)> ValidateGLAsync(DateTime? asOfDate = null, string? bookId = null)
        {
            _logger?.LogInformation("Validating GL balance");

            try
            {
                var trialBalance = await GenerateTrialBalanceAsync(asOfDate, bookId);
                
                decimal totalDebits = 0m;
                decimal totalCredits = 0m;

                foreach (var account in trialBalance)
                {
                    decimal balance = account.CURRENT_BALANCE ?? 0m;

                    // Determine if debit or credit based on normal balance and calculated balance
                    if (balance >= 0)
                    {
                        if (account.NORMAL_BALANCE == "DEBIT")
                        {
                            totalDebits += balance;
                        }
                        else // CREDIT
                        {
                            totalCredits += balance;
                        }
                    }
                    else // balance < 0 (contra-balance)
                    {
                        if (account.NORMAL_BALANCE == "DEBIT")
                        {
                            totalCredits += Math.Abs(balance);
                        }
                        else // CREDIT
                        {
                            totalDebits += Math.Abs(balance);
                        }
                    }
                }

                decimal difference = Math.Abs(totalDebits - totalCredits);
                bool isBalanced = difference <= BalanceTolerance;

                if (!isBalanced)
                {
                    _logger?.LogError("Trial Balance OUT OF BALANCE: Debits {Debits}, Credits {Credits}, Difference {Diff}",
                        totalDebits, totalCredits, difference);
                }
                else
                {
                    _logger?.LogInformation("Trial Balance OK: Total {Total:C}", totalDebits);
                }

                return (isBalanced, totalDebits, totalCredits, difference);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error validating GL: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get post-closing trial balance (permanent accounts only: Assets, Liabilities, Equity)
        /// Temporary accounts (Revenue, Expense) are closed
        /// </summary>
        public async Task<List<GL_ACCOUNT>> GetPostClosingTrialBalanceAsync(DateTime? asOfDate = null, string? bookId = null)
        {
            _logger?.LogInformation("Generating post-closing trial balance");

            try
            {
                var fullTB = await GenerateTrialBalanceAsync(asOfDate, bookId);
                
                // Post-closing TB only includes permanent accounts (Assets, Liabilities, Equity)
                var permanentTypes = new[] { "ASSET", "LIABILITY", "EQUITY" };
                return fullTB.Where(x => permanentTypes.Contains(x.ACCOUNT_TYPE)).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting post-closing trial balance: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export trial balance to CSV format
        /// </summary>
        public async Task<string> ExportToCSVAsync(DateTime? asOfDate = null, string? bookId = null)
        {
            try
            {
                var trialBalance = await GenerateTrialBalanceAsync(asOfDate, bookId);
                var csv = new StringBuilder();

                // Header
                csv.AppendLine("Account Number,Account Name,Account Type,Normal Balance,Current Balance");

                // Rows
                foreach (var account in trialBalance.OrderBy(x => x.ACCOUNT_NUMBER))
                {
                    decimal balance = account.CURRENT_BALANCE ?? 0m;
                    csv.AppendLine($"\"{account.ACCOUNT_NUMBER}\",\"{account.ACCOUNT_NAME}\",\"{account.ACCOUNT_TYPE}\"," +
                        $"\"{account.NORMAL_BALANCE}\",{balance:F2}");
                }

                // Summary
                var validation = await ValidateGLAsync(asOfDate, bookId);
                csv.AppendLine();
                csv.AppendLine($"Total Debits,{validation.TotalDebits:F2}");
                csv.AppendLine($"Total Credits,{validation.TotalCredits:F2}");
                csv.AppendLine($"Difference,{validation.Difference:F2}");
                csv.AppendLine($"Balanced,{validation.IsBalanced}");

                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting trial balance to CSV: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Check if period close is ready
        /// GL must be balanced before closing period
        /// </summary>
        public async Task<bool> CanClosePeriodAsync(DateTime? asOfDate = null, string? bookId = null)
        {
            try
            {
                var validation = await ValidateGLAsync(asOfDate, bookId);
                
                if (!validation.IsBalanced)
                {
                    _logger?.LogWarning("Cannot close period: GL is out of balance (Difference: {Difference})", validation.Difference);
                }

                return validation.IsBalanced;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking period close readiness: {Message}", ex.Message);
                throw;
            }
        }
    }
}
