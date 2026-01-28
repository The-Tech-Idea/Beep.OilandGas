using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class CREATE_DRILLING_OPERATION : ModelEntityBase
    {
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private DateTime? PlannedSpudDateValue;

        public DateTime? PlannedSpudDate

        {

            get { return this.PlannedSpudDateValue; }

            set { SetProperty(ref PlannedSpudDateValue, value); }

        }
        private decimal? TargetDepthValue;

        public decimal? TargetDepth

        {

            get { return this.TargetDepthValue; }

            set { SetProperty(ref TargetDepthValue, value); }

        }
        private string? DrillingContractorValue;

        public string? DrillingContractor

        {

            get { return this.DrillingContractorValue; }

            set { SetProperty(ref DrillingContractorValue, value); }

        }
        private string? RigNameValue;

        public string? RigName

        {

            get { return this.RigNameValue; }

            set { SetProperty(ref RigNameValue, value); }

        }
        private decimal? EstimatedDailyCostValue;

        public decimal? EstimatedDailyCost

        {

            get { return this.EstimatedDailyCostValue; }

            set { SetProperty(ref EstimatedDailyCostValue, value); }

        }
    }
}
