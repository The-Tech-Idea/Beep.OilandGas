using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Maintenance Management
    /// </summary>
    
    public class MaintenanceScheduleRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty; // WELL, FACILITY, PIPELINE, EQUIPMENT
        public string MaintenanceType { get; set; } = string.Empty; // PREVENTIVE, CORRECTIVE, EMERGENCY
        public DateTime ScheduledDate { get; set; }
        public string? Description { get; set; }
        public string? AssignedTo { get; set; }
        public bool? CreateWorkOrder { get; set; } // Whether to create a work order for this maintenance
        public Dictionary<string, object>? ScheduleData { get; set; }
    }

    public class MaintenanceExecutionRequest : ModelEntityBase
    {
        public string MaintenanceId { get; set; } = string.Empty;
        public DateTime ExecutionDate { get; set; }
        public string? ExecutedBy { get; set; }
        public string? WorkPerformed { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? ExecutionData { get; set; }
    }

    public class MaintenanceRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty; // LOW, MEDIUM, HIGH, URGENT
        public string? Description { get; set; }
        public string? RequestedBy { get; set; }
        public Dictionary<string, object>? RequestData { get; set; }
    }

    public class MaintenanceHistoryRequest : ModelEntityBase
    {
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class MaintenanceResponse : ModelEntityBase
    {
        public string MaintenanceId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string MaintenanceType { get; set; } = string.Empty;
        public DateTime? ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? Status { get; set; }
        public string? WorkOrderId { get; set; } // Associated work order ID if created
        public Dictionary<string, object>? MaintenanceData { get; set; }
    }
}





