using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Data.Accounting.Financial
{
    public class FullCostDevelopmentRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId
        {
            get { return this.CostCenterIdValue; }
            set { SetProperty(ref CostCenterIdValue, value); }
        }

        private DevelopmentCosts CostsValue = new();

        public DevelopmentCosts Costs
        {
            get { return this.CostsValue; }
            set { SetProperty(ref CostsValue, value); }
        }
    }
}
