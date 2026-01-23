using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Inspection Management
    /// </summary>
    
    public class InspectionScheduleRequest : ModelEntityBase
    {
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        } // WELL, FACILITY, PIPELINE, EQUIPMENT
        private string InspectionTypeValue = string.Empty;

        public string InspectionType

        {

            get { return this.InspectionTypeValue; }

            set { SetProperty(ref InspectionTypeValue, value); }

        } // REGULAR, COMPLIANCE, SAFETY, INTEGRITY
        private DateTime ScheduledDateValue;

        public DateTime ScheduledDate

        {

            get { return this.ScheduledDateValue; }

            set { SetProperty(ref ScheduledDateValue, value); }

        }
        private string? InspectorValue;

        public string? Inspector

        {

            get { return this.InspectorValue; }

            set { SetProperty(ref InspectorValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        public Dictionary<string, object>? ScheduleData { get; set; }
    }

    public class InspectionExecutionRequest : ModelEntityBase
    {
        private string InspectionIdValue = string.Empty;

        public string InspectionId

        {

            get { return this.InspectionIdValue; }

            set { SetProperty(ref InspectionIdValue, value); }

        }
        private DateTime ExecutionDateValue;

        public DateTime ExecutionDate

        {

            get { return this.ExecutionDateValue; }

            set { SetProperty(ref ExecutionDateValue, value); }

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

    public class InspectionResponse : ModelEntityBase
    {
        private string InspectionIdValue = string.Empty;

        public string InspectionId

        {

            get { return this.InspectionIdValue; }

            set { SetProperty(ref InspectionIdValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string InspectionTypeValue = string.Empty;

        public string InspectionType

        {

            get { return this.InspectionTypeValue; }

            set { SetProperty(ref InspectionTypeValue, value); }

        }
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
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? InspectionData { get; set; }
    }
}








