using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.DTOs.Accounting
{
    public class CreateSalesTransactionRequest
    {
        public string RunTicketId { get; set; }
        public string? SalesAgreementId { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime SalesDate { get; set; }
        public decimal NetVolume { get; set; }
        public decimal PricePerBarrel { get; set; }
        public decimal? TotalCosts { get; set; }
        public decimal? TotalTaxes { get; set; }
        public string? Status { get; set; }
    }

    public class SalesTransactionResponse
    {
        public string SalesTransactionId { get; set; }
        public string RunTicketId { get; set; }
        public string? SalesAgreementId { get; set; }
        public string CustomerBaId { get; set; }
        public DateTime SalesDate { get; set; }
        public decimal NetVolume { get; set; }
        public decimal PricePerBarrel { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal? TotalCosts { get; set; }
        public decimal? TotalTaxes { get; set; }
        public decimal? NetRevenue { get; set; }
        public string Status { get; set; }
        public string ApprovalStatus { get; set; }
    }

    public class CreateReceivableRequest
    {
        public string SalesTransactionId { get; set; }
        public string CustomerBaId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal OriginalAmount { get; set; }
    }

    public class ReceivableResponse
    {
        public string ReceivableId { get; set; }
        public string SalesTransactionId { get; set; }
        public string CustomerBaId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal OutstandingBalance { get; set; }
        public string Status { get; set; }
    }

    public class SalesApprovalResult
    {
        public string SalesTransactionId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class SalesReconciliationRequest
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? PropertyOrLeaseId { get; set; }
        public string? CustomerBaId { get; set; }
    }

    public class SalesReconciliationResult
    {
        public string ReconciliationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalProductionVolume { get; set; }
        public decimal TotalSalesVolume { get; set; }
        public decimal VolumeDifference { get; set; }
        public decimal TotalProductionRevenue { get; set; }
        public decimal TotalSalesRevenue { get; set; }
        public decimal RevenueDifference { get; set; }
        public List<SalesReconciliationIssue> Issues { get; set; } = new List<SalesReconciliationIssue>();
        public bool IsReconciled { get; set; }
    }

    public class SalesReconciliationIssue
    {
        public string IssueType { get; set; }
        public string Description { get; set; }
        public string ReferenceId { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Volume { get; set; }
    }
}

