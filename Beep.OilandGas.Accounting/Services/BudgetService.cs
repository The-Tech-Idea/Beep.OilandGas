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
    /// Budget Service - Budget planning, tracking, and variance analysis
    /// Supports monthly, quarterly, and annual budget vs actual comparison
    /// Essential for financial planning and performance management
    /// </summary>
    public class BudgetService
    {
        private readonly IDMEEditor _editor;
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly GLAccountService _glAccountService;
        private readonly ILogger<BudgetService> _logger;
        private const string ConnectionName = "PPDM39";

        public BudgetService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            GLAccountService glAccountService,
            ILogger<BudgetService> logger)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _glAccountService = glAccountService ?? throw new ArgumentNullException(nameof(glAccountService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Create/Save budget for a period
        /// </summary>
        public async Task<Budget> CreateBudgetAsync(
            string budgetName,
            DateTime budgetStart,
            DateTime budgetEnd,
            List<BudgetLine> budgetLines,
            string userId)
        {
            _logger?.LogInformation("Creating budget {Name} from {Start} to {End}",
                budgetName, budgetStart.Date, budgetEnd.Date);

            try
            {
                if (string.IsNullOrWhiteSpace(budgetName))
                    throw new ArgumentNullException(nameof(budgetName));

                if (budgetLines == null || !budgetLines.Any())
                    throw new InvalidOperationException("Budget must contain at least one line item");

                var budget = new Budget
                {
                    BudgetName = budgetName,
                    BudgetStart = budgetStart,
                    BudgetEnd = budgetEnd,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = userId,
                    BudgetLines = budgetLines,
                    Status = "ACTIVE"
                };

                // Calculate totals
                budget.TotalBudgeted = budgetLines.Sum(x => x.BudgetAmount);
                budget.BudgetMonths = ((budgetEnd.Year - budgetStart.Year) * 12 + (budgetEnd.Month - budgetStart.Month) + 1);

                _logger?.LogInformation("Budget created. Name: {Name}, Total: {Total:C}, Lines: {Count}",
                    budgetName, budget.TotalBudgeted, budgetLines.Count);

                return budget;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating budget: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generate budget vs actual comparison
        /// </summary>
        public async Task<BudgetVarianceReport> GenerateBudgetVarianceReportAsync(
            Budget budget,
            DateTime asOfDate,
            string reportName = "")
        {
            _logger?.LogInformation("Generating budget variance report for {Budget} as of {Date}",
                budget.BudgetName, asOfDate.Date);

            try
            {
                if (budget == null)
                    throw new ArgumentNullException(nameof(budget));

                if (string.IsNullOrWhiteSpace(reportName))
                    reportName = $"Budget vs Actual - {budget.BudgetName} as of {asOfDate:MMMM dd, yyyy}";

                var report = new BudgetVarianceReport
                {
                    ReportName = reportName,
                    GeneratedDate = DateTime.UtcNow,
                    AsOfDate = asOfDate,
                    BudgetName = budget.BudgetName,
                    BudgetPeriod = $"{budget.BudgetStart:MMM yyyy} to {budget.BudgetEnd:MMM yyyy}",
                    VarianceLines = new List<BudgetVarianceLine>()
                };

                decimal totalBudget = 0m;
                decimal totalActual = 0m;
                decimal totalVariance = 0m;

                // Process each budget line
                foreach (var budgetLine in budget.BudgetLines)
                {
                    // Get actual GL balance
                    var actualBalance = await _glAccountService.GetAccountBalanceAsync(budgetLine.AccountNumber, asOfDate);

                    decimal budgetAmount = budgetLine.BudgetAmount;
                    decimal actualAmount = Math.Abs(actualBalance);
                    decimal variance = actualAmount - budgetAmount;
                    decimal variancePercent = budgetAmount != 0 ? (variance / budgetAmount) * 100 : 0;

                    var varianceLine = new BudgetVarianceLine
                    {
                        AccountNumber = budgetLine.AccountNumber,
                        AccountName = budgetLine.AccountName,
                        BudgetAmount = budgetAmount,
                        ActualAmount = actualAmount,
                        Variance = variance,
                        VariancePercent = variancePercent,
                        Status = Math.Abs(variance) < (budgetAmount * 0.05m) ? "ON_TRACK" : "OVER_BUDGET"
                    };

                    report.VarianceLines.Add(varianceLine);

                    totalBudget += budgetAmount;
                    totalActual += actualAmount;
                    totalVariance += variance;
                }

                report.TotalBudget = totalBudget;
                report.TotalActual = totalActual;
                report.TotalVariance = totalVariance;
                report.VariancePercent = totalBudget != 0 ? (totalVariance / totalBudget) * 100 : 0;
                report.VarianceStatus = Math.Abs(totalVariance) < (totalBudget * 0.10m) ? "ACCEPTABLE" : "REVIEW_REQUIRED";

                // Sort by variance (largest first)
                report.VarianceLines = report.VarianceLines.OrderByDescending(x => Math.Abs(x.Variance)).ToList();

                _logger?.LogInformation("Budget variance report generated. Total Variance: {Variance:C} ({Percent:P})",
                    totalVariance, report.VariancePercent / 100);

                return report;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating budget variance report: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate year-to-date budget performance
        /// </summary>
        public async Task<YTDPerformance> CalculateYTDPerformanceAsync(
            Budget budget,
            DateTime asOfDate)
        {
            _logger?.LogInformation("Calculating YTD performance for {Budget}", budget.BudgetName);

            try
            {
                if (budget == null)
                    throw new ArgumentNullException(nameof(budget));

                var performance = new YTDPerformance
                {
                    BudgetName = budget.BudgetName,
                    AsOfDate = asOfDate,
                    CalculatedDate = DateTime.UtcNow,
                    BudgetYearStart = new DateTime(asOfDate.Year, 1, 1),
                    Items = new List<YTDItem>()
                };

                decimal yearBudgeted = 0m;
                decimal yearActual = 0m;

                // Calculate portion of year elapsed
                var yearStart = new DateTime(asOfDate.Year, 1, 1);
                int daysInYear = DateTime.IsLeapYear(asOfDate.Year) ? 366 : 365;
                int daysElapsed = (int)(asOfDate - yearStart).TotalDays + 1;
                performance.ProportionOfYearElapsed = (decimal)daysElapsed / daysInYear;
                performance.MonthsElapsed = daysElapsed / 30;

                // Process each budget line
                foreach (var budgetLine in budget.BudgetLines)
                {
                    var ytdActual = await _glAccountService.GetAccountBalanceAsync(budgetLine.AccountNumber, asOfDate);
                    var ytdActualAbs = Math.Abs(ytdActual);

                    // Calculate pro-rata budget for YTD
                    decimal proRataBudget = budgetLine.BudgetAmount * performance.ProportionOfYearElapsed;

                    var item = new YTDItem
                    {
                        AccountNumber = budgetLine.AccountNumber,
                        AccountName = budgetLine.AccountName,
                        FullYearBudget = budgetLine.BudgetAmount,
                        ProRataBudget = proRataBudget,
                        YTDActual = ytdActualAbs,
                        YTDVariance = ytdActualAbs - proRataBudget,
                        OnTrack = ytdActualAbs <= proRataBudget * 1.10m  // 10% tolerance
                    };

                    performance.Items.Add(item);

                    yearBudgeted += budgetLine.BudgetAmount;
                    yearActual += ytdActualAbs;
                }

                performance.FullYearBudget = yearBudgeted;
                performance.ProRataBudget = yearBudgeted * performance.ProportionOfYearElapsed;
                performance.YTDActual = yearActual;
                performance.YTDVariance = yearActual - performance.ProRataBudget;
                performance.OnTrackCount = performance.Items.Count(x => x.OnTrack);
                performance.OffTrackCount = performance.Items.Count(x => !x.OnTrack);

                _logger?.LogInformation("YTD Performance: On Track: {OnTrack}, Off Track: {OffTrack}",
                    performance.OnTrackCount, performance.OffTrackCount);

                return performance;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating YTD performance: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Forecast budget usage through year-end
        /// </summary>
        public async Task<BudgetForecast> ForecastBudgetAsync(
            Budget budget,
            DateTime asOfDate,
            TrendMethod method = TrendMethod.LinearTrend)
        {
            _logger?.LogInformation("Forecasting budget using {Method}", method);

            try
            {
                if (budget == null)
                    throw new ArgumentNullException(nameof(budget));

                var forecast = new BudgetForecast
                {
                    BudgetName = budget.BudgetName,
                    AsOfDate = asOfDate,
                    ForecastDate = DateTime.UtcNow,
                    YearEnd = new DateTime(asOfDate.Year, 12, 31),
                    ForecastMethod = method.ToString(),
                    Items = new List<ForecastItem>()
                };

                // Calculate days remaining in year
                int daysRemaining = (int)(forecast.YearEnd - asOfDate).TotalDays;
                forecast.DaysRemaining = daysRemaining;

                // Process each budget line
                foreach (var budgetLine in budget.BudgetLines)
                {
                    var ytdActual = await _glAccountService.GetAccountBalanceAsync(budgetLine.AccountNumber, asOfDate);
                    decimal ytdActualAbs = Math.Abs(ytdActual);

                    // Calculate daily run-rate
                    int daysElapsed = (int)(asOfDate - new DateTime(asOfDate.Year, 1, 1)).TotalDays + 1;
                    decimal dailyRate = daysElapsed > 0 ? ytdActualAbs / daysElapsed : 0;

                    // Forecast year-end based on trend
                    decimal forecastedYearEnd = ytdActualAbs + (dailyRate * daysRemaining);

                    // Calculate variance to full year budget
                    decimal forecastVariance = forecastedYearEnd - budgetLine.BudgetAmount;
                    decimal forecastVariancePercent = budgetLine.BudgetAmount != 0 ? (forecastVariance / budgetLine.BudgetAmount) * 100 : 0;

                    var forecastItem = new ForecastItem
                    {
                        AccountNumber = budgetLine.AccountNumber,
                        AccountName = budgetLine.AccountName,
                        FullYearBudget = budgetLine.BudgetAmount,
                        YTDActual = ytdActualAbs,
                        ForecastedYearEnd = forecastedYearEnd,
                        ForecastedVariance = forecastVariance,
                        ForecastedVariancePercent = forecastVariancePercent,
                        WillExceedBudget = forecastedYearEnd > budgetLine.BudgetAmount
                    };

                    forecast.Items.Add(forecastItem);
                }

                forecast.TotalBudget = budget.BudgetLines.Sum(x => x.BudgetAmount);
                forecast.TotalYTDActual = forecast.Items.Sum(x => x.YTDActual);
                forecast.TotalForecastedYearEnd = forecast.Items.Sum(x => x.ForecastedYearEnd);
                forecast.TotalForecastedVariance = forecast.TotalForecastedYearEnd - forecast.TotalBudget;
                forecast.ExceedingBudgetCount = forecast.Items.Count(x => x.WillExceedBudget);

                _logger?.LogInformation("Budget forecast: {Count} accounts projected to exceed budget",
                    forecast.ExceedingBudgetCount);

                return forecast;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error forecasting budget: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Export budget variance report as formatted text
        /// </summary>
        public string ExportBudgetVarianceReportAsText(BudgetVarianceReport report)
        {
            _logger?.LogInformation("Exporting budget variance report as text");

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine(report.ReportName);
                sb.AppendLine("═══════════════════════════════════════════════════════════════════");
                sb.AppendLine();

                sb.AppendLine($"Budget Period:  {report.BudgetPeriod}");
                sb.AppendLine($"Report As Of:   {report.AsOfDate:MMMM dd, yyyy}");
                sb.AppendLine($"Status:         {report.VarianceStatus}");
                sb.AppendLine();

                sb.AppendLine("Account# | Account Name              | Budgeted     | Actual       | Variance     | Variance%  | Status");
                sb.AppendLine("─────────────────────────────────────────────────────────────────────────────────────────────────────");

                foreach (var line in report.VarianceLines)
                {
                    sb.AppendLine($"{line.AccountNumber,-8} | {line.AccountName,-25} | ${line.BudgetAmount,11:N2} | ${line.ActualAmount,11:N2} | ${line.Variance,11:N2} | {line.VariancePercent,9:N1}% | {line.Status}");
                }

                sb.AppendLine("═════════════════════════════════════════════════════════════════════════════════════════════════════");
                sb.AppendLine($"{"TOTAL",-34} | ${report.TotalBudget,11:N2} | ${report.TotalActual,11:N2} | ${report.TotalVariance,11:N2} | {report.VariancePercent,9:N1}%");
                sb.AppendLine("═════════════════════════════════════════════════════════════════════════════════════════════════════");
                sb.AppendLine();
                sb.AppendLine($"Report Generated: {report.GeneratedDate:MMMM dd, yyyy hh:mm:ss tt}");

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error exporting budget variance report: {Message}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Budget Definition
    /// </summary>
    public class Budget
    {
        public string BudgetName { get; set; }
        public DateTime BudgetStart { get; set; }
        public DateTime BudgetEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public List<BudgetLine> BudgetLines { get; set; }
        public decimal TotalBudgeted { get; set; }
        public int BudgetMonths { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Budget Line Item
    /// </summary>
    public class BudgetLine
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BudgetAmount { get; set; }
    }

    /// <summary>
    /// Budget Variance Report
    /// </summary>
    public class BudgetVarianceReport
    {
        public string ReportName { get; set; }
        public DateTime GeneratedDate { get; set; }
        public DateTime AsOfDate { get; set; }
        public string BudgetName { get; set; }
        public string BudgetPeriod { get; set; }
        public List<BudgetVarianceLine> VarianceLines { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalActual { get; set; }
        public decimal TotalVariance { get; set; }
        public decimal VariancePercent { get; set; }
        public string VarianceStatus { get; set; }
    }

    /// <summary>
    /// Budget Variance Line
    /// </summary>
    public class BudgetVarianceLine
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal BudgetAmount { get; set; }
        public decimal ActualAmount { get; set; }
        public decimal Variance { get; set; }
        public decimal VariancePercent { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Year-to-Date Performance
    /// </summary>
    public class YTDPerformance
    {
        public string BudgetName { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime CalculatedDate { get; set; }
        public DateTime BudgetYearStart { get; set; }
        public decimal ProportionOfYearElapsed { get; set; }
        public decimal MonthsElapsed { get; set; }
        public decimal FullYearBudget { get; set; }
        public decimal ProRataBudget { get; set; }
        public decimal YTDActual { get; set; }
        public decimal YTDVariance { get; set; }
        public List<YTDItem> Items { get; set; }
        public int OnTrackCount { get; set; }
        public int OffTrackCount { get; set; }
    }

    /// <summary>
    /// YTD Item
    /// </summary>
    public class YTDItem
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal FullYearBudget { get; set; }
        public decimal ProRataBudget { get; set; }
        public decimal YTDActual { get; set; }
        public decimal YTDVariance { get; set; }
        public bool OnTrack { get; set; }
    }

    /// <summary>
    /// Budget Forecast
    /// </summary>
    public class BudgetForecast
    {
        public string BudgetName { get; set; }
        public DateTime AsOfDate { get; set; }
        public DateTime ForecastDate { get; set; }
        public DateTime YearEnd { get; set; }
        public int DaysRemaining { get; set; }
        public string ForecastMethod { get; set; }
        public List<ForecastItem> Items { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal TotalYTDActual { get; set; }
        public decimal TotalForecastedYearEnd { get; set; }
        public decimal TotalForecastedVariance { get; set; }
        public int ExceedingBudgetCount { get; set; }
    }

    /// <summary>
    /// Forecast Item
    /// </summary>
    public class ForecastItem
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public decimal FullYearBudget { get; set; }
        public decimal YTDActual { get; set; }
        public decimal ForecastedYearEnd { get; set; }
        public decimal ForecastedVariance { get; set; }
        public decimal ForecastedVariancePercent { get; set; }
        public bool WillExceedBudget { get; set; }
    }

    /// <summary>
    /// Trend forecasting method
    /// </summary>
    public enum TrendMethod
    {
        LinearTrend,
        MovingAverage,
        ExponentialSmoothing
    }
}
