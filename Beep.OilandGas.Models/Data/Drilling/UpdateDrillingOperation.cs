using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class UpdateDrillingOperation : ModelEntityBase
    {
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? CurrentDepthValue;

        public decimal? CurrentDepth

        {

            get { return this.CurrentDepthValue; }

            set { SetProperty(ref CurrentDepthValue, value); }

        }
        private decimal? DailyCostValue;

        public decimal? DailyCost

        {

            get { return this.DailyCostValue; }

            set { SetProperty(ref DailyCostValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
    }
}
