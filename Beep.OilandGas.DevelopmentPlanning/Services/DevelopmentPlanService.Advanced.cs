using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.Calculations;
using Microsoft.Extensions.Logging;

namespace Beep.OilandGas.DevelopmentPlanning.Services
{
    /// <summary>
    /// Advanced development planning analysis methods.
    /// Provides comprehensive field development strategy, reserves estimation, drilling program,
    /// infrastructure planning, cost budgeting, scheduling, risk analysis, and investment evaluation.
    /// </summary>
    public partial class DevelopmentPlanService
    {
        private readonly ILogger<DevelopmentPlanService>? _logger;

        /// <summary>
        /// Analyzes field development strategy based on field characteristics, reserves, and market conditions.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="fieldArea">Field area in square kilometers</param>
        /// <param name="estimatedReserves">Estimated oil/gas reserves in MMbbl or BCF</param>
        /// <param name="depthRange">Average well depth in meters</param>
        /// <param name="reservoirType">Type of reservoir (e.g., Sandstone, Carbonate, Shale)</param>
        /// <returns>Development strategy analysis with recommendations</returns>
        public async Task<FieldDevelopmentStrategyResult> AnalyzeFieldDevelopmentStrategyAsync(
            string fieldId,
            double fieldArea,
            double estimatedReserves,
            int depthRange,
            string reservoirType)
        {
            try
            {
                _logger?.LogInformation("Starting field development strategy analysis for field {FieldId}, area={Area}km2, reserves={Reserves}MMbbl, depth={Depth}m, type={Type}",
                    fieldId, fieldArea, estimatedReserves, depthRange, reservoirType);

                ValidateStrategyInputs(fieldId, fieldArea, estimatedReserves, depthRange);

                var strategy = new FieldDevelopmentStrategyResult
                {
                    FieldId = fieldId,
                    AnalysisDate = DateTime.UtcNow,
                    FieldArea = fieldArea,
                    EstimatedReserves = estimatedReserves,
                    ReservoirType = reservoirType
                };

                // Determine development approach based on field characteristics
                strategy.RecommendedApproach = DetermineDevApproach(fieldArea, estimatedReserves, depthRange);
                strategy.WellSpacing = CalculateOptimalWellSpacing(fieldArea, estimatedReserves, reservoirType);
                strategy.PhaseCount = DeterminePhaseCount(estimatedReserves);
                strategy.AnnualProduction = CalculateAnnualProduction(estimatedReserves, strategy.PhaseCount);
                strategy.Complexity = AssessComplexity(depthRange, reservoirType, fieldArea);
                strategy.RiskLevel = AssessRiskLevel(reservoirType, depthRange, fieldArea);
                strategy.RecommendedTechnologies = SelectRecommendedTechnologies(reservoirType, depthRange);
                strategy.InfrastructureRequirements = DetermineInfrastructureRequirements(strategy.WellSpacing, strategy.AnnualProduction);

                _logger?.LogInformation("Field development strategy analysis complete for field {FieldId}: approach={Approach}, spacing={Spacing}acres, phases={Phases}",
                    fieldId, strategy.RecommendedApproach, strategy.WellSpacing, strategy.PhaseCount);

                return await Task.FromResult(strategy);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analyzing field development strategy for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Estimates recoverable reserves based on geology, seismic data, and well performance.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="initialReserves">Initial reserve estimate in MMbbl/BCF</param>
        /// <param name="recoveryFactor">Expected recovery factor (0.0-1.0)</param>
        /// <param name="productionHistory">Historical production data points</param>
        /// <returns>Detailed reserve estimation with confidence intervals</returns>
        public async Task<RecoverableReservesEstimate> EstimateRecoverableReservesAsync(
            string fieldId,
            double initialReserves,
            double recoveryFactor,
            List<ProductionDataPoint> productionHistory)
        {
            try
            {
                _logger?.LogInformation("Estimating recoverable reserves for field {FieldId}, initial={Initial}MMbbl, RF={RF}, data_points={Points}",
                    fieldId, initialReserves, recoveryFactor, productionHistory?.Count ?? 0);

                ValidateReserveInputs(fieldId, initialReserves, recoveryFactor);

                var estimate = new RecoverableReservesEstimate
                {
                    FieldId = fieldId,
                    EstimationDate = DateTime.UtcNow,
                    InitialReserves = initialReserves,
                    RecoveryFactor = recoveryFactor,
                    RecoverableReserves = initialReserves * recoveryFactor
                };

                // Calculate production history trend if available
                if (productionHistory != null && productionHistory.Count > 0)
                {
                    estimate.ProductionTrend = AnalyzeProductionTrend(productionHistory);
                    estimate.CumulativeProduction = productionHistory.Sum(p => p.MonthlyProduction);
                    estimate.RemainingReserves = estimate.RecoverableReserves - estimate.CumulativeProduction;
                    estimate.ProductionDecline = CalculateDeclineRate(productionHistory);
                }
                else
                {
                    estimate.RemainingReserves = estimate.RecoverableReserves;
                }

                // Confidence intervals (Monte Carlo style)
                estimate.P90Estimate = estimate.RecoverableReserves * 0.9;  // Conservative
                estimate.P50Estimate = estimate.RecoverableReserves * 1.0;  // Most likely
                estimate.P10Estimate = estimate.RecoverableReserves * 1.15; // Optimistic

                estimate.UncertaintyRange = new UncertaintyRange
                {
                    LowCase = estimate.P90Estimate,
                    BaseCase = estimate.P50Estimate,
                    HighCase = estimate.P10Estimate
                };

                _logger?.LogInformation("Reserve estimation complete for field {FieldId}: recoverable={Recoverable}MMbbl, remaining={Remaining}MMbbl, P90/P50/P10={P90}/{P50}/{P10}",
                    fieldId, estimate.RecoverableReserves, estimate.RemainingReserves, estimate.P90Estimate, estimate.P50Estimate, estimate.P10Estimate);

                return await Task.FromResult(estimate);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error estimating recoverable reserves for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Generates a comprehensive drilling program with well locations, sequence, and specifications.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="totalWellsPlanned">Total number of wells to be drilled</param>
        /// <param name="phaseDuration">Expected duration of drilling phase in months</param>
        /// <param name="wellTypeDistribution">Distribution of well types (e.g., vertical, deviated, horizontal)</param>
        /// <returns>Drilling program with well schedule and specifications</returns>
        public async Task<DrillingProgramResult> GenerateDrillingProgramAsync(
            string fieldId,
            int totalWellsPlanned,
            int phaseDuration,
            Dictionary<string, int> wellTypeDistribution)
        {
            try
            {
                _logger?.LogInformation("Generating drilling program for field {FieldId}, wells={Total}, duration={Months}months, types={Types}",
                    fieldId, totalWellsPlanned, phaseDuration, string.Join(",", wellTypeDistribution?.Keys ?? Array.Empty<string>()));

                ValidateDrillingInputs(fieldId, totalWellsPlanned, phaseDuration);

                var program = new DrillingProgramResult
                {
                    FieldId = fieldId,
                    ProgramDate = DateTime.UtcNow,
                    TotalWellsPlanned = totalWellsPlanned,
                    PhaseDuration = phaseDuration,
                    WellTypeDistribution = wellTypeDistribution ?? new Dictionary<string, int>()
                };

                // Calculate drilling capacity and schedule
                program.RigsRequired = CalculateRigsRequired(totalWellsPlanned, phaseDuration);
                program.AverageDaysPerWell = CalculateAverageDaysPerWell(phaseDuration, totalWellsPlanned);
                program.SpudSchedule = GenerateSpudSchedule(totalWellsPlanned, phaseDuration);
                program.CompletionSchedule = GenerateCompletionSchedule(program.SpudSchedule, program.AverageDaysPerWell);
                program.OperationalChallenges = IdentifyOperationalChallenges(program.RigsRequired, totalWellsPlanned);
                program.LogisticsRequirements = DetermineLogisticsNeeds(program.RigsRequired, totalWellsPlanned);
                program.EnvironmentalConsiderations = IdentifyEnvironmentalConsiderations(program.RigsRequired);

                _logger?.LogInformation("Drilling program generated for field {FieldId}: rigs={Rigs}, days/well={DaysPerWell}, spud_count={SpudCount}",
                    fieldId, program.RigsRequired, program.AverageDaysPerWell, program.SpudSchedule?.Count ?? 0);

                return await Task.FromResult(program);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating drilling program for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Plans infrastructure requirements including processing, transportation, and facilities.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="peakProduction">Peak production rate in bbl/d or MMcf/d</param>
        /// <param name="productType">Type of product (oil, gas, condensate)</param>
        /// <param name="distanceToMarket">Distance to market in kilometers</param>
        /// <returns>Infrastructure planning with facility specifications</returns>
        public async Task<InfrastructurePlanningResult> PlanInfrastructureRequirementsAsync(
            string fieldId,
            double peakProduction,
            string productType,
            double distanceToMarket)
        {
            try
            {
                _logger?.LogInformation("Planning infrastructure for field {FieldId}, peak_prod={Peak}{Type}, distance={Distance}km",
                    fieldId, peakProduction, productType, distanceToMarket);

                ValidateInfrastructureInputs(fieldId, peakProduction, distanceToMarket);

                var infrastructure = new InfrastructurePlanningResult
                {
                    FieldId = fieldId,
                    PlanningDate = DateTime.UtcNow,
                    PeakProduction = peakProduction,
                    ProductType = productType,
                    DistanceToMarket = distanceToMarket
                };

                // Size processing facilities
                infrastructure.ProcessingFacility = SizeProcessingFacility(peakProduction, productType);
                infrastructure.StorageRequirements = DetermineStorageNeeds(peakProduction, productType);
                infrastructure.TransportationMethod = SelectTransportationMethod(distanceToMarket, peakProduction);
                infrastructure.PipelineSpecifications = DesignPipelineSystem(peakProduction, distanceToMarket, productType);
                infrastructure.PowerRequirements = CalculatePowerRequirements(infrastructure.ProcessingFacility);
                infrastructure.WaterHandling = DetermineWaterHandlingNeeds(peakProduction);
                infrastructure.SafetySystemsRequired = IdentifySafetySystems(productType, peakProduction);
                infrastructure.EnvironmentalControlsRequired = IdentifyEnvironmentalControls(productType);

                _logger?.LogInformation("Infrastructure planning complete for field {FieldId}: transport_method={Method}, storage={Storage}bbl, power={Power}kW",
                    fieldId, infrastructure.TransportationMethod, infrastructure.StorageRequirements.CrudeOilStorage, infrastructure.PowerRequirements);

                return await Task.FromResult(infrastructure);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error planning infrastructure for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Estimates capital and operating costs for field development over project lifecycle.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="wellCount">Number of wells to be drilled</param>
        /// <param name="drillingCostPerWell">Average drilling cost per well in USD millions</param>
        /// <param name="facilityCapex">Estimated facility CAPEX in USD millions</param>
        /// <returns>Comprehensive cost analysis with breakdown and escalation</returns>
        public async Task<DevelopmentCostAnalysisResult> EstimateDevelopmentCostsAsync(
            string fieldId,
            int wellCount,
            double drillingCostPerWell,
            double facilityCapex)
        {
            try
            {
                _logger?.LogInformation("Estimating development costs for field {FieldId}, wells={Wells}, drill_cost={DrillCost}M, facility={Facility}M",
                    fieldId, wellCount, drillingCostPerWell, facilityCapex);

                ValidateCostInputs(fieldId, wellCount, drillingCostPerWell);

                var costAnalysis = new DevelopmentCostAnalysisResult
                {
                    FieldId = fieldId,
                    AnalysisDate = DateTime.UtcNow,
                    WellCount = wellCount,
                    DrillingCostPerWell = drillingCostPerWell,
                    FacilityCapex = facilityCapex
                };

                // Calculate cost components
                costAnalysis.TotalDrillingCost = wellCount * drillingCostPerWell;
                costAnalysis.CompletionCost = CalculateCompletionCost(wellCount, drillingCostPerWell);
                costAnalysis.EhsAndPermits = CalculateEHSCosts(wellCount);
                costAnalysis.TotalCapex = costAnalysis.TotalDrillingCost + costAnalysis.CompletionCost + 
                                         costAnalysis.EhsAndPermits + facilityCapex;

                // Operating costs
                costAnalysis.AnnualLaborCost = CalculateAnnualLaborCost(wellCount);
                costAnalysis.AnnualMaterialsCost = CalculateAnnualMaterialsCost(wellCount);
                costAnalysis.AnnualMaintenance = CalculateAnnualMaintenanceCost(costAnalysis.TotalCapex);
                costAnalysis.AnnualOpex = costAnalysis.AnnualLaborCost + costAnalysis.AnnualMaterialsCost + costAnalysis.AnnualMaintenance;

                // Cost escalation
                costAnalysis.CostEscalationFactors = CalculateCostEscalation();
                costAnalysis.ContingencyAllowance = costAnalysis.TotalCapex * 0.15;  // 15% contingency
                costAnalysis.TotalProjectCost = costAnalysis.TotalCapex + costAnalysis.ContingencyAllowance;

                // Cost breakdown by category
                costAnalysis.CostBreakdown = CreateCostBreakdown(costAnalysis);

                _logger?.LogInformation("Cost analysis complete for field {FieldId}: capex={Capex}M, opex_annual={Opex}M, total={Total}M",
                    fieldId, costAnalysis.TotalCapex, costAnalysis.AnnualOpex, costAnalysis.TotalProjectCost);

                return await Task.FromResult(costAnalysis);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error estimating development costs for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Optimizes production scheduling to maximize economic return while managing operational constraints.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="productionCapacity">Field production capacity in bbl/d or MMcf/d</param>
        /// <param name="marketPrice">Current commodity price in USD/unit</param>
        /// <param name="operatingCostPerUnit">Operating cost per unit produced in USD</param>
        /// <returns>Optimized production schedule with economic metrics</returns>
        public async Task<ProductionScheduleOptimizationResult> OptimizeProductionScheduleAsync(
            string fieldId,
            double productionCapacity,
            double marketPrice,
            double operatingCostPerUnit)
        {
            try
            {
                _logger?.LogInformation("Optimizing production schedule for field {FieldId}, capacity={Capacity}, price={Price}/unit, opex={Opex}/unit",
                    fieldId, productionCapacity, marketPrice, operatingCostPerUnit);

                ValidateScheduleInputs(fieldId, productionCapacity, marketPrice);

                var schedule = new ProductionScheduleOptimizationResult
                {
                    FieldId = fieldId,
                    OptimizationDate = DateTime.UtcNow,
                    ProductionCapacity = productionCapacity,
                    MarketPrice = marketPrice,
                    OperatingCostPerUnit = operatingCostPerUnit
                };

                // Generate production schedule phases
                schedule.ProductionPhases = GenerateProductionPhases(productionCapacity);
                schedule.MonthlySchedule = GenerateMonthlySchedule(schedule.ProductionPhases);
                
                // Calculate economics
                schedule.Breakeven = operatingCostPerUnit;
                schedule.Margin = marketPrice - operatingCostPerUnit;
                schedule.MonthlyRevenue = new List<double>();
                schedule.MonthlyCashFlow = new List<double>();

                foreach (var month in schedule.MonthlySchedule)
                {
                    var revenue = month * marketPrice;
                    var cost = month * operatingCostPerUnit;
                    var cashflow = revenue - cost;
                    
                    schedule.MonthlyRevenue.Add(revenue);
                    schedule.MonthlyCashFlow.Add(cashflow);
                }

                schedule.TotalProjectRevenue = schedule.MonthlyRevenue.Sum();
                schedule.TotalProjectCost = schedule.MonthlyCashFlow.Sum(cf => -cf * (cf < 0 ? 1 : 0));
                schedule.NetCashFlow = schedule.MonthlyRevenue.Sum() - (schedule.MonthlyRevenue.Sum() - schedule.MonthlyCashFlow.Sum());

                _logger?.LogInformation("Production schedule optimization complete for field {FieldId}: phases={Phases}, revenue={Revenue}M, cashflow={Cashflow}M",
                    fieldId, schedule.ProductionPhases?.Count ?? 0, schedule.TotalProjectRevenue, schedule.NetCashFlow);

                return await Task.FromResult(schedule);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error optimizing production schedule for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Assesses environmental and social impact of field development activities.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="location">Field location description</param>
        /// <param name="environmentalSensitivity">Environmental sensitivity level (Low, Medium, High)</param>
        /// <returns>Environmental impact assessment with mitigation measures</returns>
        public async Task<EnvironmentalImpactAssessmentResult> AssessEnvironmentalImpactAsync(
            string fieldId,
            string location,
            string environmentalSensitivity)
        {
            try
            {
                _logger?.LogInformation("Assessing environmental impact for field {FieldId}, location={Location}, sensitivity={Sensitivity}",
                    fieldId, location, environmentalSensitivity);

                ValidateEnvironmentalInputs(fieldId, location);

                var assessment = new EnvironmentalImpactAssessmentResult
                {
                    FieldId = fieldId,
                    AssessmentDate = DateTime.UtcNow,
                    Location = location,
                    EnvironmentalSensitivity = environmentalSensitivity
                };

                // Identify impact categories
                assessment.AirQualityImpact = AssessAirQualityImpact(environmentalSensitivity);
                assessment.WaterQualityImpact = AssessWaterQualityImpact(environmentalSensitivity, location);
                assessment.SoilAndLandImpact = AssessSoilAndLandImpact(environmentalSensitivity);
                assessment.BiodiversityImpact = AssessBiodiversityImpact(environmentalSensitivity, location);
                assessment.GHGEmissions = EstimateGHGEmissions(assessment);
                assessment.MitigationMeasures = DevelopMitigationMeasures(assessment);
                assessment.ComplianceRequirements = IdentifyComplianceRequirements(location);
                assessment.CostOfMitigation = CalculateMitigationCost(assessment.MitigationMeasures);

                _logger?.LogInformation("Environmental assessment complete for field {FieldId}: air={Air}, water={Water}, ghg={GHG}tCO2e, mitigation_cost={Cost}M",
                    fieldId, assessment.AirQualityImpact, assessment.WaterQualityImpact, assessment.GHGEmissions, assessment.CostOfMitigation);

                return await Task.FromResult(assessment);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error assessing environmental impact for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Performs comprehensive risk analysis for field development project.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="projectDuration">Expected project duration in years</param>
        /// <param name="projectedNPV">Projected project NPV in USD millions</param>
        /// <returns>Risk analysis with identified risks and mitigation strategies</returns>
        public async Task<RiskAnalysisResult> PerformRiskAnalysisAsync(
            string fieldId,
            int projectDuration,
            double projectedNPV)
        {
            try
            {
                _logger?.LogInformation("Performing risk analysis for field {FieldId}, duration={Years}years, NPV={NPV}M",
                    fieldId, projectDuration, projectedNPV);

                ValidateRiskInputs(fieldId, projectDuration);

                var riskAnalysis = new RiskAnalysisResult
                {
                    FieldId = fieldId,
                    AnalysisDate = DateTime.UtcNow,
                    ProjectDuration = projectDuration,
                    ProjectedNPV = projectedNPV
                };

                // Identify risks across categories
                riskAnalysis.TechnicalRisks = IdentifyTechnicalRisks();
                riskAnalysis.CommercialRisks = IdentifyCommercialRisks();
                riskAnalysis.OperationalRisks = IdentifyOperationalRisks();
                riskAnalysis.RegulatoryRisks = IdentifyRegulatoryRisks();
                riskAnalysis.EnvironmentalRisks = IdentifyEnvironmentalRisks();

                // Calculate risk metrics
                riskAnalysis.TotalIdentifiedRisks = (riskAnalysis.TechnicalRisks?.Count ?? 0) +
                                                   (riskAnalysis.CommercialRisks?.Count ?? 0) +
                                                   (riskAnalysis.OperationalRisks?.Count ?? 0) +
                                                   (riskAnalysis.RegulatoryRisks?.Count ?? 0) +
                                                   (riskAnalysis.EnvironmentalRisks?.Count ?? 0);

                riskAnalysis.HighPriorityRisks = CalculateHighPriorityRisks(riskAnalysis);
                riskAnalysis.MitigationStrategies = DevelopRiskMitigationStrategies(riskAnalysis);
                riskAnalysis.ContingencyReserve = CalculateContingencyReserve(projectedNPV);
                riskAnalysis.OverallRiskRating = CalculateOverallRiskRating(riskAnalysis);

                _logger?.LogInformation("Risk analysis complete for field {FieldId}: total_risks={Total}, high_priority={High}, contingency={Contingency}M, rating={Rating}",
                    fieldId, riskAnalysis.TotalIdentifiedRisks, riskAnalysis.HighPriorityRisks.Count, riskAnalysis.ContingencyReserve, riskAnalysis.OverallRiskRating);

                return await Task.FromResult(riskAnalysis);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error performing risk analysis for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Designs facility specifications including production equipment and processing systems.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="productionRate">Design production rate in bbl/d or MMcf/d</param>
        /// <param name="productType">Type of production (oil, gas, condensate)</param>
        /// <returns>Facility design with equipment specifications</returns>
        public async Task<FacilityDesignResult> GenerateFacilityDesignAsync(
            string fieldId,
            double productionRate,
            string productType)
        {
            try
            {
                _logger?.LogInformation("Generating facility design for field {FieldId}, rate={Rate}{Type}",
                    fieldId, productionRate, productType);

                ValidateFacilityInputs(fieldId, productionRate);

                var design = new FacilityDesignResult
                {
                    FieldId = fieldId,
                    DesignDate = DateTime.UtcNow,
                    ProductionRate = productionRate,
                    ProductType = productType
                };

                // Size process equipment
                design.SeparatorSpecifications = DesignSeparators(productionRate, productType);
                design.CompressorRequirements = DesignCompressors(productionRate);
                design.PumpingRequirements = DesignPumps(productionRate, productType);
                design.HeatExchangerSpecifications = DesignHeatExchangers(productionRate);
                design.ControlSystemsSpecification = DesignControlSystems();
                design.LaySgDownRequirements = DesignLayDownAreas(productionRate);
                design.UtilityRequirements = DesignUtilityNeeds(productionRate);
                design.CostEstimate = EstimateFacilityCost(design);

                _logger?.LogInformation("Facility design complete for field {FieldId}: sep_count={Seps}, compressors={Comps}, cost={Cost}M",
                    fieldId, design.SeparatorSpecifications?.Count ?? 0, design.CompressorRequirements?.Count ?? 0, design.CostEstimate);

                return await Task.FromResult(design);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating facility design for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Evaluates investment metrics and economic viability of the development project.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="initialCapex">Initial capital expenditure in USD millions</param>
        /// <param name="projectedCashFlows">Annual cash flows for project lifetime in USD millions</param>
        /// <param name="discountRate">Discount rate for NPV calculation (0.0-1.0)</param>
        /// <returns>Investment evaluation with key economic metrics</returns>
        public async Task<InvestmentEvaluationResult> EvaluateInvestmentAsync(
            string fieldId,
            double initialCapex,
            List<double> projectedCashFlows,
            double discountRate)
        {
            try
            {
                _logger?.LogInformation("Evaluating investment for field {FieldId}, capex={Capex}M, cashflows={CFCount}years, discount_rate={Rate}%",
                    fieldId, initialCapex, projectedCashFlows?.Count ?? 0, discountRate * 100);

                ValidateInvestmentInputs(fieldId, initialCapex, discountRate);

                var evaluation = new InvestmentEvaluationResult
                {
                    FieldId = fieldId,
                    EvaluationDate = DateTime.UtcNow,
                    InitialCapex = initialCapex,
                    DiscountRate = discountRate,
                    ProjectedCashFlows = projectedCashFlows ?? new List<double>()
                };

                // Calculate investment metrics
                evaluation.NPV = CalculateNPV(initialCapex, projectedCashFlows, discountRate);
                evaluation.IRR = CalculateIRR(initialCapex, projectedCashFlows);
                evaluation.PaybackPeriod = CalculatePaybackPeriod(initialCapex, projectedCashFlows);
                evaluation.ProfitabilityIndex = CalculateProfitabilityIndex(evaluation.NPV, initialCapex);
                evaluation.TotalProjectValue = initialCapex + evaluation.NPV;
                evaluation.SensitivityAnalysis = PerformSensitivityAnalysis(evaluation);
                evaluation.ScenarioAnalysis = PerformScenarioAnalysis(evaluation);
                evaluation.InvestmentRating = RateInvestment(evaluation);

                _logger?.LogInformation("Investment evaluation complete for field {FieldId}: NPV={NPV}M, IRR={IRR}%, payback={Payback}years, rating={Rating}",
                    fieldId, evaluation.NPV, evaluation.IRR * 100, evaluation.PaybackPeriod, evaluation.InvestmentRating);

                return await Task.FromResult(evaluation);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error evaluating investment for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Creates detailed development phase schedule with key milestones and dependencies.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="developmentStartDate">Planned development start date</param>
        /// <param name="phaseCount">Number of development phases</param>
        /// <returns>Detailed phase schedule with milestones and dependencies</returns>
        public async Task<DevelopmentPhaseScheduleResult> CreateDevelopmentPhaseScheduleAsync(
            string fieldId,
            DateTime developmentStartDate,
            int phaseCount)
        {
            try
            {
                _logger?.LogInformation("Creating development phase schedule for field {FieldId}, start_date={StartDate:yyyy-MM-dd}, phases={Phases}",
                    fieldId, developmentStartDate, phaseCount);

                ValidateScheduleInputs(fieldId, phaseCount);

                var schedule = new DevelopmentPhaseScheduleResult
                {
                    FieldId = fieldId,
                    ScheduleDate = DateTime.UtcNow,
                    DevelopmentStartDate = developmentStartDate,
                    PhaseCount = phaseCount,
                    Phases = new List<DevelopmentPhase>()
                };

                // Generate phases with milestones
                var currentDate = developmentStartDate;
                for (int i = 1; i <= phaseCount; i++)
                {
                    var phase = new DevelopmentPhase
                    {
                        PhaseNumber = i,
                        PhaseName = $"Phase {i}",
                        StartDate = currentDate,
                        Duration = CalculatePhaseDuration(i, phaseCount),
                        EndDate = currentDate.AddMonths(CalculatePhaseDuration(i, phaseCount)),
                        Milestones = GeneratePhaseMilestones(i, currentDate),
                        Dependencies = GeneratePhaseDependencies(i)
                    };

                    schedule.Phases.Add(phase);
                    currentDate = phase.EndDate.AddMonths(1);
                }

                schedule.TotalProjectDuration = (int)(schedule.Phases.Last().EndDate - developmentStartDate).TotalMonths;
                schedule.CriticalPath = IdentifyCriticalPath(schedule.Phases);
                schedule.ProjectCompletion = schedule.Phases.Last().EndDate;

                _logger?.LogInformation("Development phase schedule created for field {FieldId}: completion={Completion:yyyy-MM-dd}, duration={Duration}months",
                    fieldId, schedule.ProjectCompletion, schedule.TotalProjectDuration);

                return await Task.FromResult(schedule);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating development phase schedule for field {FieldId}", fieldId);
                throw;
            }
        }

        /// <summary>
        /// Compares alternative development strategies with cost and economic trade-offs.
        /// </summary>
        /// <param name="fieldId">The field identifier</param>
        /// <param name="strategies">List of alternative strategies to compare</param>
        /// <returns>Comparison analysis with recommendations</returns>
        public async Task<StrategiesComparisonResult> CompareAlternativeStrategiesAsync(
            string fieldId,
            List<DevelopmentStrategy> strategies)
        {
            try
            {
                _logger?.LogInformation("Comparing {StrategyCount} alternative strategies for field {FieldId}",
                    strategies?.Count ?? 0, fieldId);

                ValidateStrategyComparison(fieldId, strategies);

                var comparison = new StrategiesComparisonResult
                {
                    FieldId = fieldId,
                    ComparisonDate = DateTime.UtcNow,
                    StrategyCount = strategies.Count,
                    Strategies = strategies
                };

                // Compare each strategy
                comparison.CostComparison = CompareCosts(strategies);
                comparison.EconomicComparison = CompareEconomics(strategies);
                comparison.RiskComparison = CompareRisks(strategies);
                comparison.ScheduleComparison = CompareSchedules(strategies);
                comparison.EnvironmentalComparison = CompareEnvironmental(strategies);
                comparison.RecommendedStrategy = SelectBestStrategy(strategies);
                comparison.AlternativeRankings = RankStrategies(strategies);
                comparison.TradeOffAnalysis = PerformTradeOffAnalysis(strategies);

                _logger?.LogInformation("Strategy comparison complete for field {FieldId}: recommended={Recommended}, ranking_count={Rankings}",
                    fieldId, comparison.RecommendedStrategy, comparison.AlternativeRankings?.Count ?? 0);

                return await Task.FromResult(comparison);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error comparing alternative strategies for field {FieldId}", fieldId);
                throw;
            }
        }

        // ==================== VALIDATION METHODS ====================

        private void ValidateStrategyInputs(string fieldId, double fieldArea, double reserves, int depth)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (fieldArea <= 0)
                throw new ArgumentException("Field area must be greater than 0.", nameof(fieldArea));
            if (reserves <= 0)
                throw new ArgumentException("Reserves must be greater than 0.", nameof(reserves));
            if (depth <= 0)
                throw new ArgumentException("Depth must be greater than 0.", nameof(depth));
        }

        private void ValidateReserveInputs(string fieldId, double reserves, double rf)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (reserves <= 0)
                throw new ArgumentException("Initial reserves must be greater than 0.", nameof(reserves));
            if (rf <= 0 || rf > 1.0)
                throw new ArgumentException("Recovery factor must be between 0 and 1.", nameof(rf));
        }

        private void ValidateDrillingInputs(string fieldId, int wells, int duration)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (wells <= 0)
                throw new ArgumentException("Well count must be greater than 0.", nameof(wells));
            if (duration <= 0)
                throw new ArgumentException("Phase duration must be greater than 0.", nameof(duration));
        }

        private void ValidateInfrastructureInputs(string fieldId, double production, double distance)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (production <= 0)
                throw new ArgumentException("Production must be greater than 0.", nameof(production));
            if (distance < 0)
                throw new ArgumentException("Distance cannot be negative.", nameof(distance));
        }

        private void ValidateCostInputs(string fieldId, int wells, double cost)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (wells <= 0)
                throw new ArgumentException("Well count must be greater than 0.", nameof(wells));
            if (cost < 0)
                throw new ArgumentException("Cost cannot be negative.", nameof(cost));
        }

        private void ValidateScheduleInputs(string fieldId, double production, double price)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (production <= 0)
                throw new ArgumentException("Production capacity must be greater than 0.", nameof(production));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));
        }

        private void ValidateScheduleInputs(string fieldId, int phases)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (phases <= 0)
                throw new ArgumentException("Phase count must be greater than 0.", nameof(phases));
        }

        private void ValidateEnvironmentalInputs(string fieldId, string location)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (string.IsNullOrWhiteSpace(location))
                throw new ArgumentException("Location cannot be null or empty.", nameof(location));
        }

        private void ValidateRiskInputs(string fieldId, int duration)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (duration <= 0)
                throw new ArgumentException("Project duration must be greater than 0.", nameof(duration));
        }

        private void ValidateFacilityInputs(string fieldId, double rate)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (rate <= 0)
                throw new ArgumentException("Production rate must be greater than 0.", nameof(rate));
        }

        private void ValidateInvestmentInputs(string fieldId, double capex, double discount)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (capex < 0)
                throw new ArgumentException("CAPEX cannot be negative.", nameof(capex));
            if (discount < 0 || discount > 1.0)
                throw new ArgumentException("Discount rate must be between 0 and 1.", nameof(discount));
        }

        private void ValidateStrategyComparison(string fieldId, List<DevelopmentStrategy> strategies)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentException("Field ID cannot be null or empty.", nameof(fieldId));
            if (strategies == null || strategies.Count < 2)
                throw new ArgumentException("At least 2 strategies required for comparison.", nameof(strategies));
        }

        // ==================== HELPER CALCULATION METHODS ====================

        private string DetermineDevApproach(double area, double reserves, int depth)
        {
            if (reserves > 500 && area > 100) return "Full Field Development";
            if (reserves > 100 && area > 50) return "Phased Development";
            if (reserves > 50) return "Pilot Development";
            return "Exploration to Development";
        }

        private int CalculateOptimalWellSpacing(double area, double reserves, string type)
        {
            // Typical well spacing calculation
            double basespacing = type.Contains("Shale") ? 40 : 
                                type.Contains("Sandstone") ? 80 : 120;
            return (int)(basespacing * (reserves / 100));
        }

        private int DeterminePhaseCount(double reserves)
        {
            if (reserves > 1000) return 4;
            if (reserves > 500) return 3;
            if (reserves > 100) return 2;
            return 1;
        }

        private double CalculateAnnualProduction(double reserves, int phases)
        {
            return reserves / (phases * 3.5);  // Average 3.5 year production per phase
        }

        private string AssessComplexity(int depth, string type, double area)
        {
            int score = 0;
            if (depth > 3000) score += 2;
            if (type.Contains("Shale")) score += 2;
            if (area > 200) score += 1;
            
            return score >= 4 ? "High" : score >= 2 ? "Medium" : "Low";
        }

        private string AssessRiskLevel(string type, int depth, double area)
        {
            int score = 0;
            if (type.Contains("Shale")) score += 2;
            if (depth > 4000) score += 2;
            if (area < 50) score += 1;
            
            return score >= 4 ? "High" : score >= 2 ? "Medium" : "Low";
        }

        private List<string> SelectRecommendedTechnologies(string type, int depth)
        {
            var techs = new List<string> { "4D Seismic", "Real-time Monitoring" };
            
            if (depth > 3000) techs.Add("Extended Reach Drilling");
            if (type.Contains("Shale")) techs.Add("Horizontal Drilling & Hydraulic Fracturing");
            if (type.Contains("Carbonate")) techs.Add("Acid Stimulation");
            
            return techs;
        }

        private List<string> DetermineInfrastructureRequirements(int spacing, double production)
        {
            var reqs = new List<string> { "Central Processing Facility", "Transmission Pipeline" };
            
            if (spacing < 80) reqs.Add("Advanced Metering System");
            if (production > 50000) reqs.Add("Dual Export Lines");
            
            return reqs;
        }

        private string AnalyzeProductionTrend(List<ProductionDataPoint> data)
        {
            if (data.Count < 2) return "Insufficient Data";
            
            var trend = data.Last().MonthlyProduction - data.First().MonthlyProduction;
            return trend > 0 ? "Increasing" : trend < 0 ? "Declining" : "Stable";
        }

        private double CalculateDeclineRate(List<ProductionDataPoint> data)
        {
            if (data.Count < 2) return 0;
            return (data.Last().MonthlyProduction - data.First().MonthlyProduction) / data.Count / data.First().MonthlyProduction;
        }

        private int CalculateRigsRequired(int wells, int months)
        {
            return (wells * 30 + (months * 30 - 1)) / (months * 30);  // Days per well: ~30
        }

        private int CalculateAverageDaysPerWell(int months, int wells)
        {
            return (months * 30) / wells;
        }

        private List<DateTime> GenerateSpudSchedule(int wells, int months)
        {
            var schedule = new List<DateTime>();
            var daysPerWell = (months * 30) / wells;
            var current = DateTime.UtcNow;
            
            for (int i = 0; i < wells; i++)
            {
                schedule.Add(current);
                current = current.AddDays(daysPerWell);
            }
            
            return schedule;
        }

        private List<DateTime> GenerateCompletionSchedule(List<DateTime> spudSchedule, int daysPerWell)
        {
            return spudSchedule.Select(s => s.AddDays(daysPerWell)).ToList();
        }

        private List<string> IdentifyOperationalChallenges(int rigs, int wells)
        {
            var challenges = new List<string>();
            
            if (rigs < 2) challenges.Add("Limited drilling capacity");
            if (wells > 20) challenges.Add("Complex well coordination");
            
            challenges.Add("Supply chain logistics");
            challenges.Add("Workforce scheduling");
            
            return challenges;
        }

        private List<string> DetermineLogisticsNeeds(int rigs, int wells)
        {
            var needs = new List<string> { "Rig transportation", "Crane services", "Supply chain" };
            
            if (rigs > 2) needs.Add("Advanced base operations");
            if (wells > 30) needs.Add("Contingency equipment");
            
            return needs;
        }

        private List<string> IdentifyEnvironmentalConsiderations(int rigs)
        {
            var considerations = new List<string> { "Air emissions monitoring", "Noise control" };
            
            if (rigs > 1) considerations.Add("Cumulative impact assessment");
            
            return considerations;
        }

        private ProcessingFacilitySpecification SizeProcessingFacility(double production, string type)
        {
            return new ProcessingFacilitySpecification
            {
                DesignCapacity = production,
                Efficiency = 0.92,
                CostPerUnit = type.Contains("Gas") ? 15 : 25
            };
        }

        private StorageRequirementsResult DetermineStorageNeeds(double production, string type)
        {
            return new StorageRequirementsResult
            {
                CrudeOilStorage = type.Contains("Oil") ? production * 3 : 0,
                CondensateStorage = type.Contains("Condensate") ? production * 2 : 0,
                GasStorage = type.Contains("Gas") ? production * 0.5 : 0
            };
        }

        private string SelectTransportationMethod(double distance, double production)
        {
            if (distance > 500) return "Pipeline";
            if (distance > 200 && production > 50000) return "Pipeline + Trucking";
            return "Local Processing";
        }

        private PipelineSpecification DesignPipelineSystem(double production, double distance, string type)
        {
            return new PipelineSpecification
            {
                Diameter = production > 100000 ? 16 : 12,
                Length = distance,
                Pressure = type.Contains("Gas") ? 600 : 200,
                Material = "Carbon Steel",
                CostPerKm = type.Contains("Gas") ? 5 : 8
            };
        }

        private double CalculatePowerRequirements(ProcessingFacilitySpecification facility)
        {
            return facility.DesignCapacity * (facility.Efficiency > 0.9 ? 5 : 8);
        }

        private string DetermineWaterHandlingNeeds(double production)
        {
            return production > 100000 ? "Advanced Treatment System" : "Standard Treatment";
        }

        private List<string> IdentifySafetySystems(string type, double production)
        {
            var systems = new List<string> { "ESD Systems", "Fire Detection", "Personnel Safety" };
            
            if (type.Contains("Gas")) systems.Add("Gas Detection");
            if (production > 100000) systems.Add("Redundant Systems");
            
            return systems;
        }

        private List<string> IdentifyEnvironmentalControls(string type)
        {
            var controls = new List<string> { "Spill Prevention", "Emissions Monitoring" };
            
            if (type.Contains("Gas")) controls.Add("Vapor Recovery");
            
            return controls;
        }

        private double CalculateCompletionCost(int wells, double costPerWell)
        {
            return wells * costPerWell * 0.3;  // 30% of drilling cost
        }

        private double CalculateEHSCosts(int wells)
        {
            return wells * 0.5;  // $0.5M per well
        }

        private double CalculateAnnualLaborCost(int wells)
        {
            return wells * 1.5;  // $1.5M per well annually
        }

        private double CalculateAnnualMaterialsCost(int wells)
        {
            return wells * 0.8;  // $0.8M per well annually
        }

        private double CalculateAnnualMaintenanceCost(double capex)
        {
            return capex * 0.05;  // 5% of capex
        }

        private Dictionary<string, double> CalculateCostEscalation()
        {
            return new Dictionary<string, double>
            {
                { "Year1", 1.0 },
                { "Year2", 1.03 },
                { "Year3", 1.06 },
                { "Year4", 1.09 },
                { "Year5", 1.12 }
            };
        }

        private CostBreakdownResult CreateCostBreakdown(DevelopmentCostAnalysisResult analysis)
        {
            return new CostBreakdownResult
            {
                DrillingCost = analysis.TotalDrillingCost,
                CompletionCost = analysis.CompletionCost,
                EhsPermits = analysis.EhsAndPermits,
                FacilityCost = analysis.FacilityCapex,
                ContingencyAllowance = analysis.ContingencyAllowance
            };
        }

        private List<ProductionPhase> GenerateProductionPhases(double capacity)
        {
            var phases = new List<ProductionPhase>();
            double phaseCapacity = capacity;
            
            for (int i = 1; i <= 3; i++)
            {
                phases.Add(new ProductionPhase
                {
                    PhaseNumber = i,
                    TargetCapacity = phaseCapacity,
                    DurationMonths = 24
                });
                phaseCapacity *= 0.8;  // Decline curve
            }
            
            return phases;
        }

        private List<double> GenerateMonthlySchedule(List<ProductionPhase> phases)
        {
            var schedule = new List<double>();
            
            foreach (var phase in phases)
            {
                double monthlyProd = phase.TargetCapacity / phase.DurationMonths;
                for (int i = 0; i < phase.DurationMonths; i++)
                {
                    schedule.Add(monthlyProd * (1 - (i / (double)phase.DurationMonths) * 0.3));  // Decline
                }
            }
            
            return schedule;
        }

        private string AssessAirQualityImpact(string sensitivity)
        {
            return sensitivity == "High" ? "Significant" : sensitivity == "Medium" ? "Moderate" : "Minor";
        }

        private string AssessWaterQualityImpact(string sensitivity, string location)
        {
            return location.Contains("offshore") || sensitivity == "High" ? "Significant" : "Moderate";
        }

        private string AssessSoilAndLandImpact(string sensitivity)
        {
            return sensitivity == "High" ? "High Impact" : "Moderate Impact";
        }

        private string AssessBiodiversityImpact(string sensitivity, string location)
        {
            return location.Contains("protected") || sensitivity == "High" ? "Significant" : "Minor";
        }

        private double EstimateGHGEmissions(EnvironmentalImpactAssessmentResult assessment)
        {
            return 10000;  // Typical emission level in tCO2e
        }

        private List<string> DevelopMitigationMeasures(EnvironmentalImpactAssessmentResult assessment)
        {
            var measures = new List<string> { "Environmental monitoring", "Habitat restoration" };
            
            if (assessment.EnvironmentalSensitivity == "High")
            {
                measures.Add("Advanced emission controls");
                measures.Add("Biodiversity offset program");
            }
            
            return measures;
        }

        private List<string> IdentifyComplianceRequirements(string location)
        {
            var reqs = new List<string> { "EIA", "Environmental Permits", "Community Engagement" };
            
            if (location.Contains("coastal")) reqs.Add("Coastal Zone Management");
            if (location.Contains("international")) reqs.Add("International Compliance");
            
            return reqs;
        }

        private double CalculateMitigationCost(List<string> measures)
        {
            return measures.Count * 2.5;  // $2.5M per measure
        }

        private List<RiskItem> IdentifyTechnicalRisks()
        {
            return new List<RiskItem>
            {
                new RiskItem { RiskId = "TR001", Description = "Reservoir uncertainty", Probability = 0.3, Impact = 0.8 },
                new RiskItem { RiskId = "TR002", Description = "Drilling complications", Probability = 0.4, Impact = 0.6 }
            };
        }

        private List<RiskItem> IdentifyCommercialRisks()
        {
            return new List<RiskItem>
            {
                new RiskItem { RiskId = "CR001", Description = "Price volatility", Probability = 0.7, Impact = 0.9 },
                new RiskItem { RiskId = "CR002", Description = "Market access issues", Probability = 0.2, Impact = 0.7 }
            };
        }

        private List<RiskItem> IdentifyOperationalRisks()
        {
            return new List<RiskItem>
            {
                new RiskItem { RiskId = "OR001", Description = "Supply chain disruption", Probability = 0.3, Impact = 0.6 },
                new RiskItem { RiskId = "OR002", Description = "Equipment failure", Probability = 0.2, Impact = 0.5 }
            };
        }

        private List<RiskItem> IdentifyRegulatoryRisks()
        {
            return new List<RiskItem>
            {
                new RiskItem { RiskId = "RG001", Description = "Policy changes", Probability = 0.4, Impact = 0.8 },
                new RiskItem { RiskId = "RG002", Description = "Permit delays", Probability = 0.5, Impact = 0.4 }
            };
        }

        private List<RiskItem> IdentifyEnvironmentalRisks()
        {
            return new List<RiskItem>
            {
                new RiskItem { RiskId = "ER001", Description = "Environmental incidents", Probability = 0.1, Impact = 0.9 },
                new RiskItem { RiskId = "ER002", Description = "Climate impacts", Probability = 0.2, Impact = 0.7 }
            };
        }

        private List<RiskItem> CalculateHighPriorityRisks(RiskAnalysisResult analysis)
        {
            var allRisks = new List<RiskItem>();
            if (analysis.TechnicalRisks != null) allRisks.AddRange(analysis.TechnicalRisks);
            if (analysis.CommercialRisks != null) allRisks.AddRange(analysis.CommercialRisks);
            if (analysis.OperationalRisks != null) allRisks.AddRange(analysis.OperationalRisks);
            if (analysis.RegulatoryRisks != null) allRisks.AddRange(analysis.RegulatoryRisks);
            if (analysis.EnvironmentalRisks != null) allRisks.AddRange(analysis.EnvironmentalRisks);

            return allRisks.Where(r => (r.Probability * r.Impact) > 0.4).OrderByDescending(r => r.Probability * r.Impact).ToList();
        }

        private List<string> DevelopRiskMitigationStrategies(RiskAnalysisResult analysis)
        {
            return new List<string>
            {
                "Diversify supply sources",
                "Establish contingency reserves",
                "Regular monitoring and review",
                "Insurance coverage",
                "Stakeholder engagement program"
            };
        }

        private double CalculateContingencyReserve(double npv)
        {
            return Math.Abs(npv) * 0.25;  // 25% contingency
        }

        private string CalculateOverallRiskRating(RiskAnalysisResult analysis)
        {
            double avgRisk = analysis.HighPriorityRisks.Average(r => r.Probability * r.Impact);
            return avgRisk > 0.6 ? "High" : avgRisk > 0.3 ? "Medium" : "Low";
        }

        private List<ProcessingEquipment> DesignSeparators(double rate, string type)
        {
            return new List<ProcessingEquipment>
            {
                new ProcessingEquipment { EquipmentType = "3-Phase Separator", Size = "3-Phase", Capacity = rate }
            };
        }

        private List<ProcessingEquipment> DesignCompressors(double rate)
        {
            return new List<ProcessingEquipment>
            {
                new ProcessingEquipment { EquipmentType = "Centrifugal Compressor", Size = "Large", Capacity = rate * 10 }
            };
        }

        private List<ProcessingEquipment> DesignPumps(double rate, string type)
        {
            var pumps = new List<ProcessingEquipment>();
            if (type.Contains("Oil"))
            {
                pumps.Add(new ProcessingEquipment { EquipmentType = "Crude Oil Transfer Pump", Capacity = rate });
            }
            return pumps;
        }

        private List<ProcessingEquipment> DesignHeatExchangers(double rate)
        {
            return new List<ProcessingEquipment>
            {
                new ProcessingEquipment { EquipmentType = "Plate-Frame Heat Exchanger", Capacity = rate * 5 }
            };
        }

        private List<string> DesignControlSystems()
        {
            return new List<string> { "SCADA System", "PLC Controls", "Real-time Monitoring" };
        }

        private List<string> DesignLayDownAreas(double rate)
        {
            return new List<string> { "Equipment storage", "Spare parts warehouse", "Maintenance shop" };
        }

        private List<string> DesignUtilityNeeds(double rate)
        {
            return new List<string> { "Power generation", "Water supply", "Compressed air" };
        }

        private double EstimateFacilityCost(FacilityDesignResult design)
        {
            return design.ProductionRate * 5;  // $5M per unit capacity estimate
        }

        private double CalculateNPV(double capex, List<double> cashflows, double discount)
        {
            double npv = -capex;
            for (int i = 0; i < cashflows.Count; i++)
            {
                npv += cashflows[i] / Math.Pow(1 + discount, i + 1);
            }
            return npv;
        }

        private double CalculateIRR(double capex, List<double> cashflows)
        {
            // Simplified IRR calculation - approximate
            double irr = 0;
            for (double rate = 0; rate < 1; rate += 0.01)
            {
                double npv = -capex;
                for (int i = 0; i < cashflows.Count; i++)
                {
                    npv += cashflows[i] / Math.Pow(1 + rate, i + 1);
                }
                if (npv < 0.01 && npv > -0.01) { irr = rate; break; }
            }
            return irr;
        }

        private double CalculatePaybackPeriod(double capex, List<double> cashflows)
        {
            double cumulative = 0;
            for (int i = 0; i < cashflows.Count; i++)
            {
                cumulative += cashflows[i];
                if (cumulative >= capex) return i + 1;
            }
            return cashflows.Count;
        }

        private double CalculateProfitabilityIndex(double npv, double capex)
        {
            return (npv + capex) / capex;
        }

        private SensitivityAnalysisResult PerformSensitivityAnalysis(InvestmentEvaluationResult eval)
        {
            return new SensitivityAnalysisResult
            {
                PriceVariation = new List<double> { eval.NPV * 0.8, eval.NPV, eval.NPV * 1.2 },
                VolumeVariation = new List<double> { eval.NPV * 0.9, eval.NPV, eval.NPV * 1.1 },
                CostVariation = new List<double> { eval.NPV * 1.1, eval.NPV, eval.NPV * 0.9 }
            };
        }

        private ScenarioAnalysisResult PerformScenarioAnalysis(InvestmentEvaluationResult eval)
        {
            return new ScenarioAnalysisResult
            {
                BaseCase = eval.NPV,
                DownsideCase = eval.NPV * 0.6,
                UpsideCase = eval.NPV * 1.4,
                ProbabilityBaseCase = 0.5,
                ProbabilityDownside = 0.3,
                ProbabilityUpside = 0.2
            };
        }

        private string RateInvestment(InvestmentEvaluationResult eval)
        {
            if (eval.NPV > 1000 && eval.IRR > 0.15) return "Excellent";
            if (eval.NPV > 500 && eval.IRR > 0.10) return "Good";
            if (eval.NPV > 0 && eval.IRR > 0.05) return "Acceptable";
            return "Poor";
        }

        private int CalculatePhaseDuration(int phaseNum, int totalPhases)
        {
            return 18 + (phaseNum * 2);  // Increasing duration per phase
        }

        private List<string> GeneratePhaseMilestones(int phase, DateTime start)
        {
            return new List<string>
            {
                $"Phase {phase} - Permits",
                $"Phase {phase} - Design",
                $"Phase {phase} - Procurement",
                $"Phase {phase} - Execution",
                $"Phase {phase} - Completion"
            };
        }

        private List<int> GeneratePhaseDependencies(int phase)
        {
            return phase == 1 ? new List<int>() : new List<int> { phase - 1 };
        }

        private string IdentifyCriticalPath(List<DevelopmentPhase> phases)
        {
            return "Design → Procurement → Execution";
        }

        private CostComparisonResult CompareCosts(List<DevelopmentStrategy> strategies)
        {
            return new CostComparisonResult
            {
                LowestCost = strategies.Min(s => s.EstimatedCapex),
                HighestCost = strategies.Max(s => s.EstimatedCapex),
                AverageCost = strategies.Average(s => s.EstimatedCapex)
            };
        }

        private EconomicComparisonResult CompareEconomics(List<DevelopmentStrategy> strategies)
        {
            return new EconomicComparisonResult
            {
                HighestNPV = strategies.Max(s => s.ProjectNPV),
                LowestNPV = strategies.Min(s => s.ProjectNPV),
                BestIRR = strategies.Max(s => s.ProjectIRR)
            };
        }

        private RiskComparisonResult CompareRisks(List<DevelopmentStrategy> strategies)
        {
            return new RiskComparisonResult
            {
                LowestRisk = strategies.Min(s => s.RiskLevel),
                HighestRisk = strategies.Max(s => s.RiskLevel)
            };
        }

        private ScheduleComparisonResult CompareSchedules(List<DevelopmentStrategy> strategies)
        {
            return new ScheduleComparisonResult
            {
                ShortestDuration = strategies.Min(s => s.ProjectDuration),
                LongestDuration = strategies.Max(s => s.ProjectDuration)
            };
        }

        private EnvironmentalComparisonResult CompareEnvironmental(List<DevelopmentStrategy> strategies)
        {
            return new EnvironmentalComparisonResult
            {
                LowestEmissions = strategies.Min(s => s.EstimatedEmissions),
                HighestEmissions = strategies.Max(s => s.EstimatedEmissions)
            };
        }

        private string SelectBestStrategy(List<DevelopmentStrategy> strategies)
        {
            // Score based on NPV and risk
            var bestScore = strategies.Max(s => s.ProjectNPV / Math.Max(s.RiskLevel, 0.1));
            return strategies.First(s => (s.ProjectNPV / Math.Max(s.RiskLevel, 0.1)) == bestScore).StrategyName;
        }

        private List<StrategyRanking> RankStrategies(List<DevelopmentStrategy> strategies)
        {
            return strategies
                .OrderByDescending(s => s.ProjectNPV / Math.Max(s.RiskLevel, 0.1))
                .Select((s, i) => new StrategyRanking { Rank = i + 1, Strategy = s.StrategyName, Score = 100 - (i * 20) })
                .ToList();
        }

        private TradeOffAnalysisResult PerformTradeOffAnalysis(List<DevelopmentStrategy> strategies)
        {
            return new TradeOffAnalysisResult
            {
                CostVsSchedule = "Expedited schedules typically increase costs by 15-25%",
                RiskVsReward = "Higher risk strategies show 20-40% higher NPV potential",
                EnvironmentalVsEconomic = "Green technologies add 5-10% to capital costs"
            };
        }
    }
}
