using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.LifeCycle
{
    /// <summary>
    /// DTOs for Facility Management operations
    /// </summary>
    
    public class FacilityCreationRequest
    {
        public string FacilityName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string? FacilityType { get; set; }
        public string? FacilityPurpose { get; set; }
        public decimal? Capacity { get; set; }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class FacilityOperationsRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class FacilityMaintenanceRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty; // PREVENTIVE, CORRECTIVE, EMERGENCY
        public string? Description { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? MaintenanceData { get; set; }
    }

    public class FacilityInspectionRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string InspectionType { get; set; } = string.Empty; // REGULAR, COMPLIANCE, SAFETY, INTEGRITY
        public DateTime InspectionDate { get; set; }
        public string? Inspector { get; set; }
        public string? Findings { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? InspectionData { get; set; }
    }

    public class FacilityIntegrityRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string AssessmentType { get; set; } = string.Empty;
        public DateTime AssessmentDate { get; set; }
        public string? AssessmentResult { get; set; }
        public Dictionary<string, object>? AssessmentData { get; set; }
    }

    public class FacilityEquipmentRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string? EquipmentName { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public DateTime? InstallationDate { get; set; }
        public Dictionary<string, object>? EquipmentData { get; set; }
    }

    public class FacilityResponse
    {
        public string FacilityId { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string? Status { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class FacilityPerformanceResponse
    {
        public string FacilityId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public Dictionary<string, decimal> Metrics { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    // Work order-specific response DTOs
    public class FacilityWorkOrderResponse
    {
        public string FacilityId { get; set; } = string.Empty;
        public string WorkOrderId { get; set; } = string.Empty;
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string WorkOrderType { get; set; } = string.Empty; // MAINTENANCE, REPAIR, UPGRADE, etc.
        public string? Status { get; set; }
        public string? AfeId { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public Dictionary<string, object>? WorkOrderData { get; set; }
    }
}

