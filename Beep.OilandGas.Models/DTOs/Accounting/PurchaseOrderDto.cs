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
}

