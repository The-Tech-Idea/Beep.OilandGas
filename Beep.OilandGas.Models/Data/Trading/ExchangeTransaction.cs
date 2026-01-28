using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public partial class ExchangeTransaction : ModelEntityBase
    {
        private System.String TransactionIdValue;
        public System.String TransactionId
        {
            get { return this.TransactionIdValue; }
            set { SetProperty(ref TransactionIdValue, value); }
        }

        private System.String ContractIdValue;
        public System.String ContractId
        {
            get { return this.ContractIdValue; }
            set { SetProperty(ref ContractIdValue, value); }
        }

        private System.DateTime TransactionDateValue;
        public System.DateTime TransactionDate
        {
            get { return this.TransactionDateValue; }
            set { SetProperty(ref TransactionDateValue, value); }
        }

        private System.Decimal ReceiptVolumeValue;
        public System.Decimal ReceiptVolume
        {
            get { return this.ReceiptVolumeValue; }
            set { SetProperty(ref ReceiptVolumeValue, value); }
        }

        private System.Decimal ReceiptPriceValue;
        public System.Decimal ReceiptPrice
        {
            get { return this.ReceiptPriceValue; }
            set { SetProperty(ref ReceiptPriceValue, value); }
        }

        private System.String ReceiptLocationValue;
        public System.String ReceiptLocation
        {
            get { return this.ReceiptLocationValue; }
            set { SetProperty(ref ReceiptLocationValue, value); }
        }

        private System.Decimal DeliveryVolumeValue;
        public System.Decimal DeliveryVolume
        {
            get { return this.DeliveryVolumeValue; }
            set { SetProperty(ref DeliveryVolumeValue, value); }
        }

        private System.Decimal DeliveryPriceValue;
        public System.Decimal DeliveryPrice
        {
            get { return this.DeliveryPriceValue; }
            set { SetProperty(ref DeliveryPriceValue, value); }
        }

        private System.String DeliveryLocationValue;
        public System.String DeliveryLocation
        {
            get { return this.DeliveryLocationValue; }
            set { SetProperty(ref DeliveryLocationValue, value); }
        }

        public System.Decimal ReceiptValue => ReceiptVolume * ReceiptPrice;
        public System.Decimal DeliveryValue => DeliveryVolume * DeliveryPrice;
        public System.Decimal NetValue => ReceiptValue - DeliveryValue;
    }
}
