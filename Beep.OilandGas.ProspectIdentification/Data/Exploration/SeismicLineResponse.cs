using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
