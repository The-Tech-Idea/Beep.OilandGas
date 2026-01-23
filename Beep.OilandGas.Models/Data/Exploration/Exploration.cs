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
        private string? ProspectIdValue;

        public string? ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ProspectNameValue = string.Empty;

        public string ProspectName

        {

            get { return this.ProspectNameValue; }

            set { SetProperty(ref ProspectNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service, but included for clarity
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Status and classification
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "ACTIVE", "DRILLED", "ABANDONED"
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        } // e.g., "LOW", "MEDIUM", "HIGH"
        private string? ProspectTypeValue;

        public string? ProspectType

        {

            get { return this.ProspectTypeValue; }

            set { SetProperty(ref ProspectTypeValue, value); }

        }
        private string? PlayTypeValue;

        public string? PlayType

        {

            get { return this.PlayTypeValue; }

            set { SetProperty(ref PlayTypeValue, value); }

        }
        
        // Dates
        private DateTime? DiscoveryDateValue;

        public DateTime? DiscoveryDate

        {

            get { return this.DiscoveryDateValue; }

            set { SetProperty(ref DiscoveryDateValue, value); }

        }
        private DateTime? FirstDrillDateValue;

        public DateTime? FirstDrillDate

        {

            get { return this.FirstDrillDateValue; }

            set { SetProperty(ref FirstDrillDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        
        // Location information
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private decimal? ElevationValue;

        public decimal? Elevation

        {

            get { return this.ElevationValue; }

            set { SetProperty(ref ElevationValue, value); }

        }
        private string? ElevationOuomValue;

        public string? ElevationOuom

        {

            get { return this.ElevationOuomValue; }

            set { SetProperty(ref ElevationOuomValue, value); }

        } // Unit of measure
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Depth information
        private decimal? TopDepthValue;

        public decimal? TopDepth

        {

            get { return this.TopDepthValue; }

            set { SetProperty(ref TopDepthValue, value); }

        }
        private decimal? BaseDepthValue;

        public decimal? BaseDepth

        {

            get { return this.BaseDepthValue; }

            set { SetProperty(ref BaseDepthValue, value); }

        }
        private string? DepthOuomValue;

        public string? DepthOuom

        {

            get { return this.DepthOuomValue; }

            set { SetProperty(ref DepthOuomValue, value); }

        } // Unit of measure (e.g., "FT", "M")
        
        // Estimated resources
        private decimal? EstimatedOilVolumeValue;

        public decimal? EstimatedOilVolume

        {

            get { return this.EstimatedOilVolumeValue; }

            set { SetProperty(ref EstimatedOilVolumeValue, value); }

        }
        private decimal? EstimatedGasVolumeValue;

        public decimal? EstimatedGasVolume

        {

            get { return this.EstimatedGasVolumeValue; }

            set { SetProperty(ref EstimatedGasVolumeValue, value); }

        }
        private string? EstimatedVolumeOuomValue;

        public string? EstimatedVolumeOuom

        {

            get { return this.EstimatedVolumeOuomValue; }

            set { SetProperty(ref EstimatedVolumeOuomValue, value); }

        } // e.g., "BBL", "MSCF"
        private decimal? EstimatedValueValue;

        public decimal? EstimatedValue

        {

            get { return this.EstimatedValueValue; }

            set { SetProperty(ref EstimatedValueValue, value); }

        }
        private string? EstimatedValueCurrencyValue;

        public string? EstimatedValueCurrency

        {

            get { return this.EstimatedValueCurrencyValue; }

            set { SetProperty(ref EstimatedValueCurrencyValue, value); }

        }
        
        // Geological information
        private string? FormationNameValue;

        public string? FormationName

        {

            get { return this.FormationNameValue; }

            set { SetProperty(ref FormationNameValue, value); }

        }
        private string? StratUnitIdValue;

        public string? StratUnitId

        {

            get { return this.StratUnitIdValue; }

            set { SetProperty(ref StratUnitIdValue, value); }

        }
        private decimal? PorosityValue;

        public decimal? Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }
        private decimal? PermeabilityValue;

        public decimal? Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private decimal? NetPayValue;

        public decimal? NetPay

        {

            get { return this.NetPayValue; }

            set { SetProperty(ref NetPayValue, value); }

        }
        private string? NetPayOuomValue;

        public string? NetPayOuom

        {

            get { return this.NetPayOuomValue; }

            set { SetProperty(ref NetPayOuomValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
    }

    /// <summary>
    /// Response containing prospect data (includes audit fields from PROSPECT table)
    /// </summary>
    public class ProspectResponse : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string ProspectNameValue = string.Empty;

        public string ProspectName

        {

            get { return this.ProspectNameValue; }

            set { SetProperty(ref ProspectNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Status and classification
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? RiskLevelValue;

        public string? RiskLevel

        {

            get { return this.RiskLevelValue; }

            set { SetProperty(ref RiskLevelValue, value); }

        }
        private string? ProspectTypeValue;

        public string? ProspectType

        {

            get { return this.ProspectTypeValue; }

            set { SetProperty(ref ProspectTypeValue, value); }

        }
        private string? PlayTypeValue;

        public string? PlayType

        {

            get { return this.PlayTypeValue; }

            set { SetProperty(ref PlayTypeValue, value); }

        }
        
        // Dates
        private DateTime? DiscoveryDateValue;

        public DateTime? DiscoveryDate

        {

            get { return this.DiscoveryDateValue; }

            set { SetProperty(ref DiscoveryDateValue, value); }

        }
        private DateTime? FirstDrillDateValue;

        public DateTime? FirstDrillDate

        {

            get { return this.FirstDrillDateValue; }

            set { SetProperty(ref FirstDrillDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        
        // Location information
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private decimal? ElevationValue;

        public decimal? Elevation

        {

            get { return this.ElevationValue; }

            set { SetProperty(ref ElevationValue, value); }

        }
        private string? ElevationOuomValue;

        public string? ElevationOuom

        {

            get { return this.ElevationOuomValue; }

            set { SetProperty(ref ElevationOuomValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Depth information
        private decimal? TopDepthValue;

        public decimal? TopDepth

        {

            get { return this.TopDepthValue; }

            set { SetProperty(ref TopDepthValue, value); }

        }
        private decimal? BaseDepthValue;

        public decimal? BaseDepth

        {

            get { return this.BaseDepthValue; }

            set { SetProperty(ref BaseDepthValue, value); }

        }
        private string? DepthOuomValue;

        public string? DepthOuom

        {

            get { return this.DepthOuomValue; }

            set { SetProperty(ref DepthOuomValue, value); }

        }
        
        // Estimated resources
        private decimal? EstimatedOilVolumeValue;

        public decimal? EstimatedOilVolume

        {

            get { return this.EstimatedOilVolumeValue; }

            set { SetProperty(ref EstimatedOilVolumeValue, value); }

        }
        private decimal? EstimatedGasVolumeValue;

        public decimal? EstimatedGasVolume

        {

            get { return this.EstimatedGasVolumeValue; }

            set { SetProperty(ref EstimatedGasVolumeValue, value); }

        }
        private string? EstimatedVolumeOuomValue;

        public string? EstimatedVolumeOuom

        {

            get { return this.EstimatedVolumeOuomValue; }

            set { SetProperty(ref EstimatedVolumeOuomValue, value); }

        }
        private decimal? EstimatedValueValue;

        public decimal? EstimatedValue

        {

            get { return this.EstimatedValueValue; }

            set { SetProperty(ref EstimatedValueValue, value); }

        }
        private string? EstimatedValueCurrencyValue;

        public string? EstimatedValueCurrency

        {

            get { return this.EstimatedValueCurrencyValue; }

            set { SetProperty(ref EstimatedValueCurrencyValue, value); }

        }
        
        // Geological information
        private string? FormationNameValue;

        public string? FormationName

        {

            get { return this.FormationNameValue; }

            set { SetProperty(ref FormationNameValue, value); }

        }
        private string? StratUnitIdValue;

        public string? StratUnitId

        {

            get { return this.StratUnitIdValue; }

            set { SetProperty(ref StratUnitIdValue, value); }

        }
        private decimal? PorosityValue;

        public decimal? Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }
        private decimal? PermeabilityValue;

        public decimal? Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private decimal? NetPayValue;

        public decimal? NetPay

        {

            get { return this.NetPayValue; }

            set { SetProperty(ref NetPayValue, value); }

        }
        private string? NetPayOuomValue;

        public string? NetPayOuom

        {

            get { return this.NetPayOuomValue; }

            set { SetProperty(ref NetPayOuomValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }

    #endregion

    #region Seismic Survey DTOs

    /// <summary>
    /// Request for creating or updating a seismic survey (maps to SEIS_ACQTN_SURVEY table)
    /// </summary>
    public class SeismicSurveyRequest : ModelEntityBase
    {
        private string? SurveyIdValue;

        public string? SurveyId

        {

            get { return this.SurveyIdValue; }

            set { SetProperty(ref SurveyIdValue, value); }

        }
        private string SurveyNameValue = string.Empty;

        public string SurveyName

        {

            get { return this.SurveyNameValue; }

            set { SetProperty(ref SurveyNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Survey classification
        private string? SurveyTypeValue;

        public string? SurveyType

        {

            get { return this.SurveyTypeValue; }

            set { SetProperty(ref SurveyTypeValue, value); }

        } // e.g., "2D", "3D", "4D"
        private string? AcquisitionMethodValue;

        public string? AcquisitionMethod

        {

            get { return this.AcquisitionMethodValue; }

            set { SetProperty(ref AcquisitionMethodValue, value); }

        } // e.g., "MARINE", "LAND", "AIRBORNE"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "PLANNED", "ACQUIRED", "PROCESSED", "INTERPRETED"
        
        // Dates
        private DateTime? SurveyStartDateValue;

        public DateTime? SurveyStartDate

        {

            get { return this.SurveyStartDateValue; }

            set { SetProperty(ref SurveyStartDateValue, value); }

        }
        private DateTime? SurveyEndDateValue;

        public DateTime? SurveyEndDate

        {

            get { return this.SurveyEndDateValue; }

            set { SetProperty(ref SurveyEndDateValue, value); }

        }
        private DateTime? ProcessingDateValue;

        public DateTime? ProcessingDate

        {

            get { return this.ProcessingDateValue; }

            set { SetProperty(ref ProcessingDateValue, value); }

        }
        private DateTime? InterpretationDateValue;

        public DateTime? InterpretationDate

        {

            get { return this.InterpretationDateValue; }

            set { SetProperty(ref InterpretationDateValue, value); }

        }
        
        // Location and coverage
        private decimal? SurveyAreaValue;

        public decimal? SurveyArea

        {

            get { return this.SurveyAreaValue; }

            set { SetProperty(ref SurveyAreaValue, value); }

        }
        private string? SurveyAreaOuomValue;

        public string? SurveyAreaOuom

        {

            get { return this.SurveyAreaOuomValue; }

            set { SetProperty(ref SurveyAreaOuomValue, value); }

        } // e.g., "ACRE", "KM2"
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Technical parameters
        private int? LineCountValue;

        public int? LineCount

        {

            get { return this.LineCountValue; }

            set { SetProperty(ref LineCountValue, value); }

        }
        private decimal? LineSpacingValue;

        public decimal? LineSpacing

        {

            get { return this.LineSpacingValue; }

            set { SetProperty(ref LineSpacingValue, value); }

        }
        private string? LineSpacingOuomValue;

        public string? LineSpacingOuom

        {

            get { return this.LineSpacingOuomValue; }

            set { SetProperty(ref LineSpacingOuomValue, value); }

        }
        private decimal? RecordLengthValue;

        public decimal? RecordLength

        {

            get { return this.RecordLengthValue; }

            set { SetProperty(ref RecordLengthValue, value); }

        } // Seconds
        private int? SampleRateValue;

        public int? SampleRate

        {

            get { return this.SampleRateValue; }

            set { SetProperty(ref SampleRateValue, value); }

        } // Hz
        private string? DataFormatValue;

        public string? DataFormat

        {

            get { return this.DataFormatValue; }

            set { SetProperty(ref DataFormatValue, value); }

        }
        
        // Contractor and cost information
        private string? ContractorIdValue;

        public string? ContractorId

        {

            get { return this.ContractorIdValue; }

            set { SetProperty(ref ContractorIdValue, value); }

        }
        private decimal? SurveyCostValue;

        public decimal? SurveyCost

        {

            get { return this.SurveyCostValue; }

            set { SetProperty(ref SurveyCostValue, value); }

        }
        private string? SurveyCostCurrencyValue;

        public string? SurveyCostCurrency

        {

            get { return this.SurveyCostCurrencyValue; }

            set { SetProperty(ref SurveyCostCurrencyValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
    }

    /// <summary>
    /// Response containing seismic survey data (includes audit fields from SEIS_ACQTN_SURVEY table)
    /// </summary>
    public class SeismicSurveyResponse : ModelEntityBase
    {
        private string SurveyIdValue = string.Empty;

        public string SurveyId

        {

            get { return this.SurveyIdValue; }

            set { SetProperty(ref SurveyIdValue, value); }

        }
        private string SurveyNameValue = string.Empty;

        public string SurveyName

        {

            get { return this.SurveyNameValue; }

            set { SetProperty(ref SurveyNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Survey classification
        private string? SurveyTypeValue;

        public string? SurveyType

        {

            get { return this.SurveyTypeValue; }

            set { SetProperty(ref SurveyTypeValue, value); }

        }
        private string? AcquisitionMethodValue;

        public string? AcquisitionMethod

        {

            get { return this.AcquisitionMethodValue; }

            set { SetProperty(ref AcquisitionMethodValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Dates
        private DateTime? SurveyStartDateValue;

        public DateTime? SurveyStartDate

        {

            get { return this.SurveyStartDateValue; }

            set { SetProperty(ref SurveyStartDateValue, value); }

        }
        private DateTime? SurveyEndDateValue;

        public DateTime? SurveyEndDate

        {

            get { return this.SurveyEndDateValue; }

            set { SetProperty(ref SurveyEndDateValue, value); }

        }
        private DateTime? ProcessingDateValue;

        public DateTime? ProcessingDate

        {

            get { return this.ProcessingDateValue; }

            set { SetProperty(ref ProcessingDateValue, value); }

        }
        private DateTime? InterpretationDateValue;

        public DateTime? InterpretationDate

        {

            get { return this.InterpretationDateValue; }

            set { SetProperty(ref InterpretationDateValue, value); }

        }
        
        // Location and coverage
        private decimal? SurveyAreaValue;

        public decimal? SurveyArea

        {

            get { return this.SurveyAreaValue; }

            set { SetProperty(ref SurveyAreaValue, value); }

        }
        private string? SurveyAreaOuomValue;

        public string? SurveyAreaOuom

        {

            get { return this.SurveyAreaOuomValue; }

            set { SetProperty(ref SurveyAreaOuomValue, value); }

        }
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Technical parameters
        private int? LineCountValue;

        public int? LineCount

        {

            get { return this.LineCountValue; }

            set { SetProperty(ref LineCountValue, value); }

        }
        private decimal? LineSpacingValue;

        public decimal? LineSpacing

        {

            get { return this.LineSpacingValue; }

            set { SetProperty(ref LineSpacingValue, value); }

        }
        private string? LineSpacingOuomValue;

        public string? LineSpacingOuom

        {

            get { return this.LineSpacingOuomValue; }

            set { SetProperty(ref LineSpacingOuomValue, value); }

        }
        private decimal? RecordLengthValue;

        public decimal? RecordLength

        {

            get { return this.RecordLengthValue; }

            set { SetProperty(ref RecordLengthValue, value); }

        }
        private int? SampleRateValue;

        public int? SampleRate

        {

            get { return this.SampleRateValue; }

            set { SetProperty(ref SampleRateValue, value); }

        }
        private string? DataFormatValue;

        public string? DataFormat

        {

            get { return this.DataFormatValue; }

            set { SetProperty(ref DataFormatValue, value); }

        }
        
        // Contractor and cost information
        private string? ContractorIdValue;

        public string? ContractorId

        {

            get { return this.ContractorIdValue; }

            set { SetProperty(ref ContractorIdValue, value); }

        }
        private decimal? SurveyCostValue;

        public decimal? SurveyCost

        {

            get { return this.SurveyCostValue; }

            set { SetProperty(ref SurveyCostValue, value); }

        }
        private string? SurveyCostCurrencyValue;

        public string? SurveyCostCurrency

        {

            get { return this.SurveyCostCurrencyValue; }

            set { SetProperty(ref SurveyCostCurrencyValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }

    #endregion

    #region Seismic Line DTOs

    /// <summary>
    /// Request for creating or updating a seismic line (maps to SEISMIC_LINE table)
    /// </summary>
    public class SeismicLineRequest : ModelEntityBase
    {
        private string? LineIdValue;

        public string? LineId

        {

            get { return this.LineIdValue; }

            set { SetProperty(ref LineIdValue, value); }

        }
        private string LineNameValue = string.Empty;

        public string LineName

        {

            get { return this.LineNameValue; }

            set { SetProperty(ref LineNameValue, value); }

        }
        private string? SurveyIdValue;

        public string? SurveyId

        {

            get { return this.SurveyIdValue; }

            set { SetProperty(ref SurveyIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Line classification
        private string? LineTypeValue;

        public string? LineType

        {

            get { return this.LineTypeValue; }

            set { SetProperty(ref LineTypeValue, value); }

        } // e.g., "INLINE", "CROSSLINE", "RANDOM"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Coordinates
        private decimal? StartLatitudeValue;

        public decimal? StartLatitude

        {

            get { return this.StartLatitudeValue; }

            set { SetProperty(ref StartLatitudeValue, value); }

        }
        private decimal? StartLongitudeValue;

        public decimal? StartLongitude

        {

            get { return this.StartLongitudeValue; }

            set { SetProperty(ref StartLongitudeValue, value); }

        }
        private decimal? EndLatitudeValue;

        public decimal? EndLatitude

        {

            get { return this.EndLatitudeValue; }

            set { SetProperty(ref EndLatitudeValue, value); }

        }
        private decimal? EndLongitudeValue;

        public decimal? EndLongitude

        {

            get { return this.EndLongitudeValue; }

            set { SetProperty(ref EndLongitudeValue, value); }

        }
        private decimal? LineLengthValue;

        public decimal? LineLength

        {

            get { return this.LineLengthValue; }

            set { SetProperty(ref LineLengthValue, value); }

        }
        private string? LineLengthOuomValue;

        public string? LineLengthOuom

        {

            get { return this.LineLengthOuomValue; }

            set { SetProperty(ref LineLengthOuomValue, value); }

        } // e.g., "KM", "MI"
        
        // Technical parameters
        private int? ShotPointCountValue;

        public int? ShotPointCount

        {

            get { return this.ShotPointCountValue; }

            set { SetProperty(ref ShotPointCountValue, value); }

        }
        private decimal? ShotPointIntervalValue;

        public decimal? ShotPointInterval

        {

            get { return this.ShotPointIntervalValue; }

            set { SetProperty(ref ShotPointIntervalValue, value); }

        }
        private string? ShotPointIntervalOuomValue;

        public string? ShotPointIntervalOuom

        {

            get { return this.ShotPointIntervalOuomValue; }

            set { SetProperty(ref ShotPointIntervalOuomValue, value); }

        }
        private int? TraceCountValue;

        public int? TraceCount

        {

            get { return this.TraceCountValue; }

            set { SetProperty(ref TraceCountValue, value); }

        }
        private decimal? TraceIntervalValue;

        public decimal? TraceInterval

        {

            get { return this.TraceIntervalValue; }

            set { SetProperty(ref TraceIntervalValue, value); }

        }
        private string? TraceIntervalOuomValue;

        public string? TraceIntervalOuom

        {

            get { return this.TraceIntervalOuomValue; }

            set { SetProperty(ref TraceIntervalOuomValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
    }

    /// <summary>
    /// Response containing seismic line data (includes audit fields from SEISMIC_LINE table)
    /// </summary>
    public class SeismicLineResponse : ModelEntityBase
    {
        private string LineIdValue = string.Empty;

        public string LineId

        {

            get { return this.LineIdValue; }

            set { SetProperty(ref LineIdValue, value); }

        }
        private string LineNameValue = string.Empty;

        public string LineName

        {

            get { return this.LineNameValue; }

            set { SetProperty(ref LineNameValue, value); }

        }
        private string? SurveyIdValue;

        public string? SurveyId

        {

            get { return this.SurveyIdValue; }

            set { SetProperty(ref SurveyIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Line classification
        private string? LineTypeValue;

        public string? LineType

        {

            get { return this.LineTypeValue; }

            set { SetProperty(ref LineTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Coordinates
        private decimal? StartLatitudeValue;

        public decimal? StartLatitude

        {

            get { return this.StartLatitudeValue; }

            set { SetProperty(ref StartLatitudeValue, value); }

        }
        private decimal? StartLongitudeValue;

        public decimal? StartLongitude

        {

            get { return this.StartLongitudeValue; }

            set { SetProperty(ref StartLongitudeValue, value); }

        }
        private decimal? EndLatitudeValue;

        public decimal? EndLatitude

        {

            get { return this.EndLatitudeValue; }

            set { SetProperty(ref EndLatitudeValue, value); }

        }
        private decimal? EndLongitudeValue;

        public decimal? EndLongitude

        {

            get { return this.EndLongitudeValue; }

            set { SetProperty(ref EndLongitudeValue, value); }

        }
        private decimal? LineLengthValue;

        public decimal? LineLength

        {

            get { return this.LineLengthValue; }

            set { SetProperty(ref LineLengthValue, value); }

        }
        private string? LineLengthOuomValue;

        public string? LineLengthOuom

        {

            get { return this.LineLengthOuomValue; }

            set { SetProperty(ref LineLengthOuomValue, value); }

        }
        
        // Technical parameters
        private int? ShotPointCountValue;

        public int? ShotPointCount

        {

            get { return this.ShotPointCountValue; }

            set { SetProperty(ref ShotPointCountValue, value); }

        }
        private decimal? ShotPointIntervalValue;

        public decimal? ShotPointInterval

        {

            get { return this.ShotPointIntervalValue; }

            set { SetProperty(ref ShotPointIntervalValue, value); }

        }
        private string? ShotPointIntervalOuomValue;

        public string? ShotPointIntervalOuom

        {

            get { return this.ShotPointIntervalOuomValue; }

            set { SetProperty(ref ShotPointIntervalOuomValue, value); }

        }
        private int? TraceCountValue;

        public int? TraceCount

        {

            get { return this.TraceCountValue; }

            set { SetProperty(ref TraceCountValue, value); }

        }
        private decimal? TraceIntervalValue;

        public decimal? TraceInterval

        {

            get { return this.TraceIntervalValue; }

            set { SetProperty(ref TraceIntervalValue, value); }

        }
        private string? TraceIntervalOuomValue;

        public string? TraceIntervalOuom

        {

            get { return this.TraceIntervalOuomValue; }

            set { SetProperty(ref TraceIntervalOuomValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }

    #endregion

    #region Exploratory Well DTOs

    /// <summary>
    /// Request for creating or updating an exploratory well (maps to WELL table with WELL_TYPE='EXPLORATION')
    /// </summary>
    public class ExploratoryWellRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Auto-set by service
        private string? ProspectIdValue;

        public string? ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Well classification
        private string WellTypeValue = "EXPLORATION";

        public string WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        } // Should be "EXPLORATION"
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // e.g., "DRILLING", "COMPLETED", "ABANDONED"
        private string? WellClassificationValue;

        public string? WellClassification

        {

            get { return this.WellClassificationValue; }

            set { SetProperty(ref WellClassificationValue, value); }

        } // e.g., "WILDCAT", "APPRAISAL"
        
        // Dates
        private DateTime? SpudDateValue;

        public DateTime? SpudDate

        {

            get { return this.SpudDateValue; }

            set { SetProperty(ref SpudDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private DateTime? RigReleaseDateValue;

        public DateTime? RigReleaseDate

        {

            get { return this.RigReleaseDateValue; }

            set { SetProperty(ref RigReleaseDateValue, value); }

        }
        
        // Location
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private decimal? GroundElevationValue;

        public decimal? GroundElevation

        {

            get { return this.GroundElevationValue; }

            set { SetProperty(ref GroundElevationValue, value); }

        }
        private string? GroundElevationOuomValue;

        public string? GroundElevationOuom

        {

            get { return this.GroundElevationOuomValue; }

            set { SetProperty(ref GroundElevationOuomValue, value); }

        }
        private decimal? KBElevationValue;

        public decimal? KBElevation

        {

            get { return this.KBElevationValue; }

            set { SetProperty(ref KBElevationValue, value); }

        } // Kelly Bushing Elevation
        private string? KBElevationOuomValue;

        public string? KBElevationOuom

        {

            get { return this.KBElevationOuomValue; }

            set { SetProperty(ref KBElevationOuomValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Depth information
        private decimal? TotalDepthValue;

        public decimal? TotalDepth

        {

            get { return this.TotalDepthValue; }

            set { SetProperty(ref TotalDepthValue, value); }

        }
        private string? TotalDepthOuomValue;

        public string? TotalDepthOuom

        {

            get { return this.TotalDepthOuomValue; }

            set { SetProperty(ref TotalDepthOuomValue, value); }

        } // e.g., "FT", "M"
        private decimal? MeasuredDepthValue;

        public decimal? MeasuredDepth

        {

            get { return this.MeasuredDepthValue; }

            set { SetProperty(ref MeasuredDepthValue, value); }

        }
        private string? MeasuredDepthOuomValue;

        public string? MeasuredDepthOuom

        {

            get { return this.MeasuredDepthOuomValue; }

            set { SetProperty(ref MeasuredDepthOuomValue, value); }

        }
        private decimal? TrueVerticalDepthValue;

        public decimal? TrueVerticalDepth

        {

            get { return this.TrueVerticalDepthValue; }

            set { SetProperty(ref TrueVerticalDepthValue, value); }

        }
        private string? TrueVerticalDepthOuomValue;

        public string? TrueVerticalDepthOuom

        {

            get { return this.TrueVerticalDepthOuomValue; }

            set { SetProperty(ref TrueVerticalDepthOuomValue, value); }

        }
        
        // Results
        private string? WellResultValue;

        public string? WellResult

        {

            get { return this.WellResultValue; }

            set { SetProperty(ref WellResultValue, value); }

        } // e.g., "DRY", "OIL", "GAS", "OIL_GAS"
        private string? DiscoveryIndicatorValue;

        public string? DiscoveryIndicator

        {

            get { return this.DiscoveryIndicatorValue; }

            set { SetProperty(ref DiscoveryIndicatorValue, value); }

        } // e.g., "Y", "N"
        private decimal? EstimatedOilVolumeValue;

        public decimal? EstimatedOilVolume

        {

            get { return this.EstimatedOilVolumeValue; }

            set { SetProperty(ref EstimatedOilVolumeValue, value); }

        }
        private decimal? EstimatedGasVolumeValue;

        public decimal? EstimatedGasVolume

        {

            get { return this.EstimatedGasVolumeValue; }

            set { SetProperty(ref EstimatedGasVolumeValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
    }

    /// <summary>
    /// Response containing exploratory well data (includes audit fields from WELL table)
    /// </summary>
    public class ExploratoryWellResponse : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? ProspectIdValue;

        public string? ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Well classification
        private string? WellTypeValue;

        public string? WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? WellClassificationValue;

        public string? WellClassification

        {

            get { return this.WellClassificationValue; }

            set { SetProperty(ref WellClassificationValue, value); }

        }
        
        // Dates
        private DateTime? SpudDateValue;

        public DateTime? SpudDate

        {

            get { return this.SpudDateValue; }

            set { SetProperty(ref SpudDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private DateTime? RigReleaseDateValue;

        public DateTime? RigReleaseDate

        {

            get { return this.RigReleaseDateValue; }

            set { SetProperty(ref RigReleaseDateValue, value); }

        }
        
        // Location
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private decimal? GroundElevationValue;

        public decimal? GroundElevation

        {

            get { return this.GroundElevationValue; }

            set { SetProperty(ref GroundElevationValue, value); }

        }
        private string? GroundElevationOuomValue;

        public string? GroundElevationOuom

        {

            get { return this.GroundElevationOuomValue; }

            set { SetProperty(ref GroundElevationOuomValue, value); }

        }
        private decimal? KBElevationValue;

        public decimal? KBElevation

        {

            get { return this.KBElevationValue; }

            set { SetProperty(ref KBElevationValue, value); }

        }
        private string? KBElevationOuomValue;

        public string? KBElevationOuom

        {

            get { return this.KBElevationOuomValue; }

            set { SetProperty(ref KBElevationOuomValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Depth information
        private decimal? TotalDepthValue;

        public decimal? TotalDepth

        {

            get { return this.TotalDepthValue; }

            set { SetProperty(ref TotalDepthValue, value); }

        }
        private string? TotalDepthOuomValue;

        public string? TotalDepthOuom

        {

            get { return this.TotalDepthOuomValue; }

            set { SetProperty(ref TotalDepthOuomValue, value); }

        }
        private decimal? MeasuredDepthValue;

        public decimal? MeasuredDepth

        {

            get { return this.MeasuredDepthValue; }

            set { SetProperty(ref MeasuredDepthValue, value); }

        }
        private string? MeasuredDepthOuomValue;

        public string? MeasuredDepthOuom

        {

            get { return this.MeasuredDepthOuomValue; }

            set { SetProperty(ref MeasuredDepthOuomValue, value); }

        }
        private decimal? TrueVerticalDepthValue;

        public decimal? TrueVerticalDepth

        {

            get { return this.TrueVerticalDepthValue; }

            set { SetProperty(ref TrueVerticalDepthValue, value); }

        }
        private string? TrueVerticalDepthOuomValue;

        public string? TrueVerticalDepthOuom

        {

            get { return this.TrueVerticalDepthOuomValue; }

            set { SetProperty(ref TrueVerticalDepthOuomValue, value); }

        }
        
        // Results
        private string? WellResultValue;

        public string? WellResult

        {

            get { return this.WellResultValue; }

            set { SetProperty(ref WellResultValue, value); }

        }
        private string? DiscoveryIndicatorValue;

        public string? DiscoveryIndicator

        {

            get { return this.DiscoveryIndicatorValue; }

            set { SetProperty(ref DiscoveryIndicatorValue, value); }

        }
        private decimal? EstimatedOilVolumeValue;

        public decimal? EstimatedOilVolume

        {

            get { return this.EstimatedOilVolumeValue; }

            set { SetProperty(ref EstimatedOilVolumeValue, value); }

        }
        private decimal? EstimatedGasVolumeValue;

        public decimal? EstimatedGasVolume

        {

            get { return this.EstimatedGasVolumeValue; }

            set { SetProperty(ref EstimatedGasVolumeValue, value); }

        }
        
        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }
        
        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
    }

    #endregion

    #region Risk Assessment DTOs

    /// <summary>
    /// Request for prospect risk assessment calculation
    /// </summary>
    public class RiskAssessmentRequest : ModelEntityBase
    {
        private string? ProspectIdValue;

        public string? ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        
        // Risk parameters
        private string? RiskModelValue;

        public string? RiskModel

        {

            get { return this.RiskModelValue; }

            set { SetProperty(ref RiskModelValue, value); }

        } // e.g., "VOLUMETRIC", "MONTE_CARLO", "DETERMINISTIC"
        private decimal? TrapRiskValue;

        public decimal? TrapRisk

        {

            get { return this.TrapRiskValue; }

            set { SetProperty(ref TrapRiskValue, value); }

        } // Probability (0-1)
        private decimal? ReservoirRiskValue;

        public decimal? ReservoirRisk

        {

            get { return this.ReservoirRiskValue; }

            set { SetProperty(ref ReservoirRiskValue, value); }

        } // Probability (0-1)
        private decimal? SealRiskValue;

        public decimal? SealRisk

        {

            get { return this.SealRiskValue; }

            set { SetProperty(ref SealRiskValue, value); }

        } // Probability (0-1)
        private decimal? SourceRiskValue;

        public decimal? SourceRisk

        {

            get { return this.SourceRiskValue; }

            set { SetProperty(ref SourceRiskValue, value); }

        } // Probability (0-1)
        private decimal? TimingRiskValue;

        public decimal? TimingRisk

        {

            get { return this.TimingRiskValue; }

            set { SetProperty(ref TimingRiskValue, value); }

        } // Probability (0-1)
        
        // Volume estimates (for volumetric risk)
        private decimal? LowEstimateOilValue;

        public decimal? LowEstimateOil

        {

            get { return this.LowEstimateOilValue; }

            set { SetProperty(ref LowEstimateOilValue, value); }

        } // P90
        private decimal? BestEstimateOilValue;

        public decimal? BestEstimateOil

        {

            get { return this.BestEstimateOilValue; }

            set { SetProperty(ref BestEstimateOilValue, value); }

        } // P50
        private decimal? HighEstimateOilValue;

        public decimal? HighEstimateOil

        {

            get { return this.HighEstimateOilValue; }

            set { SetProperty(ref HighEstimateOilValue, value); }

        } // P10
        private decimal? LowEstimateGasValue;

        public decimal? LowEstimateGas

        {

            get { return this.LowEstimateGasValue; }

            set { SetProperty(ref LowEstimateGasValue, value); }

        } // P90
        private decimal? BestEstimateGasValue;

        public decimal? BestEstimateGas

        {

            get { return this.BestEstimateGasValue; }

            set { SetProperty(ref BestEstimateGasValue, value); }

        } // P50
        private decimal? HighEstimateGasValue;

        public decimal? HighEstimateGas

        {

            get { return this.HighEstimateGasValue; }

            set { SetProperty(ref HighEstimateGasValue, value); }

        } // P10
        
        // Economic parameters (optional)
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private decimal? DevelopmentCostValue;

        public decimal? DevelopmentCost

        {

            get { return this.DevelopmentCostValue; }

            set { SetProperty(ref DevelopmentCostValue, value); }

        }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of prospect risk assessment calculation
    /// </summary>
    public class RiskAssessmentResponse : ModelEntityBase
    {
        private string AssessmentIdValue = string.Empty;

        public string AssessmentId

        {

            get { return this.AssessmentIdValue; }

            set { SetProperty(ref AssessmentIdValue, value); }

        }
        private string? ProspectIdValue;

        public string? ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? RiskModelValue;

        public string? RiskModel

        {

            get { return this.RiskModelValue; }

            set { SetProperty(ref RiskModelValue, value); }

        }
        private DateTime AssessmentDateValue;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        
        // Risk probabilities
        private decimal? TrapRiskValue;

        public decimal? TrapRisk

        {

            get { return this.TrapRiskValue; }

            set { SetProperty(ref TrapRiskValue, value); }

        }
        private decimal? ReservoirRiskValue;

        public decimal? ReservoirRisk

        {

            get { return this.ReservoirRiskValue; }

            set { SetProperty(ref ReservoirRiskValue, value); }

        }
        private decimal? SealRiskValue;

        public decimal? SealRisk

        {

            get { return this.SealRiskValue; }

            set { SetProperty(ref SealRiskValue, value); }

        }
        private decimal? SourceRiskValue;

        public decimal? SourceRisk

        {

            get { return this.SourceRiskValue; }

            set { SetProperty(ref SourceRiskValue, value); }

        }
        private decimal? TimingRiskValue;

        public decimal? TimingRisk

        {

            get { return this.TimingRiskValue; }

            set { SetProperty(ref TimingRiskValue, value); }

        }
        private decimal? OverallGeologicalRiskValue;

        public decimal? OverallGeologicalRisk

        {

            get { return this.OverallGeologicalRiskValue; }

            set { SetProperty(ref OverallGeologicalRiskValue, value); }

        } // Product of all risks
        
        // Risked volumes (unrisked volume * overall risk)
        private decimal? RiskedOilVolumeValue;

        public decimal? RiskedOilVolume

        {

            get { return this.RiskedOilVolumeValue; }

            set { SetProperty(ref RiskedOilVolumeValue, value); }

        }
        private decimal? RiskedGasVolumeValue;

        public decimal? RiskedGasVolume

        {

            get { return this.RiskedGasVolumeValue; }

            set { SetProperty(ref RiskedGasVolumeValue, value); }

        }
        private decimal? UnriskedOilVolumeValue;

        public decimal? UnriskedOilVolume

        {

            get { return this.UnriskedOilVolumeValue; }

            set { SetProperty(ref UnriskedOilVolumeValue, value); }

        }
        private decimal? UnriskedGasVolumeValue;

        public decimal? UnriskedGasVolume

        {

            get { return this.UnriskedGasVolumeValue; }

            set { SetProperty(ref UnriskedGasVolumeValue, value); }

        }
        
        // Volume estimates
        private decimal? LowEstimateOilValue;

        public decimal? LowEstimateOil

        {

            get { return this.LowEstimateOilValue; }

            set { SetProperty(ref LowEstimateOilValue, value); }

        }
        private decimal? BestEstimateOilValue;

        public decimal? BestEstimateOil

        {

            get { return this.BestEstimateOilValue; }

            set { SetProperty(ref BestEstimateOilValue, value); }

        }
        private decimal? HighEstimateOilValue;

        public decimal? HighEstimateOil

        {

            get { return this.HighEstimateOilValue; }

            set { SetProperty(ref HighEstimateOilValue, value); }

        }
        private decimal? LowEstimateGasValue;

        public decimal? LowEstimateGas

        {

            get { return this.LowEstimateGasValue; }

            set { SetProperty(ref LowEstimateGasValue, value); }

        }
        private decimal? BestEstimateGasValue;

        public decimal? BestEstimateGas

        {

            get { return this.BestEstimateGasValue; }

            set { SetProperty(ref BestEstimateGasValue, value); }

        }
        private decimal? HighEstimateGasValue;

        public decimal? HighEstimateGas

        {

            get { return this.HighEstimateGasValue; }

            set { SetProperty(ref HighEstimateGasValue, value); }

        }
        
        // Economic assessment (if provided)
        private decimal? RiskedNPVValue;

        public decimal? RiskedNPV

        {

            get { return this.RiskedNPVValue; }

            set { SetProperty(ref RiskedNPVValue, value); }

        }
        private decimal? UnriskedNPVValue;

        public decimal? UnriskedNPV

        {

            get { return this.UnriskedNPVValue; }

            set { SetProperty(ref UnriskedNPVValue, value); }

        }
        private decimal? ExpectedMonetaryValueValue;

        public decimal? ExpectedMonetaryValue

        {

            get { return this.ExpectedMonetaryValueValue; }

            set { SetProperty(ref ExpectedMonetaryValueValue, value); }

        }
        
        // Risk classification
        private string? RiskCategoryValue;

        public string? RiskCategory

        {

            get { return this.RiskCategoryValue; }

            set { SetProperty(ref RiskCategoryValue, value); }

        } // e.g., "LOW", "MEDIUM", "HIGH", "VERY_HIGH"
        private List<string> RiskFactorsValue = new List<string>();

        public List<string> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        } // List of key risk factors
        private List<string> RecommendationsValue = new List<string>();

        public List<string> Recommendations

        {

            get { return this.RecommendationsValue; }

            set { SetProperty(ref RecommendationsValue, value); }

        } // Risk mitigation recommendations
        
        // Additional metadata
        public Dictionary<string, object>? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    #endregion
}


