using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.General
{
    public class OutstandingItem : ModelEntityBase
    {
        private string ItemIdValue;

        public string ItemId

        {

            get { return this.ItemIdValue; }

            set { SetProperty(ref ItemIdValue, value); }

        }
        private DateTime ItemDateValue;

        public DateTime ItemDate

        {

            get { return this.ItemDateValue; }

            set { SetProperty(ref ItemDateValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private string TypeValue;

        public string Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        } // Check, Deposit
        private int DaysOutstandingValue;

        public int DaysOutstanding

        {

            get { return this.DaysOutstandingValue; }

            set { SetProperty(ref DaysOutstandingValue, value); }

        }
    }
}

