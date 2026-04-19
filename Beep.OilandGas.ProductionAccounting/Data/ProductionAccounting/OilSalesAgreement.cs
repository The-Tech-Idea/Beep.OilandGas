using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class OilSalesAgreement : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the agreement identifier.
        /// </summary>
        private string AgreementIdValue = string.Empty;

        public string AgreementId

        {

            get { return this.AgreementIdValue; }

            set { SetProperty(ref AgreementIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the agreement name or description.
        /// </summary>
        private string AgreementNameValue = string.Empty;

        public string AgreementName

        {

            get { return this.AgreementNameValue; }

            set { SetProperty(ref AgreementNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the seller (producer).
        /// </summary>
        private string SellerValue = string.Empty;

        public string Seller

        {

            get { return this.SellerValue; }

            set { SetProperty(ref SellerValue, value); }

        }

        /// <summary>
        /// Gets or sets the purchaser.
        /// </summary>
        private string PurchaserValue = string.Empty;

        public string Purchaser

        {

            get { return this.PurchaserValue; }

            set { SetProperty(ref PurchaserValue, value); }

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
        /// Gets or sets the pricing terms.
        /// </summary>
        private PricingTerms PricingTermsValue = new();

        public PricingTerms PricingTerms

        {

            get { return this.PricingTermsValue; }

            set { SetProperty(ref PricingTermsValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery terms.
        /// </summary>
        private DeliveryTerms DeliveryTermsValue = new();

        public DeliveryTerms DeliveryTerms

        {

            get { return this.DeliveryTermsValue; }

            set { SetProperty(ref DeliveryTermsValue, value); }

        }

        /// <summary>
        /// Gets or sets the quality specifications.
        /// </summary>
        private CRUDE_OIL_SPECIFICATIONS? QualitySpecificationsValue;

        public CRUDE_OIL_SPECIFICATIONS? QualitySpecifications

        {

            get { return this.QualitySpecificationsValue; }

            set { SetProperty(ref QualitySpecificationsValue, value); }

        }

        /// <summary>
        /// Gets or sets the minimum volume commitment (barrels).
        /// </summary>
        private decimal? MinimumVolumeCommitmentValue;

        public decimal? MinimumVolumeCommitment

        {

            get { return this.MinimumVolumeCommitmentValue; }

            set { SetProperty(ref MinimumVolumeCommitmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the maximum volume commitment (barrels).
        /// </summary>
        private decimal? MaximumVolumeCommitmentValue;

        public decimal? MaximumVolumeCommitment

        {

            get { return this.MaximumVolumeCommitmentValue; }

            set { SetProperty(ref MaximumVolumeCommitmentValue, value); }

        }

        /// <summary>
        /// Gets or sets the payment terms (days).
        /// </summary>
        private int PaymentTermsDaysValue = 30;

        public int PaymentTermsDays

        {

            get { return this.PaymentTermsDaysValue; }

            set { SetProperty(ref PaymentTermsDaysValue, value); }

        }
    }
}
