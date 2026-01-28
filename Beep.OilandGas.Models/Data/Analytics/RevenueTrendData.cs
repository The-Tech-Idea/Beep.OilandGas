using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Analytics
{
    public class RevenueTrendData : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private decimal RevenueValue;

        public decimal Revenue

        {

            get { return this.RevenueValue; }

            set { SetProperty(ref RevenueValue, value); }

        }
    }
}
