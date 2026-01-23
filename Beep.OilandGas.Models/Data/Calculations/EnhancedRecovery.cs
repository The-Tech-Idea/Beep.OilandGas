using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Comprehensive set of DTOs for Enhanced Oil Recovery (EOR) analysis.
    /// Covers waterflooding, gas injection, chemical EOR, thermal recovery, and economic analysis.
    /// </summary>

    /// <summary>
    /// DTO for waterflooding performance analysis results
    /// </summary>
    public class WaterfloodPerformanceAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private int DataPointsAnalyzedValue;

        public int DataPointsAnalyzed

        {

            get { return this.DataPointsAnalyzedValue; }

            set { SetProperty(ref DataPointsAnalyzedValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double CumulativeProductionValue;

        public double CumulativeProduction

        {

            get { return this.CumulativeProductionValue; }

            set { SetProperty(ref CumulativeProductionValue, value); }

        }
        private double RecoveryFactorValue;

        public double RecoveryFactor

        {

            get { return this.RecoveryFactorValue; }

            set { SetProperty(ref RecoveryFactorValue, value); }

        }
        private double IncrementalRecoveryFactorValue;

        public double IncrementalRecoveryFactor

        {

            get { return this.IncrementalRecoveryFactorValue; }

            set { SetProperty(ref IncrementalRecoveryFactorValue, value); }

        }
        private double PressureMaintenanceEfficiencyValue;

        public double PressureMaintenanceEfficiency

        {

            get { return this.PressureMaintenanceEfficiencyValue; }

            set { SetProperty(ref PressureMaintenanceEfficiencyValue, value); }

        }
        private WaterCutTrend WaterCutTrendValue;

        public WaterCutTrend WaterCutTrend

        {

            get { return this.WaterCutTrendValue; }

            set { SetProperty(ref WaterCutTrendValue, value); }

        }
        private double FloodFrontVelocityValue;

        public double FloodFrontVelocity

        {

            get { return this.FloodFrontVelocityValue; }

            set { SetProperty(ref FloodFrontVelocityValue, value); }

        }
        private double SweepEfficiencyValue;

        public double SweepEfficiency

        {

            get { return this.SweepEfficiencyValue; }

            set { SetProperty(ref SweepEfficiencyValue, value); }

        }
        private double ProjectedRecovery20YearsValue;

        public double ProjectedRecovery20Years

        {

            get { return this.ProjectedRecovery20YearsValue; }

            set { SetProperty(ref ProjectedRecovery20YearsValue, value); }

        }
    }

    /// <summary>
    /// DTO for water cut trend analysis
    /// </summary>
    public class WaterCutTrend : ModelEntityBase
    {
        private double InitialWaterCutValue;

        public double InitialWaterCut

        {

            get { return this.InitialWaterCutValue; }

            set { SetProperty(ref InitialWaterCutValue, value); }

        }
        private double FinalWaterCutValue;

        public double FinalWaterCut

        {

            get { return this.FinalWaterCutValue; }

            set { SetProperty(ref FinalWaterCutValue, value); }

        }
        private double RateOfIncreasePerMonthValue;

        public double RateOfIncreasePerMonth

        {

            get { return this.RateOfIncreasePerMonthValue; }

            set { SetProperty(ref RateOfIncreasePerMonthValue, value); }

        }
        private double TimeToHighWaterCutValue;

        public double TimeToHighWaterCut

        {

            get { return this.TimeToHighWaterCutValue; }

            set { SetProperty(ref TimeToHighWaterCutValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas injection EOR analysis
    /// </summary>
    public class GasInjectionAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string GasTypeValue;

        public string GasType

        {

            get { return this.GasTypeValue; }

            set { SetProperty(ref GasTypeValue, value); }

        }
        private double InjectionPressureValue;

        public double InjectionPressure

        {

            get { return this.InjectionPressureValue; }

            set { SetProperty(ref InjectionPressureValue, value); }

        }
        private double MinimumMiscibilityPressureValue;

        public double MinimumMiscibilityPressure

        {

            get { return this.MinimumMiscibilityPressureValue; }

            set { SetProperty(ref MinimumMiscibilityPressureValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private bool IsMiscibleValue;

        public bool IsMiscible

        {

            get { return this.IsMiscibleValue; }

            set { SetProperty(ref IsMiscibleValue, value); }

        }
        private string DisplacementMechanismValue;

        public string DisplacementMechanism

        {

            get { return this.DisplacementMechanismValue; }

            set { SetProperty(ref DisplacementMechanismValue, value); }

        }
        private double DisplacementEfficiencyValue;

        public double DisplacementEfficiency

        {

            get { return this.DisplacementEfficiencyValue; }

            set { SetProperty(ref DisplacementEfficiencyValue, value); }

        }
        private double ResidualOilSaturationValue;

        public double ResidualOilSaturation

        {

            get { return this.ResidualOilSaturationValue; }

            set { SetProperty(ref ResidualOilSaturationValue, value); }

        }
        private double ProductionImprovementValue;

        public double ProductionImprovement

        {

            get { return this.ProductionImprovementValue; }

            set { SetProperty(ref ProductionImprovementValue, value); }

        }
        private double TertiaryRecoveryPotentialValue;

        public double TertiaryRecoveryPotential

        {

            get { return this.TertiaryRecoveryPotentialValue; }

            set { SetProperty(ref TertiaryRecoveryPotentialValue, value); }

        }
        private GasTypeCharacteristics GasTypeCharacteristicsValue;

        public GasTypeCharacteristics GasTypeCharacteristics

        {

            get { return this.GasTypeCharacteristicsValue; }

            set { SetProperty(ref GasTypeCharacteristicsValue, value); }

        }
    }

    /// <summary>
    /// DTO for gas type specific characteristics
    /// </summary>
    public class GasTypeCharacteristics : ModelEntityBase
    {
        private string GasTypeValue;

        public string GasType

        {

            get { return this.GasTypeValue; }

            set { SetProperty(ref GasTypeValue, value); }

        }
        private double DensityValue;

        public double Density

        {

            get { return this.DensityValue; }

            set { SetProperty(ref DensityValue, value); }

        }
        private double CriticalTemperatureValue;

        public double CriticalTemperature

        {

            get { return this.CriticalTemperatureValue; }

            set { SetProperty(ref CriticalTemperatureValue, value); }

        }
        private double CriticalPressureValue;

        public double CriticalPressure

        {

            get { return this.CriticalPressureValue; }

            set { SetProperty(ref CriticalPressureValue, value); }

        }
        private string MiscibilityAdvantageValue;

        public string MiscibilityAdvantage

        {

            get { return this.MiscibilityAdvantageValue; }

            set { SetProperty(ref MiscibilityAdvantageValue, value); }

        }
    }

    /// <summary>
    /// DTO for chemical EOR analysis
    /// </summary>
    public class ChemicalEORAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string ChemicalTypeValue;

        public string ChemicalType

        {

            get { return this.ChemicalTypeValue; }

            set { SetProperty(ref ChemicalTypeValue, value); }

        }
        private double ReservoirTemperatureValue;

        public double ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private double SalinityValue;

        public double Salinity

        {

            get { return this.SalinityValue; }

            set { SetProperty(ref SalinityValue, value); }

        }
        private double CrudePaveViscosityValue;

        public double CrudePaveViscosity

        {

            get { return this.CrudePaveViscosityValue; }

            set { SetProperty(ref CrudePaveViscosityValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string SuitabilityValue;

        public string Suitability

        {

            get { return this.SuitabilityValue; }

            set { SetProperty(ref SuitabilityValue, value); }

        }
        private double InterfacialTensionReductionValue;

        public double InterfacialTensionReduction

        {

            get { return this.InterfacialTensionReductionValue; }

            set { SetProperty(ref InterfacialTensionReductionValue, value); }

        }
        private double OilRecoveryIncrementValue;

        public double OilRecoveryIncrement

        {

            get { return this.OilRecoveryIncrementValue; }

            set { SetProperty(ref OilRecoveryIncrementValue, value); }

        }
        private double CostPerBarrelRecoveredValue;

        public double CostPerBarrelRecovered

        {

            get { return this.CostPerBarrelRecoveredValue; }

            set { SetProperty(ref CostPerBarrelRecoveredValue, value); }

        }
        private List<string> EnvironmentalConcernsValue = new();

        public List<string> EnvironmentalConcerns

        {

            get { return this.EnvironmentalConcernsValue; }

            set { SetProperty(ref EnvironmentalConcernsValue, value); }

        }
        private ChemicalParameters ChemicalParametersValue;

        public ChemicalParameters ChemicalParameters

        {

            get { return this.ChemicalParametersValue; }

            set { SetProperty(ref ChemicalParametersValue, value); }

        }
    }

    /// <summary>
    /// DTO for chemical-specific parameters
    /// </summary>
    public class ChemicalParameters : ModelEntityBase
    {
        private string ChemicalTypeValue;

        public string ChemicalType

        {

            get { return this.ChemicalTypeValue; }

            set { SetProperty(ref ChemicalTypeValue, value); }

        }
        private double OptimalTemperatureValue;

        public double OptimalTemperature

        {

            get { return this.OptimalTemperatureValue; }

            set { SetProperty(ref OptimalTemperatureValue, value); }

        }
        private double OptimalSalinityValue;

        public double OptimalSalinity

        {

            get { return this.OptimalSalinityValue; }

            set { SetProperty(ref OptimalSalinityValue, value); }

        }
        private double DegradationRateValue;

        public double DegradationRate

        {

            get { return this.DegradationRateValue; }

            set { SetProperty(ref DegradationRateValue, value); }

        }
        private double AdsorptionFactorValue;

        public double AdsorptionFactor

        {

            get { return this.AdsorptionFactorValue; }

            set { SetProperty(ref AdsorptionFactorValue, value); }

        }
    }

    /// <summary>
    /// DTO for thermal recovery analysis
    /// </summary>
    public class ThermalRecoveryAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string ThermalMethodValue;

        public string ThermalMethod

        {

            get { return this.ThermalMethodValue; }

            set { SetProperty(ref ThermalMethodValue, value); }

        }
        private double ReservoirTemperatureValue;

        public double ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private double CrudePaveViscosityValue;

        public double CrudePaveViscosity

        {

            get { return this.CrudePaveViscosityValue; }

            set { SetProperty(ref CrudePaveViscosityValue, value); }

        }
        private double OilSaturationValue;

        public double OilSaturation

        {

            get { return this.OilSaturationValue; }

            set { SetProperty(ref OilSaturationValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string SuitabilityValue;

        public string Suitability

        {

            get { return this.SuitabilityValue; }

            set { SetProperty(ref SuitabilityValue, value); }

        }
        private double ViscosityReductionValue;

        public double ViscosityReduction

        {

            get { return this.ViscosityReductionValue; }

            set { SetProperty(ref ViscosityReductionValue, value); }

        }
        private double MobilityImprovementValue;

        public double MobilityImprovement

        {

            get { return this.MobilityImprovementValue; }

            set { SetProperty(ref MobilityImprovementValue, value); }

        }
        private double EnergyRequirementValue;

        public double EnergyRequirement

        {

            get { return this.EnergyRequirementValue; }

            set { SetProperty(ref EnergyRequirementValue, value); }

        }
        private double ProjectedRecoveryFactorValue;

        public double ProjectedRecoveryFactor

        {

            get { return this.ProjectedRecoveryFactorValue; }

            set { SetProperty(ref ProjectedRecoveryFactorValue, value); }

        }
        private double OperatingCostPerBarrelValue;

        public double OperatingCostPerBarrel

        {

            get { return this.OperatingCostPerBarrelValue; }

            set { SetProperty(ref OperatingCostPerBarrelValue, value); }

        }
        private string EnvironmentalImpactValue;

        public string EnvironmentalImpact

        {

            get { return this.EnvironmentalImpactValue; }

            set { SetProperty(ref EnvironmentalImpactValue, value); }

        }
    }

    /// <summary>
    /// DTO for comparing multiple EOR methods
    /// </summary>
    public class EORMethodComparison : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private int MethodsComparedValue;

        public int MethodsCompared

        {

            get { return this.MethodsComparedValue; }

            set { SetProperty(ref MethodsComparedValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        public Dictionary<string, EORMethodScore> MethodScores { get; set; } = new();
        private List<RankedEORMethod> RankedMethodsValue = new();

        public List<RankedEORMethod> RankedMethods

        {

            get { return this.RankedMethodsValue; }

            set { SetProperty(ref RankedMethodsValue, value); }

        }
        private string RecommendedMethodValue;

        public string RecommendedMethod

        {

            get { return this.RecommendedMethodValue; }

            set { SetProperty(ref RecommendedMethodValue, value); }

        }
        private double SynergyPotentialValue;

        public double SynergyPotential

        {

            get { return this.SynergyPotentialValue; }

            set { SetProperty(ref SynergyPotentialValue, value); }

        }
    }

    /// <summary>
    /// DTO for EOR method scoring criteria
    /// </summary>
    public class EORMethodScore : ModelEntityBase
    {
        private string MethodValue;

        public string Method

        {

            get { return this.MethodValue; }

            set { SetProperty(ref MethodValue, value); }

        }
        private double TemperatureSuitabilityValue;

        public double TemperatureSuitability

        {

            get { return this.TemperatureSuitabilityValue; }

            set { SetProperty(ref TemperatureSuitabilityValue, value); }

        }
        private double ViscositySuitabilityValue;

        public double ViscositySuitability

        {

            get { return this.ViscositySuitabilityValue; }

            set { SetProperty(ref ViscositySuitabilityValue, value); }

        }
        private double SaturationSuitabilityValue;

        public double SaturationSuitability

        {

            get { return this.SaturationSuitabilityValue; }

            set { SetProperty(ref SaturationSuitabilityValue, value); }

        }
        private double OverallScoreValue;

        public double OverallScore

        {

            get { return this.OverallScoreValue; }

            set { SetProperty(ref OverallScoreValue, value); }

        }
    }

    /// <summary>
    /// DTO for ranked EOR methods
    /// </summary>
    public class RankedEORMethod : ModelEntityBase
    {
        private int RankValue;

        public int Rank

        {

            get { return this.RankValue; }

            set { SetProperty(ref RankValue, value); }

        }
        private string MethodNameValue;

        public string MethodName

        {

            get { return this.MethodNameValue; }

            set { SetProperty(ref MethodNameValue, value); }

        }
        private EORMethodScore ScoreValue;

        public EORMethodScore Score

        {

            get { return this.ScoreValue; }

            set { SetProperty(ref ScoreValue, value); }

        }
    }

    /// <summary>
    /// DTO for injection well placement optimization
    /// </summary>
    public class InjectionWellOptimization : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private int DesiredWellCountValue;

        public int DesiredWellCount

        {

            get { return this.DesiredWellCountValue; }

            set { SetProperty(ref DesiredWellCountValue, value); }

        }
        private double ReservoirAreaValue;

        public double ReservoirArea

        {

            get { return this.ReservoirAreaValue; }

            set { SetProperty(ref ReservoirAreaValue, value); }

        }
        private double ReservoirThicknessValue;

        public double ReservoirThickness

        {

            get { return this.ReservoirThicknessValue; }

            set { SetProperty(ref ReservoirThicknessValue, value); }

        }
        private double PermeabilityValue;

        public double Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double OptimalWellSpacingValue;

        public double OptimalWellSpacing

        {

            get { return this.OptimalWellSpacingValue; }

            set { SetProperty(ref OptimalWellSpacingValue, value); }

        }
        private double AreaPerWellValue;

        public double AreaPerWell

        {

            get { return this.AreaPerWellValue; }

            set { SetProperty(ref AreaPerWellValue, value); }

        }
        private double MaxInjectionRatePerWellValue;

        public double MaxInjectionRatePerWell

        {

            get { return this.MaxInjectionRatePerWellValue; }

            set { SetProperty(ref MaxInjectionRatePerWellValue, value); }

        }
        private double TotalInjectionCapacityValue;

        public double TotalInjectionCapacity

        {

            get { return this.TotalInjectionCapacityValue; }

            set { SetProperty(ref TotalInjectionCapacityValue, value); }

        }
        private double EstimatedArealSweepValue;

        public double EstimatedArealSweep

        {

            get { return this.EstimatedArealSweepValue; }

            set { SetProperty(ref EstimatedArealSweepValue, value); }

        }
        private string SuggestedPlacementPatternValue;

        public string SuggestedPlacementPattern

        {

            get { return this.SuggestedPlacementPatternValue; }

            set { SetProperty(ref SuggestedPlacementPatternValue, value); }

        }
        private List<string> RiskFactorsValue = new();

        public List<string> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        }
    }

    /// <summary>
    /// DTO for pressure performance analysis in EOR operations
    /// </summary>
    public class PressurePerformanceAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private double InitialReservoirPressureValue;

        public double InitialReservoirPressure

        {

            get { return this.InitialReservoirPressureValue; }

            set { SetProperty(ref InitialReservoirPressureValue, value); }

        }
        private double CurrentReservoirPressureValue;

        public double CurrentReservoirPressure

        {

            get { return this.CurrentReservoirPressureValue; }

            set { SetProperty(ref CurrentReservoirPressureValue, value); }

        }
        private double InjectionRateValue;

        public double InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
        private int OperationMonthsValue;

        public int OperationMonths

        {

            get { return this.OperationMonthsValue; }

            set { SetProperty(ref OperationMonthsValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double PressureChangeValue;

        public double PressureChange

        {

            get { return this.PressureChangeValue; }

            set { SetProperty(ref PressureChangeValue, value); }

        }
        private double PressureChangePerMonthValue;

        public double PressureChangePerMonth

        {

            get { return this.PressureChangePerMonthValue; }

            set { SetProperty(ref PressureChangePerMonthValue, value); }

        }
        private double EffectiveCompressibilityValue;

        public double EffectiveCompressibility

        {

            get { return this.EffectiveCompressibilityValue; }

            set { SetProperty(ref EffectiveCompressibilityValue, value); }

        }
        private double PressureMaintenanceEfficiencyValue;

        public double PressureMaintenanceEfficiency

        {

            get { return this.PressureMaintenanceEfficiencyValue; }

            set { SetProperty(ref PressureMaintenanceEfficiencyValue, value); }

        }
        private double ProjectedPressure12MonthsValue;

        public double ProjectedPressure12Months

        {

            get { return this.ProjectedPressure12MonthsValue; }

            set { SetProperty(ref ProjectedPressure12MonthsValue, value); }

        }
        private double PressureGradientToBoundaryValue;

        public double PressureGradientToBoundary

        {

            get { return this.PressureGradientToBoundaryValue; }

            set { SetProperty(ref PressureGradientToBoundaryValue, value); }

        }
        private string OverPressureRiskValue;

        public string OverPressureRisk

        {

            get { return this.OverPressureRiskValue; }

            set { SetProperty(ref OverPressureRiskValue, value); }

        }
    }

    /// <summary>
    /// DTO for economic analysis of EOR projects
    /// </summary>
    public class EOREconomicAnalysis : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private double EstimatedIncrementalOilValue;

        public double EstimatedIncrementalOil

        {

            get { return this.EstimatedIncrementalOilValue; }

            set { SetProperty(ref EstimatedIncrementalOilValue, value); }

        }
        private double OilPriceValue;

        public double OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private double CapitalCostValue;

        public double CapitalCost

        {

            get { return this.CapitalCostValue; }

            set { SetProperty(ref CapitalCostValue, value); }

        }
        private double OperatingCostPerBarrelValue;

        public double OperatingCostPerBarrel

        {

            get { return this.OperatingCostPerBarrelValue; }

            set { SetProperty(ref OperatingCostPerBarrelValue, value); }

        }
        private int ProjectLifeYearsValue;

        public int ProjectLifeYears

        {

            get { return this.ProjectLifeYearsValue; }

            set { SetProperty(ref ProjectLifeYearsValue, value); }

        }
        private double DiscountRateValue;

        public double DiscountRate

        {

            get { return this.DiscountRateValue; }

            set { SetProperty(ref DiscountRateValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double GrossRevenueValue;

        public double GrossRevenue

        {

            get { return this.GrossRevenueValue; }

            set { SetProperty(ref GrossRevenueValue, value); }

        }
        private double TotalOperatingCostValue;

        public double TotalOperatingCost

        {

            get { return this.TotalOperatingCostValue; }

            set { SetProperty(ref TotalOperatingCostValue, value); }

        }
        private double NetPresentValueValue;

        public double NetPresentValue

        {

            get { return this.NetPresentValueValue; }

            set { SetProperty(ref NetPresentValueValue, value); }

        }
        private double InternalRateOfReturnValue;

        public double InternalRateOfReturn

        {

            get { return this.InternalRateOfReturnValue; }

            set { SetProperty(ref InternalRateOfReturnValue, value); }

        }
        private double PaybackPeriodYearsValue;

        public double PaybackPeriodYears

        {

            get { return this.PaybackPeriodYearsValue; }

            set { SetProperty(ref PaybackPeriodYearsValue, value); }

        }
        private double ProfitabilityIndexValue;

        public double ProfitabilityIndex

        {

            get { return this.ProfitabilityIndexValue; }

            set { SetProperty(ref ProfitabilityIndexValue, value); }

        }
        private double NpvAt20PercentOilPriceValue;

        public double NpvAt20PercentOilPrice

        {

            get { return this.NpvAt20PercentOilPriceValue; }

            set { SetProperty(ref NpvAt20PercentOilPriceValue, value); }

        }
        private double NpvAt50PercentOilPriceValue;

        public double NpvAt50PercentOilPrice

        {

            get { return this.NpvAt50PercentOilPriceValue; }

            set { SetProperty(ref NpvAt50PercentOilPriceValue, value); }

        }
    }
}




