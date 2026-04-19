using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Data
{
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
}
