using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Financial
{
    public class FullCostExplorationRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private ExplorationCosts CostsValue = new();

        public ExplorationCosts Costs

        {

            get { return this.CostsValue; }

            set { SetProperty(ref CostsValue, value); }

        }
    }
}
