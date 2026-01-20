using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Operations Management
    /// </summary>
    
    public class DailyOperationsRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty; // FIELD, WELL, FACILITY, PIPELINE
        public DateTime OperationDate { get; set; }
        public string? Shift { get; set; }
        public string? Operator { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class ShiftHandoverRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime HandoverDate { get; set; }
        public string FromShift { get; set; } = string.Empty;
        public string ToShift { get; set; } = string.Empty;
        public string? HandoverNotes { get; set; }
        public Dictionary<string, object>? HandoverData { get; set; }
    }

    public class IncidentRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime IncidentDate { get; set; }
        public string IncidentType { get; set; } = string.Empty; // SAFETY, ENVIRONMENTAL, OPERATIONAL, EQUIPMENT
        public string Severity { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
        public string? Description { get; set; }
        public string? ReportedBy { get; set; }
        public Dictionary<string, object>? IncidentData { get; set; }
    }

    public class SafetyAssessmentRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public string? Assessor { get; set; }
        public string? Findings { get; set; }
        public Dictionary<string, object>? AssessmentData { get; set; }
    }

    public class ComplianceRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string ComplianceType { get; set; } = string.Empty;
        public DateTime ComplianceDate { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? ComplianceData { get; set; }
    }

    public class OperationsResponse : ModelEntityBase
    {
        public string OperationId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }
}





