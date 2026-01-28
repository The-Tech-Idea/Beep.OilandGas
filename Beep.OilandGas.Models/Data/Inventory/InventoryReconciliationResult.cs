using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Inventory
{
    public class InventoryReconciliationResult : ModelEntityBase
    {
        private string InventoryItemIdValue;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private decimal BookQuantityValue;

        public decimal BookQuantity

        {

            get { return this.BookQuantityValue; }

            set { SetProperty(ref BookQuantityValue, value); }

        }
        private decimal PhysicalQuantityValue;

        public decimal PhysicalQuantity

        {

            get { return this.PhysicalQuantityValue; }

            set { SetProperty(ref PhysicalQuantityValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private decimal VarianceAmountValue;

        public decimal VarianceAmount

        {

            get { return this.VarianceAmountValue; }

            set { SetProperty(ref VarianceAmountValue, value); }

        }
        private bool RequiresAdjustmentValue;

        public bool RequiresAdjustment

        {

            get { return this.RequiresAdjustmentValue; }

            set { SetProperty(ref RequiresAdjustmentValue, value); }

        }
        private string AdjustmentIdValue;

        public string AdjustmentId

        {

            get { return this.AdjustmentIdValue; }

            set { SetProperty(ref AdjustmentIdValue, value); }

        }
    }
}
