using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Revenue
{
    public class WorkingInterestRequest : ModelEntityBase
    {
        private string OwnerIdValue = string.Empty;

        public string OwnerId

        {

            get { return this.OwnerIdValue; }

            set { SetProperty(ref OwnerIdValue, value); }

        }
        private decimal InterestPercentageValue;

        public decimal InterestPercentage

        {

            get { return this.InterestPercentageValue; }

            set { SetProperty(ref InterestPercentageValue, value); }

        }
    }
}
