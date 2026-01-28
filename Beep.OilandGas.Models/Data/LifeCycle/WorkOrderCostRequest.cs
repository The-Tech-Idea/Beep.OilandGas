using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class WorkOrderCostRequest : ModelEntityBase
    {
        private string WorkOrderIdValue = string.Empty;

        public string WorkOrderId

        {

            get { return this.WorkOrderIdValue; }

            set { SetProperty(ref WorkOrderIdValue, value); }

        }
        private string CostTypeValue = string.Empty;

        public string CostType

        {

            get { return this.CostTypeValue; }

            set { SetProperty(ref CostTypeValue, value); }

        } // WORKOVER, MAINTENANCE, REPAIR, etc.
        private string CostCategoryValue = string.Empty;

        public string CostCategory

        {

            get { return this.CostCategoryValue; }

            set { SetProperty(ref CostCategoryValue, value); }

        } // LABOR, MATERIALS, EQUIPMENT, etc.
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private bool IsCapitalizedValue;

        public bool IsCapitalized

        {

            get { return this.IsCapitalizedValue; }

            set { SetProperty(ref IsCapitalizedValue, value); }

        }
        private bool IsExpensedValue;

        public bool IsExpensed

        {

            get { return this.IsExpensedValue; }

            set { SetProperty(ref IsExpensedValue, value); }

        }
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
