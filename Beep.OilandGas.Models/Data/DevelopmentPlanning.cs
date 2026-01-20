using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for development plan.
    /// </summary>
    public class DevelopmentPlan : ModelEntityBase
    {
        public string PlanId { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? PlanDate { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public string? Status { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Currency { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public List<WellPlan> WellPlans { get; set; } = new();
        public List<FacilityPlan> FacilityPlans { get; set; } = new();
        public List<PermitApplication> PermitApplications { get; set; } = new();
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for well plan.
    /// </summary>
    public class WellPlan : ModelEntityBase
    {
        public string WellPlanId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string? WellUWI { get; set; }
        public string WellName { get; set; } = string.Empty;
        public string? WellType { get; set; }
        public string? DrillingMethod { get; set; }
        public decimal? TargetDepth { get; set; }
        public string? TargetFormation { get; set; }
        public DateTime? PlannedSpudDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    /// <summary>
    /// DTO for facility plan.
    /// </summary>
    public class FacilityPlan : ModelEntityBase
    {
        public string FacilityPlanId { get; set; } = string.Empty;
        public string PlanId { get; set; } = string.Empty;
        public string FacilityName { get; set; } = string.Empty;
        public string FacilityType { get; set; } = string.Empty;
        public string? Location { get; set; }
        public decimal? Capacity { get; set; }
        public string? CapacityUnit { get; set; }
        public DateTime? PlannedStartDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Status { get; set; }
        public string? Remarks { get; set; }
    }

    /// <summary>
    /// DTO for permit application (simplified from PermitsAndApplications).
    /// </summary>
    public class PermitApplication : ModelEntityBase
    {
        public string ApplicationId { get; set; } = string.Empty;
        public string ApplicationType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Country { get; set; }
        public string? StateProvince { get; set; }
        public string? RegulatoryAuthority { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public DateTime? DecisionDate { get; set; }
        public string? Decision { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// DTO for creating a development plan.
    /// </summary>
    public class CreateDevelopmentPlan : ModelEntityBase
    {
        public string PlanName { get; set; } = string.Empty;
        public string FieldId { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public decimal? EstimatedCost { get; set; }
        public string? Currency { get; set; }
    }

    /// <summary>
    /// DTO for updating a development plan.
    /// </summary>
    public class UpdateDevelopmentPlan : ModelEntityBase
    {
        public string? PlanName { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public DateTime? TargetStartDate { get; set; }
        public DateTime? TargetCompletionDate { get; set; }
        public decimal? EstimatedCost { get; set; }
    }

    /// <summary>
    /// DTO for creating a well plan.
    /// </summary>
    public class CreateWellPlan : ModelEntityBase
    {
        public string WellName { get; set; } = string.Empty;
        public string? WellType { get; set; }
        public string? DrillingMethod { get; set; }
        public decimal? TargetDepth { get; set; }
        public string? TargetFormation { get; set; }
        public DateTime? PlannedSpudDate { get; set; }
        public DateTime? PlannedCompletionDate { get; set; }
        public decimal? EstimatedCost { get; set; }
    }
}





