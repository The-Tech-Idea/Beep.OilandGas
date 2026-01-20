using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.EconomicAnalysis.Services
{
    /// <summary>
    /// Advanced economic analysis methods for comprehensive financial evaluation.
    /// Provides Monte Carlo simulation, real options analysis, decision tree analysis, and more.
    /// </summary>
    public partial class EconomicAnalysisService
    {
        /// <summary>
        /// Performs Monte Carlo simulation for probabilistic economic analysis.
        /// </summary>
        public async Task<MonteCarloSimulationResult> PerformMonteCarloSimulationAsync(
            CashFlow[] baseCashFlows,
            double variationRange,
            double discountRate,
            int simulationCount = 10000)
        {
            try
            {
                _logger?.LogInformation("Starting Monte Carlo simulation: simulations={SimCount}, variation={Range}%, base_cashflows={CFCount}",
                    simulationCount, variationRange * 100, baseCashFlows?.Length ?? 0);

                if (baseCashFlows == null || baseCashFlows.Length == 0)
                    throw new ArgumentException("Cash flows cannot be null or empty", nameof(baseCashFlows));
                if (variationRange < 0 || variationRange > 1.0)
                    throw new ArgumentException("Variation range must be between 0 and 1", nameof(variationRange));
                if (simulationCount <= 0)
                    throw new ArgumentException("Simulation count must be positive", nameof(simulationCount));

                var result = new MonteCarloSimulationResult
                {
                    AnalysisId = _defaults.FormatIdForTable("MONTECARLO", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    SimulationCount = simulationCount,
                    NPVDistribution = new List<double>(),
                    IRRDistribution = new List<double>()
                };

                var random = new Random();
                var baseNPV = CalculateNPV(baseCashFlows, discountRate);

                for (int i = 0; i < simulationCount; i++)
                {
                    var simulatedCashFlows = baseCashFlows.Select(cf => new CashFlow
                    {
                        Period = cf.Period,
                        Amount = cf.Amount * (1 + ((random.NextDouble() - 0.5) * 2 * variationRange))
                    }).ToArray();

                    var npv = CalculateNPV(simulatedCashFlows, discountRate);
                    var irr = CalculateIRR(simulatedCashFlows);

                    result.NPVDistribution.Add(npv);
                    result.IRRDistribution.Add(irr);
                }

                // Calculate statistics
                result.MeanNPV = result.NPVDistribution.Average();
                result.MeanIRR = result.IRRDistribution.Average();
                result.StdDevNPV = CalculateStdDev(result.NPVDistribution);
                result.StdDevIRR = CalculateStdDev(result.IRRDistribution);

                var sortedNPVs = result.NPVDistribution.OrderBy(x => x).ToList();
                result.P10NPV = sortedNPVs[(int)(simulationCount * 0.10)];
                result.P50NPV = sortedNPVs[(int)(simulationCount * 0.50)];
                result.P90NPV = sortedNPVs[(int)(simulationCount * 0.90)];
                result.MinNPV = result.NPVDistribution.Min();
                result.MaxNPV = result.NPVDistribution.Max();
                result.ProbabilityOfLoss = result.NPVDistribution.Count(x => x < 0) / (double)simulationCount;

                _logger?.LogInformation("Monte Carlo simulation complete: mean_NPV={MeanNPV}, std_dev={StdDev}, P50={P50}",
                    result.MeanNPV, result.StdDevNPV, result.P50NPV);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing Monte Carlo simulation");
                throw;
            }
        }

        /// <summary>
        /// Performs real options analysis including expansion, abandonment, and switching options.
        /// </summary>
        public async Task<RealOptionsAnalysisResult> PerformRealOptionsAnalysisAsync(
            double initialNPV,
            double volatility,
            int projectLife,
            double discountRate)
        {
            try
            {
                _logger?.LogInformation("Performing real options analysis: initial_NPV={NPV}, volatility={Vol}%, life={Life}years",
                    initialNPV, volatility * 100, projectLife);

                if (volatility <= 0 || volatility > 2.0)
                    throw new ArgumentException("Volatility must be between 0 and 2.0", nameof(volatility));
                if (projectLife <= 0)
                    throw new ArgumentException("Project life must be positive", nameof(projectLife));
                if (discountRate < 0 || discountRate > 1.0)
                    throw new ArgumentException("Discount rate must be between 0 and 1", nameof(discountRate));

                var result = new RealOptionsAnalysisResult
                {
                    AnalysisId = _defaults.FormatIdForTable("REALOPTIONS", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    InitialNPV = initialNPV,
                    ProjectLife = projectLife,
                    Volatility = volatility,
                    Options = new List<OptionValuation>()
                };

                // Calculate expansion option (20% upside scenario)
                var expansionValue = CalculateExpansionOptionValue(initialNPV, volatility, projectLife, discountRate);
                    result.Options.Add(new OptionValuation
                {
                    OptionType = "Expansion",
                    OptionValue = expansionValue,
                    ScenarioValue = initialNPV * 1.2,
                    ExerciseCost = initialNPV * 0.1
                });

                // Calculate abandonment option (downside protection)
                var abandonmentValue = CalculateAbandonmentOptionValue(initialNPV, volatility, projectLife, discountRate);
                    result.Options.Add(new OptionValuation
                {
                    OptionType = "Abandonment",
                    OptionValue = abandonmentValue,
                    ScenarioValue = initialNPV * 0.5,
                    SalvageValue = initialNPV * 0.3
                });

                // Calculate switching option
                var switchingValue = CalculateSwitchingOptionValue(initialNPV, volatility, projectLife, discountRate);
                    result.Options.Add(new OptionValuation
                {
                    OptionType = "Switching",
                    OptionValue = switchingValue,
                    AlternativeNPV = initialNPV * 1.15,
                    SwitchingCost = initialNPV * 0.05
                });

                result.TotalOptionValue = result.Options.Sum(o => o.OptionValue);
                result.ProjectValueWithOptions = initialNPV + result.TotalOptionValue;
                result.FlexibilityPremium = result.TotalOptionValue / initialNPV * 100;

                _logger?.LogInformation("Real options analysis complete: total_option_value={Value}, flexibility_premium={Premium}%",
                    result.TotalOptionValue, result.FlexibilityPremium);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing real options analysis");
                throw;
            }
        }

        /// <summary>
        /// Performs decision tree analysis for multi-stage investment decisions.
        /// </summary>
        public async Task<DecisionTreeAnalysisResult> PerformDecisionTreeAnalysisAsync(
            double initialInvestment,
            double successProbability,
            CashFlow[] successCashFlows,
            CashFlow[] failureCashFlows,
            double discountRate)
        {
            try
            {
                _logger?.LogInformation("Performing decision tree analysis: initial_inv={Investment}, success_prob={Prob}%",
                    initialInvestment, successProbability * 100);

                if (initialInvestment < 0)
                    throw new ArgumentException("Investment cannot be negative", nameof(initialInvestment));
                if (successProbability < 0 || successProbability > 1.0)
                    throw new ArgumentException("Probability must be between 0 and 1", nameof(successProbability));
                if (successCashFlows == null || successCashFlows.Length == 0)
                    throw new ArgumentException("Cash flows cannot be null or empty", nameof(successCashFlows));

                var result = new DecisionTreeAnalysisResult
                {
                    AnalysisId = _defaults.FormatIdForTable("DECISION_TREE", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    InitialInvestment = initialInvestment,
                    SuccessProbability = successProbability
                };

                // Success scenario
                var successNPV = CalculateNPV(successCashFlows, discountRate);
                result.SuccessScenario = new DecisionScenario
                {
                    ScenarioName = "Success",
                    Probability = successProbability,
                    NPV = successNPV - initialInvestment,
                    IRR = CalculateIRR(successCashFlows),
                    CumulativeCashFlow = successCashFlows.Sum(cf => cf.Amount)
                };

                // Failure scenario
                var failureProbability = 1.0 - successProbability;
                var failureNPV = failureCashFlows != null && failureCashFlows.Length > 0
                    ? CalculateNPV(failureCashFlows, discountRate)
                    : -initialInvestment * 0.5;

                result.FailureScenario = new DecisionScenario
                {
                    ScenarioName = "Failure",
                    Probability = failureProbability,
                    NPV = failureNPV - initialInvestment,
                    IRR = failureCashFlows != null ? CalculateIRR(failureCashFlows) : -1.0,
                    CumulativeCashFlow = failureCashFlows?.Sum(cf => cf.Amount) ?? 0
                };

                // Expected value
                result.ExpectedNPV = (result.SuccessScenario.NPV * successProbability) +
                                    (result.FailureScenario.NPV * failureProbability);

                result.VarianceOfNPV = Math.Pow(result.SuccessScenario.NPV - result.ExpectedNPV, 2) * successProbability +
                                      Math.Pow(result.FailureScenario.NPV - result.ExpectedNPV, 2) * failureProbability;

                result.StandardDeviation = Math.Sqrt(result.VarianceOfNPV);
                result.Decision = result.ExpectedNPV > 0 ? "Proceed" : "Do Not Proceed";

                _logger?.LogInformation("Decision tree analysis complete: expected_NPV={NPV}, decision={Decision}",
                    result.ExpectedNPV, result.Decision);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing decision tree analysis");
                throw;
            }
        }

        /// <summary>
        /// Analyzes after-tax economic metrics considering corporate tax and depreciation.
        /// </summary>
        public async Task<AfterTaxAnalysisResult> PerformAfterTaxAnalysisAsync(
            double taxRate,
            double[] depreciationSchedule,
            CashFlow[] preTaxCashFlows,
            double discountRate)
        {
            try
            {
                _logger?.LogInformation("Performing after-tax analysis: tax_rate={Rate}%, depreciation_years={DepYears}",
                    taxRate * 100, depreciationSchedule?.Length ?? 0);

                if (taxRate < 0 || taxRate > 1.0)
                    throw new ArgumentException("Tax rate must be between 0 and 1", nameof(taxRate));
                if (preTaxCashFlows == null || preTaxCashFlows.Length == 0)
                    throw new ArgumentException("Cash flows cannot be null or empty", nameof(preTaxCashFlows));

                var result = new AfterTaxAnalysisResult
                {
                    AnalysisId = _defaults.FormatIdForTable("AFTERTAX", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    TaxRate = taxRate,
                    PreTaxNPV = CalculateNPV(preTaxCashFlows, discountRate),
                    AfterTaxCashFlows = new List<CashFlow>()
                };

                // Calculate after-tax cash flows
                for (int i = 0; i < preTaxCashFlows.Length; i++)
                {
                    var taxableIncome = preTaxCashFlows[i].Amount;
                    if (i < (depreciationSchedule?.Length ?? 0))
                    {
                        taxableIncome -= depreciationSchedule[i];
                    }

                    var taxes = taxableIncome * taxRate;
                    var afterTaxCF = preTaxCashFlows[i].Amount - taxes;

                    result.AfterTaxCashFlows.Add(new CashFlow
                    {
                        Period = preTaxCashFlows[i].Period,
                        Amount = afterTaxCF
                    });
                }

                result.AfterTaxNPV = CalculateNPV(result.AfterTaxCashFlows.ToArray(), discountRate);
                result.AfterTaxIRR = CalculateIRR(result.AfterTaxCashFlows.ToArray());
                result.TaxShield = result.PreTaxNPV - result.AfterTaxNPV;
                result.EffectiveTaxRate = Math.Abs(result.TaxShield / result.PreTaxNPV);

                _logger?.LogInformation("After-tax analysis complete: pre_tax_NPV={PreTax}, after_tax_NPV={AfterTax}, tax_shield={Shield}",
                    result.PreTaxNPV, result.AfterTaxNPV, result.TaxShield);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing after-tax analysis");
                throw;
            }
        }

        /// <summary>
        /// Calculates DCF enterprise value using discounted cash flow methodology.
        /// </summary>
        public async Task<DCFValuationResult> CalculateEnterpriseValueAsync(
            CashFlow[] projectedCashFlows,
            double terminalGrowthRate,
            double wacc)
        {
            try
            {
                _logger?.LogInformation("Calculating DCF enterprise value: cashflows={Count}, terminal_growth={Growth}%, WACC={WACC}%",
                    projectedCashFlows?.Length ?? 0, terminalGrowthRate * 100, wacc * 100);

                if (projectedCashFlows == null || projectedCashFlows.Length == 0)
                    throw new ArgumentException("Cash flows cannot be null or empty", nameof(projectedCashFlows));
                if (terminalGrowthRate < 0 || terminalGrowthRate > 0.1)
                    throw new ArgumentException("Terminal growth rate must be between 0 and 10%", nameof(terminalGrowthRate));
                if (wacc <= terminalGrowthRate)
                    throw new ArgumentException("WACC must be greater than terminal growth rate");

                var result = new DCFValuationResult
                {
                    AnalysisId = _defaults.FormatIdForTable("DCF_VALUATION", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    TerminalGrowthRate = terminalGrowthRate,
                    WACC = wacc,
                    PresentValueComponents = new List<PVComponent>()
                };

                double totalPV = 0;

                // Calculate PV of explicit forecast period
                for (int i = 0; i < projectedCashFlows.Length; i++)
                {
                    var pv = projectedCashFlows[i].Amount / Math.Pow(1 + wacc, i + 1);
                    totalPV += pv;

                    result.PresentValueComponents.Add(new PVComponent
                    {
                        Year = i + 1,
                        CashFlow = projectedCashFlows[i].Amount,
                        DiscountFactor = 1 / Math.Pow(1 + wacc, i + 1),
                        PresentValue = pv
                    });
                }

                // Calculate terminal value
                var lastCashFlow = projectedCashFlows[projectedCashFlows.Length - 1].Amount;
                var terminalCashFlow = lastCashFlow * (1 + terminalGrowthRate);
                var terminalValue = terminalCashFlow / (wacc - terminalGrowthRate);
                var pvTerminalValue = terminalValue / Math.Pow(1 + wacc, projectedCashFlows.Length);

                result.ExplicitPeriodValue = totalPV;
                result.TerminalValue = terminalValue;
                result.PVTerminalValue = pvTerminalValue;
                result.EnterpriseValue = totalPV + pvTerminalValue;
                result.TerminalValuePercentage = (pvTerminalValue / result.EnterpriseValue) * 100;

                _logger?.LogInformation("DCF valuation complete: enterprise_value={Value}M, terminal_value_pct={TermPct}%",
                    result.EnterpriseValue, result.TerminalValuePercentage);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error calculating enterprise value");
                throw;
            }
        }

        /// <summary>
        /// Performs lease vs. buy analysis for asset decisions.
        /// </summary>
        public async Task<LeaseBuyAnalysisResult> PerformLeaseBuyAnalysisAsync(
            double assetCost,
            double[] leasePayments,
            int leaseTerm,
            double salvageValue,
            double discountRate)
        {
            try
            {
                _logger?.LogInformation("Performing lease vs. buy analysis: asset_cost={Cost}M, lease_term={Term}years",
                    assetCost, leaseTerm);

                if (assetCost <= 0)
                    throw new ArgumentException("Asset cost must be positive", nameof(assetCost));
                if (leasePayments == null || leasePayments.Length == 0)
                    throw new ArgumentException("Lease payments cannot be null or empty", nameof(leasePayments));
                if (leaseTerm <= 0)
                    throw new ArgumentException("Lease term must be positive", nameof(leaseTerm));

                var result = new LeaseBuyAnalysisResult
                {
                    AnalysisId = _defaults.FormatIdForTable("LEASE_BUY", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    AssetCost = assetCost,
                    LeaseTerm = leaseTerm,
                    SalvageValue = salvageValue
                };

                // Calculate buy costs
                var buyCosts = new List<CashFlow> { new CashFlow { Period = 0, Amount = -assetCost } };
                for (int i = 1; i <= leaseTerm; i++)
                {
                    buyCosts.Add(new CashFlow { Period = i, Amount = -assetCost * 0.02 });  // 2% annual maintenance
                }
                buyCosts.Add(new CashFlow { Period = leaseTerm, Amount = salvageValue });

                // Calculate lease costs
                var leaseCosts = new List<CashFlow> { new CashFlow { Period = 0, Amount = 0 } };
                for (int i = 1; i <= leaseTerm; i++)
                {
                    var paymentIndex = Math.Min(i - 1, leasePayments.Length - 1);
                    leaseCosts.Add(new CashFlow { Period = i, Amount = -leasePayments[paymentIndex] });
                }

                result.BuyNPV = CalculateNPV(buyCosts.ToArray(), discountRate);
                result.LeaseNPV = CalculateNPV(leaseCosts.ToArray(), discountRate);
                result.NetAdvantageOfLeasing = result.BuyNPV - result.LeaseNPV;
                result.Recommendation = result.NetAdvantageOfLeasing > 0 ? "Lease" : "Buy";

                _logger?.LogInformation("Lease vs. buy analysis complete: buy_NPV={Buy}M, lease_NPV={Lease}M, recommendation={Rec}",
                    result.BuyNPV, result.LeaseNPV, result.Recommendation);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing lease vs. buy analysis");
                throw;
            }
        }

        /// <summary>
        /// Analyzes optimal capital structure and financial risk.
        /// </summary>
        public async Task<CapitalStructureAnalysisResult> AnalyzeOptimalCapitalStructureAsync(
            double unleveredValue,
            double taxRate,
            double debtCost,
            double equityCost)
        {
            try
            {
                _logger?.LogInformation("Analyzing optimal capital structure: unlevered_value={Value}M, tax_rate={Tax}%",
                    unleveredValue, taxRate * 100);

                if (unleveredValue <= 0)
                    throw new ArgumentException("Value must be positive", nameof(unleveredValue));
                if (taxRate < 0 || taxRate > 1.0)
                    throw new ArgumentException("Tax rate must be between 0 and 1", nameof(taxRate));
                if (debtCost < 0 || debtCost > 1.0)
                    throw new ArgumentException("Debt cost must be between 0 and 1", nameof(debtCost));
                if (equityCost < 0 || equityCost > 1.0)
                    throw new ArgumentException("Equity cost must be between 0 and 1", nameof(equityCost));

                var result = new CapitalStructureAnalysisResult
                {
                    AnalysisId = _defaults.FormatIdForTable("CAPSTRUCT", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    UnleveredValue = unleveredValue,
                    TaxRate = taxRate,
                    Scenarios = new List<CapitalStructureScenario>()
                };

                // Analyze different debt ratios
                for (double debtRatio = 0; debtRatio <= 1.0; debtRatio += 0.1)
                {
                    var equityRatio = 1.0 - debtRatio;
                    if (equityRatio < 0.01) continue;

                    var debtValue = unleveredValue * debtRatio;
                    var equityValue = unleveredValue * equityRatio;
                    var taxShield = debtValue * taxRate;
                    var leveredValue = unleveredValue + taxShield;
                    var wacc = (equityRatio * equityCost) + (debtRatio * debtCost * (1 - taxRate));

                    result.Scenarios.Add(new CapitalStructureScenario
                    {
                        DebtRatio = debtRatio,
                        EquityRatio = equityRatio,
                        DebtValue = debtValue,
                        EquityValue = equityValue,
                        TaxShield = taxShield,
                        LeveredValue = leveredValue,
                        WACC = wacc,
                        FinancialRisk = debtRatio > 0.5 ? "High" : debtRatio > 0.3 ? "Medium" : "Low"
                    });
                }

                var optimalScenario = result.Scenarios.OrderBy(s => s.WACC).First();
                result.OptimalDebtRatio = optimalScenario.DebtRatio;
                result.OptimalWACC = optimalScenario.WACC;
                result.OptimalLeveredValue = optimalScenario.LeveredValue;
                result.ValueCreation = result.OptimalLeveredValue - unleveredValue;

                _logger?.LogInformation("Capital structure analysis complete: optimal_debt_ratio={Ratio}%, optimal_WACC={WACC}%",
                    optimalScenario.DebtRatio * 100, optimalScenario.WACC * 100);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing capital structure");
                throw;
            }
        }

        /// <summary>
        /// Analyzes commodity price sensitivity for oil and gas projects.
        /// </summary>
        public async Task<CommodityPriceSensitivityResult> AnalyzeCommodityPriceSensitivityAsync(
            CashFlow[] baseCashFlows,
            double basePrice,
            double priceRange,
            double discountRate)
        {
            try
            {
                _logger?.LogInformation("Analyzing commodity price sensitivity: base_price=${Price}/unit, range={Range}%",
                    basePrice, priceRange * 100);

                if (baseCashFlows == null || baseCashFlows.Length == 0)
                    throw new ArgumentException("Cash flows cannot be null or empty", nameof(baseCashFlows));
                if (basePrice <= 0)
                    throw new ArgumentException("Price must be positive", nameof(basePrice));
                if (discountRate < 0 || discountRate > 1.0)
                    throw new ArgumentException("Discount rate must be between 0 and 1", nameof(discountRate));

                var result = new CommodityPriceSensitivityResult
                {
                    AnalysisId = _defaults.FormatIdForTable("PRICE_SENS", Guid.NewGuid().ToString()),
                    AnalysisDate = DateTime.UtcNow,
                    BasePrice = basePrice,
                    PriceScenarios = new List<PriceScenario>()
                };

                var baseNPV = CalculateNPV(baseCashFlows, discountRate);
                var minPrice = basePrice * (1 - priceRange);
                var maxPrice = basePrice * (1 + priceRange);
                var step = (maxPrice - minPrice) / 20;

                for (double price = minPrice; price <= maxPrice; price += step)
                {
                    var priceMultiplier = price / basePrice;
                    var adjustedCashFlows = baseCashFlows.Select(cf => new CashFlow
                    {
                        Period = cf.Period,
                        Amount = cf.Amount * priceMultiplier
                    }).ToArray();

                    var npv = CalculateNPV(adjustedCashFlows, discountRate);
                    var irr = CalculateIRR(adjustedCashFlows);

                    result.PriceScenarios.Add(new PriceScenario
                    {
                        Price = price,
                        NPV = npv,
                        IRR = irr,
                        IsBreakeven = Math.Abs(npv) < 100
                    });
                }

                var breakevenScenario = result.PriceScenarios.OrderBy(s => Math.Abs(s.NPV)).FirstOrDefault();
                result.BreakevenPrice = breakevenScenario?.Price ?? 0;
                result.BaseNPV = baseNPV;

                _logger?.LogInformation("Price sensitivity analysis complete: breakeven_price=${Price}/unit",
                    result.BreakevenPrice);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing commodity price sensitivity");
                throw;
            }
        }

        // ==================== HELPER METHODS ====================

        private double CalculateStdDev(List<double> values)
        {
            if (values.Count == 0) return 0;
            var mean = values.Average();
            var variance = values.Sum(x => Math.Pow(x - mean, 2)) / values.Count;
            return Math.Sqrt(variance);
        }

        private double CalculateExpansionOptionValue(double baseNPV, double volatility, int projectLife, double discountRate)
        {
            return baseNPV * 0.20;  // Simplified: 20% of base NPV
        }

        private double CalculateAbandonmentOptionValue(double baseNPV, double volatility, int projectLife, double discountRate)
        {
            return baseNPV * 0.15;  // Simplified: 15% of base NPV
        }

         private double CalculateSwitchingOptionValue(double baseNPV, double volatility, int projectLife, double discountRate)
         {
             return baseNPV * 0.10;  // Simplified: 10% of base NPV
         }
     }
}
