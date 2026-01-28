
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CreateRevenueTransactionRequest : ModelEntityBase
    {
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
        private string ContractIdValue;

        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string RevenueTypeValue;

        public string RevenueType

        {

            get { return this.RevenueTypeValue; }

            set { SetProperty(ref RevenueTypeValue, value); }

        }
        private decimal GrossRevenueValue;

        public decimal GrossRevenue

        {

            get { return this.GrossRevenueValue; }

            set { SetProperty(ref GrossRevenueValue, value); }

        }
        private decimal? OilVolumeValue;

        public decimal? OilVolume

        {

            get { return this.OilVolumeValue; }

            set { SetProperty(ref OilVolumeValue, value); }

        }
        private decimal? GasVolumeValue;

        public decimal? GasVolume

        {

            get { return this.GasVolumeValue; }

            set { SetProperty(ref GasVolumeValue, value); }

        }
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private string CurrencyCodeValue;

        public string CurrencyCode

        {

            get { return this.CurrencyCodeValue; }

            set { SetProperty(ref CurrencyCodeValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
