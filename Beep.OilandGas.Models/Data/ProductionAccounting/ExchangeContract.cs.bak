using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ExchangeContract : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the contract identifier.
        /// </summary>
        private string ContractIdValue = string.Empty;

        public string ContractId

        {

            get { return this.ContractIdValue; }

            set { SetProperty(ref ContractIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the contract name or description.
        /// </summary>
        private string ContractNameValue = string.Empty;

        public string ContractName

        {

            get { return this.ContractNameValue; }

            set { SetProperty(ref ContractNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the contract type.
        /// </summary>
        private ExchangeContractType ContractTypeValue;

        public ExchangeContractType ContractType

        {

            get { return this.ContractTypeValue; }

            set { SetProperty(ref ContractTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the parties to the contract.
        /// </summary>
        private List<ExchangeParty> PartiesValue = new();

        public List<ExchangeParty> Parties

        {

            get { return this.PartiesValue; }

            set { SetProperty(ref PartiesValue, value); }

        }

        /// <summary>
        /// Gets or sets the effective date.
        /// </summary>
        private DateTime EffectiveDateValue;

        public DateTime EffectiveDate

        {

            get { return this.EffectiveDateValue; }

            set { SetProperty(ref EffectiveDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the expiration date.
        /// </summary>
        private DateTime? ExpirationDateValue;

        public DateTime? ExpirationDate

        {

            get { return this.ExpirationDateValue; }

            set { SetProperty(ref ExpirationDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the exchange terms.
        /// </summary>
        private ExchangeTerms TermsValue = new();

        public ExchangeTerms Terms

        {

            get { return this.TermsValue; }

            set { SetProperty(ref TermsValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery points.
        /// </summary>
        private List<ExchangeDeliveryPoint> DeliveryPointsValue = new();

        public List<ExchangeDeliveryPoint> DeliveryPoints

        {

            get { return this.DeliveryPointsValue; }

            set { SetProperty(ref DeliveryPointsValue, value); }

        }

        /// <summary>
        /// Gets or sets the pricing terms.
        /// </summary>
        private ExchangePricingTerms PricingTermsValue = new();

        public ExchangePricingTerms PricingTerms

        {

            get { return this.PricingTermsValue; }

            set { SetProperty(ref PricingTermsValue, value); }

        }
    }
}
