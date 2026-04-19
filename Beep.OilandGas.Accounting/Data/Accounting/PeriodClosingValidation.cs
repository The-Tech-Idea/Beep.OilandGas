using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PeriodClosingValidation : ModelEntityBase
    {
        private bool IsReadyValue;

        public bool IsReady

        {

            get { return this.IsReadyValue; }

            set { SetProperty(ref IsReadyValue, value); }

        }
        private List<string> PrerequisitesValue = new();

        public List<string> Prerequisites

        {

            get { return this.PrerequisitesValue; }

            set { SetProperty(ref PrerequisitesValue, value); }

        }
        private List<string> FailedChecksValue = new();

        public List<string> FailedChecks

        {

            get { return this.FailedChecksValue; }

            set { SetProperty(ref FailedChecksValue, value); }

        }
        public Dictionary<string, object> CheckDetails { get; set; } = new();
    }
}
