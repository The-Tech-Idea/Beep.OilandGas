using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Data
{
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
}
