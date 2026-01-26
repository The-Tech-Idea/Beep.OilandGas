using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Result of field development strategy analysis
    /// </summary>
    public class FieldDevelopmentStrategyResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double FieldAreaValue;

        public double FieldArea

        {

            get { return this.FieldAreaValue; }

            set { SetProperty(ref FieldAreaValue, value); }

        }
        private double EstimatedReservesValue;

        public double EstimatedReserves

        {

            get { return this.EstimatedReservesValue; }

            set { SetProperty(ref EstimatedReservesValue, value); }

        }
        private string ReservoirTypeValue;

        public string ReservoirType

        {

            get { return this.ReservoirTypeValue; }

            set { SetProperty(ref ReservoirTypeValue, value); }

        }
        private string RecommendedApproachValue;

        public string RecommendedApproach

        {

            get { return this.RecommendedApproachValue; }

            set { SetProperty(ref RecommendedApproachValue, value); }

        }
        private int WellSpacingValue;

        public int WellSpacing

        {

            get { return this.WellSpacingValue; }

            set { SetProperty(ref WellSpacingValue, value); }

        }
        private int PhaseCountValue;

        public int PhaseCount

        {

            get { return this.PhaseCountValue; }

            set { SetProperty(ref PhaseCountValue, value); }

        }
        private double AnnualProductionValue;

        public double AnnualProduction

        {

            get { return this.AnnualProductionValue; }

            set { SetProperty(ref AnnualProductionValue, value); }

        }
        private string ComplexityValue;

        public string Complexity

        {

            get { return this.ComplexityValue; }

            set { SetProperty(ref ComplexityValue, value); }

        }
        private string RiskLevelValue;

        public string RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private List<string> RecommendedTechnologiesValue;

        public List<string> RecommendedTechnologies

        {

            get { return this.RecommendedTechnologiesValue; }

            set { SetProperty(ref RecommendedTechnologiesValue, value); }

        }
        private List<string> InfrastructureRequirementsValue;

        public List<string> InfrastructureRequirements

        {

            get { return this.InfrastructureRequirementsValue; }

            set { SetProperty(ref InfrastructureRequirementsValue, value); }

        }
    }

    /// <summary>
    /// Recoverable reserves estimation with probability ranges
    /// </summary>
    public class RecoverableReservesEstimate : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime EstimationDateValue;

        public DateTime EstimationDate

        {

            get { return this.EstimationDateValue; }

            set { SetProperty(ref EstimationDateValue, value); }

        }
        private double InitialReservesValue;

        public double InitialReserves

        {

            get { return this.InitialReservesValue; }

            set { SetProperty(ref InitialReservesValue, value); }

        }
        private double RecoveryFactorValue;

        public double RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        }
        private double RecoverableReservesValue;

        public double RecoverableReserves

        {

            get { return this.RecoverableReservesValue; }

            set { SetProperty(ref RecoverableReservesValue, value); }

        }
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }
        private double RemainingReservesValue;

        public double RemainingReserves

        {

            get { return this.RemainingReservesValue; }

            set { SetProperty(ref RemainingReservesValue, value); }

        }
        private string ProductionTrendValue;

        public string ProductionTrend

        {

            get { return this.ProductionTrendValue; }

            set { SetProperty(ref ProductionTrendValue, value); }

        }
        private double ProductionDeclineValue;

        public double ProductionDecline

        {

            get { return this.ProductionDeclineValue; }

            set { SetProperty(ref ProductionDeclineValue, value); }

        }
        private double P90EstimateValue;

        public double P90Estimate

        {

            get { return this.P90EstimateValue; }

            set { SetProperty(ref P90EstimateValue, value); }

        }  // Conservative
        private double P50EstimateValue;

        public double P50Estimate

        {

            get { return this.P50EstimateValue; }

            set { SetProperty(ref P50EstimateValue, value); }

        }  // Most likely
        private double P10EstimateValue;

        public double P10Estimate

        {

            get { return this.P10EstimateValue; }

            set { SetProperty(ref P10EstimateValue, value); }

        }  // Optimistic
        private UncertaintyRange UncertaintyRangeValue;

        public UncertaintyRange UncertaintyRange

        {

            get { return this.UncertaintyRangeValue; }

            set { SetProperty(ref UncertaintyRangeValue, value); }

        }
    }

    /// <summary>
    /// Uncertainty range for reserves estimation
    /// </summary>
    public class UncertaintyRange : ModelEntityBase
    {
        private double LowCaseValue;

        public double LowCase

        {

            get { return this.LowCaseValue; }

            set { SetProperty(ref LowCaseValue, value); }

        }
        private double BaseCaseValue;

        public double BaseCase

        {

            get { return this.BaseCaseValue; }

            set { SetProperty(ref BaseCaseValue, value); }

        }
        private double HighCaseValue;

        public double HighCase

        {

            get { return this.HighCaseValue; }

            set { SetProperty(ref HighCaseValue, value); }

        }
    }

    /// <summary>
    /// Production data point for historical analysis
    /// </summary>
    public class ProductionDataPoint : ModelEntityBase
    {
        private DateTime MonthValue;

        public DateTime Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }
        private double MonthlyProductionValue;

        public double MonthlyProduction

        {

            get { return this.MonthlyProductionValue; }

            set { SetProperty(ref MonthlyProductionValue, value); }

        }
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }
    }

    /// <summary>
    /// Drilling program with schedule and specifications
    /// </summary>
    public class DrillingProgramResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime ProgramDateValue;

        public DateTime ProgramDate

        {

            get { return this.ProgramDateValue; }

            set { SetProperty(ref ProgramDateValue, value); }

        }
        private int TotalWellsPlannedValue;

        public int TotalWellsPlanned

        {

            get { return this.TotalWellsPlannedValue; }

            set { SetProperty(ref TotalWellsPlannedValue, value); }

        }
        private int PhaseDurationValue;

        public int PhaseDuration

        {

            get { return this.PhaseDurationValue; }

            set { SetProperty(ref PhaseDurationValue, value); }

        }
        public List<DevelopmentWellTypeDistributionEntry> WellTypeDistribution { get; set; } = new();
        private int RigsRequiredValue;

        public int RigsRequired

        {

            get { return this.RigsRequiredValue; }

            set { SetProperty(ref RigsRequiredValue, value); }

        }
        private int AverageDaysPerWellValue;

        public int AverageDaysPerWell

        {

            get { return this.AverageDaysPerWellValue; }

            set { SetProperty(ref AverageDaysPerWellValue, value); }

        }
        private List<DateTime> SpudScheduleValue;

        public List<DateTime> SpudSchedule

        {

            get { return this.SpudScheduleValue; }

            set { SetProperty(ref SpudScheduleValue, value); }

        }
        private List<DateTime> CompletionScheduleValue;

        public List<DateTime> CompletionSchedule

        {

            get { return this.CompletionScheduleValue; }

            set { SetProperty(ref CompletionScheduleValue, value); }

        }
        private List<string> OperationalChallengesValue;

        public List<string> OperationalChallenges

        {

            get { return this.OperationalChallengesValue; }

            set { SetProperty(ref OperationalChallengesValue, value); }

        }
        private List<string> LogisticsRequirementsValue;

        public List<string> LogisticsRequirements

        {

            get { return this.LogisticsRequirementsValue; }

            set { SetProperty(ref LogisticsRequirementsValue, value); }

        }
        private List<string> EnvironmentalConsiderationsValue;

        public List<string> EnvironmentalConsiderations

        {

            get { return this.EnvironmentalConsiderationsValue; }

            set { SetProperty(ref EnvironmentalConsiderationsValue, value); }

        }
    }

    /// <summary>
    /// Infrastructure planning results
    /// </summary>
    public class InfrastructurePlanningResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime PlanningDateValue;

        public DateTime PlanningDate

        {

            get { return this.PlanningDateValue; }

            set { SetProperty(ref PlanningDateValue, value); }

        }
        private double PeakProductionValue;

        public double PeakProduction

        {

            get { return this.PeakProductionValue; }

            set { SetProperty(ref PeakProductionValue, value); }

        }
        private string ProductTypeValue;

        public string ProductType

        {

            get { return this.ProductTypeValue; }

            set { SetProperty(ref ProductTypeValue, value); }

        }
        private double DistanceToMarketValue;

        public double DistanceToMarket

        {

            get { return this.DistanceToMarketValue; }

            set { SetProperty(ref DistanceToMarketValue, value); }

        }
        private ProcessingFacilitySpecification ProcessingFacilityValue;

        public ProcessingFacilitySpecification ProcessingFacility

        {

            get { return this.ProcessingFacilityValue; }

            set { SetProperty(ref ProcessingFacilityValue, value); }

        }
        private StorageRequirementsResult StorageRequirementsValue;

        public StorageRequirementsResult StorageRequirements

        {

            get { return this.StorageRequirementsValue; }

            set { SetProperty(ref StorageRequirementsValue, value); }

        }
        private string TransportationMethodValue;

        public string TransportationMethod

        {

            get { return this.TransportationMethodValue; }

            set { SetProperty(ref TransportationMethodValue, value); }

        }
        private PipelineSpecification PipelineSpecificationsValue;

        public PipelineSpecification PipelineSpecifications

        {

            get { return this.PipelineSpecificationsValue; }

            set { SetProperty(ref PipelineSpecificationsValue, value); }

        }
        private double PowerRequirementsValue;

        public double PowerRequirements

        {

            get { return this.PowerRequirementsValue; }

            set { SetProperty(ref PowerRequirementsValue, value); }

        }
        private string WaterHandlingValue;

        public string WaterHandling

        {

            get { return this.WaterHandlingValue; }

            set { SetProperty(ref WaterHandlingValue, value); }

        }
        private List<string> SafetySystemsRequiredValue;

        public List<string> SafetySystemsRequired

        {

            get { return this.SafetySystemsRequiredValue; }

            set { SetProperty(ref SafetySystemsRequiredValue, value); }

        }
        private List<string> EnvironmentalControlsRequiredValue;

        public List<string> EnvironmentalControlsRequired

        {

            get { return this.EnvironmentalControlsRequiredValue; }

            set { SetProperty(ref EnvironmentalControlsRequiredValue, value); }

        }
    }

    /// <summary>
    /// Processing facility specifications
    /// </summary>
    public class ProcessingFacilitySpecification : ModelEntityBase
    {
        private double DesignCapacityValue;

        public double DesignCapacity

        {

            get { return this.DesignCapacityValue; }

            set { SetProperty(ref DesignCapacityValue, value); }

        }
        private double EfficiencyValue;

        public double Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private double CostPerUnitValue;

        public double CostPerUnit

        {

            get { return this.CostPerUnitValue; }

            set { SetProperty(ref CostPerUnitValue, value); }

        }
    }

    /// <summary>
    /// Storage requirements breakdown
    /// </summary>
    public class StorageRequirementsResult : ModelEntityBase
    {
        private double CrudeOilStorageValue;

        public double CrudeOilStorage

        {

            get { return this.CrudeOilStorageValue; }

            set { SetProperty(ref CrudeOilStorageValue, value); }

        }
        private double CondensateStorageValue;

        public double CondensateStorage

        {

            get { return this.CondensateStorageValue; }

            set { SetProperty(ref CondensateStorageValue, value); }

        }
        private double GasStorageValue;

        public double GasStorage

        {

            get { return this.GasStorageValue; }

            set { SetProperty(ref GasStorageValue, value); }

        }
    }

    /// <summary>
    /// Pipeline system design specifications
    /// </summary>
    public class PipelineSpecification : ModelEntityBase
    {
        private int DiameterValue;

        public int Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }
        private double LengthValue;

        public double Length

        {

            get { return this.LengthValue; }

            set { SetProperty(ref LengthValue, value); }

        }
        private int PressureValue;

        public int Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private string MaterialValue;

        public string Material

        {

            get { return this.MaterialValue; }

            set { SetProperty(ref MaterialValue, value); }

        }
        private double CostPerKmValue;

        public double CostPerKm

        {

            get { return this.CostPerKmValue; }

            set { SetProperty(ref CostPerKmValue, value); }

        }
    }

    /// <summary>
    /// Development cost analysis
    /// </summary>
    public class DevelopmentCostAnalysisResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private int WellCountValue;

        public int WellCount

        {

            get { return this.WellCountValue; }

            set { SetProperty(ref WellCountValue, value); }

        }
        private double DrillingCostPerWellValue;

        public double DrillingCostPerWell

        {

            get { return this.DrillingCostPerWellValue; }

            set { SetProperty(ref DrillingCostPerWellValue, value); }

        }
        private double FacilityCapexValue;

        public double FacilityCapex

        {

            get { return this.FacilityCapexValue; }

            set { SetProperty(ref FacilityCapexValue, value); }

        }
        private double TotalDrillingCostValue;

        public double TotalDrillingCost

        {

            get { return this.TotalDrillingCostValue; }

            set { SetProperty(ref TotalDrillingCostValue, value); }

        }
        private double CompletionCostValue;

        public double CompletionCost

        {

            get { return this.CompletionCostValue; }

            set { SetProperty(ref CompletionCostValue, value); }

        }
        private double EhsAndPermitsValue;

        public double EhsAndPermits

        {

            get { return this.EhsAndPermitsValue; }

            set { SetProperty(ref EhsAndPermitsValue, value); }

        }
        private double TotalCapexValue;

        public double TotalCapex

        {

            get { return this.TotalCapexValue; }

            set { SetProperty(ref TotalCapexValue, value); }

        }
        private double AnnualLaborCostValue;

        public double AnnualLaborCost

        {

            get { return this.AnnualLaborCostValue; }

            set { SetProperty(ref AnnualLaborCostValue, value); }

        }
        private double AnnualMaterialsCostValue;

        public double AnnualMaterialsCost

        {

            get { return this.AnnualMaterialsCostValue; }

            set { SetProperty(ref AnnualMaterialsCostValue, value); }

        }
        private double AnnualMaintenanceValue;

        public double AnnualMaintenance

        {

            get { return this.AnnualMaintenanceValue; }

            set { SetProperty(ref AnnualMaintenanceValue, value); }

        }
        private double AnnualOpexValue;

        public double AnnualOpex

        {

            get { return this.AnnualOpexValue; }

            set { SetProperty(ref AnnualOpexValue, value); }

        }
        public List<DevelopmentCostEscalationFactorEntry> CostEscalationFactors { get; set; } = new();
        private double ContingencyAllowanceValue;

        public double ContingencyAllowance

        {

            get { return this.ContingencyAllowanceValue; }

            set { SetProperty(ref ContingencyAllowanceValue, value); }

        }
        private double TotalProjectCostValue;

        public double TotalProjectCost

        {

            get { return this.TotalProjectCostValue; }

            set { SetProperty(ref TotalProjectCostValue, value); }

        }
        private CostBreakdownResult CostBreakdownValue;

        public CostBreakdownResult CostBreakdown

        {

            get { return this.CostBreakdownValue; }

            set { SetProperty(ref CostBreakdownValue, value); }

        }
    }

    /// <summary>
    /// Cost breakdown by category
    /// </summary>
    public class CostBreakdownResult : ModelEntityBase
    {
        private double DrillingCostValue;

        public double DrillingCost

        {

            get { return this.DrillingCostValue; }

            set { SetProperty(ref DrillingCostValue, value); }

        }
        private double CompletionCostValue;

        public double CompletionCost

        {

            get { return this.CompletionCostValue; }

            set { SetProperty(ref CompletionCostValue, value); }

        }
        private double EhsPermitsValue;

        public double EhsPermits

        {

            get { return this.EhsPermitsValue; }

            set { SetProperty(ref EhsPermitsValue, value); }

        }
        private double FacilityCostValue;

        public double FacilityCost

        {

            get { return this.FacilityCostValue; }

            set { SetProperty(ref FacilityCostValue, value); }

        }
        private double ContingencyAllowanceValue;

        public double ContingencyAllowance

        {

            get { return this.ContingencyAllowanceValue; }

            set { SetProperty(ref ContingencyAllowanceValue, value); }

        }
    }

    /// <summary>
    /// Production phase definition
    /// </summary>
    public class ProductionPhase : ModelEntityBase
    {
        private int PhaseNumberValue;

        public int PhaseNumber

        {

            get { return this.PhaseNumberValue; }

            set { SetProperty(ref PhaseNumberValue, value); }

        }
        private double TargetCapacityValue;

        public double TargetCapacity

        {

            get { return this.TargetCapacityValue; }

            set { SetProperty(ref TargetCapacityValue, value); }

        }
        private int DurationMonthsValue;

        public int DurationMonths

        {

            get { return this.DurationMonthsValue; }

            set { SetProperty(ref DurationMonthsValue, value); }

        }
    }

    /// <summary>
    /// Production schedule optimization results
    /// </summary>
    public class ProductionScheduleOptimizationResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime OptimizationDateValue;

        public DateTime OptimizationDate

        {

            get { return this.OptimizationDateValue; }

            set { SetProperty(ref OptimizationDateValue, value); }

        }
        private double ProductionCapacityValue;

        public double ProductionCapacity

        {

            get { return this.ProductionCapacityValue; }

            set { SetProperty(ref ProductionCapacityValue, value); }

        }
        private double MarketPriceValue;

        public double MarketPrice

        {

            get { return this.MarketPriceValue; }

            set { SetProperty(ref MarketPriceValue, value); }

        }
        private double OperatingCostPerUnitValue;

        public double OperatingCostPerUnit

        {

            get { return this.OperatingCostPerUnitValue; }

            set { SetProperty(ref OperatingCostPerUnitValue, value); }

        }
        private List<ProductionPhase> ProductionPhasesValue;

        public List<ProductionPhase> ProductionPhases

        {

            get { return this.ProductionPhasesValue; }

            set { SetProperty(ref ProductionPhasesValue, value); }

        }
        private List<double> MonthlyScheduleValue;

        public List<double> MonthlySchedule

        {

            get { return this.MonthlyScheduleValue; }

            set { SetProperty(ref MonthlyScheduleValue, value); }

        }
        private double BreakevenValue;

        public double Breakeven

        {

            get { return this.BreakevenValue; }

            set { SetProperty(ref BreakevenValue, value); }

        }
        private double MarginValue;

        public double Margin

        {

            get { return this.MarginValue; }

            set { SetProperty(ref MarginValue, value); }

        }
        private List<double> MonthlyRevenueValue;

        public List<double> MonthlyRevenue

        {

            get { return this.MonthlyRevenueValue; }

            set { SetProperty(ref MonthlyRevenueValue, value); }

        }
        private List<double> MonthlyCashFlowValue;

        public List<double> MonthlyCashFlow

        {

            get { return this.MonthlyCashFlowValue; }

            set { SetProperty(ref MonthlyCashFlowValue, value); }

        }
        private double TotalProjectRevenueValue;

        public double TotalProjectRevenue

        {

            get { return this.TotalProjectRevenueValue; }

            set { SetProperty(ref TotalProjectRevenueValue, value); }

        }
        private double TotalProjectCostValue;

        public double TotalProjectCost

        {

            get { return this.TotalProjectCostValue; }

            set { SetProperty(ref TotalProjectCostValue, value); }

        }
        private double NetCashFlowValue;

        public double NetCashFlow

        {

            get { return this.NetCashFlowValue; }

            set { SetProperty(ref NetCashFlowValue, value); }

        }
    }

    /// <summary>
    /// Environmental impact assessment results
    /// </summary>
    public class EnvironmentalImpactAssessmentResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AssessmentDateValue;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        private string LocationValue;

        public string Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private string EnvironmentalSensitivityValue;

        public string EnvironmentalSensitivity

        {

            get { return this.EnvironmentalSensitivityValue; }

            set { SetProperty(ref EnvironmentalSensitivityValue, value); }

        }
        private string AirQualityImpactValue;

        public string AirQualityImpact

        {

            get { return this.AirQualityImpactValue; }

            set { SetProperty(ref AirQualityImpactValue, value); }

        }
        private string WaterQualityImpactValue;

        public string WaterQualityImpact

        {

            get { return this.WaterQualityImpactValue; }

            set { SetProperty(ref WaterQualityImpactValue, value); }

        }
        private string SoilAndLandImpactValue;

        public string SoilAndLandImpact

        {

            get { return this.SoilAndLandImpactValue; }

            set { SetProperty(ref SoilAndLandImpactValue, value); }

        }
        private string BiodiversityImpactValue;

        public string BiodiversityImpact

        {

            get { return this.BiodiversityImpactValue; }

            set { SetProperty(ref BiodiversityImpactValue, value); }

        }
        private double GHGEmissionsValue;

        public double GHGEmissions

        {

            get { return this.GHGEmissionsValue; }

            set { SetProperty(ref GHGEmissionsValue, value); }

        }
        private List<string> MitigationMeasuresValue;

        public List<string> MitigationMeasures

        {

            get { return this.MitigationMeasuresValue; }

            set { SetProperty(ref MitigationMeasuresValue, value); }

        }
        private List<string> ComplianceRequirementsValue;

        public List<string> ComplianceRequirements

        {

            get { return this.ComplianceRequirementsValue; }

            set { SetProperty(ref ComplianceRequirementsValue, value); }

        }
        private double CostOfMitigationValue;

        public double CostOfMitigation

        {

            get { return this.CostOfMitigationValue; }

            set { SetProperty(ref CostOfMitigationValue, value); }

        }
    }

    /// <summary>
    /// Risk analysis results
    /// </summary>
    public class RiskAnalysisResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private int ProjectDurationValue;

        public int ProjectDuration

        {

            get { return this.ProjectDurationValue; }

            set { SetProperty(ref ProjectDurationValue, value); }

        }
        private double ProjectedNPVValue;

        public double ProjectedNPV

        {

            get { return this.ProjectedNPVValue; }

            set { SetProperty(ref ProjectedNPVValue, value); }

        }
        private List<RiskItem> TechnicalRisksValue;

        public List<RiskItem> TechnicalRisks

        {

            get { return this.TechnicalRisksValue; }

            set { SetProperty(ref TechnicalRisksValue, value); }

        }
        private List<RiskItem> CommercialRisksValue;

        public List<RiskItem> CommercialRisks

        {

            get { return this.CommercialRisksValue; }

            set { SetProperty(ref CommercialRisksValue, value); }

        }
        private List<RiskItem> OperationalRisksValue;

        public List<RiskItem> OperationalRisks

        {

            get { return this.OperationalRisksValue; }

            set { SetProperty(ref OperationalRisksValue, value); }

        }
        private List<RiskItem> RegulatoryRisksValue;

        public List<RiskItem> RegulatoryRisks

        {

            get { return this.RegulatoryRisksValue; }

            set { SetProperty(ref RegulatoryRisksValue, value); }

        }
        private List<RiskItem> EnvironmentalRisksValue;

        public List<RiskItem> EnvironmentalRisks

        {

            get { return this.EnvironmentalRisksValue; }

            set { SetProperty(ref EnvironmentalRisksValue, value); }

        }
        private int TotalIdentifiedRisksValue;

        public int TotalIdentifiedRisks

        {

            get { return this.TotalIdentifiedRisksValue; }

            set { SetProperty(ref TotalIdentifiedRisksValue, value); }

        }
        private List<RiskItem> HighPriorityRisksValue;

        public List<RiskItem> HighPriorityRisks

        {

            get { return this.HighPriorityRisksValue; }

            set { SetProperty(ref HighPriorityRisksValue, value); }

        }
        private List<string> MitigationStrategiesValue;

        public List<string> MitigationStrategies

        {

            get { return this.MitigationStrategiesValue; }

            set { SetProperty(ref MitigationStrategiesValue, value); }

        }
        private double ContingencyReserveValue;

        public double ContingencyReserve

        {

            get { return this.ContingencyReserveValue; }

            set { SetProperty(ref ContingencyReserveValue, value); }

        }
        private string OverallRiskRatingValue;

        public string OverallRiskRating

        {

            get { return this.OverallRiskRatingValue; }

            set { SetProperty(ref OverallRiskRatingValue, value); }

        }
    }

    /// <summary>
    /// Individual risk item
    /// </summary>
    public class RiskItem : ModelEntityBase
    {
        private string RiskIdValue;

        public string RiskId

        {

            get { return this.RiskIdValue; }

            set { SetProperty(ref RiskIdValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private double ProbabilityValue;

        public double Probability

        {

            get { return this.ProbabilityValue; }

            set { SetProperty(ref ProbabilityValue, value); }

        }
        private double ImpactValue;

        public double Impact

        {

            get { return this.ImpactValue; }

            set { SetProperty(ref ImpactValue, value); }

        }
    }

    /// <summary>
    /// Facility design results
    /// </summary>
    public class FacilityDesignResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime DesignDateValue;

        public DateTime DesignDate

        {

            get { return this.DesignDateValue; }

            set { SetProperty(ref DesignDateValue, value); }

        }
        private double ProductionRateValue;

        public double ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
        private string ProductTypeValue;

        public string ProductType

        {

            get { return this.ProductTypeValue; }

            set { SetProperty(ref ProductTypeValue, value); }

        }
        private List<ProcessingEquipment> SeparatorSpecificationsValue;

        public List<ProcessingEquipment> SeparatorSpecifications

        {

            get { return this.SeparatorSpecificationsValue; }

            set { SetProperty(ref SeparatorSpecificationsValue, value); }

        }
        private List<ProcessingEquipment> CompressorRequirementsValue;

        public List<ProcessingEquipment> CompressorRequirements

        {

            get { return this.CompressorRequirementsValue; }

            set { SetProperty(ref CompressorRequirementsValue, value); }

        }
        private List<ProcessingEquipment> PumpingRequirementsValue;

        public List<ProcessingEquipment> PumpingRequirements

        {

            get { return this.PumpingRequirementsValue; }

            set { SetProperty(ref PumpingRequirementsValue, value); }

        }
        private List<ProcessingEquipment> HeatExchangerSpecificationsValue;

        public List<ProcessingEquipment> HeatExchangerSpecifications

        {

            get { return this.HeatExchangerSpecificationsValue; }

            set { SetProperty(ref HeatExchangerSpecificationsValue, value); }

        }
        private List<string> ControlSystemsSpecificationValue;

        public List<string> ControlSystemsSpecification

        {

            get { return this.ControlSystemsSpecificationValue; }

            set { SetProperty(ref ControlSystemsSpecificationValue, value); }

        }
        private List<string> LaySgDownRequirementsValue;

        public List<string> LaySgDownRequirements

        {

            get { return this.LaySgDownRequirementsValue; }

            set { SetProperty(ref LaySgDownRequirementsValue, value); }

        }
        private List<string> UtilityRequirementsValue;

        public List<string> UtilityRequirements

        {

            get { return this.UtilityRequirementsValue; }

            set { SetProperty(ref UtilityRequirementsValue, value); }

        }
        private double CostEstimateValue;

        public double CostEstimate

        {

            get { return this.CostEstimateValue; }

            set { SetProperty(ref CostEstimateValue, value); }

        }
    }

    /// <summary>
    /// Processing equipment specification
    /// </summary>
    public class ProcessingEquipment : ModelEntityBase
    {
        private string EquipmentTypeValue;

        public string EquipmentType

        {

            get { return this.EquipmentTypeValue; }

            set { SetProperty(ref EquipmentTypeValue, value); }

        }
        private string SizeValue;

        public string Size

        {

            get { return this.SizeValue; }

            set { SetProperty(ref SizeValue, value); }

        }
        private double CapacityValue;

        public double Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
    }

    /// <summary>
    /// Investment evaluation results
    /// </summary>
    public class InvestmentEvaluationResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime EvaluationDateValue;

        public DateTime EvaluationDate

        {

            get { return this.EvaluationDateValue; }

            set { SetProperty(ref EvaluationDateValue, value); }

        }
        private double InitialCapexValue;

        public double InitialCapex

        {

            get { return this.InitialCapexValue; }

            set { SetProperty(ref InitialCapexValue, value); }

        }
        private double DiscountRateValue;

        public double DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
        private List<double> ProjectedCashFlowsValue;

        public List<double> ProjectedCashFlows

        {

            get { return this.ProjectedCashFlowsValue; }

            set { SetProperty(ref ProjectedCashFlowsValue, value); }

        }
        private double NPVValue;

        public double NPV

        {

            get { return this.NPVValue; }

            set { SetProperty(ref NPVValue, value); }

        }
        private double IRRValue;

        public double IRR

        {

            get { return this.IRRValue; }

            set { SetProperty(ref IRRValue, value); }

        }
        private double PaybackPeriodValue;

        public double PaybackPeriod

        {

            get { return this.PaybackPeriodValue; }

            set { SetProperty(ref PaybackPeriodValue, value); }

        }
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }
        private double TotalProjectValueValue;

        public double TotalProjectValue

        {

            get { return this.TotalProjectValueValue; }

            set { SetProperty(ref TotalProjectValueValue, value); }

        }
        private SensitivityAnalysisResult SensitivityAnalysisValue;

        public SensitivityAnalysisResult SensitivityAnalysis

        {

            get { return this.SensitivityAnalysisValue; }

            set { SetProperty(ref SensitivityAnalysisValue, value); }

        }
        private ScenarioAnalysisResult ScenarioAnalysisValue;

        public ScenarioAnalysisResult ScenarioAnalysis

        {

            get { return this.ScenarioAnalysisValue; }

            set { SetProperty(ref ScenarioAnalysisValue, value); }

        }
        private string InvestmentRatingValue;

        public string InvestmentRating

        {

            get { return this.InvestmentRatingValue; }

            set { SetProperty(ref InvestmentRatingValue, value); }

        }
    }

    /// <summary>
    /// Sensitivity analysis results
    /// </summary>
    public class SensitivityAnalysisResult : ModelEntityBase
    {
        private List<double> PriceVariationValue;

        public List<double> PriceVariation

        {

            get { return this.PriceVariationValue; }

            set { SetProperty(ref PriceVariationValue, value); }

        }
        private List<double> VolumeVariationValue;

        public List<double> VolumeVariation

        {

            get { return this.VolumeVariationValue; }

            set { SetProperty(ref VolumeVariationValue, value); }

        }
        private List<double> CostVariationValue;

        public List<double> CostVariation

        {

            get { return this.CostVariationValue; }

            set { SetProperty(ref CostVariationValue, value); }

        }
    }

    /// <summary>
    /// Scenario analysis results
    /// </summary>
    public class ScenarioAnalysisResult : ModelEntityBase
    {
        private double BaseCaseValue;

        public double BaseCase

        {

            get { return this.BaseCaseValue; }

            set { SetProperty(ref BaseCaseValue, value); }

        }
        private double DownsideCaseValue;

        public double DownsideCase

        {

            get { return this.DownsideCaseValue; }

            set { SetProperty(ref DownsideCaseValue, value); }

        }
        private double UpsideCaseValue;

        public double UpsideCase

        {

            get { return this.UpsideCaseValue; }

            set { SetProperty(ref UpsideCaseValue, value); }

        }
        private double ProbabilityBaseCaseValue;

        public double ProbabilityBaseCase

        {

            get { return this.ProbabilityBaseCaseValue; }

            set { SetProperty(ref ProbabilityBaseCaseValue, value); }

        }
        private double ProbabilityDownsideValue;

        public double ProbabilityDownside

        {

            get { return this.ProbabilityDownsideValue; }

            set { SetProperty(ref ProbabilityDownsideValue, value); }

        }
        private double ProbabilityUpsideValue;

        public double ProbabilityUpside

        {

            get { return this.ProbabilityUpsideValue; }

            set { SetProperty(ref ProbabilityUpsideValue, value); }

        }
    }

    /// <summary>
    /// Development phase schedule
    /// </summary>
    public class DevelopmentPhaseScheduleResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime ScheduleDateValue;

        public DateTime ScheduleDate

        {

            get { return this.ScheduleDateValue; }

            set { SetProperty(ref ScheduleDateValue, value); }

        }
        private DateTime DevelopmentStartDateValue;

        public DateTime DevelopmentStartDate

        {

            get { return this.DevelopmentStartDateValue; }

            set { SetProperty(ref DevelopmentStartDateValue, value); }

        }
        private int PhaseCountValue;

        public int PhaseCount

        {

            get { return this.PhaseCountValue; }

            set { SetProperty(ref PhaseCountValue, value); }

        }
        private List<DevelopmentPhase> PhasesValue;

        public List<DevelopmentPhase> Phases

        {

            get { return this.PhasesValue; }

            set { SetProperty(ref PhasesValue, value); }

        }
        private int TotalProjectDurationValue;

        public int TotalProjectDuration

        {

            get { return this.TotalProjectDurationValue; }

            set { SetProperty(ref TotalProjectDurationValue, value); }

        }
        private string CriticalPathValue;

        public string CriticalPath

        {

            get { return this.CriticalPathValue; }

            set { SetProperty(ref CriticalPathValue, value); }

        }
        private DateTime ProjectCompletionValue;

        public DateTime ProjectCompletion

        {

            get { return this.ProjectCompletionValue; }

            set { SetProperty(ref ProjectCompletionValue, value); }

        }
    }

    /// <summary>
    /// Individual development phase with milestones
    /// </summary>
    public class DevelopmentPhase : ModelEntityBase
    {
        private int PhaseNumberValue;

        public int PhaseNumber

        {

            get { return this.PhaseNumberValue; }

            set { SetProperty(ref PhaseNumberValue, value); }

        }
        private string PhaseNameValue;

        public string PhaseName

        {

            get { return this.PhaseNameValue; }

            set { SetProperty(ref PhaseNameValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private int DurationValue;

        public int Duration

        {

            get { return this.DurationValue; }

            set { SetProperty(ref DurationValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string> MilestonesValue;

        public List<string> Milestones

        {

            get { return this.MilestonesValue; }

            set { SetProperty(ref MilestonesValue, value); }

        }
        private List<int> DependenciesValue;

        public List<int> Dependencies

        {

            get { return this.DependenciesValue; }

            set { SetProperty(ref DependenciesValue, value); }

        }
    }

    /// <summary>
    /// Development strategy definition
    /// </summary>
    public class DevelopmentStrategy : ModelEntityBase
    {
        private string StrategyNameValue;

        public string StrategyName

        {

            get { return this.StrategyNameValue; }

            set { SetProperty(ref StrategyNameValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private double EstimatedCapexValue;

        public double EstimatedCapex

        {

            get { return this.EstimatedCapexValue; }

            set { SetProperty(ref EstimatedCapexValue, value); }

        }
        private double ProjectNPVValue;

        public double ProjectNPV

        {

            get { return this.ProjectNPVValue; }

            set { SetProperty(ref ProjectNPVValue, value); }

        }
        private double ProjectIRRValue;

        public double ProjectIRR

        {

            get { return this.ProjectIRRValue; }

            set { SetProperty(ref ProjectIRRValue, value); }

        }
        private double RiskLevelValue;

        public double RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private int ProjectDurationValue;

        public int ProjectDuration

        {

            get { return this.ProjectDurationValue; }

            set { SetProperty(ref ProjectDurationValue, value); }

        }
        private double EstimatedEmissionsValue;

        public double EstimatedEmissions

        {

            get { return this.EstimatedEmissionsValue; }

            set { SetProperty(ref EstimatedEmissionsValue, value); }

        }
        private List<string> KeyBenefitsValue;

        public List<string> KeyBenefits

        {

            get { return this.KeyBenefitsValue; }

            set { SetProperty(ref KeyBenefitsValue, value); }

        }
        private List<string> KeyChallengesValue;

        public List<string> KeyChallenges

        {

            get { return this.KeyChallengesValue; }

            set { SetProperty(ref KeyChallengesValue, value); }

        }
    }

    /// <summary>
    /// Strategy ranking entry
    /// </summary>
    public class StrategyRanking : ModelEntityBase
    {
        private int RankValue;

        public int Rank

        {

            get { return this.RankValue; }

            set { SetProperty(ref RankValue, value); }

        }
        private string StrategyValue;

        public string Strategy

        {

            get { return this.StrategyValue; }

            set { SetProperty(ref StrategyValue, value); }

        }
        private double ScoreValue;

        public double Score

        {

            get { return this.ScoreValue; }

            set { SetProperty(ref ScoreValue, value); }

        }
    }

    /// <summary>
    /// Alternative strategies comparison results
    /// </summary>
    public class StrategiesComparisonResult : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime ComparisonDateValue;

        public DateTime ComparisonDate

        {

            get { return this.ComparisonDateValue; }

            set { SetProperty(ref ComparisonDateValue, value); }

        }
        private int StrategyCountValue;

        public int StrategyCount

        {

            get { return this.StrategyCountValue; }

            set { SetProperty(ref StrategyCountValue, value); }

        }
        private List<DevelopmentStrategy> StrategiesValue;

        public List<DevelopmentStrategy> Strategies

        {

            get { return this.StrategiesValue; }

            set { SetProperty(ref StrategiesValue, value); }

        }
        private CostComparisonResult CostComparisonValue;

        public CostComparisonResult CostComparison

        {

            get { return this.CostComparisonValue; }

            set { SetProperty(ref CostComparisonValue, value); }

        }
        private EconomicComparisonResult EconomicComparisonValue;

        public EconomicComparisonResult EconomicComparison

        {

            get { return this.EconomicComparisonValue; }

            set { SetProperty(ref EconomicComparisonValue, value); }

        }
        private RiskComparisonResult RiskComparisonValue;

        public RiskComparisonResult RiskComparison

        {

            get { return this.RiskComparisonValue; }

            set { SetProperty(ref RiskComparisonValue, value); }

        }
        private ScheduleComparisonResult ScheduleComparisonValue;

        public ScheduleComparisonResult ScheduleComparison

        {

            get { return this.ScheduleComparisonValue; }

            set { SetProperty(ref ScheduleComparisonValue, value); }

        }
        private EnvironmentalComparisonResult EnvironmentalComparisonValue;

        public EnvironmentalComparisonResult EnvironmentalComparison

        {

            get { return this.EnvironmentalComparisonValue; }

            set { SetProperty(ref EnvironmentalComparisonValue, value); }

        }
        private string RecommendedStrategyValue;

        public string RecommendedStrategy

        {

            get { return this.RecommendedStrategyValue; }

            set { SetProperty(ref RecommendedStrategyValue, value); }

        }
        private List<StrategyRanking> AlternativeRankingsValue;

        public List<StrategyRanking> AlternativeRankings

        {

            get { return this.AlternativeRankingsValue; }

            set { SetProperty(ref AlternativeRankingsValue, value); }

        }
        private TradeOffAnalysisResult TradeOffAnalysisValue;

        public TradeOffAnalysisResult TradeOffAnalysis

        {

            get { return this.TradeOffAnalysisValue; }

            set { SetProperty(ref TradeOffAnalysisValue, value); }

        }
    }

    /// <summary>
    /// Cost comparison between strategies
    /// </summary>
    public class CostComparisonResult : ModelEntityBase
    {
        private double LowestCostValue;

        public double LowestCost

        {

            get { return this.LowestCostValue; }

            set { SetProperty(ref LowestCostValue, value); }

        }
        private double HighestCostValue;

        public double HighestCost

        {

            get { return this.HighestCostValue; }

            set { SetProperty(ref HighestCostValue, value); }

        }
        private double AverageCostValue;

        public double AverageCost

        {

            get { return this.AverageCostValue; }

            set { SetProperty(ref AverageCostValue, value); }

        }
    }

    /// <summary>
    /// Economic comparison between strategies
    /// </summary>
    public class EconomicComparisonResult : ModelEntityBase
    {
        private double HighestNPVValue;

        public double HighestNPV

        {

            get { return this.HighestNPVValue; }

            set { SetProperty(ref HighestNPVValue, value); }

        }
        private double LowestNPVValue;

        public double LowestNPV

        {

            get { return this.LowestNPVValue; }

            set { SetProperty(ref LowestNPVValue, value); }

        }
        private double BestIRRValue;

        public double BestIRR

        {

            get { return this.BestIRRValue; }

            set { SetProperty(ref BestIRRValue, value); }

        }
    }

    /// <summary>
    /// Risk comparison between strategies
    /// </summary>
    public class RiskComparisonResult : ModelEntityBase
    {
        private double LowestRiskValue;

        public double LowestRisk

        {

            get { return this.LowestRiskValue; }

            set { SetProperty(ref LowestRiskValue, value); }

        }
        private double HighestRiskValue;

        public double HighestRisk

        {

            get { return this.HighestRiskValue; }

            set { SetProperty(ref HighestRiskValue, value); }

        }
    }

    /// <summary>
    /// Schedule comparison between strategies
    /// </summary>
    public class ScheduleComparisonResult : ModelEntityBase
    {
        private int ShortestDurationValue;

        public int ShortestDuration

        {

            get { return this.ShortestDurationValue; }

            set { SetProperty(ref ShortestDurationValue, value); }

        }
        private int LongestDurationValue;

        public int LongestDuration

        {

            get { return this.LongestDurationValue; }

            set { SetProperty(ref LongestDurationValue, value); }

        }
    }

    /// <summary>
    /// Environmental comparison between strategies
    /// </summary>
    public class EnvironmentalComparisonResult : ModelEntityBase
    {
        private double LowestEmissionsValue;

        public double LowestEmissions

        {

            get { return this.LowestEmissionsValue; }

            set { SetProperty(ref LowestEmissionsValue, value); }

        }
        private double HighestEmissionsValue;

        public double HighestEmissions

        {

            get { return this.HighestEmissionsValue; }

            set { SetProperty(ref HighestEmissionsValue, value); }

        }
    }

    /// <summary>
    /// Trade-off analysis between strategies
    /// </summary>
    public class TradeOffAnalysisResult : ModelEntityBase
    {
        private string CostVsScheduleValue;

        public string CostVsSchedule

        {

            get { return this.CostVsScheduleValue; }

            set { SetProperty(ref CostVsScheduleValue, value); }

        }
        private string RiskVsRewardValue;

        public string RiskVsReward

        {

            get { return this.RiskVsRewardValue; }

            set { SetProperty(ref RiskVsRewardValue, value); }

        }
        private string EnvironmentalVsEconomicValue;

        public string EnvironmentalVsEconomic

        {

            get { return this.EnvironmentalVsEconomicValue; }

            set { SetProperty(ref EnvironmentalVsEconomicValue, value); }

        }
    }
}



