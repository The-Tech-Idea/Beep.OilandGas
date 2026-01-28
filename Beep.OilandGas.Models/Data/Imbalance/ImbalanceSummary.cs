using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Imbalance
{
    public class ImbalanceSummary : ModelEntityBase
    {
        private string PeriodStartValue;

        public string PeriodStart

        {

            get { return this.PeriodStartValue; }

            set { SetProperty(ref PeriodStartValue, value); }

        }
        private string PeriodEndValue;

        public string PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal TotalNominatedVolumeValue;

        public decimal TotalNominatedVolume

        {

            get { return this.TotalNominatedVolumeValue; }

            set { SetProperty(ref TotalNominatedVolumeValue, value); }

        }
        private decimal TotalActualVolumeValue;

        public decimal TotalActualVolume

        {

            get { return this.TotalActualVolumeValue; }

            set { SetProperty(ref TotalActualVolumeValue, value); }

        }
        private decimal TotalImbalanceAmountValue;

        public decimal TotalImbalanceAmount

        {

            get { return this.TotalImbalanceAmountValue; }

            set { SetProperty(ref TotalImbalanceAmountValue, value); }

        }
        private int ImbalanceCountValue;

        public int ImbalanceCount

        {

            get { return this.ImbalanceCountValue; }

            set { SetProperty(ref ImbalanceCountValue, value); }

        }
        private int BalancedCountValue;

        public int BalancedCount

        {

            get { return this.BalancedCountValue; }

            set { SetProperty(ref BalancedCountValue, value); }

        }
        private int OverDeliveredCountValue;

        public int OverDeliveredCount

        {

            get { return this.OverDeliveredCountValue; }

            set { SetProperty(ref OverDeliveredCountValue, value); }

        }
        private int UnderDeliveredCountValue;

        public int UnderDeliveredCount

        {

            get { return this.UnderDeliveredCountValue; }

            set { SetProperty(ref UnderDeliveredCountValue, value); }

        }
    }
}
