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
    /// Dashboard Service - Executive KPI calculations and summary reporting
    /// Provides financial health metrics, ratios, and trend analysis
    /// Critical for C-suite decision making and board reporting
    /// </summary>
    public class DashboardService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly TrialBalanceService _trialBalanceService;
        private readonly ILogger<DashboardService> _logger;
        private const string ConnectionName = "PPDM39";

        public DashboardService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            TrialBalanceService trialBalanceService,
            ILogger<DashboardService> logger)
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
        /// Generate comprehensive financial dashboard
        /// </summary>
        public async Task<FinancialDashboard> GenerateFinancialDashboardAsync(DateTime asOfDate)
        {
            _logger?.LogInformation("Generating financial dashboard as of {Date}", asOfDate.Date);

            try
            {
                // Get trial balance
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(asOfDate);

                var dashboard = new FinancialDashboard
                {
                    AsOfDate = asOfDate,
                    GeneratedDate = DateTime.UtcNow,
                    KPIs = new List<KPI>()
                };

                // Extract key accounts
                var assets = trialBalance.Where(x => x.ACCOUNT_TYPE == "ASSET").Sum(x => x.CURRENT_BALANCE ?? 0m);
                var currentAssets = trialBalance.Where(x => x.ACCOUNT_TYPE == "ASSET" && x.ACCOUNT_NUMBER.StartsWith("1")).Sum(x => x.CURRENT_BALANCE ?? 0m);
                var liabilities = trialBalance.Where(x => x.ACCOUNT_TYPE == "LIABILITY").Sum(x => x.CURRENT_BALANCE ?? 0m);
                var currentLiabilities = trialBalance.Where(x => x.ACCOUNT_TYPE == "LIABILITY" && x.ACCOUNT_NUMBER.StartsWith("20")).Sum(x => x.CURRENT_BALANCE ?? 0m);
                var equity = trialBalance.Where(x => x.ACCOUNT_TYPE == "EQUITY").Sum(x => x.CURRENT_BALANCE ?? 0m);
                var revenue = trialBalance.Where(x => x.ACCOUNT_TYPE == "REVENUE").Sum(x => x.CURRENT_BALANCE ?? 0m);
                var expenses = trialBalance.Where(x => x.ACCOUNT_TYPE == "EXPENSE" || x.ACCOUNT_TYPE == "COGS").Sum(x => x.CURRENT_BALANCE ?? 0m);
                var cash = trialBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1000")?.CURRENT_BALANCE ?? 0m;
                var inventory = trialBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1300")?.CURRENT_BALANCE ?? 0m;
                var ar = trialBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "1110")?.CURRENT_BALANCE ?? 0m;
                var ap = trialBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "2000")?.CURRENT_BALANCE ?? 0m;
                var longTermDebt = trialBalance.FirstOrDefault(x => x.ACCOUNT_NUMBER == "2100")?.CURRENT_BALANCE ?? 0m;

                // Calculate financial position KPIs
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Total Assets",
                    Value = assets,
                    Format = "Currency",
                    Status = GetStatus(assets, 0),
                    Trend = "Neutral"
                });

                dashboard.KPIs.Add(new KPI
                {
                    Name = "Total Liabilities",
                    Value = liabilities,
                    Format = "Currency",
                    Status = GetStatus(liabilities, 0),
                    Trend = "Neutral"
                });

                dashboard.KPIs.Add(new KPI
                {
                    Name = "Total Equity",
                    Value = equity,
                    Format = "Currency",
                    Status = equity > 0 ? "Good" : "Caution",
                    Trend = "Neutral"
                });

                // Calculate liquidity ratios
                decimal currentRatio = currentLiabilities > 0 ? currentAssets / currentLiabilities : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Current Ratio",
                    Value = currentRatio,
                    Format = "Ratio",
                    Status = currentRatio >= 1.5m ? "Good" : (currentRatio >= 1.0m ? "Caution" : "Poor"),
                    Trend = "Neutral",
                    Target = 1.5m,
                    Threshold = 1.0m
                });

                decimal quickRatio = currentLiabilities > 0 ? (currentAssets - inventory) / currentLiabilities : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Quick Ratio",
                    Value = quickRatio,
                    Format = "Ratio",
                    Status = quickRatio >= 1.0m ? "Good" : (quickRatio >= 0.8m ? "Caution" : "Poor"),
                    Trend = "Neutral",
                    Target = 1.0m,
                    Threshold = 0.8m
                });

                decimal cashRatio = currentLiabilities > 0 ? cash / currentLiabilities : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Cash Ratio",
                    Value = cashRatio,
                    Format = "Ratio",
                    Status = cashRatio >= 0.5m ? "Good" : (cashRatio >= 0.2m ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                // Calculate leverage ratios
                decimal debtToEquity = equity != 0 ? liabilities / equity : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Debt-to-Equity Ratio",
                    Value = debtToEquity,
                    Format = "Ratio",
                    Status = debtToEquity <= 1.0m ? "Good" : (debtToEquity <= 2.0m ? "Caution" : "Poor"),
                    Trend = "Neutral",
                    Target = 1.0m,
                    Threshold = 2.0m
                });

                decimal debtRatio = assets > 0 ? liabilities / assets : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Debt Ratio",
                    Value = debtRatio,
                    Format = "Percentage",
                    Status = debtRatio <= 0.5m ? "Good" : (debtRatio <= 0.7m ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                // Calculate profitability ratios (annualized if needed)
                decimal profit = revenue - expenses;
                decimal profitMargin = revenue > 0 ? profit / revenue : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Profit Margin",
                    Value = profitMargin,
                    Format = "Percentage",
                    Status = profitMargin > 0.15m ? "Good" : (profitMargin > 0 ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                decimal roe = equity != 0 ? profit / equity : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Return on Equity (ROE)",
                    Value = roe,
                    Format = "Percentage",
                    Status = roe > 0.15m ? "Good" : (roe > 0 ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                decimal roa = assets > 0 ? profit / assets : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Return on Assets (ROA)",
                    Value = roa,
                    Format = "Percentage",
                    Status = roa > 0.10m ? "Good" : (roa > 0 ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                // Calculate efficiency ratios
                decimal assetTurnover = revenue > 0 && assets > 0 ? revenue / assets : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Asset Turnover",
                    Value = assetTurnover,
                    Format = "Ratio",
                    Status = assetTurnover > 1.0m ? "Good" : (assetTurnover > 0.5m ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                // Calculate working capital metrics
                decimal workingCapital = currentAssets - currentLiabilities;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Working Capital",
                    Value = workingCapital,
                    Format = "Currency",
                    Status = workingCapital > 0 ? "Good" : "Poor",
                    Trend = "Neutral"
                });

                // Calculate AR and AP metrics
                decimal arTurnover = revenue > 0 && ar > 0 ? revenue / ar : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "AR Turnover Ratio",
                    Value = arTurnover,
                    Format = "Ratio",
                    Status = arTurnover > 5 ? "Good" : (arTurnover > 2 ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                decimal daysInAR = arTurnover > 0 ? 365 / arTurnover : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Days Sales Outstanding (DSO)",
                    Value = daysInAR,
                    Format = "Days",
                    Status = daysInAR < 45 ? "Good" : (daysInAR < 60 ? "Caution" : "Poor"),
                    Trend = "Neutral"
                });

                // Cash flow indicator
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Available Cash",
                    Value = cash,
                    Format = "Currency",
                    Status = cash > 0 ? "Good" : "Poor",
                    Trend = "Neutral"
                });

                // Debt service coverage (simplified)
                decimal debtServiceCoverage = expenses > 0 ? revenue / expenses : 0;
                dashboard.KPIs.Add(new KPI
                {
                    Name = "Revenue to Expense Ratio",
                    Value = debtServiceCoverage,
                    Format = "Ratio",
                    Status = debtServiceCoverage > 1.0m ? "Good" : "Poor",
                    Trend = "Neutral"
                });

                _logger?.LogInformation("Financial dashboard generated. KPIs: {Count}", dashboard.KPIs.Count);
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating financial dashboard: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate executive summary
        /// </summary>
        public async Task<ExecutiveSummary> GenerateExecutiveSummaryAsync(
            DateTime periodStart,
            DateTime periodEnd,
            DateTime asOfDate)
        {
            _logger?.LogInformation("Generating executive summary for period {Start} to {End}",
                periodStart.Date, periodEnd.Date);

            try
            {
                var dashboard = await GenerateFinancialDashboardAsync(asOfDate);
                var trialBalance = await _trialBalanceService.GenerateTrialBalanceAsync(asOfDate);

                var summary = new ExecutiveSummary
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    AsOfDate = asOfDate,
                    GeneratedDate = DateTime.UtcNow,
                    HighlightedMetrics = new List<HighlightedMetric>()
                };

                // Top 3 metrics
                summary.HighlightedMetrics.Add(new HighlightedMetric
                {
                    Label = "Financial Health",
                    Value = dashboard.KPIs.FirstOrDefault(x => x.Name == "Current Ratio")?.Value ?? 0,
                    Status = dashboard.KPIs.FirstOrDefault(x => x.Name == "Current Ratio")?.Status ?? "Unknown",
                    Insight = "Liquidity position"
                });

                summary.HighlightedMetrics.Add(new HighlightedMetric
                {
                    Label = "Profitability",
                    Value = dashboard.KPIs.FirstOrDefault(x => x.Name == "Profit Margin")?.Value ?? 0,
                    Status = dashboard.KPIs.FirstOrDefault(x => x.Name == "Profit Margin")?.Status ?? "Unknown",
                    Insight = "Operational efficiency"
                });

                summary.HighlightedMetrics.Add(new HighlightedMetric
                {
                    Label = "Leverage",
                    Value = dashboard.KPIs.FirstOrDefault(x => x.Name == "Debt-to-Equity Ratio")?.Value ?? 0,
                    Status = dashboard.KPIs.FirstOrDefault(x => x.Name == "Debt-to-Equity Ratio")?.Status ?? "Unknown",
                    Insight = "Capital structure"
                });

                summary.TotalAssets = trialBalance.Where(x => x.ACCOUNT_TYPE == "ASSET").Sum(x => x.CURRENT_BALANCE ?? 0m);
                summary.TotalLiabilities = trialBalance.Where(x => x.ACCOUNT_TYPE == "LIABILITY").Sum(x => x.CURRENT_BALANCE ?? 0m);
                summary.TotalEquity = trialBalance.Where(x => x.ACCOUNT_TYPE == "EQUITY").Sum(x => x.CURRENT_BALANCE ?? 0m);

                var revenue = trialBalance.Where(x => x.ACCOUNT_TYPE == "REVENUE").Sum(x => x.CURRENT_BALANCE ?? 0m);
                var expenses = trialBalance.Where(x => x.ACCOUNT_TYPE == "EXPENSE" || x.ACCOUNT_TYPE == "COGS").Sum(x => x.CURRENT_BALANCE ?? 0m);
                summary.NetIncome = revenue - expenses;

                summary.HealthScore = CalculateHealthScore(dashboard.KPIs);

                _logger?.LogInformation("Executive summary generated. Health Score: {Score:N1}%", summary.HealthScore);
                return summary;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating executive summary: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate trend analysis
        /// </summary>
        public async Task<TrendAnalysis> AnalyzeTrendsAsync(
            DateTime periodStart,
            DateTime periodEnd,
            int monthsToAnalyze = 12)
        {
            _logger?.LogInformation("Analyzing trends from {Start} to {End}",
                periodStart.Date, periodEnd.Date);

            try
            {
                var analysis = new TrendAnalysis
                {
                    AnalysisStart = periodStart,
                    AnalysisEnd = periodEnd,
                    MonthsAnalyzed = monthsToAnalyze,
                    AnalysisDate = DateTime.UtcNow,
                    TrendLines = new List<TrendLine>()
                };

                // Calculate monthly snapshots
                var currentDate = periodStart;
                var values = new List<decimal>();

                while (currentDate <= periodEnd)
                {
                    var dashboard = await GenerateFinancialDashboardAsync(currentDate);
                    var revenue = dashboard.KPIs.FirstOrDefault(x => x.Name.Contains("Profit"))?.Value ?? 0;
                    values.Add(revenue);
                    currentDate = currentDate.AddMonths(1);
                }

                // Calculate trend line
                if (values.Count > 1)
                {
                    decimal firstValue = values.First();
                    decimal lastValue = values.Last();
                    decimal change = lastValue - firstValue;
                    decimal changePercent = firstValue != 0 ? (change / firstValue) * 100 : 0;

                    analysis.TrendLines.Add(new TrendLine
                    {
                        MetricName = "Revenue Trend",
                        StartValue = firstValue,
                        EndValue = lastValue,
                        Change = change,
                        ChangePercent = changePercent,
                        Direction = change > 0 ? "Upward" : (change < 0 ? "Downward" : "Flat"),
                        DataPoints = values.Count
                    });
                }

                analysis.OverallTrend = analysis.TrendLines.Any(x => x.ChangePercent > 0) ? "Positive" : "Negative";

                _logger?.LogInformation("Trend analysis completed. Overall Trend: {Trend}", analysis.OverallTrend);
                return analysis;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing trends: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export dashboard as formatted text
        /// </summary>
        public string ExportDashboardAsText(FinancialDashboard dashboard)
        {
            _logger?.LogInformation("Exporting dashboard as text");

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine("                    FINANCIAL EXECUTIVE DASHBOARD");
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();
                sb.AppendLine($"As of: {dashboard.AsOfDate:MMMM dd, yyyy}");
                sb.AppendLine();

                // Group KPIs by category
                var liquidityKPIs = dashboard.KPIs.Where(x => x.Name.Contains("Ratio") || x.Name.Contains("Cash")).ToList();
                var profitabilityKPIs = dashboard.KPIs.Where(x => x.Name.Contains("Margin") || x.Name.Contains("Return")).ToList();
                var leverageKPIs = dashboard.KPIs.Where(x => x.Name.Contains("Debt") || x.Name.Contains("Equity")).ToList();
                var efficiencyKPIs = dashboard.KPIs.Where(x => x.Name.Contains("Turnover") || x.Name.Contains("Days")).ToList();

                sb.AppendLine("LIQUIDITY METRICS:");
                sb.AppendLine("─────────────────────────────────────────────────────────────────");
                foreach (var kpi in liquidityKPIs.OrderByDescending(x => x.Status))
                {
                    string value = FormatKPIValue(kpi);
                    sb.AppendLine($"  {kpi.Name,-35} {value,15} [{kpi.Status}]");
                }
                sb.AppendLine();

                sb.AppendLine("PROFITABILITY METRICS:");
                sb.AppendLine("─────────────────────────────────────────────────────────────────");
                foreach (var kpi in profitabilityKPIs.OrderByDescending(x => x.Status))
                {
                    string value = FormatKPIValue(kpi);
                    sb.AppendLine($"  {kpi.Name,-35} {value,15} [{kpi.Status}]");
                }
                sb.AppendLine();

                sb.AppendLine("LEVERAGE METRICS:");
                sb.AppendLine("─────────────────────────────────────────────────────────────────");
                foreach (var kpi in leverageKPIs.OrderByDescending(x => x.Status))
                {
                    string value = FormatKPIValue(kpi);
                    sb.AppendLine($"  {kpi.Name,-35} {value,15} [{kpi.Status}]");
                }
                sb.AppendLine();

                sb.AppendLine("EFFICIENCY METRICS:");
                sb.AppendLine("─────────────────────────────────────────────────────────────────");
                foreach (var kpi in efficiencyKPIs.OrderByDescending(x => x.Status))
                {
                    string value = FormatKPIValue(kpi);
                    sb.AppendLine($"  {kpi.Name,-35} {value,15} [{kpi.Status}]");
                }
                sb.AppendLine();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine($"Report Generated: {dashboard.GeneratedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting dashboard: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Helper: Get status based on value
        /// </summary>
        private string GetStatus(decimal value, decimal threshold)
        {
            if (value > threshold * 1.2m) return "Good";
            if (value > threshold) return "Caution";
            return "Poor";
        }

        /// <summary>
        /// Helper: Format KPI value for display
        /// </summary>
        private string FormatKPIValue(KPI kpi)
        {
            return kpi.Format switch
            {
                "Currency" => $"${kpi.Value:N2}",
                "Percentage" => $"{(kpi.Value * 100):N1}%",
                "Ratio" => $"{kpi.Value:N2}",
                "Days" => $"{kpi.Value:N0}",
                _ => $"{kpi.Value:N2}"
            };
        }

        /// <summary>
        /// Helper: Calculate overall health score (0-100)
        /// </summary>
        private decimal CalculateHealthScore(List<KPI> kpis)
        {
            if (!kpis.Any()) return 0;

            decimal score = 0;
            foreach (var kpi in kpis)
            {
                if (kpi.Status == "Good") score += 100;
                else if (kpi.Status == "Caution") score += 50;
                // Poor = 0
            }

            return score / kpis.Count;
        }
    }

    /// <summary>
    /// Financial Dashboard
    /// </summary>
    public class FinancialDashboard
    {
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public List<KPI> KPIs { get; set; }
    }

    /// <summary>
    /// Key Performance Indicator
    /// </summary>
    public class KPI
    {
        public string Name { get; set; }
        public decimal Value { get; set; }
        public string Format { get; set; }  // Currency, Percentage, Ratio, Days
        public string Status { get; set; }  // Good, Caution, Poor
        public string Trend { get; set; }   // Upward, Downward, Neutral
        public decimal Target { get; set; }
        public decimal Threshold { get; set; }
    }

    /// <summary>
    /// Executive Summary
    /// </summary>
    public class ExecutiveSummary
    {
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime GeneratedDate { get; set; }
        public List<HighlightedMetric> HighlightedMetrics { get; set; }
        public decimal TotalAssets { get; set; }
        public decimal TotalLiabilities { get; set; }
        public decimal TotalEquity { get; set; }
        public decimal NetIncome { get; set; }
        public decimal HealthScore { get; set; }
    }

    /// <summary>
    /// Highlighted Metric
    /// </summary>
    public class HighlightedMetric
    {
        public string Label { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; }
        public string Insight { get; set; }
    }

    /// <summary>
    /// Trend Analysis
    /// </summary>
    public class TrendAnalysis
    {
        public DateTime AnalysisStart { get; set; }
        public DateTime AnalysisEnd { get; set; }
        public int MonthsAnalyzed { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<TrendLine> TrendLines { get; set; }
        public string OverallTrend { get; set; }
    }

    /// <summary>
    /// Trend Line
    /// </summary>
    public class TrendLine
    {
        public string MetricName { get; set; }
        public decimal StartValue { get; set; }
        public decimal EndValue { get; set; }
        public decimal Change { get; set; }
        public decimal ChangePercent { get; set; }
        public string Direction { get; set; }
        public int DataPoints { get; set; }
    }
}
