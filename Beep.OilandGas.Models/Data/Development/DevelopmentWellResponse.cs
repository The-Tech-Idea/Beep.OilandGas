using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class DevelopmentWellResponse : ModelEntityBase
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
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

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
        private string? CompletionTypeValue;

        public string? CompletionType

        {

            get { return this.CompletionTypeValue; }

            set { SetProperty(ref CompletionTypeValue, value); }

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
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

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
        
        // Completion information
        private decimal? CompletionTopDepthValue;

        public decimal? CompletionTopDepth

        {

            get { return this.CompletionTopDepthValue; }

            set { SetProperty(ref CompletionTopDepthValue, value); }

        }
        private string? CompletionTopDepthOuomValue;

        public string? CompletionTopDepthOuom

        {

            get { return this.CompletionTopDepthOuomValue; }

            set { SetProperty(ref CompletionTopDepthOuomValue, value); }

        }
        private decimal? CompletionBaseDepthValue;

        public decimal? CompletionBaseDepth

        {

            get { return this.CompletionBaseDepthValue; }

            set { SetProperty(ref CompletionBaseDepthValue, value); }

        }
        private string? CompletionBaseDepthOuomValue;

        public string? CompletionBaseDepthOuom

        {

            get { return this.CompletionBaseDepthOuomValue; }

            set { SetProperty(ref CompletionBaseDepthOuomValue, value); }

        }
        private decimal? PerforationTopDepthValue;

        public decimal? PerforationTopDepth

        {

            get { return this.PerforationTopDepthValue; }

            set { SetProperty(ref PerforationTopDepthValue, value); }

        }
        private string? PerforationTopDepthOuomValue;

        public string? PerforationTopDepthOuom

        {

            get { return this.PerforationTopDepthOuomValue; }

            set { SetProperty(ref PerforationTopDepthOuomValue, value); }

        }
        private decimal? PerforationBaseDepthValue;

        public decimal? PerforationBaseDepth

        {

            get { return this.PerforationBaseDepthValue; }

            set { SetProperty(ref PerforationBaseDepthValue, value); }

        }
        private string? PerforationBaseDepthOuomValue;

        public string? PerforationBaseDepthOuom

        {

            get { return this.PerforationBaseDepthOuomValue; }

            set { SetProperty(ref PerforationBaseDepthOuomValue, value); }

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
}
