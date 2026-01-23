namespace Beep.OilandGas.Models.Data.Accounting
{
    public class PurchaseOrder : ModelEntityBase
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
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

        }
        private DateTime? PoDateValue;

        public DateTime? PoDate

        {

            get { return this.PoDateValue; }

            set { SetProperty(ref PoDateValue, value); }

        }
        private DateTime? ExpectedDeliveryDateValue;

        public DateTime? ExpectedDeliveryDate

        {

            get { return this.ExpectedDeliveryDateValue; }

            set { SetProperty(ref ExpectedDeliveryDateValue, value); }

        }
        private decimal? TotalAmountValue;

        public decimal? TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class CreatePurchaseOrderRequest : ModelEntityBase
    {
        private string PoNumberValue;

        public string PoNumber

        {

            get { return this.PoNumberValue; }

            set { SetProperty(ref PoNumberValue, value); }

        }
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

        }
        private DateTime PoDateValue;

        public DateTime PoDate

        {

            get { return this.PoDateValue; }

            set { SetProperty(ref PoDateValue, value); }

        }
        private DateTime? ExpectedDeliveryDateValue;

        public DateTime? ExpectedDeliveryDate

        {

            get { return this.ExpectedDeliveryDateValue; }

            set { SetProperty(ref ExpectedDeliveryDateValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class PurchaseOrderResponse : PurchaseOrder
    {
    }

    public class UpdatePurchaseOrderRequest : ModelEntityBase
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
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

        }
        private DateTime? PoDateValue;

        public DateTime? PoDate

        {

            get { return this.PoDateValue; }

            set { SetProperty(ref PoDateValue, value); }

        }
        private DateTime? ExpectedDeliveryDateValue;

        public DateTime? ExpectedDeliveryDate

        {

            get { return this.ExpectedDeliveryDateValue; }

            set { SetProperty(ref ExpectedDeliveryDateValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

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

    public class POApprovalResult : ModelEntityBase
    {
        private string PurchaseOrderIdValue;

        public string PurchaseOrderId

        {

            get { return this.PurchaseOrderIdValue; }

            set { SetProperty(ref PurchaseOrderIdValue, value); }

        }
        private bool IsApprovedValue;

        public bool IsApproved

        {

            get { return this.IsApprovedValue; }

            set { SetProperty(ref IsApprovedValue, value); }

        }
        private string ApproverIdValue;

        public string ApproverId

        {

            get { return this.ApproverIdValue; }

            set { SetProperty(ref ApproverIdValue, value); }

        }
        private DateTime ApprovalDateValue = DateTime.UtcNow;

        public DateTime ApprovalDate

        {

            get { return this.ApprovalDateValue; }

            set { SetProperty(ref ApprovalDateValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string CommentsValue;

        public string Comments

        {

            get { return this.CommentsValue; }

            set { SetProperty(ref CommentsValue, value); }

        }
    }

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







