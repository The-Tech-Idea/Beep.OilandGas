using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
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
}
