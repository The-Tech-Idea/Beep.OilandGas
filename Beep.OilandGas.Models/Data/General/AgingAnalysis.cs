using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.General
{
    public class AgingAnalysis : ModelEntityBase
    {
        private string AccountIdValue;

        public string AccountId

        {

            get { return this.AccountIdValue; }

            set { SetProperty(ref AccountIdValue, value); }

        }
        private DateTime AsOfDateValue;

        public DateTime AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
        private List<AgingBucket> BucketsValue = new();

        public List<AgingBucket> Buckets

        {

            get { return this.BucketsValue; }

            set { SetProperty(ref BucketsValue, value); }

        }
        private decimal TotalAmountValue;

        public decimal TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
    }
}
