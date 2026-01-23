using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Operations Management
    /// </summary>
    
    public class DailyOperationsRequest : ModelEntityBase
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

        } // FIELD, WELL, FACILITY, PIPELINE
        private DateTime OperationDateValue;

        public DateTime OperationDate

        {

            get { return this.OperationDateValue; }

            set { SetProperty(ref OperationDateValue, value); }

        }
        private string? ShiftValue;

        public string? Shift

        {

            get { return this.ShiftValue; }

            set { SetProperty(ref ShiftValue, value); }

        }
        private string? OperatorValue;

        public string? Operator

        {

            get { return this.OperatorValue; }

            set { SetProperty(ref OperatorValue, value); }

        }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class ShiftHandoverRequest : ModelEntityBase
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

        }
        private DateTime HandoverDateValue;

        public DateTime HandoverDate

        {

            get { return this.HandoverDateValue; }

            set { SetProperty(ref HandoverDateValue, value); }

        }
        private string FromShiftValue = string.Empty;

        public string FromShift

        {

            get { return this.FromShiftValue; }

            set { SetProperty(ref FromShiftValue, value); }

        }
        private string ToShiftValue = string.Empty;

        public string ToShift

        {

            get { return this.ToShiftValue; }

            set { SetProperty(ref ToShiftValue, value); }

        }
        private string? HandoverNotesValue;

        public string? HandoverNotes

        {

            get { return this.HandoverNotesValue; }

            set { SetProperty(ref HandoverNotesValue, value); }

        }
        public Dictionary<string, object>? HandoverData { get; set; }
    }

    public class IncidentRequest : ModelEntityBase
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

        }
        private DateTime IncidentDateValue;

        public DateTime IncidentDate

        {

            get { return this.IncidentDateValue; }

            set { SetProperty(ref IncidentDateValue, value); }

        }
        private string IncidentTypeValue = string.Empty;

        public string IncidentType

        {

            get { return this.IncidentTypeValue; }

            set { SetProperty(ref IncidentTypeValue, value); }

        } // SAFETY, ENVIRONMENTAL, OPERATIONAL, EQUIPMENT
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
        private string? ReportedByValue;

        public string? ReportedBy

        {

            get { return this.ReportedByValue; }

            set { SetProperty(ref ReportedByValue, value); }

        }
        public Dictionary<string, object>? IncidentData { get; set; }
    }

    public class SafetyAssessmentRequest : ModelEntityBase
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

        }
        private DateTime AssessmentDateValue;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        private string? AssessorValue;

        public string? Assessor

        {

            get { return this.AssessorValue; }

            set { SetProperty(ref AssessorValue, value); }

        }
        private string? FindingsValue;

        public string? Findings

        {

            get { return this.FindingsValue; }

            set { SetProperty(ref FindingsValue, value); }

        }
        public Dictionary<string, object>? AssessmentData { get; set; }
    }

    public class ComplianceRequest : ModelEntityBase
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

        }
        private string ComplianceTypeValue = string.Empty;

        public string ComplianceType

        {

            get { return this.ComplianceTypeValue; }

            set { SetProperty(ref ComplianceTypeValue, value); }

        }
        private DateTime ComplianceDateValue;

        public DateTime ComplianceDate

        {

            get { return this.ComplianceDateValue; }

            set { SetProperty(ref ComplianceDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? ComplianceData { get; set; }
    }

    public class OperationsResponse : ModelEntityBase
    {
        private string OperationIdValue = string.Empty;

        public string OperationId

        {

            get { return this.OperationIdValue; }

            set { SetProperty(ref OperationIdValue, value); }

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
        private DateTime OperationDateValue;

        public DateTime OperationDate

        {

            get { return this.OperationDateValue; }

            set { SetProperty(ref OperationDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? OperationData { get; set; }
    }
}








