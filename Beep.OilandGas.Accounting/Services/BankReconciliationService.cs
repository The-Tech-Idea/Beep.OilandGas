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
    /// Bank Reconciliation Service - Reconcile GL cash to bank statements
    /// Identifies outstanding items and timing differences
    /// Critical for cash position validation and fraud detection
    /// </summary>
    public class BankReconciliationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<BankReconciliationService> _logger;
        private const string ConnectionName = "PPDM39";

        public BankReconciliationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<BankReconciliationService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Perform bank reconciliation
        /// </summary>
        public async Task<BankReconciliation> ReconcileBankAccountAsync(
            string accountNumber,
            decimal bankStatementBalance,
            DateTime statementDate,
            List<OutstandingCheck> outstandingChecks = null,
            List<DepositInTransit> depositsInTransit = null,
            string reportName = "")
        {
            _logger?.LogInformation("Performing bank reconciliation for account {Account} as of {Date}",
                accountNumber, statementDate.Date);

            try
            {
                if (string.IsNullOrWhiteSpace(accountNumber))
                    throw new ArgumentNullException(nameof(accountNumber));

                if (bankStatementBalance < 0)
                    throw new InvalidOperationException("Bank statement balance cannot be negative");

                if (string.IsNullOrWhiteSpace(reportName))
                    reportName = $"Bank Reconciliation - {accountNumber} as of {statementDate:MMMM dd, yyyy}";

                // Get GL account balance
                var account = await _glAccountService.GetAccountByNumberAsync(accountNumber);
                if (account == null)
                    throw new InvalidOperationException($"Account {accountNumber} not found");

                var glBalance = await _glAccountService.GetAccountBalanceAsync(accountNumber, statementDate);

                // Initialize reconciliation
                var reconciliation = new BankReconciliation
                {
                    ReportName = reportName,
                    GeneratedDate = DateTime.UtcNow,
                    AccountNumber = accountNumber,
                    StatementDate = statementDate,
                    BankStatementBalance = bankStatementBalance,
                    GLBalance = glBalance,
                    OutstandingChecks = outstandingChecks ?? new List<OutstandingCheck>(),
                    DepositsInTransit = depositsInTransit ?? new List<DepositInTransit>()
                };

                // Calculate outstanding checks total
                decimal totalOutstandingChecks = reconciliation.OutstandingChecks.Sum(x => x.Amount);
                reconciliation.TotalOutstandingChecks = totalOutstandingChecks;

                // Calculate deposits in transit total
                decimal totalDepositsInTransit = reconciliation.DepositsInTransit.Sum(x => x.Amount);
                reconciliation.TotalDepositsInTransit = totalDepositsInTransit;

                // Calculate reconciled GL balance
                // GL Balance = Bank Balance + Deposits in Transit - Outstanding Checks
                decimal reconciledGLBalance = bankStatementBalance + totalDepositsInTransit - totalOutstandingChecks;
                reconciliation.ReconciledGLBalance = reconciledGLBalance;

                // Check if reconciliation balances
                decimal difference = Math.Abs(glBalance - reconciledGLBalance);
                reconciliation.Difference = difference;
                reconciliation.IsReconciled = difference < 0.01m;

                if (reconciliation.IsReconciled)
                {
                    _logger?.LogInformation("Bank reconciliation successful. GL Balance: {Balance:C}, Reconciled Balance: {Reconciled:C}",
                        glBalance, reconciledGLBalance);
                }
                else
                {
                    _logger?.LogWarning("Bank reconciliation OUT OF BALANCE. GL Balance: {Balance:C}, Reconciled Balance: {Reconciled:C}, Difference: {Diff:C}",
                        glBalance, reconciledGLBalance, difference);
                }

                return reconciliation;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing bank reconciliation: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Analyze check clearing pattern
        /// </summary>
        public async Task<CheckClearingAnalysis> AnalyzeCheckClearingAsync(
            string accountNumber,
            DateTime periodStart,
            DateTime periodEnd)
        {
            _logger?.LogInformation("Analyzing check clearing pattern for account {Account} from {Start} to {End}",
                accountNumber, periodStart.Date, periodEnd.Date);

            try
            {
                if (string.IsNullOrWhiteSpace(accountNumber))
                    throw new ArgumentNullException(nameof(accountNumber));

                // Get AP payment data
                var metadata = await _metadata.GetTableMetadataAsync("AP_PAYMENT");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "AP_PAYMENT");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "PAYMENT_DATE", Operator = ">=", FilterValue = periodStart.ToString("yyyy-MM-dd") },
                    new AppFilter { FieldName = "PAYMENT_DATE", Operator = "<=", FilterValue = periodEnd.ToString("yyyy-MM-dd") }
                };

                var payments = await repo.GetAsync(filters) as List<AP_PAYMENT> ?? new List<AP_PAYMENT>();

                var analysis = new CheckClearingAnalysis
                {
                    AccountNumber = accountNumber,
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    AnalysisDate = DateTime.UtcNow,
                    TotalPayments = payments.Count,
                    TotalAmount = payments.Sum(x => x.PAYMENT_AMOUNT ?? 0m),
                    PaymentsByType = new List<PaymentTypeAnalysis>(),
                    AverageProcessingTime = CalculateAverageProcessingTime(payments)
                };

                // Group by payment method
                var groupedByMethod = payments.GroupBy(x => x.PAYMENT_METHOD ?? "UNKNOWN").ToList();
                foreach (var group in groupedByMethod)
                {
                    analysis.PaymentsByType.Add(new PaymentTypeAnalysis
                    {
                        PaymentMethod = group.Key,
                        Count = group.Count(),
                        TotalAmount = group.Sum(x => x.PAYMENT_AMOUNT ?? 0m),
                        AverageAmount = group.Average(x => x.PAYMENT_AMOUNT ?? 0m)
                    });
                }

                _logger?.LogInformation("Check clearing analysis completed. Total: {Total:C}, Payments: {Count}",
                    analysis.TotalAmount, analysis.TotalPayments);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing check clearing: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Identify aged outstanding items
        /// </summary>
        public async Task<AgedItemsReport> AnalyzeAgedOutstandingItemsAsync(
            string accountNumber,
            DateTime asOfDate)
        {
            _logger?.LogInformation("Analyzing aged outstanding items for account {Account} as of {Date}",
                accountNumber, asOfDate.Date);

            try
            {
                if (string.IsNullOrWhiteSpace(accountNumber))
                    throw new ArgumentNullException(nameof(accountNumber));

                var report = new AgedItemsReport
                {
                    AccountNumber = accountNumber,
                    AsOfDate = asOfDate,
                    AnalysisDate = DateTime.UtcNow,
                    Current = new List<AgedItem>(),
                    _30to60Days = new List<AgedItem>(),
                    _60to90Days = new List<AgedItem>(),
                    Over90Days = new List<AgedItem>()
                };

                // Get GL entries for the account
                var metadata = await _metadata.GetTableMetadataAsync("JOURNAL_ENTRY_LINE");
                var entityType = Type.GetType($"Beep.OilandGas.Models.Data.ProductionAccounting.{metadata.EntityTypeName}");
                var repo = new PPDMGenericRepository(
                    _editor, _commonColumnHandler, _defaults, _metadata,
                    entityType, ConnectionName, "JOURNAL_ENTRY_LINE");

                var filters = new List<AppFilter>
                {
                    new AppFilter { FieldName = "GL_ACCOUNT_NUMBER", Operator = "=", FilterValue = accountNumber }
                };

                var entries = await repo.GetAsync(filters) as List<JOURNAL_ENTRY_LINE> ?? new List<JOURNAL_ENTRY_LINE>();

                // Classify by age
                foreach (var entry in entries.Where(x => (x.DEBIT_AMOUNT ?? 0m) > 0 || (x.CREDIT_AMOUNT ?? 0m) > 0))
                {
                    var entryDate = entry.ROW_CREATED_DATE ?? DateTime.Today;
                    int ageInDays = (int)(asOfDate - entryDate).TotalDays;

                    var agedItem = new AgedItem
                    {
                        EntryDate = entryDate,
                        Description = entry.DESCRIPTION,
                        Amount = (entry.DEBIT_AMOUNT ?? 0m) > 0 ? (entry.DEBIT_AMOUNT ?? 0m) : (entry.CREDIT_AMOUNT ?? 0m),
                        AgeInDays = ageInDays,
                        Reference = entry.ROW_ID
                    };

                    if (ageInDays <= 30)
                        report.Current.Add(agedItem);
                    else if (ageInDays <= 60)
                        report._30to60Days.Add(agedItem);
                    else if (ageInDays <= 90)
                        report._60to90Days.Add(agedItem);
                    else
                        report.Over90Days.Add(agedItem);
                }

                // Calculate totals by aging bucket
                report.CurrentTotal = report.Current.Sum(x => x.Amount);
                report._30to60Total = report._30to60Days.Sum(x => x.Amount);
                report._60to90Total = report._60to90Days.Sum(x => x.Amount);
                report.Over90Total = report.Over90Days.Sum(x => x.Amount);
                report.GrandTotal = report.CurrentTotal + report._30to60Total + report._60to90Total + report.Over90Total;

                _logger?.LogInformation("Aged items analysis completed. Total Items: {Total}, Grand Total: {Amount:C}",
                    report.Current.Count + report._30to60Days.Count + report._60to90Days.Count + report.Over90Days.Count,
                    report.GrandTotal);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing aged outstanding items: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export bank reconciliation as formatted text
        /// </summary>
        public string ExportBankReconciliationAsText(BankReconciliation reconciliation)
        {
            _logger?.LogInformation("Exporting bank reconciliation as text");

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine(reconciliation.ReportName);
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine("BANK RECONCILIATION");
                sb.AppendLine("─────────────────────────────────────────────────────────────────");
                sb.AppendLine($"Account Number:              {reconciliation.AccountNumber}");
                sb.AppendLine($"Statement Date:              {reconciliation.StatementDate:MMMM dd, yyyy}");
                sb.AppendLine();

                sb.AppendLine("RECONCILIATION CALCULATION:");
                sb.AppendLine($"  Bank Statement Balance:    ${reconciliation.BankStatementBalance,15:N2}");
                sb.AppendLine($"  Add: Deposits in Transit:  ${reconciliation.TotalDepositsInTransit,15:N2}");
                sb.AppendLine($"  Less: Outstanding Checks:  ${reconciliation.TotalOutstandingChecks,15:N2}");
                sb.AppendLine("  ─────────────────────────────────────");
                sb.AppendLine($"  Reconciled GL Balance:     ${reconciliation.ReconciledGLBalance,15:N2}");
                sb.AppendLine();

                sb.AppendLine($"GL BALANCE FROM RECORDS:    ${reconciliation.GLBalance,15:N2}");
                sb.AppendLine();

                sb.AppendLine("RECONCILIATION STATUS:");
                if (reconciliation.IsReconciled)
                {
                    sb.AppendLine("✓ RECONCILED - Bank and GL accounts match");
                }
                else
                {
                    sb.AppendLine("✗ OUT OF BALANCE");
                    sb.AppendLine($"  Difference: ${reconciliation.Difference:N2}");
                }

                sb.AppendLine();
                sb.AppendLine("OUTSTANDING CHECKS:");
                sb.AppendLine("Check# | Amount      | Date");
                sb.AppendLine("─────────────────────────────────────");
                foreach (var check in reconciliation.OutstandingChecks)
                {
                    sb.AppendLine($"{check.CheckNumber,-6} | ${check.Amount,11:N2} | {check.CheckDate:MM/dd/yyyy}");
                }
                sb.AppendLine($"Total Outstanding:         ${reconciliation.TotalOutstandingChecks,15:N2}");
                sb.AppendLine();

                sb.AppendLine("DEPOSITS IN TRANSIT:");
                sb.AppendLine("Amount      | Date");
                sb.AppendLine("─────────────────────────────────────");
                foreach (var deposit in reconciliation.DepositsInTransit)
                {
                    sb.AppendLine($"${deposit.Amount,11:N2} | {deposit.DepositDate:MM/dd/yyyy}");
                }
                sb.AppendLine($"Total Deposits in Transit:  ${reconciliation.TotalDepositsInTransit,15:N2}");
                sb.AppendLine();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"Report Generated: {reconciliation.GeneratedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting bank reconciliation: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate average check clearing time in days
        /// </summary>
        private int CalculateAverageProcessingTime(List<AP_PAYMENT> payments)
        {
            if (!payments.Any())
                return 0;

            var clearingTimes = new List<int>();
            foreach (var payment in payments.Where(x => x.PAYMENT_DATE.HasValue && x.ROW_CHANGED_DATE.HasValue))
            {
                int days = (int)(payment.ROW_CHANGED_DATE.Value - payment.PAYMENT_DATE.Value).TotalDays;
                if (days >= 0 && days <= 60)  // Reasonable processing time
                    clearingTimes.Add(days);
            }

            return clearingTimes.Any() ? (int)clearingTimes.Average() : 0;
        }
    }

    /// <summary>
    /// Bank Reconciliation Result
    /// </summary>
    public class BankReconciliation
    {
        public string ReportName { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string AccountNumber { get; set; }
        public DateTime StatementDate { get; set; }
        public decimal BankStatementBalance { get; set; }
        public decimal GLBalance { get; set; }
        public decimal TotalOutstandingChecks { get; set; }
        public decimal TotalDepositsInTransit { get; set; }
        public decimal ReconciledGLBalance { get; set; }
        public decimal Difference { get; set; }
        public bool IsReconciled { get; set; }
        public List<OutstandingCheck> OutstandingChecks { get; set; }
        public List<DepositInTransit> DepositsInTransit { get; set; }
    }

    /// <summary>
    /// Outstanding Check
    /// </summary>
    public class OutstandingCheck
    {
        public string CheckNumber { get; set; }
        public decimal Amount { get; set; }
        public DateTime CheckDate { get; set; }
    }

    /// <summary>
    /// Deposit In Transit
    /// </summary>
    public class DepositInTransit
    {
        public decimal Amount { get; set; }
        public DateTime DepositDate { get; set; }
    }

    /// <summary>
    /// Check Clearing Analysis
    /// </summary>
    public class CheckClearingAnalysis
    {
        public string AccountNumber { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime AnalysisDate { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public int AverageProcessingTime { get; set; }
        public List<PaymentTypeAnalysis> PaymentsByType { get; set; }
    }

    /// <summary>
    /// Payment Type Analysis
    /// </summary>
    public class PaymentTypeAnalysis
    {
        public string PaymentMethod { get; set; }
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
    }

    /// <summary>
    /// Aged Outstanding Items Report
    /// </summary>
    public class AgedItemsReport
    {
        public string AccountNumber { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<AgedItem> Current { get; set; }
        public List<AgedItem> _30to60Days { get; set; }
        public List<AgedItem> _60to90Days { get; set; }
        public List<AgedItem> Over90Days { get; set; }
        public decimal CurrentTotal { get; set; }
        public decimal _30to60Total { get; set; }
        public decimal _60to90Total { get; set; }
        public decimal Over90Total { get; set; }
        public decimal GrandTotal { get; set; }
    }

    /// <summary>
    /// Aged Item
    /// </summary>
    public class AgedItem
    {
        public DateTime EntryDate { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int AgeInDays { get; set; }
        public string Reference { get; set; }
    }
}
