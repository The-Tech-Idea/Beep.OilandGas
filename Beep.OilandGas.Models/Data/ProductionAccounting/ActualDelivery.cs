using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ActualDelivery : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delivery identifier.
        /// </summary>
        private string DeliveryIdValue = string.Empty;

        public string DeliveryId

        {

            get { return this.DeliveryIdValue; }

            set { SetProperty(ref DeliveryIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery date.
        /// </summary>
        private DateTime DeliveryDateValue;

        public DateTime DeliveryDate

        {

            get { return this.DeliveryDateValue; }

            set { SetProperty(ref DeliveryDateValue, value); }

        }

        /// <summary>
        /// Gets or sets the actual volume delivered in barrels.
        /// </summary>
        private decimal ActualVolumeValue;

        public decimal ActualVolume

        {

            get { return this.ActualVolumeValue; }

            set { SetProperty(ref ActualVolumeValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery point.
        /// </summary>
        private string DeliveryPointValue = string.Empty;

        public string DeliveryPoint

        {

            get { return this.DeliveryPointValue; }

            set { SetProperty(ref DeliveryPointValue, value); }

        }

        /// <summary>
        /// Gets or sets the allocation method used.
        /// </summary>
        private string AllocationMethodValue = string.Empty;

        public string AllocationMethod

        {

            get { return this.AllocationMethodValue; }

            set { SetProperty(ref AllocationMethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the run ticket reference.
        /// </summary>
        private string? RunTicketNumberValue;

        public string? RunTicketNumber

        {

            get { return this.RunTicketNumberValue; }

            set { SetProperty(ref RunTicketNumberValue, value); }

        }
    }
}
