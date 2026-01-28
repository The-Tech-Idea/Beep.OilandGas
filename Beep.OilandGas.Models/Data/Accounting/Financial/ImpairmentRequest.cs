using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Financial
{
    public class ImpairmentRequest : ModelEntityBase
    {
        private string PropertyIdValue = string.Empty;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private decimal ImpairmentAmountValue;

        public decimal ImpairmentAmount

        {

            get { return this.ImpairmentAmountValue; }

            set { SetProperty(ref ImpairmentAmountValue, value); }

        }
    }
}
