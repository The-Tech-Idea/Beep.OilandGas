using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class FacilityInspectionRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string InspectionTypeValue = string.Empty;

        public string InspectionType

        {

            get { return this.InspectionTypeValue; }

            set { SetProperty(ref InspectionTypeValue, value); }

        } // REGULAR, COMPLIANCE, SAFETY, INTEGRITY
        private DateTime InspectionDateValue;

        public DateTime InspectionDate

        {

            get { return this.InspectionDateValue; }

            set { SetProperty(ref InspectionDateValue, value); }

        }
        private string? InspectorValue;

        public string? Inspector

        {

            get { return this.InspectorValue; }

            set { SetProperty(ref InspectorValue, value); }

        }
        private string? FindingsValue;

        public string? Findings

        {

            get { return this.FindingsValue; }

            set { SetProperty(ref FindingsValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? InspectionData { get; set; }
    }
}
