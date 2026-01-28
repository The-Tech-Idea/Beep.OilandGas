using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.ProductionOperations;

namespace Beep.OilandGas.Models.Data.Accounting.Financial
{
    public class CeilingTestRequest : ModelEntityBase
    {
        private string CostCenterIdValue = string.Empty;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private ProvedReserves ReservesValue = new();

        public ProvedReserves Reserves

        {

            get { return this.ReservesValue; }

            set { SetProperty(ref ReservesValue, value); }

        }
        private ProductionData ProductionValue = new();

        public ProductionData Production

        {

            get { return this.ProductionValue; }

            set { SetProperty(ref ProductionValue, value); }

        }
    }
}
