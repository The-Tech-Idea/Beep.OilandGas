using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class WellConstruction : ModelEntityBase
    {
        private string ConstructionIdValue = string.Empty;

        public string ConstructionId

        {

            get { return this.ConstructionIdValue; }

            set { SetProperty(ref ConstructionIdValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private DateTime? StartDateValue;

        public DateTime? StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime? CompletionDateValue;

        public DateTime? CompletionDate

        {

            get { return this.CompletionDateValue; }

            set { SetProperty(ref CompletionDateValue, value); }

        }
        private List<CasingString> CasingStringsValue = new();

        public List<CasingString> CasingStrings

        {

            get { return this.CasingStringsValue; }

            set { SetProperty(ref CasingStringsValue, value); }

        }
        private List<CompletionString> CompletionStringsValue = new();

        public List<CompletionString> CompletionStrings

        {

            get { return this.CompletionStringsValue; }

            set { SetProperty(ref CompletionStringsValue, value); }

        }
        private string? RemarksValue;

        public string? Remarks

        {

            get { return this.RemarksValue; }

            set { SetProperty(ref RemarksValue, value); }

        }
    }
}
