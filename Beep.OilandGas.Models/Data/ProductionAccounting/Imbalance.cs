using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class Imbalance : ModelEntityBase
    {
        private string ImbalanceIdValue = string.Empty;

        public string ImbalanceId

        {

            get { return this.ImbalanceIdValue; }

            set { SetProperty(ref ImbalanceIdValue, value); }

        }
        private string LeaseIdValue = string.Empty;

        public string LeaseId

        {

            get { return this.LeaseIdValue; }

            set { SetProperty(ref LeaseIdValue, value); }

        }
        private DateTime ImbalanceDateValue;

        public DateTime ImbalanceDate

        {

            get { return this.ImbalanceDateValue; }

            set { SetProperty(ref ImbalanceDateValue, value); }

        }
        private decimal ProductionAvailValue;

        public decimal ProductionAvail

        {

            get { return this.ProductionAvailValue; }

            set { SetProperty(ref ProductionAvailValue, value); }

        }
        private decimal NominatedVolumeValue;

        public decimal NominatedVolume

        {

            get { return this.NominatedVolumeValue; }

            set { SetProperty(ref NominatedVolumeValue, value); }

        }
        private decimal ActualDeliveredValue;

        public decimal ActualDelivered

        {

            get { return this.ActualDeliveredValue; }

            set { SetProperty(ref ActualDeliveredValue, value); }

        }
        private decimal ImbalanceVolumeValue;

        public decimal ImbalanceVolume

        {

            get { return this.ImbalanceVolumeValue; }

            set { SetProperty(ref ImbalanceVolumeValue, value); }

        }
        private string ImbalanceTypeValue = string.Empty;

        public string ImbalanceType

        {

            get { return this.ImbalanceTypeValue; }

            set { SetProperty(ref ImbalanceTypeValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }
}
