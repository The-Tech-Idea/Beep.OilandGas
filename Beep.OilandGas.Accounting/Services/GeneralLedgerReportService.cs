using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// General Ledger Report Service - Generate detailed GL reports
    /// Provides GL detail, summary, and drill-down capability
    /// Critical for audit trail and transaction verification
    /// </summary>
    public class GeneralLedgerReportService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<GeneralLedgerReportService> _logger;
        private const string ConnectionName = "PPDM39";

        public GeneralLedgerReportService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<GeneralLedgerReportService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate detailed GL report with all transactions for an account
        /// </summary>
        public async Task<GLDetailReport> GenerateGLDetailReportAsync(
            string accountNumber,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string reportName = "",
            string? bookId = null)
        {
            _logger?.LogInformation("Generating GL detail report for account {Account} from {Start} to {End}",
                accountNumber, startDate?.Date ?? DateTime.MinValue, endDate?.Date ?? DateTime.MaxValue);

            try
            {
                if (string.IsNullOrWhiteSpace(accountNumber))
                    throw new ArgumentNullException(nameof(accountNumber));

                if (string.IsNullOrWhiteSpace(reportName))
                    reportName = $"GL Detail Report - Account {accountNumber}";

                // Get account details
                var account = await _glAccountService.GetAccountByNumberAsync(accountNumber);
                if (account == null)
                    throw new InvalidOperationException($"Account {accountNumber} not found");

                // Get GL entries for this account
                var repo = await GetRepoAsync<JOURNAL_ENTRY_LINE>("JOURNAL_ENTRY_LINE", ConnectionName);

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "GL_ACCOUNT_NUMBER", Operator = "=", FilterValue = accountNumber }
                };
                if (!string.IsNullOrWhiteSpace(bookId))
                    filters.Add(new AppFilter { FieldName = "SOURCE", Operator = "=", FilterValue = bookId });

                if (startDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "ENTRY_DATE", Operator = ">=", FilterValue = startDate.Value.ToString("yyyy-MM-dd") });

                if (endDate.HasValue)
                    filters.Add(new AppFilter { FieldName = "ENTRY_DATE", Operator = "<=", FilterValue = endDate.Value.ToString("yyyy-MM-dd") });

                var entries = await repo.GetAsync(filters) as List<JOURNAL_ENTRY_LINE> ?? new List<JOURNAL_ENTRY_LINE>();

                // Build report
                var report = new GLDetailReport
                {
                    ReportName = reportName,
                    GeneratedDate = DateTime.UtcNow,
                    AccountNumber = accountNumber,
                    AccountName = account.ACCOUNT_NAME,
                    AccountType = account.ACCOUNT_TYPE,
                    NormalBalance = account.NORMAL_BALANCE,
                    StartDate = startDate,
                    EndDate = endDate,
                    GLEntries = new List<GLEntryLine>()
                };

                decimal balance = 0m;
                decimal totalDebits = 0m;
                decimal totalCredits = 0m;

                // Sort by ROW_CREATED_DATE (entry creation date)
                var sortedEntries = entries.OrderBy(x => x.ROW_CREATED_DATE).ThenBy(x => x.LINE_NUMBER).ToList();

                foreach (var entry in sortedEntries)
                {
                    decimal debitAmount = entry.DEBIT_AMOUNT ?? 0m;
                    decimal creditAmount = entry.CREDIT_AMOUNT ?? 0m;

                    // Calculate running balance
                    if (debitAmount > 0)
                        balance += debitAmount;
                    else if (creditAmount > 0)
                        balance -= creditAmount;

                    totalDebits += debitAmount;
                    totalCredits += creditAmount;

                    report.GLEntries.Add(new GLEntryLine
                    {
                        EntryDate = entry.ROW_CREATED_DATE ?? DateTime.Today,
                        Description = entry.DESCRIPTION,
                        Reference = entry.ROW_ID,
                        EntryType = "JOURNAL_ENTRY",
                        DebitAmount = debitAmount,
                        CreditAmount = creditAmount,
                        RunningBalance = balance
                    });
                }

                report.TotalDebits = totalDebits;
                report.TotalCredits = totalCredits;
                report.EndingBalance = balance;

                _logger?.LogInformation("GL Detail Report generated. Entries: {Count}, Debits: {Debits:C}, Credits: {Credits:C}",
                    entries.Count, totalDebits, totalCredits);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating GL detail report: {Message}", ex.Message);
                throw;
            }
        }

        private async Task<PPDMGenericRepository> GetRepoAsync<T>(string tableName, string cn)
        {
            var metadata = await _metadata.GetTableMetadataAsync(tableName);
            
            return new PPDMGenericRepository(
                _editor, _commonColumnHandler, _defaults, _metadata,
                typeof(T), cn, tableName);
        }

        /// <summary>
        /// Generate GL summary report by account
        /// </summary>
        public async Task<GLSummaryReport> GenerateGLSummaryReportAsync(
            DateTime? asOfDate = null,
            string reportName = "",
            string? bookId = null)
        {
            _logger?.LogInformation("Generating GL summary report as of {Date}", asOfDate?.Date ?? DateTime.Today);

            try
            {
                if (string.IsNullOrWhiteSpace(reportName))
                    reportName = $"GL Summary Report as of {(asOfDate?.Date ?? DateTime.Today):MMMM dd, yyyy}";

                // Get all accounts
                var accounts = await _glAccountService.GetAllAccountsAsync();

                var report = new GLSummaryReport
                {
                    ReportName = reportName,
                    GeneratedDate = DateTime.UtcNow,
                    AsOfDate = asOfDate ?? DateTime.Today,
                    Accounts = new List<GLSummaryLine>()
                };

                decimal totalDebits = 0m;
                decimal totalCredits = 0m;

                foreach (var account in accounts)
                {
                    var balance = await _glAccountService.GetAccountBalanceAsync(account.ACCOUNT_NUMBER, asOfDate, bookId);

                    // Only include accounts with balance
                    if (Math.Abs(balance) > 0.01m)
                    {
                        var summaryLine = new GLSummaryLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            AccountType = account.ACCOUNT_TYPE,
                            NormalBalance = account.NORMAL_BALANCE,
                            Balance = balance
                        };

                        report.Accounts.Add(summaryLine);

                        // Track debits and credits by normal balance
                        if (balance >= 0)
                        {
                            if (account.NORMAL_BALANCE == "DEBIT")
                                totalDebits += balance;
                            else
                                totalCredits += balance;
                        }
                        else  // balance < 0
                        {
                            if (account.NORMAL_BALANCE == "DEBIT")
                                totalCredits += Math.Abs(balance);
                            else
                                totalDebits += Math.Abs(balance);
                        }
                    }
                }

                report.TotalDebits = totalDebits;
                report.TotalCredits = totalCredits;
                report.IsBalanced = Math.Abs(totalDebits - totalCredits) < 0.01m;

                // Sort by account number
                report.Accounts = report.Accounts.OrderBy(x => x.AccountNumber).ToList();

                _logger?.LogInformation("GL Summary Report generated. Accounts: {Count}, Balanced: {Balanced}",
                    report.Accounts.Count, report.IsBalanced);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating GL summary report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate GL report by account type
        /// </summary>
        public async Task<List<GLSummaryLine>> GenerateGLByTypeReportAsync(
            string accountType,
            DateTime? asOfDate = null)
        {
            _logger?.LogInformation("Generating GL report for account type {Type}", accountType);

            try
            {
                if (string.IsNullOrWhiteSpace(accountType))
                    throw new ArgumentNullException(nameof(accountType));

                var accounts = await _glAccountService.GetAllAccountsAsync();
                var typeAccounts = accounts.Where(x => x.ACCOUNT_TYPE == accountType).ToList();

                var results = new List<GLSummaryLine>();

                foreach (var account in typeAccounts)
                {
                    var balance = await _glAccountService.GetAccountBalanceAsync(account.ACCOUNT_NUMBER, asOfDate);

                    if (Math.Abs(balance) > 0.01m)
                    {
                        results.Add(new GLSummaryLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            AccountType = account.ACCOUNT_TYPE,
                            NormalBalance = account.NORMAL_BALANCE,
                            Balance = balance
                        });
                    }
                }

                return results.OrderBy(x => x.AccountNumber).ToList();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating GL type report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export GL report as formatted text
        /// </summary>
        public string ExportGLDetailReportAsText(GLDetailReport report)
        {
            _logger?.LogInformation("Exporting GL detail report as text");

            try
            {
                var sb = new StringBuilder();

                // HEADER
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine(report.ReportName);
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                // ACCOUNT INFO
                sb.AppendLine($"Account Number:        {report.AccountNumber}");
                sb.AppendLine($"Account Name:          {report.AccountName}");
                sb.AppendLine($"Account Type:          {report.AccountType}");
                sb.AppendLine($"Normal Balance:        {report.NormalBalance}");
                sb.AppendLine($"Period:                {report.StartDate?.Date:MM/dd/yyyy} to {report.EndDate?.Date:MM/dd/yyyy}");
                sb.AppendLine();

                // TRANSACTIONS
                sb.AppendLine("Date        | Reference           | Description              | Debit      | Credit     | Balance");
                sb.AppendLine("─────────────────────────────────────────────────────────────────────────────────────────────────────");

                foreach (var entry in report.GLEntries)
                {
                    sb.AppendLine($"{entry.EntryDate:MM/dd/yyyy} | {entry.Reference,-19} | {entry.Description,-24} | {entry.DebitAmount,10:N2} | {entry.CreditAmount,10:N2} | {entry.RunningBalance,10:N2}");
                }

                sb.AppendLine("═════════════════════════════════════════════════════════════════════════════════════════════════════");
                sb.AppendLine($"{"TOTALS",-47} | {report.TotalDebits,10:N2} | {report.TotalCredits,10:N2} | {report.EndingBalance,10:N2}");
                sb.AppendLine("═════════════════════════════════════════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Ending Balance: {report.EndingBalance:C}");
                sb.AppendLine($"Report Generated: {report.GeneratedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting GL detail report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export GL summary as formatted text
        /// </summary>
        public string ExportGLSummaryReportAsText(GLSummaryReport report)
        {
            _logger?.LogInformation("Exporting GL summary report as text");

            try
            {
                var sb = new StringBuilder();

                // HEADER
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine(report.ReportName);
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                // GROUP BY TYPE
                var groupedByType = report.Accounts.GroupBy(x => x.AccountType).OrderBy(x => x.Key).ToList();

                foreach (var typeGroup in groupedByType)
                {
                    sb.AppendLine($"\n{typeGroup.Key} ACCOUNTS:");
                    sb.AppendLine("─────────────────────────────────────────────────────────────");
                    sb.AppendLine("Account# | Account Name                      | Debit      | Credit     | Balance");
                    sb.AppendLine("─────────────────────────────────────────────────────────────");

                    decimal typeDebits = 0m;
                    decimal typeCredits = 0m;

                    foreach (var account in typeGroup.OrderBy(x => x.AccountNumber))
                    {
                        decimal displayBalance = account.Balance;
                        decimal debit = 0m;
                        decimal credit = 0m;

                        if (displayBalance >= 0)
                        {
                            if (account.NormalBalance == "DEBIT")
                                debit = displayBalance;
                            else
                                credit = displayBalance;
                        }
                        else
                        {
                            if (account.NormalBalance == "DEBIT")
                                credit = Math.Abs(displayBalance);
                            else
                                debit = Math.Abs(displayBalance);
                        }

                        typeDebits += debit;
                        typeCredits += credit;

                        sb.AppendLine($"{account.AccountNumber,-8} | {account.AccountName,-35} | {debit,10:N2} | {credit,10:N2} | {displayBalance,10:N2}");
                    }

                    sb.AppendLine($"{"Subtotal",-46} | {typeDebits,10:N2} | {typeCredits,10:N2}");
                    sb.AppendLine();
                }

                // TOTAL
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"{"GRAND TOTAL",-46} | {report.TotalDebits,10:N2} | {report.TotalCredits,10:N2}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Balance Status: {(report.IsBalanced ? "✓ BALANCED" : "✗ OUT OF BALANCE")}");
                sb.AppendLine($"Report Generated: {report.GeneratedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting GL summary report: {Message}", ex.Message);
                throw;
            }
        }
    }
}
