using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Pool DTOs

    /// <summary>
    /// Request for creating or updating a pool (maps to POOL table)
    /// </summary>
    public class PoolRequest : ModelEntityBase
    {
        public string? PoolId { get; set; }
        public string PoolName { get; set; } = string.Empty;
        public string? FieldId { get; set; } // Auto-set by service
        public string? Description { get; set; }
        
        // Pool classification
        public string? PoolType { get; set; } // e.g., "OIL", "GAS", "OIL_GAS", "WATER"
        public string? Status { get; set; } // e.g., "ACTIVE", "INACTIVE", "DEPLETED"
        public string? FormationName { get; set; }
        public string? StratUnitId { get; set; }
        
        // Reservoir properties
        public decimal? InitialReservoirPressure { get; set; }
        public string? InitialReservoirPressureOuom { get; set; } // e.g., "PSI", "KPA"
        public decimal? ReservoirTemperature { get; set; }
        public string? ReservoirTemperatureOuom { get; set; } // e.g., "F", "C"
        public decimal? AveragePorosity { get; set; } // Percentage or fraction
        public decimal? AveragePermeability { get; set; }
        public string? PermeabilityOuom { get; set; } // e.g., "MD", "DARCY"
        public decimal? AverageThickness { get; set; }
        public string? ThicknessOuom { get; set; } // e.g., "FT", "M"
        public decimal? NetPay { get; set; }
        public string? NetPayOuom { get; set; }
        
        // Fluid properties
        public decimal? OilGravity { get; set; } // API gravity
        public decimal? GasGravity { get; set; }
        public decimal? BubblePointPressure { get; set; }
        public string? BubblePointPressureOuom { get; set; }
        public decimal? OilViscosity { get; set; }
        public string? OilViscosityOuom { get; set; } // e.g., "CP", "MPA_S"
        public decimal? GasViscosity { get; set; }
        public string? GasViscosityOuom { get; set; }
        public decimal? FormationVolumeFactor { get; set; }
        public decimal? TotalCompressibility { get; set; }
        public string? CompressibilityOuom { get; set; }
        
        // Reserves estimates
        public decimal? OriginalOilInPlace { get; set; } // OOIP
        public decimal? OriginalGasInPlace { get; set; } // OGIP
        public string? ReservesOuom { get; set; } // e.g., "BBL", "MSCF"
        public decimal? RecoveryFactor { get; set; } // Percentage
        
        // Drainage information
        public decimal? DrainageArea { get; set; }
        public string? DrainageAreaOuom { get; set; } // e.g., "ACRE", "KM2"
        public decimal? DrainageRadius { get; set; }
        public string? DrainageRadiusOuom { get; set; } // e.g., "FT", "M"
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing pool data (includes audit fields from POOL table)
    /// </summary>
    public class PoolResponse : ModelEntityBase
    {
        public string PoolId { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? Description { get; set; }
        
        // Pool classification
        public string? PoolType { get; set; }
        public string? Status { get; set; }
        public string? FormationName { get; set; }
        public string? StratUnitId { get; set; }
        
        // Reservoir properties
        public decimal? InitialReservoirPressure { get; set; }
        public string? InitialReservoirPressureOuom { get; set; }
        public decimal? ReservoirTemperature { get; set; }
        public string? ReservoirTemperatureOuom { get; set; }
        public decimal? AveragePorosity { get; set; }
        public decimal? AveragePermeability { get; set; }
        public string? PermeabilityOuom { get; set; }
        public decimal? AverageThickness { get; set; }
        public string? ThicknessOuom { get; set; }
        public decimal? NetPay { get; set; }
        public string? NetPayOuom { get; set; }
        
        // Fluid properties
        public decimal? OilGravity { get; set; }
        public decimal? GasGravity { get; set; }
        public decimal? BubblePointPressure { get; set; }
        public string? BubblePointPressureOuom { get; set; }
        public decimal? OilViscosity { get; set; }
        public string? OilViscosityOuom { get; set; }
        public decimal? GasViscosity { get; set; }
        public string? GasViscosityOuom { get; set; }
        public decimal? FormationVolumeFactor { get; set; }
        public decimal? TotalCompressibility { get; set; }
        public string? CompressibilityOuom { get; set; }
        
        // Reserves estimates
        public decimal? OriginalOilInPlace { get; set; }
        public decimal? OriginalGasInPlace { get; set; }
        public string? ReservesOuom { get; set; }
        public decimal? RecoveryFactor { get; set; }
        
        // Drainage information
        public decimal? DrainageArea { get; set; }
        public string? DrainageAreaOuom { get; set; }
        public decimal? DrainageRadius { get; set; }
        public string? DrainageRadiusOuom { get; set; }
        
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

    #region Facility DTOs

    /// <summary>
    /// Request for creating or updating a facility (maps to FACILITY table)
    /// </summary>
    public class FacilityRequest : ModelEntityBase
    {
        public string? FacilityId { get; set; }
        public string FacilityName { get; set; } = string.Empty;
        public string? FieldId { get; set; } // Auto-set by service
        public string? Description { get; set; }
        
        // Facility classification
        public string? FacilityType { get; set; } // e.g., "PRODUCTION", "PROCESSING", "STORAGE", "TRANSPORTATION"
        public string? FacilityCategory { get; set; } // e.g., "PLATFORM", "FPSO", "PIPELINE_TERMINAL"
        public string? Status { get; set; } // e.g., "ACTIVE", "INACTIVE", "DECOMMISSIONED"
        
        // Dates
        public DateTime? ConstructionStartDate { get; set; }
        public DateTime? ConstructionEndDate { get; set; }
        public DateTime? CommissionDate { get; set; }
        public DateTime? DecommissionDate { get; set; }
        
        // Location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Elevation { get; set; }
        public string? ElevationOuom { get; set; }
        public string? LocationDescription { get; set; }
        
        // Capacity and specifications
        public decimal? ProcessingCapacity { get; set; } // Volume per day
        public string? ProcessingCapacityOuom { get; set; } // e.g., "BBL/D", "MSCF/D"
        public decimal? StorageCapacity { get; set; }
        public string? StorageCapacityOuom { get; set; }
        public string? DesignSpecifications { get; set; }
        
        // Cost information
        public decimal? ConstructionCost { get; set; }
        public string? ConstructionCostCurrency { get; set; }
        public decimal? OperatingCost { get; set; } // Per period
        public string? OperatingCostCurrency { get; set; }
        
        // Operator information
        public string? OperatorId { get; set; } // BUSINESS_ASSOCIATE_ID
        public string? OwnerId { get; set; } // BUSINESS_ASSOCIATE_ID
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing facility data (includes audit fields from FACILITY table)
    /// </summary>
    public class FacilityResponse : ModelEntityBase
    {
        public string FacilityId { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? Description { get; set; }
        
        // Facility classification
        public string? FacilityType { get; set; }
        public string? FacilityCategory { get; set; }
        public string? Status { get; set; }
        
        // Dates
        public DateTime? ConstructionStartDate { get; set; }
        public DateTime? ConstructionEndDate { get; set; }
        public DateTime? CommissionDate { get; set; }
        public DateTime? DecommissionDate { get; set; }
        
        // Location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Elevation { get; set; }
        public string? ElevationOuom { get; set; }
        public string? LocationDescription { get; set; }
        
        // Capacity and specifications
        public decimal? ProcessingCapacity { get; set; }
        public string? ProcessingCapacityOuom { get; set; }
        public decimal? StorageCapacity { get; set; }
        public string? StorageCapacityOuom { get; set; }
        public string? DesignSpecifications { get; set; }
        
        // Cost information
        public decimal? ConstructionCost { get; set; }
        public string? ConstructionCostCurrency { get; set; }
        public decimal? OperatingCost { get; set; }
        public string? OperatingCostCurrency { get; set; }
        
        // Operator information
        public string? OperatorId { get; set; }
        public string? OwnerId { get; set; }
        
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

    #region Pipeline DTOs

    /// <summary>
    /// Request for creating or updating a pipeline (maps to PIPELINE table)
    /// </summary>
    public class PipelineRequest : ModelEntityBase
    {
        public string? PipelineId { get; set; }
        public string PipelineName { get; set; } = string.Empty;
        public string? FieldId { get; set; } // Auto-set by service
        public string? Description { get; set; }
        
        // Pipeline classification
        public string? PipelineType { get; set; } // e.g., "GATHERING", "TRANSMISSION", "DISTRIBUTION", "EXPORT"
        public string? FluidType { get; set; } // e.g., "OIL", "GAS", "MULTIPHASE", "WATER"
        public string? Status { get; set; } // e.g., "ACTIVE", "INACTIVE", "ABANDONED"
        
        // Route information
        public string? OriginFacilityId { get; set; }
        public string? DestinationFacilityId { get; set; }
        public decimal? PipelineLength { get; set; }
        public string? PipelineLengthOuom { get; set; } // e.g., "KM", "MI"
        public decimal? Diameter { get; set; }
        public string? DiameterOuom { get; set; } // e.g., "IN", "MM"
        public decimal? WallThickness { get; set; }
        public string? WallThicknessOuom { get; set; }
        
        // Specifications
        public decimal? DesignPressure { get; set; }
        public string? DesignPressureOuom { get; set; } // e.g., "PSI", "BAR"
        public decimal? OperatingPressure { get; set; }
        public string? OperatingPressureOuom { get; set; }
        public decimal? FlowCapacity { get; set; }
        public string? FlowCapacityOuom { get; set; } // e.g., "BBL/D", "MSCF/D"
        public string? Material { get; set; } // e.g., "STEEL", "COMPOSITE"
        public string? CoatingType { get; set; }
        
        // Dates
        public DateTime? ConstructionStartDate { get; set; }
        public DateTime? ConstructionEndDate { get; set; }
        public DateTime? CommissionDate { get; set; }
        public DateTime? DecommissionDate { get; set; }
        
        // Cost information
        public decimal? ConstructionCost { get; set; }
        public string? ConstructionCostCurrency { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing pipeline data (includes audit fields from PIPELINE table)
    /// </summary>
    public class PipelineResponse : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public string PipelineName { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? Description { get; set; }
        
        // Pipeline classification
        public string? PipelineType { get; set; }
        public string? FluidType { get; set; }
        public string? Status { get; set; }
        
        // Route information
        public string? OriginFacilityId { get; set; }
        public string? DestinationFacilityId { get; set; }
        public decimal? PipelineLength { get; set; }
        public string? PipelineLengthOuom { get; set; }
        public decimal? Diameter { get; set; }
        public string? DiameterOuom { get; set; }
        public decimal? WallThickness { get; set; }
        public string? WallThicknessOuom { get; set; }
        
        // Specifications
        public decimal? DesignPressure { get; set; }
        public string? DesignPressureOuom { get; set; }
        public decimal? OperatingPressure { get; set; }
        public string? OperatingPressureOuom { get; set; }
        public decimal? FlowCapacity { get; set; }
        public string? FlowCapacityOuom { get; set; }
        public string? Material { get; set; }
        public string? CoatingType { get; set; }
        
        // Dates
        public DateTime? ConstructionStartDate { get; set; }
        public DateTime? ConstructionEndDate { get; set; }
        public DateTime? CommissionDate { get; set; }
        public DateTime? DecommissionDate { get; set; }
        
        // Cost information
        public decimal? ConstructionCost { get; set; }
        public string? ConstructionCostCurrency { get; set; }
        
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

    #region Development Well DTOs

    /// <summary>
    /// Request for creating or updating a development well (maps to WELL table with WELL_TYPE='DEVELOPMENT')
    /// </summary>
    public class DevelopmentWellRequest : ModelEntityBase
    {
        public string? WellId { get; set; }
        public string WellName { get; set; } = string.Empty;
        public string? FieldId { get; set; } // Auto-set by service
        public string? PoolId { get; set; }
        public string? Description { get; set; }
        
        // Well classification
        public string WellType { get; set; } = "DEVELOPMENT"; // Should be "DEVELOPMENT"
        public string? Status { get; set; } // e.g., "DRILLING", "COMPLETED", "PRODUCING", "SHUT_IN"
        public string? WellClassification { get; set; } // e.g., "PRODUCER", "INJECTOR", "OBSERVATION"
        public string? CompletionType { get; set; } // e.g., "OPEN_HOLE", "CASED_HOLE", "FRAC"
        
        // Dates
        public DateTime? SpudDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? FirstProductionDate { get; set; }
        public DateTime? RigReleaseDate { get; set; }
        
        // Location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GroundElevation { get; set; }
        public string? GroundElevationOuom { get; set; }
        public decimal? KBElevation { get; set; } // Kelly Bushing Elevation
        public string? KBElevationOuom { get; set; }
        public string? LocationDescription { get; set; }
        
        // Depth information
        public decimal? TotalDepth { get; set; }
        public string? TotalDepthOuom { get; set; } // e.g., "FT", "M"
        public decimal? MeasuredDepth { get; set; }
        public string? MeasuredDepthOuom { get; set; }
        public decimal? TrueVerticalDepth { get; set; }
        public string? TrueVerticalDepthOuom { get; set; }
        
        // Completion information
        public decimal? CompletionTopDepth { get; set; }
        public string? CompletionTopDepthOuom { get; set; }
        public decimal? CompletionBaseDepth { get; set; }
        public string? CompletionBaseDepthOuom { get; set; }
        public decimal? PerforationTopDepth { get; set; }
        public string? PerforationTopDepthOuom { get; set; }
        public decimal? PerforationBaseDepth { get; set; }
        public string? PerforationBaseDepthOuom { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing development well data (includes audit fields from WELL table)
    /// </summary>
    public class DevelopmentWellResponse : ModelEntityBase
    {
        public string WellId { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PoolId { get; set; }
        public string? Description { get; set; }
        
        // Well classification
        public string? WellType { get; set; }
        public string? Status { get; set; }
        public string? WellClassification { get; set; }
        public string? CompletionType { get; set; }
        
        // Dates
        public DateTime? SpudDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public DateTime? FirstProductionDate { get; set; }
        public DateTime? RigReleaseDate { get; set; }
        
        // Location
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? GroundElevation { get; set; }
        public string? GroundElevationOuom { get; set; }
        public decimal? KBElevation { get; set; }
        public string? KBElevationOuom { get; set; }
        public string? LocationDescription { get; set; }
        
        // Depth information
        public decimal? TotalDepth { get; set; }
        public string? TotalDepthOuom { get; set; }
        public decimal? MeasuredDepth { get; set; }
        public string? MeasuredDepthOuom { get; set; }
        public decimal? TrueVerticalDepth { get; set; }
        public string? TrueVerticalDepthOuom { get; set; }
        
        // Completion information
        public decimal? CompletionTopDepth { get; set; }
        public string? CompletionTopDepthOuom { get; set; }
        public decimal? CompletionBaseDepth { get; set; }
        public string? CompletionBaseDepthOuom { get; set; }
        public decimal? PerforationTopDepth { get; set; }
        public string? PerforationTopDepthOuom { get; set; }
        public decimal? PerforationBaseDepth { get; set; }
        public string? PerforationBaseDepthOuom { get; set; }
        
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

    #region Wellbore DTOs

    /// <summary>
    /// Request for creating or updating a wellbore (maps to WELLBORE table)
    /// </summary>
    public class WellboreRequest : ModelEntityBase
    {
        public string? WellboreId { get; set; }
        public string WellboreName { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? Description { get; set; }
        
        // Wellbore classification
        public string? WellboreType { get; set; } // e.g., "PRIMARY", "LATERAL", "SIDETRACK"
        public string? Status { get; set; } // e.g., "ACTIVE", "PLUGGED", "ABANDONED"
        public string? TrajectoryType { get; set; } // e.g., "VERTICAL", "DIRECTIONAL", "HORIZONTAL", "DEVIATED"
        
        // Depth information
        public decimal? MeasuredDepth { get; set; }
        public string? MeasuredDepthOuom { get; set; } // e.g., "FT", "M"
        public decimal? TrueVerticalDepth { get; set; }
        public string? TrueVerticalDepthOuom { get; set; }
        public decimal? KickoffDepth { get; set; }
        public string? KickoffDepthOuom { get; set; }
        
        // Geometry
        public decimal? HoleDiameter { get; set; }
        public string? HoleDiameterOuom { get; set; } // e.g., "IN", "MM"
        public decimal? CasingDiameter { get; set; }
        public string? CasingDiameterOuom { get; set; }
        public decimal? TubingDiameter { get; set; }
        public string? TubingDiameterOuom { get; set; }
        
        // Completion information
        public decimal? CompletionTopDepth { get; set; }
        public string? CompletionTopDepthOuom { get; set; }
        public decimal? CompletionBaseDepth { get; set; }
        public string? CompletionBaseDepthOuom { get; set; }
        public decimal? NetPay { get; set; }
        public string? NetPayOuom { get; set; }
        
        // Dates
        public DateTime? DrillingStartDate { get; set; }
        public DateTime? DrillingEndDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing wellbore data (includes audit fields from WELLBORE table)
    /// </summary>
    public class WellboreResponse : ModelEntityBase
    {
        public string WellboreId { get; set; } = string.Empty;
        public string WellboreName { get; set; } = string.Empty;
        public string? WellId { get; set; }
        public string? Description { get; set; }
        
        // Wellbore classification
        public string? WellboreType { get; set; }
        public string? Status { get; set; }
        public string? TrajectoryType { get; set; }
        
        // Depth information
        public decimal? MeasuredDepth { get; set; }
        public string? MeasuredDepthOuom { get; set; }
        public decimal? TrueVerticalDepth { get; set; }
        public string? TrueVerticalDepthOuom { get; set; }
        public decimal? KickoffDepth { get; set; }
        public string? KickoffDepthOuom { get; set; }
        
        // Geometry
        public decimal? HoleDiameter { get; set; }
        public string? HoleDiameterOuom { get; set; }
        public decimal? CasingDiameter { get; set; }
        public string? CasingDiameterOuom { get; set; }
        public decimal? TubingDiameter { get; set; }
        public string? TubingDiameterOuom { get; set; }
        
        // Completion information
        public decimal? CompletionTopDepth { get; set; }
        public string? CompletionTopDepthOuom { get; set; }
        public decimal? CompletionBaseDepth { get; set; }
        public string? CompletionBaseDepthOuom { get; set; }
        public decimal? NetPay { get; set; }
        public string? NetPayOuom { get; set; }
        
        // Dates
        public DateTime? DrillingStartDate { get; set; }
        public DateTime? DrillingEndDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        
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

    #region Feasibility Study DTOs

    /// <summary>
    /// Request for feasibility study calculation
    /// </summary>
    public class FeasibilityStudyRequest : ModelEntityBase
    {
        public string? FieldId { get; set; }
        public string? ProjectId { get; set; }
        public string? StudyType { get; set; } // e.g., "ECONOMIC", "TECHNICAL", "REGULATORY", "COMPREHENSIVE"
        
        // Project scope
        public int? NumberOfWells { get; set; }
        public int? NumberOfFacilities { get; set; }
        public decimal? DevelopmentArea { get; set; }
        public string? DevelopmentAreaOuom { get; set; }
        
        // Capital costs
        public decimal? DrillingCostPerWell { get; set; }
        public string? DrillingCostCurrency { get; set; }
        public decimal? CompletionCostPerWell { get; set; }
        public string? CompletionCostCurrency { get; set; }
        public decimal? FacilityCost { get; set; }
        public string? FacilityCostCurrency { get; set; }
        public decimal? InfrastructureCost { get; set; } // Pipelines, roads, etc.
        public string? InfrastructureCostCurrency { get; set; }
        public decimal? TotalCapitalCost { get; set; }
        public string? TotalCapitalCostCurrency { get; set; }
        
        // Operating costs
        public decimal? OperatingCostPerUnit { get; set; } // Per volume unit
        public string? OperatingCostCurrency { get; set; }
        public decimal? AnnualOperatingCost { get; set; }
        public string? AnnualOperatingCostCurrency { get; set; }
        
        // Production forecast
        public List<FeasibilityProductionPoint>? ProductionForecast { get; set; }
        public int? ProductionLifetimeYears { get; set; }
        
        // Economic parameters
        public decimal? OilPrice { get; set; }
        public string? OilPriceCurrency { get; set; }
        public decimal? GasPrice { get; set; }
        public string? GasPriceCurrency { get; set; }
        public decimal? DiscountRate { get; set; } // Percentage
        public decimal? InflationRate { get; set; } // Percentage
        
        // Fiscal terms
        public decimal? RoyaltyRate { get; set; } // Percentage
        public decimal? TaxRate { get; set; } // Percentage
        public decimal? WorkingInterest { get; set; } // Percentage
        
        // Technical constraints
        public decimal? MaximumProductionRate { get; set; }
        public string? MaximumProductionRateOuom { get; set; }
        public List<string>? RegulatoryRequirements { get; set; }
        public List<string>? TechnicalConstraints { get; set; }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Production point for feasibility study
    /// </summary>
    public class FeasibilityProductionPoint : ModelEntityBase
    {
        public int Year { get; set; }
        public decimal? OilVolume { get; set; }
        public decimal? GasVolume { get; set; }
        public decimal? WaterVolume { get; set; }
        public string? VolumeOuom { get; set; }
        public decimal? OperatingCost { get; set; }
    }

    /// <summary>
    /// Result of feasibility study calculation
    /// </summary>
    public class FeasibilityStudyResponse : ModelEntityBase
    {
        public string StudyId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? ProjectId { get; set; }
        public string? StudyType { get; set; }
        public DateTime StudyDate { get; set; }
        
        // Economic feasibility
        public decimal? NetPresentValue { get; set; } // NPV
        public decimal? InternalRateOfReturn { get; set; } // IRR (percentage)
        public decimal? PaybackPeriod { get; set; } // Years
        public decimal? ReturnOnInvestment { get; set; } // ROI (percentage)
        public decimal? ProfitabilityIndex { get; set; }
        public bool? IsEconomicallyFeasible { get; set; }
        
        // Technical feasibility
        public bool? IsTechnicallyFeasible { get; set; }
        public List<string> TechnicalChallenges { get; set; } = new List<string>();
        public List<string> TechnicalRecommendations { get; set; } = new List<string>();
        
        // Regulatory feasibility
        public bool? IsRegulatorilyFeasible { get; set; }
        public List<string> RegulatoryRequirements { get; set; } = new List<string>();
        public List<string> RegulatoryChallenges { get; set; } = new List<string>();
        
        // Overall feasibility assessment
        public bool? IsFeasible { get; set; }
        public string? FeasibilityStatus { get; set; } // e.g., "FEASIBLE", "MARGINAL", "NOT_FEASIBLE"
        public string? FeasibilityRecommendation { get; set; } // Overall recommendation
        
        // Cost analysis
        public decimal? TotalCapitalCost { get; set; }
        public string? TotalCapitalCostCurrency { get; set; }
        public decimal? TotalOperatingCost { get; set; }
        public string? TotalOperatingCostCurrency { get; set; }
        public decimal? TotalRevenue { get; set; }
        public string? TotalRevenueCurrency { get; set; }
        public decimal? NetCashFlow { get; set; }
        
        // Cash flow analysis
        public List<FeasibilityCashFlowPoint> CashFlowPoints { get; set; } = new List<FeasibilityCashFlowPoint>();
        
        // Production analysis
        public decimal? TotalOilProduction { get; set; }
        public decimal? TotalGasProduction { get; set; }
        public string? ProductionOuom { get; set; }
        public decimal? PeakProductionRate { get; set; }
        public int? PeakProductionYear { get; set; }
        
        // Risk assessment
        public string? RiskLevel { get; set; } // e.g., "LOW", "MEDIUM", "HIGH"
        public List<string> KeyRisks { get; set; } = new List<string>();
        public List<string> RiskMitigationStrategies { get; set; } = new List<string>();
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Cash flow point for feasibility study
    /// </summary>
    public class FeasibilityCashFlowPoint : ModelEntityBase
    {
        public int Year { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? CapitalCosts { get; set; }
        public decimal? OperatingCosts { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? Royalties { get; set; }
        public decimal? NetCashFlow { get; set; }
        public decimal? CumulativeCashFlow { get; set; }
        public decimal? DiscountedCashFlow { get; set; }
        public decimal? CumulativeDiscountedCashFlow { get; set; }
    }

    #endregion
}





