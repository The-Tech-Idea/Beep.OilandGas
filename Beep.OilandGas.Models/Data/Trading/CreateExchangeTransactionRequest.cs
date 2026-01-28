using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public class CreateExchangeTransactionRequest : ModelEntityBase
    {
        private string ExchangeContractIdValue;

        public string ExchangeContractId

        {

            get { return this.ExchangeContractIdValue; }

            set { SetProperty(ref ExchangeContractIdValue, value); }

        }
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private decimal ReceiptVolumeValue;

        public decimal ReceiptVolume

        {

            get { return this.ReceiptVolumeValue; }

            set { SetProperty(ref ReceiptVolumeValue, value); }

        }
        private decimal ReceiptPriceValue;

        public decimal ReceiptPrice

        {

            get { return this.ReceiptPriceValue; }

            set { SetProperty(ref ReceiptPriceValue, value); }

        }
        private string ReceiptLocationValue;

        public string ReceiptLocation

        {

            get { return this.ReceiptLocationValue; }

            set { SetProperty(ref ReceiptLocationValue, value); }

        }
        private decimal DeliveryVolumeValue;

        public decimal DeliveryVolume

        {

            get { return this.DeliveryVolumeValue; }

            set { SetProperty(ref DeliveryVolumeValue, value); }

        }
        private decimal DeliveryPriceValue;

        public decimal DeliveryPrice

        {

            get { return this.DeliveryPriceValue; }

            set { SetProperty(ref DeliveryPriceValue, value); }

        }
        private string DeliveryLocationValue;

        public string DeliveryLocation

        {

            get { return this.DeliveryLocationValue; }

            set { SetProperty(ref DeliveryLocationValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
