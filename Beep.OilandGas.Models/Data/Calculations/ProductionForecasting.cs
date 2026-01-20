using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Request for generating production forecast
    /// </summary>
    public class GenerateForecastRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier) - optional if FieldId is provided
        /// </summary>
        public string? WellUWI { get; set; }

        /// <summary>
        /// Field identifier - optional if WellUWI is provided
        /// </summary>
        public string? FieldId { get; set; }

        /// <summary>
        /// Forecast method (e.g., "DCA", "ARPS", "HYP")
        /// </summary>
        [Required(ErrorMessage = "ForecastMethod is required")]
        public string ForecastMethod { get; set; } = string.Empty;

        /// <summary>
        /// Forecast period in months
        /// </summary>
        [Required]
        [Range(1, 600, ErrorMessage = "ForecastPeriod must be between 1 and 600 months")]
        public int ForecastPeriod { get; set; }
        /// <summary>
        /// Variable operating cost per barrel (USD/STB)
        /// </summary>
        public decimal OperatingCostPerBarrel { get; set; } = 10m;

        /// <summary>
        /// Fixed OPEX per period (USD)
        /// </summary>
        public decimal FixedOpexPerPeriod { get; set; } = 0m;

        /// <summary>
        /// Optional capital schedule (date, amount) to apply per period
        /// </summary>
        public List<CapitalScheduleItem> CapitalSchedule { get; set; } = new();
    }

    /// <summary>
    /// Request for decline curve analysis
    /// </summary>
    public class DeclineCurveAnalysisRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Start date for analysis period
        /// </summary>
        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date for analysis period
        /// </summary>
        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// Production forecast point DTO
    /// </summary>
    public class ProductionForecastPoint : ModelEntityBase
    {
        /// <summary>
        /// Forecast date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Oil production rate (STB/day)
        /// </summary>
        public decimal? OilRate { get; set; }

        /// <summary>
        /// Gas production rate (MSCF/day)
        /// </summary>
        public decimal? GasRate { get; set; }

        /// <summary>
        /// Water production rate (STB/day)
        /// </summary>
        public decimal? WaterRate { get; set; }

        /// <summary>
        /// Cumulative oil production (STB)
        /// </summary>
        public decimal? CumulativeOil { get; set; }

        /// <summary>
        /// Cumulative gas production (MSCF)
        /// </summary>
        public decimal? CumulativeGas { get; set; }

        /// <summary>
        /// Confidence level (for probabilistic forecasts)
        /// </summary>
        public decimal? ConfidenceLevel { get; set; }

        /// <summary>
        /// P10 rate (low case)
        /// </summary>
        public decimal? P10Rate { get; set; }

        /// <summary>
        /// P50 rate (most likely case)
        /// </summary>
        public decimal? P50Rate { get; set; }

        /// <summary>
        /// P90 rate (high case)
        /// </summary>
        public decimal? P90Rate { get; set; }
    }

    /// <summary>
    /// Production forecast result DTO
    /// </summary>
    public class ProductionForecastResult : ModelEntityBase
    {
        /// <summary>
        /// Forecast ID
        /// </summary>
        public string ForecastId { get; set; } = string.Empty;

        /// <summary>
        /// Well UWI
        /// </summary>
        public string? WellUWI { get; set; }

        /// <summary>
        /// Field ID
        /// </summary>
        public string? FieldId { get; set; }

        /// <summary>
        /// Forecast creation date
        /// </summary>
        public DateTime ForecastDate { get; set; }

        /// <summary>
        /// Forecast method used
        /// </summary>
        public string ForecastMethod { get; set; } = string.Empty;

        /// <summary>
        /// Forecast points
        /// </summary>
        public List<ProductionForecastPoint> ForecastPoints { get; set; } = new();

        /// <summary>
        /// Estimated reserves (STB or MSCF)
        /// </summary>
        public decimal EstimatedReserves { get; set; }

        /// <summary>
        /// Forecast status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// R-squared for curve fit quality
        /// </summary>
        public decimal? RSquared { get; set; }

        /// <summary>
        /// Root mean square error
        /// </summary>
        public decimal? RMSE { get; set; }

        /// <summary>
        /// Forecast confidence interval (%)
        /// </summary>
        public decimal? ConfidenceInterval { get; set; }

        /// <summary>
        /// Economic limit rate (STB/day)
        /// </summary>
        public decimal? EconomicLimit { get; set; }

        /// <summary>
        /// Forecast notes/comments
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Decline curve analysis result DTO
    /// </summary>
    public class DeclineCurveAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Analysis ID
        /// </summary>
        public string AnalysisId { get; set; } = string.Empty;

        /// <summary>
        /// Well UWI
        /// </summary>
        public string WellUWI { get; set; } = string.Empty;

        /// <summary>
        /// Analysis date
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Decline type (Exponential, Hyperbolic, Harmonic)
        /// </summary>
        public string DeclineType { get; set; } = string.Empty;

        /// <summary>
        /// Initial decline rate (1/time)
        /// </summary>
        public decimal DeclineRate { get; set; }

        /// <summary>
        /// Hyperbolic b-factor (for hyperbolic decline)
        /// </summary>
        public decimal? BFactor { get; set; }

        /// <summary>
        /// Initial production rate (STB/day or MSCF/day)
        /// </summary>
        public decimal InitialRate { get; set; }

        /// <summary>
        /// Estimated ultimate recovery (EUR)
        /// </summary>
        public decimal EstimatedReserves { get; set; }

        /// <summary>
        /// R-squared for curve fit quality
        /// </summary>
        public decimal? RSquared { get; set; }

        /// <summary>
        /// Root mean square error
        /// </summary>
        public decimal? RMSE { get; set; }

        /// <summary>
        /// Mean absolute error
        /// </summary>
        public decimal? MAE { get; set; }

        /// <summary>
        /// Akaike information criterion
        /// </summary>
        public decimal? AIC { get; set; }

        /// <summary>
        /// Bayesian information criterion
        /// </summary>
        public decimal? BIC { get; set; }

        /// <summary>
        /// Analysis status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Analysis notes/comments
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Historical production data points used in analysis
        /// </summary>
        public List<ForecastProductionDataPoint> HistoricalData { get; set; } = new();
    }

    /// <summary>
    /// Production data point for analysis
    /// </summary>
    public class ForecastProductionDataPoint : ModelEntityBase
    {
        /// <summary>
        /// Production date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Time since start (days)
        /// </summary>
        public decimal Time { get; set; }

        /// <summary>
        /// Oil rate (STB/day)
        /// </summary>
        public decimal? OilRate { get; set; }

        /// <summary>
        /// Gas rate (MSCF/day)
        /// </summary>
        public decimal? GasRate { get; set; }

        /// <summary>
        /// Water rate (STB/day)
        /// </summary>
        public decimal? WaterRate { get; set; }

        /// <summary>
        /// Cumulative production
        /// </summary>
        public decimal? Cumulative { get; set; }
    }

    /// <summary>
    /// Probabilistic forecast result DTO
    /// </summary>
    public class ProbabilisticForecast : ModelEntityBase
    {
        /// <summary>
        /// Forecast ID
        /// </summary>
        public string ForecastId { get; set; } = string.Empty;

        /// <summary>
        /// Well UWI
        /// </summary>
        public string? WellUWI { get; set; }

        /// <summary>
        /// Field ID
        /// </summary>
        public string? FieldId { get; set; }

        /// <summary>
        /// P10 case forecast (low case)
        /// </summary>
        public ProductionForecastResult P10Forecast { get; set; } = new();

        /// <summary>
        /// P50 case forecast (most likely)
        /// </summary>
        public ProductionForecastResult P50Forecast { get; set; } = new();

        /// <summary>
        /// P90 case forecast (high case)
        /// </summary>
        public ProductionForecastResult P90Forecast { get; set; } = new();

        /// <summary>
        /// Expected value forecast
        /// </summary>
        public ProductionForecastResult ExpectedForecast { get; set; } = new();

        /// <summary>
        /// Risk analysis results
        /// </summary>
        public ForecastRiskAnalysisResult RiskAnalysis { get; set; } = new();
    }

    /// <summary>
    /// Risk analysis result DTO
    /// </summary>
    public class ForecastRiskAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Risk analysis ID
        /// </summary>
        public string AnalysisId { get; set; } = string.Empty;

        /// <summary>
        /// Probability of commercial success (%)
        /// </summary>
        public decimal CommercialSuccessProbability { get; set; }

        /// <summary>
        /// Risk factors identified
        /// </summary>
        public List<RiskFactor> RiskFactors { get; set; } = new();

        /// <summary>
        /// Mitigation strategies
        /// </summary>
        public List<string> MitigationStrategies { get; set; } = new();

        /// <summary>
        /// Overall risk rating
        /// </summary>
        public string RiskRating { get; set; } = string.Empty;
    }

    /// <summary>
    /// Risk factor DTO
    /// </summary>
    public class RiskFactor : ModelEntityBase
    {
        /// <summary>
        /// Risk factor description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Risk probability (%)
        /// </summary>
        public decimal Probability { get; set; }

        /// <summary>
        /// Risk impact (High, Medium, Low)
        /// </summary>
        public string Impact { get; set; } = string.Empty;

        /// <summary>
        /// Risk category
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }

    /// <summary>
    /// Economic analysis result DTO
    /// </summary>
    public class EconomicAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Analysis ID
        /// </summary>
        public string AnalysisId { get; set; } = string.Empty;

        /// <summary>
        /// Net present value (NPV)
        /// </summary>
        public decimal NPV { get; set; }

        /// <summary>
        /// Internal rate of return (IRR) (%)
        /// </summary>
        public decimal IRR { get; set; }

        /// <summary>
        /// Profitability index
        /// </summary>
        public decimal ProfitabilityIndex { get; set; }

        /// <summary>
        /// Payback period (years)
        /// </summary>
        public decimal PaybackPeriod { get; set; }

        /// <summary>
        /// Break-even price ($/unit)
        /// </summary>
        public decimal BreakEvenPrice { get; set; }

        /// <summary>
        /// Discount rate used (%)
        /// </summary>
        public decimal DiscountRate { get; set; }

        /// <summary>
        /// Economic limit (STB/day)
        /// </summary>
        public decimal EconomicLimit { get; set; }

        /// <summary>
        /// Cash flow projections
        /// </summary>
        public List<CashFlowPoint> CashFlows { get; set; } = new();
    }

    /// <summary>
    /// Capital schedule item for CAPEX by date
    /// </summary>
    public class CapitalScheduleItem : ModelEntityBase
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }

    /// <summary>
    /// Request for performing economic analysis
    /// </summary>
    public class EconomicAnalysisRequest : ModelEntityBase
    {
        public string ForecastId { get; set; } = string.Empty;
        public decimal OilPrice { get; set; }
        public decimal GasPrice { get; set; }
        public decimal DiscountRate { get; set; }

        public decimal OperatingCostPerBarrel { get; set; } = 10m;
        public decimal FixedOpexPerPeriod { get; set; } = 0m;
        public List<CapitalScheduleItem> CapitalSchedule { get; set; } = new();
    }

    /// <summary>
    /// Cash flow point DTO
    /// </summary>
    public class CashFlowPoint : ModelEntityBase
    {
        /// <summary>
        /// Period date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Revenue ($)
        /// </summary>
        public decimal Revenue { get; set; }

        /// <summary>
        /// Operating costs ($)
        /// </summary>
        public decimal OperatingCosts { get; set; }

        /// <summary>
        /// Capital costs ($)
        /// </summary>
        public decimal CapitalCosts { get; set; }

        /// <summary>
        /// Net cash flow ($)
        /// </summary>
        public decimal NetCashFlow { get; set; }

        /// <summary>
        /// Cumulative cash flow ($)
        /// </summary>
        public decimal CumulativeCashFlow { get; set; }
    }
}




