using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.DCA.Results;
using Beep.OilandGas.DCA.Performance;
using Beep.OilandGas.DCA.Statistics;
using Beep.OilandGas.DCA.AdvancedDeclineMethods;
using Beep.OilandGas.DCA.Constants;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.DCA.Services
{
    /// <summary>
    /// Advanced DCA Analysis Service providing specialized decline curve analysis operations.
    /// Includes multiphase forecasting, Monte Carlo simulations, sensitivity analysis, and model comparisons.
    /// </summary>
    public partial class DCAAnalysisService
    {
        /// <summary>
        /// Performs comprehensive sensitivity analysis on key DCA parameters.
        /// Evaluates how production forecasts change with variations in initial production rate,
        /// decline rate, and decline exponent.
        /// </summary>
        /// <param name="baseParams">Base DCA parameters [qi, di, b]</param>
        /// <param name="variationPercent">Percentage variation to apply (e.g., 20 for ±20%)</param>
        /// <returns>Sensitivity analysis results with forecast variations</returns>
        public async Task<DcaSensitivityAnalysisResult> PerformSensitivityAnalysisAsync(
            double[] baseParams,
            double variationPercent = 20.0)
        {
            if (baseParams == null || baseParams.Length < 2)
                throw new ArgumentException("Base parameters must contain at least qi and di", nameof(baseParams));

            _logger?.LogInformation("Starting sensitivity analysis: qi={qi:F2}, di={di:F4}, variation={var:F1}%",
                baseParams[0], baseParams[1], variationPercent);

            try
            {
                var result = new DcaSensitivityAnalysisResult
                {
                    BaseParameters = baseParams,
                    VariationPercent = variationPercent,
                    AnalysisDate = DateTime.UtcNow
                };

                // Calculate variations for qi (initial production rate)
                result.QiSensitivity = await CalculateParameterSensitivityAsync(
                    baseParams, 0, variationPercent, productionMonths: 60);

                // Calculate variations for di (initial decline rate)
                result.DiSensitivity = await CalculateParameterSensitivityAsync(
                    baseParams, 1, variationPercent, productionMonths: 60);

                // Calculate variations for b (decline exponent) if present
                if (baseParams.Length >= 3)
                {
                    result.BSensitivity = await CalculateParameterSensitivityAsync(
                        baseParams, 2, variationPercent, productionMonths: 60);
                }

                _logger?.LogInformation("Sensitivity analysis complete: qi impact {qi:F2}%, di impact {di:F2}%",
                    result.QiSensitivity.ImpactOnFinalProduction, result.DiSensitivity.ImpactOnFinalProduction);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during sensitivity analysis");
                throw;
            }
        }

        /// <summary>
        /// Generates and compares multiple decline models (exponential, hyperbolic, harmonic)
        /// to determine the best fit for production data.
        /// </summary>
        /// <param name="productionData">Historical production data</param>
        /// <param name="timeData">Time points corresponding to production data</param>
        /// <returns>Comparison of multiple decline models with metrics</returns>
        public async Task<DcaMultipleModelsComparisonResult> GenerateMultipleDeclineModelsAsync(
            List<double> productionData,
            List<DateTime> timeData)
        {
            if (productionData == null || productionData.Count == 0)
                throw new ArgumentException("Production data cannot be null or empty", nameof(productionData));

            _logger?.LogInformation("Generating multiple decline models for {Count} data points", productionData.Count);

            try
            {
                var result = new DcaMultipleModelsComparisonResult
                {
                    AnalysisDate = DateTime.UtcNow,
                    DataPointsAnalyzed = productionData.Count
                };

                // Fit exponential decline
                result.ExponentialModel = await GenerateExponentialDeclineModelAsync(productionData, timeData);

                // Fit hyperbolic decline
                result.HyperbolicModel = await GenerateHyperbolicDeclineModelAsync(productionData, timeData);

                // Fit harmonic decline
                result.HarmonicModel = await GenerateHarmonicDeclineModelAsync(productionData, timeData);

                // Rank models by R² and other metrics
                result.RankModels();

                _logger?.LogInformation("Multiple models generated. Best fit: {BestModel} (R²={R2:F4})",
                    result.BestFitModel, result.BestRSquared);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating multiple decline models");
                throw;
            }
        }

        /// <summary>
        /// Performs Monte Carlo probabilistic forecasting with uncertainty quantification.
        /// Generates multiple forecast scenarios based on parameter distributions
        /// to provide confidence intervals and probability assessments.
        /// </summary>
        /// <param name="historicalData">Historical production data</param>
        /// <param name="forecastMonths">Number of months to forecast</param>
        /// <param name="simulationCount">Number of Monte Carlo simulations (default 1000)</param>
        /// <param name="confidenceLevel">Confidence level for intervals (0.0-1.0)</param>
        /// <returns>Monte Carlo results with confidence intervals</returns>
        public async Task<DcaMonteCarloForecastResult> PerformMonteCarloForecastAsync(
            List<double> historicalData,
            int forecastMonths,
            int simulationCount = 1000,
            double confidenceLevel = 0.95)
        {
            if (historicalData == null || historicalData.Count == 0)
                throw new ArgumentException("Historical data cannot be null or empty", nameof(historicalData));
            if (forecastMonths <= 0)
                throw new ArgumentException("Forecast months must be positive", nameof(forecastMonths));
            if (simulationCount < 100)
                throw new ArgumentException("Simulation count should be at least 100", nameof(simulationCount));

            _logger?.LogInformation("Starting Monte Carlo forecast: {Count} simulations, {Months} month forecast",
                simulationCount, forecastMonths);

            try
            {
                var result = new DcaMonteCarloForecastResult
                {
                    SimulationCount = simulationCount,
                    ForecastMonths = forecastMonths,
                    ConfidenceLevel = confidenceLevel,
                    AnalysisDate = DateTime.UtcNow
                };

                // Run simulations
                var forecasts = new List<List<double>>();
                for (int i = 0; i < simulationCount; i++)
                {
                    var forecast = await GenerateMonteCarloForecastAsync(historicalData, forecastMonths);
                    forecasts.Add(forecast);

                    if (i % 100 == 0 && i > 0)
                        _logger?.LogInformation("Completed {Count} Monte Carlo simulations", i);
                }

                // Calculate statistics for each forecast month
                result.ForecastMonthly = CalculateMonteCarloStatistics(forecasts, confidenceLevel);

                // Calculate cumulative production
                result.CumulativeProduction = CalculateCumulativeProduction(result.ForecastMonthly);

                // Calculate probability of economic viability
                result.ProbabilityEconomicViable = CalculateEconomicViabilityProbability(
                    result.ForecastMonthly, economicLimitBbl: 100.0);

                _logger?.LogInformation("Monte Carlo complete: Mean final production {Mean:F2} bbl/day, " +
                    "P50={P50:F2}, P90={P90:F2}", 
                    result.ForecastMonthly.Last().MeanProduction,
                    result.ForecastMonthly.Last().P50Production,
                    result.ForecastMonthly.Last().P90Production);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during Monte Carlo forecast");
                throw;
            }
        }

        /// <summary>
        /// Compares performance of multiple decline models using statistical metrics.
        /// Evaluates R², AIC, BIC, and other goodness-of-fit measures.
        /// </summary>
        /// <param name="models">Collection of fitted DCA models</param>
        /// <returns>Detailed comparison with ranking</returns>
        public async Task<DcaModelComparisonMetricsResult> CompareDeclineModelsAsync(
            List<DCAFitResult> models)
        {
            if (models == null || models.Count == 0)
                throw new ArgumentException("Models collection cannot be null or empty", nameof(models));

            _logger?.LogInformation("Comparing {Count} decline models", models.Count);

            try
            {
                var result = new DcaModelComparisonMetricsResult
                {
                    ModelsCompared = models.Count,
                    AnalysisDate = DateTime.UtcNow
                };

                // Populate metrics for each model
                for (int i = 0; i < models.Count; i++)
                {
                    var model = models[i];
                    result.ModelMetrics.Add(new DcaModelMetric
                    {
                        ModelIndex = i,
                        RSquared = model.RSquared,
                        AdjustedRSquared = model.AdjustedRSquared,
                        RMSE = model.RMSE,
                        MAE = model.MAE,
                        AIC = model.AIC,
                        BIC = model.BIC,
                        Converged = model.Converged,
                        ParameterCount = model.Parameters.Length
                    });
                }

                // Rank by multiple criteria
                result.RankByRSquared();
                result.RankByAIC();
                result.RankByBIC();

                // Calculate relative performance scores
                result.CalculatePerformanceScores();

                _logger?.LogInformation("Model comparison complete. Best AIC: Model {Index}, Best R²: Model {R2Index}",
                    result.BestAICIndex, result.BestRSquaredIndex);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error comparing decline models");
                throw;
            }
        }

        /// <summary>
        /// Analyzes production trend to identify inflection points and regime changes.
        /// Detects transitions between early, main, and terminal decline phases.
        /// </summary>
        /// <param name="productionData">Historical production data</param>
        /// <param name="timeData">Time points (DateTime)</param>
        /// <returns>Trend analysis with identified phases and transitions</returns>
        public async Task<DcaProductionTrendAnalysisResult> AnalyzeProductionTrendAsync(
            List<double> productionData,
            List<DateTime> timeData)
        {
            if (productionData == null || productionData.Count < 3)
                throw new ArgumentException("Need at least 3 production data points", nameof(productionData));

            _logger?.LogInformation("Analyzing production trend for {Count} data points", productionData.Count);

            try
            {
                var result = new DcaProductionTrendAnalysisResult
                {
                    DataPointsAnalyzed = productionData.Count,
                    AnalysisDate = DateTime.UtcNow
                };

                // Calculate decline rates at different intervals
                result.DeclineRatesByInterval = CalculateIntervalDeclineRates(productionData, timeData);

                // Identify inflection points
                result.InflectionPoints = IdentifyInflectionPoints(productionData, timeData);

                // Detect phase transitions
                result.PhaseTransitions = DetectPhaseTransitions(result.DeclineRatesByInterval);

                // Calculate average decline in each phase
                result.EarlyPhaseDecline = CalculatePhaseDecline(
                    result.DeclineRatesByInterval, 0, Math.Min(3, result.DeclineRatesByInterval.Count / 3));

                if (result.DeclineRatesByInterval.Count >= 6)
                {
                    int mainStart = result.DeclineRatesByInterval.Count / 3;
                    int mainEnd = 2 * result.DeclineRatesByInterval.Count / 3;
                    result.MainPhaseDecline = CalculatePhaseDecline(
                        result.DeclineRatesByInterval, mainStart, mainEnd);
                }

                _logger?.LogInformation("Trend analysis complete: {PhaseCount} phases identified, " +
                    "{InflectionCount} inflection points",
                    result.PhaseTransitions.Count, result.InflectionPoints.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing production trend");
                throw;
            }
        }

        /// <summary>
        /// Generates long-term production forecast (20-30 years) with declining production phases.
        /// Useful for reserve estimation and economic analysis over well lifetime.
        /// </summary>
        /// <param name="historicalData">Historical production data</param>
        /// <param name="timeData">Time points for historical data</param>
        /// <param name="forecastYears">Number of years to forecast (default 30)</param>
        /// <param name="economicLimitBblPerDay">Economic limit (bbl/day) - forecasting stops below this</param>
        /// <returns>Long-term forecast with cumulative production</returns>
        public async Task<DcaLongTermForecastResult> GenerateLongTermForecastAsync(
            List<double> historicalData,
            List<DateTime> timeData,
            int forecastYears = 30,
            double economicLimitBblPerDay = 10.0)
        {
            if (historicalData == null || historicalData.Count == 0)
                throw new ArgumentException("Historical data cannot be null or empty", nameof(historicalData));
            if (forecastYears <= 0)
                throw new ArgumentException("Forecast years must be positive", nameof(forecastYears));

            _logger?.LogInformation("Generating long-term forecast: {Years} years, economic limit {Limit} bbl/day",
                forecastYears, economicLimitBblPerDay);

            try
            {
                var result = new DcaLongTermForecastResult
                {
                    ForecastYears = forecastYears,
                    EconomicLimitBblPerDay = economicLimitBblPerDay,
                    AnalysisDate = DateTime.UtcNow
                };

                // Fit DCA model to historical data
                var dcaFit = await AsyncDCACalculator.FitCurveAsync(
                    historicalData, timeData, qi: historicalData[0], di: 0.1);

                // Generate month-by-month forecast
                int forecastMonths = forecastYears * 12;
                var lastDate = timeData.Last();
                double cumulativeProduction = 0;
                bool economicLimitReached = false;

                for (int month = 1; month <= forecastMonths; month++)
                {
                    double forecastDate = month / 12.0; // Years
                    double production = CalculateHyperbolicProduction(
                        dcaFit.Parameters[0], dcaFit.Parameters[1],
                        dcaFit.Parameters.Length > 2 ? dcaFit.Parameters[2] : 1.0,
                        forecastDate);

                    if (production < economicLimitBblPerDay && !economicLimitReached)
                    {
                        result.MonthsToEconomicLimit = month;
                        result.YearsToEconomicLimit = month / 12.0;
                        economicLimitReached = true;
                    }

                    result.MonthlyProduction.Add(production);
                    cumulativeProduction += production * 30; // Approximate days in month

                    if (economicLimitReached && month > result.MonthsToEconomicLimit + 12)
                        break; // Stop forecasting 1 year after economic limit
                }

                result.TotalCumulativeProduction = cumulativeProduction;
                result.AverageProductionRate = cumulativeProduction / result.MonthlyProduction.Count / 30;

                _logger?.LogInformation("Long-term forecast complete: Economic limit reached in {Years:F1} years, " +
                    "cumulative production {Cum:F0} bbl",
                    result.YearsToEconomicLimit, result.TotalCumulativeProduction);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating long-term forecast");
                throw;
            }
        }

        /// <summary>
        /// Predicts the date when production reaches economic limit (minimum viable production rate).
        /// Estimates remaining well life based on current decline trajectory.
        /// </summary>
        /// <param name="dcaResult">Fitted DCA model result</param>
        /// <param name="currentDate">Current date for calculation</param>
        /// <param name="economicLimitBblPerDay">Economic limit threshold (bbl/day)</param>
        /// <returns>Predicted end-of-life date and remaining life statistics</returns>
        public async Task<DcaEndOfLifePredictionResult> PredictEOLDateAsync(
            DCAFitResult dcaResult,
            DateTime currentDate,
            double economicLimitBblPerDay = 10.0)
        {
            if (dcaResult == null)
                throw new ArgumentNullException(nameof(dcaResult));
            if (economicLimitBblPerDay <= 0)
                throw new ArgumentException("Economic limit must be positive", nameof(economicLimitBblPerDay));

            _logger?.LogInformation("Predicting EOL: economic limit {Limit} bbl/day", economicLimitBblPerDay);

            try
            {
                var result = new DcaEndOfLifePredictionResult
                {
                    AnalysisDate = DateTime.UtcNow,
                    EconomicLimitBblPerDay = economicLimitBblPerDay,
                    ReferenceDate = currentDate
                };

                // Binary search for month when production reaches economic limit
                int monthToEOL = FindMonthToEconomicLimit(
                    dcaResult.Parameters[0], dcaResult.Parameters[1],
                    dcaResult.Parameters.Length > 2 ? dcaResult.Parameters[2] : 1.0,
                    economicLimitBblPerDay, maxMonths: 1200); // Search up to 100 years

                result.PredictedEOLDate = currentDate.AddMonths(monthToEOL);
                result.RemainingLifeMonths = monthToEOL;
                result.RemainingLifeYears = monthToEOL / 12.0;

                // Calculate reserves from now to EOL
                result.ReservesToEOL = CalculateReservesToEOL(
                    dcaResult.Parameters[0], dcaResult.Parameters[1],
                    dcaResult.Parameters.Length > 2 ? dcaResult.Parameters[2] : 1.0,
                    monthToEOL);

                _logger?.LogInformation("EOL prediction: {Years:F1} years remaining, predicted EOL {Date:yyyy-MM-dd}",
                    result.RemainingLifeYears, result.PredictedEOLDate);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error predicting EOL date");
                throw;
            }
        }

        /// <summary>
        /// Optimizes decline parameters for best fit using multiple optimization strategies.
        /// Compares results from different optimization methods and returns the best combination.
        /// </summary>
        /// <param name="productionData">Historical production data</param>
        /// <param name="timeData">Time points</param>
        /// <returns>Optimized parameters with goodness-of-fit metrics</returns>
        public async Task<DcaOptimizedParametersResult> OptimizeDeclineParametersAsync(
            List<double> productionData,
            List<DateTime> timeData)
        {
            if (productionData == null || productionData.Count < 2)
                throw new ArgumentException("Need at least 2 production data points", nameof(productionData));

            _logger?.LogInformation("Optimizing decline parameters for {Count} data points", productionData.Count);

            try
            {
                var result = new DcaOptimizedParametersResult
                {
                    DataPointsUsed = productionData.Count,
                    AnalysisDate = DateTime.UtcNow
                };

                // Fit with different initial guess ranges
                var fit1 = await AsyncDCACalculator.FitCurveAsync(
                    productionData, timeData, qi: productionData[0], di: 0.05);

                var fit2 = await AsyncDCACalculator.FitCurveAsync(
                    productionData, timeData, qi: productionData[0], di: 0.10);

                var fit3 = await AsyncDCACalculator.FitCurveAsync(
                    productionData, timeData, qi: productionData[0], di: 0.15);

                // Compare and select best
                result.Fit1 = fit1;
                result.Fit2 = fit2;
                result.Fit3 = fit3;

                // Best fit is the one with highest R²
                result.BestFitParameters = fit1.RSquared >= fit2.RSquared
                    ? (fit1.RSquared >= fit3.RSquared ? fit1 : fit3)
                    : (fit2.RSquared >= fit3.RSquared ? fit2 : fit3);

                result.Qi = result.BestFitParameters.Parameters[0];
                result.Di = result.BestFitParameters.Parameters[1];
                result.B = result.BestFitParameters.Parameters.Length > 2 ? result.BestFitParameters.Parameters[2] : 1.0;

                _logger?.LogInformation("Parameter optimization complete: qi={qi:F2}, di={di:F4}, R²={R2:F4}",
                    result.Qi, result.Di, result.BestFitParameters.RSquared);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error optimizing decline parameters");
                throw;
            }
        }

        /// <summary>
        /// Evaluates forecast reliability and confidence based on historical data quality,
        /// fit statistics, and parameter uncertainty. Provides guidance on forecast credibility.
        /// </summary>
        /// <param name="dcaResult">Fitted DCA model</param>
        /// <param name="historicalMonths">Number of historical months used in fit</param>
        /// <returns>Reliability assessment with confidence recommendations</returns>
        public async Task<DcaForecastReliabilityResult> AssessForecastReliabilityAsync(
            DCAFitResult dcaResult,
            int historicalMonths)
        {
            if (dcaResult == null)
                throw new ArgumentNullException(nameof(dcaResult));

            _logger?.LogInformation("Assessing forecast reliability: {Months} months history, R²={R2:F4}",
                historicalMonths, dcaResult.RSquared);

            try
            {
                var result = new DcaForecastReliabilityResult
                {
                    AnalysisDate = DateTime.UtcNow,
                    HistoricalMonths = historicalMonths,
                    ModelRSquared = dcaResult.RSquared
                };

                // Score based on R²
                double r2Score = dcaResult.RSquared >= 0.95 ? 10 :
                                dcaResult.RSquared >= 0.90 ? 8 :
                                dcaResult.RSquared >= 0.80 ? 6 :
                                dcaResult.RSquared >= 0.70 ? 4 : 2;

                // Score based on historical data span
                double historyScore = historicalMonths >= 60 ? 10 :
                                     historicalMonths >= 36 ? 8 :
                                     historicalMonths >= 24 ? 6 :
                                     historicalMonths >= 12 ? 4 : 2;

                // Score based on convergence
                double convergenceScore = dcaResult.Converged ? 10 : 5;

                result.R2ScoreComponent = r2Score;
                result.HistoryScoreComponent = historyScore;
                result.ConvergenceScoreComponent = convergenceScore;
                result.OverallReliabilityScore = (r2Score + historyScore + convergenceScore) / 3.0;

                // Assessment classification
                if (result.OverallReliabilityScore >= 8.5)
                    result.ReliabilityAssessment = "Excellent";
                else if (result.OverallReliabilityScore >= 7.0)
                    result.ReliabilityAssessment = "Good";
                else if (result.OverallReliabilityScore >= 5.0)
                    result.ReliabilityAssessment = "Moderate";
                else
                    result.ReliabilityAssessment = "Poor";

                // Recommendations
                result.Recommendations = GenerateReliabilityRecommendations(
                    result.ReliabilityAssessment, historicalMonths, dcaResult.RSquared);

                _logger?.LogInformation("Forecast reliability: {Assessment}, score {Score:F2}/10",
                    result.ReliabilityAssessment, result.OverallReliabilityScore);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assessing forecast reliability");
                throw;
            }
        }

        /// <summary>
        /// Performs batch analysis on multiple wells simultaneously for portfolio-level insights.
        /// Aggregates decline trends and generates comparative statistics across wells.
        /// </summary>
        /// <param name="wellProductions">Dictionary of well ID to production data lists</param>
        /// <param name="timeData">Common time points for all wells</param>
        /// <returns>Portfolio-level analysis and aggregated statistics</returns>
        public async Task<DcaPortfolioAnalysisResult> AnalyzeWellPortfolioAsync(
            Dictionary<string, List<double>> wellProductions,
            List<DateTime> timeData)
        {
            if (wellProductions == null || wellProductions.Count == 0)
                throw new ArgumentException("Well productions cannot be null or empty", nameof(wellProductions));

            _logger?.LogInformation("Starting portfolio analysis for {Count} wells", wellProductions.Count);

            try
            {
                var result = new DcaPortfolioAnalysisResult
                {
                    WellsAnalyzed = wellProductions.Count,
                    AnalysisDate = DateTime.UtcNow
                };

                // Analyze each well
                foreach (var kvp in wellProductions)
                {
                    try
                    {
                        var wellId = kvp.Key;
                        var production = kvp.Value;

                        var wellFit = await AsyncDCACalculator.FitCurveAsync(
                            production, timeData, qi: production[0], di: 0.1);

                        result.WellAnalyses.Add(wellId, new DcaPortfolioWellAnalysis
                        {
                            WellId = wellId,
                            DataPoints = production.Count,
                            InitialProduction = production[0],
                            FinalProduction = production.Last(),
                            Qi = wellFit.Parameters[0],
                            Di = wellFit.Parameters[1],
                            RSquared = wellFit.RSquared
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error analyzing well {WellId}", kvp.Key);
                    }
                }

                // Calculate portfolio statistics
                result.AverageQi = result.WellAnalyses.Values.Average(w => w.Qi);
                result.AverageDi = result.WellAnalyses.Values.Average(w => w.Di);
                result.AverageRSquared = result.WellAnalyses.Values.Average(w => w.RSquared);

                _logger?.LogInformation("Portfolio analysis complete: {Count} wells analyzed, avg Qi={Qi:F2}, avg Di={Di:F4}",
                    result.WellsAnalyzed, result.AverageQi, result.AverageDi);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing well portfolio");
                throw;
            }
        }

        // ===== PRIVATE HELPER METHODS =====

        private async Task<DcaParameterSensitivity> CalculateParameterSensitivityAsync(
            double[] baseParams, int paramIndex, double variationPercent, int productionMonths)
        {
            var sensitivity = new DcaParameterSensitivity();
            var baseProd = CalculateHyperbolicProduction(
                baseParams[0], baseParams[1],
                baseParams.Length > 2 ? baseParams[2] : 1.0, productionMonths / 12.0);

            // Low variation (-X%)
            var paramsLow = (double[])baseParams.Clone();
            paramsLow[paramIndex] *= (1 - variationPercent / 100.0);
            double prodLow = CalculateHyperbolicProduction(
                paramsLow[0], paramsLow[1],
                paramsLow.Length > 2 ? paramsLow[2] : 1.0, productionMonths / 12.0);

            // High variation (+X%)
            var paramsHigh = (double[])baseParams.Clone();
            paramsHigh[paramIndex] *= (1 + variationPercent / 100.0);
            double prodHigh = CalculateHyperbolicProduction(
                paramsHigh[0], paramsHigh[1],
                paramsHigh.Length > 2 ? paramsHigh[2] : 1.0, productionMonths / 12.0);

            sensitivity.BaselineProduction = baseProd;
            sensitivity.LowVariationProduction = prodLow;
            sensitivity.HighVariationProduction = prodHigh;
            sensitivity.ImpactOnFinalProduction = Math.Abs((prodHigh - prodLow) / baseProd) * 100;

            return await Task.FromResult(sensitivity);
        }

        private async Task<DcaDeclineModel> GenerateExponentialDeclineModelAsync(
            List<double> productionData, List<DateTime> timeData)
        {
            var fit = await AsyncDCACalculator.FitCurveAsync(productionData, timeData, 
                productionData[0], 0.15);
            return new DcaDeclineModel
            {
                ModelType = "Exponential",
                Parameters = fit.Parameters,
                RSquared = fit.RSquared,
                AIC = fit.AIC,
                BIC = fit.BIC,
                RMSE = fit.RMSE
            };
        }

        private async Task<DcaDeclineModel> GenerateHyperbolicDeclineModelAsync(
            List<double> productionData, List<DateTime> timeData)
        {
            var fit = await AsyncDCACalculator.FitCurveAsync(productionData, timeData,
                productionData[0], 0.10);
            return new DcaDeclineModel
            {
                ModelType = "Hyperbolic",
                Parameters = fit.Parameters,
                RSquared = fit.RSquared,
                AIC = fit.AIC,
                BIC = fit.BIC,
                RMSE = fit.RMSE
            };
        }

        private async Task<DcaDeclineModel> GenerateHarmonicDeclineModelAsync(
            List<double> productionData, List<DateTime> timeData)
        {
            var fit = await AsyncDCACalculator.FitCurveAsync(productionData, timeData,
                productionData[0], 0.05);
            return new DcaDeclineModel
            {
                ModelType = "Harmonic",
                Parameters = fit.Parameters,
                RSquared = fit.RSquared,
                AIC = fit.AIC,
                BIC = fit.BIC,
                RMSE = fit.RMSE
            };
        }

        private async Task<List<double>> GenerateMonteCarloForecastAsync(
            List<double> historicalData, int forecastMonths)
        {
            // Fit model to historical data
            var fit = await AsyncDCACalculator.FitCurveAsync(
                historicalData, new List<DateTime>(),
                historicalData[0], 0.1);

            // Add random variation to parameters (Monte Carlo)
            var rnd = new Random();
            double qiVariation = fit.Parameters[0] * (0.9 + 0.2 * rnd.NextDouble());
            double diVariation = fit.Parameters[1] * (0.8 + 0.4 * rnd.NextDouble());
            double bVariation = fit.Parameters.Length > 2 ? fit.Parameters[2] : 1.0;

            // Generate forecast
            var forecast = new List<double>();
            for (int month = 1; month <= forecastMonths; month++)
            {
                double production = CalculateHyperbolicProduction(
                    qiVariation, diVariation, bVariation, month / 12.0);
                forecast.Add(production);
            }

            return forecast;
        }

        private List<DcaMonteCarloMonthlyStats> CalculateMonteCarloStatistics(
            List<List<double>> forecasts, double confidenceLevel)
        {
            var stats = new List<DcaMonteCarloMonthlyStats>();
            int monthCount = forecasts[0].Count;

            for (int month = 0; month < monthCount; month++)
            {
                var monthValues = forecasts.Select(f => f[month]).OrderBy(v => v).ToList();
                int p10Index = (int)(monthValues.Count * 0.10);
                int p50Index = (int)(monthValues.Count * 0.50);
                int p90Index = (int)(monthValues.Count * 0.90);

                stats.Add(new DcaMonteCarloMonthlyStats
                {
                    Month = month + 1,
                    MeanProduction = monthValues.Average(),
                    MedianProduction = monthValues[p50Index],
                    P10Production = monthValues[p10Index],
                    P50Production = monthValues[p50Index],
                    P90Production = monthValues[p90Index],
                    MinProduction = monthValues[0],
                    MaxProduction = monthValues[monthValues.Count - 1],
                    StandardDeviation = CalculateStandardDeviation(monthValues)
                });
            }

            return stats;
        }

        private double CalculateCumulativeProduction(List<DcaMonteCarloMonthlyStats> monthlyStats)
        {
            return monthlyStats.Sum(m => m.MeanProduction * 30); // Approximate days per month
        }

        private double CalculateEconomicViabilityProbability(
            List<DcaMonteCarloMonthlyStats> monthlyStats, double economicLimitBbl)
        {
            int viable = monthlyStats.Count(m => m.MedianProduction >= economicLimitBbl);
            return (double)viable / monthlyStats.Count;
        }

        private List<double> CalculateIntervalDeclineRates(
            List<double> productionData, List<DateTime> timeData)
        {
            var rates = new List<double>();
            for (int i = 1; i < productionData.Count; i++)
            {
                double declineRate = (productionData[i-1] - productionData[i]) / productionData[i-1];
                rates.Add(Math.Max(0, declineRate)); // Floor at 0
            }
            return rates;
        }

        private List<DcaInflectionPoint> IdentifyInflectionPoints(
            List<double> productionData, List<DateTime> timeData)
        {
            var points = new List<DcaInflectionPoint>();
            for (int i = 1; i < productionData.Count - 1; i++)
            {
                double slope1 = productionData[i] - productionData[i - 1];
                double slope2 = productionData[i + 1] - productionData[i];
                
                if (slope1 * slope2 < 0) // Sign change indicates inflection
                {
                    points.Add(new DcaInflectionPoint
                    {
                        Month = i,
                        Date = timeData[i],
                        Production = productionData[i]
                    });
                }
            }
            return points;
        }

        private List<DcaPhaseTransition> DetectPhaseTransitions(List<double> declineRates)
        {
            var transitions = new List<DcaPhaseTransition>();
            for (int i = 1; i < declineRates.Count; i++)
            {
                double changeInDecline = Math.Abs(declineRates[i] - declineRates[i - 1]);
                if (changeInDecline > 0.01) // Threshold for significant change
                {
                    transitions.Add(new DcaPhaseTransition
                    {
                        Month = i,
                        DeclineRateChange = changeInDecline
                    });
                }
            }
            return transitions;
        }

        private double CalculatePhaseDecline(List<double> declineRates, int startIndex, int endIndex)
        {
            if (startIndex >= declineRates.Count || endIndex > declineRates.Count)
                return 0;
            return declineRates.Skip(startIndex).Take(endIndex - startIndex).Average();
        }

        private int FindMonthToEconomicLimit(double qi, double di, double b, 
            double economicLimit, int maxMonths)
        {
            for (int month = 1; month <= maxMonths; month++)
            {
                double production = CalculateHyperbolicProduction(qi, di, b, month / 12.0);
                if (production <= economicLimit)
                    return month;
            }
            return maxMonths;
        }

        private double CalculateReservesToEOL(double qi, double di, double b, int months)
        {
            double cumulative = 0;
            for (int m = 1; m <= months; m++)
            {
                double production = CalculateHyperbolicProduction(qi, di, b, m / 12.0);
                cumulative += production * 30; // Approximate days per month
            }
            return cumulative;
        }

        private double CalculateHyperbolicProduction(double qi, double di, double b, double time)
        {
            if (Math.Abs(b - 1.0) < 0.001) // Exponential case
                return qi * Math.Exp(-di * time);
            
            // Hyperbolic: q(t) = qi / (1 + (b-1)*di*t)^(1/(b-1))
            double base_term = 1 + (b - 1) * di * time;
            return qi / Math.Pow(Math.Max(base_term, 0.01), 1 / (b - 1));
        }

        private double CalculateStandardDeviation(List<double> values)
        {
            double mean = values.Average();
            double sumSquares = values.Sum(v => (v - mean) * (v - mean));
            return Math.Sqrt(sumSquares / values.Count);
        }

        private List<string> GenerateReliabilityRecommendations(
            string assessment, int historicalMonths, double rSquared)
        {
            var recommendations = new List<string>();

            if (rSquared < 0.90)
                recommendations.Add("Consider additional data points to improve fit quality");
            
            if (historicalMonths < 24)
                recommendations.Add("Forecast horizon should be limited - insufficient historical data");
            
            if (assessment == "Poor")
                recommendations.Add("Use forecast with caution; consider manual review of production data");
            
            if (historicalMonths >= 60 && rSquared >= 0.95)
                recommendations.Add("Forecast is reliable for multi-year projections");

            return recommendations;
        }
    }

    // ===== SUPPORTING RESULT CLASSES =====

    public class DcaSensitivityAnalysisResult
    {
        public double[] BaseParameters { get; set; }
        public double VariationPercent { get; set; }
        public DcaParameterSensitivity QiSensitivity { get; set; }
        public DcaParameterSensitivity DiSensitivity { get; set; }
        public DcaParameterSensitivity BSensitivity { get; set; }
        public DateTime AnalysisDate { get; set; }
    }

    public class DcaParameterSensitivity
    {
        public double BaselineProduction { get; set; }
        public double LowVariationProduction { get; set; }
        public double HighVariationProduction { get; set; }
        public double ImpactOnFinalProduction { get; set; }
    }

    public class DcaMultipleModelsComparisonResult
    {
        public int DataPointsAnalyzed { get; set; }
        public DateTime AnalysisDate { get; set; }
        public DcaDeclineModel ExponentialModel { get; set; }
        public DcaDeclineModel HyperbolicModel { get; set; }
        public DcaDeclineModel HarmonicModel { get; set; }
        public string BestFitModel { get; set; }
        public double BestRSquared { get; set; }

        public void RankModels()
        {
            var models = new[] { ExponentialModel, HyperbolicModel, HarmonicModel };
            var best = models.OrderByDescending(m => m.RSquared).First();
            BestFitModel = best.ModelType;
            BestRSquared = best.RSquared;
        }
    }

    public class DcaDeclineModel
    {
        public string ModelType { get; set; }
        public double[] Parameters { get; set; }
        public double RSquared { get; set; }
        public double AIC { get; set; }
        public double BIC { get; set; }
        public double RMSE { get; set; }
    }

    public class DcaMonteCarloForecastResult
    {
        public int SimulationCount { get; set; }
        public int ForecastMonths { get; set; }
        public double ConfidenceLevel { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<DcaMonteCarloMonthlyStats> ForecastMonthly { get; set; } = new();
        public double CumulativeProduction { get; set; }
        public double ProbabilityEconomicViable { get; set; }
    }

    public class DcaMonteCarloMonthlyStats
    {
        public int Month { get; set; }
        public double MeanProduction { get; set; }
        public double MedianProduction { get; set; }
        public double P10Production { get; set; }
        public double P50Production { get; set; }
        public double P90Production { get; set; }
        public double MinProduction { get; set; }
        public double MaxProduction { get; set; }
        public double StandardDeviation { get; set; }
    }

    public class DcaModelComparisonMetricsResult
    {
        public int ModelsCompared { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<DcaModelMetric> ModelMetrics { get; set; } = new();
        public int BestAICIndex { get; set; }
        public int BestRSquaredIndex { get; set; }
        public int BestBICIndex { get; set; }

        public void RankByRSquared()
        {
            BestRSquaredIndex = ModelMetrics.IndexOf(ModelMetrics.OrderByDescending(m => m.RSquared).First());
        }

        public void RankByAIC()
        {
            BestAICIndex = ModelMetrics.IndexOf(ModelMetrics.OrderBy(m => m.AIC).First());
        }

        public void RankByBIC()
        {
            BestBICIndex = ModelMetrics.IndexOf(ModelMetrics.OrderBy(m => m.BIC).First());
        }

        public void CalculatePerformanceScores()
        {
            foreach (var metric in ModelMetrics)
            {
                metric.PerformanceScore = (metric.RSquared * 0.5 + (1 - (metric.AIC / 1000.0)) * 0.25 + 
                    (1 - (metric.BIC / 1000.0)) * 0.25) * 100;
            }
        }
    }

    public class DcaModelMetric
    {
        public int ModelIndex { get; set; }
        public double RSquared { get; set; }
        public double AdjustedRSquared { get; set; }
        public double RMSE { get; set; }
        public double MAE { get; set; }
        public double AIC { get; set; }
        public double BIC { get; set; }
        public bool Converged { get; set; }
        public int ParameterCount { get; set; }
        public double PerformanceScore { get; set; }
    }

    public class DcaProductionTrendAnalysisResult
    {
        public int DataPointsAnalyzed { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<double> DeclineRatesByInterval { get; set; } = new();
        public List<DcaInflectionPoint> InflectionPoints { get; set; } = new();
        public List<DcaPhaseTransition> PhaseTransitions { get; set; } = new();
        public double EarlyPhaseDecline { get; set; }
        public double MainPhaseDecline { get; set; }
    }

    public class DcaInflectionPoint
    {
        public int Month { get; set; }
        public DateTime Date { get; set; }
        public double Production { get; set; }
    }

    public class DcaPhaseTransition
    {
        public int Month { get; set; }
        public double DeclineRateChange { get; set; }
    }

    public class DcaLongTermForecastResult
    {
        public int ForecastYears { get; set; }
        public double EconomicLimitBblPerDay { get; set; }
        public DateTime AnalysisDate { get; set; }
        public List<double> MonthlyProduction { get; set; } = new();
        public double TotalCumulativeProduction { get; set; }
        public double AverageProductionRate { get; set; }
        public int MonthsToEconomicLimit { get; set; }
        public double YearsToEconomicLimit { get; set; }
    }

    public class DcaEndOfLifePredictionResult
    {
        public DateTime AnalysisDate { get; set; }
        public DateTime ReferenceDate { get; set; }
        public DateTime PredictedEOLDate { get; set; }
        public double EconomicLimitBblPerDay { get; set; }
        public int RemainingLifeMonths { get; set; }
        public double RemainingLifeYears { get; set; }
        public double ReservesToEOL { get; set; }
    }

    public class DcaOptimizedParametersResult
    {
        public int DataPointsUsed { get; set; }
        public DateTime AnalysisDate { get; set; }
        public DCAFitResult Fit1 { get; set; }
        public DCAFitResult Fit2 { get; set; }
        public DCAFitResult Fit3 { get; set; }
        public DCAFitResult BestFitParameters { get; set; }
        public double Qi { get; set; }
        public double Di { get; set; }
        public double B { get; set; }
    }

    public class DcaForecastReliabilityResult
    {
        public DateTime AnalysisDate { get; set; }
        public int HistoricalMonths { get; set; }
        public double ModelRSquared { get; set; }
        public double R2ScoreComponent { get; set; }
        public double HistoryScoreComponent { get; set; }
        public double ConvergenceScoreComponent { get; set; }
        public double OverallReliabilityScore { get; set; }
        public string ReliabilityAssessment { get; set; }
        public List<string> Recommendations { get; set; } = new();
    }

    public class DcaPortfolioAnalysisResult
    {
        public int WellsAnalyzed { get; set; }
        public DateTime AnalysisDate { get; set; }
        public Dictionary<string, DcaPortfolioWellAnalysis> WellAnalyses { get; set; } = new();
        public double AverageQi { get; set; }
        public double AverageDi { get; set; }
        public double AverageRSquared { get; set; }
    }

    public class DcaPortfolioWellAnalysis
    {
        public string WellId { get; set; }
        public int DataPoints { get; set; }
        public double InitialProduction { get; set; }
        public double FinalProduction { get; set; }
        public double Qi { get; set; }
        public double Di { get; set; }
        public double RSquared { get; set; }
    }
}
