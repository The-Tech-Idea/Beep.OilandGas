using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Work Order Management operations
    /// </summary>
    
    public class WorkOrderCreationRequest : ModelEntityBase
    {
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string WorkOrderTypeValue = string.Empty;

        public string WorkOrderType

        {

            get { return this.WorkOrderTypeValue; }

            set { SetProperty(ref WorkOrderTypeValue, value); }

        } // RIG_WORKOVER, RIGLESS_WORKOVER, WIRELINE_WORK, etc.
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        } // WELL, FACILITY, PIPELINE
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        } // WELL_ID, FACILITY_ID, or PIPELINE_ID
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class WorkOrderUpdateRequest : ModelEntityBase
    {
        private string WorkOrderIdValue = string.Empty;

        public string WorkOrderId

        {

            get { return this.WorkOrderIdValue; }

            set { SetProperty(ref WorkOrderIdValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private DateTime? CompleteDateValue;

        public DateTime? CompleteDate

        {

            get { return this.CompleteDateValue; }

            set { SetProperty(ref CompleteDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class WorkOrderCostRequest : ModelEntityBase
    {
        private string WorkOrderIdValue = string.Empty;

        public string WorkOrderId

        {

            get { return this.WorkOrderIdValue; }

            set { SetProperty(ref WorkOrderIdValue, value); }

        }
        private string CostTypeValue = string.Empty;

        public string CostType

        {

            get { return this.CostTypeValue; }

            set { SetProperty(ref CostTypeValue, value); }

        } // WORKOVER, MAINTENANCE, REPAIR, etc.
        private string CostCategoryValue = string.Empty;

        public string CostCategory

        {

            get { return this.CostCategoryValue; }

            set { SetProperty(ref CostCategoryValue, value); }

        } // LABOR, MATERIALS, EQUIPMENT, etc.
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private bool IsCapitalizedValue;

        public bool IsCapitalized

        {

            get { return this.IsCapitalizedValue; }

            set { SetProperty(ref IsCapitalizedValue, value); }

        }
        private bool IsExpensedValue;

        public bool IsExpensed

        {

            get { return this.IsExpensedValue; }

            set { SetProperty(ref IsExpensedValue, value); }

        }
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class WorkOrderResponse : ModelEntityBase
    {
        private string WorkOrderIdValue = string.Empty;

        public string WorkOrderId

        {

            get { return this.WorkOrderIdValue; }

            set { SetProperty(ref WorkOrderIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string WorkOrderTypeValue = string.Empty;

        public string WorkOrderType

        {

            get { return this.WorkOrderTypeValue; }

            set { SetProperty(ref WorkOrderTypeValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? AfeIdValue;

        public string? AfeId

        {

            get { return this.AfeIdValue; }

            set { SetProperty(ref AfeIdValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private DateTime? CompleteDateValue;

        public DateTime? CompleteDate

        {

            get { return this.CompleteDateValue; }

            set { SetProperty(ref CompleteDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private decimal? ActualCostValue;

        public decimal? ActualCost

        {

            get { return this.ActualCostValue; }

            set { SetProperty(ref ActualCostValue, value); }

        }
        public Dictionary<string, object>? Properties { get; set; }
    }

    // Well-specific workover DTOs
    public class RigWorkoverRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? WorkoverData { get; set; }
    }

    public class RiglessWorkoverRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string WorkoverSubTypeValue = string.Empty;

        public string WorkoverSubType

        {

            get { return this.WorkoverSubTypeValue; }

            set { SetProperty(ref WorkoverSubTypeValue, value); }

        } // WIRELINE, COILED_TUBING, SNUBBING, TESTING, STIMULATION, CLEANOUT
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? WorkoverData { get; set; }
    }

    public class WirelineWorkRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string WirelineOperationValue = string.Empty;

        public string WirelineOperation

        {

            get { return this.WirelineOperationValue; }

            set { SetProperty(ref WirelineOperationValue, value); }

        } // LOGGING, PERFORATING, FISHING, PLUG_SETTING
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class CoiledTubingWorkRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string OperationTypeValue = string.Empty;

        public string OperationType

        {

            get { return this.OperationTypeValue; }

            set { SetProperty(ref OperationTypeValue, value); }

        } // CLEANOUT, STIMULATION, LOGGING, FISHING
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class WellTestRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string TestTypeValue = string.Empty;

        public string TestType

        {

            get { return this.TestTypeValue; }

            set { SetProperty(ref TestTypeValue, value); }

        } // PRODUCTION_TEST, PRESSURE_TEST, FLOW_TEST
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? TestData { get; set; }
    }

    public class StimulationRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string StimulationTypeValue = string.Empty;

        public string StimulationType

        {

            get { return this.StimulationTypeValue; }

            set { SetProperty(ref StimulationTypeValue, value); }

        } // FRACTURING, ACIDIZING, MATRIX_STIMULATION
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? StimulationData { get; set; }
    }

    public class CleanoutRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string CleanoutTypeValue = string.Empty;

        public string CleanoutType

        {

            get { return this.CleanoutTypeValue; }

            set { SetProperty(ref CleanoutTypeValue, value); }

        } // SAND_CLEANOUT, SCALE_REMOVAL, DEBRIS_REMOVAL
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? CleanoutData { get; set; }
    }

    // Facility-specific work order DTOs
    public class FacilityWorkOrderRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WorkOrderNumberValue = string.Empty;

        public string WorkOrderNumber

        {

            get { return this.WorkOrderNumberValue; }

            set { SetProperty(ref WorkOrderNumberValue, value); }

        }
        private string WorkOrderTypeValue = string.Empty;

        public string WorkOrderType

        {

            get { return this.WorkOrderTypeValue; }

            set { SetProperty(ref WorkOrderTypeValue, value); }

        } // MAINTENANCE, REPAIR, UPGRADE, INSPECTION, EQUIPMENT_INSTALLATION, EQUIPMENT_REMOVAL, MODIFICATION
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? RequestDateValue;

        public DateTime? RequestDate

        {

            get { return this.RequestDateValue; }

            set { SetProperty(ref RequestDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? InstructionsValue;

        public string? Instructions

        {

            get { return this.InstructionsValue; }

            set { SetProperty(ref InstructionsValue, value); }

        }
        public Dictionary<string, object>? WorkOrderData { get; set; }
    }
}








