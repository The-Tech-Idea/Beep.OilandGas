namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class PurchaseOrderDto
    {
        public string PurchaseOrderId { get; set; }
        public string PoNumber { get; set; }
        public string VendorBaId { get; set; }
        public DateTime? PoDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
    }

    public class CreatePurchaseOrderRequest
    {
        public string PoNumber { get; set; }
        public string VendorBaId { get; set; }
        public DateTime PoDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Description { get; set; }
    }

    public class PurchaseOrderResponse : PurchaseOrderDto
    {
    }

    public class UpdatePurchaseOrderRequest
    {
        public string PurchaseOrderId { get; set; }
        public string PoNumber { get; set; }
        public string VendorBaId { get; set; }
        public DateTime? PoDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class CreatePOReceiptRequest
    {
        public string PurchaseOrderId { get; set; }
        public string PoLineItemId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public decimal QuantityReceived { get; set; }
        public string ReceiptNumber { get; set; }
        public string Description { get; set; }
    }

    public class POApprovalResult
    {
        public string PurchaseOrderId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    public class POStatusSummary
    {
        public string PurchaseOrderId { get; set; }
        public string PoNumber { get; set; }
        public string Status { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? ReceivedAmount { get; set; }
        public decimal? PendingAmount { get; set; }
        public int TotalLineItems { get; set; }
        public int ReceivedLineItems { get; set; }
        public int PendingLineItems { get; set; }
        public bool IsFullyReceived { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}

