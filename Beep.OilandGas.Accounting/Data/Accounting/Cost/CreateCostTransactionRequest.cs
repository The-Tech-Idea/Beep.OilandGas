using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Cost
{
    public class CreateCostTransactionRequest : ModelEntityBase
    {
        private decimal CostAmountValue;

        public decimal CostAmount

        {

            get { return this.CostAmountValue; }

            set { SetProperty(ref CostAmountValue, value); }

        }
        private bool IsCapitalizedValue;

        public bool IsCapitalized

        {

            get { return this.IsCapitalizedValue; }

            set { SetProperty(ref IsCapitalizedValue, value); }

        }
        private bool IsCashValue;

        public bool IsCash

        {

            get { return this.IsCashValue; }

            set { SetProperty(ref IsCashValue, value); }

        }
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string? PropertyIdValue;

        public string? PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string? DescriptionValue;

        public string? Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
