using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public class CreateExchangeContractRequest : ModelEntityBase
    {
        private string ContractIdValue = string.Empty;

        [Required]
        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }
        private string ContractNameValue = string.Empty;

        [Required]
        public string ContractName

        {

            get { return this.ContractNameValue; }

            set { SetProperty(ref ContractNameValue, value); }

        }
        private ExchangeContractType ContractTypeValue;

        [Required]
        public ExchangeContractType ContractType

        {

            get { return this.ContractTypeValue; }

            set { SetProperty(ref ContractTypeValue, value); }

        }
        private List<ExchangeParty> PartiesValue = new();

        [Required]
        public List<ExchangeParty> Parties

        {

            get { return this.PartiesValue; }

            set { SetProperty(ref PartiesValue, value); }

        }
        private DateTime EffectiveDateValue;

        [Required]
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
        private ExchangeTerms? TermsValue;

        public ExchangeTerms? Terms

        {

            get { return this.TermsValue; }

            set { SetProperty(ref TermsValue, value); }

        }
        private List<ExchangeDeliveryPoint>? DeliveryPointsValue;

        public List<ExchangeDeliveryPoint>? DeliveryPoints

        {

            get { return this.DeliveryPointsValue; }

            set { SetProperty(ref DeliveryPointsValue, value); }

        }
        private ExchangePricingTerms? PricingTermsValue;

        public ExchangePricingTerms? PricingTerms

        {

            get { return this.PricingTermsValue; }

            set { SetProperty(ref PricingTermsValue, value); }

        }
    }
}
