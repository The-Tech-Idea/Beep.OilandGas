using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class FieldResponse : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

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
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }
        
        // Dates
        private DateTime? DiscoveryDateValue;

        public DateTime? DiscoveryDate

        {

            get { return this.DiscoveryDateValue; }

            set { SetProperty(ref DiscoveryDateValue, value); }

        }
        private DateTime? FirstProductionDateValue;

        public DateTime? FirstProductionDate

        {

            get { return this.FirstProductionDateValue; }

            set { SetProperty(ref FirstProductionDateValue, value); }

        }
        
        // Classification
        private string? FieldTypeValue;

        public string? FieldType

        {

            get { return this.FieldTypeValue; }

            set { SetProperty(ref FieldTypeValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        
        // Area information
        private decimal? AreaValue;

        public decimal? Area

        {

            get { return this.AreaValue; }

            set { SetProperty(ref AreaValue, value); }

        }
        private string? AreaOuomValue;

        public string? AreaOuom

        {

            get { return this.AreaOuomValue; }

            set { SetProperty(ref AreaOuomValue, value); }

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
        private object FieldValue;

        public object Field

        {

            get { return this.FieldValue; }

            set { SetProperty(ref FieldValue, value); }

        }

        public string CurrentPhase { get; set; }
        public string AreaId { get; set; }
    }
}
