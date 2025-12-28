namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class InvoiceDto
    {
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceDue { get; set; }
        public string Status { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
    }

    public class CreateInvoiceRequest
    {
        public string InvoiceNumber { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Subtotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
    }

    public class InvoiceResponse : InvoiceDto
    {
    }
}

