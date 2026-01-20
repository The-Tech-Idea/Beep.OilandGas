using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
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
        public async Task<IncomeStatement> GenerateIncomeStatementAsync(DateTime periodStart, DateTime periodEnd, string periodName = "", string? bookId = null)
        {
            _logger?.LogInformation("Generating Income Statement for period {PeriodStart} to {PeriodEnd}", periodStart, periodEnd);

            try
            {
                if (string.IsNullOrWhiteSpace(periodName))
                    periodName = $"{periodStart:MMM yyyy} to {periodEnd:MMM yyyy}";

                // Get trial balance as of period end
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEnd, bookId);

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
        public async Task<BalanceSheet> GenerateBalanceSheetAsync(DateTime asOfDate, string reportName = "", string? bookId = null)
        {
            _logger?.LogInformation("Generating Balance Sheet as of {AsOfDate}", asOfDate);

            try
            {
                if (string.IsNullOrWhiteSpace(reportName))
                    reportName = $"Balance Sheet as of {asOfDate:MMMM dd, yyyy}";

                // Get trial balance as of date
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(asOfDate, bookId);

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
                var currentAssetOverrides = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    DefaultGlAccounts.Cash,
                    DefaultGlAccounts.AccountsReceivable,
                    DefaultGlAccounts.Inventory,
                    DefaultGlAccounts.ContractAsset,
                    DefaultGlAccounts.GrantReceivable,
                    DefaultGlAccounts.FinancialInstrumentAsset,
                    DefaultGlAccounts.ReinsuranceAsset,
                    DefaultGlAccounts.LossAllowance,
                    DefaultGlAccounts.HeldForSaleAsset,
                    DefaultGlAccounts.IntercompanyReceivable,
                    DefaultGlAccounts.GaapContractAsset
                };
                var fixedAssetOverrides = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    DefaultGlAccounts.FixedAssets,
                    DefaultGlAccounts.AccumulatedDepreciation,
                    DefaultGlAccounts.RightOfUseAsset,
                    DefaultGlAccounts.AssetRetirementCost,
                    DefaultGlAccounts.IntangibleAssets,
                    DefaultGlAccounts.AccumulatedAmortization,
                    DefaultGlAccounts.ImpairmentAllowance,
                    DefaultGlAccounts.InvestmentProperty,
                    DefaultGlAccounts.BiologicalAssets,
                    DefaultGlAccounts.GaapRightOfUseAsset
                };
                var otherAssetOverrides = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    DefaultGlAccounts.DeferredTaxAsset,
                    DefaultGlAccounts.RetirementPlanAsset,
                    DefaultGlAccounts.AssociateInvestment,
                    DefaultGlAccounts.JointVentureInvestment,
                    DefaultGlAccounts.Goodwill,
                    DefaultGlAccounts.ExplorationAsset,
                    DefaultGlAccounts.RegulatoryDeferralAsset
                };

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
                        if (otherAssetOverrides.Contains(account.ACCOUNT_NUMBER))
                        {
                            balanceSheet.OtherAssets.Add(line);
                            totalOtherAssets += balance;
                        }
                        else if (currentAssetOverrides.Contains(account.ACCOUNT_NUMBER) || account.ACCOUNT_NUMBER.StartsWith("1"))  // 1xxx = Current Assets
                        {
                            balanceSheet.CurrentAssets.Add(line);
                            totalCurrentAssets += balance;
                        }
                        else if (fixedAssetOverrides.Contains(account.ACCOUNT_NUMBER) || account.ACCOUNT_NUMBER.StartsWith("12"))  // 12xx = Fixed Assets
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
                var currentLiabilityOverrides = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    DefaultGlAccounts.AccountsPayable,
                    DefaultGlAccounts.AccruedRoyalties,
                    DefaultGlAccounts.ContractLiability,
                    DefaultGlAccounts.IncomeTaxPayable,
                    DefaultGlAccounts.EmployeeBenefitLiability,
                    DefaultGlAccounts.DeferredGrantLiability,
                    DefaultGlAccounts.InterestPayable,
                    DefaultGlAccounts.FinancialInstrumentLiability,
                    DefaultGlAccounts.InsuranceContractLiability,
                    DefaultGlAccounts.ContractualServiceMargin,
                    DefaultGlAccounts.HeldForSaleLiability,
                    DefaultGlAccounts.IntercompanyPayable,
                    DefaultGlAccounts.RegulatoryDeferralLiability,
                    DefaultGlAccounts.GaapContractLiability,
                    DefaultGlAccounts.GaapLeaseLiability
                };
                var longTermLiabilityOverrides = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    DefaultGlAccounts.AssetRetirementObligation,
                    DefaultGlAccounts.LeaseLiability,
                    DefaultGlAccounts.DeferredTaxLiability
                };

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
                        if (currentLiabilityOverrides.Contains(account.ACCOUNT_NUMBER) || account.ACCOUNT_NUMBER.StartsWith("20"))  // 20xx = Current
                        {
                            balanceSheet.CurrentLiabilities.Add(line);
                            totalCurrentLiabilities += balance;
                        }
                        else if (longTermLiabilityOverrides.Contains(account.ACCOUNT_NUMBER))
                        {
                            balanceSheet.LongTermLiabilities.Add(line);
                            totalLongTermLiabilities += balance;
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
        public async Task<CashFlowStatement> GenerateCashFlowStatementAsync(DateTime periodStart, DateTime periodEnd, string periodName = "", string? bookId = null)
        {
            _logger?.LogInformation("Generating Cash Flow Statement for period {PeriodStart} to {PeriodEnd}", periodStart, periodEnd);

            try
            {
                if (string.IsNullOrWhiteSpace(periodName))
                    periodName = $"{periodStart:MMM yyyy} to {periodEnd:MMM yyyy}";

                // Get trial balance for period
                var beginningBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodStart.AddDays(-1), bookId);
                var endingBalance = await _trialBalanceService.GenerateTrialBalanceAsync(periodEnd, bookId);

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
                var incomeStatement = await GenerateIncomeStatementAsync(periodStart, periodEnd, periodName, bookId);
                decimal netIncome = incomeStatement.NetIncome;

                cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Net Income", Amount = netIncome });

                // Add depreciation (from Fixed Asset accounts)
                var depreciationAccount = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccumulatedDepreciation);
                if (depreciationAccount != null)
                {
                    var beginningDepreciation = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccumulatedDepreciation)?.CURRENT_BALANCE ?? 0m;
                    var endingDepreciation = depreciationAccount.CURRENT_BALANCE ?? 0m;
                    decimal depreciationChange = Math.Abs(endingDepreciation - beginningDepreciation);
                    if (depreciationChange != 0)
                    {
                        cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Add: Depreciation", Amount = depreciationChange });
                    }
                }

                var accretionExpense = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccretionExpense);
                if (accretionExpense != null)
                {
                    var beginningAccretion = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccretionExpense)?.CURRENT_BALANCE ?? 0m;
                    var endingAccretion = accretionExpense.CURRENT_BALANCE ?? 0m;
                    var accretionChange = Math.Abs(endingAccretion - beginningAccretion);
                    if (accretionChange != 0)
                    {
                        cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Add: ARO Accretion", Amount = accretionChange });
                    }
                }

                var leaseAmortization = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.LeaseAmortizationExpense);
                if (leaseAmortization != null)
                {
                    var beginningLeaseAmort = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.LeaseAmortizationExpense)?.CURRENT_BALANCE ?? 0m;
                    var endingLeaseAmort = leaseAmortization.CURRENT_BALANCE ?? 0m;
                    var leaseAmortChange = Math.Abs(endingLeaseAmort - beginningLeaseAmort);
                    if (leaseAmortChange != 0)
                    {
                        cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Add: Lease Amortization", Amount = leaseAmortChange });
                    }
                }

                var intangibleAmort = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AmortizationExpense);
                if (intangibleAmort != null)
                {
                    var beginningAmort = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AmortizationExpense)?.CURRENT_BALANCE ?? 0m;
                    var endingAmort = intangibleAmort.CURRENT_BALANCE ?? 0m;
                    var amortChange = Math.Abs(endingAmort - beginningAmort);
                    if (amortChange != 0)
                    {
                        cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Add: Intangible Amortization", Amount = amortChange });
                    }
                }

                var impairmentLoss = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ImpairmentLoss);
                if (impairmentLoss != null)
                {
                    var beginningImpairment = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ImpairmentLoss)?.CURRENT_BALANCE ?? 0m;
                    var endingImpairment = impairmentLoss.CURRENT_BALANCE ?? 0m;
                    var impairmentChange = Math.Abs(endingImpairment - beginningImpairment);
                    if (impairmentChange != 0)
                    {
                        cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Add: Impairment Loss", Amount = impairmentChange });
                    }
                }

                var lossAllowance = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.LossAllowance);
                if (lossAllowance != null)
                {
                    var beginningAllowance = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.LossAllowance)?.CURRENT_BALANCE ?? 0m;
                    var endingAllowance = lossAllowance.CURRENT_BALANCE ?? 0m;
                    var allowanceChange = Math.Abs(endingAllowance - beginningAllowance);
                    if (allowanceChange != 0)
                    {
                        cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Add: Expected Credit Loss", Amount = allowanceChange });
                    }
                }

                // Changes in Working Capital (AR, AP, Inventory)
                var beginningAR = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccountsReceivable)?.CURRENT_BALANCE ?? 0m;
                var endingAR = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccountsReceivable)?.CURRENT_BALANCE ?? 0m;
                decimal arChange = beginningAR - endingAR;  // Decrease = cash inflow
                if (arChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Accounts Receivable", Amount = arChange });

                var beginningAP = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccountsPayable)?.CURRENT_BALANCE ?? 0m;
                var endingAP = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AccountsPayable)?.CURRENT_BALANCE ?? 0m;
                decimal apChange = endingAP - beginningAP;  // Increase = cash inflow
                if (apChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Accounts Payable", Amount = apChange });

                var beginningInventory = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.Inventory)?.CURRENT_BALANCE ?? 0m;
                var endingInventory = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.Inventory)?.CURRENT_BALANCE ?? 0m;
                decimal inventoryChange = beginningInventory - endingInventory;  // Decrease = cash inflow
                if (inventoryChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Inventory", Amount = inventoryChange });

                var beginningContractAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ContractAsset)?.CURRENT_BALANCE ?? 0m;
                var endingContractAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ContractAsset)?.CURRENT_BALANCE ?? 0m;
                var contractAssetChange = beginningContractAsset - endingContractAsset;
                if (contractAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Contract Assets", Amount = contractAssetChange });

                var beginningContractLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ContractLiability)?.CURRENT_BALANCE ?? 0m;
                var endingContractLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ContractLiability)?.CURRENT_BALANCE ?? 0m;
                var contractLiabilityChange = endingContractLiability - beginningContractLiability;
                if (contractLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Contract Liabilities", Amount = contractLiabilityChange });

                var beginningAro = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AssetRetirementObligation)?.CURRENT_BALANCE ?? 0m;
                var endingAro = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.AssetRetirementObligation)?.CURRENT_BALANCE ?? 0m;
                var aroChange = endingAro - beginningAro;
                if (aroChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in ARO Liability", Amount = aroChange });

                var beginningTaxPayable = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.IncomeTaxPayable)?.CURRENT_BALANCE ?? 0m;
                var endingTaxPayable = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.IncomeTaxPayable)?.CURRENT_BALANCE ?? 0m;
                var taxPayableChange = endingTaxPayable - beginningTaxPayable;
                if (taxPayableChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Income Taxes Payable", Amount = taxPayableChange });

                var beginningDeferredTaxAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.DeferredTaxAsset)?.CURRENT_BALANCE ?? 0m;
                var endingDeferredTaxAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.DeferredTaxAsset)?.CURRENT_BALANCE ?? 0m;
                var deferredTaxAssetChange = beginningDeferredTaxAsset - endingDeferredTaxAsset;
                if (deferredTaxAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Deferred Tax Assets", Amount = deferredTaxAssetChange });

                var beginningDeferredTaxLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.DeferredTaxLiability)?.CURRENT_BALANCE ?? 0m;
                var endingDeferredTaxLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.DeferredTaxLiability)?.CURRENT_BALANCE ?? 0m;
                var deferredTaxLiabilityChange = endingDeferredTaxLiability - beginningDeferredTaxLiability;
                if (deferredTaxLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Deferred Tax Liabilities", Amount = deferredTaxLiabilityChange });

                var beginningGrantReceivable = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GrantReceivable)?.CURRENT_BALANCE ?? 0m;
                var endingGrantReceivable = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GrantReceivable)?.CURRENT_BALANCE ?? 0m;
                var grantReceivableChange = beginningGrantReceivable - endingGrantReceivable;
                if (grantReceivableChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Grant Receivables", Amount = grantReceivableChange });

                var beginningGrantLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.DeferredGrantLiability)?.CURRENT_BALANCE ?? 0m;
                var endingGrantLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.DeferredGrantLiability)?.CURRENT_BALANCE ?? 0m;
                var grantLiabilityChange = endingGrantLiability - beginningGrantLiability;
                if (grantLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Deferred Grant Liabilities", Amount = grantLiabilityChange });

                var beginningBenefitLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.EmployeeBenefitLiability)?.CURRENT_BALANCE ?? 0m;
                var endingBenefitLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.EmployeeBenefitLiability)?.CURRENT_BALANCE ?? 0m;
                var benefitLiabilityChange = endingBenefitLiability - beginningBenefitLiability;
                if (benefitLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Employee Benefit Liabilities", Amount = benefitLiabilityChange });

                var beginningInterestPayable = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.InterestPayable)?.CURRENT_BALANCE ?? 0m;
                var endingInterestPayable = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.InterestPayable)?.CURRENT_BALANCE ?? 0m;
                var interestPayableChange = endingInterestPayable - beginningInterestPayable;
                if (interestPayableChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Interest Payable", Amount = interestPayableChange });

                var beginningFinancialInstrumentAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.FinancialInstrumentAsset)?.CURRENT_BALANCE ?? 0m;
                var endingFinancialInstrumentAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.FinancialInstrumentAsset)?.CURRENT_BALANCE ?? 0m;
                var financialInstrumentAssetChange = beginningFinancialInstrumentAsset - endingFinancialInstrumentAsset;
                if (financialInstrumentAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Financial Instrument Assets", Amount = financialInstrumentAssetChange });

                var beginningFinancialInstrumentLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.FinancialInstrumentLiability)?.CURRENT_BALANCE ?? 0m;
                var endingFinancialInstrumentLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.FinancialInstrumentLiability)?.CURRENT_BALANCE ?? 0m;
                var financialInstrumentLiabilityChange = endingFinancialInstrumentLiability - beginningFinancialInstrumentLiability;
                if (financialInstrumentLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Financial Instrument Liabilities", Amount = financialInstrumentLiabilityChange });

                var beginningRetirementPlanAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.RetirementPlanAsset)?.CURRENT_BALANCE ?? 0m;
                var endingRetirementPlanAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.RetirementPlanAsset)?.CURRENT_BALANCE ?? 0m;
                var retirementPlanAssetChange = beginningRetirementPlanAsset - endingRetirementPlanAsset;
                if (retirementPlanAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Retirement Plan Assets", Amount = retirementPlanAssetChange });

                var beginningInsuranceLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.InsuranceContractLiability)?.CURRENT_BALANCE ?? 0m;
                var endingInsuranceLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.InsuranceContractLiability)?.CURRENT_BALANCE ?? 0m;
                var insuranceLiabilityChange = endingInsuranceLiability - beginningInsuranceLiability;
                if (insuranceLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Insurance Contract Liabilities", Amount = insuranceLiabilityChange });

                var beginningReinsuranceAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ReinsuranceAsset)?.CURRENT_BALANCE ?? 0m;
                var endingReinsuranceAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.ReinsuranceAsset)?.CURRENT_BALANCE ?? 0m;
                var reinsuranceAssetChange = beginningReinsuranceAsset - endingReinsuranceAsset;
                if (reinsuranceAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Reinsurance Assets", Amount = reinsuranceAssetChange });

                var beginningHeldForSaleAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.HeldForSaleAsset)?.CURRENT_BALANCE ?? 0m;
                var endingHeldForSaleAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.HeldForSaleAsset)?.CURRENT_BALANCE ?? 0m;
                var heldForSaleAssetChange = beginningHeldForSaleAsset - endingHeldForSaleAsset;
                if (heldForSaleAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Held-for-Sale Assets", Amount = heldForSaleAssetChange });

                var beginningIntercompanyReceivable = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.IntercompanyReceivable)?.CURRENT_BALANCE ?? 0m;
                var endingIntercompanyReceivable = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.IntercompanyReceivable)?.CURRENT_BALANCE ?? 0m;
                var intercompanyReceivableChange = beginningIntercompanyReceivable - endingIntercompanyReceivable;
                if (intercompanyReceivableChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Intercompany Receivables", Amount = intercompanyReceivableChange });

                var beginningIntercompanyPayable = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.IntercompanyPayable)?.CURRENT_BALANCE ?? 0m;
                var endingIntercompanyPayable = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.IntercompanyPayable)?.CURRENT_BALANCE ?? 0m;
                var intercompanyPayableChange = endingIntercompanyPayable - beginningIntercompanyPayable;
                if (intercompanyPayableChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Intercompany Payables", Amount = intercompanyPayableChange });

                var beginningRegulatoryAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.RegulatoryDeferralAsset)?.CURRENT_BALANCE ?? 0m;
                var endingRegulatoryAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.RegulatoryDeferralAsset)?.CURRENT_BALANCE ?? 0m;
                var regulatoryAssetChange = beginningRegulatoryAsset - endingRegulatoryAsset;
                if (regulatoryAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Regulatory Deferral Assets", Amount = regulatoryAssetChange });

                var beginningRegulatoryLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.RegulatoryDeferralLiability)?.CURRENT_BALANCE ?? 0m;
                var endingRegulatoryLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.RegulatoryDeferralLiability)?.CURRENT_BALANCE ?? 0m;
                var regulatoryLiabilityChange = endingRegulatoryLiability - beginningRegulatoryLiability;
                if (regulatoryLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in Regulatory Deferral Liabilities", Amount = regulatoryLiabilityChange });

                var beginningGaapContractAsset = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GaapContractAsset)?.CURRENT_BALANCE ?? 0m;
                var endingGaapContractAsset = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GaapContractAsset)?.CURRENT_BALANCE ?? 0m;
                var gaapContractAssetChange = beginningGaapContractAsset - endingGaapContractAsset;
                if (gaapContractAssetChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in ASC 606 Contract Assets", Amount = gaapContractAssetChange });

                var beginningGaapContractLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GaapContractLiability)?.CURRENT_BALANCE ?? 0m;
                var endingGaapContractLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GaapContractLiability)?.CURRENT_BALANCE ?? 0m;
                var gaapContractLiabilityChange = endingGaapContractLiability - beginningGaapContractLiability;
                if (gaapContractLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in ASC 606 Contract Liabilities", Amount = gaapContractLiabilityChange });

                var beginningCeclAllowance = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.CeclAllowance)?.CURRENT_BALANCE ?? 0m;
                var endingCeclAllowance = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.CeclAllowance)?.CURRENT_BALANCE ?? 0m;
                var ceclAllowanceChange = endingCeclAllowance - beginningCeclAllowance;
                if (ceclAllowanceChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in CECL Allowance", Amount = ceclAllowanceChange });

                var beginningGaapLeaseLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GaapLeaseLiability)?.CURRENT_BALANCE ?? 0m;
                var endingGaapLeaseLiability = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.GaapLeaseLiability)?.CURRENT_BALANCE ?? 0m;
                var gaapLeaseLiabilityChange = endingGaapLeaseLiability - beginningGaapLeaseLiability;
                if (gaapLeaseLiabilityChange != 0)
                    cashFlow.OperatingActivities.Add(new CashFlowLine { Description = "Change in ASC 842 Lease Liabilities", Amount = gaapLeaseLiabilityChange });

                decimal netOperatingCash = cashFlow.OperatingActivities.Sum(x => x.Amount);
                cashFlow.NetCashFromOperating = netOperatingCash;

                // INVESTING ACTIVITIES
                // Fixed asset purchases, equipment sales
                var excludedFixedAssets = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    DefaultGlAccounts.AccumulatedDepreciation,
                    DefaultGlAccounts.RightOfUseAsset,
                    DefaultGlAccounts.AssetRetirementCost,
                    DefaultGlAccounts.ImpairmentAllowance,
                    DefaultGlAccounts.AccumulatedAmortization
                };
                var fixedAssetAccounts = endingBalance
                    .Where(x => x.ACCOUNT_TYPE == "ASSET" && x.ACCOUNT_NUMBER.StartsWith("12") && !excludedFixedAssets.Contains(x.ACCOUNT_NUMBER))
                    .ToList();
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
                var leaseLiabilityAccount = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.LeaseLiability);
                if (leaseLiabilityAccount != null)
                {
                    var beginningLeaseLiability = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.LeaseLiability)?.CURRENT_BALANCE ?? 0m;
                    var leaseLiabilityChange = (leaseLiabilityAccount.CURRENT_BALANCE ?? 0m) - beginningLeaseLiability;
                    if (Math.Abs(leaseLiabilityChange) > 0.01m)
                    {
                        cashFlow.FinancingActivities.Add(new CashFlowLine
                        {
                            Description = "Lease Liability Change",
                            Amount = leaseLiabilityChange
                        });
                    }
                }

                decimal netFinancingCash = cashFlow.FinancingActivities.Sum(x => x.Amount);
                cashFlow.NetCashFromFinancing = netFinancingCash;

                // NET CHANGE IN CASH
                cashFlow.NetChangeInCash = netOperatingCash + netInvestingCash + netFinancingCash;

                var beginningCash = beginningBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.Cash)?.CURRENT_BALANCE ?? 0m;
                var endingCash = endingBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == DefaultGlAccounts.Cash)?.CURRENT_BALANCE ?? 0m;

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

}
