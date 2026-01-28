using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Inventory
{
    public class CreateInventoryAdjustmentRequest : ModelEntityBase
    {
        private string InventoryItemIdValue;

        public string InventoryItemId

        {

            get { return this.InventoryItemIdValue; }

            set { SetProperty(ref InventoryItemIdValue, value); }

        }
        private DateTime AdjustmentDateValue;

        public DateTime AdjustmentDate

        {

            get { return this.AdjustmentDateValue; }

            set { SetProperty(ref AdjustmentDateValue, value); }

        }
        private string AdjustmentTypeValue;

        public string AdjustmentType

        {

            get { return this.AdjustmentTypeValue; }

            set { SetProperty(ref AdjustmentTypeValue, value); }

        } // Increase, Decrease
        private decimal QuantityAdjustmentValue;

        public decimal QuantityAdjustment

        {

            get { return this.QuantityAdjustmentValue; }

            set { SetProperty(ref QuantityAdjustmentValue, value); }

        }
        private decimal? UnitCostAdjustmentValue;

        public decimal? UnitCostAdjustment

        {

            get { return this.UnitCostAdjustmentValue; }

            set { SetProperty(ref UnitCostAdjustmentValue, value); }

        }
        private string ReasonValue;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
