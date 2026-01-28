using System;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class DeliveryTerms : ModelEntityBase
    {
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
        /// Gets or sets the delivery method (pipeline, truck, etc.).
        /// </summary>
        private string DeliveryMethodValue = string.Empty;

        public string DeliveryMethod

        {

            get { return this.DeliveryMethodValue; }

            set { SetProperty(ref DeliveryMethodValue, value); }

        }

        /// <summary>
        /// Gets or sets the delivery frequency.
        /// </summary>
        private string DeliveryFrequencyValue = "Daily";

        public string DeliveryFrequency

        {

            get { return this.DeliveryFrequencyValue; }

            set { SetProperty(ref DeliveryFrequencyValue, value); }

        }

        /// <summary>
        /// Gets or sets whether title transfers at delivery point.
        /// </summary>
        private bool TitleTransferAtDeliveryPointValue = true;

        public bool TitleTransferAtDeliveryPoint

        {

            get { return this.TitleTransferAtDeliveryPointValue; }

            set { SetProperty(ref TitleTransferAtDeliveryPointValue, value); }

        }
    }
}
