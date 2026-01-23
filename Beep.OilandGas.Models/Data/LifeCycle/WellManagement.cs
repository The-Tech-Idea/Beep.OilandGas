using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Well Management operations
    /// </summary>
    
    public class WellCreationRequest : ModelEntityBase
    {
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? WellTypeValue;

        public string? WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        }
        private string? WellPurposeValue;

        public string? WellPurpose

        {

            get { return this.WellPurposeValue; }

            set { SetProperty(ref WellPurposeValue, value); }

        }
        private decimal? SurfaceLatitudeValue;

        public decimal? SurfaceLatitude

        {

            get { return this.SurfaceLatitudeValue; }

            set { SetProperty(ref SurfaceLatitudeValue, value); }

        }
        private decimal? SurfaceLongitudeValue;

        public decimal? SurfaceLongitude

        {

            get { return this.SurfaceLongitudeValue; }

            set { SetProperty(ref SurfaceLongitudeValue, value); }

        }
        private decimal? BottomHoleLatitudeValue;

        public decimal? BottomHoleLatitude

        {

            get { return this.BottomHoleLatitudeValue; }

            set { SetProperty(ref BottomHoleLatitudeValue, value); }

        }
        private decimal? BottomHoleLongitudeValue;

        public decimal? BottomHoleLongitude

        {

            get { return this.BottomHoleLongitudeValue; }

            set { SetProperty(ref BottomHoleLongitudeValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class WellPlanningRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string PlanningTypeValue = string.Empty;

        public string PlanningType

        {

            get { return this.PlanningTypeValue; }

            set { SetProperty(ref PlanningTypeValue, value); }

        } // DRILLING, COMPLETION, WORKOVER, etc.
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? TargetStartDateValue;

        public DateTime? TargetStartDate

        {

            get { return this.TargetStartDateValue; }

            set { SetProperty(ref TargetStartDateValue, value); }

        }
        private DateTime? TargetEndDateValue;

        public DateTime? TargetEndDate

        {

            get { return this.TargetEndDateValue; }

            set { SetProperty(ref TargetEndDateValue, value); }

        }
        public Dictionary<string, object>? PlanningData { get; set; }
    }

    public class WellOperationsRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string OperationTypeValue = string.Empty;

        public string OperationType

        {

            get { return this.OperationTypeValue; }

            set { SetProperty(ref OperationTypeValue, value); }

        }
        private DateTime OperationDateValue;

        public DateTime OperationDate

        {

            get { return this.OperationDateValue; }

            set { SetProperty(ref OperationDateValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class WellMaintenanceRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string MaintenanceTypeValue = string.Empty;

        public string MaintenanceType

        {

            get { return this.MaintenanceTypeValue; }

            set { SetProperty(ref MaintenanceTypeValue, value); }

        } // PREVENTIVE, CORRECTIVE, EMERGENCY
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? ScheduledDateValue;

        public DateTime? ScheduledDate

        {

            get { return this.ScheduledDateValue; }

            set { SetProperty(ref ScheduledDateValue, value); }

        }
        private DateTime? CompletedDateValue;

        public DateTime? CompletedDate

        {

            get { return this.CompletedDateValue; }

            set { SetProperty(ref CompletedDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? MaintenanceData { get; set; }
    }

    public class WellInspectionRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string InspectionTypeValue = string.Empty;

        public string InspectionType

        {

            get { return this.InspectionTypeValue; }

            set { SetProperty(ref InspectionTypeValue, value); }

        } // REGULAR, COMPLIANCE, SAFETY, INTEGRITY
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

    public class WellEquipmentRequest : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string EquipmentTypeValue = string.Empty;

        public string EquipmentType

        {

            get { return this.EquipmentTypeValue; }

            set { SetProperty(ref EquipmentTypeValue, value); }

        }
        private string? EquipmentNameValue;

        public string? EquipmentName

        {

            get { return this.EquipmentNameValue; }

            set { SetProperty(ref EquipmentNameValue, value); }

        }
        private string? ManufacturerValue;

        public string? Manufacturer

        {

            get { return this.ManufacturerValue; }

            set { SetProperty(ref ManufacturerValue, value); }

        }
        private string? ModelValue;

        public string? Model

        {

            get { return this.ModelValue; }

            set { SetProperty(ref ModelValue, value); }

        }
        private DateTime? InstallationDateValue;

        public DateTime? InstallationDate

        {

            get { return this.InstallationDateValue; }

            set { SetProperty(ref InstallationDateValue, value); }

        }
        public Dictionary<string, object>? EquipmentData { get; set; }
    }

    public class WellResponse : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? CurrentStateValue;

        public string? CurrentState

        {

            get { return this.CurrentStateValue; }

            set { SetProperty(ref CurrentStateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class WellPerformanceResponse : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private DateTime ReportDateValue;

        public DateTime ReportDate

        {

            get { return this.ReportDateValue; }

            set { SetProperty(ref ReportDateValue, value); }

        }
        public Dictionary<string, decimal> Metrics { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    // Workover-specific response DTOs
    public class WellWorkoverResponse : ModelEntityBase
    {
        private string WellIdValue = string.Empty;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
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
        private string WorkoverTypeValue = string.Empty;

        public string WorkoverType

        {

            get { return this.WorkoverTypeValue; }

            set { SetProperty(ref WorkoverTypeValue, value); }

        } // RIG_WORKOVER, RIGLESS_WORKOVER, etc.
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
        public Dictionary<string, object>? WorkoverData { get; set; }
    }
}








