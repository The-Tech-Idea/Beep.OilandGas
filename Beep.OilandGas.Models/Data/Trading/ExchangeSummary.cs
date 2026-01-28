using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public partial class ExchangeSummary : ModelEntityBase
    {
        private System.Decimal TotalVolumeValue;
        public System.Decimal TotalVolume
        {
            get { return this.TotalVolumeValue; }
            set { SetProperty(ref TotalVolumeValue, value); }
        }

        private System.Decimal AveragePriceValue;
        public System.Decimal AveragePrice
        {
            get { return this.AveragePriceValue; }
            set { SetProperty(ref AveragePriceValue, value); }
        }

        public System.Decimal TotalValue => TotalVolume * AveragePrice;

        private System.Int32 TransactionCountValue;
        public System.Int32 TransactionCount
        {
            get { return this.TransactionCountValue; }
            set { SetProperty(ref TransactionCountValue, value); }
        }
    }
}
