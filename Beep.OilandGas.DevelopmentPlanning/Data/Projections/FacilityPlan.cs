using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Data
{
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
}
