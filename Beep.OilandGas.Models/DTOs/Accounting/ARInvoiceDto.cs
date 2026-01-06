using System;
using System.Collections.Generic;

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

    public class UpdateARInvoiceRequest
    {
        public string ArInvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
    }

    public class CreateARPaymentRequest
    {
        public string CustomerBaId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal PaymentAmount { get; set; }
        public string PaymentMethod { get; set; } // Check, ACH, Wire, Cash
        public string CheckNumber { get; set; }
        public string ReferenceNumber { get; set; }
        public string Description { get; set; }
    }

    public class CreateARCreditMemoRequest
    {
        public string ArInvoiceId { get; set; }
        public string CreditMemoNumber { get; set; }
        public DateTime CreditMemoDate { get; set; }
        public decimal CreditAmount { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
    }

    public class ARApprovalResult
    {
        public string ArInvoiceId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string Comments { get; set; }
    }

    public class ARAgingSummary
    {
        public string ArInvoiceId { get; set; }
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

    public class CustomerSummary
    {
        public string CustomerBaId { get; set; }
        public decimal TotalInvoices { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public int NumberOfInvoices { get; set; }
        public int NumberOfOverdueInvoices { get; set; }
        public decimal AverageDaysToPay { get; set; }
    }

    public class PaymentApplicationRequest
    {
        public string PaymentId { get; set; }
        public List<PaymentApplicationItem> Applications { get; set; } = new List<PaymentApplicationItem>();
    }

    public class PaymentApplicationItem
    {
        public string InvoiceId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PaymentApplicationResult
    {
        public string PaymentId { get; set; }
        public decimal TotalApplied { get; set; }
        public int SuccessfulApplications { get; set; }
        public int FailedApplications { get; set; }
        public List<string> AppliedInvoiceIds { get; set; } = new List<string>();
        public List<string> FailedInvoiceIds { get; set; } = new List<string>();
    }
}




