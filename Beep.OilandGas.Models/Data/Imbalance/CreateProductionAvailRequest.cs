using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Imbalance
{
    public class CreateProductionAvailRequest : ModelEntityBase
    {
        private string PropertyIdValue;

        public string PropertyId

        {

            get { return this.PropertyIdValue; }

            set { SetProperty(ref PropertyIdValue, value); }

        }
        private DateTime AvailDateValue;

        public DateTime AvailDate

        {

            get { return this.AvailDateValue; }

            set { SetProperty(ref AvailDateValue, value); }

        }
        private decimal EstimatedVolumeValue;

        public decimal EstimatedVolume

        {

            get { return this.EstimatedVolumeValue; }

            set { SetProperty(ref EstimatedVolumeValue, value); }

        }
        private decimal? AvailableForDeliveryValue;

        public decimal? AvailableForDelivery

        {

            get { return this.AvailableForDeliveryValue; }

            set { SetProperty(ref AvailableForDeliveryValue, value); }

        }
    }
}
