using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class InspectionFindingRequest : ModelEntityBase
    {
        private string InspectionIdValue = string.Empty;

        public string InspectionId

        {

            get { return this.InspectionIdValue; }

            set { SetProperty(ref InspectionIdValue, value); }

        }
        private string FindingTypeValue = string.Empty;

        public string FindingType

        {

            get { return this.FindingTypeValue; }

            set { SetProperty(ref FindingTypeValue, value); }

        } // DEFICIENCY, NON_COMPLIANCE, SAFETY_HAZARD, INTEGRITY_ISSUE
        private string SeverityValue = string.Empty;

        public string Severity

        {

            get { return this.SeverityValue; }

            set { SetProperty(ref SeverityValue, value); }

        } // LOW, MEDIUM, HIGH, CRITICAL
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? RecommendedActionValue;

        public string? RecommendedAction

        {

            get { return this.RecommendedActionValue; }

            set { SetProperty(ref RecommendedActionValue, value); }

        }
        public Dictionary<string, object>? FindingData { get; set; }
    }
}
