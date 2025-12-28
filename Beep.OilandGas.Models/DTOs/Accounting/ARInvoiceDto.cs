namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class ARInvoiceDto
    {
        public string ArInvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceDue { get; set; }
        public string Status { get; set; }
    }

    public class CreateARInvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ARInvoiceResponse : ARInvoiceDto
    {
    }
}

