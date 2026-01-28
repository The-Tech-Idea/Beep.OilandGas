using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Operations
{
    public class ManageInjectionRequest : ModelEntityBase
    {
        private string InjectionWellIdValue = string.Empty;

        public string InjectionWellId

        {

            get { return this.InjectionWellIdValue; }

            set { SetProperty(ref InjectionWellIdValue, value); }

        }
        private decimal InjectionRateValue;

        public decimal InjectionRate

        {

            get { return this.InjectionRateValue; }

            set { SetProperty(ref InjectionRateValue, value); }

        }
    }
}
