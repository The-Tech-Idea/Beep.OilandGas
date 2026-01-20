using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.EconomicAnalysis.Calculations;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Microsoft.Extensions.Logging;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.EconomicAnalysis.Services
{
    /// <summary>
    /// Service for economic analysis operations.
    /// Uses PPDMGenericRepository for data persistence following LifeCycle patterns.
    /// </summary>
    public partial class EconomicAnalysisService : IEconomicAnalysisService
    {
        private readonly ICommonColumnHandler _commonColumnHandler;
        private readonly IPPDM39DefaultsRepository _defaults;
        private readonly IPPDMMetadataRepository _metadata;
        private readonly IDMEEditor _editor;
        private readonly string _connectionName;
        private readonly ILogger<EconomicAnalysisService>? _logger;


        public EconomicAnalysisService(
            IDMEEditor editor,
            ICommonColumnHandler commonColumnHandler,
            IPPDM39DefaultsRepository defaults,
            IPPDMMetadataRepository metadata,
            string connectionName = "PPDM39",
            ILogger<EconomicAnalysisService>? logger = null)
        {
            _editor = editor ?? throw new ArgumentNullException(nameof(editor));
            _commonColumnHandler = commonColumnHandler ?? throw new ArgumentNullException(nameof(commonColumnHandler));
            _defaults = defaults ?? throw new ArgumentNullException(nameof(defaults));
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _connectionName = connectionName ?? throw new ArgumentNullException(nameof(connectionName));
            _logger = logger;
        }

        public double CalculateNPV(CashFlow[] cashFlows, double discountRate)
        {
            _logger?.LogDebug("Calculating NPV for {Count} cash flows with discount rate {Rate}", 
                cashFlows?.Length ?? 0, discountRate);
            return EconomicCalculator.CalculateNPV(cashFlows, discountRate);
        }

        public double CalculateIRR(CashFlow[] cashFlows, double initialGuess = 0.1)
        {
            _logger?.LogDebug("Calculating IRR for {Count} cash flows with initial guess {Guess}", 
                cashFlows?.Length ?? 0, initialGuess);
            return EconomicCalculator.CalculateIRR(cashFlows, initialGuess);
        }

        public EconomicResult Analyze(CashFlow[] cashFlows, double discountRate, double financeRate = 0.1, double reinvestRate = 0.1)
        {
            _logger?.LogInformation("Performing comprehensive economic analysis for {Count} cash flows", 
                cashFlows?.Length ?? 0);
            var result = EconomicCalculator.Analyze(cashFlows, discountRate, financeRate, reinvestRate);
            _logger?.LogInformation("Economic analysis completed: NPV={NPV}, IRR={IRR}", result.NPV, result.IRR);
            return result;
        }

        public List<NPVProfilePoint> GenerateNPVProfile(CashFlow[] cashFlows, double minRate = 0.0, double maxRate = 1.0, int points = 50)
        {
            _logger?.LogDebug("Generating NPV profile with {Points} points from {MinRate} to {MaxRate}", 
                points, minRate, maxRate);
            return EconomicCalculator.GenerateNPVProfile(cashFlows, minRate, maxRate, points);
        }

        public async Task SaveAnalysisResultAsync(string analysisId, EconomicResult result, string userId)
        {
            if (string.IsNullOrWhiteSpace(analysisId))
                throw new ArgumentException("Analysis ID cannot be null or empty", nameof(analysisId));
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));

            _logger?.LogInformation("Saving economic analysis result {AnalysisId}", analysisId);

            // Create repository for economic analysis result
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ECONOMIC_ANALYSIS_RESULT), _connectionName, "ECONOMIC_ANALYSIS_RESULT", null);

            // Create entity
            var entity = new ECONOMIC_ANALYSIS_RESULT
            {
                ANALYSIS_ID = analysisId,
                ANALYSIS_DATE = DateTime.UtcNow,
                NPV = (decimal)result.NPV,
                IRR = (decimal)result.IRR,
                PAYBACK_PERIOD = (decimal)result.PaybackPeriod,
                DISCOUNT_RATE = (decimal)result.DiscountRate,
                ACTIVE_IND = "Y"
            };

            // Prepare for insert (sets common columns)
            if (entity is IPPDMEntity ppdmEntity)
            {
                _commonColumnHandler.PrepareForInsert(ppdmEntity, userId);
            }
            await repo.InsertAsync(entity, userId);

            _logger?.LogInformation("Successfully saved economic analysis result {AnalysisId}", analysisId);
        }

        public async Task<EconomicResult?> GetAnalysisResultAsync(string analysisId)
        {
            if (string.IsNullOrWhiteSpace(analysisId))
            {
                _logger?.LogWarning("GetAnalysisResultAsync called with null or empty analysisId");
                return null;
            }

            _logger?.LogInformation("Getting economic analysis result {AnalysisId}", analysisId);

            // Create repository for economic analysis result
            var repo = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
                typeof(ECONOMIC_ANALYSIS_RESULT), _connectionName, "ECONOMIC_ANALYSIS_RESULT", null);

            // Get entity using filters
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "ANALYSIS_ID", Operator = "=", FilterValue = analysisId },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = "Y" }
            };

            var entities = await repo.GetAsync(filters);
            var entity = entities.Cast<ECONOMIC_ANALYSIS_RESULT>().FirstOrDefault();

            if (entity == null)
            {
                _logger?.LogWarning("Economic analysis result {AnalysisId} not found", analysisId);
                return null;
            }

            // Map entity to DTO
            var result = new EconomicResult
            {
                NPV = (double)(entity.NPV ?? 0),
                IRR = (double)(entity.IRR ?? 0),
                PaybackPeriod = (double)(entity.PAYBACK_PERIOD ?? 0),
                DiscountRate = (double)(entity.DISCOUNT_RATE ?? 0)
            };

            _logger?.LogInformation("Successfully retrieved economic analysis result {AnalysisId}", analysisId);
            return result;
        }

        /// <summary>
        /// Performs sensitivity analysis on key variables affecting NPV.
        /// </summary>
        public async Task<SensitivityAnalysis> PerformSensitivityAnalysisAsync(CashFlow[] cashFlows, double discountRate)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty", nameof(cashFlows));

            _logger?.LogInformation("Performing sensitivity analysis for {Count} cash flows", cashFlows.Length);

            var baseNPV = CalculateNPV(cashFlows, discountRate);
            var result = new SensitivityAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("SENSITIVITY", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                BaseNPV = baseNPV,
                BaseIRR = CalculateIRR(cashFlows),
                Parameters = new List<SensitivityParameter>()
            };

            // Sensitivity to discount rate
            var negativeDiscountRate = discountRate * 0.9;
            var positiveDiscountRate = discountRate * 1.1;
            var negativeNPV = CalculateNPV(cashFlows, negativeDiscountRate);
            var positiveNPV = CalculateNPV(cashFlows, positiveDiscountRate);

            result.Parameters.Add(new SensitivityParameter
            {
                ParameterName = "Discount Rate",
                BaseValue = discountRate,
                NegativeVariationNPV = negativeNPV,
                PositiveVariationNPV = positiveNPV,
                NPVImpact = Math.Abs(positiveNPV - negativeNPV),
                SensitivityIndex = (positiveNPV - negativeNPV) / (baseNPV * 0.2)
            });

            // Sensitivity to initial investment (reduce/increase first cash flow)
            if (cashFlows.Length > 0)
            {
                var reducedCashFlows = (CashFlow[])cashFlows.Clone();
                var increasedCashFlows = (CashFlow[])cashFlows.Clone();
                
                reducedCashFlows[0] = new CashFlow { Period = reducedCashFlows[0].Period, Amount = reducedCashFlows[0].Amount * 0.9 };
                increasedCashFlows[0] = new CashFlow { Period = increasedCashFlows[0].Period, Amount = increasedCashFlows[0].Amount * 1.1 };

                var reducedNPV = CalculateNPV(reducedCashFlows, discountRate);
                var increasedNPV = CalculateNPV(increasedCashFlows, discountRate);

                result.Parameters.Add(new SensitivityParameter
                {
                    ParameterName = "Initial Investment",
                    BaseValue = Math.Abs(cashFlows[0].Amount),
                    NegativeVariationNPV = reducedNPV,
                    PositiveVariationNPV = increasedNPV,
                    NPVImpact = Math.Abs(increasedNPV - reducedNPV),
                    SensitivityIndex = (increasedNPV - reducedNPV) / (baseNPV * 0.2)
                });
            }

            _logger?.LogInformation("Sensitivity analysis complete: {ParameterCount} parameters analyzed", result.Parameters.Count);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Performs scenario analysis with best case, base case, and worst case scenarios.
        /// </summary>
        public async Task<ScenarioAnalysis> PerformScenarioAnalysisAsync(CashFlow[] baseCase, CashFlow[] bestCase, CashFlow[] worstCase, double discountRate)
        {
            if (baseCase == null || baseCase.Length == 0)
                throw new ArgumentException("Base case cash flows cannot be null or empty", nameof(baseCase));

            _logger?.LogInformation("Performing scenario analysis with three scenarios");

            var baseNPV = CalculateNPV(baseCase, discountRate);
            var bestNPV = bestCase != null ? CalculateNPV(bestCase, discountRate) : baseNPV * 1.5;
            var worstNPV = worstCase != null ? CalculateNPV(worstCase, discountRate) : baseNPV * 0.5;

            // Calculate expected value and statistics
            var expectedNPV = (worstNPV * 0.25) + (baseNPV * 0.5) + (bestNPV * 0.25);
            var variance = Math.Pow(worstNPV - expectedNPV, 2) * 0.25 + Math.Pow(baseNPV - expectedNPV, 2) * 0.5 + Math.Pow(bestNPV - expectedNPV, 2) * 0.25;
            var stdDev = Math.Sqrt(variance);
            var cv = expectedNPV != 0 ? stdDev / expectedNPV : 0;

            var result = new ScenarioAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("SCENARIO", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                BaseNPV = baseNPV,
                BestCaseNPV = bestNPV,
                WorstCaseNPV = worstNPV,
                ExpectedValueNPV = expectedNPV,
                StandardDeviation = stdDev,
                CoefficientOfVariation = cv,
                Scenarios = new List<ScenarioResult>
                {
                    new ScenarioResult
                    {
                        ScenarioName = "Worst Case",
                        Probability = 0.25,
                        NPV = worstNPV,
                        IRR = CalculateIRR(worstCase ?? baseCase),
                        PaybackPeriod = CalculatePaybackPeriod(worstCase ?? baseCase)
                    },
                    new ScenarioResult
                    {
                        ScenarioName = "Base Case",
                        Probability = 0.5,
                        NPV = baseNPV,
                        IRR = CalculateIRR(baseCase),
                        PaybackPeriod = CalculatePaybackPeriod(baseCase)
                    },
                    new ScenarioResult
                    {
                        ScenarioName = "Best Case",
                        Probability = 0.25,
                        NPV = bestNPV,
                        IRR = CalculateIRR(bestCase ?? baseCase),
                        PaybackPeriod = CalculatePaybackPeriod(bestCase ?? baseCase)
                    }
                }
            };

            _logger?.LogInformation("Scenario analysis complete: Expected NPV={Expected}", expectedNPV);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Calculates comprehensive financial metrics including ROI, Profitability Index, and Modified IRR.
        /// </summary>
        public async Task<FinancialMetrics> CalculateFinancialMetricsAsync(CashFlow[] cashFlows, double discountRate, double financeRate = 0.1, double reinvestRate = 0.1)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty", nameof(cashFlows));

            _logger?.LogInformation("Calculating comprehensive financial metrics");

            var npv = CalculateNPV(cashFlows, discountRate);
            var irr = CalculateIRR(cashFlows);
            var paybackPeriod = CalculatePaybackPeriod(cashFlows);
            var profitabilityIndex = CalculateProfitabilityIndex(cashFlows, discountRate);
            var roi = CalculateROI(cashFlows);
            var modifiedIrr = CalculateModifiedIRR(cashFlows, financeRate, reinvestRate);
            var equivalentAnnualCost = CalculateEquivalentAnnualCost(cashFlows, discountRate);

            var result = new FinancialMetrics
            {
                MetricsId = _defaults.FormatIdForTable("METRICS", Guid.NewGuid().ToString()),
                CalculationDate = DateTime.UtcNow,
                NPV = npv,
                IRR = irr,
                PaybackPeriod = paybackPeriod,
                ProfitabilityIndex = profitabilityIndex,
                ROI = roi,
                ModifiedIRR = modifiedIrr,
                EquivalentAnnualCost = equivalentAnnualCost,
                VestigialValue = Math.Max(0, npv + Math.Abs(cashFlows[0].Amount))
            };

            _logger?.LogInformation("Financial metrics calculated: NPV={NPV}, IRR={IRR}, PI={PI}", npv, irr, profitabilityIndex);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Performs breakeven analysis for price, volume, or cost variables.
        /// </summary>
        public async Task<BreakevenAnalysis> PerformBreakevenAnalysisAsync(CashFlow[] baseCashFlows, double discountRate, string variableType = "Price")
        {
            if (baseCashFlows == null || baseCashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty", nameof(baseCashFlows));

            _logger?.LogInformation("Performing breakeven analysis for variable: {VariableType}", variableType);

            var baseNPV = CalculateNPV(baseCashFlows, discountRate);
            var result = new BreakevenAnalysis
            {
                AnalysisId = _defaults.FormatIdForTable("BREAKEVEN", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                BreakevenPoints = new List<BreakevenPoint>()
            };

            // Calculate breakeven through binary search or iteration
            var range = 100.0; // +/- 100% variation
            var step = 5.0;

            for (double variation = -range; variation <= range; variation += step)
            {
                var adjustmentFactor = 1.0 + (variation / 100.0);
                var adjustedCashFlows = baseCashFlows.Select(cf => new CashFlow 
                { 
                    Period = cf.Period, 
                    Amount = cf.Amount * adjustmentFactor 
                }).ToArray();

                var npvAtVariation = CalculateNPV(adjustedCashFlows, discountRate);
                var isBreakeven = Math.Abs(npvAtVariation) < 100; // Within $100 of breakeven

                result.BreakevenPoints.Add(new BreakevenPoint
                {
                    Variable = adjustmentFactor,
                    NPVAtVariable = npvAtVariation,
                    IsBreakevenPoint = isBreakeven
                });

                if (isBreakeven && result.BreakevenPrice == 0)
                {
                    result.BreakevenPrice = adjustmentFactor;
                }
            }

            result.MarginOfSafety = (result.BreakevenPrice > 0) ? (1.0 - result.BreakevenPrice) * 100 : 0;
            result.ContributionMargin = result.MarginOfSafety > 0 ? (baseNPV / Math.Abs(baseCashFlows[0].Amount)) * 100 : 0;

            _logger?.LogInformation("Breakeven analysis complete: Breakeven at {Breakeven}x", result.BreakevenPrice);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Performs risk metrics analysis including Value at Risk and Conditional Value at Risk.
        /// </summary>
        public async Task<RiskMetrics> AnalyzeRiskMetricsAsync(CashFlow[] cashFlows, double discountRate, int simulationCount = 1000)
        {
            if (cashFlows == null || cashFlows.Length == 0)
                throw new ArgumentException("Cash flows cannot be null or empty", nameof(cashFlows));

            _logger?.LogInformation("Analyzing risk metrics with {SimulationCount} simulations", simulationCount);

            var expectedNPV = CalculateNPV(cashFlows, discountRate);

            // Simulate with +/- 20% variation
            var npvDistribution = new List<double>();
            for (int i = 0; i < simulationCount; i++)
            {
                var variation = (new Random().NextDouble() - 0.5) * 0.4; // -20% to +20%
                var adjustedCashFlows = cashFlows.Select(cf => new CashFlow 
                { 
                    Period = cf.Period, 
                    Amount = cf.Amount * (1 + variation) 
                }).ToArray();
                npvDistribution.Add(CalculateNPV(adjustedCashFlows, discountRate));
            }

            var sortedNPVs = npvDistribution.OrderBy(x => x).ToList();
            var variance = npvDistribution.Sum(x => Math.Pow(x - expectedNPV, 2)) / simulationCount;
            var stdDev = Math.Sqrt(variance);
            var var95 = sortedNPVs[(int)(simulationCount * 0.05)];
            var cvar95 = sortedNPVs.Take((int)(simulationCount * 0.05)).Average();
            var probLoss = npvDistribution.Count(x => x < 0) / (double)simulationCount;

            var result = new RiskMetrics
            {
                RiskAnalysisId = _defaults.FormatIdForTable("RISK_METRICS", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                ExpectedNPV = expectedNPV,
                StandardDeviation = stdDev,
                Variance = variance,
                CoefficientOfVariation = expectedNPV != 0 ? stdDev / expectedNPV : 0,
                ValueAtRisk = var95,
                ConditionalVaR = cvar95,
                ProbabilityOfLoss = probLoss
            };

            _logger?.LogInformation("Risk metrics analysis complete: VaR95={VaR}, CVaR95={CVaR}", var95, cvar95);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Compares multiple projects and ranks them based on financial metrics.
        /// </summary>
        public async Task<ProjectComparison> CompareProjectsAsync(Dictionary<string, CashFlow[]> projects, double discountRate, string rankingMethod = "NPV")
        {
            if (projects == null || projects.Count == 0)
                throw new ArgumentException("Projects dictionary cannot be null or empty", nameof(projects));

            _logger?.LogInformation("Comparing {ProjectCount} projects using {RankingMethod} method", projects.Count, rankingMethod);

            var result = new ProjectComparison
            {
                ComparisonId = _defaults.FormatIdForTable("COMPARISON", Guid.NewGuid().ToString()),
                AnalysisDate = DateTime.UtcNow,
                Projects = new List<ProjectMetrics>(),
                RankingMethod = rankingMethod
            };

            var projectMetrics = new List<ProjectMetrics>();

            foreach (var project in projects)
            {
                var metrics = new ProjectMetrics
                {
                    ProjectName = project.Key,
                    NPV = CalculateNPV(project.Value, discountRate),
                    IRR = CalculateIRR(project.Value),
                    PaybackPeriod = CalculatePaybackPeriod(project.Value),
                    ProfitabilityIndex = CalculateProfitabilityIndex(project.Value, discountRate)
                };
                projectMetrics.Add(metrics);
            }

            // Rank projects based on selected method
            var rankedProjects = rankingMethod.ToLower() switch
            {
                "irr" => projectMetrics.OrderByDescending(x => x.IRR).ToList(),
                "pi" => projectMetrics.OrderByDescending(x => x.ProfitabilityIndex).ToList(),
                "payback" => projectMetrics.OrderBy(x => x.PaybackPeriod).ToList(),
                _ => projectMetrics.OrderByDescending(x => x.NPV).ToList()
            };

            for (int i = 0; i < rankedProjects.Count; i++)
            {
                rankedProjects[i].Rank = i + 1;
                rankedProjects[i].Score = (rankedProjects.Count - i) * 10.0 / rankedProjects.Count;
            }

            result.Projects = rankedProjects;
            result.RecommendedProject = rankedProjects.First().ProjectName;

            _logger?.LogInformation("Project comparison complete: {Count} projects ranked", result.Projects.Count);
            await Task.CompletedTask;
            return result;
        }

        /// <summary>
        /// Helper method to calculate payback period.
        /// </summary>
        private double CalculatePaybackPeriod(CashFlow[] cashFlows)
        {
            double cumulative = 0;
            for (int i = 0; i < cashFlows.Length; i++)
            {
                cumulative += cashFlows[i].Amount;
                if (cumulative >= 0)
                    return i;
            }
            return cashFlows.Length;
        }

        /// <summary>
        /// Helper method to calculate Profitability Index.
        /// </summary>
        private double CalculateProfitabilityIndex(CashFlow[] cashFlows, double discountRate)
        {
            var initialInvestment = Math.Abs(cashFlows[0].Amount);
            if (initialInvestment == 0) return 0;

            var presentValueOfCashInflows = 0.0;
            for (int i = 1; i < cashFlows.Length; i++)
            {
                if (cashFlows[i].Amount > 0)
                    presentValueOfCashInflows += cashFlows[i].Amount / Math.Pow(1 + discountRate, i);
            }

            return presentValueOfCashInflows / initialInvestment;
        }

        /// <summary>
        /// Helper method to calculate Return on Investment.
        /// </summary>
        private double CalculateROI(CashFlow[] cashFlows)
        {
            var totalProfit = cashFlows.Sum(cf => cf.Amount);
            var initialInvestment = Math.Abs(cashFlows[0].Amount);
            if (initialInvestment == 0) return 0;
            return (totalProfit / initialInvestment) * 100;
        }

        /// <summary>
        /// Helper method to calculate Modified Internal Rate of Return.
        /// </summary>
        private double CalculateModifiedIRR(CashFlow[] cashFlows, double financeRate, double reinvestRate)
        {
            if (cashFlows.Length < 2) return 0;

            var negativeCF = Math.Abs(cashFlows[0].Amount);
            var positiveCFSum = 0.0;

            for (int i = 1; i < cashFlows.Length; i++)
            {
                if (cashFlows[i].Amount > 0)
                    positiveCFSum += cashFlows[i].Amount * Math.Pow(1 + reinvestRate, cashFlows.Length - 1 - i);
            }

            var numberOfPeriods = cashFlows.Length - 1;
            return Math.Pow(positiveCFSum / negativeCF, 1.0 / numberOfPeriods) - 1;
        }

        /// <summary>
        /// Helper method to calculate Equivalent Annual Cost.
        /// </summary>
        private double CalculateEquivalentAnnualCost(CashFlow[] cashFlows, double discountRate)
        {
            var npv = CalculateNPV(cashFlows, discountRate);
            var numberOfPeriods = cashFlows.Length;
            if (numberOfPeriods == 0 || discountRate == 0) return 0;

            var annuityFactor = (1 - Math.Pow(1 + discountRate, -numberOfPeriods)) / discountRate;
            return npv / annuityFactor;
        }
    }
}

