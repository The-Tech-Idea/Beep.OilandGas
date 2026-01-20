using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Pipeline Management operations
    /// </summary>
    
    public class PipelineCreationRequest : ModelEntityBase
    {
        public string PipelineName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string? PipelineType { get; set; }
        public decimal? Diameter { get; set; }
        public decimal? Length { get; set; }
        public string? Material { get; set; }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class PipelineOperationsRequest : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public decimal? FlowRate { get; set; }
        public decimal? Pressure { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class PipelineMaintenanceRequest : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty; // PREVENTIVE, CORRECTIVE, EMERGENCY
        public string? Description { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? MaintenanceData { get; set; }
    }

    public class PipelineInspectionRequest : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public string InspectionType { get; set; } = string.Empty; // REGULAR, COMPLIANCE, SAFETY, INTEGRITY
        public DateTime InspectionDate { get; set; }
        public string? Inspector { get; set; }
        public string? Findings { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? InspectionData { get; set; }
    }

    public class PipelineIntegrityRequest : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public string? AssessmentResult { get; set; }
        public Dictionary<string, object>? AssessmentData { get; set; }
    }

    public class PipelineFlowRequest : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal FlowRate { get; set; }
        public decimal Pressure { get; set; }
        public string? ProductType { get; set; }
        public DateTime FlowDate { get; set; }
    }

    public class PipelineResponse : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public string PipelineName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string? Status { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class PipelineCapacityResponse : ModelEntityBase
    {
        public string PipelineId { get; set; } = string.Empty;
        public decimal MaximumCapacity { get; set; }
        public decimal CurrentUtilization { get; set; }
        public decimal AvailableCapacity { get; set; }
        public DateTime ReportDate { get; set; }
    }
}





