using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PPDM39.Core.DTOs
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
        public decimal? GasLiquidRatio { get; set; }
        public decimal? Velocity { get; set; }
    }

    #endregion

    #region Calculation Service Interface

    /// <summary>
    /// Interface for calculation services
    /// </summary>
    public interface ICalculationService
    {
        /// <summary>
        /// Performs Decline Curve Analysis
        /// </summary>
        Task<DCAResult> PerformDCAAnalysisAsync(DCARequest request);

        /// <summary>
        /// Performs Economic Analysis
        /// </summary>
        Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request);

        /// <summary>
        /// Performs Nodal Analysis
        /// </summary>
        Task<NodalAnalysisResult> PerformNodalAnalysisAsync(NodalAnalysisRequest request);

        /// <summary>
        /// Gets calculation results by ID
        /// </summary>
        Task<object?> GetCalculationResultAsync(string calculationId, string calculationType);

        /// <summary>
        /// Gets all calculation results for a well, pool, or field
        /// </summary>
        Task<List<object>> GetCalculationResultsAsync(string? wellId = null, string? poolId = null, string? fieldId = null, string? calculationType = null);
    }

    #endregion
}
