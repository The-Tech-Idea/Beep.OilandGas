using System.ComponentModel.DataAnnotations;

using Beep.OilandGas.Models.Data;
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
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Field identifier - optional if WellUWI is provided
        /// </summary>
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }

        /// <summary>
        /// Forecast method (e.g., "DCA", "ARPS", "HYP")
        /// </summary>
        private string ForecastMethodValue = string.Empty;

        [Required(ErrorMessage = "ForecastMethod is required")]
        public string ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        }

        /// <summary>
        /// Forecast period in months
        /// </summary>
        private int ForecastPeriodValue;

        [Required]
        [Range(1, 600, ErrorMessage = "ForecastPeriod must be between 1 and 600 months")]
        public int ForecastPeriod

        {

            get { return this.ForecastPeriodValue; }

            set { SetProperty(ref ForecastPeriodValue, value); }

        }
        /// <summary>
        /// Variable operating cost per barrel (USD/STB)
        /// </summary>
        private decimal OperatingCostPerBarrelValue = 10m;

        public decimal OperatingCostPerBarrel

        {

            get { return this.OperatingCostPerBarrelValue; }

            set { SetProperty(ref OperatingCostPerBarrelValue, value); }

        }

        /// <summary>
        /// Fixed OPEX per period (USD)
        /// </summary>
        private decimal FixedOpexPerPeriodValue = 0m;

        public decimal FixedOpexPerPeriod

        {

            get { return this.FixedOpexPerPeriodValue; }

            set { SetProperty(ref FixedOpexPerPeriodValue, value); }

        }

        /// <summary>
        /// Optional capital schedule (date, amount) to apply per period
        /// </summary>
        private List<CapitalScheduleItem> CapitalScheduleValue = new();

        public List<CapitalScheduleItem> CapitalSchedule

        {

            get { return this.CapitalScheduleValue; }

            set { SetProperty(ref CapitalScheduleValue, value); }

        }
    }

    /// <summary>
    /// Request for decline curve analysis
    /// </summary>
    public class DeclineCurveAnalysisRequest : ModelEntityBase
    {
        /// <summary>
        /// Well UWI (Unique Well Identifier)
        /// </summary>
        private string WellUWIValue = string.Empty;

        [Required(ErrorMessage = "WellUWI is required")]
        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Start date for analysis period
        /// </summary>
        private DateTime StartDateValue;

        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }

        /// <summary>
        /// End date for analysis period
        /// </summary>
        private DateTime EndDateValue;

        [Required(ErrorMessage = "EndDate is required")]
        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
    }

    /// <summary>
    /// Production forecast point DTO
    /// </summary>
    public class ProductionForecastPoint : ModelEntityBase
    {
        /// <summary>
        /// Forecast date
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Oil production rate (STB/day)
        /// </summary>
        private decimal? OilRateValue;

        public decimal? OilRate

        {

            get { return this.OilRateValue; }

            set { SetProperty(ref OilRateValue, value); }

        }

        /// <summary>
        /// Gas production rate (MSCF/day)
        /// </summary>
        private decimal? GasRateValue;

        public decimal? GasRate

        {

            get { return this.GasRateValue; }

            set { SetProperty(ref GasRateValue, value); }

        }

        /// <summary>
        /// Water production rate (STB/day)
        /// </summary>
        private decimal? WaterRateValue;

        public decimal? WaterRate

        {

            get { return this.WaterRateValue; }

            set { SetProperty(ref WaterRateValue, value); }

        }

        /// <summary>
        /// Cumulative oil production (STB)
        /// </summary>
        private decimal? CumulativeOilValue;

        public decimal? CumulativeOil

        {

            get { return this.CumulativeOilValue; }

            set { SetProperty(ref CumulativeOilValue, value); }

        }

        /// <summary>
        /// Cumulative gas production (MSCF)
        /// </summary>
        private decimal? CumulativeGasValue;

        public decimal? CumulativeGas

        {

            get { return this.CumulativeGasValue; }

            set { SetProperty(ref CumulativeGasValue, value); }

        }

        /// <summary>
        /// Confidence level (for probabilistic forecasts)
        /// </summary>
        private decimal? ConfidenceLevelValue;

        public decimal? ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }

        /// <summary>
        /// P10 rate (low case)
        /// </summary>
        private decimal? P10RateValue;

        public decimal? P10Rate

        {

            get { return this.P10RateValue; }

            set { SetProperty(ref P10RateValue, value); }

        }

        /// <summary>
        /// P50 rate (most likely case)
        /// </summary>
        private decimal? P50RateValue;

        public decimal? P50Rate

        {

            get { return this.P50RateValue; }

            set { SetProperty(ref P50RateValue, value); }

        }

        /// <summary>
        /// P90 rate (high case)
        /// </summary>
        private decimal? P90RateValue;

        public decimal? P90Rate

        {

            get { return this.P90RateValue; }

            set { SetProperty(ref P90RateValue, value); }

        }
    }

    /// <summary>
    /// Production forecast result DTO
    /// </summary>
    public class ProductionForecastResult : ModelEntityBase
    {
        /// <summary>
        /// Forecast ID
        /// </summary>
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Field ID
        /// </summary>
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }

        /// <summary>
        /// Forecast creation date
        /// </summary>
        private DateTime ForecastDateValue;

        public DateTime ForecastDate

        {

            get { return this.ForecastDateValue; }

            set { SetProperty(ref ForecastDateValue, value); }

        }

        /// <summary>
        /// Forecast method used
        /// </summary>
        private string ForecastMethodValue = string.Empty;

        public string ForecastMethod

        {

            get { return this.ForecastMethodValue; }

            set { SetProperty(ref ForecastMethodValue, value); }

        }

        /// <summary>
        /// Forecast points
        /// </summary>
        private List<ProductionForecastPoint> ForecastPointsValue = new();

        public List<ProductionForecastPoint> ForecastPoints

        {

            get { return this.ForecastPointsValue; }

            set { SetProperty(ref ForecastPointsValue, value); }

        }

        /// <summary>
        /// Estimated reserves (STB or MSCF)
        /// </summary>
        private decimal EstimatedReservesValue;

        public decimal EstimatedReserves

        {

            get { return this.EstimatedReservesValue; }

            set { SetProperty(ref EstimatedReservesValue, value); }

        }

        /// <summary>
        /// Forecast status
        /// </summary>
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// R-squared for curve fit quality
        /// </summary>
        private decimal? RSquaredValue;

        public decimal? RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private decimal? RMSEValue;

        public decimal? RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }

        /// <summary>
        /// Forecast confidence interval (%)
        /// </summary>
        private decimal? ConfidenceIntervalValue;

        public decimal? ConfidenceInterval

        {

            get { return this.ConfidenceIntervalValue; }

            set { SetProperty(ref ConfidenceIntervalValue, value); }

        }

        /// <summary>
        /// Economic limit rate (STB/day)
        /// </summary>
        private decimal? EconomicLimitValue;

        public decimal? EconomicLimit

        {

            get { return this.EconomicLimitValue; }

            set { SetProperty(ref EconomicLimitValue, value); }

        }

        /// <summary>
        /// Forecast notes/comments
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }
    }

    /// <summary>
    /// Decline curve analysis result DTO
    /// </summary>
    public class DeclineCurveAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Analysis ID
        /// </summary>
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Analysis date
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Decline type (Exponential, Hyperbolic, Harmonic)
        /// </summary>
        private string DeclineTypeValue = string.Empty;

        public string DeclineType

        {

            get { return this.DeclineTypeValue; }

            set { SetProperty(ref DeclineTypeValue, value); }

        }

        /// <summary>
        /// Initial decline rate (1/time)
        /// </summary>
        private decimal DeclineRateValue;

        public decimal DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }

        /// <summary>
        /// Hyperbolic b-factor (for hyperbolic decline)
        /// </summary>
        private decimal? BFactorValue;

        public decimal? BFactor

        {

            get { return this.BFactorValue; }

            set { SetProperty(ref BFactorValue, value); }

        }

        /// <summary>
        /// Initial production rate (STB/day or MSCF/day)
        /// </summary>
        private decimal InitialRateValue;

        public decimal InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }

        /// <summary>
        /// Estimated ultimate recovery (EUR)
        /// </summary>
        private decimal EstimatedReservesValue;

        public decimal EstimatedReserves

        {

            get { return this.EstimatedReservesValue; }

            set { SetProperty(ref EstimatedReservesValue, value); }

        }

        /// <summary>
        /// R-squared for curve fit quality
        /// </summary>
        private decimal? RSquaredValue;

        public decimal? RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private decimal? RMSEValue;

        public decimal? RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }

        /// <summary>
        /// Mean absolute error
        /// </summary>
        private decimal? MAEValue;

        public decimal? MAE

        {

            get { return this.MAEValue; }

            set { SetProperty(ref MAEValue, value); }

        }

        /// <summary>
        /// Akaike information criterion
        /// </summary>
        private decimal? AICValue;

        public decimal? AIC

        {

            get { return this.AICValue; }

            set { SetProperty(ref AICValue, value); }

        }

        /// <summary>
        /// Bayesian information criterion
        /// </summary>
        private decimal? BICValue;

        public decimal? BIC

        {

            get { return this.BICValue; }

            set { SetProperty(ref BICValue, value); }

        }

        /// <summary>
        /// Analysis status
        /// </summary>
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        /// <summary>
        /// Analysis notes/comments
        /// </summary>
        private string? NotesValue;

        public string? Notes

        {

            get { return this.NotesValue; }

            set { SetProperty(ref NotesValue, value); }

        }

        /// <summary>
        /// Historical production data points used in analysis
        /// </summary>
        private List<ForecastProductionDataPoint> HistoricalDataValue = new();

        public List<ForecastProductionDataPoint> HistoricalData

        {

            get { return this.HistoricalDataValue; }

            set { SetProperty(ref HistoricalDataValue, value); }

        }
    }

    /// <summary>
    /// Production data point for analysis
    /// </summary>
    public class ForecastProductionDataPoint : ModelEntityBase
    {
        /// <summary>
        /// Production date
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Time since start (days)
        /// </summary>
        private decimal TimeValue;

        public decimal Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        }

        /// <summary>
        /// Oil rate (STB/day)
        /// </summary>
        private decimal? OilRateValue;

        public decimal? OilRate

        {

            get { return this.OilRateValue; }

            set { SetProperty(ref OilRateValue, value); }

        }

        /// <summary>
        /// Gas rate (MSCF/day)
        /// </summary>
        private decimal? GasRateValue;

        public decimal? GasRate

        {

            get { return this.GasRateValue; }

            set { SetProperty(ref GasRateValue, value); }

        }

        /// <summary>
        /// Water rate (STB/day)
        /// </summary>
        private decimal? WaterRateValue;

        public decimal? WaterRate

        {

            get { return this.WaterRateValue; }

            set { SetProperty(ref WaterRateValue, value); }

        }

        /// <summary>
        /// Cumulative production
        /// </summary>
        private decimal? CumulativeValue;

        public decimal? Cumulative

        {

            get { return this.CumulativeValue; }

            set { SetProperty(ref CumulativeValue, value); }

        }
    }

    /// <summary>
    /// Probabilistic forecast result DTO
    /// </summary>
    public class ProbabilisticForecast : ModelEntityBase
    {
        /// <summary>
        /// Forecast ID
        /// </summary>
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }

        /// <summary>
        /// Well UWI
        /// </summary>
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }

        /// <summary>
        /// Field ID
        /// </summary>
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }

        /// <summary>
        /// P10 case forecast (low case)
        /// </summary>
        private ProductionForecastResult P10ForecastValue = new();

        public ProductionForecastResult P10Forecast

        {

            get { return this.P10ForecastValue; }

            set { SetProperty(ref P10ForecastValue, value); }

        }

        /// <summary>
        /// P50 case forecast (most likely)
        /// </summary>
        private ProductionForecastResult P50ForecastValue = new();

        public ProductionForecastResult P50Forecast

        {

            get { return this.P50ForecastValue; }

            set { SetProperty(ref P50ForecastValue, value); }

        }

        /// <summary>
        /// P90 case forecast (high case)
        /// </summary>
        private ProductionForecastResult P90ForecastValue = new();

        public ProductionForecastResult P90Forecast

        {

            get { return this.P90ForecastValue; }

            set { SetProperty(ref P90ForecastValue, value); }

        }

        /// <summary>
        /// Expected value forecast
        /// </summary>
        private ProductionForecastResult ExpectedForecastValue = new();

        public ProductionForecastResult ExpectedForecast

        {

            get { return this.ExpectedForecastValue; }

            set { SetProperty(ref ExpectedForecastValue, value); }

        }

        /// <summary>
        /// Risk analysis results
        /// </summary>
        private ForecastRiskAnalysisResult RiskAnalysisValue = new();

        public ForecastRiskAnalysisResult RiskAnalysis

        {

            get { return this.RiskAnalysisValue; }

            set { SetProperty(ref RiskAnalysisValue, value); }

        }
    }

    /// <summary>
    /// Risk analysis result DTO
    /// </summary>
    public class ForecastRiskAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Risk analysis ID
        /// </summary>
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Probability of commercial success (%)
        /// </summary>
        private decimal CommercialSuccessProbabilityValue;

        public decimal CommercialSuccessProbability

        {

            get { return this.CommercialSuccessProbabilityValue; }

            set { SetProperty(ref CommercialSuccessProbabilityValue, value); }

        }

        /// <summary>
        /// Risk factors identified
        /// </summary>
        private List<RiskFactor> RiskFactorsValue = new();

        public List<RiskFactor> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        }

        /// <summary>
        /// Mitigation strategies
        /// </summary>
        private List<string> MitigationStrategiesValue = new();

        public List<string> MitigationStrategies

        {

            get { return this.MitigationStrategiesValue; }

            set { SetProperty(ref MitigationStrategiesValue, value); }

        }

        /// <summary>
        /// Overall risk rating
        /// </summary>
        private string RiskRatingValue = string.Empty;

        public string RiskRating

        {

            get { return this.RiskRatingValue; }

            set { SetProperty(ref RiskRatingValue, value); }

        }
    }

    /// <summary>
    /// Risk factor DTO
    /// </summary>
    public class RiskFactor : ModelEntityBase
    {
        /// <summary>
        /// Risk factor description
        /// </summary>
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }

        /// <summary>
        /// Risk probability (%)
        /// </summary>
        private decimal ProbabilityValue;

        public decimal Probability

        {

            get { return this.ProbabilityValue; }

            set { SetProperty(ref ProbabilityValue, value); }

        }

        /// <summary>
        /// Risk impact (High, Medium, Low)
        /// </summary>
        private string ImpactValue = string.Empty;

        public string Impact

        {

            get { return this.ImpactValue; }

            set { SetProperty(ref ImpactValue, value); }

        }

        /// <summary>
        /// Risk category
        /// </summary>
        private string CategoryValue = string.Empty;

        public string Category

        {

            get { return this.CategoryValue; }

            set { SetProperty(ref CategoryValue, value); }

        }
    }

    /// <summary>
    /// Economic analysis result DTO
    /// </summary>
    public class EconomicAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Analysis ID
        /// </summary>
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }

        /// <summary>
        /// Net present value (NPV)
        /// </summary>
        private decimal NPVValue;

        public decimal NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }

        /// <summary>
        /// Internal rate of return (IRR) (%)
        /// </summary>
        private decimal IRRValue;

        public decimal IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }

        /// <summary>
        /// Profitability index
        /// </summary>
        private decimal ProfitabilityIndexValue;

        public decimal ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }

        /// <summary>
        /// Payback period (years)
        /// </summary>
        private decimal PaybackPeriodValue;

        public decimal PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }

        /// <summary>
        /// Break-even price ($/unit)
        /// </summary>
        private decimal BreakEvenPriceValue;

        public decimal BreakEvenPrice

        {

            get { return this.BreakEvenPriceValue; }

            set { SetProperty(ref BreakEvenPriceValue, value); }

        }

        /// <summary>
        /// Discount rate used (%)
        /// </summary>
        private decimal DiscountRateValue;

        public decimal DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }

        /// <summary>
        /// Economic limit (STB/day)
        /// </summary>
        private decimal EconomicLimitValue;

        public decimal EconomicLimit

        {

            get { return this.EconomicLimitValue; }

            set { SetProperty(ref EconomicLimitValue, value); }

        }

        /// <summary>
        /// Cash flow projections
        /// </summary>
        private List<CashFlowPoint> CashFlowsValue = new();

        public List<CashFlowPoint> CashFlows

        {

            get { return this.CashFlowsValue; }

            set { SetProperty(ref CashFlowsValue, value); }

        }
    }



    /// <summary>
    /// Request for performing economic analysis
    /// </summary>
    public class ForecastEconomicAnalysisRequest : ModelEntityBase
    {
        private string ForecastIdValue = string.Empty;

        public string ForecastId

        {

            get { return this.ForecastIdValue; }

            set { SetProperty(ref ForecastIdValue, value); }

        }
        private decimal OilPriceValue;

        public decimal OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal GasPriceValue;

        public decimal GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private decimal DiscountRateValue;

        public decimal DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }

        private decimal OperatingCostPerBarrelValue = 10m;


        public decimal OperatingCostPerBarrel


        {


            get { return this.OperatingCostPerBarrelValue; }


            set { SetProperty(ref OperatingCostPerBarrelValue, value); }


        }
        private decimal FixedOpexPerPeriodValue = 0m;

        public decimal FixedOpexPerPeriod

        {

            get { return this.FixedOpexPerPeriodValue; }

            set { SetProperty(ref FixedOpexPerPeriodValue, value); }

        }
        private List<CapitalScheduleItem> CapitalScheduleValue = new();

        public List<CapitalScheduleItem> CapitalSchedule

        {

            get { return this.CapitalScheduleValue; }

            set { SetProperty(ref CapitalScheduleValue, value); }

        }
    }

    /// <summary>
    /// Cash flow point DTO
    /// </summary>
    public class CashFlowPoint : ModelEntityBase
    {
        /// <summary>
        /// Period date
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Revenue ($)
        /// </summary>
        private decimal RevenueValue;

        public decimal Revenue

        {

            get { return this.RevenueValue; }

            set { SetProperty(ref RevenueValue, value); }

        }

        /// <summary>
        /// Operating costs ($)
        /// </summary>
        private decimal OperatingCostsValue;

        public decimal OperatingCosts

        {

            get { return this.OperatingCostsValue; }

            set { SetProperty(ref OperatingCostsValue, value); }

        }

        /// <summary>
        /// Capital costs ($)
        /// </summary>
        private decimal CapitalCostsValue;

        public decimal CapitalCosts

        {

            get { return this.CapitalCostsValue; }

            set { SetProperty(ref CapitalCostsValue, value); }

        }

        /// <summary>
        /// Net cash flow ($)
        /// </summary>
        private decimal NetCashFlowValue;

        public decimal NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }

        /// <summary>
        /// Cumulative cash flow ($)
        /// </summary>
        private decimal CumulativeCashFlowValue;

        public decimal CumulativeCashFlow

        {

            get { return this.CumulativeCashFlowValue; }

            set { SetProperty(ref CumulativeCashFlowValue, value); }

        }
    }
}







