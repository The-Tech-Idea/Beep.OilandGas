using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Result of field development strategy analysis
    /// </summary>
    public class FieldDevelopmentStrategyResult
    {
        public string FieldId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double FieldArea { get; set; }
        public double EstimatedReserves { get; set; }
        public string ReservoirType { get; set; }
        public string RecommendedApproach { get; set; }
        public int WellSpacing { get; set; }
        public int PhaseCount { get; set; }
        public double AnnualProduction { get; set; }
        public string Complexity { get; set; }
        public string RiskLevel { get; set; }
        public List<string> RecommendedTechnologies { get; set; }
        public List<string> InfrastructureRequirements { get; set; }
    }

    /// <summary>
    /// Recoverable reserves estimation with probability ranges
    /// </summary>
    public class RecoverableReservesEstimate
    {
        public string FieldId { get; set; }
        public DateTime EstimationDate { get; set; }
        public double InitialReserves { get; set; }
        public double RecoveryFactor { get; set; }
        public double RecoverableReserves { get; set; }
        public double CumulativeProduction { get; set; }
        public double RemainingReserves { get; set; }
        public string ProductionTrend { get; set; }
        public double ProductionDecline { get; set; }
        public double P90Estimate { get; set; }  // Conservative
        public double P50Estimate { get; set; }  // Most likely
        public double P10Estimate { get; set; }  // Optimistic
        public UncertaintyRange UncertaintyRange { get; set; }
    }

    /// <summary>
    /// Uncertainty range for reserves estimation
    /// </summary>
    public class UncertaintyRange
    {
        public double LowCase { get; set; }
        public double BaseCase { get; set; }
        public double HighCase { get; set; }
    }

    /// <summary>
    /// Production data point for historical analysis
    /// </summary>
    public class ProductionDataPoint
    {
        public DateTime Month { get; set; }
        public double MonthlyProduction { get; set; }
        public double CumulativeProduction { get; set; }
    }

    /// <summary>
    /// Drilling program with schedule and specifications
    /// </summary>
    public class DrillingProgramResult
    {
        public string FieldId { get; set; }
        public DateTime ProgramDate { get; set; }
        public int TotalWellsPlanned { get; set; }
        public int PhaseDuration { get; set; }
        public Dictionary<string, int> WellTypeDistribution { get; set; }
        public int RigsRequired { get; set; }
        public int AverageDaysPerWell { get; set; }
        public List<DateTime> SpudSchedule { get; set; }
        public List<DateTime> CompletionSchedule { get; set; }
        public List<string> OperationalChallenges { get; set; }
        public List<string> LogisticsRequirements { get; set; }
        public List<string> EnvironmentalConsiderations { get; set; }
    }

    /// <summary>
    /// Infrastructure planning results
    /// </summary>
    public class InfrastructurePlanningResult
    {
        public string FieldId { get; set; }
        public DateTime PlanningDate { get; set; }
        public double PeakProduction { get; set; }
        public string ProductType { get; set; }
        public double DistanceToMarket { get; set; }
        public ProcessingFacilitySpecification ProcessingFacility { get; set; }
        public StorageRequirementsResult StorageRequirements { get; set; }
        public string TransportationMethod { get; set; }
        public PipelineSpecification PipelineSpecifications { get; set; }
        public double PowerRequirements { get; set; }
        public string WaterHandling { get; set; }
        public List<string> SafetySystemsRequired { get; set; }
        public List<string> EnvironmentalControlsRequired { get; set; }
    }

    /// <summary>
    /// Processing facility specifications
    /// </summary>
    public class ProcessingFacilitySpecification
    {
        public double DesignCapacity { get; set; }
        public double Efficiency { get; set; }
        public double CostPerUnit { get; set; }
    }

    /// <summary>
    /// Storage requirements breakdown
    /// </summary>
    public class StorageRequirementsResult
    {
        public double CrudeOilStorage { get; set; }
        public double CondensateStorage { get; set; }
        public double GasStorage { get; set; }
    }

    /// <summary>
    /// Pipeline system design specifications
    /// </summary>
    public class PipelineSpecification
    {
        public int Diameter { get; set; }
        public double Length { get; set; }
        public int Pressure { get; set; }
        public string Material { get; set; }
        public double CostPerKm { get; set; }
    }

    /// <summary>
    /// Development cost analysis
    /// </summary>
    public class DevelopmentCostAnalysisResult
    {
        public string FieldId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public int WellCount { get; set; }
        public double DrillingCostPerWell { get; set; }
        public double FacilityCapex { get; set; }
        public double TotalDrillingCost { get; set; }
        public double CompletionCost { get; set; }
        public double EhsAndPermits { get; set; }
        public double TotalCapex { get; set; }
        public double AnnualLaborCost { get; set; }
        public double AnnualMaterialsCost { get; set; }
        public double AnnualMaintenance { get; set; }
        public double AnnualOpex { get; set; }
        public Dictionary<string, double> CostEscalationFactors { get; set; }
        public double ContingencyAllowance { get; set; }
        public double TotalProjectCost { get; set; }
        public CostBreakdownResult CostBreakdown { get; set; }
    }

    /// <summary>
    /// Cost breakdown by category
    /// </summary>
    public class CostBreakdownResult
    {
        public double DrillingCost { get; set; }
        public double CompletionCost { get; set; }
        public double EhsPermits { get; set; }
        public double FacilityCost { get; set; }
        public double ContingencyAllowance { get; set; }
    }

    /// <summary>
    /// Production phase definition
    /// </summary>
    public class ProductionPhase
    {
        public int PhaseNumber { get; set; }
        public double TargetCapacity { get; set; }
        public int DurationMonths { get; set; }
    }

    /// <summary>
    /// Production schedule optimization results
    /// </summary>
    public class ProductionScheduleOptimizationResult
    {
        public string FieldId { get; set; }
        public DateTime OptimizationDate { get; set; }
        public double ProductionCapacity { get; set; }
        public double MarketPrice { get; set; }
        public double OperatingCostPerUnit { get; set; }
        public List<ProductionPhase> ProductionPhases { get; set; }
        public List<double> MonthlySchedule { get; set; }
        public double Breakeven { get; set; }
        public double Margin { get; set; }
        public List<double> MonthlyRevenue { get; set; }
        public List<double> MonthlyCashFlow { get; set; }
        public double TotalProjectRevenue { get; set; }
        public double TotalProjectCost { get; set; }
        public double NetCashFlow { get; set; }
    }

    /// <summary>
    /// Environmental impact assessment results
    /// </summary>
    public class EnvironmentalImpactAssessmentResult
    {
        public string FieldId { get; set; }
        public DateTime AssessmentDate { get; set; }
        public string Location { get; set; }
        public string EnvironmentalSensitivity { get; set; }
        public string AirQualityImpact { get; set; }
        public string WaterQualityImpact { get; set; }
        public string SoilAndLandImpact { get; set; }
        public string BiodiversityImpact { get; set; }
        public double GHGEmissions { get; set; }
        public List<string> MitigationMeasures { get; set; }
        public List<string> ComplianceRequirements { get; set; }
        public double CostOfMitigation { get; set; }
    }

    /// <summary>
    /// Risk analysis results
    /// </summary>
    public class RiskAnalysisResult
    {
        public string FieldId { get; set; }
        public DateTime AnalysisDate { get; set; }
        public int ProjectDuration { get; set; }
        public double ProjectedNPV { get; set; }
        public List<RiskItem> TechnicalRisks { get; set; }
        public List<RiskItem> CommercialRisks { get; set; }
        public List<RiskItem> OperationalRisks { get; set; }
        public List<RiskItem> RegulatoryRisks { get; set; }
        public List<RiskItem> EnvironmentalRisks { get; set; }
        public int TotalIdentifiedRisks { get; set; }
        public List<RiskItem> HighPriorityRisks { get; set; }
        public List<string> MitigationStrategies { get; set; }
        public double ContingencyReserve { get; set; }
        public string OverallRiskRating { get; set; }
    }

    /// <summary>
    /// Individual risk item
    /// </summary>
    public class RiskItem
    {
        public string RiskId { get; set; }
        public string Description { get; set; }
        public double Probability { get; set; }
        public double Impact { get; set; }
    }

    /// <summary>
    /// Facility design results
    /// </summary>
    public class FacilityDesignResult
    {
        public string FieldId { get; set; }
        public DateTime DesignDate { get; set; }
        public double ProductionRate { get; set; }
        public string ProductType { get; set; }
        public List<ProcessingEquipment> SeparatorSpecifications { get; set; }
        public List<ProcessingEquipment> CompressorRequirements { get; set; }
        public List<ProcessingEquipment> PumpingRequirements { get; set; }
        public List<ProcessingEquipment> HeatExchangerSpecifications { get; set; }
        public List<string> ControlSystemsSpecification { get; set; }
        public List<string> LaySgDownRequirements { get; set; }
        public List<string> UtilityRequirements { get; set; }
        public double CostEstimate { get; set; }
    }

    /// <summary>
    /// Processing equipment specification
    /// </summary>
    public class ProcessingEquipment
    {
        public string EquipmentType { get; set; }
        public string Size { get; set; }
        public double Capacity { get; set; }
    }

    /// <summary>
    /// Investment evaluation results
    /// </summary>
    public class InvestmentEvaluationResult
    {
        public string FieldId { get; set; }
        public DateTime EvaluationDate { get; set; }
        public double InitialCapex { get; set; }
        public double DiscountRate { get; set; }
        public List<double> ProjectedCashFlows { get; set; }
        public double NPV { get; set; }
        public double IRR { get; set; }
        public double PaybackPeriod { get; set; }
        public double ProfitabilityIndex { get; set; }
        public double TotalProjectValue { get; set; }
        public SensitivityAnalysisResult SensitivityAnalysis { get; set; }
        public ScenarioAnalysisResult ScenarioAnalysis { get; set; }
        public string InvestmentRating { get; set; }
    }

    /// <summary>
    /// Sensitivity analysis results
    /// </summary>
    public class SensitivityAnalysisResult
    {
        public List<double> PriceVariation { get; set; }
        public List<double> VolumeVariation { get; set; }
        public List<double> CostVariation { get; set; }
    }

    /// <summary>
    /// Scenario analysis results
    /// </summary>
    public class ScenarioAnalysisResult
    {
        public double BaseCase { get; set; }
        public double DownsideCase { get; set; }
        public double UpsideCase { get; set; }
        public double ProbabilityBaseCase { get; set; }
        public double ProbabilityDownside { get; set; }
        public double ProbabilityUpside { get; set; }
    }

    /// <summary>
    /// Development phase schedule
    /// </summary>
    public class DevelopmentPhaseScheduleResult
    {
        public string FieldId { get; set; }
        public DateTime ScheduleDate { get; set; }
        public DateTime DevelopmentStartDate { get; set; }
        public int PhaseCount { get; set; }
        public List<DevelopmentPhase> Phases { get; set; }
        public int TotalProjectDuration { get; set; }
        public string CriticalPath { get; set; }
        public DateTime ProjectCompletion { get; set; }
    }

    /// <summary>
    /// Individual development phase with milestones
    /// </summary>
    public class DevelopmentPhase
    {
        public int PhaseNumber { get; set; }
        public string PhaseName { get; set; }
        public DateTime StartDate { get; set; }
        public int Duration { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> Milestones { get; set; }
        public List<int> Dependencies { get; set; }
    }

    /// <summary>
    /// Development strategy definition
    /// </summary>
    public class DevelopmentStrategy
    {
        public string StrategyName { get; set; }
        public string Description { get; set; }
        public double EstimatedCapex { get; set; }
        public double ProjectNPV { get; set; }
        public double ProjectIRR { get; set; }
        public double RiskLevel { get; set; }
        public int ProjectDuration { get; set; }
        public double EstimatedEmissions { get; set; }
        public List<string> KeyBenefits { get; set; }
        public List<string> KeyChallenges { get; set; }
    }

    /// <summary>
    /// Strategy ranking entry
    /// </summary>
    public class StrategyRanking
    {
        public int Rank { get; set; }
        public string Strategy { get; set; }
        public double Score { get; set; }
    }

    /// <summary>
    /// Alternative strategies comparison results
    /// </summary>
    public class StrategiesComparisonResult
    {
        public string FieldId { get; set; }
        public DateTime ComparisonDate { get; set; }
        public int StrategyCount { get; set; }
        public List<DevelopmentStrategy> Strategies { get; set; }
        public CostComparisonResult CostComparison { get; set; }
        public EconomicComparisonResult EconomicComparison { get; set; }
        public RiskComparisonResult RiskComparison { get; set; }
        public ScheduleComparisonResult ScheduleComparison { get; set; }
        public EnvironmentalComparisonResult EnvironmentalComparison { get; set; }
        public string RecommendedStrategy { get; set; }
        public List<StrategyRanking> AlternativeRankings { get; set; }
        public TradeOffAnalysisResult TradeOffAnalysis { get; set; }
    }

    /// <summary>
    /// Cost comparison between strategies
    /// </summary>
    public class CostComparisonResult
    {
        public double LowestCost { get; set; }
        public double HighestCost { get; set; }
        public double AverageCost { get; set; }
    }

    /// <summary>
    /// Economic comparison between strategies
    /// </summary>
    public class EconomicComparisonResult
    {
        public double HighestNPV { get; set; }
        public double LowestNPV { get; set; }
        public double BestIRR { get; set; }
    }

    /// <summary>
    /// Risk comparison between strategies
    /// </summary>
    public class RiskComparisonResult
    {
        public double LowestRisk { get; set; }
        public double HighestRisk { get; set; }
    }

    /// <summary>
    /// Schedule comparison between strategies
    /// </summary>
    public class ScheduleComparisonResult
    {
        public int ShortestDuration { get; set; }
        public int LongestDuration { get; set; }
    }

    /// <summary>
    /// Environmental comparison between strategies
    /// </summary>
    public class EnvironmentalComparisonResult
    {
        public double LowestEmissions { get; set; }
        public double HighestEmissions { get; set; }
    }

    /// <summary>
    /// Trade-off analysis between strategies
    /// </summary>
    public class TradeOffAnalysisResult
    {
        public string CostVsSchedule { get; set; }
        public string RiskVsReward { get; set; }
        public string EnvironmentalVsEconomic { get; set; }
    }
}
