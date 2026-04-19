using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class TransportationAgreement : ModelEntityBase
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
        /// Gets or sets the carrier or pipeline company.
        /// </summary>
        private string CarrierValue = string.Empty;

        public string Carrier

        {

            get { return this.CarrierValue; }

            set { SetProperty(ref CarrierValue, value); }

        }

        /// <summary>
        /// Gets or sets the origin point.
        /// </summary>
        private string OriginPointValue = string.Empty;

        public string OriginPoint

        {

            get { return this.OriginPointValue; }

            set { SetProperty(ref OriginPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the destination point.
        /// </summary>
        private string DestinationPointValue = string.Empty;

        public string DestinationPoint

        {

            get { return this.DestinationPointValue; }

            set { SetProperty(ref DestinationPointValue, value); }

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
        /// Gets or sets the tariff rate per barrel.
        /// </summary>
        private decimal TariffRateValue;

        public decimal TariffRate

        {

            get { return this.TariffRateValue; }

            set { SetProperty(ref TariffRateValue, value); }

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
        /// Gets or sets the maximum volume capacity (barrels per day).
        /// </summary>
        private decimal? MaximumCapacityValue;

        public decimal? MaximumCapacity

        {

            get { return this.MaximumCapacityValue; }

            set { SetProperty(ref MaximumCapacityValue, value); }

        }
    }
}
