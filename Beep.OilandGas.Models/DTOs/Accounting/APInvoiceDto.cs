namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class APInvoiceDto
    {
        public string ApInvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string VendorBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceDue { get; set; }
        public string Status { get; set; }
    }

    public class CreateAPInvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public string VendorBaId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class APInvoiceResponse : APInvoiceDto
    {
    }
}

