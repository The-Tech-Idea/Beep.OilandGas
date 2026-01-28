using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public class DeliveryInformation : ModelEntityBase
    {
        private DateTime DeliveryDateValue;

        public DateTime DeliveryDate

        {

            get { return this.DeliveryDateValue; }

            set { SetProperty(ref DeliveryDateValue, value); }

        }
        private string DeliveryPointValue = string.Empty;

        public string DeliveryPoint

        {

            get { return this.DeliveryPointValue; }

            set { SetProperty(ref DeliveryPointValue, value); }

        }
        private string DeliveryMethodValue = string.Empty;

        public string DeliveryMethod

        {

            get { return this.DeliveryMethodValue; }

            set { SetProperty(ref DeliveryMethodValue, value); }

        }
        private bool TitleTransferredAtDeliveryPointValue = true;

        public bool TitleTransferredAtDeliveryPoint

        {

            get { return this.TitleTransferredAtDeliveryPointValue; }

            set { SetProperty(ref TitleTransferredAtDeliveryPointValue, value); }

        }
    }
}
