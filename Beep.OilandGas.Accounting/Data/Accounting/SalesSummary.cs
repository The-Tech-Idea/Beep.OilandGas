using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class SalesSummary : ModelEntityBase
    {
        private System.Decimal TotalNetVolumeValue;
        public System.Decimal TotalNetVolume
        {
            get { return this.TotalNetVolumeValue; }
            set { SetProperty(ref TotalNetVolumeValue, value); }
        }

        private System.Decimal AveragePricePerBarrelValue;
        public System.Decimal AveragePricePerBarrel
        {
            get { return this.AveragePricePerBarrelValue; }
            set { SetProperty(ref AveragePricePerBarrelValue, value); }
        }

        private System.Decimal TotalGrossRevenueValue;
        public System.Decimal TotalGrossRevenue
        {
            get { return this.TotalGrossRevenueValue; }
            set { SetProperty(ref TotalGrossRevenueValue, value); }
        }

        private System.Decimal TotalCostsValue;
        public System.Decimal TotalCosts
        {
            get { return this.TotalCostsValue; }
            set { SetProperty(ref TotalCostsValue, value); }
        }

        private System.Decimal TotalTaxesValue;
        public System.Decimal TotalTaxes
        {
            get { return this.TotalTaxesValue; }
            set { SetProperty(ref TotalTaxesValue, value); }
        }

        public System.Decimal TotalNetRevenue => TotalGrossRevenue - TotalCosts - TotalTaxes;

        private System.Int32 TransactionCountValue;
        public System.Int32 TransactionCount
        {
            get { return this.TransactionCountValue; }
            set { SetProperty(ref TransactionCountValue, value); }
        }
    }
}
