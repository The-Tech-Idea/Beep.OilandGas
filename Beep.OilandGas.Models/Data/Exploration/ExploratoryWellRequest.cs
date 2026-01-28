using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
