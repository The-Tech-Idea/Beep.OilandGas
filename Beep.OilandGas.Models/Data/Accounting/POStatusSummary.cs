
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class POStatusSummary : ModelEntityBase
    {
        private string PurchaseOrderIdValue;

        public string PurchaseOrderId

        {

            get { return this.PurchaseOrderIdValue; }

            set { SetProperty(ref PurchaseOrderIdValue, value); }

        }
        private string PoNumberValue;

        public string PoNumber

        {

            get { return this.PoNumberValue; }

            set { SetProperty(ref PoNumberValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private decimal? TotalAmountValue;

        public decimal? TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private decimal? ReceivedAmountValue;

        public decimal? ReceivedAmount

        {

            get { return this.ReceivedAmountValue; }

            set { SetProperty(ref ReceivedAmountValue, value); }

        }
        private decimal? PendingAmountValue;

        public decimal? PendingAmount

        {

            get { return this.PendingAmountValue; }

            set { SetProperty(ref PendingAmountValue, value); }

        }
        private int TotalLineItemsValue;

        public int TotalLineItems

        {

            get { return this.TotalLineItemsValue; }

            set { SetProperty(ref TotalLineItemsValue, value); }

        }
        private int ReceivedLineItemsValue;

        public int ReceivedLineItems

        {

            get { return this.ReceivedLineItemsValue; }

            set { SetProperty(ref ReceivedLineItemsValue, value); }

        }
        private int PendingLineItemsValue;

        public int PendingLineItems

        {

            get { return this.PendingLineItemsValue; }

            set { SetProperty(ref PendingLineItemsValue, value); }

        }
        private bool IsFullyReceivedValue;

        public bool IsFullyReceived

        {

            get { return this.IsFullyReceivedValue; }

            set { SetProperty(ref IsFullyReceivedValue, value); }

        }
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }
        private DateTime? ApprovalDateValue;

        public DateTime? ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }
    }
}
