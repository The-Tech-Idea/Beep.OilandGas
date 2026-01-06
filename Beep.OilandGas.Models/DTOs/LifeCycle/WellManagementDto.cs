using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.LifeCycle
{
    /// <summary>
    /// DTOs for Well Management operations
    /// </summary>
    
    public class WellCreationRequest
    {
        public string WellName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string? WellType { get; set; }
        public string? WellPurpose { get; set; }
        public decimal? SurfaceLatitude { get; set; }
        public decimal? SurfaceLongitude { get; set; }
        public decimal? BottomHoleLatitude { get; set; }
        public decimal? BottomHoleLongitude { get; set; }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class WellPlanningRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string PlanningType { get; set; } = string.Empty; // DRILLING, COMPLETION, WORKOVER, etc.
        public string? Description { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetEndDate { get; set; }
        public Dictionary<string, object>? PlanningData { get; set; }
    }

    public class WellOperationsRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class WellMaintenanceRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty; // PREVENTIVE, CORRECTIVE, EMERGENCY
        public string? Description { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? MaintenanceData { get; set; }
    }

    public class WellInspectionRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string InspectionType { get; set; } = string.Empty; // REGULAR, COMPLIANCE, SAFETY, INTEGRITY
        public DateTime InspectionDate { get; set; }
        public string? Inspector { get; set; }
        public string? Findings { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? InspectionData { get; set; }
    }

    public class WellEquipmentRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string EquipmentType { get; set; } = string.Empty;
        public string? EquipmentName { get; set; }
        public string? Manufacturer { get; set; }
        public string? Model { get; set; }
        public DateTime? InstallationDate { get; set; }
        public Dictionary<string, object>? EquipmentData { get; set; }
    }

    public class WellResponse
    {
        public string WellId { get; set; } = string.Empty;
        public string WellName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string? CurrentState { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class WellPerformanceResponse
    {
        public string WellId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public Dictionary<string, decimal> Metrics { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    // Workover-specific response DTOs
    public class WellWorkoverResponse
    {
        public string WellId { get; set; } = string.Empty;
        public string WorkOrderId { get; set; } = string.Empty;
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string WorkoverType { get; set; } = string.Empty; // RIG_WORKOVER, RIGLESS_WORKOVER, etc.
        public string? Status { get; set; }
        public string? AfeId { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public Dictionary<string, object>? WorkoverData { get; set; }
    }
}




