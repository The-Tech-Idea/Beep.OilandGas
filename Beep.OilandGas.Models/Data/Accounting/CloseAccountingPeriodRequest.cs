using System;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CloseAccountingPeriodRequest : ModelEntityBase
    {
        private DateTime periodEndValue;

        public DateTime PeriodEnd
        {
            get { return this.periodEndValue; }
            set { SetProperty(ref periodEndValue, value); }
        }
    }
}