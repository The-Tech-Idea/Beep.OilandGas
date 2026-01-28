using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class ExchangeDeliveryPoint : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the delivery point identifier.
        /// </summary>
        private string DeliveryPointIdValue = string.Empty;

        public string DeliveryPointId

        {

            get { return this.DeliveryPointIdValue; }

            set { SetProperty(ref DeliveryPointIdValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery point name.
        /// </summary>
        private string DeliveryPointNameValue = string.Empty;

        public string DeliveryPointName

        {

            get { return this.DeliveryPointNameValue; }

            set { SetProperty(ref DeliveryPointNameValue, value); }

        }

        /// <summary>
        /// Gets or sets the location.
        /// </summary>
        private string LocationValue = string.Empty;

        public string Location

        {

            get { return this.LocationValue; }

            set { SetProperty(ref LocationValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a receipt point.
        /// </summary>
        private bool IsReceiptPointValue;

        public bool IsReceiptPoint

        {

            get { return this.IsReceiptPointValue; }

            set { SetProperty(ref IsReceiptPointValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a delivery point.
        /// </summary>
        private bool IsDeliveryPointValue;

        public bool IsDeliveryPoint

        {

            get { return this.IsDeliveryPointValue; }

            set { SetProperty(ref IsDeliveryPointValue, value); }

        }
    }
}
