using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Field DTOs

    /// <summary>
    /// Request for creating or updating a field (maps to FIELD table)
    /// </summary>
    public class FieldRequest : ModelEntityBase
    {
        public string? FieldId { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Location information
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? LocationDescription { get; set; }
        
        // Dates
        public DateTime? DiscoveryDate { get; set; }
        public DateTime? FirstProductionDate { get; set; }
        
        // Classification
        public string? FieldType { get; set; }
        public string? Status { get; set; }
        
        // Area information
        public decimal? Area { get; set; }
        public string? AreaOuom { get; set; } // Unit of measure (e.g., "ACRE", "KM2")
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing field data (includes audit fields from FIELD table)
    /// </summary>
    public class FieldResponse : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? Description { get; set; }
        
        // Location information
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? LocationDescription { get; set; }
        
        // Dates
        public DateTime? DiscoveryDate { get; set; }
        public DateTime? FirstProductionDate { get; set; }
        
        // Classification
        public string? FieldType { get; set; }
        public string? Status { get; set; }
        
        // Area information
        public decimal? Area { get; set; }
        public string? AreaOuom { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
        public object Field { get; set; }
    }

    #endregion

    #region Production DTOs

    /// <summary>
    /// Request for creating or updating production data (maps to PDEN_VOL_SUMMARY table)
    /// </summary>
    public class ProductionRequest : ModelEntityBase
    {
        public string? ProductionId { get; set; }
        public string? WellId { get; set; }
        public string? PoolId { get; set; }
        public string? FieldId { get; set; }
        
        // Production date and period
        public DateTime? ProductionDate { get; set; }
        public DateTime? ProductionPeriodStart { get; set; }
        public DateTime? ProductionPeriodEnd { get; set; }
        public int? ProductionDays { get; set; }
        
        // Volumes
        public decimal? OilVolume { get; set; }
        public decimal? GasVolume { get; set; }
        public decimal? WaterVolume { get; set; }
        public decimal? CondensateVolume { get; set; }
        public string? VolumeOuom { get; set; } // e.g., "BBL", "MSCF", "BBL/D"
        
        // Rates
        public decimal? OilRate { get; set; }
        public decimal? GasRate { get; set; }
        public decimal? WaterRate { get; set; }
        public string? RateOuom { get; set; } // e.g., "BBL/D", "MSCF/D"
        
        // Pressures
        public decimal? FlowingPressure { get; set; }
        public decimal? StaticPressure { get; set; }
        public decimal? BottomHolePressure { get; set; }
        public string? PressureOuom { get; set; } // e.g., "PSI", "KPA"
        
        // Production classification
        public string? ProductionType { get; set; } // e.g., "NORMAL", "TEST", "ESTIMATED"
        public string? Status { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing production data (includes audit fields from PDEN_VOL_SUMMARY table)
    /// </summary>
    public class ProductionResponse : ModelEntityBase
    {
        public string ProductionId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? PoolId { get; set; }
        public string? FieldId { get; set; }
        
        // Production date and period
        public DateTime? ProductionDate { get; set; }
        public DateTime? ProductionPeriodStart { get; set; }
        public DateTime? ProductionPeriodEnd { get; set; }
        public int? ProductionDays { get; set; }
        
        // Volumes
        public decimal? OilVolume { get; set; }
        public decimal? GasVolume { get; set; }
        public decimal? WaterVolume { get; set; }
        public decimal? CondensateVolume { get; set; }
        public string? VolumeOuom { get; set; }
        
        // Rates
        public decimal? OilRate { get; set; }
        public decimal? GasRate { get; set; }
        public decimal? WaterRate { get; set; }
        public string? RateOuom { get; set; }
        
        // Pressures
        public decimal? FlowingPressure { get; set; }
        public decimal? StaticPressure { get; set; }
        public decimal? BottomHolePressure { get; set; }
        public string? PressureOuom { get; set; }
        
        // Production classification
        public string? ProductionType { get; set; }
        public string? Status { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Reserves DTOs

    /// <summary>
    /// Request for creating or updating reserves data (maps to RESENT table)
    /// </summary>
    public class ReservesRequest : ModelEntityBase
    {
        public string? ReservesId { get; set; }
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public string? WellId { get; set; }
        
        // Reserve classification
        public string? ReserveCategory { get; set; } // e.g., "PROVED", "PROBABLE", "POSSIBLE"
        public string? ReserveType { get; set; } // e.g., "DEVELOPED", "UNDEVELOPED"
        public string? ReserveClassification { get; set; } // e.g., "PROVED_PRODUCING", "PROVED_NON_PRODUCING"
        
        // Effective date
        public DateTime? EffectiveDate { get; set; }
        
        // Volumes
        public decimal? OilReserves { get; set; }
        public decimal? GasReserves { get; set; }
        public decimal? CondensateReserves { get; set; }
        public string? ReservesOuom { get; set; } // e.g., "BBL", "MSCF"
        
        // Recovery factors
        public decimal? OilRecoveryFactor { get; set; } // Percentage
        public decimal? GasRecoveryFactor { get; set; } // Percentage
        
        // Economic parameters
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
        public string? PriceCurrency { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing reserves data (includes audit fields from RESERVE_ENTITY table)
    /// </summary>
    public class ReservesResponse : ModelEntityBase
    {
        public string ReservesId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public string? WellId { get; set; }
        
        // Reserve classification
        public string? ReserveCategory { get; set; }
        public string? ReserveType { get; set; }
        public string? ReserveClassification { get; set; }
        
        // Effective date
        public DateTime? EffectiveDate { get; set; }
        
        // Volumes
        public decimal? OilReserves { get; set; }
        public decimal? GasReserves { get; set; }
        public decimal? CondensateReserves { get; set; }
        public string? ReservesOuom { get; set; }
        
        // Recovery factors
        public decimal? OilRecoveryFactor { get; set; }
        public decimal? GasRecoveryFactor { get; set; }
        
        // Economic parameters
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
        public string? PriceCurrency { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Production Forecast DTOs

    /// <summary>
    /// Request for creating or updating production forecast (maps to PRODUCTION_FORECAST table)
    /// </summary>
    public class ProductionForecastRequest : ModelEntityBase
    {
        public string? ForecastId { get; set; }
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public string? WellId { get; set; }
        
        // Forecast classification
        public string? ForecastType { get; set; } // e.g., "DCA", "RESERVOIR_SIMULATION", "ANALOG"
        public string? ForecastMethod { get; set; } // e.g., "EXPONENTIAL", "HYPERBOLIC", "HARMONIC"
        public string? ForecastName { get; set; }
        public string? Description { get; set; }
        
        // Forecast period
        public DateTime? ForecastStartDate { get; set; }
        public DateTime? ForecastEndDate { get; set; }
        public int? ForecastPeriodMonths { get; set; }
        
        // Forecast parameters
        public decimal? InitialRate { get; set; }
        public decimal? DeclineRate { get; set; }
        public decimal? DeclineConstant { get; set; }
        public decimal? HyperbolicExponent { get; set; }
        
        // Forecasted volumes
        public decimal? ForecastOilVolume { get; set; }
        public decimal? ForecastGasVolume { get; set; }
        public string? ForecastVolumeOuom { get; set; }
        
        // Confidence levels
        public decimal? P10Forecast { get; set; } // 10th percentile (optimistic)
        public decimal? P50Forecast { get; set; } // 50th percentile (most likely)
        public decimal? P90Forecast { get; set; } // 90th percentile (conservative)
        
        // Status
        public string? Status { get; set; } // e.g., "DRAFT", "APPROVED", "SUPERSEDED"
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing production forecast data (includes audit fields from PRODUCTION_FORECAST table)
    /// </summary>
    public class ProductionForecastResponse : ModelEntityBase
    {
        public string ForecastId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public string? WellId { get; set; }
        
        // Forecast classification
        public string? ForecastType { get; set; }
        public string? ForecastMethod { get; set; }
        public string? ForecastName { get; set; }
        public string? Description { get; set; }
        
        // Forecast period
        public DateTime? ForecastStartDate { get; set; }
        public DateTime? ForecastEndDate { get; set; }
        public int? ForecastPeriodMonths { get; set; }
        
        // Forecast parameters
        public decimal? InitialRate { get; set; }
        public decimal? DeclineRate { get; set; }
        public decimal? DeclineConstant { get; set; }
        public decimal? HyperbolicExponent { get; set; }
        
        // Forecasted volumes
        public decimal? ForecastOilVolume { get; set; }
        public decimal? ForecastGasVolume { get; set; }
        public string? ForecastVolumeOuom { get; set; }
        
        // Confidence levels
        public decimal? P10Forecast { get; set; }
        public decimal? P50Forecast { get; set; }
        public decimal? P90Forecast { get; set; }
        
        // Status
        public string? Status { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Well Test DTOs

    /// <summary>
    /// Request for creating or updating well test data (maps to WELL_TEST table)
    /// </summary>
    public class WellTestRequest : ModelEntityBase
    {
        public string? TestId { get; set; }
        public string? WellId { get; set; }
        public string? WellboreId { get; set; }
        
        // Test classification
        public string? TestType { get; set; } // e.g., "FLOW_TEST", "BUILDUP_TEST", "INTERFERENCE_TEST"
        public string? TestPurpose { get; set; } // e.g., "PRODUCTION_TEST", "DRILLSTEM_TEST"
        public string? TestName { get; set; }
        public string? Description { get; set; }
        
        // Test dates
        public DateTime? TestStartDate { get; set; }
        public DateTime? TestEndDate { get; set; }
        public DateTime? TestDate { get; set; }
        
        // Test conditions
        public decimal? TestDuration { get; set; } // Hours
        public string? TestStatus { get; set; } // e.g., "COMPLETED", "ABANDONED", "IN_PROGRESS"
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing well test data (includes audit fields from WELL_TEST table)
    /// </summary>
    public class WellTestResponse : ModelEntityBase
    {
        public string TestId { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? WellboreId { get; set; }
        
        // Test classification
        public string? TestType { get; set; }
        public string? TestPurpose { get; set; }
        public string? TestName { get; set; }
        public string? Description { get; set; }
        
        // Test dates
        public DateTime? TestStartDate { get; set; }
        public DateTime? TestEndDate { get; set; }
        public DateTime? TestDate { get; set; }
        
        // Test conditions
        public decimal? TestDuration { get; set; }
        public string? TestStatus { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Well Test Analysis DTOs

    /// <summary>
    /// Request for creating or updating well test analysis data (maps to WELL_TEST_ANALYSIS table)
    /// </summary>
    public class WellTestAnalysisRequest : ModelEntityBase
    {
        public string? AnalysisId { get; set; }
        public string? TestId { get; set; }
        
        // Analysis parameters
        public decimal? Permeability { get; set; }
        public string? PermeabilityOuom { get; set; } // e.g., "MD", "DARCY"
        public decimal? Skin { get; set; }
        public decimal? ProductivityIndex { get; set; }
        public string? ProductivityIndexOuom { get; set; }
        
        // Flow potential
        public decimal? AofPotential { get; set; } // Absolute Open Flow
        public string? AofPotentialOuom { get; set; }
        
        // Wellbore storage
        public decimal? WellboreStorageCoeff { get; set; }
        public string? WellboreStorageOuom { get; set; }
        
        // Flow efficiency
        public decimal? FlowEfficiency { get; set; } // Percentage
        
        // Analysis method
        public string? AnalysisMethod { get; set; } // e.g., "HORNER", "MDH", "AGARWAL"
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing well test analysis data (includes audit fields from WELL_TEST_ANALYSIS table)
    /// </summary>
    public class WellTestAnalysisResponse : ModelEntityBase
    {
        public string AnalysisId { get; set; } = string.Empty;
        public string? TestId { get; set; }
        
        // Analysis parameters
        public decimal? Permeability { get; set; }
        public string? PermeabilityOuom { get; set; }
        public decimal? Skin { get; set; }
        public decimal? ProductivityIndex { get; set; }
        public string? ProductivityIndexOuom { get; set; }
        
        // Flow potential
        public decimal? AofPotential { get; set; }
        public string? AofPotentialOuom { get; set; }
        
        // Wellbore storage
        public decimal? WellboreStorageCoeff { get; set; }
        public string? WellboreStorageOuom { get; set; }
        
        // Flow efficiency
        public decimal? FlowEfficiency { get; set; }
        
        // Analysis method
        public string? AnalysisMethod { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Well Test Flow DTOs

    /// <summary>
    /// Request for creating or updating well test flow data (maps to WELL_TEST_FLOW table)
    /// </summary>
    public class WellTestFlowRequest : ModelEntityBase
    {
        public string? FlowId { get; set; }
        public string? TestId { get; set; }
        
        // Flow period
        public DateTime? FlowDate { get; set; }
        public decimal? FlowDuration { get; set; } // Hours
        
        // Flow rates
        public decimal? FlowRateOil { get; set; }
        public decimal? FlowRateGas { get; set; }
        public decimal? FlowRateWater { get; set; }
        public string? FlowRateOuom { get; set; } // e.g., "BBL/D", "MSCF/D"
        
        // Choke information
        public decimal? ChokeSize { get; set; }
        public string? ChokeSizeOuom { get; set; } // e.g., "IN", "MM"
        public string? ChokeType { get; set; } // e.g., "BEAN", "ADJUSTABLE"
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing well test flow data (includes audit fields from WELL_TEST_FLOW table)
    /// </summary>
    public class WellTestFlowResponse : ModelEntityBase
    {
        public string FlowId { get; set; } = string.Empty;
        public string? TestId { get; set; }
        
        // Flow period
        public DateTime? FlowDate { get; set; }
        public decimal? FlowDuration { get; set; }
        
        // Flow rates
        public decimal? FlowRateOil { get; set; }
        public decimal? FlowRateGas { get; set; }
        public decimal? FlowRateWater { get; set; }
        public string? FlowRateOuom { get; set; }
        
        // Choke information
        public decimal? ChokeSize { get; set; }
        public string? ChokeSizeOuom { get; set; }
        public string? ChokeType { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion

    #region Well Test Pressure DTOs

    /// <summary>
    /// Request for creating or updating well test pressure data (maps to WELL_TEST_PRESSURE table)
    /// </summary>
    public class WellTestPressureRequest : ModelEntityBase
    {
        public string? PressureId { get; set; }
        public string? TestId { get; set; }
        
        // Pressure measurement
        public DateTime? PressureDate { get; set; }
        public decimal? StaticPressure { get; set; }
        public decimal? FlowingPressure { get; set; }
        public decimal? BottomHolePressure { get; set; }
        public decimal? WellheadPressure { get; set; }
        public string? PressureOuom { get; set; } // e.g., "PSI", "KPA", "BAR"
        
        // Pressure type
        public string? PressureType { get; set; } // e.g., "INITIAL", "FINAL", "AVERAGE"
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing well test pressure data (includes audit fields from WELL_TEST_PRESSURE table)
    /// </summary>
    public class WellTestPressureResponse : ModelEntityBase
    {
        public string PressureId { get; set; } = string.Empty;
        public string? TestId { get; set; }
        
        // Pressure measurement
        public DateTime? PressureDate { get; set; }
        public decimal? StaticPressure { get; set; }
        public decimal? FlowingPressure { get; set; }
        public decimal? BottomHolePressure { get; set; }
        public decimal? WellheadPressure { get; set; }
        public string? PressureOuom { get; set; }
        
        // Pressure type
        public string? PressureType { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
        public string? PreferredInd { get; set; }
        
        // Audit fields
        public DateTime? CreateDate { get; set; }
        public string? CreateUser { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? UpdateUser { get; set; }
    }

    #endregion
}




