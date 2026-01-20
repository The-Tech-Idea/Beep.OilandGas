using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// DTOs for Field Management operations
    /// </summary>
    
    public class FieldCreationRequest : ModelEntityBase
    {
        public string FieldName { get; set; } = string.Empty;
        public string? FieldType { get; set; }
        public string? AreaId { get; set; }
        public string? BasinId { get; set; }
        public string? Country { get; set; }
        public string? StateProvince { get; set; }
        public string? County { get; set; }
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public class FieldPlanningRequest : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public string PlanningType { get; set; } = string.Empty; // EXPLORATION, DEVELOPMENT, PRODUCTION, etc.
        public string? Description { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetEndDate { get; set; }
        public Dictionary<string, object>? PlanningData { get; set; }
    }

    public class FieldOperationsRequest : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public string OperationType { get; set; } = string.Empty;
        public DateTime OperationDate { get; set; }
        public string? Description { get; set; }
        public Dictionary<string, object>? OperationData { get; set; }
    }

    public class FieldConfigurationRequest : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new Dictionary<string, object>();
    }

    public class FieldPerformanceRequest : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Metrics { get; set; }
    }

    public class FieldResponse : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string? CurrentPhase { get; set; }
        public string? Status { get; set; }
        public Dictionary<string, object>? Properties { get; set; }
    }

    public class FieldPerformanceResponse : ModelEntityBase
    {
        public string FieldId { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public Dictionary<string, decimal> Metrics { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
}





