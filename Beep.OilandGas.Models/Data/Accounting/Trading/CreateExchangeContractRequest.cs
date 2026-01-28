using System;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting.Trading
{
    public class CreateExchangeContractRequest : ModelEntityBase
    {
        private string ContractIdValue = string.Empty;

        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }
        private string ContractNameValue = string.Empty;

        public string ContractName

        {

            get { return this.ContractNameValue; }

            set { SetProperty(ref ContractNameValue, value); }

        }
        private ExchangeContractType ContractTypeValue;

        public ExchangeContractType ContractType

        {

            get { return this.ContractTypeValue; }

            set { SetProperty(ref ContractTypeValue, value); }

        }
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }
    }
}
