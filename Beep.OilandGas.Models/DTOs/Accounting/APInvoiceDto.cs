using System;

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

    public class UpdateAPInvoiceRequest
    {
        public string ApInvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string VendorBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class CreateAPPaymentRequest
    {
        public string ApInvoiceId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; } // Check, ACH, Wire
        public string CheckNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
    }

    public class CreateAPCreditMemoRequest
    {
        public string ApInvoiceId { get; set; }
        public string CreditMemoNumber { get; set; }
        public DateTime CreditMemoDate { get; set; }
        public decimal CreditAmount { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
    }

    public class APApprovalResult
    {
        public string ApInvoiceId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    public class APAgingSummary
    {
        public string ApInvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string VendorBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public decimal? BalanceDue { get; set; }
        public int DaysPastDue { get; set; }
        public string AgingBucket { get; set; } // Current, 1-30, 31-60, 61-90, 90+
    }

    public class VendorSummary
    {
        public string VendorBaId { get; set; }
        public decimal TotalInvoices { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int NumberOfInvoices { get; set; }
        public int NumberOfOverdueInvoices { get; set; }
        public decimal AverageDaysToPay { get; set; }
    }

    public class PaymentBatchRequest
    {
        public List<string> PaymentIds { get; set; } = new List<string>();
        public DateTime BatchDate { get; set; }
        public string PaymentMethod { get; set; }
        public string BatchDescription { get; set; }
    }

    public class PaymentBatchResult
    {
        public string BatchId { get; set; } = Guid.NewGuid().ToString();
        public DateTime BatchDate { get; set; }
        public int TotalPayments { get; set; }
        public decimal TotalAmount { get; set; }
        public int SuccessfulPayments { get; set; }
        public int FailedPayments { get; set; }
        public List<string> ProcessedPaymentIds { get; set; } = new List<string>();
        public List<string> FailedPaymentIds { get; set; } = new List<string>();
    }
}

