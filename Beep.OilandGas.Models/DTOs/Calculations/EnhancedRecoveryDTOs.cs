using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Calculations
{
    /// <summary>
    /// Comprehensive set of DTOs for Enhanced Oil Recovery (EOR) analysis.
    /// Covers waterflooding, gas injection, chemical EOR, thermal recovery, and economic analysis.
    /// </summary>

    /// <summary>
    /// DTO for waterflooding performance analysis results
    /// </summary>
    public class WaterfloodPerformanceAnalysisDto
    {
        public string FieldId { get; set; }
        public int DataPointsAnalyzed { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double CumulativeProduction { get; set; }
        public double RecoveryFactor { get; set; }
        public double IncrementalRecoveryFactor { get; set; }
        public double PressureMaintenanceEfficiency { get; set; }
        public WaterCutTrendDto WaterCutTrend { get; set; }
        public double FloodFrontVelocity { get; set; }
        public double SweepEfficiency { get; set; }
        public double ProjectedRecovery20Years { get; set; }
    }

    /// <summary>
    /// DTO for water cut trend analysis
    /// </summary>
    public class WaterCutTrendDto
    {
        public double InitialWaterCut { get; set; }
        public double FinalWaterCut { get; set; }
        public double RateOfIncreasePerMonth { get; set; }
        public double TimeToHighWaterCut { get; set; }
    }

    /// <summary>
    /// DTO for gas injection EOR analysis
    /// </summary>
    public class GasInjectionAnalysisDto
    {
        public string FieldId { get; set; }
        public string GasType { get; set; }
        public double InjectionPressure { get; set; }
        public double MinimumMiscibilityPressure { get; set; }
        public DateTime AnalysisDate { get; set; }
        public bool IsMiscible { get; set; }
        public string DisplacementMechanism { get; set; }
        public double DisplacementEfficiency { get; set; }
        public double ResidualOilSaturation { get; set; }
        public double ProductionImprovement { get; set; }
        public double TertiaryRecoveryPotential { get; set; }
        public GasTypeCharacteristicsDto GasTypeCharacteristics { get; set; }
    }

    /// <summary>
    /// DTO for gas type specific characteristics
    /// </summary>
    public class GasTypeCharacteristicsDto
    {
        public string GasType { get; set; }
        public double Density { get; set; }
        public double CriticalTemperature { get; set; }
        public double CriticalPressure { get; set; }
        public string MiscibilityAdvantage { get; set; }
    }

    /// <summary>
    /// DTO for chemical EOR analysis
    /// </summary>
    public class ChemicalEORAnalysisDto
    {
        public string FieldId { get; set; }
        public string ChemicalType { get; set; }
        public double ReservoirTemperature { get; set; }
        public double Salinity { get; set; }
        public double CrudePaveViscosity { get; set; }
        public DateTime AnalysisDate { get; set; }
        public string Suitability { get; set; }
        public double InterfacialTensionReduction { get; set; }
        public double OilRecoveryIncrement { get; set; }
        public double CostPerBarrelRecovered { get; set; }
        public List<string> EnvironmentalConcerns { get; set; } = new();
        public ChemicalParametersDto ChemicalParameters { get; set; }
    }

    /// <summary>
    /// DTO for chemical-specific parameters
    /// </summary>
    public class ChemicalParametersDto
    {
        public string ChemicalType { get; set; }
        public double OptimalTemperature { get; set; }
        public double OptimalSalinity { get; set; }
        public double DegradationRate { get; set; }
        public double AdsorptionFactor { get; set; }
    }

    /// <summary>
    /// DTO for thermal recovery analysis
    /// </summary>
    public class ThermalRecoveryAnalysisDto
    {
        public string FieldId { get; set; }
        public string ThermalMethod { get; set; }
        public double ReservoirTemperature { get; set; }
        public double CrudePaveViscosity { get; set; }
        public double OilSaturation { get; set; }
        public DateTime AnalysisDate { get; set; }
        public string Suitability { get; set; }
        public double ViscosityReduction { get; set; }
        public double MobilityImprovement { get; set; }
        public double EnergyRequirement { get; set; }
        public double ProjectedRecoveryFactor { get; set; }
        public double OperatingCostPerBarrel { get; set; }
        public string EnvironmentalImpact { get; set; }
    }

    /// <summary>
    /// DTO for comparing multiple EOR methods
    /// </summary>
    public class EORMethodComparisonDto
    {
        public string FieldId { get; set; }
        public int MethodsCompared { get; set; }
        public DateTime AnalysisDate { get; set; }
        public Dictionary<string, EORMethodScoreDto> MethodScores { get; set; } = new();
        public List<RankedEORMethodDto> RankedMethods { get; set; } = new();
        public string RecommendedMethod { get; set; }
        public double SynergyPotential { get; set; }
    }

    /// <summary>
    /// DTO for EOR method scoring criteria
    /// </summary>
    public class EORMethodScoreDto
    {
        public string Method { get; set; }
        public double TemperatureSuitability { get; set; }
        public double ViscositySuitability { get; set; }
        public double SaturationSuitability { get; set; }
        public double OverallScore { get; set; }
    }

    /// <summary>
    /// DTO for ranked EOR methods
    /// </summary>
    public class RankedEORMethodDto
    {
        public int Rank { get; set; }
        public string MethodName { get; set; }
        public EORMethodScoreDto Score { get; set; }
    }

    /// <summary>
    /// DTO for injection well placement optimization
    /// </summary>
    public class InjectionWellOptimizationDto
    {
        public string FieldId { get; set; }
        public int DesiredWellCount { get; set; }
        public double ReservoirArea { get; set; }
        public double ReservoirThickness { get; set; }
        public double Permeability { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double OptimalWellSpacing { get; set; }
        public double AreaPerWell { get; set; }
        public double MaxInjectionRatePerWell { get; set; }
        public double TotalInjectionCapacity { get; set; }
        public double EstimatedArealSweep { get; set; }
        public string SuggestedPlacementPattern { get; set; }
        public List<string> RiskFactors { get; set; } = new();
    }

    /// <summary>
    /// DTO for pressure performance analysis in EOR operations
    /// </summary>
    public class PressurePerformanceAnalysisDto
    {
        public string FieldId { get; set; }
        public double InitialReservoirPressure { get; set; }
        public double CurrentReservoirPressure { get; set; }
        public double InjectionRate { get; set; }
        public int OperationMonths { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double PressureChange { get; set; }
        public double PressureChangePerMonth { get; set; }
        public double EffectiveCompressibility { get; set; }
        public double PressureMaintenanceEfficiency { get; set; }
        public double ProjectedPressure12Months { get; set; }
        public double PressureGradientToBoundary { get; set; }
        public string OverPressureRisk { get; set; }
    }

    /// <summary>
    /// DTO for economic analysis of EOR projects
    /// </summary>
    public class EOREconomicAnalysisDto
    {
        public string FieldId { get; set; }
        public double EstimatedIncrementalOil { get; set; }
        public double OilPrice { get; set; }
        public double CapitalCost { get; set; }
        public double OperatingCostPerBarrel { get; set; }
        public int ProjectLifeYears { get; set; }
        public double DiscountRate { get; set; }
        public DateTime AnalysisDate { get; set; }
        public double GrossRevenue { get; set; }
        public double TotalOperatingCost { get; set; }
        public double NetPresentValue { get; set; }
        public double InternalRateOfReturn { get; set; }
        public double PaybackPeriodYears { get; set; }
        public double ProfitabilityIndex { get; set; }
        public double NpvAt20PercentOilPrice { get; set; }
        public double NpvAt50PercentOilPrice { get; set; }
    }
}
