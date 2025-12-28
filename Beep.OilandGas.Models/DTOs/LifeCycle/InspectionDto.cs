using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.LifeCycle
{
    /// <summary>
    /// DTOs for Inspection Management
    /// </summary>
    
    public class InspectionScheduleRequest
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty; // WELL, FACILITY, PIPELINE, EQUIPMENT
        public string InspectionType { get; set; } = string.Empty; // REGULAR, COMPLIANCE, SAFETY, INTEGRITY
        public DateTime ScheduledDate { get; set; }
        public string? Inspector { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? ScheduleData { get; set; }
    }

    public class InspectionExecutionRequest
    {
        public string InspectionId { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public string? Inspector { get; set; }
        public string? Findings { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? InspectionData { get; set; }
    }

    public class InspectionFindingRequest
    {
        public string InspectionId { get; set; } = string.Empty;
        public string FindingType { get; set; } = string.Empty; // DEFICIENCY, NON_COMPLIANCE, SAFETY_HAZARD, INTEGRITY_ISSUE
        public string Severity { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, CRITICAL
        public string? Description { get; set; }
        public string? RecommendedAction { get; set; }
        public Dictionary<string, object>? FindingData { get; set; }
    }

    public class InspectionResponse
    {
        public string InspectionId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string InspectionType { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }
        public string? Inspector { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? InspectionData { get; set; }
    }
}

