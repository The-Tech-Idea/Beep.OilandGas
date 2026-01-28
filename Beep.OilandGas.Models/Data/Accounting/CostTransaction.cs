
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CostTransaction : ModelEntityBase
    {
        private string CostTransactionIdValue;

        public string CostTransactionId

        {

            get { return this.CostTransactionIdValue; }

            set { SetProperty(ref CostTransactionIdValue, value); }

        }
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private string WellIdValue;

        public string WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string CostTypeValue;

        public string CostType

        {

            get { return this.CostTypeValue; }

            set { SetProperty(ref CostTypeValue, value); }

        }
        private string CostCategoryValue;

        public string CostCategory

        {

            get { return this.CostCategoryValue; }

            set { SetProperty(ref CostCategoryValue, value); }

        }
        private decimal? AmountValue;

        public decimal? Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private DateTime? TransactionDateValue;

        public DateTime? TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string IsCapitalizedValue;

        public string IsCapitalized

        {

            get { return this.IsCapitalizedValue; }

            set { SetProperty(ref IsCapitalizedValue, value); }

        }
        private string IsExpensedValue;

        public string IsExpensed

        {

            get { return this.IsExpensedValue; }

            set { SetProperty(ref IsExpensedValue, value); }

        }
        private string AfeIdValue;

        public string AfeId

        {

            get { return this.AfeIdValue; }

            set { SetProperty(ref AfeIdValue, value); }

        }
        private string CostCenterIdValue;

        public string CostCenterId

        {

            get { return this.CostCenterIdValue; }

            set { SetProperty(ref CostCenterIdValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
