using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.Models.Data
{
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
