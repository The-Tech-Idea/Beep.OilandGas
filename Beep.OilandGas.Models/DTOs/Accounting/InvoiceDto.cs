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

    public class UpdateInvoiceRequest
    {
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? Subtotal { get; set; }
        public decimal? TaxAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class CreateInvoicePaymentRequest
    {
        public string InvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
    }

    public class InvoiceApprovalResult
    {
        public string InvoiceId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    public class InvoiceAgingSummary
    {
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceDue { get; set; }
        public int DaysPastDue { get; set; }
        public string AgingBucket { get; set; } // Current, 1-30, 31-60, 61-90, 90+
    }

    public class InvoicePaymentStatus
    {
        public string InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceDue { get; set; }
        public string PaymentStatus { get; set; } // Paid, Partial, Unpaid, Overpaid
        public int NumberOfPayments { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysPastDue { get; set; }
    }
}




