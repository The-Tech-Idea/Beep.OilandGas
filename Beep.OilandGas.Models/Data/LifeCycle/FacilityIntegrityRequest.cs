using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class FacilityIntegrityRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string AssessmentTypeValue = string.Empty;

        public string AssessmentType

        {

            get { return this.AssessmentTypeValue; }

            set { SetProperty(ref AssessmentTypeValue, value); }

        }
        private DateTime AssessmentDateValue;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        private string? AssessmentResultValue;

        public string? AssessmentResult

        {

            get { return this.AssessmentResultValue; }

            set { SetProperty(ref AssessmentResultValue, value); }

        }
        public Dictionary<string, object>? AssessmentData { get; set; }
    }
}
