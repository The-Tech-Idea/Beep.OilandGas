using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class InventoryReportSummary : ModelEntityBase
    {
        private decimal OpeningInventoryValue;

        public decimal OpeningInventory

        {

            get { return this.OpeningInventoryValue; }

            set { SetProperty(ref OpeningInventoryValue, value); }

        }
        private decimal ReceiptsValue;

        public decimal Receipts

        {

            get { return this.ReceiptsValue; }

            set { SetProperty(ref ReceiptsValue, value); }

        }
        private decimal DeliveriesValue;

        public decimal Deliveries

        {

            get { return this.DeliveriesValue; }

            set { SetProperty(ref DeliveriesValue, value); }

        }
        private decimal ClosingInventoryValue;

        public decimal ClosingInventory

        {

            get { return this.ClosingInventoryValue; }

            set { SetProperty(ref ClosingInventoryValue, value); }

        }
    }
}
