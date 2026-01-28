using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class FacilityCreationRequest : ModelEntityBase
    {
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? FacilityTypeValue;

        public string? FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        }
        private string? FacilityPurposeValue;

        public string? FacilityPurpose

        {

            get { return this.FacilityPurposeValue; }

            set { SetProperty(ref FacilityPurposeValue, value); }

        }
        private decimal? CapacityValue;

        public decimal? Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }
}
