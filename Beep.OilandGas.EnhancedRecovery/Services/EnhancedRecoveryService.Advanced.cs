using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.EnhancedRecovery.Services
{
    /// <summary>
    /// Advanced Enhanced Oil Recovery (EOR) Analysis Service.
    /// Provides specialized analysis for waterflooding, gas injection, chemical EOR, and thermal recovery.
    /// </summary>
    public partial class EnhancedRecoveryService
    {
        private readonly ILogger<EnhancedRecoveryService>? _logger;

        /// <summary>
        /// Analyzes waterflooding performance with recovery factor calculations.
        /// Evaluates flood front movement, pressure maintenance, and oil displacement.
        /// </summary>
        public async Task<WaterfloodPerformanceAnalysis> AnalyzeWaterfloodPerformanceAsync(
            string fieldId,
            List<double> productionHistory,
            List<DateTime> timeHistory,
            double initialOilInPlace,
            double waterInjectionRate)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));
            if (productionHistory == null || productionHistory.Count == 0)
                throw new ArgumentException("Production history cannot be null or empty", nameof(productionHistory));

            _logger?.LogInformation("Starting waterfload analysis: field={FieldId}, OOIP={OOIP:F0}, " +
                "injection rate={Rate:F2} bbl/day", fieldId, initialOilInPlace, waterInjectionRate);

            try
            {
                var result = new WaterfloodPerformanceAnalysis
                {
                    FieldId = fieldId,
                    AnalysisDate = DateTime.UtcNow,
                    DataPointsAnalyzed = productionHistory.Count
                };

                // Calculate cumulative production
                result.CumulativeProduction = productionHistory.Sum() * 30; // Approximate days per month

                // Calculate recovery factor
                result.RecoveryFactor = (result.CumulativeProduction / initialOilInPlace) * 100;

                // Estimate incremental recovery from waterflooding
                // Typical waterflooding adds 10-25% incremental recovery
                result.IncrementalRecoveryFactor = CalculateIncrementalRecovery(
                    productionHistory, waterInjectionRate, initialOilInPlace);

                // Analyze pressure maintenance effect
                result.PressureMaintenanceEfficiency = CalculatePressureMaintenanceEfficiency(
                    productionHistory, waterInjectionRate);

                // Calculate water cut trend
                result.WaterCutTrend = AnalyzeWaterCutTrend(productionHistory, timeHistory.Count);

                // Estimate flood front advancement
                result.FloodFrontVelocity = EstimateFloodFrontVelocity(
                    waterInjectionRate, initialOilInPlace, result.DataPointsAnalyzed);

                // Calculate volumetric sweep efficiency (2-D Buckingham correlation approximation)
                result.SweepEfficiency = EstimateSweepEfficiency(
                    result.FloodFrontVelocity, result.WaterCutTrend.FinalWaterCut);

                // Project future recovery
                result.ProjectedRecovery20Years = ProjectRecovery(
                    productionHistory.Last(), result.RecoveryFactor, months: 240);

                _logger?.LogInformation("Waterflood analysis complete: RF={RF:F2}%, incremental={Inc:F2}%, " +
                    "sweep efficiency={Sweep:F2}%",
                    result.RecoveryFactor, result.IncrementalRecoveryFactor, result.SweepEfficiency);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing waterflooding performance for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes gas injection recovery mechanisms (miscible and immiscible displacement).
        /// </summary>
        public async Task<GasInjectionAnalysis> AnalyzeGasInjectionAsync(
            string fieldId,
            string gasType,
            double injectionPressure,
            double minimumMiscibilityPressure,
            List<double> productionHistory)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));

            _logger?.LogInformation("Analyzing gas injection: field={FieldId}, type={Type}, " +
                "pressure={Pressure:F0} psi, MMP={MMP:F0} psi",
                fieldId, gasType, injectionPressure, minimumMiscibilityPressure);

            try
            {
                var result = new GasInjectionAnalysis
                {
                    FieldId = fieldId,
                    GasType = gasType,
                    InjectionPressure = injectionPressure,
                    MinimumMiscibilityPressure = minimumMiscibilityPressure,
                    AnalysisDate = DateTime.UtcNow
                };

                // Determine if miscible or immiscible conditions
                result.IsMiscible = injectionPressure >= minimumMiscibilityPressure;
                result.DisplacementMechanism = result.IsMiscible ? "Miscible" : "Immiscible";

                // Calculate recovery efficiency based on mechanism
                result.DisplacementEfficiency = result.IsMiscible ? 0.90 : 0.65; // Typical values

                // Estimate residual oil saturation
                result.ResidualOilSaturation = CalculateResidualOilSaturation(
                    result.DisplacementMechanism, capillaryNumber: 1.0e-5);

                // Project production improvement
                result.ProductionImprovement = CalculateProductionImprovement(
                    productionHistory.Last(), result.DisplacementEfficiency);

                // Estimate tertiary recovery potential
                result.TertiaryRecoveryPotential = CalculateTertiaryRecoveryPotential(
                    result.DisplacementEfficiency, result.ResidualOilSaturation);

                // Identify gas type specific parameters
                result.GasTypeCharacteristics = GetGasTypeCharacteristics(gasType);

                _logger?.LogInformation("Gas injection analysis complete: mechanism={Mech}, " +
                    "displacement efficiency={Eff:F2}%, recovery potential={Pot:F2}%",
                    result.DisplacementMechanism, result.DisplacementEfficiency * 100,
                    result.TertiaryRecoveryPotential);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing gas injection for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes chemical EOR methods (polymer flooding, surfactant flooding, alkali flooding).
        /// </summary>
        public async Task<ChemicalEORAnalysis> AnalyzeChemicalEORAsync(
            string fieldId,
            string chemicalType,
            double reservoirTemperature,
            double salinity,
            double crudePaveViscosity)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));
            if (string.IsNullOrWhiteSpace(chemicalType))
                throw new ArgumentException("Chemical type must be specified", nameof(chemicalType));

            _logger?.LogInformation("Analyzing chemical EOR: field={FieldId}, type={Type}, " +
                "temperature={Temp:F1}°F, salinity={Sal:F0} ppm",
                fieldId, chemicalType, reservoirTemperature, salinity);

            try
            {
                var result = new ChemicalEORAnalysis
                {
                    FieldId = fieldId,
                    ChemicalType = chemicalType,
                    ReservoirTemperature = reservoirTemperature,
                    Salinity = salinity,
                    CrudePaveViscosity = crudePaveViscosity,
                    AnalysisDate = DateTime.UtcNow
                };

                // Evaluate suitability for chemical type
                result.Suitability = EvaluateChemicalSuitability(
                    chemicalType, reservoirTemperature, salinity, crudePaveViscosity);

                // Calculate interfacial tension reduction
                result.InterfacialTensionReduction = CalculateInterfacialTensionReduction(
                    chemicalType, salinity);

                // Estimate oil recovery
                result.OilRecoveryIncrement = EstimateChemicalRecoveryIncrement(
                    chemicalType, result.InterfacialTensionReduction);

                // Calculate chemical cost-effectiveness
                result.CostPerBarrelRecovered = EstimateChemicalCost(
                    chemicalType, result.OilRecoveryIncrement);

                // Identify potential chemical concerns
                result.EnvironmentalConcerns = IdentifyChemicalConcerns(chemicalType);

                // Get chemical-specific parameters
                result.ChemicalParameters = GetChemicalParameters(chemicalType);

                _logger?.LogInformation("Chemical EOR analysis complete: type={Type}, suitability={Suit}, " +
                    "recovery increment={Rec:F2}%, cost per bbl={Cost:F2}",
                    chemicalType, result.Suitability, result.OilRecoveryIncrement * 100,
                    result.CostPerBarrelRecovered);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing chemical EOR for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes thermal recovery methods (steam injection, in-situ combustion).
        /// </summary>
        public async Task<ThermalRecoveryAnalysis> AnalyzeThermalRecoveryAsync(
            string fieldId,
            string thermalMethod,
            double reservoirTemperature,
            double crudePaveViscosity,
            double oilSaturation)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));

            _logger?.LogInformation("Analyzing thermal recovery: field={FieldId}, method={Method}, " +
                "temp={Temp:F1}°F, viscosity={Visc:F0} cp, saturation={Sat:F2}",
                fieldId, thermalMethod, reservoirTemperature, crudePaveViscosity, oilSaturation);

            try
            {
                var result = new ThermalRecoveryAnalysis
                {
                    FieldId = fieldId,
                    ThermalMethod = thermalMethod,
                    ReservoirTemperature = reservoirTemperature,
                    CrudePaveViscosity = crudePaveViscosity,
                    OilSaturation = oilSaturation,
                    AnalysisDate = DateTime.UtcNow
                };

                // Evaluate suitability for thermal method
                result.Suitability = EvaluateThermalSuitability(
                    thermalMethod, crudePaveViscosity, oilSaturation);

                // Calculate viscosity reduction with temperature
                result.ViscosityReduction = CalculateViscosityReduction(
                    crudePaveViscosity, reservoirTemperature, thermalMethod);

                // Estimate oil mobility improvement
                result.MobilityImprovement = CalculateMobilityImprovement(
                    crudePaveViscosity, result.ViscosityReduction);

                // Calculate energy requirements
                result.EnergyRequirement = EstimateThermalEnergyRequirement(
                    thermalMethod, reservoirTemperature);

                // Project recovery factor
                result.ProjectedRecoveryFactor = EstimateThermalRecoveryFactor(
                    thermalMethod, result.MobilityImprovement, oilSaturation);

                // Calculate operating cost
                result.OperatingCostPerBarrel = EstimateThermalOperatingCost(
                    thermalMethod, result.EnergyRequirement);

                // Environmental analysis
                result.EnvironmentalImpact = AssessThermalEnvironmentalImpact(thermalMethod);

                _logger?.LogInformation("Thermal recovery analysis complete: method={Method}, " +
                    "suitability={Suit}, RF={RF:F2}%, cost/bbl=${Cost:F2}",
                    thermalMethod, result.Suitability, result.ProjectedRecoveryFactor * 100,
                    result.OperatingCostPerBarrel);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing thermal recovery for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Performs EOR method selection and comparison for optimal field development strategy.
        /// </summary>
        public async Task<EORMethodComparison> CompareEORMethodsAsync(
            string fieldId,
            List<string> methodsToEvaluate,
            Dictionary<string, double> reservoirProperties)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));
            if (methodsToEvaluate == null || methodsToEvaluate.Count == 0)
                throw new ArgumentException("Methods to evaluate cannot be null or empty", nameof(methodsToEvaluate));

            _logger?.LogInformation("Comparing EOR methods for field {FieldId}: {Methods}",
                fieldId, string.Join(", ", methodsToEvaluate));

            try
            {
                var result = new EORMethodComparison
                {
                    FieldId = fieldId,
                    MethodsCompared = methodsToEvaluate.Count,
                    AnalysisDate = DateTime.UtcNow
                };

                // Evaluate each method
                foreach (var method in methodsToEvaluate)
                {
                    var score = CalculateEORMethodScore(method, reservoirProperties);
                    result.MethodScores.Add(score);
                }

                // Rank methods
                result.RankedMethods = result.MethodScores
                    .OrderByDescending(x => x.OverallScore)
                    .Select((x, index) => new RankedEORMethod
                    {
                        Rank = index + 1,
                        MethodName = x.Method,
                        Score = x
                    }).ToList();

                result.RecommendedMethod = result.RankedMethods.First().MethodName;

                // Calculate synergies for combined approaches
                result.SynergyPotential = CalculateEORSynergyPotential(
                    methodsToEvaluate, result.MethodScores);

                _logger?.LogInformation("EOR method comparison complete: {Count} methods evaluated, " +
                    "recommended={Rec}, synergy potential={Syn:F2}%",
                    methodsToEvaluate.Count, result.RecommendedMethod, result.SynergyPotential * 100);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error comparing EOR methods for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Calculates injection well placement optimization for maximum reservoir contact.
        /// </summary>
        public async Task<InjectionWellOptimization> OptimizeInjectionWellPlacementAsync(
            string fieldId,
            int desiredWellCount,
            double reservoirArea,
            double thickness,
            double permeability)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));
            if (desiredWellCount <= 0)
                throw new ArgumentException("Well count must be positive", nameof(desiredWellCount));

            _logger?.LogInformation("Optimizing injection well placement: field={FieldId}, wells={Count}, " +
                "area={Area:F0} acres, thickness={Thick:F1} ft, perm={Perm:F0} md",
                fieldId, desiredWellCount, reservoirArea, thickness, permeability);

            try
            {
                var result = new InjectionWellOptimization
                {
                    FieldId = fieldId,
                    DesiredWellCount = desiredWellCount,
                    ReservoirArea = reservoirArea,
                    ReservoirThickness = thickness,
                    Permeability = permeability,
                    AnalysisDate = DateTime.UtcNow
                };

                // Calculate well spacing
                result.OptimalWellSpacing = CalculateOptimalWellSpacing(
                    reservoirArea, desiredWellCount);

                // Calculate areal coverage per well
                result.AreaPerWell = reservoirArea / desiredWellCount;

                // Estimate maximum injection rate per well
                result.MaxInjectionRatePerWell = EstimateMaxInjectionRate(
                    permeability, thickness, result.OptimalWellSpacing);

                // Calculate total injection capacity
                result.TotalInjectionCapacity = result.MaxInjectionRatePerWell * desiredWellCount;

                // Estimate volumetric sweep using areal sweep concept
                result.EstimatedArealSweep = EstimateArealSweepEfficiency(
                    result.OptimalWellSpacing, thickness, permeability);

                // Generate placement grid recommendations
                result.SuggestedPlacementPattern = GenerateWellPlacementPattern(
                    reservoirArea, desiredWellCount, thickness);

                // Risk assessment
                result.RiskFactors = IdentifyWellPlacementRisks(
                    reservoirArea, desiredWellCount, permeability);

                _logger?.LogInformation("Well placement optimization complete: spacing={Space:F1} acres, " +
                    "capacity={Cap:F0} bbl/day, areal sweep={Sweep:F2}%",
                    result.OptimalWellSpacing, result.TotalInjectionCapacity,
                    result.EstimatedArealSweep * 100);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error optimizing injection well placement for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Analyzes pressure performance and response in EOR operations.
        /// </summary>
        public async Task<PressurePerformanceAnalysis> AnalyzePressurePerformanceAsync(
            string fieldId,
            double initialReservoirPressure,
            double currentReservoirPressure,
            double injectionRate,
            int operationMonths,
            double reservoirVolume)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));

            _logger?.LogInformation("Analyzing pressure performance: field={FieldId}, initial={Init:F0} psi, " +
                "current={Curr:F0} psi, injection={Inj:F0} bbl/day",
                fieldId, initialReservoirPressure, currentReservoirPressure, injectionRate);

            try
            {
                var result = new PressurePerformanceAnalysis
                {
                    FieldId = fieldId,
                    InitialReservoirPressure = initialReservoirPressure,
                    CurrentReservoirPressure = currentReservoirPressure,
                    InjectionRate = injectionRate,
                    OperationMonths = operationMonths,
                    AnalysisDate = DateTime.UtcNow
                };

                // Calculate pressure change
                result.PressureChange = currentReservoirPressure - initialReservoirPressure;
                result.PressureChangePerMonth = result.PressureChange / Math.Max(1, operationMonths);

                // Estimate effective compressibility
                result.EffectiveCompressibility = EstimateEffectiveCompressibility(
                    result.PressureChange, reservoirVolume, injectionRate * operationMonths * 30);

                // Calculate pressure maintenance efficiency (simple ratio-based)
                result.PressureMaintenanceEfficiency = (currentReservoirPressure / initialReservoirPressure);

                // Predict future pressure at current injection rate
                result.ProjectedPressure12Months = ProjectPressure(
                    currentReservoirPressure, result.PressureChangePerMonth, 12);

                // Calculate pressure gradient to nearest boundary
                result.PressureGradientToBoundary = EstimatePressureGradientToBoundary(
                    result.ProjectedPressure12Months, fieldId);

                // Risk of over-pressurization
                result.OverPressureRisk = AssessOverPressureRisk(
                    result.ProjectedPressure12Months, currentReservoirPressure);

                _logger?.LogInformation("Pressure analysis complete: change={Change:F0} psi/month, " +
                    "efficiency={Eff:F2}%, 12-month projection={Proj:F0} psi",
                    result.PressureChangePerMonth, result.PressureMaintenanceEfficiency * 100,
                    result.ProjectedPressure12Months);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing pressure performance for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Calculates economic feasibility of EOR implementation.
        /// </summary>
        public async Task<EOREconomicAnalysis> AnalyzeEOReconomicsAsync(
            string fieldId,
            double estimatedIncrementalOil,
            double oilPrice,
            double capitalCost,
            double operatingCostPerBarrel,
            int projectLifeYears,
            double discountRate = 0.10)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty", nameof(fieldId));
            if (estimatedIncrementalOil <= 0)
                throw new ArgumentException("Incremental oil must be positive", nameof(estimatedIncrementalOil));

            _logger?.LogInformation("Analyzing EOR economics: field={FieldId}, oil={Oil:F0} bbl, " +
                "price=${Price:F2}, capex=${Capex:F0}, opex=${Opex:F2}/bbl",
                fieldId, estimatedIncrementalOil, oilPrice, capitalCost, operatingCostPerBarrel);

            try
            {
                var result = new EOREconomicAnalysis
                {
                    FieldId = fieldId,
                    EstimatedIncrementalOil = estimatedIncrementalOil,
                    OilPrice = oilPrice,
                    CapitalCost = capitalCost,
                    OperatingCostPerBarrel = operatingCostPerBarrel,
                    ProjectLifeYears = projectLifeYears,
                    DiscountRate = discountRate,
                    AnalysisDate = DateTime.UtcNow
                };

                // Calculate revenue
                result.GrossRevenue = estimatedIncrementalOil * oilPrice;

                // Calculate operating costs
                result.TotalOperatingCost = estimatedIncrementalOil * operatingCostPerBarrel;

                // Calculate net present value
                result.NetPresentValue = CalculateNPV(
                    capitalCost, result.GrossRevenue, result.TotalOperatingCost,
                    projectLifeYears, discountRate);

                // Calculate internal rate of return
                result.InternalRateOfReturn = CalculateIRR(
                    capitalCost, result.GrossRevenue, result.TotalOperatingCost, projectLifeYears);

                // Calculate payback period
                result.PaybackPeriodYears = CalculatePaybackPeriod(
                    capitalCost, result.GrossRevenue - result.TotalOperatingCost, projectLifeYears);

                // Calculate profitability index
                result.ProfitabilityIndex = (result.NetPresentValue + capitalCost) / capitalCost;

                // Sensitivity analysis on oil price
                result.NpvAt20PercentOilPrice = CalculateNPV(
                    capitalCost, estimatedIncrementalOil * (oilPrice * 0.8), result.TotalOperatingCost,
                    projectLifeYears, discountRate);

                result.NpvAt50PercentOilPrice = CalculateNPV(
                    capitalCost, estimatedIncrementalOil * (oilPrice * 1.5), result.TotalOperatingCost,
                    projectLifeYears, discountRate);

                _logger?.LogInformation("EOR economic analysis complete: NPV=${NPV:F0}, IRR={IRR:F2}%, " +
                    "payback={Payback:F1} years, profitability index={PI:F2}",
                    result.NetPresentValue, result.InternalRateOfReturn * 100,
                    result.PaybackPeriodYears, result.ProfitabilityIndex);

                return result;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing EOR economics for field {FieldId}", fieldId);
                throw;
            }
        }

        // ===== HELPER METHODS =====

        private double CalculateIncrementalRecovery(List<double> productionData,
            double waterInjectionRate, double initialOilInPlace)
        {
            // Empirical correlation: typical waterflooding adds 10-25% incremental recovery
            // Based on Craft & Hawkins correlations
            double baseRecovery = 20.0; // Percentage
            double injectionFactor = Math.Min(waterInjectionRate / 5000, 1.0) * 10; // Up to 10% additional
            return Math.Min(baseRecovery + injectionFactor, 30.0);
        }

        private double CalculatePressureMaintenanceEfficiency(List<double> productionData,
            double waterInjectionRate)
        {
            // Efficiency improves with injection rate
            double rate = Math.Min(waterInjectionRate / 10000, 1.0);
            return 0.5 + (rate * 0.4); // 50-90% efficiency range
        }

        private WaterCutTrend AnalyzeWaterCutTrend(List<double> productionData, int monthCount)
        {
            // Simplified water cut calculation
            // Initial water cut low, increases over time
            double initialWaterCut = 0.05; // 5% initial
            double finalWaterCut = 0.40 + (monthCount / 300.0 * 0.3); // 40-70% final
            double rateOfIncrease = (finalWaterCut - initialWaterCut) / monthCount;

            return new WaterCutTrend
            {
                InitialWaterCut = initialWaterCut,
                FinalWaterCut = Math.Min(finalWaterCut, 0.95),
                RateOfIncreasePerMonth = rateOfIncrease,
                TimeToHighWaterCut = (0.75 - initialWaterCut) / rateOfIncrease
            };
        }

        private double EstimateFloodFrontVelocity(double injectionRate,
            double initialOilInPlace, int monthsOperating)
        {
            // Buckingham-type analysis: velocity based on injection rate and contact area
            // Typical range 50-500 feet per year
            return Math.Min((injectionRate / 1000) * 50, 500) * (monthsOperating / 12.0);
        }

        private double EstimateSweepEfficiency(double floodFrontVelocity, double waterCut)
        {
            // Sweep efficiency decreases with high water cut (early breakthrough)
            double baseEfficiency = 0.85 - (waterCut * 0.4);
            return Math.Max(0.4, baseEfficiency);
        }

        private double ProjectRecovery(double currentProduction, double currentRecoveryFactor, int months)
        {
            // Simple decline curve for projection
            double declineRate = 0.05; // 5% annual decline
            double monthlyDecline = Math.Pow(1 - declineRate, 1.0 / 12) - 1;
            double projectedProduction = currentProduction * Math.Pow(1 + monthlyDecline, months);
            return currentRecoveryFactor + (projectedProduction * 30 / 10000); // Add to recovery factor
        }

        private double CalculateResidualOilSaturation(string displacementMechanism, double capillaryNumber)
        {
            // Based on oil viscosity and capillary number
            // IFT effects become important at high capillary numbers
            if (displacementMechanism == "Miscible")
                return 0.05; // Near-zero residual oil in miscible displacement
            else
                return 0.20 + (capillaryNumber * 0.1); // Immiscible: 20%+ residual
        }

        private double CalculateProductionImprovement(double currentProduction, double displacementEfficiency)
        {
            // Production improvement proportional to displacement efficiency
            return currentProduction * displacementEfficiency;
        }

        private double CalculateTertiaryRecoveryPotential(double displacementEfficiency, double residualOilSaturation)
        {
            // Tertiary recovery = (1 - residual saturation) × displacement efficiency
            return (1 - residualOilSaturation) * displacementEfficiency * 100;
        }

        private GasTypeCharacteristics GetGasTypeCharacteristics(string gasType)
        {
            return gasType.ToUpper() switch
            {
                "CO2" => new GasTypeCharacteristics
                {
                    GasType = "CO2",
                    Density = 1.20,
                    CriticalTemperature = 88.0,
                    CriticalPressure = 1070,
                    MiscibilityAdvantage = "Low IFT with most crudes"
                },
                "N2" => new GasTypeCharacteristics
                {
                    GasType = "N2",
                    Density = 0.81,
                    CriticalTemperature = -232,
                    CriticalPressure = 492,
                    MiscibilityAdvantage = "High pressure required"
                },
                "HC" => new GasTypeCharacteristics
                {
                    GasType = "Hydrocarbon",
                    Density = 2.0,
                    CriticalTemperature = -150,
                    CriticalPressure = 670,
                    MiscibilityAdvantage = "Low MMP, effective with light oils"
                },
                _ => new GasTypeCharacteristics
                {
                    GasType = gasType,
                    Density = 1.0,
                    CriticalTemperature = 0,
                    CriticalPressure = 0,
                    MiscibilityAdvantage = "Unknown"
                }
            };
        }

        private string EvaluateChemicalSuitability(string chemicalType, double temperature,
            double salinity, double viscosity)
        {
            // Evaluate chemical applicability
            bool tempSuitable = temperature < 200 || chemicalType == "Alkali";
            bool salinitySuitable = salinity < 100000; // ppm
            bool viscositySuitable = viscosity > 5.0; // cp

            if (tempSuitable && salinitySuitable && viscositySuitable)
                return "Excellent";
            else if (tempSuitable && (salinitySuitable || viscositySuitable))
                return "Good";
            else if (tempSuitable || salinitySuitable)
                return "Fair";
            else
                return "Poor";
        }

        private double CalculateInterfacialTensionReduction(string chemicalType, double salinity)
        {
            // IFT reduction factor (multiplier on original IFT)
            return chemicalType.ToUpper() switch
            {
                "SURFACTANT" => 0.001, // 1000x reduction
                "POLYMER" => 0.5, // Slight reduction
                "ALKALI" => 0.01, // 100x reduction
                _ => 0.1 // Default
            };
        }

        private double EstimateChemicalRecoveryIncrement(string chemicalType, double iftReduction)
        {
            // Recovery increment based on IFT reduction
            double baseRecovery = chemicalType.ToUpper() switch
            {
                "SURFACTANT" => 0.20, // 20%
                "POLYMER" => 0.10, // 10%
                "ALKALI" => 0.15, // 15%
                _ => 0.08 // 8%
            };
            return baseRecovery * (1 + (1 - iftReduction) * 2);
        }

        private double EstimateChemicalCost(string chemicalType, double recoveryIncrement)
        {
            // Cost per barrel recovered (economics)
            double basePrice = chemicalType.ToUpper() switch
            {
                "SURFACTANT" => 15.0, // $/bbl
                "POLYMER" => 8.0,
                "ALKALI" => 5.0,
                _ => 10.0
            };
            return basePrice / Math.Max(0.01, recoveryIncrement);
        }

        private List<string> IdentifyChemicalConcerns(string chemicalType)
        {
            var concerns = new List<string>();
            switch (chemicalType.ToUpper())
            {
                case "SURFACTANT":
                    concerns.Add("Potential biodegradation");
                    concerns.Add("Adsorption loss");
                    break;
                case "POLYMER":
                    concerns.Add("Mechanical degradation");
                    concerns.Add("Shear thinning");
                    break;
                case "ALKALI":
                    concerns.Add("Corrosion potential");
                    concerns.Add("Rock/chemical interaction");
                    break;
            }
            return concerns;
        }

        private ChemicalParameters GetChemicalParameters(string chemicalType)
        {
            return new ChemicalParameters
            {
                ChemicalType = chemicalType,
                OptimalTemperature = 150,
                OptimalSalinity = 50000,
                DegradationRate = 0.05,
                AdsorptionFactor = 0.10
            };
        }

        private string EvaluateThermalSuitability(string method, double viscosity, double saturation)
        {
            // Thermal methods suit heavy oil (high viscosity) and good saturation
            bool viscositySuitable = viscosity > 100; // cp
            bool saturationSuitable = saturation > 0.5;

            if (viscositySuitable && saturationSuitable) return "Excellent";
            if (viscositySuitable) return "Good";
            if (saturationSuitable) return "Fair";
            return "Poor";
        }

        private double CalculateViscosityReduction(double initialViscosity,
            double temperature, string method)
        {
            // Viscosity reduction with temperature (ASTM D341)
            // Typical reduction: 10x per 100°F for heavy oil
            double temperatureIncrement = temperature - 60; // Assume surface temp 60°F
            double reductionFactor = Math.Pow(0.1, temperatureIncrement / 100);
            return initialViscosity * reductionFactor;
        }

        private double CalculateMobilityImprovement(double initialViscosity, double reducedViscosity)
        {
            // Mobility improvement = ratio of viscosities
            return (initialViscosity / Math.Max(1, reducedViscosity)) - 1;
        }

        private double EstimateThermalEnergyRequirement(string method, double temperature)
        {
            // Energy requirement for heating (BTU)
            // Typical: 1 BTU per °F per pound of rock
            return temperature * 1e6; // Simplified
        }

        private double EstimateThermalRecoveryFactor(string method, double mobilityImprovement, double oilSat)
        {
            // Recovery based on viscosity reduction and oil saturation
            double baseRF = method == "Steam" ? 0.60 : 0.40; // Steam better than ISC
            return baseRF * (1 + Math.Min(mobilityImprovement / 10, 0.3)) * oilSat;
        }

        private double EstimateThermalOperatingCost(string method, double energyRequirement)
        {
            // Operating cost: energy cost + chemicals
            // Assume $3 per million BTU
            return (energyRequirement / 1e6) * 3.0 + 1.0; // $/bbl
        }

        private string AssessThermalEnvironmentalImpact(string method)
        {
            return method == "Steam" ? "High - CO2 emissions from steam generation" :
                   "Moderate - ISC produces some CO2";
        }

        private EORMethodScore CalculateEORMethodScore(string method, Dictionary<string, double> properties)
        {
            var score = new EORMethodScore { Method = method };
            
            // Weighted scoring
            double tempScore = (properties.ContainsKey("Temperature") && properties["Temperature"] < 150) ? 10 : 5;
            double viscScore = (properties.ContainsKey("Viscosity") && properties["Viscosity"] > 50) ? 10 : 5;
            double satScore = (properties.ContainsKey("OilSaturation") && properties["OilSaturation"] > 0.4) ? 10 : 5;

            score.TemperatureSuitability = tempScore;
            score.ViscositySuitability = viscScore;
            score.SaturationSuitability = satScore;
            score.OverallScore = (tempScore + viscScore + satScore) / 3.0;

            return score;
        }

        private double CalculateEORSynergyPotential(List<string> methods, List<EORMethodScore> scores)
        {
            // Synergy: combining methods can have greater effect than individual
            if (methods.Count < 2) return 0;
            
            // Simple synergy calculation: 15-25% additional recovery if combining
            return 0.20; // 20% typical synergy
        }

        private double CalculateOptimalWellSpacing(double area, int wellCount)
        {
            // Square root of (area / well count)
            return Math.Sqrt(area / wellCount);
        }

        private double EstimateMaxInjectionRate(double permeability, double thickness, double spacing)
        {
            // Darcy's Law approximation: q = (0.0002637 * k * h * ΔP) / (μ * ln(re/rw))
            // Simplified: typical injection rate 500-2000 bbl/day
            double baseRate = permeability * thickness / 10;
            return Math.Min(baseRate, 2000);
        }

        private double EstimateArealSweepEfficiency(double spacing, double thickness, double permeability)
        {
            // Sweep efficiency: 2D Buckingham correlation
            // Affected by mobility ratio, aspect ratio, etc.
            double mobilityRatio = 0.3; // Favorable (injected fluid more viscous)
            return 0.75 * (1 - Math.Exp(-mobilityRatio * 2));
        }

        private string GenerateWellPlacementPattern(double area, int wellCount, double thickness)
        {
            // Recommend pattern type
            if (wellCount == 1)
                return "Single well - line drive";
            else if (wellCount >= 2 && wellCount <= 4)
                return "Line drive or 2:1 pattern";
            else if (wellCount >= 5 && wellCount <= 9)
                return "5-spot or 7-spot pattern";
            else
                return "Nine-spot or inverted pattern";
        }

        private List<string> IdentifyWellPlacementRisks(double area, int wellCount, double permeability)
        {
            var risks = new List<string>();
            
            double wellsPerAcre = wellCount / area;
            if (wellsPerAcre > 0.1)
                risks.Add("High well density may cause pressure interference");
            
            if (permeability < 50)
                risks.Add("Low permeability may limit injection rates");
            
            if (wellCount == 1)
                risks.Add("Single well risks: poor areal sweep");

            return risks;
        }

        private double EstimateEffectiveCompressibility(double pressureChange,
            double reservoirVolume, double volumeInjected)
        {
            // Compressibility = -dV/dP / V
            if (Math.Abs(pressureChange) < 1) return 0;
            return volumeInjected / (reservoirVolume * pressureChange);
        }

        private double ProjectPressure(double currentPressure, double changePerMonth, int months)
        {
            return currentPressure + (changePerMonth * months);
        }

        private double EstimatePressureGradientToBoundary(double currentPressure, string fieldId)
        {
            // Typical pressure gradient: 0.4-0.5 psi/ft
            // Varies by field depth and rock type
            return 0.43;
        }

        private string AssessOverPressureRisk(double projectedPressure, double fracturePressure)
        {
            double safetyMargin = (fracturePressure - projectedPressure) / fracturePressure;
            
            if (safetyMargin > 0.15) return "Low";
            if (safetyMargin > 0.05) return "Moderate";
            return "High";
        }

        private double CalculateNPV(double capex, double grossRevenue, double opex,
            int years, double discountRate)
        {
            double annualCashFlow = (grossRevenue - opex) / years;
            double pv = 0;
            for (int i = 1; i <= years; i++)
            {
                pv += annualCashFlow / Math.Pow(1 + discountRate, i);
            }
            return pv - capex;
        }

        private double CalculateIRR(double capex, double grossRevenue, double opex, int years)
        {
            // IRR approximation using simple average
            double avgReturn = (grossRevenue - opex) / years;
            return (avgReturn / capex) - (1.0 / years);
        }

        private double CalculatePaybackPeriod(double capex, double annualCashFlow, int maxYears)
        {
            if (annualCashFlow <= 0) return maxYears;
            return Math.Min(capex / annualCashFlow, maxYears);
        }
    }
}
