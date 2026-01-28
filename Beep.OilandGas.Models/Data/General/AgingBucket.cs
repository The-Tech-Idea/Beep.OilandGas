using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.General
{
    public class AgingBucket : ModelEntityBase
    {
        private string BucketNameValue;

        public string BucketName

        {

            get { return this.BucketNameValue; }

            set { SetProperty(ref BucketNameValue, value); }

        }
        private int DaysMinValue;

        public int DaysMin

        {

            get { return this.DaysMinValue; }

            set { SetProperty(ref DaysMinValue, value); }

        }
        private int DaysMaxValue;

        public int DaysMax

        {

            get { return this.DaysMaxValue; }

            set { SetProperty(ref DaysMaxValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private decimal PercentageOfTotalValue;

        public decimal PercentageOfTotal

        {

            get { return this.PercentageOfTotalValue; }

            set { SetProperty(ref PercentageOfTotalValue, value); }

        }
        private List<string> ItemsValue = new();

        public List<string> Items

        {

            get { return this.ItemsValue; }

            set { SetProperty(ref ItemsValue, value); }

        }
    }
}
