using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Comprehensive set of DTOs for Decline Curve Analysis (DCA) advanced operations.
    /// Supports sensitivity analysis, Monte Carlo forecasting, model comparison, and portfolio analysis.
    /// </summary>

    /// <summary>
    /// DTO for sensitivity analysis results showing how DCA parameters affect forecasts.
    /// </summary>
    public class DcaSensitivityAnalysisResultDto
    {
        /// <summary>
        /// Base parameters used for analysis [qi, di, b]
        /// </summary>
        public double[] BaseParameters { get; set; }

        /// <summary>
        /// Percentage variation applied to parameters (e.g., 20 for ±20%)
        /// </summary>
        public double VariationPercent { get; set; }

        /// <summary>
        /// Sensitivity results for initial production rate (qi)
        /// </summary>
        public DcaParameterSensitivityDto QiSensitivity { get; set; }

        /// <summary>
        /// Sensitivity results for initial decline rate (di)
        /// </summary>
        public DcaParameterSensitivityDto DiSensitivity { get; set; }

        /// <summary>
        /// Sensitivity results for decline exponent (b)
        /// </summary>
        public DcaParameterSensitivityDto BSensitivity { get; set; }

        /// <summary>
        /// Date the analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }
    }

    /// <summary>
    /// DTO representing sensitivity of a single parameter
    /// </summary>
    public class DcaParameterSensitivityDto
    {
        /// <summary>
        /// Production at baseline parameter value
        /// </summary>
        public double BaselineProduction { get; set; }

        /// <summary>
        /// Production with low variation (-X%)
        /// </summary>
        public double LowVariationProduction { get; set; }

        /// <summary>
        /// Production with high variation (+X%)
        /// </summary>
        public double HighVariationProduction { get; set; }

        /// <summary>
        /// Total impact on final production as percentage
        /// </summary>
        public double ImpactOnFinalProduction { get; set; }
    }

    /// <summary>
    /// DTO for multiple decline models comparison
    /// </summary>
    public class DcaMultipleModelsComparisonResultDto
    {
        /// <summary>
        /// Number of data points analyzed
        /// </summary>
        public int DataPointsAnalyzed { get; set; }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Exponential decline model results
        /// </summary>
        public DcaDeclineModelDto ExponentialModel { get; set; }

        /// <summary>
        /// Hyperbolic decline model results
        /// </summary>
        public DcaDeclineModelDto HyperbolicModel { get; set; }

        /// <summary>
        /// Harmonic decline model results
        /// </summary>
        public DcaDeclineModelDto HarmonicModel { get; set; }

        /// <summary>
        /// Name of the best fitting model
        /// </summary>
        public string BestFitModel { get; set; }

        /// <summary>
        /// R² value of the best fitting model
        /// </summary>
        public double BestRSquared { get; set; }
    }

    /// <summary>
    /// DTO representing a single decline curve model
    /// </summary>
    public class DcaDeclineModelDto
    {
        /// <summary>
        /// Type of decline model (Exponential, Hyperbolic, Harmonic)
        /// </summary>
        public string ModelType { get; set; }

        /// <summary>
        /// Fitted parameters [qi, di, b]
        /// </summary>
        public double[] Parameters { get; set; }

        /// <summary>
        /// Coefficient of determination (R²)
        /// </summary>
        public double RSquared { get; set; }

        /// <summary>
        /// Akaike Information Criterion
        /// </summary>
        public double AIC { get; set; }

        /// <summary>
        /// Bayesian Information Criterion
        /// </summary>
        public double BIC { get; set; }

        /// <summary>
        /// Root mean square error
        /// </summary>
        public double RMSE { get; set; }
    }

    /// <summary>
    /// DTO for Monte Carlo probabilistic forecasting results
    /// </summary>
    public class DcaMonteCarloForecastResultDto
    {
        /// <summary>
        /// Number of simulations performed
        /// </summary>
        public int SimulationCount { get; set; }

        /// <summary>
        /// Number of months forecasted
        /// </summary>
        public int ForecastMonths { get; set; }

        /// <summary>
        /// Confidence level used (0.0-1.0)
        /// </summary>
        public double ConfidenceLevel { get; set; }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Monthly forecast statistics for each forecast month
        /// </summary>
        public List<DcaMonteCarloMonthlyStatsDto> ForecastMonthly { get; set; } = new();

        /// <summary>
        /// Total cumulative production across all months
        /// </summary>
        public double CumulativeProduction { get; set; }

        /// <summary>
        /// Probability well remains above economic limit
        /// </summary>
        public double ProbabilityEconomicViable { get; set; }
    }

    /// <summary>
    /// DTO for monthly statistics from Monte Carlo simulation
    /// </summary>
    public class DcaMonteCarloMonthlyStatsDto
    {
        /// <summary>
        /// Forecast month number (1-N)
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Mean production rate (bbl/day)
        /// </summary>
        public double MeanProduction { get; set; }

        /// <summary>
        /// Median production rate (bbl/day)
        /// </summary>
        public double MedianProduction { get; set; }

        /// <summary>
        /// 10th percentile production rate
        /// </summary>
        public double P10Production { get; set; }

        /// <summary>
        /// 50th percentile production rate
        /// </summary>
        public double P50Production { get; set; }

        /// <summary>
        /// 90th percentile production rate
        /// </summary>
        public double P90Production { get; set; }

        /// <summary>
        /// Minimum production in simulations
        /// </summary>
        public double MinProduction { get; set; }

        /// <summary>
        /// Maximum production in simulations
        /// </summary>
        public double MaxProduction { get; set; }

        /// <summary>
        /// Standard deviation of production
        /// </summary>
        public double StandardDeviation { get; set; }
    }

    /// <summary>
    /// DTO for model comparison metrics and ranking
    /// </summary>
    public class DcaModelComparisonMetricsResultDto
    {
        /// <summary>
        /// Number of models compared
        /// </summary>
        public int ModelsCompared { get; set; }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Metrics for each model
        /// </summary>
        public List<DcaModelMetricDto> ModelMetrics { get; set; } = new();

        /// <summary>
        /// Index of model with best AIC
        /// </summary>
        public int BestAICIndex { get; set; }

        /// <summary>
        /// Index of model with best R²
        /// </summary>
        public int BestRSquaredIndex { get; set; }

        /// <summary>
        /// Index of model with best BIC
        /// </summary>
        public int BestBICIndex { get; set; }
    }

    /// <summary>
    /// DTO for individual model metrics
    /// </summary>
    public class DcaModelMetricDto
    {
        /// <summary>
        /// Model index in comparison
        /// </summary>
        public int ModelIndex { get; set; }

        /// <summary>
        /// R² coefficient of determination
        /// </summary>
        public double RSquared { get; set; }

        /// <summary>
        /// Adjusted R²
        /// </summary>
        public double AdjustedRSquared { get; set; }

        /// <summary>
        /// Root mean square error
        /// </summary>
        public double RMSE { get; set; }

        /// <summary>
        /// Mean absolute error
        /// </summary>
        public double MAE { get; set; }

        /// <summary>
        /// Akaike Information Criterion
        /// </summary>
        public double AIC { get; set; }

        /// <summary>
        /// Bayesian Information Criterion
        /// </summary>
        public double BIC { get; set; }

        /// <summary>
        /// Whether model converged
        /// </summary>
        public bool Converged { get; set; }

        /// <summary>
        /// Number of parameters in model
        /// </summary>
        public int ParameterCount { get; set; }

        /// <summary>
        /// Overall performance score (0-100)
        /// </summary>
        public double PerformanceScore { get; set; }
    }

    /// <summary>
    /// DTO for production trend analysis
    /// </summary>
    public class DcaProductionTrendAnalysisResultDto
    {
        /// <summary>
        /// Number of data points analyzed
        /// </summary>
        public int DataPointsAnalyzed { get; set; }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Decline rates by interval
        /// </summary>
        public List<double> DeclineRatesByInterval { get; set; } = new();

        /// <summary>
        /// Identified inflection points
        /// </summary>
        public List<DcaInflectionPointDto> InflectionPoints { get; set; } = new();

        /// <summary>
        /// Detected phase transitions
        /// </summary>
        public List<DcaPhaseTransitionDto> PhaseTransitions { get; set; } = new();

        /// <summary>
        /// Average decline rate in early phase
        /// </summary>
        public double EarlyPhaseDecline { get; set; }

        /// <summary>
        /// Average decline rate in main phase
        /// </summary>
        public double MainPhaseDecline { get; set; }
    }

    /// <summary>
    /// DTO for inflection point identification
    /// </summary>
    public class DcaInflectionPointDto
    {
        /// <summary>
        /// Month number of inflection
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Date of inflection
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Production rate at inflection
        /// </summary>
        public double Production { get; set; }
    }

    /// <summary>
    /// DTO for phase transition detection
    /// </summary>
    public class DcaPhaseTransitionDto
    {
        /// <summary>
        /// Month of transition
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Change in decline rate
        /// </summary>
        public double DeclineRateChange { get; set; }
    }

    /// <summary>
    /// DTO for long-term production forecasting
    /// </summary>
    public class DcaLongTermForecastResultDto
    {
        /// <summary>
        /// Number of years in forecast
        /// </summary>
        public int ForecastYears { get; set; }

        /// <summary>
        /// Economic limit threshold (bbl/day)
        /// </summary>
        public double EconomicLimitBblPerDay { get; set; }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Monthly production forecast
        /// </summary>
        public List<double> MonthlyProduction { get; set; } = new();

        /// <summary>
        /// Total cumulative production
        /// </summary>
        public double TotalCumulativeProduction { get; set; }

        /// <summary>
        /// Average production rate
        /// </summary>
        public double AverageProductionRate { get; set; }

        /// <summary>
        /// Months until economic limit reached
        /// </summary>
        public int MonthsToEconomicLimit { get; set; }

        /// <summary>
        /// Years until economic limit reached
        /// </summary>
        public double YearsToEconomicLimit { get; set; }
    }

    /// <summary>
    /// DTO for end-of-life prediction
    /// </summary>
    public class DcaEndOfLifePredictionResultDto
    {
        /// <summary>
        /// Date analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Reference date for analysis
        /// </summary>
        public DateTime ReferenceDate { get; set; }

        /// <summary>
        /// Predicted end-of-life date
        /// </summary>
        public DateTime PredictedEOLDate { get; set; }

        /// <summary>
        /// Economic limit threshold (bbl/day)
        /// </summary>
        public double EconomicLimitBblPerDay { get; set; }

        /// <summary>
        /// Remaining well life in months
        /// </summary>
        public int RemainingLifeMonths { get; set; }

        /// <summary>
        /// Remaining well life in years
        /// </summary>
        public double RemainingLifeYears { get; set; }

        /// <summary>
        /// Estimated reserves to end-of-life
        /// </summary>
        public double ReservesToEOL { get; set; }
    }

    /// <summary>
    /// DTO for optimized decline parameters
    /// </summary>
    public class DcaOptimizedParametersResultDto
    {
        /// <summary>
        /// Number of data points used
        /// </summary>
        public int DataPointsUsed { get; set; }

        /// <summary>
        /// Date optimization was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Initial production rate (bbl/day)
        /// </summary>
        public double Qi { get; set; }

        /// <summary>
        /// Initial decline rate (1/year)
        /// </summary>
        public double Di { get; set; }

        /// <summary>
        /// Decline exponent (unitless)
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// Goodness of fit (R²)
        /// </summary>
        public double RSquared { get; set; }

        /// <summary>
        /// Root mean square error
        /// </summary>
        public double RMSE { get; set; }
    }

    /// <summary>
    /// DTO for forecast reliability assessment
    /// </summary>
    public class DcaForecastReliabilityResultDto
    {
        /// <summary>
        /// Date assessment was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Months of historical data available
        /// </summary>
        public int HistoricalMonths { get; set; }

        /// <summary>
        /// R² of the fitted model
        /// </summary>
        public double ModelRSquared { get; set; }

        /// <summary>
        /// Score component from R² (0-10)
        /// </summary>
        public double R2ScoreComponent { get; set; }

        /// <summary>
        /// Score component from historical data span (0-10)
        /// </summary>
        public double HistoryScoreComponent { get; set; }

        /// <summary>
        /// Score component from convergence (0-10)
        /// </summary>
        public double ConvergenceScoreComponent { get; set; }

        /// <summary>
        /// Overall reliability score (0-10)
        /// </summary>
        public double OverallReliabilityScore { get; set; }

        /// <summary>
        /// Reliability assessment (Excellent, Good, Moderate, Poor)
        /// </summary>
        public string ReliabilityAssessment { get; set; }

        /// <summary>
        /// Recommendations for improving forecast
        /// </summary>
        public List<string> Recommendations { get; set; } = new();
    }

    /// <summary>
    /// DTO for multi-well portfolio analysis
    /// </summary>
    public class DcaPortfolioAnalysisResultDto
    {
        /// <summary>
        /// Number of wells analyzed
        /// </summary>
        public int WellsAnalyzed { get; set; }

        /// <summary>
        /// Date analysis was performed
        /// </summary>
        public DateTime AnalysisDate { get; set; }

        /// <summary>
        /// Analysis results for each well
        /// </summary>
        public Dictionary<string, DcaPortfolioWellAnalysisDto> WellAnalyses { get; set; } = new();

        /// <summary>
        /// Average initial production rate (qi) across portfolio
        /// </summary>
        public double AverageQi { get; set; }

        /// <summary>
        /// Average initial decline rate (di) across portfolio
        /// </summary>
        public double AverageDi { get; set; }

        /// <summary>
        /// Average R² across portfolio models
        /// </summary>
        public double AverageRSquared { get; set; }
    }

    /// <summary>
    /// DTO for individual well analysis in portfolio
    /// </summary>
    public class DcaPortfolioWellAnalysisDto
    {
        /// <summary>
        /// Well identifier
        /// </summary>
        public string WellId { get; set; }

        /// <summary>
        /// Number of data points for this well
        /// </summary>
        public int DataPoints { get; set; }

        /// <summary>
        /// Initial production rate (bbl/day)
        /// </summary>
        public double InitialProduction { get; set; }

        /// <summary>
        /// Final production rate (bbl/day)
        /// </summary>
        public double FinalProduction { get; set; }

        /// <summary>
        /// Fitted qi parameter
        /// </summary>
        public double Qi { get; set; }

        /// <summary>
        /// Fitted di parameter
        /// </summary>
        public double Di { get; set; }

        /// <summary>
        /// R² of model fit
        /// </summary>
        public double RSquared { get; set; }
    }
}
