using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    /// <summary>
    /// DTO for development plan.
    /// </summary>
    public class DevelopmentPlan : ModelEntityBase
    {
        private string PlanIdValue = string.Empty;

        public string PlanId

        {

            get { return this.PlanIdValue; }

            set { SetProperty(ref PlanIdValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string PlanNameValue = string.Empty;

        public string PlanName

        {

            get { return this.PlanNameValue; }

            set { SetProperty(ref PlanNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private DateTime? PlanDateValue;

        public DateTime? PlanDate

        {

            get { return this.PlanDateValue; }

            set { SetProperty(ref PlanDateValue, value); }

        }
        private DateTime? TargetStartDateValue;

        public DateTime? TargetStartDate

        {

            get { return this.TargetStartDateValue; }

            set { SetProperty(ref TargetStartDateValue, value); }

        }
        private DateTime? TargetCompletionDateValue;

        public DateTime? TargetCompletionDate

        {

            get { return this.TargetCompletionDateValue; }

            set { SetProperty(ref TargetCompletionDateValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? CurrencyValue;

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
        private string? ApprovedByValue;

        public string? ApprovedBy

        {

            get { return this.ApprovedByValue; }

            set { SetProperty(ref ApprovedByValue, value); }

        }
        private DateTime? ApprovalDateValue;

        public DateTime? ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }
        private List<WellPlan> WellPlansValue = new();

        public List<WellPlan> WellPlans

        {

            get { return this.WellPlansValue; }

            set { SetProperty(ref WellPlansValue, value); }

        }
        private List<FacilityPlan> FacilityPlansValue = new();

        public List<FacilityPlan> FacilityPlans

        {

            get { return this.FacilityPlansValue; }

            set { SetProperty(ref FacilityPlansValue, value); }

        }
        private List<PermitApplication> PermitApplicationsValue = new();

        public List<PermitApplication> PermitApplications

        {

            get { return this.PermitApplicationsValue; }

            set { SetProperty(ref PermitApplicationsValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for well plan.
    /// </summary>
    public class WellPlan : ModelEntityBase
    {
        private string WellPlanIdValue = string.Empty;

        public string WellPlanId

        {

            get { return this.WellPlanIdValue; }

            set { SetProperty(ref WellPlanIdValue, value); }

        }
        private string PlanIdValue = string.Empty;

        public string PlanId

        {

            get { return this.PlanIdValue; }

            set { SetProperty(ref PlanIdValue, value); }

        }
        private string? WellUWIValue;

        public string? WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string? WellTypeValue;

        public string? WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        }
        private string? DrillingMethodValue;

        public string? DrillingMethod

        {

            get { return this.DrillingMethodValue; }

            set { SetProperty(ref DrillingMethodValue, value); }

        }
        private decimal? TargetDepthValue;

        public decimal? TargetDepth

        {

            get { return this.TargetDepthValue; }

            set { SetProperty(ref TargetDepthValue, value); }

        }
        private string? TargetFormationValue;

        public string? TargetFormation

        {

            get { return this.TargetFormationValue; }

            set { SetProperty(ref TargetFormationValue, value); }

        }
        private DateTime? PlannedSpudDateValue;

        public DateTime? PlannedSpudDate

        {

            get { return this.PlannedSpudDateValue; }

            set { SetProperty(ref PlannedSpudDateValue, value); }

        }
        private DateTime? PlannedCompletionDateValue;

        public DateTime? PlannedCompletionDate

        {

            get { return this.PlannedCompletionDateValue; }

            set { SetProperty(ref PlannedCompletionDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
        private DateTime CreatedDateValue;

        public DateTime CreatedDate

        {

            get { return this.CreatedDateValue; }

            set { SetProperty(ref CreatedDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for facility plan.
    /// </summary>
    public class FacilityPlan : ModelEntityBase
    {
        private string FacilityPlanIdValue = string.Empty;

        public string FacilityPlanId

        {

            get { return this.FacilityPlanIdValue; }

            set { SetProperty(ref FacilityPlanIdValue, value); }

        }
        private string PlanIdValue = string.Empty;

        public string PlanId

        {

            get { return this.PlanIdValue; }

            set { SetProperty(ref PlanIdValue, value); }

        }
        private string FacilityNameValue = string.Empty;

        public string FacilityName

        {

            get { return this.FacilityNameValue; }

            set { SetProperty(ref FacilityNameValue, value); }

        }
        private string FacilityTypeValue = string.Empty;

        public string FacilityType

        {

            get { return this.FacilityTypeValue; }

            set { SetProperty(ref FacilityTypeValue, value); }

        }
        private string? LocationValue;

        public string? Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }
        private decimal? CapacityValue;

        public decimal? Capacity

        {

            get { return this.CapacityValue; }

            set { SetProperty(ref CapacityValue, value); }

        }
        private string? CapacityUnitValue;

        public string? CapacityUnit

        {

            get { return this.CapacityUnitValue; }

            set { SetProperty(ref CapacityUnitValue, value); }

        }
        private DateTime? PlannedStartDateValue;

        public DateTime? PlannedStartDate

        {

            get { return this.PlannedStartDateValue; }

            set { SetProperty(ref PlannedStartDateValue, value); }

        }
        private DateTime? PlannedCompletionDateValue;

        public DateTime? PlannedCompletionDate

        {

            get { return this.PlannedCompletionDateValue; }

            set { SetProperty(ref PlannedCompletionDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }

    /// <summary>
    /// DTO for permit application (simplified from PermitsAndApplications).
    /// </summary>
    public class PermitApplication : ModelEntityBase
    {
        private string ApplicationIdValue = string.Empty;

        public string ApplicationId

        {

            get { return this.ApplicationIdValue; }

            set { SetProperty(ref ApplicationIdValue, value); }

        }
        private string ApplicationTypeValue = string.Empty;

        public string ApplicationType

        {

            get { return this.ApplicationTypeValue; }

            set { SetProperty(ref ApplicationTypeValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

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
        private string? RegulatoryAuthorityValue;

        public string? RegulatoryAuthority

        {

            get { return this.RegulatoryAuthorityValue; }

            set { SetProperty(ref RegulatoryAuthorityValue, value); }

        }
        private DateTime? SubmittedDateValue;

        public DateTime? SubmittedDate

        {

            get { return this.SubmittedDateValue; }

            set { SetProperty(ref SubmittedDateValue, value); }

        }
        private DateTime? DecisionDateValue;

        public DateTime? DecisionDate

        {

            get { return this.DecisionDateValue; }

            set { SetProperty(ref DecisionDateValue, value); }

        }
        private string? DecisionValue;

        public string? Decision

        {

            get { return this.DecisionValue; }

            set { SetProperty(ref DecisionValue, value); }

        }
        private DateTime? ExpiryDateValue;

        public DateTime? ExpiryDate

        {

            get { return this.ExpiryDateValue; }

            set { SetProperty(ref ExpiryDateValue, value); }

        }
    }

    /// <summary>
    /// DTO for creating a development plan.
    /// </summary>
    public class CreateDevelopmentPlan : ModelEntityBase
    {
        private string PlanNameValue = string.Empty;

        public string PlanName

        {

            get { return this.PlanNameValue; }

            set { SetProperty(ref PlanNameValue, value); }

        }
        private string FieldIdValue = string.Empty;

        public string FieldId

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
        private DateTime? TargetStartDateValue;

        public DateTime? TargetStartDate

        {

            get { return this.TargetStartDateValue; }

            set { SetProperty(ref TargetStartDateValue, value); }

        }
        private DateTime? TargetCompletionDateValue;

        public DateTime? TargetCompletionDate

        {

            get { return this.TargetCompletionDateValue; }

            set { SetProperty(ref TargetCompletionDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
        private string? CurrencyValue;

        public string? Currency

        {

            get { return this.CurrencyValue; }

            set { SetProperty(ref CurrencyValue, value); }

        }
    }

    /// <summary>
    /// DTO for updating a development plan.
    /// </summary>
    public class UpdateDevelopmentPlan : ModelEntityBase
    {
        private string? PlanNameValue;

        public string? PlanName

        {

            get { return this.PlanNameValue; }

            set { SetProperty(ref PlanNameValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? TargetStartDateValue;

        public DateTime? TargetStartDate

        {

            get { return this.TargetStartDateValue; }

            set { SetProperty(ref TargetStartDateValue, value); }

        }
        private DateTime? TargetCompletionDateValue;

        public DateTime? TargetCompletionDate

        {

            get { return this.TargetCompletionDateValue; }

            set { SetProperty(ref TargetCompletionDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
    }

    /// <summary>
    /// DTO for creating a well plan.
    /// </summary>
    public class CreateWellPlan : ModelEntityBase
    {
        private string WellNameValue = string.Empty;

        public string WellName

        {

            get { return this.WellNameValue; }

            set { SetProperty(ref WellNameValue, value); }

        }
        private string? WellTypeValue;

        public string? WellType

        {

            get { return this.WellTypeValue; }

            set { SetProperty(ref WellTypeValue, value); }

        }
        private string? DrillingMethodValue;

        public string? DrillingMethod

        {

            get { return this.DrillingMethodValue; }

            set { SetProperty(ref DrillingMethodValue, value); }

        }
        private decimal? TargetDepthValue;

        public decimal? TargetDepth

        {

            get { return this.TargetDepthValue; }

            set { SetProperty(ref TargetDepthValue, value); }

        }
        private string? TargetFormationValue;

        public string? TargetFormation

        {

            get { return this.TargetFormationValue; }

            set { SetProperty(ref TargetFormationValue, value); }

        }
        private DateTime? PlannedSpudDateValue;

        public DateTime? PlannedSpudDate

        {

            get { return this.PlannedSpudDateValue; }

            set { SetProperty(ref PlannedSpudDateValue, value); }

        }
        private DateTime? PlannedCompletionDateValue;

        public DateTime? PlannedCompletionDate

        {

            get { return this.PlannedCompletionDateValue; }

            set { SetProperty(ref PlannedCompletionDateValue, value); }

        }
        private decimal? EstimatedCostValue;

        public decimal? EstimatedCost

        {

            get { return this.EstimatedCostValue; }

            set { SetProperty(ref EstimatedCostValue, value); }

        }
    }
}







