using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Financial
{
    public class FullCostAcquisitionRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private UnprovedProperty PropertyValue = new();

        public UnprovedProperty Property

        {

            get { return this.PropertyValue; }

            set { SetProperty(ref PropertyValue, value); }

        }
    }
}
