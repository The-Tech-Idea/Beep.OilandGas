using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Facility Management operations
    /// </summary>
    
    public class FacilityCreationRequest : ModelEntityBase
    {
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? FacilityTypeValue;

        public string? FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        }
        private string? FacilityPurposeValue;

        public string? FacilityPurpose

        {

            get { return this.FacilityPurposeValue; }

            set { SetProperty(ref FacilityPurposeValue, value); }

        }
        private decimal? CapacityValue;

        public decimal? Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class FacilityOperationsRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

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

    public class FacilityMaintenanceRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

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

    public class FacilityInspectionRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

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

    public class FacilityIntegrityRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string AssessmentTypeValue = string.Empty;

        public string AssessmentType

        {

            get { return this.AssessmentTypeValue; }

            set { SetProperty(ref AssessmentTypeValue, value); }

        }
        private DateTime AssessmentDateValue;

        public DateTime AssessmentDate

        {

            get { return this.AssessmentDateValue; }

            set { SetProperty(ref AssessmentDateValue, value); }

        }
        private string? AssessmentResultValue;

        public string? AssessmentResult

        {

            get { return this.AssessmentResultValue; }

            set { SetProperty(ref AssessmentResultValue, value); }

        }
        public Dictionary<string, object>? AssessmentData { get; set; }
    }

    public class FacilityEquipmentRequest : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

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

    public class FacilityResponse : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }

        // Facility classification
        private string? FacilityTypeValue;

        public string? FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        }
        private string? FacilityCategoryValue;

        public string? FacilityCategory

        {

            get { return this.FacilityCategoryValue; }

            set { SetProperty(ref FacilityCategoryValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }

        // Dates
        private DateTime? ConstructionStartDateValue;

        public DateTime? ConstructionStartDate

        {

            get { return this.ConstructionStartDateValue; }

            set { SetProperty(ref ConstructionStartDateValue, value); }

        }
        private DateTime? ConstructionEndDateValue;

        public DateTime? ConstructionEndDate

        {

            get { return this.ConstructionEndDateValue; }

            set { SetProperty(ref ConstructionEndDateValue, value); }

        }
        private DateTime? CommissionDateValue;

        public DateTime? CommissionDate

        {

            get { return this.CommissionDateValue; }

            set { SetProperty(ref CommissionDateValue, value); }

        }
        private DateTime? DecommissionDateValue;

        public DateTime? DecommissionDate

        {

            get { return this.DecommissionDateValue; }

            set { SetProperty(ref DecommissionDateValue, value); }

        }

        // Location
        private decimal? LatitudeValue;

        public decimal? Latitude

        {

            get { return this.LatitudeValue; }

            set { SetProperty(ref LatitudeValue, value); }

        }
        private decimal? LongitudeValue;

        public decimal? Longitude

        {

            get { return this.LongitudeValue; }

            set { SetProperty(ref LongitudeValue, value); }

        }
        private decimal? ElevationValue;

        public decimal? Elevation

        {

            get { return this.ElevationValue; }

            set { SetProperty(ref ElevationValue, value); }

        }
        private string? ElevationOuomValue;

        public string? ElevationOuom

        {

            get { return this.ElevationOuomValue; }

            set { SetProperty(ref ElevationOuomValue, value); }

        }
        private string? LocationDescriptionValue;

        public string? LocationDescription

        {

            get { return this.LocationDescriptionValue; }

            set { SetProperty(ref LocationDescriptionValue, value); }

        }

        // Capacity and specifications
        private decimal? ProcessingCapacityValue;

        public decimal? ProcessingCapacity

        {

            get { return this.ProcessingCapacityValue; }

            set { SetProperty(ref ProcessingCapacityValue, value); }

        }
        private string? ProcessingCapacityOuomValue;

        public string? ProcessingCapacityOuom

        {

            get { return this.ProcessingCapacityOuomValue; }

            set { SetProperty(ref ProcessingCapacityOuomValue, value); }

        }
        private decimal? StorageCapacityValue;

        public decimal? StorageCapacity

        {

            get { return this.StorageCapacityValue; }

            set { SetProperty(ref StorageCapacityValue, value); }

        }
        private string? StorageCapacityOuomValue;

        public string? StorageCapacityOuom

        {

            get { return this.StorageCapacityOuomValue; }

            set { SetProperty(ref StorageCapacityOuomValue, value); }

        }
        private string? DesignSpecificationsValue;

        public string? DesignSpecifications

        {

            get { return this.DesignSpecificationsValue; }

            set { SetProperty(ref DesignSpecificationsValue, value); }

        }

        // Cost information
        private decimal? ConstructionCostValue;

        public decimal? ConstructionCost

        {

            get { return this.ConstructionCostValue; }

            set { SetProperty(ref ConstructionCostValue, value); }

        }
        private string? ConstructionCostCurrencyValue;

        public string? ConstructionCostCurrency

        {

            get { return this.ConstructionCostCurrencyValue; }

            set { SetProperty(ref ConstructionCostCurrencyValue, value); }

        }
        private decimal? OperatingCostValue;

        public decimal? OperatingCost

        {

            get { return this.OperatingCostValue; }

            set { SetProperty(ref OperatingCostValue, value); }

        }
        private string? OperatingCostCurrencyValue;

        public string? OperatingCostCurrency

        {

            get { return this.OperatingCostCurrencyValue; }

            set { SetProperty(ref OperatingCostCurrencyValue, value); }

        }

        // Operator information
        private string? OperatorIdValue;

        public string? OperatorId

        {

            get { return this.OperatorIdValue; }

            set { SetProperty(ref OperatorIdValue, value); }

        }
        private string? OwnerIdValue;

        public string? OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }

        // Common PPDM fields
        private string? ActiveIndValue;

        public string? ActiveInd

        {

            get { return this.ActiveIndValue; }

            set { SetProperty(ref ActiveIndValue, value); }

        }

        private string? RowQualityValue;

        public string? RowQuality

        {

            get { return this.RowQualityValue; }

            set { SetProperty(ref RowQualityValue, value); }

        }
        private string? PreferredIndValue;

        public string? PreferredInd

        {

            get { return this.PreferredIndValue; }

            set { SetProperty(ref PreferredIndValue, value); }

        }

        // Audit fields
        private DateTime? CreateDateValue;

        public DateTime? CreateDate

        {

            get { return this.CreateDateValue; }

            set { SetProperty(ref CreateDateValue, value); }

        }
        private string? CreateUserValue;

        public string? CreateUser

        {

            get { return this.CreateUserValue; }

            set { SetProperty(ref CreateUserValue, value); }

        }
        private DateTime? UpdateDateValue;

        public DateTime? UpdateDate

        {

            get { return this.UpdateDateValue; }

            set { SetProperty(ref UpdateDateValue, value); }

        }
        private string? UpdateUserValue;

        public string? UpdateUser

        {

            get { return this.UpdateUserValue; }

            set { SetProperty(ref UpdateUserValue, value); }

        }
      
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class FacilityPerformanceResponse : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

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

    // Work order-specific response DTOs
    public class FacilityWorkOrderResponse : ModelEntityBase
    {
        private string FacilityIdValue = string.Empty;

        public string FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

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
        private string WorkOrderTypeValue = string.Empty;

        public string WorkOrderType

        {

            get { return this.WorkOrderTypeValue; }

            set { SetProperty(ref WorkOrderTypeValue, value); }

        } // MAINTENANCE, REPAIR, UPGRADE, etc.
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
        public Dictionary<string, object>? WorkOrderData { get; set; }
    }
}








