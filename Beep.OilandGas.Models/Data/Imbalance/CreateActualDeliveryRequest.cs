using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Imbalance
{
    public class CreateActualDeliveryRequest : ModelEntityBase
    {
        private string NominationIdValue;

        public string NominationId

        {

            get { return this.NominationIdValue; }

            set { SetProperty(ref NominationIdValue, value); }

        }
        private DateTime DeliveryDateValue;

        public DateTime DeliveryDate

        {

            get { return this.DeliveryDateValue; }

            set { SetProperty(ref DeliveryDateValue, value); }

        }
        private decimal ActualVolumeValue;

        public decimal ActualVolume

        {

            get { return this.ActualVolumeValue; }

            set { SetProperty(ref ActualVolumeValue, value); }

        }
        private string DeliveryPointValue;

        public string DeliveryPoint

        {

            get { return this.DeliveryPointValue; }

            set { SetProperty(ref DeliveryPointValue, value); }

        }
        private string AllocationMethodValue;

        public string AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }
        private string RunTicketNumberValue;

        public string RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
    }
}
