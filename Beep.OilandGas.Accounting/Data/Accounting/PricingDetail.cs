using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class PricingDetail : ModelEntityBase
    {
        private System.DateTime DateValue;
        public System.DateTime Date
        {
            get { return this.DateValue; }
            set { SetProperty(ref DateValue, value); }
        }

        private System.Decimal PricePerBarrelValue;
        public System.Decimal PricePerBarrel
        {
            get { return this.PricePerBarrelValue; }
            set { SetProperty(ref PricePerBarrelValue, value); }
        }

        private System.String PricingMethodValue = string.Empty;
        public System.String PricingMethod
        {
            get { return this.PricingMethodValue; }
            set { SetProperty(ref PricingMethodValue, value); }
        }

        private System.String? PriceIndexValue;
        public System.String? PriceIndex
        {
            get { return this.PriceIndexValue; }
            set { SetProperty(ref PriceIndexValue, value); }
        }
    }
}
