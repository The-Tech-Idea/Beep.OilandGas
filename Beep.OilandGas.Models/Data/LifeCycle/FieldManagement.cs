using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Field Management operations
    /// </summary>
    
    public class FieldCreationRequest : ModelEntityBase
    {
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? FieldTypeValue;

        public string? FieldType

        {

            get { return this.FieldTypeValue; }

            set { SetProperty(ref FieldTypeValue, value); }

        }
        private string? AreaIdValue;

        public string? AreaId

        {

            get { return this.AreaIdValue; }

            set { SetProperty(ref AreaIdValue, value); }

        }
        private string? BasinIdValue;

        public string? BasinId

        {

            get { return this.BasinIdValue; }

            set { SetProperty(ref BasinIdValue, value); }

        }
        private string? CountryValue;

        public string? Country

        {

            get { return this.CountryValue; }

            set { SetProperty(ref CountryValue, value); }

        }
        private string? StateProvinceValue;

        public string? StateProvince

        {

            get { return this.StateProvinceValue; }

            set { SetProperty(ref StateProvinceValue, value); }

        }
        private string? CountyValue;

        public string? County

        {

            get { return this.CountyValue; }

            set { SetProperty(ref CountyValue, value); }

        }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class FieldPlanningRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string PlanningTypeValue = string.Empty;

        public string PlanningType

        {

            get { return this.PlanningTypeValue; }

            set { SetProperty(ref PlanningTypeValue, value); }

        } // EXPLORATION, DEVELOPMENT, PRODUCTION, etc.
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

    public class FieldOperationsRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

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

    public class FieldConfigurationRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
    }

    public class FieldPerformanceRequest : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? EndDateValue;

        public DateTime? EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private List<string>? MetricsValue;

        public List<string>? Metrics

        {

            get { return this.MetricsValue; }

            set { SetProperty(ref MetricsValue, value); }

        }
    }

    public class FieldResponse : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string FieldNameValue = string.Empty;

        public string FieldName

        {

            get { return this.FieldNameValue; }

            set { SetProperty(ref FieldNameValue, value); }

        }
        private string? CurrentPhaseValue;

        public string? CurrentPhase

        {

            get { return this.CurrentPhaseValue; }

            set { SetProperty(ref CurrentPhaseValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class FieldPerformanceResponse : ModelEntityBase
    {
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

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
}








