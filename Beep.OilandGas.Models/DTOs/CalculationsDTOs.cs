using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs
{
    #region Decline Curve Analysis (DCA) DTOs

    /// <summary>
    /// Request for Decline Curve Analysis calculation
    /// </summary>
    public class DCARequest
    {
        public string? WellId { get; set; }
        public string? PoolId { get; set; }
        public string? FieldId { get; set; }
        public string CalculationType { get; set; } = "DCA"; // DCA, DCA_EXPONENTIAL, DCA_HYPERBOLIC, DCA_HARMONIC
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ProductionFluidType { get; set; } // OIL, GAS, WATER
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Decline Curve Analysis calculation
    /// </summary>
    public class DCAResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? PoolId { get; set; }
        public string? FieldId { get; set; }
        public string CalculationType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        public string? ProductionFluidType { get; set; }
        
        // Decline curve parameters
        public decimal? InitialRate { get; set; }
        public decimal? DeclineRate { get; set; }
        public decimal? DeclineConstant { get; set; }
        public decimal? NominalDeclineRate { get; set; }
        public decimal? EffectiveDeclineRate { get; set; }
        public decimal? HyperbolicExponent { get; set; }
        
        // Forecasted production
        public List<DCAForecastPoint> ForecastPoints { get; set; } = new List<DCAForecastPoint>();
        
        // Statistical metrics
        public decimal? RMSE { get; set; } // Root Mean Square Error
        public decimal? R2 { get; set; } // Coefficient of determination
        public decimal? CorrelationCoefficient { get; set; }
        
        // Estimated reserves
        public decimal? EstimatedEUR { get; set; } // Estimated Ultimate Recovery
        public decimal? RemainingReserves { get; set; }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED, PARTIAL
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Forecast point for a specific date
    /// </summary>
    public class DCAForecastPoint
    {
        public DateTime Date { get; set; }
        public decimal? ProductionRate { get; set; }
        public decimal? CumulativeProduction { get; set; }
        public decimal? DeclineRate { get; set; }
    }

    #endregion

    #region Economic Analysis DTOs

    /// <summary>
    /// Request for Economic Analysis calculation
    /// </summary>
    public class EconomicAnalysisRequest
    {
        public string? WellId { get; set; }
        public string? PoolId { get; set; }
        public string? FieldId { get; set; }
        public string? ProjectId { get; set; }
        public string AnalysisType { get; set; } = "NPV"; // NPV, IRR, PAYBACK, ROI, BREAKEVEN
        
        // Economic parameters
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
        public decimal? DiscountRate { get; set; }
        public decimal? InflationRate { get; set; }
        public decimal? OperatingCostPerUnit { get; set; }
        public decimal? CapitalInvestment { get; set; }
        
        // Production forecast
        public List<EconomicProductionPoint>? ProductionForecast { get; set; }
        
        // Time parameters
        public DateTime? AnalysisStartDate { get; set; }
        public DateTime? AnalysisEndDate { get; set; }
        public int? AnalysisPeriodYears { get; set; }
        
        // Fiscal terms
        public decimal? RoyaltyRate { get; set; }
        public decimal? TaxRate { get; set; }
        public decimal? WorkingInterest { get; set; }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Production point for economic analysis
    /// </summary>
    public class EconomicProductionPoint
    {
        public DateTime Date { get; set; }
        public decimal? OilVolume { get; set; }
        public decimal? GasVolume { get; set; }
        public decimal? WaterVolume { get; set; }
        public decimal? OperatingCost { get; set; }
        public decimal? Revenue { get; set; }
    }

    /// <summary>
    /// Result of Economic Analysis calculation
    /// </summary>
    public class EconomicAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? PoolId { get; set; }
        public string? FieldId { get; set; }
        public string? ProjectId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Key economic metrics
        public decimal? NetPresentValue { get; set; } // NPV
        public decimal? InternalRateOfReturn { get; set; } // IRR (as percentage)
        public decimal? PaybackPeriod { get; set; } // In years
        public decimal? ReturnOnInvestment { get; set; } // ROI (as percentage)
        public decimal? BreakevenPrice { get; set; }
        public decimal? BreakevenVolume { get; set; }
        public decimal? ProfitabilityIndex { get; set; }
        
        // Cash flow analysis
        public List<EconomicCashFlowPoint> CashFlowPoints { get; set; } = new List<EconomicCashFlowPoint>();
        public decimal? TotalRevenue { get; set; }
        public decimal? TotalOperatingCosts { get; set; }
        public decimal? TotalCapitalCosts { get; set; }
        public decimal? TotalTaxes { get; set; }
        public decimal? TotalRoyalties { get; set; }
        public decimal? NetCashFlow { get; set; }
        
        // Sensitivity analysis (optional)
        public List<EconomicSensitivityPoint>? SensitivityAnalysis { get; set; }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED, PARTIAL
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Cash flow point for a specific period
    /// </summary>
    public class EconomicCashFlowPoint
    {
        public DateTime Date { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? OperatingCosts { get; set; }
        public decimal? CapitalCosts { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Royalties { get; set; }
        public decimal? NetCashFlow { get; set; }
        public decimal? CumulativeCashFlow { get; set; }
        public decimal? DiscountedCashFlow { get; set; }
        public decimal? CumulativeDiscountedCashFlow { get; set; }
    }

    /// <summary>
    /// Sensitivity analysis point
    /// </summary>
    public class EconomicSensitivityPoint
    {
        public string VariableName { get; set; } = string.Empty;
        public decimal VariableValue { get; set; }
        public decimal? NPV { get; set; }
        public decimal? IRR { get; set; }
    }

    #endregion

    #region Nodal Analysis DTOs

    /// <summary>
    /// Request for Nodal Analysis calculation
    /// </summary>
    public class NodalAnalysisRequest
    {
        public string? WellId { get; set; }
        public string? WellboreId { get; set; }
        public string? FieldId { get; set; }
        public string AnalysisType { get; set; } = "PRODUCTION"; // PRODUCTION, INJECTION
        
        // Well configuration
        public decimal? WellheadPressure { get; set; }
        public decimal? BottomHolePressure { get; set; }
        public decimal? TubingDiameter { get; set; }
        public decimal? CasingDiameter { get; set; }
        public decimal? WellDepth { get; set; }
        public decimal? PerforationDepth { get; set; }
        
        // Fluid properties
        public decimal? OilGravity { get; set; } // API gravity
        public decimal? GasGravity { get; set; }
        public decimal? WaterCut { get; set; } // Percentage
        public decimal? GasOilRatio { get; set; } // GOR
        public decimal? WaterOilRatio { get; set; } // WOR
        public decimal? Temperature { get; set; }
        
        // Inflow Performance Relationship (IPR) parameters
        public decimal? ProductivityIndex { get; set; }
        public decimal? ReservoirPressure { get; set; }
        public string? IPRModel { get; set; } // VOGEL, FETKOVICH, etc.
        public Dictionary<string, object>? IPRParameters { get; set; }
        
        // Vertical Lift Performance (VLP) parameters
        public string? VLPModel { get; set; } // HAGEDORN_BROWN, DUNS_ROS, etc.
        public Dictionary<string, object>? VLPParameters { get; set; }
        
        // Operating conditions
        public decimal? ChokeSize { get; set; }
        public decimal? FlowRateRangeMin { get; set; }
        public decimal? FlowRateRangeMax { get; set; }
        public int? NumberOfPoints { get; set; }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Nodal Analysis calculation
    /// </summary>
    public class NodalAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? WellboreId { get; set; }
        public string? FieldId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Operating point (intersection of IPR and VLP curves)
        public decimal? OperatingFlowRate { get; set; }
        public decimal? OperatingPressure { get; set; }
        public decimal? OperatingTemperature { get; set; }
        
        // IPR curve points
        public List<NodalCurvePoint> IPRCurve { get; set; } = new List<NodalCurvePoint>();
        
        // VLP curve points
        public List<NodalCurvePoint> VLPCurve { get; set; } = new List<NodalCurvePoint>();
        
        // Performance metrics
        public decimal? MaximumFlowRate { get; set; }
        public decimal? MinimumFlowRate { get; set; }
        public decimal? OptimalFlowRate { get; set; }
        public decimal? PressureDrop { get; set; }
        
        // System efficiency
        public decimal? SystemEfficiency { get; set; } // Percentage
        public decimal? LiftingEfficiency { get; set; } // Percentage
        
        // Recommendations
        public List<string> Recommendations { get; set; } = new List<string>();
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED, PARTIAL
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Point on a nodal analysis curve
    /// </summary>
    public class NodalCurvePoint
    {
        public decimal FlowRate { get; set; }
        public decimal Pressure { get; set; }
        public decimal? Temperature { get; set; }
    }

    #endregion

    #region Well Test Analysis DTOs

    /// <summary>
    /// Request for Well Test Analysis calculation
    /// </summary>
    public class WellTestAnalysisCalculationRequest
    {
        public string? WellId { get; set; }
        public string? TestId { get; set; }
        public string AnalysisType { get; set; } = "BUILDUP"; // BUILDUP, DRAWDOWN, MULTI_RATE
        public string? AnalysisMethod { get; set; } // HORNER, MDH, AGARWAL
        
        // Well test data (if provided directly)
        public List<WellTestDataPoint>? PressureTimeData { get; set; }
        
        // Well properties (if not in PPDM)
        public decimal? FlowRate { get; set; } // BPD or Mscf/day
        public decimal? WellboreRadius { get; set; } // feet
        public decimal? FormationThickness { get; set; } // feet
        public decimal? Porosity { get; set; }
        public decimal? TotalCompressibility { get; set; } // psi^-1
        public decimal? OilViscosity { get; set; } // cp
        public decimal? OilFormationVolumeFactor { get; set; } // RB/STB
        public decimal? ProductionTime { get; set; } // hours (for build-up)
        public bool? IsGasWell { get; set; }
        public decimal? GasSpecificGravity { get; set; }
        public decimal? ReservoirTemperature { get; set; } // Fahrenheit
        public decimal? InitialReservoirPressure { get; set; } // psi
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Well test data point (pressure-time pair)
    /// </summary>
    public class WellTestDataPoint
    {
        public double Time { get; set; } // hours
        public double Pressure { get; set; } // psi
    }

    /// <summary>
    /// Result of Well Test Analysis calculation
    /// </summary>
    public class WellTestAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? TestId { get; set; }
        public string? FieldId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public string AnalysisMethod { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Analysis results
        public decimal Permeability { get; set; } // md
        public decimal SkinFactor { get; set; }
        public decimal ReservoirPressure { get; set; } // psi
        public decimal ProductivityIndex { get; set; } // BPD/psi or Mscf/day/psi
        public decimal FlowEfficiency { get; set; } // dimensionless
        public decimal DamageRatio { get; set; } // dimensionless
        public decimal RadiusOfInvestigation { get; set; } // feet
        
        // Reservoir model identification
        public string IdentifiedModel { get; set; } = "INFINITE_ACTING";
        
        // Quality metrics
        public decimal RSquared { get; set; }
        
        // Diagnostic data
        public List<WellTestDataPoint>? DiagnosticPoints { get; set; }
        public List<WellTestDataPoint>? DerivativePoints { get; set; }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED, PARTIAL
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    #endregion

    #region Flash Calculation DTOs

    /// <summary>
    /// Request for Flash Calculation
    /// </summary>
    public class FlashCalculationRequest
    {
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public string CalculationType { get; set; } = "ISOTHERMAL"; // ISOTHERMAL, ISOBARIC, etc.
        
        // Flash conditions (if provided directly)
        public decimal? Pressure { get; set; } // psia
        public decimal? Temperature { get; set; } // Rankine
        public List<ComponentData>? FeedComposition { get; set; }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Component data for flash calculations
    /// </summary>
    public class ComponentData
    {
        public string Name { get; set; } = string.Empty;
        public decimal MoleFraction { get; set; }
        public decimal? CriticalTemperature { get; set; } // Rankine
        public decimal? CriticalPressure { get; set; } // psia
        public decimal? AcentricFactor { get; set; }
        public decimal? MolecularWeight { get; set; }
    }

    /// <summary>
    /// Result of Flash Calculation
    /// </summary>
    public class FlashCalculationResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public string? FieldId { get; set; }
        public string CalculationType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Flash results
        public decimal VaporFraction { get; set; } // 0-1
        public decimal LiquidFraction { get; set; } // 0-1
        
        // Phase compositions (mole fractions)
        public Dictionary<string, decimal> VaporComposition { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, decimal> LiquidComposition { get; set; } = new Dictionary<string, decimal>();
        
        // K-values (equilibrium ratios)
        public Dictionary<string, decimal> KValues { get; set; } = new Dictionary<string, decimal>();
        
        // Phase properties
        public PhasePropertiesData? VaporProperties { get; set; }
        public PhasePropertiesData? LiquidProperties { get; set; }
        
        // Convergence information
        public int Iterations { get; set; }
        public bool Converged { get; set; }
        public decimal ConvergenceError { get; set; }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED, PARTIAL
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Phase properties data
    /// </summary>
    public class PhasePropertiesData
    {
        public decimal Density { get; set; } // lb/ft³
        public decimal MolecularWeight { get; set; }
        public decimal SpecificGravity { get; set; }
        public decimal? Volume { get; set; } // ft³
    }

    #endregion

    #region Choke Analysis DTOs

    /// <summary>
    /// Request for Choke Analysis calculation
    /// </summary>
    public class ChokeAnalysisRequest
    {
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; } // WELL_EQUIPMENT ROW_ID
        public string AnalysisType { get; set; } = "DOWNHOLE"; // DOWNHOLE, UPHOLE, SIZING, PRESSURE
        
        // Choke properties (optional, will be retrieved from WELL_EQUIPMENT if not provided)
        public decimal? ChokeDiameter { get; set; } // inches
        public string? ChokeType { get; set; } // BEAN, ADJUSTABLE, POSITIVE
        public decimal? DischargeCoefficient { get; set; }
        
        // Gas properties (optional, will be retrieved from WELL if not provided)
        public decimal? UpstreamPressure { get; set; } // psia
        public decimal? DownstreamPressure { get; set; } // psia
        public decimal? Temperature { get; set; } // Rankine
        public decimal? GasSpecificGravity { get; set; }
        public decimal? ZFactor { get; set; }
        public decimal? FlowRate { get; set; } // Mscf/day (for pressure calculation)
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Choke Analysis calculation
    /// </summary>
    public class ChokeAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Choke properties used
        public decimal ChokeDiameter { get; set; } // inches
        public string ChokeType { get; set; } = string.Empty;
        public decimal DischargeCoefficient { get; set; }
        
        // Flow results
        public decimal FlowRate { get; set; } // Mscf/day
        public decimal UpstreamPressure { get; set; } // psia
        public decimal DownstreamPressure { get; set; } // psia
        public decimal PressureRatio { get; set; }
        public string FlowRegime { get; set; } = string.Empty; // SONIC, SUBSONIC
        public decimal CriticalPressureRatio { get; set; }
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    #endregion

    #region Gas Lift Analysis DTOs

    /// <summary>
    /// Request for Gas Lift Analysis calculation
    /// </summary>
    public class GasLiftAnalysisRequest
    {
        public string? WellId { get; set; }
        public string AnalysisType { get; set; } = "POTENTIAL"; // POTENTIAL, VALVE_DESIGN, VALVE_SPACING
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        public decimal? WellDepth { get; set; } // feet
        public decimal? TubingDiameter { get; set; } // inches
        public decimal? CasingDiameter { get; set; } // inches
        public decimal? WellheadPressure { get; set; } // psia
        public decimal? BottomHolePressure { get; set; } // psia
        public decimal? WellheadTemperature { get; set; } // Rankine
        public decimal? BottomHoleTemperature { get; set; } // Rankine
        public decimal? OilGravity { get; set; } // API
        public decimal? WaterCut { get; set; } // fraction 0-1
        public decimal? GasOilRatio { get; set; } // scf/bbl
        public decimal? GasSpecificGravity { get; set; }
        public decimal? DesiredProductionRate { get; set; } // bbl/day
        
        // Analysis parameters
        public decimal? MinGasInjectionRate { get; set; } // Mscf/day
        public decimal? MaxGasInjectionRate { get; set; } // Mscf/day
        public int? NumberOfPoints { get; set; } // for performance curve
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Gas Lift Analysis calculation
    /// </summary>
    public class GasLiftAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Optimal results
        public decimal OptimalGasInjectionRate { get; set; } // Mscf/day
        public decimal MaximumProductionRate { get; set; } // bbl/day
        public decimal OptimalGasLiquidRatio { get; set; }
        
        // Performance curve points
        public List<GasLiftPerformancePoint> PerformancePoints { get; set; } = new List<GasLiftPerformancePoint>();
        
        // Valve design results (if AnalysisType is VALVE_DESIGN)
        public List<GasLiftValveData>? Valves { get; set; }
        public decimal? TotalGasInjectionRate { get; set; }
        public decimal? ExpectedProductionRate { get; set; }
        public decimal? SystemEfficiency { get; set; }
        
        // Valve spacing results (if AnalysisType is VALVE_SPACING)
        public List<decimal>? ValveDepths { get; set; } // feet
        public List<decimal>? OpeningPressures { get; set; } // psia
        public int? NumberOfValves { get; set; }
        public decimal? TotalDepthCoverage { get; set; } // feet
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Gas lift performance point
    /// </summary>
    public class GasLiftPerformancePoint
    {
        public decimal GasInjectionRate { get; set; } // Mscf/day
        public decimal ProductionRate { get; set; } // bbl/day
        public decimal GasLiquidRatio { get; set; }
        public decimal BottomHolePressure { get; set; } // psia
    }

    /// <summary>
    /// Gas lift valve data
    /// </summary>
    public class GasLiftValveData
    {
        public decimal Depth { get; set; } // feet
        public decimal PortSize { get; set; } // inches
        public decimal OpeningPressure { get; set; } // psia
        public decimal ClosingPressure { get; set; } // psia
        public string ValveType { get; set; } = string.Empty;
        public decimal Temperature { get; set; } // Rankine
        public decimal GasInjectionRate { get; set; } // Mscf/day
    }

    #endregion

    #region Pump Performance Analysis DTOs

    /// <summary>
    /// Request for Pump Performance Analysis calculation
    /// </summary>
    public class PumpAnalysisRequest
    {
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public string? EquipmentId { get; set; } // WELL_EQUIPMENT or FACILITY_EQUIPMENT ROW_ID
        public string PumpType { get; set; } = "ESP"; // ESP, CENTRIFUGAL, POSITIVE_DISPLACEMENT, JET
        public string AnalysisType { get; set; } = "PERFORMANCE"; // PERFORMANCE, DESIGN, EFFICIENCY, NPSH, SYSTEM_CURVE
        
        // Pump properties (optional, will be retrieved from equipment if not provided)
        public decimal? FlowRate { get; set; } // GPM or bbl/day
        public decimal? Head { get; set; } // feet
        public decimal? Power { get; set; } // horsepower
        public decimal? Efficiency { get; set; } // fraction 0-1
        public decimal? Speed { get; set; } // RPM
        public decimal? ImpellerDiameter { get; set; } // inches
        public int? NumberOfStages { get; set; } // for ESP
        
        // System properties
        public decimal? SuctionPressure { get; set; } // psia
        public decimal? DischargePressure { get; set; } // psia
        public decimal? FluidDensity { get; set; } // lb/ft³
        public decimal? FluidViscosity { get; set; } // cP
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Pump Performance Analysis calculation
    /// </summary>
    public class PumpAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? FacilityId { get; set; }
        public string? EquipmentId { get; set; }
        public string PumpType { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Performance results
        public decimal? FlowRate { get; set; } // GPM or bbl/day
        public decimal? Head { get; set; } // feet
        public decimal? Power { get; set; } // horsepower
        public decimal? Efficiency { get; set; } // fraction 0-1
        public decimal? BestEfficiencyPoint { get; set; } // flow rate at BEP
        
        // Performance curve points
        public List<PumpPerformancePoint>? PerformanceCurve { get; set; }
        
        // System analysis
        public decimal? OperatingPointFlowRate { get; set; }
        public decimal? OperatingPointHead { get; set; }
        public decimal? NPSHAvailable { get; set; } // feet
        public decimal? NPSHRequired { get; set; } // feet
        public bool? CavitationRisk { get; set; }
        
        // Design results (for ESP)
        public int? RecommendedStages { get; set; }
        public decimal? RecommendedMotorSize { get; set; } // horsepower
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Pump performance point
    /// </summary>
    public class PumpPerformancePoint
    {
        public decimal FlowRate { get; set; } // GPM or bbl/day
        public decimal Head { get; set; } // feet
        public decimal Power { get; set; } // horsepower
        public decimal Efficiency { get; set; } // fraction 0-1
    }

    #endregion

    #region Sucker Rod Pumping Analysis DTOs

    /// <summary>
    /// Request for Sucker Rod Pumping Analysis calculation
    /// </summary>
    public class SuckerRodAnalysisRequest
    {
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; } // WELL_EQUIPMENT ROW_ID
        public string AnalysisType { get; set; } = "LOAD"; // LOAD, POWER, PUMP_CARD, OPTIMIZATION
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        public decimal? WellDepth { get; set; } // feet
        public decimal? TubingDiameter { get; set; } // inches
        public decimal? RodStringLength { get; set; } // feet
        public decimal? RodStringWeight { get; set; } // lb
        public decimal? PumpDepth { get; set; } // feet
        public decimal? PumpDiameter { get; set; } // inches
        public decimal? StrokeLength { get; set; } // inches
        public decimal? StrokeRate { get; set; } // strokes/minute
        
        // Fluid properties
        public decimal? FluidLevel { get; set; } // feet
        public decimal? FluidDensity { get; set; } // lb/ft³
        public decimal? OilGravity { get; set; } // API
        public decimal? WaterCut { get; set; } // fraction 0-1
        
        // Production parameters
        public decimal? ProductionRate { get; set; } // bbl/day
        public decimal? VolumetricEfficiency { get; set; } // fraction 0-1
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Sucker Rod Pumping Analysis calculation
    /// </summary>
    public class SuckerRodAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Load analysis
        public decimal PeakLoad { get; set; } // lb
        public decimal MinimumLoad { get; set; } // lb
        public decimal RodStringWeight { get; set; } // lb
        public decimal FluidLoad { get; set; } // lb
        public decimal DynamicLoad { get; set; } // lb
        public decimal MaximumStress { get; set; } // psi
        public decimal SafetyFactor { get; set; }
        
        // Power analysis
        public decimal PolishedRodHorsepower { get; set; } // HP
        public decimal HydraulicHorsepower { get; set; } // HP
        public decimal FrictionHorsepower { get; set; } // HP
        public decimal TotalPowerRequired { get; set; } // HP
        
        // Production analysis
        public decimal ProductionRate { get; set; } // bbl/day
        public decimal PumpDisplacement { get; set; } // bbl/day
        public decimal VolumetricEfficiency { get; set; } // fraction 0-1
        
        // Pump card data (load vs position)
        public List<PumpCardPoint>? PumpCard { get; set; }
        
        // Optimization results
        public decimal? RecommendedStrokeLength { get; set; } // inches
        public decimal? RecommendedStrokeRate { get; set; } // strokes/minute
        public decimal? RecommendedPumpSize { get; set; } // inches
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Pump card point (load vs position)
    /// </summary>
    public class PumpCardPoint
    {
        public decimal Position { get; set; } // inches (0 to stroke length)
        public decimal Load { get; set; } // lb
    }

    #endregion

    #region Compressor Analysis DTOs

    /// <summary>
    /// Request for Compressor Analysis calculation
    /// </summary>
    public class CompressorAnalysisRequest
    {
        public string? FacilityId { get; set; }
        public string? EquipmentId { get; set; } // FACILITY_EQUIPMENT ROW_ID
        public string CompressorType { get; set; } = "CENTRIFUGAL"; // CENTRIFUGAL, RECIPROCATING
        public string AnalysisType { get; set; } = "POWER"; // POWER, PRESSURE, EFFICIENCY
        
        // Compressor properties (optional, will be retrieved from equipment if not provided)
        public decimal? SuctionPressure { get; set; } // psia
        public decimal? DischargePressure { get; set; } // psia
        public decimal? SuctionTemperature { get; set; } // Rankine
        public decimal? FlowRate { get; set; } // Mscf/day or ACFM
        public decimal? GasSpecificGravity { get; set; }
        public decimal? CompressionRatio { get; set; }
        
        // Centrifugal compressor specific
        public decimal? PolytropicEfficiency { get; set; } // fraction 0-1
        public decimal? AdiabaticEfficiency { get; set; } // fraction 0-1
        public int? NumberOfStages { get; set; }
        
        // Reciprocating compressor specific
        public decimal? CylinderDisplacement { get; set; } // ACFM
        public decimal? VolumetricEfficiency { get; set; } // fraction 0-1
        public int? NumberOfCylinders { get; set; }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Compressor Analysis calculation
    /// </summary>
    public class CompressorAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? FacilityId { get; set; }
        public string? EquipmentId { get; set; }
        public string CompressorType { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Power results
        public decimal PolytropicHead { get; set; } // feet
        public decimal AdiabaticHead { get; set; } // feet
        public decimal PowerRequired { get; set; } // horsepower
        public decimal DischargeTemperature { get; set; } // Rankine
        
        // Efficiency results
        public decimal PolytropicEfficiency { get; set; } // fraction 0-1
        public decimal AdiabaticEfficiency { get; set; } // fraction 0-1
        public decimal OverallEfficiency { get; set; } // fraction 0-1
        
        // Pressure and flow results
        public decimal SuctionPressure { get; set; } // psia
        public decimal DischargePressure { get; set; } // psia
        public decimal CompressionRatio { get; set; }
        public decimal FlowRate { get; set; } // Mscf/day or ACFM
        
        // Reciprocating compressor specific
        public decimal? CylinderDisplacement { get; set; } // ACFM
        public decimal? VolumetricEfficiency { get; set; } // fraction 0-1
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    #endregion

    #region Pipeline Analysis DTOs

    /// <summary>
    /// Request for Pipeline Analysis calculation
    /// </summary>
    public class PipelineAnalysisRequest
    {
        public string? PipelineId { get; set; }
        public string PipelineType { get; set; } = "GAS"; // GAS, LIQUID
        public string AnalysisType { get; set; } = "CAPACITY"; // CAPACITY, FLOW_RATE, PRESSURE_DROP
        
        // Pipeline properties (optional, will be retrieved from PIPELINE if not provided)
        public decimal? Length { get; set; } // miles or feet
        public decimal? Diameter { get; set; } // inches
        public decimal? Roughness { get; set; } // inches (absolute roughness)
        public decimal? ElevationChange { get; set; } // feet
        
        // Flow conditions
        public decimal? InletPressure { get; set; } // psia
        public decimal? OutletPressure { get; set; } // psia
        public decimal? FlowRate { get; set; } // Mscf/day (gas) or bbl/day (liquid)
        public decimal? Temperature { get; set; } // Rankine
        
        // Product properties
        public decimal? GasSpecificGravity { get; set; } // for gas pipelines
        public decimal? LiquidDensity { get; set; } // lb/ft³ (for liquid pipelines)
        public decimal? LiquidViscosity { get; set; } // cP (for liquid pipelines)
        public decimal? ZFactor { get; set; } // for gas pipelines
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Pipeline Analysis calculation
    /// </summary>
    public class PipelineAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? PipelineId { get; set; }
        public string PipelineType { get; set; } = string.Empty;
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Flow results
        public decimal FlowRate { get; set; } // Mscf/day (gas) or bbl/day (liquid)
        public decimal InletPressure { get; set; } // psia
        public decimal OutletPressure { get; set; } // psia
        public decimal PressureDrop { get; set; } // psi
        public decimal AveragePressure { get; set; } // psia
        
        // Capacity results
        public decimal MaximumCapacity { get; set; } // Mscf/day (gas) or bbl/day (liquid)
        public decimal Utilization { get; set; } // fraction 0-1
        
        // Flow regime analysis
        public decimal ReynoldsNumber { get; set; }
        public decimal FrictionFactor { get; set; }
        public string FlowRegime { get; set; } = string.Empty; // LAMINAR, TURBULENT, TRANSITION
        
        // Pipeline properties used
        public decimal Length { get; set; } // miles or feet
        public decimal Diameter { get; set; } // inches
        public decimal Roughness { get; set; } // inches
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    #endregion

    #region Plunger Lift Analysis DTOs

    /// <summary>
    /// Request for Plunger Lift Analysis calculation
    /// </summary>
    public class PlungerLiftAnalysisRequest
    {
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; } // WELL_EQUIPMENT ROW_ID
        public string AnalysisType { get; set; } = "PERFORMANCE"; // PERFORMANCE, OPTIMIZATION, CYCLE_TIME
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        public decimal? WellDepth { get; set; } // feet
        public decimal? TubingDiameter { get; set; } // inches
        public decimal? CasingDiameter { get; set; } // inches
        public decimal? WellheadPressure { get; set; } // psia
        public decimal? BottomHolePressure { get; set; } // psia
        public decimal? WellheadTemperature { get; set; } // Rankine
        public decimal? BottomHoleTemperature { get; set; } // Rankine
        
        // Plunger properties
        public decimal? PlungerWeight { get; set; } // lb
        public decimal? PlungerLength { get; set; } // feet
        public decimal? PlungerDiameter { get; set; } // inches
        
        // Production parameters
        public decimal? GasFlowRate { get; set; } // Mscf/day
        public decimal? LiquidProductionRate { get; set; } // bbl/day
        public decimal? CycleTime { get; set; } // minutes
        
        // Fluid properties
        public decimal? GasSpecificGravity { get; set; }
        public decimal? OilGravity { get; set; } // API
        public decimal? WaterCut { get; set; } // fraction 0-1
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Plunger Lift Analysis calculation
    /// </summary>
    public class PlungerLiftAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Performance results
        public decimal ProductionRate { get; set; } // bbl/day
        public decimal CycleTime { get; set; } // minutes
        public decimal GasFlowRate { get; set; } // Mscf/day
        public decimal PlungerVelocity { get; set; } // ft/s
        
        // Optimization results
        public decimal? OptimalCycleTime { get; set; } // minutes
        public decimal? OptimalGasFlowRate { get; set; } // Mscf/day
        public decimal? MaximumProductionRate { get; set; } // bbl/day
        
        // Cycle analysis
        public decimal FallTime { get; set; } // minutes
        public decimal RiseTime { get; set; } // minutes
        public decimal ShutInTime { get; set; } // minutes
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    #endregion

    #region Hydraulic Pump Analysis DTOs

    /// <summary>
    /// Request for Hydraulic Pump Analysis calculation
    /// </summary>
    public class HydraulicPumpAnalysisRequest
    {
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; } // WELL_EQUIPMENT ROW_ID
        public string AnalysisType { get; set; } = "PERFORMANCE"; // PERFORMANCE, DESIGN, EFFICIENCY
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        public decimal? WellDepth { get; set; } // feet
        public decimal? PumpDepth { get; set; } // feet
        public decimal? TubingDiameter { get; set; } // inches
        
        // Pump properties
        public decimal? NozzleSize { get; set; } // inches
        public decimal? ThroatSize { get; set; } // inches
        public decimal? PowerFluidPressure { get; set; } // psia
        public decimal? PowerFluidRate { get; set; } // bbl/day
        public decimal? PowerFluidDensity { get; set; } // lb/ft³
        
        // Production parameters
        public decimal? ProductionRate { get; set; } // bbl/day
        public decimal? DischargePressure { get; set; } // psia
        public decimal? SuctionPressure { get; set; } // psia
        
        // Fluid properties
        public decimal? OilGravity { get; set; } // API
        public decimal? WaterCut { get; set; } // fraction 0-1
        public decimal? GasOilRatio { get; set; } // scf/bbl
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of Hydraulic Pump Analysis calculation
    /// </summary>
    public class HydraulicPumpAnalysisResult
    {
        public string CalculationId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? EquipmentId { get; set; }
        public string AnalysisType { get; set; } = string.Empty;
        public DateTime CalculationDate { get; set; }
        
        // Performance results
        public decimal ProductionRate { get; set; } // bbl/day
        public decimal PowerFluidRate { get; set; } // bbl/day
        public decimal PowerFluidPressure { get; set; } // psia
        public decimal DischargePressure { get; set; } // psia
        public decimal SuctionPressure { get; set; } // psia
        
        // Efficiency results
        public decimal HydraulicEfficiency { get; set; } // fraction 0-1
        public decimal OverallEfficiency { get; set; } // fraction 0-1
        public decimal PowerRequired { get; set; } // horsepower
        
        // Design results
        public decimal? RecommendedNozzleSize { get; set; } // inches
        public decimal? RecommendedThroatSize { get; set; } // inches
        public decimal? RecommendedPowerFluidRate { get; set; } // bbl/day
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    #endregion
}

