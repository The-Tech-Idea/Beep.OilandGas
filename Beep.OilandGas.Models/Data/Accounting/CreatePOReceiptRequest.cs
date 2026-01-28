
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class CreatePOReceiptRequest : ModelEntityBase
    {
        private string PurchaseOrderIdValue;

        public string PurchaseOrderId

        {

            get { return this.PurchaseOrderIdValue; }

            set { SetProperty(ref PurchaseOrderIdValue, value); }

        }
        private string PoLineItemIdValue;

        public string PoLineItemId

        {

            get { return this.PoLineItemIdValue; }

            set { SetProperty(ref PoLineItemIdValue, value); }

        }
        private DateTime ReceiptDateValue;

        public DateTime ReceiptDate

        {

            get { return this.ReceiptDateValue; }

            set { SetProperty(ref ReceiptDateValue, value); }

        }
        private decimal QuantityReceivedValue;

        public decimal QuantityReceived

        {

            get { return this.QuantityReceivedValue; }

            set { SetProperty(ref QuantityReceivedValue, value); }

        }
        private string ReceiptNumberValue;

        public string ReceiptNumber

        {

            get { return this.ReceiptNumberValue; }

            set { SetProperty(ref ReceiptNumberValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }
}
