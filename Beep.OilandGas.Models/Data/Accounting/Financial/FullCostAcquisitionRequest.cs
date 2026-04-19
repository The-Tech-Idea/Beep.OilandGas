using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;

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

        private ProvedProperty PropertyValue = new();

        public ProvedProperty Property
        {
            get { return this.PropertyValue; }
            set { SetProperty(ref PropertyValue, value); }
        }
    }
}
