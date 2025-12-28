using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.LifeCycle
{
    /// <summary>
    /// DTOs for Work Order Management operations
    /// </summary>
    
    public class WorkOrderCreationRequest
    {
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string WorkOrderType { get; set; } = string.Empty; // RIG_WORKOVER, RIGLESS_WORKOVER, WIRELINE_WORK, etc.
        public string EntityType { get; set; } = string.Empty; // WELL, FACILITY, PIPELINE
        public string EntityId { get; set; } = string.Empty; // WELL_ID, FACILITY_ID, or PIPELINE_ID
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string? Instructions { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class WorkOrderUpdateRequest
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string? Status { get; set; }
        public string? Instructions { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class WorkOrderCostRequest
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string CostType { get; set; } = string.Empty; // WORKOVER, MAINTENANCE, REPAIR, etc.
        public string CostCategory { get; set; } = string.Empty; // LABOR, MATERIALS, EQUIPMENT, etc.
        public decimal Amount { get; set; }
        public bool IsCapitalized { get; set; }
        public bool IsExpensed { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string? Description { get; set; }
    }

    public class WorkOrderResponse
    {
        public string WorkOrderId { get; set; } = string.Empty;
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string WorkOrderType { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string? Status { get; set; }
        public string? AfeId { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompleteDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? ActualCost { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }

    // Well-specific workover DTOs
    public class RigWorkoverRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? WorkoverData { get; set; }
    }

    public class RiglessWorkoverRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string WorkoverSubType { get; set; } = string.Empty; // WIRELINE, COILED_TUBING, SNUBBING, TESTING, STIMULATION, CLEANOUT
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? WorkoverData { get; set; }
    }

    public class WirelineWorkRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string WirelineOperation { get; set; } = string.Empty; // LOGGING, PERFORATING, FISHING, PLUG_SETTING
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class CoiledTubingWorkRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty; // CLEANOUT, STIMULATION, LOGGING, FISHING
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class WellTestRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string TestType { get; set; } = string.Empty; // PRODUCTION_TEST, PRESSURE_TEST, FLOW_TEST
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? TestData { get; set; }
    }

    public class StimulationRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string StimulationType { get; set; } = string.Empty; // FRACTURING, ACIDIZING, MATRIX_STIMULATION
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? StimulationData { get; set; }
    }

    public class CleanoutRequest
    {
        public string WellId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string CleanoutType { get; set; } = string.Empty; // SAND_CLEANOUT, SCALE_REMOVAL, DEBRIS_REMOVAL
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? CleanoutData { get; set; }
    }

    // Facility-specific work order DTOs
    public class FacilityWorkOrderRequest
    {
        public string FacilityId { get; set; } = string.Empty;
        public string? FieldId { get; set; }
        public string? PropertyId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string WorkOrderType { get; set; } = string.Empty; // MAINTENANCE, REPAIR, UPGRADE, INSPECTION, EQUIPMENT_INSTALLATION, EQUIPMENT_REMOVAL, MODIFICATION
        public string? Description { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Instructions { get; set; }
        public Dictionary<string, object>? WorkOrderData { get; set; }
    }
}

