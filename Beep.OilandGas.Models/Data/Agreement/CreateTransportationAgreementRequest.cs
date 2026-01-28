using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Agreement
{
    public class CreateTransportationAgreementRequest : ModelEntityBase
    {
        private string CarrierBaIdValue;

        public string CarrierBaId

        {

            get { return this.CarrierBaIdValue; }

            set { SetProperty(ref CarrierBaIdValue, value); }

        }
        private string OriginPointValue;

        public string OriginPoint

        {

            get { return this.OriginPointValue; }

            set { SetProperty(ref OriginPointValue, value); }

        }
        private string DestinationPointValue;

        public string DestinationPoint

        {

            get { return this.DestinationPointValue; }

            set { SetProperty(ref DestinationPointValue, value); }

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
        private decimal TariffRateValue;

        public decimal TariffRate

        {

            get { return this.TariffRateValue; }

            set { SetProperty(ref TariffRateValue, value); }

        }
        private decimal? MinimumVolumeCommitmentValue;

        public decimal? MinimumVolumeCommitment

        {

            get { return this.MinimumVolumeCommitmentValue; }

            set { SetProperty(ref MinimumVolumeCommitmentValue, value); }

        }
        private decimal? MaximumCapacityValue;

        public decimal? MaximumCapacity

        {

            get { return this.MaximumCapacityValue; }

            set { SetProperty(ref MaximumCapacityValue, value); }

        }
    }
}
