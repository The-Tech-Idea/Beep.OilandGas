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
    /// Tax Calculation Service - Tax accruals, provisions, and estimated payments
    /// Supports corporate income tax, quarterly estimated payments, and deductions
    /// Critical for tax compliance and cash flow planning
    /// </summary>
    public class TaxCalculationService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<TaxCalculationService> _logger;
        private const string ConnectionName = "PPDM39";

        public TaxCalculationService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<TaxCalculationService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calculate annual tax provision
        /// </summary>
        public async Task<TaxProvision> CalculateTaxProvisionAsync(
            decimal taxableIncome,
            decimal effectiveTaxRate,
            List<TaxDeduction> deductions = null,
            List<TaxCredit> credits = null)
        {
            _logger?.LogInformation("Calculating tax provision. Taxable Income: {Income:C}, Rate: {Rate:P}",
                taxableIncome, effectiveTaxRate);

            try
            {
                if (taxableIncome < 0)
                    throw new InvalidOperationException("Taxable income cannot be negative");

                if (effectiveTaxRate < 0 || effectiveTaxRate > 1)
                    throw new InvalidOperationException("Tax rate must be between 0 and 1");

                var provision = new TaxProvision
                {
                    CalculatedDate = DateTime.UtcNow,
                    TaxableIncome = taxableIncome,
                    EffectiveTaxRate = effectiveTaxRate,
                    Deductions = deductions ?? new List<TaxDeduction>(),
                    Credits = credits ?? new List<TaxCredit>()
                };

                // Calculate adjusted taxable income
                decimal totalDeductions = provision.Deductions.Sum(x => x.Amount);
                provision.TotalDeductions = totalDeductions;
                provision.AdjustedTaxableIncome = Math.Max(0, taxableIncome - totalDeductions);

                // Calculate base tax
                provision.BaseTax = provision.AdjustedTaxableIncome * effectiveTaxRate;

                // Apply credits
                decimal totalCredits = provision.Credits.Sum(x => x.Amount);
                provision.TotalCredits = totalCredits;
                provision.TaxAfterCredits = Math.Max(0, provision.BaseTax - totalCredits);

                // Alternative Minimum Tax (simplified - 20% of taxable income)
                provision.AlternativeMinimumTax = provision.AdjustedTaxableIncome * 0.20m;
                provision.TaxProvisionAmount = Math.Max(provision.TaxAfterCredits, provision.AlternativeMinimumTax);

                // Effective rate calculation
                provision.EffectiveProvisionRate = provision.AdjustedTaxableIncome > 0 
                    ? provision.TaxProvisionAmount / provision.AdjustedTaxableIncome 
                    : 0;

                _logger?.LogInformation("Tax provision calculated. Amount: {Amount:C}, Rate: {Rate:P}",
                    provision.TaxProvisionAmount, provision.EffectiveProvisionRate);

                return provision;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating tax provision: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate quarterly estimated tax payments
        /// </summary>
        public async Task<QuarterlyEstimatedTax> CalculateQuarterlyEstimatedTaxAsync(
            int year,
            decimal annualTaxableIncome,
            decimal estimatedTaxRate,
            List<QuarterlyAdjustment> adjustments = null)
        {
            _logger?.LogInformation("Calculating quarterly estimated tax for year {Year}",
                year);

            try
            {
                if (annualTaxableIncome < 0)
                    throw new InvalidOperationException("Annual taxable income cannot be negative");

                var quarterly = new QuarterlyEstimatedTax
                {
                    TaxYear = year,
                    CalculatedDate = DateTime.UtcNow,
                    AnnualTaxableIncome = annualTaxableIncome,
                    EstimatedTaxRate = estimatedTaxRate,
                    Quarters = new List<QuarterlyPayment>(),
                    Adjustments = adjustments ?? new List<QuarterlyAdjustment>()
                };

                // Base quarterly payment (1/4 of annual)
                decimal baseQuarterlyPayment = (annualTaxableIncome * estimatedTaxRate) / 4;

                // Calculate each quarter's payment
                for (int quarter = 1; quarter <= 4; quarter++)
                {
                    var payment = new QuarterlyPayment
                    {
                        Quarter = quarter,
                        DueDate = GetQuarterlyDueDate(year, quarter),
                        BasePayment = baseQuarterlyPayment,
                        Adjustments = 0,
                        TotalPayment = baseQuarterlyPayment
                    };

                    // Apply quarter-specific adjustments
                    var quarterAdjustments = quarterly.Adjustments.Where(x => x.Quarter == quarter).ToList();
                    payment.Adjustments = quarterAdjustments.Sum(x => x.Amount);
                    payment.TotalPayment = Math.Max(0, baseQuarterlyPayment + payment.Adjustments);

                    quarterly.Quarters.Add(payment);
                }

                quarterly.TotalAnnualPayment = quarterly.Quarters.Sum(x => x.TotalPayment);
                quarterly.TotalAdjustments = quarterly.Adjustments.Sum(x => x.Amount);

                _logger?.LogInformation("Quarterly estimated tax calculated. Total: {Total:C}",
                    quarterly.TotalAnnualPayment);

                return quarterly;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating quarterly estimated tax: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate tax deferral opportunities
        /// </summary>
        public async Task<TaxDeferralAnalysis> AnalyzeTaxDeferralOpportunitiesAsync(
            decimal currentYearIncome,
            DateTime analysisDate)
        {
            _logger?.LogInformation("Analyzing tax deferral opportunities as of {Date}", analysisDate.Date);

            try
            {
                var analysis = new TaxDeferralAnalysis
                {
                    AnalysisDate = analysisDate,
                    CurrentYearIncome = currentYearIncome,
                    Opportunities = new List<DeferralOpportunity>()
                };

                // Opportunity 1: 401(k) contributions
                decimal _401kLimit = 23500;  // 2024 limit
                decimal _401kDeferral = Math.Min(currentYearIncome * 0.15m, _401kLimit);
                analysis.Opportunities.Add(new DeferralOpportunity
                {
                    OpportunityType = "401(k) Contribution",
                    Description = "Defer income through retirement plan",
                    DeferralAmount = _401kDeferral,
                    TaxSavings = _401kDeferral * 0.37m,  // 37% federal + state
                    CashFlowImpact = _401kDeferral,
                    Deadline = new DateTime(analysisDate.Year, 12, 31)
                });

                // Opportunity 2: Charitable contributions
                decimal charityLimit = currentYearIncome * 0.50m;
                decimal charityDeferral = Math.Min(currentYearIncome * 0.05m, charityLimit);
                analysis.Opportunities.Add(new DeferralOpportunity
                {
                    OpportunityType = "Charitable Contribution",
                    Description = "Deduct charitable donations",
                    DeferralAmount = charityDeferral,
                    TaxSavings = charityDeferral * 0.37m,
                    CashFlowImpact = charityDeferral,
                    Deadline = new DateTime(analysisDate.Year, 12, 31)
                });

                // Opportunity 3: Depreciation/Section 179
                decimal section179Limit = 1160000;  // 2024 limit
                decimal depreciationDeferral = Math.Min(currentYearIncome * 0.10m, section179Limit);
                analysis.Opportunities.Add(new DeferralOpportunity
                {
                    OpportunityType = "Section 179 Deduction",
                    Description = "Accelerate business asset depreciation",
                    DeferralAmount = depreciationDeferral,
                    TaxSavings = depreciationDeferral * 0.37m,
                    CashFlowImpact = depreciationDeferral,
                    Deadline = new DateTime(analysisDate.Year, 12, 31)
                });

                // Opportunity 4: Qualified Business Income (QBI) Deduction
                decimal qbiDeduction = (currentYearIncome * 0.20m) * 0.20m;  // 20% of up to 20% QBI
                analysis.Opportunities.Add(new DeferralOpportunity
                {
                    OpportunityType = "QBI Deduction",
                    Description = "Pass-through entity deduction for qualified business income",
                    DeferralAmount = qbiDeduction,
                    TaxSavings = qbiDeduction * 0.37m,
                    CashFlowImpact = qbiDeduction,
                    Deadline = new DateTime(analysisDate.Year, 12, 31)
                });

                analysis.TotalDeferralAmount = analysis.Opportunities.Sum(x => x.DeferralAmount);
                analysis.TotalTaxSavings = analysis.Opportunities.Sum(x => x.TaxSavings);
                analysis.TotalCashFlowImpact = analysis.Opportunities.Sum(x => x.CashFlowImpact);

                _logger?.LogInformation("Tax deferral analysis complete. Total Savings: {Savings:C}",
                    analysis.TotalTaxSavings);

                return analysis;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing tax deferral opportunities: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export tax calculation as formatted text
        /// </summary>
        public string ExportTaxProvisionAsText(TaxProvision provision)
        {
            _logger?.LogInformation("Exporting tax provision as text");

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine("                       TAX PROVISION CALCULATION");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine("INCOME:");
                sb.AppendLine($"  Taxable Income:                ${provision.TaxableIncome,15:N2}");
                sb.AppendLine();

                sb.AppendLine("DEDUCTIONS:");
                foreach (var deduction in provision.Deductions)
                {
                    sb.AppendLine($"  {deduction.Description,-35} ${deduction.Amount,15:N2}");
                }
                sb.AppendLine($"  {"Total Deductions",-35} ${provision.TotalDeductions,15:N2}");
                sb.AppendLine();

                sb.AppendLine("ADJUSTED TAXABLE INCOME:");
                sb.AppendLine($"  Taxable Income - Deductions:   ${provision.AdjustedTaxableIncome,15:N2}");
                sb.AppendLine();

                sb.AppendLine("TAX CALCULATION:");
                sb.AppendLine($"  Base Tax ({provision.EffectiveTaxRate:P}):      ${provision.BaseTax,15:N2}");
                sb.AppendLine();

                sb.AppendLine("TAX CREDITS:");
                foreach (var credit in provision.Credits)
                {
                    sb.AppendLine($"  {credit.Description,-35} ${credit.Amount,15:N2}");
                }
                sb.AppendLine($"  {"Total Credits",-35} ${provision.TotalCredits,15:N2}");
                sb.AppendLine();

                sb.AppendLine("ALTERNATIVE MINIMUM TAX (AMT):");
                sb.AppendLine($"  AMT (20% of AGI):              ${provision.AlternativeMinimumTax,15:N2}");
                sb.AppendLine();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"  {"TAX PROVISION",-35} ${provision.TaxProvisionAmount,15:N2}");
                sb.AppendLine($"  {"Effective Tax Rate",-35} {provision.EffectiveProvisionRate,15:P}");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Calculated: {provision.CalculatedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting tax provision: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Helper: Get quarterly tax due date
        /// </summary>
        private DateTime GetQuarterlyDueDate(int year, int quarter)
        {
            return quarter switch
            {
                1 => new DateTime(year, 4, 15),   // Q1 due April 15
                2 => new DateTime(year, 6, 17),   // Q2 due June 15 (or 17 if weekend)
                3 => new DateTime(year, 9, 16),   // Q3 due September 15 (or 16 if weekend)
                4 => new DateTime(year + 1, 1, 15), // Q4 due January 15 next year
                _ => DateTime.MinValue
            };
        }
    }

    /// <summary>
    /// Tax Provision
    /// </summary>
    public class TaxProvision
    {
        public DateTime CalculatedDate { get; set; }
        public decimal TaxableIncome { get; set; }
        public decimal EffectiveTaxRate { get; set; }
        public List<TaxDeduction> Deductions { get; set; }
        public decimal TotalDeductions { get; set; }
        public decimal AdjustedTaxableIncome { get; set; }
        public decimal BaseTax { get; set; }
        public List<TaxCredit> Credits { get; set; }
        public decimal TotalCredits { get; set; }
        public decimal TaxAfterCredits { get; set; }
        public decimal AlternativeMinimumTax { get; set; }
        public decimal TaxProvisionAmount { get; set; }
        public decimal EffectiveProvisionRate { get; set; }
    }

    /// <summary>
    /// Tax Deduction
    /// </summary>
    public class TaxDeduction
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Tax Credit
    /// </summary>
    public class TaxCredit
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Quarterly Estimated Tax
    /// </summary>
    public class QuarterlyEstimatedTax
    {
        public int TaxYear { get; set; }
        public DateTime CalculatedDate { get; set; }
        public decimal AnnualTaxableIncome { get; set; }
        public decimal EstimatedTaxRate { get; set; }
        public List<QuarterlyPayment> Quarters { get; set; }
        public List<QuarterlyAdjustment> Adjustments { get; set; }
        public decimal TotalAnnualPayment { get; set; }
        public decimal TotalAdjustments { get; set; }
    }

    /// <summary>
    /// Quarterly Payment
    /// </summary>
    public class QuarterlyPayment
    {
        public int Quarter { get; set; }
        public DateTime DueDate { get; set; }
        public decimal BasePayment { get; set; }
        public decimal Adjustments { get; set; }
        public decimal TotalPayment { get; set; }
    }

    /// <summary>
    /// Quarterly Adjustment
    /// </summary>
    public class QuarterlyAdjustment
    {
        public int Quarter { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Tax Deferral Analysis
    /// </summary>
    public class TaxDeferralAnalysis
    {
        public DateTime AnalysisDate { get; set; }
        public decimal CurrentYearIncome { get; set; }
        public List<DeferralOpportunity> Opportunities { get; set; }
        public decimal TotalDeferralAmount { get; set; }
        public decimal TotalTaxSavings { get; set; }
        public decimal TotalCashFlowImpact { get; set; }
    }

    /// <summary>
    /// Deferral Opportunity
    /// </summary>
    public class DeferralOpportunity
    {
        public string OpportunityType { get; set; }
        public string Description { get; set; }
        public decimal DeferralAmount { get; set; }
        public decimal TaxSavings { get; set; }
        public decimal CashFlowImpact { get; set; }
        public DateTime Deadline { get; set; }
    }
}
