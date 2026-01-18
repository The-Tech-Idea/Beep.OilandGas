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
    /// Financial Statement Service - Generate P&L, Balance Sheet, and Cash Flow statements
    /// Uses GL_ACCOUNT balances to produce FASB-compliant financial statements
    /// Core reporting service for executive dashboards and regulatory filings
    /// </summary>
    public class FinancialStatementService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly TrialBalanceService _trialBalanceService;
        private readonly ILogger<FinancialStatementService> _logger;
        private const string ConnectionName = "PPDM39";

        public FinancialStatementService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            TrialBalanceService trialBalanceService,
            ILogger<FinancialStatementService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _trialBalanceService = trialBalanceService ?? throw new ArgumentNullException(nameof(trialBalanceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generate Income Statement (P&L)
        /// Revenue - Expenses = Net Income
        /// </summary>
        public async Task<IncomeStatement> GenerateIncomeStatementAsync(DateTime periodStart, DateTime periodEnd, string periodName = "")
        {
            _logger?.LogInformation("Generating Income Statement for period {PeriodStart} to {PeriodEnd}", periodStart, periodEnd);

            try
            {
                if (string.IsNullOrWhiteSpace(periodName))
                    periodName = $"{periodStart:MMM yyyy} to {periodEnd:MMM yyyy}";

                // Get trial balance as of period end
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEnd);

                // Initialize P&L statement
                var incomeStatement = new IncomeStatement
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    PeriodName = periodName,
                    GeneratedDate = DateTime.UtcNow,
                    Revenues = new List<IncomeStatementLine>(),
                    CostOfGoods = new List<IncomeStatementLine>(),
                    OperatingExpenses = new List<IncomeStatementLine>(),
                    OtherIncomeExpense = new List<IncomeStatementLine>()
                };

                // REVENUE SECTION (Account Type = REVENUE)
                var revenueAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "REVENUE").ToList();
                decimal totalRevenue = 0m;

                foreach (var account in revenueAccounts)
                {
                    decimal balance = Math.Abs(account.CURRENT_BALANCE ?? 0m);
                    if (balance != 0)
                    {
                        incomeStatement.Revenues.Add(new IncomeStatementLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            Amount = balance
                        });
                        totalRevenue += balance;
                    }
                }

                incomeStatement.TotalRevenues = totalRevenue;

                // COGS SECTION (Account Type = COGS)
                var cogsAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "COGS").ToList();
                decimal totalCOGS = 0m;

                foreach (var account in cogsAccounts)
                {
                    decimal balance = Math.Abs(account.CURRENT_BALANCE ?? 0m);
                    if (balance != 0)
                    {
                        incomeStatement.CostOfGoods.Add(new IncomeStatementLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            Amount = balance
                        });
                        totalCOGS += balance;
                    }
                }

                incomeStatement.TotalCostOfGoods = totalCOGS;
                incomeStatement.GrossProfit = totalRevenue - totalCOGS;

                // OPERATING EXPENSES (Account Type = EXPENSE)
                var expenseAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "EXPENSE").ToList();
                decimal totalOperatingExpense = 0m;

                foreach (var account in expenseAccounts)
                {
                    decimal balance = Math.Abs(account.CURRENT_BALANCE ?? 0m);
                    if (balance != 0)
                    {
                        incomeStatement.OperatingExpenses.Add(new IncomeStatementLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            Amount = balance
                        });
                        totalOperatingExpense += balance;
                    }
                }

                incomeStatement.TotalOperatingExpenses = totalOperatingExpense;
                incomeStatement.OperatingIncome = incomeStatement.GrossProfit - totalOperatingExpense;

                // OTHER INCOME/EXPENSE (Interest, Gains/Losses, etc.)
                var otherAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "OTHER_INCOME" || x.ACCOUNT_TYPE == "OTHER_EXPENSE").ToList();
                decimal totalOtherIncome = 0m;

                foreach (var account in otherAccounts)
                {
                    decimal balance = Math.Abs(account.CURRENT_BALANCE ?? 0m);
                    if (balance != 0)
                    {
                        incomeStatement.OtherIncomeExpense.Add(new IncomeStatementLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            Amount = account.ACCOUNT_TYPE == "OTHER_INCOME" ? balance : -balance
                        });
                        totalOtherIncome += (account.ACCOUNT_TYPE == "OTHER_INCOME" ? balance : -balance);
                    }
                }

                incomeStatement.TotalOtherIncomeExpense = totalOtherIncome;

                // BOTTOM LINE
                incomeStatement.NetIncome = incomeStatement.OperatingIncome + incomeStatement.TotalOtherIncomeExpense;

                _logger?.LogInformation("Income Statement generated. Net Income: {NetIncome:C}", incomeStatement.NetIncome);
                return incomeStatement;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating income statement: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate Balance Sheet (Statement of Financial Position)
        /// Assets = Liabilities + Equity
        /// </summary>
        public async Task<BalanceSheet> GenerateBalanceSheetAsync(DateTime asOfDate, string reportName = "")
        {
            _logger?.LogInformation("Generating Balance Sheet as of {AsOfDate}", asOfDate);

            try
            {
                if (string.IsNullOrWhiteSpace(reportName))
                    reportName = $"Balance Sheet as of {asOfDate:MMMM dd, yyyy}";

                // Get trial balance as of date
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(asOfDate);

                // Initialize Balance Sheet
                var balanceSheet = new BalanceSheet
                {
                    AsOfDate = asOfDate,
                    ReportName = reportName,
                    GeneratedDate = DateTime.UtcNow,
                    CurrentAssets = new List<BalanceSheetLine>(),
                    FixedAssets = new List<BalanceSheetLine>(),
                    OtherAssets = new List<BalanceSheetLine>(),
                    CurrentLiabilities = new List<BalanceSheetLine>(),
                    LongTermLiabilities = new List<BalanceSheetLine>(),
                    EquityAccounts = new List<BalanceSheetLine>()
                };

                // ASSETS
                var assetAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "ASSET").ToList();
                decimal totalCurrentAssets = 0m;
                decimal totalFixedAssets = 0m;
                decimal totalOtherAssets = 0m;

                foreach (var account in assetAccounts)
                {
                    decimal balance = account.CURRENT_BALANCE ?? 0m;
                    if (balance != 0)
                    {
                        var line = new BalanceSheetLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            Amount = balance
                        };

                        // Classify asset
                        if (account.ACCOUNT_NUMBER.StartsWith("1"))  // 1xxx = Current Assets
                        {
                            balanceSheet.CurrentAssets.Add(line);
                            totalCurrentAssets += balance;
                        }
                        else if (account.ACCOUNT_NUMBER.StartsWith("12"))  // 12xx = Fixed Assets
                        {
                            balanceSheet.FixedAssets.Add(line);
                            totalFixedAssets += balance;
                        }
                        else
                        {
                            balanceSheet.OtherAssets.Add(line);
                            totalOtherAssets += balance;
                        }
                    }
                }

                balanceSheet.TotalCurrentAssets = totalCurrentAssets;
                balanceSheet.TotalFixedAssets = totalFixedAssets;
                balanceSheet.TotalOtherAssets = totalOtherAssets;
                balanceSheet.TotalAssets = totalCurrentAssets + totalFixedAssets + totalOtherAssets;

                // LIABILITIES
                var liabilityAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "LIABILITY").ToList();
                decimal totalCurrentLiabilities = 0m;
                decimal totalLongTermLiabilities = 0m;

                foreach (var account in liabilityAccounts)
                {
                    decimal balance = account.CURRENT_BALANCE ?? 0m;
                    if (balance != 0)
                    {
                        var line = new BalanceSheetLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            Amount = balance
                        };

                        // Classify liability
                        if (account.ACCOUNT_NUMBER.StartsWith("20") || account.ACCOUNT_NUMBER.StartsWith("21"))  // 20xx-21xx = Current
                        {
                            balanceSheet.CurrentLiabilities.Add(line);
                            totalCurrentLiabilities += balance;
                        }
                        else
                        {
                            balanceSheet.LongTermLiabilities.Add(line);
                            totalLongTermLiabilities += balance;
                        }
                    }
                }

                balanceSheet.TotalCurrentLiabilities = totalCurrentLiabilities;
                balanceSheet.TotalLongTermLiabilities = totalLongTermLiabilities;
                balanceSheet.TotalLiabilities = totalCurrentLiabilities + totalLongTermLiabilities;

                // EQUITY
                var equityAccounts = trialBalance.Where(x => x.ACCOUNT_TYPE == "EQUITY").ToList();
                decimal totalEquity = 0m;

                foreach (var account in equityAccounts)
                {
                    decimal balance = account.CURRENT_BALANCE ?? 0m;
                    if (balance != 0)
                    {
                        balanceSheet.EquityAccounts.Add(new BalanceSheetLine
                        {
                            AccountNumber = account.ACCOUNT_NUMBER,
                            AccountName = account.ACCOUNT_NAME,
                            Amount = balance
                        });
                        totalEquity += balance;
                    }
                }

                balanceSheet.TotalEquity = totalEquity;

                // BALANCE CHECK
                balanceSheet.TotalLiabilitiesAndEquity = balanceSheet.TotalLiabilities + balanceSheet.TotalEquity;
                balanceSheet.BalanceDifference = Math.Abs(balanceSheet.TotalAssets - balanceSheet.TotalLiabilitiesAndEquity);
                balanceSheet.IsBalanced = balanceSheet.BalanceDifference < 0.01m;

                if (!balanceSheet.IsBalanced)
                {
                    _logger?.LogWarning("Balance Sheet OUT OF BALANCE: Assets {Assets:C}, Liabilities+Equity {LE:C}, Difference {Diff:C}",
                        balanceSheet.TotalAssets, balanceSheet.TotalLiabilitiesAndEquity, balanceSheet.BalanceDifference);
                }
                else
                {
                    _logger?.LogInformation("Balance Sheet balanced. Total Assets: {Assets:C}", balanceSheet.TotalAssets);
                }

                return balanceSheet;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating balance sheet: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate Cash Flow Statement (Simplified)
        /// Operating + Investing + Financing = Net Change in Cash
        /// </summary>
        public async Task<CashFlowStatement> GenerateCashFlowStatementAsync(DateTime periodStart, DateTime periodEnd, string periodName = "")
        {
            _logger?.LogInformation("Generating Cash Flow Statement for period {PeriodStart} to {PeriodEnd}", periodStart, periodEnd);

            try
            {
                if (string.IsNullOrWhiteSpace(periodName))
                    periodName = $"{periodStart:MMM yyyy} to {periodEnd:MMM yyyy}";

                // Get trial balance for period
                var beginningBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodStart.AddDays(-1));
                var endingBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEnd);

                // Initialize Cash Flow Statement
                var cashFlow = new CashFlowStatement
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    PeriodName = periodName,
                    GeneratedDate = DateTime.UtcNow,
                    OperatingActivities = new List<CashFlowLine>(),
                    InvestingActivities = new List<CashFlowLine>(),
                    FinancingActivities = new List<CashFlowLine>()
                };

                // OPERATING ACTIVITIES
                // Net Income + Non-cash items (Depreciation) - Changes in Working Capital
                var incomeStatement = await GenerateIncomeStatementAsync(periodStart, periodEnd, periodName);
                decimal netIncome = incomeStatement.NetIncome;

                cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Net Income", Amount = netIncome });

                // Add depreciation (from Fixed Asset accounts)
                var depreciationAccount = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1210"); // Accumulated Depreciation
                if (depreciationAccount != null && depreciationAccount.CURRENT_BALANCE.HasValue)
                {
                    decimal depreciationChange = Math.Abs(depreciationAccount.CURRENT_BALANCE.Value);
                    if (depreciationChange != 0)
                    {
                        cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Add: Depreciation", Amount = depreciationChange });
                    }
                }

                // Changes in Working Capital (AR, AP, Inventory)
                var beginningAR = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1110")?.CURRENT_BALANCE ?? 0m;
                var endingAR = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1110")?.CURRENT_BALANCE ?? 0m;
                decimal arChange = beginningAR - endingAR;  // Decrease = cash inflow
                if (arChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Accounts Receivable", Amount = arChange });

                var beginningAP = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "2000")?.CURRENT_BALANCE ?? 0m;
                var endingAP = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "2000")?.CURRENT_BALANCE ?? 0m;
                decimal apChange = endingAP - beginningAP;  // Increase = cash inflow
                if (apChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Accounts Payable", Amount = apChange });

                var beginningInventory = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1300")?.CURRENT_BALANCE ?? 0m;
                var endingInventory = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1300")?.CURRENT_BALANCE ?? 0m;
                decimal inventoryChange = beginningInventory - endingInventory;  // Decrease = cash inflow
                if (inventoryChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Inventory", Amount = inventoryChange });

                decimal netOperatingCash = cashFlow.OperatingActivities.Sum(x => x.Amount);
                cashFlow.NetCashFromOperating = netOperatingCash;

                // INVESTING ACTIVITIES
                // Fixed asset purchases, equipment sales
                var fixedAssetAccounts = endingBalance.Where(x => x.ACCOUNT_TYPE == "ASSET" && x.ACCOUNT_NUMBER.StartsWith("12")).ToList();
                foreach (var account in fixedAssetAccounts)
                {
                    var beginningBalance_account = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == account.ACCOUNT_NUMBER)?.CURRENT_BALANCE ?? 0m;
                    decimal change = (account.CURRENT_BALANCE ?? 0m) - beginningBalance_account;
                    if (Math.Abs(change) > 0.01m)
                    {
                        cashFlow.InvestingActivities.Add(new CashFlowLine
                        {
                            Description = $"{account.ACCOUNT_NAME}",
                            Amount = -change  // Negative for purchase
                        });
                    }
                }

                decimal netInvestingCash = cashFlow.InvestingActivities.Sum(x => x.Amount);
                cashFlow.NetCashFromInvesting = netInvestingCash;

                // FINANCING ACTIVITIES
                // Debt changes, equity changes, dividends
                var debtAccount = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "2100");  // Long-term debt
                if (debtAccount != null)
                {
                    var beginningDebt = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "2100")?.CURRENT_BALANCE ?? 0m;
                    decimal debtChange = (debtAccount.CURRENT_BALANCE ?? 0m) - beginningDebt;
                    if (Math.Abs(debtChange) > 0.01m)
                    {
                        cashFlow.FinancingActivities.Add(new CashFlowLine
                        {
                            Description = "Debt Issuance/(Repayment)",
                            Amount = debtChange
                        });
                    }
                }

                decimal netFinancingCash = cashFlow.FinancingActivities.Sum(x => x.Amount);
                cashFlow.NetCashFromFinancing = netFinancingCash;

                // NET CHANGE IN CASH
                cashFlow.NetChangeInCash = netOperatingCash + netInvestingCash + netFinancingCash;

                var beginningCash = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1000")?.CURRENT_BALANCE ?? 0m;
                var endingCash = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1000")?.CURRENT_BALANCE ?? 0m;

                cashFlow.BeginningCash = beginningCash;
                cashFlow.EndingCash = endingCash;
                cashFlow.CashReconciliation = Math.Abs((beginningCash + cashFlow.NetChangeInCash) - endingCash);

                _logger?.LogInformation("Cash Flow Statement generated. Net Change: {Change:C}, Ending Cash: {Cash:C}",
                    cashFlow.NetChangeInCash, cashFlow.EndingCash);

                return cashFlow;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating cash flow statement: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export Financial Statements as formatted text
        /// </summary>
        public string ExportFinancialStatementsAsText(IncomeStatement incomeStatement, BalanceSheet balanceSheet)
        {
            _logger?.LogInformation("Exporting financial statements as text");

            try
            {
                var sb = new StringBuilder();

                // HEADER
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine("                       CONSOLIDATED FINANCIAL STATEMENTS");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                // INCOME STATEMENT
                sb.AppendLine("INCOME STATEMENT");
                sb.AppendLine($"For the Period: {incomeStatement.PeriodName}");
                sb.AppendLine("───────────────────────────────────────────────────────────────────");
                sb.AppendLine();

                sb.AppendLine("REVENUES:");
                foreach (var line in incomeStatement.Revenues)
                    sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                sb.AppendLine($"{"Total Revenues",-40} ${incomeStatement.TotalRevenues,15:N2}");
                sb.AppendLine();

                sb.AppendLine("COST OF GOODS SOLD:");
                foreach (var line in incomeStatement.CostOfGoods)
                    sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                sb.AppendLine($"{"Total COGS",-40} ${incomeStatement.TotalCostOfGoods,15:N2}");
                sb.AppendLine($"{"Gross Profit",-40} ${incomeStatement.GrossProfit,15:N2}");
                sb.AppendLine();

                sb.AppendLine("OPERATING EXPENSES:");
                foreach (var line in incomeStatement.OperatingExpenses)
                    sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                sb.AppendLine($"{"Total Operating Expenses",-40} ${incomeStatement.TotalOperatingExpenses,15:N2}");
                sb.AppendLine($"{"Operating Income",-40} ${incomeStatement.OperatingIncome,15:N2}");
                sb.AppendLine();

                if (incomeStatement.OtherIncomeExpense.Count > 0)
                {
                    sb.AppendLine("OTHER INCOME/(EXPENSE):");
                    foreach (var line in incomeStatement.OtherIncomeExpense)
                        sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                    sb.AppendLine($"{"Total Other Income/(Expense)",-40} ${incomeStatement.TotalOtherIncomeExpense,15:N2}");
                    sb.AppendLine();
                }

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"{"NET INCOME",-40} ${incomeStatement.NetIncome,15:N2}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();
                sb.AppendLine();

                // BALANCE SHEET
                sb.AppendLine("BALANCE SHEET");
                sb.AppendLine($"As of: {balanceSheet.AsOfDate:MMMM dd, yyyy}");
                sb.AppendLine("───────────────────────────────────────────────────────────────────");
                sb.AppendLine();

                sb.AppendLine("ASSETS");
                sb.AppendLine("Current Assets:");
                foreach (var line in balanceSheet.CurrentAssets)
                    sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                sb.AppendLine($"{"Total Current Assets",-40} ${balanceSheet.TotalCurrentAssets,15:N2}");
                sb.AppendLine();

                sb.AppendLine("Fixed Assets:");
                foreach (var line in balanceSheet.FixedAssets)
                    sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                sb.AppendLine($"{"Total Fixed Assets",-40} ${balanceSheet.TotalFixedAssets,15:N2}");
                sb.AppendLine();

                if (balanceSheet.OtherAssets.Count > 0)
                {
                    sb.AppendLine("Other Assets:");
                    foreach (var line in balanceSheet.OtherAssets)
                        sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                    sb.AppendLine($"{"Total Other Assets",-40} ${balanceSheet.TotalOtherAssets,15:N2}");
                    sb.AppendLine();
                }

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"{"TOTAL ASSETS",-40} ${balanceSheet.TotalAssets,15:N2}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine("LIABILITIES & EQUITY");
                sb.AppendLine("Current Liabilities:");
                foreach (var line in balanceSheet.CurrentLiabilities)
                    sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                sb.AppendLine($"{"Total Current Liabilities",-40} ${balanceSheet.TotalCurrentLiabilities,15:N2}");
                sb.AppendLine();

                if (balanceSheet.LongTermLiabilities.Count > 0)
                {
                    sb.AppendLine("Long-Term Liabilities:");
                    foreach (var line in balanceSheet.LongTermLiabilities)
                        sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                    sb.AppendLine($"{"Total Long-Term Liabilities",-40} ${balanceSheet.TotalLongTermLiabilities,15:N2}");
                    sb.AppendLine();
                }

                sb.AppendLine($"{"Total Liabilities",-40} ${balanceSheet.TotalLiabilities,15:N2}");
                sb.AppendLine();

                sb.AppendLine("Equity:");
                foreach (var line in balanceSheet.EquityAccounts)
                    sb.AppendLine($"  {line.AccountName,-40} ${line.Amount,15:N2}");
                sb.AppendLine($"{"Total Equity",-40} ${balanceSheet.TotalEquity,15:N2}");
                sb.AppendLine();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"{"TOTAL LIABILITIES & EQUITY",-40} ${balanceSheet.TotalLiabilitiesAndEquity,15:N2}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Balance Status: {(balanceSheet.IsBalanced ? "✓ BALANCED" : "✗ OUT OF BALANCE")}");
                if (!balanceSheet.IsBalanced)
                    sb.AppendLine($"Difference: ${balanceSheet.BalanceDifference:N2}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting financial statements: {Message}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Income Statement (P&L) Data Model
    /// </summary>
    public class IncomeStatement
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PeriodName { get; set; }
        public DateTime GeneratedDate { get; set; }

        public List<IncomeStatementLine> Revenues { get; set; }
        public decimal TotalRevenues { get; set; }

        public List<IncomeStatementLine> CostOfGoods { get; set; }
        public decimal TotalCostOfGoods { get; set; }
        public decimal GrossProfit { get; set; }

        public List<IncomeStatementLine> OperatingExpenses { get; set; }
        public decimal TotalOperatingExpenses { get; set; }
        public decimal OperatingIncome { get; set; }

        public List<IncomeStatementLine> OtherIncomeExpense { get; set; }
        public decimal TotalOtherIncomeExpense { get; set; }

        public decimal NetIncome { get; set; }
    }

    /// <summary>
    /// Income Statement Line Item
    /// </summary>
    public class IncomeStatementLine
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Balance Sheet Data Model
    /// </summary>
    public class BalanceSheet
    {
        public DateTime AsOfDate { get; set; }
        public string ReportName { get; set; }
        public DateTime GeneratedDate { get; set; }

        // ASSETS
        public List<BalanceSheetLine> CurrentAssets { get; set; }
        public decimal TotalCurrentAssets { get; set; }

        public List<BalanceSheetLine> FixedAssets { get; set; }
        public decimal TotalFixedAssets { get; set; }

        public List<BalanceSheetLine> OtherAssets { get; set; }
        public decimal TotalOtherAssets { get; set; }

        public decimal TotalAssets { get; set; }

        // LIABILITIES
        public List<BalanceSheetLine> CurrentLiabilities { get; set; }
        public decimal TotalCurrentLiabilities { get; set; }

        public List<BalanceSheetLine> LongTermLiabilities { get; set; }
        public decimal TotalLongTermLiabilities { get; set; }

        public decimal TotalLiabilities { get; set; }

        // EQUITY
        public List<BalanceSheetLine> EquityAccounts { get; set; }
        public decimal TotalEquity { get; set; }

        public decimal TotalLiabilitiesAndEquity { get; set; }
        public decimal BalanceDifference { get; set; }
        public bool IsBalanced { get; set; }
    }

    /// <summary>
    /// Balance Sheet Line Item
    /// </summary>
    public class BalanceSheetLine
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Cash Flow Statement Data Model
    /// </summary>
    public class CashFlowStatement
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string PeriodName { get; set; }
        public DateTime GeneratedDate { get; set; }

        // OPERATING ACTIVITIES
        public List<CashFlowLine> OperatingActivities { get; set; }
        public decimal NetCashFromOperating { get; set; }

        // INVESTING ACTIVITIES
        public List<CashFlowLine> InvestingActivities { get; set; }
        public decimal NetCashFromInvesting { get; set; }

        // FINANCING ACTIVITIES
        public List<CashFlowLine> FinancingActivities { get; set; }
        public decimal NetCashFromFinancing { get; set; }

        // SUMMARY
        public decimal NetChangeInCash { get; set; }
        public decimal BeginningCash { get; set; }
        public decimal EndingCash { get; set; }
        public decimal CashReconciliation { get; set; }
    }

    /// <summary>
    /// Cash Flow Statement Line Item
    /// </summary>
    public class CashFlowLine
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
