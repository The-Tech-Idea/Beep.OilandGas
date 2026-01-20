using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.Accounting;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// IAS 1 financial statement presentation package.
    /// </summary>
    public class PresentationService
    {
        private readonly FinancialStatementService _financialStatementService;
        private readonly ILogger<PresentationService> _logger;

        public PresentationService(
            FinancialStatementService financialStatementService,
            ILogger<PresentationService> logger)
        {
            _financialStatementService = financialStatementService ?? throw new ArgumentNullException(nameof(financialStatementService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PresentationPackage> BuildPresentationPackageAsync(
            DateTime periodStart,
            DateTime periodEnd,
            string reportingCurrency,
            string entityName,
            string? bookId = null)
        {
            if (string.IsNullOrWhiteSpace(reportingCurrency))
                throw new ArgumentNullException(nameof(reportingCurrency));
            if (string.IsNullOrWhiteSpace(entityName))
                throw new ArgumentNullException(nameof(entityName));

            var incomeStatement = await _financialStatementService.GenerateIncomeStatementAsync(periodStart, periodEnd, bookId: bookId);
            var balanceSheet = await _financialStatementService.GenerateBalanceSheetAsync(periodEnd, bookId: bookId);
            var cashFlow = await _financialStatementService.GenerateCashFlowStatementAsync(periodStart, periodEnd, bookId: bookId);

            var package = new PresentationPackage
            {
                EntityName = entityName,
                ReportingCurrency = reportingCurrency,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                GeneratedDate = DateTime.UtcNow,
                IncomeStatement = incomeStatement,
                BalanceSheet = balanceSheet,
                CashFlowStatement = cashFlow,
                RequiredDisclosures = new List<string>
                {
                    "Basis of preparation and significant accounting policies",
                    "Management judgments and estimation uncertainty",
                    "Capital management and liquidity risk",
                    "Related party disclosures",
                    "Events after the reporting period",
                    "Revenue recognition policies",
                    "Leases accounting policies",
                    "Provisions and contingent liabilities",
                    "Impairment and recoverable amount methodology",
                    "Income taxes and deferred tax positions",
                    "Foreign currency and translation approach",
                    "Intangible assets and amortization",
                    "Retirement benefit plan disclosures",
                    "Investments in subsidiaries, associates, and joint ventures",
                    "Hyperinflation restatement methodology",
                    "Financial instruments classification and measurement",
                    "Earnings per share disclosures",
                    "Interim reporting basis and comparatives",
                    "Investment property fair value methodology",
                    "Biological assets and agricultural activity disclosures",
                    "Expected credit loss methodology and staging",
                    "Fair value measurement hierarchy and valuation techniques",
                    "First-time adoption reconciliations",
                    "Share-based payment valuation inputs",
                    "Business combination disclosures and goodwill movements",
                    "Assets held for sale and discontinued operations",
                    "Exploration and evaluation policy disclosures",
                    "Operating segment performance and assets",
                    "Consolidation policies and intercompany eliminations",
                    "Joint arrangement classification and commitments",
                    "Interests in other entities disclosure summary",
                    "Regulatory deferral balances and movements",
                    "Financial instrument risk and sensitivity disclosures",
                    "IFRS 18 presentation categories and MPM reconciliation",
                    "IFRS 19 reduced disclosures for subsidiaries",
                    "ASC 326 CECL allowance methodology",
                    "ASC 606 contract asset and liability rollforward",
                    "ASC 842 lease maturity and ROU disclosures"
                }
            };

            _logger?.LogInformation("IAS 1 presentation package generated for {Entity} {Start} - {End}",
                entityName, periodStart, periodEnd);

            return package;
        }
    }
}
