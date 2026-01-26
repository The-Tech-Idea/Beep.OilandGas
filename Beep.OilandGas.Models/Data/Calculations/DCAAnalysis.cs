using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Comprehensive set of DTOs for Decline Curve Analysis (DCA) advanced operations.
    /// Supports sensitivity analysis, Monte Carlo forecasting, model comparison, and portfolio analysis.
    /// </summary>

    /// <summary>
    /// DTO for sensitivity analysis results showing how DCA parameters affect forecasts.
    /// </summary>
    public class DcaSensitivityAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Base parameters used for analysis [qi, di, b]
        /// </summary>
        private double[] BaseParametersValue;

        public double[] BaseParameters

        {

            get { return this.BaseParametersValue; }

            set { SetProperty(ref BaseParametersValue, value); }

        }

        /// <summary>
        /// Percentage variation applied to parameters (e.g., 20 for ±20%)
        /// </summary>
        private double VariationPercentValue;

        public double VariationPercent

        {

            get { return this.VariationPercentValue; }

            set { SetProperty(ref VariationPercentValue, value); }

        }

        /// <summary>
        /// Sensitivity results for initial production rate (qi)
        /// </summary>
        private DcaParameterSensitivity QiSensitivityValue;

        public DcaParameterSensitivity QiSensitivity

        {

            get { return this.QiSensitivityValue; }

            set { SetProperty(ref QiSensitivityValue, value); }

        }

        /// <summary>
        /// Sensitivity results for initial decline rate (di)
        /// </summary>
        private DcaParameterSensitivity DiSensitivityValue;

        public DcaParameterSensitivity DiSensitivity

        {

            get { return this.DiSensitivityValue; }

            set { SetProperty(ref DiSensitivityValue, value); }

        }

        /// <summary>
        /// Sensitivity results for decline exponent (b)
        /// </summary>
        private DcaParameterSensitivity BSensitivityValue;

        public DcaParameterSensitivity BSensitivity

        {

            get { return this.BSensitivityValue; }

            set { SetProperty(ref BSensitivityValue, value); }

        }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
    }

    /// <summary>
    /// DTO representing sensitivity of a single parameter
    /// </summary>
    public class DcaParameterSensitivity : ModelEntityBase
    {
        /// <summary>
        /// Production at baseline parameter value
        /// </summary>
        private double BaselineProductionValue;

        public double BaselineProduction

        {

            get { return this.BaselineProductionValue; }

            set { SetProperty(ref BaselineProductionValue, value); }

        }

        /// <summary>
        /// Production with low variation (-X%)
        /// </summary>
        private double LowVariationProductionValue;

        public double LowVariationProduction

        {

            get { return this.LowVariationProductionValue; }

            set { SetProperty(ref LowVariationProductionValue, value); }

        }

        /// <summary>
        /// Production with high variation (+X%)
        /// </summary>
        private double HighVariationProductionValue;

        public double HighVariationProduction

        {

            get { return this.HighVariationProductionValue; }

            set { SetProperty(ref HighVariationProductionValue, value); }

        }

        /// <summary>
        /// Total impact on final production as percentage
        /// </summary>
        private double ImpactOnFinalProductionValue;

        public double ImpactOnFinalProduction

        {

            get { return this.ImpactOnFinalProductionValue; }

            set { SetProperty(ref ImpactOnFinalProductionValue, value); }

        }
    }

    /// <summary>
    /// DTO for multiple decline models comparison
    /// </summary>
    public class DcaMultipleModelsComparisonResult : ModelEntityBase
    {
        /// <summary>
        /// Number of data points analyzed
        /// </summary>
        private int DataPointsAnalyzedValue;

        public int DataPointsAnalyzed

        {

            get { return this.DataPointsAnalyzedValue; }

            set { SetProperty(ref DataPointsAnalyzedValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Exponential decline model results
        /// </summary>
        private DcaDeclineModel ExponentialModelValue;

        public DcaDeclineModel ExponentialModel

        {

            get { return this.ExponentialModelValue; }

            set { SetProperty(ref ExponentialModelValue, value); }

        }

        /// <summary>
        /// Hyperbolic decline model results
        /// </summary>
        private DcaDeclineModel HyperbolicModelValue;

        public DcaDeclineModel HyperbolicModel

        {

            get { return this.HyperbolicModelValue; }

            set { SetProperty(ref HyperbolicModelValue, value); }

        }

        /// <summary>
        /// Harmonic decline model results
        /// </summary>
        private DcaDeclineModel HarmonicModelValue;

        public DcaDeclineModel HarmonicModel

        {

            get { return this.HarmonicModelValue; }

            set { SetProperty(ref HarmonicModelValue, value); }

        }

        /// <summary>
        /// Name of the best fitting model
        /// </summary>
        private string BestFitModelValue;

        public string BestFitModel

        {

            get { return this.BestFitModelValue; }

            set { SetProperty(ref BestFitModelValue, value); }

        }

        /// <summary>
        /// R² value of the best fitting model
        /// </summary>
        private double BestRSquaredValue;

        public double BestRSquared

        {

            get { return this.BestRSquaredValue; }

            set { SetProperty(ref BestRSquaredValue, value); }

        }
    }

    /// <summary>
    /// DTO representing a single decline curve model
    /// </summary>
    public class DcaDeclineModel : ModelEntityBase
    {
        /// <summary>
        /// Type of decline model (Exponential, Hyperbolic, Harmonic)
        /// </summary>
        private string ModelTypeValue;

        public string ModelType

        {

            get { return this.ModelTypeValue; }

            set { SetProperty(ref ModelTypeValue, value); }

        }

        /// <summary>
        /// Fitted parameters [qi, di, b]
        /// </summary>
        private double[] ParametersValue;

        public double[] Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }

        /// <summary>
        /// Coefficient of determination (R²)
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Akaike Information Criterion
        /// </summary>
        private double AICValue;

        public double AIC

        {

            get { return this.AICValue; }

            set { SetProperty(ref AICValue, value); }

        }

        /// <summary>
        /// Bayesian Information Criterion
        /// </summary>
        private double BICValue;

        public double BIC

        {

            get { return this.BICValue; }

            set { SetProperty(ref BICValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private double RMSEValue;

        public double RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }
    }

    /// <summary>
    /// DTO for Monte Carlo probabilistic forecasting results
    /// </summary>
    public class DcaMonteCarloForecastResult : ModelEntityBase
    {
        /// <summary>
        /// Number of simulations performed
        /// </summary>
        private int SimulationCountValue;

        public int SimulationCount

        {

            get { return this.SimulationCountValue; }

            set { SetProperty(ref SimulationCountValue, value); }

        }

        /// <summary>
        /// Number of months forecasted
        /// </summary>
        private int ForecastMonthsValue;

        public int ForecastMonths

        {

            get { return this.ForecastMonthsValue; }

            set { SetProperty(ref ForecastMonthsValue, value); }

        }

        /// <summary>
        /// Confidence level used (0.0-1.0)
        /// </summary>
        private double ConfidenceLevelValue;

        public double ConfidenceLevel

        {

            get { return this.ConfidenceLevelValue; }

            set { SetProperty(ref ConfidenceLevelValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Monthly forecast statistics for each forecast month
        /// </summary>
        private List<DcaMonteCarloMonthlyStats> ForecastMonthlyValue = new();

        public List<DcaMonteCarloMonthlyStats> ForecastMonthly

        {

            get { return this.ForecastMonthlyValue; }

            set { SetProperty(ref ForecastMonthlyValue, value); }

        }

        /// <summary>
        /// Total cumulative production across all months
        /// </summary>
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }

        /// <summary>
        /// Probability well remains above economic limit
        /// </summary>
        private double ProbabilityEconomicViableValue;

        public double ProbabilityEconomicViable

        {

            get { return this.ProbabilityEconomicViableValue; }

            set { SetProperty(ref ProbabilityEconomicViableValue, value); }

        }
    }

    /// <summary>
    /// DTO for monthly statistics from Monte Carlo simulation
    /// </summary>
    public class DcaMonteCarloMonthlyStats : ModelEntityBase
    {
        /// <summary>
        /// Forecast month number (1-N)
        /// </summary>
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }

        /// <summary>
        /// Mean production rate (bbl/day)
        /// </summary>
        private double MeanProductionValue;

        public double MeanProduction

        {

            get { return this.MeanProductionValue; }

            set { SetProperty(ref MeanProductionValue, value); }

        }

        /// <summary>
        /// Median production rate (bbl/day)
        /// </summary>
        private double MedianProductionValue;

        public double MedianProduction

        {

            get { return this.MedianProductionValue; }

            set { SetProperty(ref MedianProductionValue, value); }

        }

        /// <summary>
        /// 10th percentile production rate
        /// </summary>
        private double P10ProductionValue;

        public double P10Production

        {

            get { return this.P10ProductionValue; }

            set { SetProperty(ref P10ProductionValue, value); }

        }

        /// <summary>
        /// 50th percentile production rate
        /// </summary>
        private double P50ProductionValue;

        public double P50Production

        {

            get { return this.P50ProductionValue; }

            set { SetProperty(ref P50ProductionValue, value); }

        }

        /// <summary>
        /// 90th percentile production rate
        /// </summary>
        private double P90ProductionValue;

        public double P90Production

        {

            get { return this.P90ProductionValue; }

            set { SetProperty(ref P90ProductionValue, value); }

        }

        /// <summary>
        /// Minimum production in simulations
        /// </summary>
        private double MinProductionValue;

        public double MinProduction

        {

            get { return this.MinProductionValue; }

            set { SetProperty(ref MinProductionValue, value); }

        }

        /// <summary>
        /// Maximum production in simulations
        /// </summary>
        private double MaxProductionValue;

        public double MaxProduction

        {

            get { return this.MaxProductionValue; }

            set { SetProperty(ref MaxProductionValue, value); }

        }

        /// <summary>
        /// Standard deviation of production
        /// </summary>
        private double StandardDeviationValue;

        public double StandardDeviation

        {

            get { return this.StandardDeviationValue; }

            set { SetProperty(ref StandardDeviationValue, value); }

        }
    }

    /// <summary>
    /// DTO for model comparison metrics and ranking
    /// </summary>
    public class DcaModelComparisonMetricsResult : ModelEntityBase
    {
        /// <summary>
        /// Number of models compared
        /// </summary>
        private int ModelsComparedValue;

        public int ModelsCompared

        {

            get { return this.ModelsComparedValue; }

            set { SetProperty(ref ModelsComparedValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Metrics for each model
        /// </summary>
        private List<DcaModelMetric> ModelMetricsValue = new();

        public List<DcaModelMetric> ModelMetrics

        {

            get { return this.ModelMetricsValue; }

            set { SetProperty(ref ModelMetricsValue, value); }

        }

        /// <summary>
        /// Index of model with best AIC
        /// </summary>
        private int BestAICIndexValue;

        public int BestAICIndex

        {

            get { return this.BestAICIndexValue; }

            set { SetProperty(ref BestAICIndexValue, value); }

        }

        /// <summary>
        /// Index of model with best R²
        /// </summary>
        private int BestRSquaredIndexValue;

        public int BestRSquaredIndex

        {

            get { return this.BestRSquaredIndexValue; }

            set { SetProperty(ref BestRSquaredIndexValue, value); }

        }

        /// <summary>
        /// Index of model with best BIC
        /// </summary>
        private int BestBICIndexValue;

        public int BestBICIndex

        {

            get { return this.BestBICIndexValue; }

            set { SetProperty(ref BestBICIndexValue, value); }

        }
    }

    /// <summary>
    /// DTO for individual model metrics
    /// </summary>
    public class DcaModelMetric : ModelEntityBase
    {
        /// <summary>
        /// Model index in comparison
        /// </summary>
        private int ModelIndexValue;

        public int ModelIndex

        {

            get { return this.ModelIndexValue; }

            set { SetProperty(ref ModelIndexValue, value); }

        }

        /// <summary>
        /// R² coefficient of determination
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Adjusted R²
        /// </summary>
        private double AdjustedRSquaredValue;

        public double AdjustedRSquared

        {

            get { return this.AdjustedRSquaredValue; }

            set { SetProperty(ref AdjustedRSquaredValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private double RMSEValue;

        public double RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }

        /// <summary>
        /// Mean absolute error
        /// </summary>
        private double MAEValue;

        public double MAE

        {

            get { return this.MAEValue; }

            set { SetProperty(ref MAEValue, value); }

        }

        /// <summary>
        /// Akaike Information Criterion
        /// </summary>
        private double AICValue;

        public double AIC

        {

            get { return this.AICValue; }

            set { SetProperty(ref AICValue, value); }

        }

        /// <summary>
        /// Bayesian Information Criterion
        /// </summary>
        private double BICValue;

        public double BIC

        {

            get { return this.BICValue; }

            set { SetProperty(ref BICValue, value); }

        }

        /// <summary>
        /// Whether model converged
        /// </summary>
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }

        /// <summary>
        /// Number of parameters in model
        /// </summary>
        private int ParameterCountValue;

        public int ParameterCount

        {

            get { return this.ParameterCountValue; }

            set { SetProperty(ref ParameterCountValue, value); }

        }

        /// <summary>
        /// Overall performance score (0-100)
        /// </summary>
        private double PerformanceScoreValue;

        public double PerformanceScore

        {

            get { return this.PerformanceScoreValue; }

            set { SetProperty(ref PerformanceScoreValue, value); }

        }
    }

    /// <summary>
    /// DTO for production trend analysis
    /// </summary>
    public class DcaProductionTrendAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Number of data points analyzed
        /// </summary>
        private int DataPointsAnalyzedValue;

        public int DataPointsAnalyzed

        {

            get { return this.DataPointsAnalyzedValue; }

            set { SetProperty(ref DataPointsAnalyzedValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Decline rates by interval
        /// </summary>
        private List<double> DeclineRatesByIntervalValue = new();

        public List<double> DeclineRatesByInterval

        {

            get { return this.DeclineRatesByIntervalValue; }

            set { SetProperty(ref DeclineRatesByIntervalValue, value); }

        }

        /// <summary>
        /// Identified inflection points
        /// </summary>
        private List<DcaInflectionPoint> InflectionPointsValue = new();

        public List<DcaInflectionPoint> InflectionPoints

        {

            get { return this.InflectionPointsValue; }

            set { SetProperty(ref InflectionPointsValue, value); }

        }

        /// <summary>
        /// Detected phase transitions
        /// </summary>
        private List<DcaPhaseTransition> PhaseTransitionsValue = new();

        public List<DcaPhaseTransition> PhaseTransitions

        {

            get { return this.PhaseTransitionsValue; }

            set { SetProperty(ref PhaseTransitionsValue, value); }

        }

        /// <summary>
        /// Average decline rate in early phase
        /// </summary>
        private double EarlyPhaseDeclineValue;

        public double EarlyPhaseDecline

        {

            get { return this.EarlyPhaseDeclineValue; }

            set { SetProperty(ref EarlyPhaseDeclineValue, value); }

        }

        /// <summary>
        /// Average decline rate in main phase
        /// </summary>
        private double MainPhaseDeclineValue;

        public double MainPhaseDecline

        {

            get { return this.MainPhaseDeclineValue; }

            set { SetProperty(ref MainPhaseDeclineValue, value); }

        }
    }

    /// <summary>
    /// DTO for inflection point identification
    /// </summary>
    public class DcaInflectionPoint : ModelEntityBase
    {
        /// <summary>
        /// Month number of inflection
        /// </summary>
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }

        /// <summary>
        /// Date of inflection
        /// </summary>
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }

        /// <summary>
        /// Production rate at inflection
        /// </summary>
        private double ProductionValue;

        public double Production

        {

            get { return this.ProductionValue; }

            set { SetProperty(ref ProductionValue, value); }

        }
    }

    /// <summary>
    /// DTO for phase transition detection
    /// </summary>
    public class DcaPhaseTransition : ModelEntityBase
    {
        /// <summary>
        /// Month of transition
        /// </summary>
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }

        /// <summary>
        /// Change in decline rate
        /// </summary>
        private double DeclineRateChangeValue;

        public double DeclineRateChange

        {

            get { return this.DeclineRateChangeValue; }

            set { SetProperty(ref DeclineRateChangeValue, value); }

        }
    }

    /// <summary>
    /// DTO for long-term production forecasting
    /// </summary>
    public class DcaLongTermForecastResult : ModelEntityBase
    {
        /// <summary>
        /// Number of years in forecast
        /// </summary>
        private int ForecastYearsValue;

        public int ForecastYears

        {

            get { return this.ForecastYearsValue; }

            set { SetProperty(ref ForecastYearsValue, value); }

        }

        /// <summary>
        /// Economic limit threshold (bbl/day)
        /// </summary>
        private double EconomicLimitBblPerDayValue;

        public double EconomicLimitBblPerDay

        {

            get { return this.EconomicLimitBblPerDayValue; }

            set { SetProperty(ref EconomicLimitBblPerDayValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Monthly production forecast
        /// </summary>
        private List<double> MonthlyProductionValue = new();

        public List<double> MonthlyProduction

        {

            get { return this.MonthlyProductionValue; }

            set { SetProperty(ref MonthlyProductionValue, value); }

        }

        /// <summary>
        /// Total cumulative production
        /// </summary>
        private double TotalCumulativeProductionValue;

        public double TotalCumulativeProduction

        {

            get { return this.TotalCumulativeProductionValue; }

            set { SetProperty(ref TotalCumulativeProductionValue, value); }

        }

        /// <summary>
        /// Average production rate
        /// </summary>
        private double AverageProductionRateValue;

        public double AverageProductionRate

        {

            get { return this.AverageProductionRateValue; }

            set { SetProperty(ref AverageProductionRateValue, value); }

        }

        /// <summary>
        /// Months until economic limit reached
        /// </summary>
        private int MonthsToEconomicLimitValue;

        public int MonthsToEconomicLimit

        {

            get { return this.MonthsToEconomicLimitValue; }

            set { SetProperty(ref MonthsToEconomicLimitValue, value); }

        }

        /// <summary>
        /// Years until economic limit reached
        /// </summary>
        private double YearsToEconomicLimitValue;

        public double YearsToEconomicLimit

        {

            get { return this.YearsToEconomicLimitValue; }

            set { SetProperty(ref YearsToEconomicLimitValue, value); }

        }
    }

    /// <summary>
    /// DTO for end-of-life prediction
    /// </summary>
    public class DcaEndOfLifePredictionResult : ModelEntityBase
    {
        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Reference date for analysis
        /// </summary>
        private DateTime ReferenceDateValue;

        public DateTime ReferenceDate

        {

            get { return this.ReferenceDateValue; }

            set { SetProperty(ref ReferenceDateValue, value); }

        }

        /// <summary>
        /// Predicted end-of-life date
        /// </summary>
        private DateTime PredictedEOLDateValue;

        public DateTime PredictedEOLDate

        {

            get { return this.PredictedEOLDateValue; }

            set { SetProperty(ref PredictedEOLDateValue, value); }

        }

        /// <summary>
        /// Economic limit threshold (bbl/day)
        /// </summary>
        private double EconomicLimitBblPerDayValue;

        public double EconomicLimitBblPerDay

        {

            get { return this.EconomicLimitBblPerDayValue; }

            set { SetProperty(ref EconomicLimitBblPerDayValue, value); }

        }

        /// <summary>
        /// Remaining well life in months
        /// </summary>
        private int RemainingLifeMonthsValue;

        public int RemainingLifeMonths

        {

            get { return this.RemainingLifeMonthsValue; }

            set { SetProperty(ref RemainingLifeMonthsValue, value); }

        }

        /// <summary>
        /// Remaining well life in years
        /// </summary>
        private double RemainingLifeYearsValue;

        public double RemainingLifeYears

        {

            get { return this.RemainingLifeYearsValue; }

            set { SetProperty(ref RemainingLifeYearsValue, value); }

        }

        /// <summary>
        /// Estimated reserves to end-of-life
        /// </summary>
        private double ReservesToEOLValue;

        public double ReservesToEOL

        {

            get { return this.ReservesToEOLValue; }

            set { SetProperty(ref ReservesToEOLValue, value); }

        }
    }

    /// <summary>
    /// DTO for optimized decline parameters
    /// </summary>
    public class DcaOptimizedParametersResult : ModelEntityBase
    {
        /// <summary>
        /// Number of data points used
        /// </summary>
        private int DataPointsUsedValue;

        public int DataPointsUsed

        {

            get { return this.DataPointsUsedValue; }

            set { SetProperty(ref DataPointsUsedValue, value); }

        }

        /// <summary>
        /// Date optimization was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Initial production rate (bbl/day)
        /// </summary>
        private double QiValue;

        public double Qi

        {

            get { return this.QiValue; }

            set { SetProperty(ref QiValue, value); }

        }

        /// <summary>
        /// Initial decline rate (1/year)
        /// </summary>
        private double DiValue;

        public double Di

        {

            get { return this.DiValue; }

            set { SetProperty(ref DiValue, value); }

        }

        /// <summary>
        /// Decline exponent (unitless)
        /// </summary>
        private double BValue;

        public double B

        {

            get { return this.BValue; }

            set { SetProperty(ref BValue, value); }

        }

        /// <summary>
        /// Goodness of fit (R²)
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }

        /// <summary>
        /// Root mean square error
        /// </summary>
        private double RMSEValue;

        public double RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        }
    }

    /// <summary>
    /// DTO for forecast reliability assessment
    /// </summary>
    public class DcaForecastReliabilityResult : ModelEntityBase
    {
        /// <summary>
        /// Date assessment was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Months of historical data available
        /// </summary>
        private int HistoricalMonthsValue;

        public int HistoricalMonths

        {

            get { return this.HistoricalMonthsValue; }

            set { SetProperty(ref HistoricalMonthsValue, value); }

        }

        /// <summary>
        /// R² of the fitted model
        /// </summary>
        private double ModelRSquaredValue;

        public double ModelRSquared

        {

            get { return this.ModelRSquaredValue; }

            set { SetProperty(ref ModelRSquaredValue, value); }

        }

        /// <summary>
        /// Score component from R² (0-10)
        /// </summary>
        private double R2ScoreComponentValue;

        public double R2ScoreComponent

        {

            get { return this.R2ScoreComponentValue; }

            set { SetProperty(ref R2ScoreComponentValue, value); }

        }

        /// <summary>
        /// Score component from historical data span (0-10)
        /// </summary>
        private double HistoryScoreComponentValue;

        public double HistoryScoreComponent

        {

            get { return this.HistoryScoreComponentValue; }

            set { SetProperty(ref HistoryScoreComponentValue, value); }

        }

        /// <summary>
        /// Score component from convergence (0-10)
        /// </summary>
        private double ConvergenceScoreComponentValue;

        public double ConvergenceScoreComponent

        {

            get { return this.ConvergenceScoreComponentValue; }

            set { SetProperty(ref ConvergenceScoreComponentValue, value); }

        }

        /// <summary>
        /// Overall reliability score (0-10)
        /// </summary>
        private double OverallReliabilityScoreValue;

        public double OverallReliabilityScore

        {

            get { return this.OverallReliabilityScoreValue; }

            set { SetProperty(ref OverallReliabilityScoreValue, value); }

        }

        /// <summary>
        /// Reliability assessment (Excellent, Good, Moderate, Poor)
        /// </summary>
        private string ReliabilityAssessmentValue;

        public string ReliabilityAssessment

        {

            get { return this.ReliabilityAssessmentValue; }

            set { SetProperty(ref ReliabilityAssessmentValue, value); }

        }

        /// <summary>
        /// Recommendations for improving forecast
        /// </summary>
        private List<string> RecommendationsValue = new();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        }
    }

    /// <summary>
    /// DTO for multi-well portfolio analysis
    /// </summary>
    public class DcaPortfolioAnalysisResult : ModelEntityBase
    {
        /// <summary>
        /// Number of wells analyzed
        /// </summary>
        private int WellsAnalyzedValue;

        public int WellsAnalyzed

        {

            get { return this.WellsAnalyzedValue; }

            set { SetProperty(ref WellsAnalyzedValue, value); }

        }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }

        /// <summary>
        /// Analysis results for each well
        /// </summary>
        public List<DcaPortfolioWellAnalysis> WellAnalyses { get; set; } = new();

        /// <summary>
        /// Average initial production rate (qi) across portfolio
        /// </summary>
        private double AverageQiValue;

        public double AverageQi

        {

            get { return this.AverageQiValue; }

            set { SetProperty(ref AverageQiValue, value); }

        }

        /// <summary>
        /// Average initial decline rate (di) across portfolio
        /// </summary>
        private double AverageDiValue;

        public double AverageDi

        {

            get { return this.AverageDiValue; }

            set { SetProperty(ref AverageDiValue, value); }

        }

        /// <summary>
        /// Average R² across portfolio models
        /// </summary>
        private double AverageRSquaredValue;

        public double AverageRSquared

        {

            get { return this.AverageRSquaredValue; }

            set { SetProperty(ref AverageRSquaredValue, value); }

        }
    }

    /// <summary>
    /// DTO for individual well analysis in portfolio
    /// </summary>
    public class DcaPortfolioWellAnalysis : ModelEntityBase
    {
        /// <summary>
        /// Well identifier
        /// </summary>
        private string WellIdValue;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }

        /// <summary>
        /// Number of data points for this well
        /// </summary>
        private int DataPointsValue;

        public int DataPoints

        {

            get { return this.DataPointsValue; }

            set { SetProperty(ref DataPointsValue, value); }

        }

        /// <summary>
        /// Initial production rate (bbl/day)
        /// </summary>
        private double InitialProductionValue;

        public double InitialProduction

        {

            get { return this.InitialProductionValue; }

            set { SetProperty(ref InitialProductionValue, value); }

        }

        /// <summary>
        /// Final production rate (bbl/day)
        /// </summary>
        private double FinalProductionValue;

        public double FinalProduction

        {

            get { return this.FinalProductionValue; }

            set { SetProperty(ref FinalProductionValue, value); }

        }

        /// <summary>
        /// Fitted qi parameter
        /// </summary>
        private double QiValue;

        public double Qi

        {

            get { return this.QiValue; }

            set { SetProperty(ref QiValue, value); }

        }

        /// <summary>
        /// Fitted di parameter
        /// </summary>
        private double DiValue;

        public double Di

        {

            get { return this.DiValue; }

            set { SetProperty(ref DiValue, value); }

        }

        /// <summary>
        /// R² of model fit
        /// </summary>
        private double RSquaredValue;

        public double RSquared

        {

            get { return this.RSquaredValue; }

            set { SetProperty(ref RSquaredValue, value); }

        }
    }
    /// <summary>
    /// Request for Decline Curve Analysis calculation
    /// </summary>
    public class DCARequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string CalculationTypeValue = "DCA";

        public string CalculationType

        {

            get { return this.CalculationTypeValue; }

            set { SetProperty(ref CalculationTypeValue, value); }

        } // DCA, DCA_EXPONENTIAL, DCA_HYPERBOLIC, DCA_HARMONIC
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private string? ProductionFluidTypeValue;

        public string? ProductionFluidType

        {

            get { return this.ProductionFluidTypeValue; }

            set { SetProperty(ref ProductionFluidTypeValue, value); }

        } // OIL, GAS, WATER
        public DcaAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of Decline Curve Analysis calculation
    /// </summary>
    public class DCAResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string CalculationTypeValue = string.Empty;

        public string CalculationType

        {

            get { return this.CalculationTypeValue; }

            set { SetProperty(ref CalculationTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private string? ProductionFluidTypeValue;

        public string? ProductionFluidType

        {

            get { return this.ProductionFluidTypeValue; }

            set { SetProperty(ref ProductionFluidTypeValue, value); }

        }
        
        // Decline curve parameters
        private decimal? InitialRateValue;

        public decimal? InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }
        private decimal? DeclineRateValue;

        public decimal? DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }
        private decimal? DeclineConstantValue;

        public decimal? DeclineConstant

        {

            get { return this.DeclineConstantValue; }

            set { SetProperty(ref DeclineConstantValue, value); }

        }
        private decimal? NominalDeclineRateValue;

        public decimal? NominalDeclineRate

        {

            get { return this.NominalDeclineRateValue; }

            set { SetProperty(ref NominalDeclineRateValue, value); }

        }
        private decimal? EffectiveDeclineRateValue;

        public decimal? EffectiveDeclineRate

        {

            get { return this.EffectiveDeclineRateValue; }

            set { SetProperty(ref EffectiveDeclineRateValue, value); }

        }
        private decimal? HyperbolicExponentValue;

        public decimal? HyperbolicExponent

        {

            get { return this.HyperbolicExponentValue; }

            set { SetProperty(ref HyperbolicExponentValue, value); }

        }
        
        // Forecasted production
        private List<DCAForecastPoint> ForecastPointsValue = new List<DCAForecastPoint>();

        public List<DCAForecastPoint> ForecastPoints

        {

            get { return this.ForecastPointsValue; }

            set { SetProperty(ref ForecastPointsValue, value); }

        }
        
        // Statistical metrics
        private decimal? RMSEValue;

        public decimal? RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        } // Root Mean Square Error
        private decimal? R2Value;

        public decimal? R2

        {

            get { return this.R2Value; }

            set { SetProperty(ref R2Value, value); }

        } // Coefficient of determination
        private decimal? CorrelationCoefficientValue;

        public decimal? CorrelationCoefficient

        {

            get { return this.CorrelationCoefficientValue; }

            set { SetProperty(ref CorrelationCoefficientValue, value); }

        }
        
        // Estimated reserves
        private decimal? EstimatedEURValue;

        public decimal? EstimatedEUR

        {

            get { return this.EstimatedEURValue; }

            set { SetProperty(ref EstimatedEURValue, value); }

        } // Estimated Ultimate Recovery
        private decimal? RemainingReservesValue;

        public decimal? RemainingReserves

        {

            get { return this.RemainingReservesValue; }

            set { SetProperty(ref RemainingReservesValue, value); }

        }
        
        // Additional metadata
        public DcaAdditionalResults? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED, PARTIAL
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string DeclineTypeValue;

        public string DeclineType

        {

            get { return this.DeclineTypeValue; }

            set { SetProperty(ref DeclineTypeValue, value); }

        }
        private List<decimal> ForecastedProductionValue;

        public List<decimal> ForecastedProduction

        {

            get { return this.ForecastedProductionValue; }

            set { SetProperty(ref ForecastedProductionValue, value); }

        }
        private bool IsSuccessfulValue;

        public bool IsSuccessful

        {

            get { return this.IsSuccessfulValue; }

            set { SetProperty(ref IsSuccessfulValue, value); }

        }
    }

    /// <summary>
    /// Forecast point for a specific date
    /// </summary>
    public class DCAForecastPoint : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private decimal? ProductionRateValue;

        public decimal? ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
        private decimal? CumulativeProductionValue;

        public decimal? CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }
        private decimal? DeclineRateValue;

        public decimal? DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }
    }
}




