using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Imbalance
{
    public class CreateNominationRequest : ModelEntityBase
    {
        private DateTime PeriodStartValue;

        public DateTime PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal NominatedVolumeValue;

        public decimal NominatedVolume

        {

            get { return this.NominatedVolumeValue; }

            set { SetProperty(ref NominatedVolumeValue, value); }

        }
        private List<string> DeliveryPointsValue = new();

        public List<string> DeliveryPoints

        {

            get { return this.DeliveryPointsValue; }

            set { SetProperty(ref DeliveryPointsValue, value); }

        }
    }
}
