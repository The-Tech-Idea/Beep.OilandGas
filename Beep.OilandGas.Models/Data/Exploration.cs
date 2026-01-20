using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    #region Prospect DTOs

    /// <summary>
    /// Request for creating or updating a prospect (maps to PROSPECT table)
    /// </summary>
    public class ProspectRequest : ModelEntityBase
    {
        public string? ProspectId { get; set; }
        public string ProspectName { get; set; } = string.Empty;
        public string? FieldId { get; set; } // Auto-set by service, but included for clarity
        public string? Description { get; set; }
        
        // Status and classification
        public string? Status { get; set; } // e.g., "ACTIVE", "DRILLED", "ABANDONED"
        public string? RiskLevel { get; set; } // e.g., "LOW", "MEDIUM", "HIGH"
        public string? ProspectType { get; set; }
        public string? PlayType { get; set; }
        
        // Dates
        public DateTime? DiscoveryDate { get; set; }
        public DateTime? FirstDrillDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        
        // Location information
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Elevation { get; set; }
        public string? ElevationOuom { get; set; } // Unit of measure
        public string? LocationDescription { get; set; }
        
        // Depth information
        public decimal? TopDepth { get; set; }
        public decimal? BaseDepth { get; set; }
        public string? DepthOuom { get; set; } // Unit of measure (e.g., "FT", "M")
        
        // Estimated resources
        public decimal? EstimatedOilVolume { get; set; }
        public decimal? EstimatedGasVolume { get; set; }
        public string? EstimatedVolumeOuom { get; set; } // e.g., "BBL", "MSCF"
        public decimal? EstimatedValue { get; set; }
        public string? EstimatedValueCurrency { get; set; }
        
        // Geological information
        public string? FormationName { get; set; }
        public string? StratUnitId { get; set; }
        public decimal? Porosity { get; set; }
        public decimal? Permeability { get; set; }
        public decimal? NetPay { get; set; }
        public string? NetPayOuom { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing prospect data (includes audit fields from PROSPECT table)
    /// </summary>
    public class ProspectResponse : ModelEntityBase
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? Description { get; set; }
        
        // Status and classification
        public string? Status { get; set; }
        public string? RiskLevel { get; set; }
        public string? ProspectType { get; set; }
        public string? PlayType { get; set; }
        
        // Dates
        public DateTime? DiscoveryDate { get; set; }
        public DateTime? FirstDrillDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        
        // Location information
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? Elevation { get; set; }
        public string? ElevationOuom { get; set; }
        public string? LocationDescription { get; set; }
        
        // Depth information
        public decimal? TopDepth { get; set; }
        public decimal? BaseDepth { get; set; }
        public string? DepthOuom { get; set; }
        
        // Estimated resources
        public decimal? EstimatedOilVolume { get; set; }
        public decimal? EstimatedGasVolume { get; set; }
        public string? EstimatedVolumeOuom { get; set; }
        public decimal? EstimatedValue { get; set; }
        public string? EstimatedValueCurrency { get; set; }
        
        // Geological information
        public string? FormationName { get; set; }
        public string? StratUnitId { get; set; }
        public decimal? Porosity { get; set; }
        public decimal? Permeability { get; set; }
        public decimal? NetPay { get; set; }
        public string? NetPayOuom { get; set; }
        
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

    #region Seismic Survey DTOs

    /// <summary>
    /// Request for creating or updating a seismic survey (maps to SEIS_ACQTN_SURVEY table)
    /// </summary>
    public class SeismicSurveyRequest : ModelEntityBase
    {
        public string? SurveyId { get; set; }
        public string SurveyName { get; set; } = string.Empty;
        public string? FieldId { get; set; } // Auto-set by service
        public string? Description { get; set; }
        
        // Survey classification
        public string? SurveyType { get; set; } // e.g., "2D", "3D", "4D"
        public string? AcquisitionMethod { get; set; } // e.g., "MARINE", "LAND", "AIRBORNE"
        public string? Status { get; set; } // e.g., "PLANNED", "ACQUIRED", "PROCESSED", "INTERPRETED"
        
        // Dates
        public DateTime? SurveyStartDate { get; set; }
        public DateTime? SurveyEndDate { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public DateTime? InterpretationDate { get; set; }
        
        // Location and coverage
        public decimal? SurveyArea { get; set; }
        public string? SurveyAreaOuom { get; set; } // e.g., "ACRE", "KM2"
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? LocationDescription { get; set; }
        
        // Technical parameters
        public int? LineCount { get; set; }
        public decimal? LineSpacing { get; set; }
        public string? LineSpacingOuom { get; set; }
        public decimal? RecordLength { get; set; } // Seconds
        public int? SampleRate { get; set; } // Hz
        public string? DataFormat { get; set; }
        
        // Contractor and cost information
        public string? ContractorId { get; set; }
        public decimal? SurveyCost { get; set; }
        public string? SurveyCostCurrency { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing seismic survey data (includes audit fields from SEIS_ACQTN_SURVEY table)
    /// </summary>
    public class SeismicSurveyResponse : ModelEntityBase
    {
        public string SurveyId { get; set; } = string.Empty;
        public string SurveyName { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? Description { get; set; }
        
        // Survey classification
        public string? SurveyType { get; set; }
        public string? AcquisitionMethod { get; set; }
        public string? Status { get; set; }
        
        // Dates
        public DateTime? SurveyStartDate { get; set; }
        public DateTime? SurveyEndDate { get; set; }
        public DateTime? ProcessingDate { get; set; }
        public DateTime? InterpretationDate { get; set; }
        
        // Location and coverage
        public decimal? SurveyArea { get; set; }
        public string? SurveyAreaOuom { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string? LocationDescription { get; set; }
        
        // Technical parameters
        public int? LineCount { get; set; }
        public decimal? LineSpacing { get; set; }
        public string? LineSpacingOuom { get; set; }
        public decimal? RecordLength { get; set; }
        public int? SampleRate { get; set; }
        public string? DataFormat { get; set; }
        
        // Contractor and cost information
        public string? ContractorId { get; set; }
        public decimal? SurveyCost { get; set; }
        public string? SurveyCostCurrency { get; set; }
        
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

    #region Seismic Line DTOs

    /// <summary>
    /// Request for creating or updating a seismic line (maps to SEISMIC_LINE table)
    /// </summary>
    public class SeismicLineRequest : ModelEntityBase
    {
        public string? LineId { get; set; }
        public string LineName { get; set; } = string.Empty;
        public string? SurveyId { get; set; }
        public string? Description { get; set; }
        
        // Line classification
        public string? LineType { get; set; } // e.g., "INLINE", "CROSSLINE", "RANDOM"
        public string? Status { get; set; }
        
        // Coordinates
        public decimal? StartLatitude { get; set; }
        public decimal? StartLongitude { get; set; }
        public decimal? EndLatitude { get; set; }
        public decimal? EndLongitude { get; set; }
        public decimal? LineLength { get; set; }
        public string? LineLengthOuom { get; set; } // e.g., "KM", "MI"
        
        // Technical parameters
        public int? ShotPointCount { get; set; }
        public decimal? ShotPointInterval { get; set; }
        public string? ShotPointIntervalOuom { get; set; }
        public int? TraceCount { get; set; }
        public decimal? TraceInterval { get; set; }
        public string? TraceIntervalOuom { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing seismic line data (includes audit fields from SEISMIC_LINE table)
    /// </summary>
    public class SeismicLineResponse : ModelEntityBase
    {
        public string LineId { get; set; } = string.Empty;
        public string LineName { get; set; } = string.Empty;
        public string? SurveyId { get; set; }
        public string? Description { get; set; }
        
        // Line classification
        public string? LineType { get; set; }
        public string? Status { get; set; }
        
        // Coordinates
        public decimal? StartLatitude { get; set; }
        public decimal? StartLongitude { get; set; }
        public decimal? EndLatitude { get; set; }
        public decimal? EndLongitude { get; set; }
        public decimal? LineLength { get; set; }
        public string? LineLengthOuom { get; set; }
        
        // Technical parameters
        public int? ShotPointCount { get; set; }
        public decimal? ShotPointInterval { get; set; }
        public string? ShotPointIntervalOuom { get; set; }
        public int? TraceCount { get; set; }
        public decimal? TraceInterval { get; set; }
        public string? TraceIntervalOuom { get; set; }
        
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

    #region Exploratory Well DTOs

    /// <summary>
    /// Request for creating or updating an exploratory well (maps to WELL table with WELL_TYPE='EXPLORATION')
    /// </summary>
    public class ExploratoryWellRequest : ModelEntityBase
    {
        public string? WellId { get; set; }
        public string WellName { get; set; } = string.Empty;
        public string? FieldId { get; set; } // Auto-set by service
        public string? ProspectId { get; set; }
        public string? Description { get; set; }
        
        // Well classification
        public string WellType { get; set; } = "EXPLORATION"; // Should be "EXPLORATION"
        public string? Status { get; set; } // e.g., "DRILLING", "COMPLETED", "ABANDONED"
        public string? WellClassification { get; set; } // e.g., "WILDCAT", "APPRAISAL"
        
        // Dates
        public DateTime? SpudDate { get; set; }
        public DateTime? CompletionDate { get; set; }
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
        
        // Results
        public string? WellResult { get; set; } // e.g., "DRY", "OIL", "GAS", "OIL_GAS"
        public string? DiscoveryIndicator { get; set; } // e.g., "Y", "N"
        public decimal? EstimatedOilVolume { get; set; }
        public decimal? EstimatedGasVolume { get; set; }
        
        // Common PPDM fields
        public string? ActiveInd { get; set; }
        public string? Remark { get; set; }
        public string? Source { get; set; }
        public string? RowQuality { get; set; }
    }

    /// <summary>
    /// Response containing exploratory well data (includes audit fields from WELL table)
    /// </summary>
    public class ExploratoryWellResponse : ModelEntityBase
    {
        public string WellId { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? ProspectId { get; set; }
        public string? Description { get; set; }
        
        // Well classification
        public string? WellType { get; set; }
        public string? Status { get; set; }
        public string? WellClassification { get; set; }
        
        // Dates
        public DateTime? SpudDate { get; set; }
        public DateTime? CompletionDate { get; set; }
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
        
        // Results
        public string? WellResult { get; set; }
        public string? DiscoveryIndicator { get; set; }
        public decimal? EstimatedOilVolume { get; set; }
        public decimal? EstimatedGasVolume { get; set; }
        
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

    #region Risk Assessment DTOs

    /// <summary>
    /// Request for prospect risk assessment calculation
    /// </summary>
    public class RiskAssessmentRequest : ModelEntityBase
    {
        public string? ProspectId { get; set; }
        public string? FieldId { get; set; }
        
        // Risk parameters
        public string? RiskModel { get; set; } // e.g., "VOLUMETRIC", "MONTE_CARLO", "DETERMINISTIC"
        public decimal? TrapRisk { get; set; } // Probability (0-1)
        public decimal? ReservoirRisk { get; set; } // Probability (0-1)
        public decimal? SealRisk { get; set; } // Probability (0-1)
        public decimal? SourceRisk { get; set; } // Probability (0-1)
        public decimal? TimingRisk { get; set; } // Probability (0-1)
        
        // Volume estimates (for volumetric risk)
        public decimal? LowEstimateOil { get; set; } // P90
        public decimal? BestEstimateOil { get; set; } // P50
        public decimal? HighEstimateOil { get; set; } // P10
        public decimal? LowEstimateGas { get; set; } // P90
        public decimal? BestEstimateGas { get; set; } // P50
        public decimal? HighEstimateGas { get; set; } // P10
        
        // Economic parameters (optional)
        public decimal? OilPrice { get; set; }
        public decimal? GasPrice { get; set; }
        public decimal? DevelopmentCost { get; set; }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        public string? UserId { get; set; }
    }

    /// <summary>
    /// Result of prospect risk assessment calculation
    /// </summary>
    public class RiskAssessmentResponse : ModelEntityBase
    {
        public string AssessmentId { get; set; } = string.Empty;
        public string? ProspectId { get; set; }
        public string? FieldId { get; set; }
        public string? RiskModel { get; set; }
        public DateTime AssessmentDate { get; set; }
        
        // Risk probabilities
        public decimal? TrapRisk { get; set; }
        public decimal? ReservoirRisk { get; set; }
        public decimal? SealRisk { get; set; }
        public decimal? SourceRisk { get; set; }
        public decimal? TimingRisk { get; set; }
        public decimal? OverallGeologicalRisk { get; set; } // Product of all risks
        
        // Risked volumes (unrisked volume * overall risk)
        public decimal? RiskedOilVolume { get; set; }
        public decimal? RiskedGasVolume { get; set; }
        public decimal? UnriskedOilVolume { get; set; }
        public decimal? UnriskedGasVolume { get; set; }
        
        // Volume estimates
        public decimal? LowEstimateOil { get; set; }
        public decimal? BestEstimateOil { get; set; }
        public decimal? HighEstimateOil { get; set; }
        public decimal? LowEstimateGas { get; set; }
        public decimal? BestEstimateGas { get; set; }
        public decimal? HighEstimateGas { get; set; }
        
        // Economic assessment (if provided)
        public decimal? RiskedNPV { get; set; }
        public decimal? UnriskedNPV { get; set; }
        public decimal? ExpectedMonetaryValue { get; set; }
        
        // Risk classification
        public string? RiskCategory { get; set; } // e.g., "LOW", "MEDIUM", "HIGH", "VERY_HIGH"
        public List<string> RiskFactors { get; set; } = new List<string>(); // List of key risk factors
        public List<string> Recommendations { get; set; } = new List<string>(); // Risk mitigation recommendations
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        public string? Status { get; set; } // SUCCESS, FAILED
        public string? ErrorMessage { get; set; }
        public string? UserId { get; set; }
    }

    #endregion
}





