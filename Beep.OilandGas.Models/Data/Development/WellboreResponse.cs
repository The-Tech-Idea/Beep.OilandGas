using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellboreResponse : ModelEntityBase
    {
        private string WellboreIdValue = string.Empty;

        public string WellboreId

        {

            get { return this.WellboreIdValue; }

            set { SetProperty(ref WellboreIdValue, value); }

        }
        private string WellboreNameValue = string.Empty;

        public string WellboreName

        {

            get { return this.WellboreNameValue; }

            set { SetProperty(ref WellboreNameValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        
        // Wellbore classification
        private string? WellboreTypeValue;

        public string? WellboreType

        {

            get { return this.WellboreTypeValue; }

            set { SetProperty(ref WellboreTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? TrajectoryTypeValue;

        public string? TrajectoryType

        {

            get { return this.TrajectoryTypeValue; }

            set { SetProperty(ref TrajectoryTypeValue, value); }

        }
        
        // Depth information
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
        private decimal? KickoffDepthValue;

        public decimal? KickoffDepth

        {

            get { return this.KickoffDepthValue; }

            set { SetProperty(ref KickoffDepthValue, value); }

        }
        private string? KickoffDepthOuomValue;

        public string? KickoffDepthOuom

        {

            get { return this.KickoffDepthOuomValue; }

            set { SetProperty(ref KickoffDepthOuomValue, value); }

        }
        
        // Geometry
        private decimal? HoleDiameterValue;

        public decimal? HoleDiameter

        {

            get { return this.HoleDiameterValue; }

            set { SetProperty(ref HoleDiameterValue, value); }

        }
        private string? HoleDiameterOuomValue;

        public string? HoleDiameterOuom

        {

            get { return this.HoleDiameterOuomValue; }

            set { SetProperty(ref HoleDiameterOuomValue, value); }

        }
        private decimal? CasingDiameterValue;

        public decimal? CasingDiameter

        {

            get { return this.CasingDiameterValue; }

            set { SetProperty(ref CasingDiameterValue, value); }

        }
        private string? CasingDiameterOuomValue;

        public string? CasingDiameterOuom

        {

            get { return this.CasingDiameterOuomValue; }

            set { SetProperty(ref CasingDiameterOuomValue, value); }

        }
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        }
        private string? TubingDiameterOuomValue;

        public string? TubingDiameterOuom

        {

            get { return this.TubingDiameterOuomValue; }

            set { SetProperty(ref TubingDiameterOuomValue, value); }

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
        
        // Dates
        private DateTime? DrillingStartDateValue;

        public DateTime? DrillingStartDate

        {

            get { return this.DrillingStartDateValue; }

            set { SetProperty(ref DrillingStartDateValue, value); }

        }
        private DateTime? DrillingEndDateValue;

        public DateTime? DrillingEndDate

        {

            get { return this.DrillingEndDateValue; }

            set { SetProperty(ref DrillingEndDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

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
