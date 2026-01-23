using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{


    public class SalesTransactionResponse : ModelEntityBase
    {
        private string SalesTransactionIdValue;

        public string SalesTransactionId

        {

            get { return this.SalesTransactionIdValue; }

            set { SetProperty(ref SalesTransactionIdValue, value); }

        }
        private string RunTicketIdValue;

        public string RunTicketId

        {

            get { return this.RunTicketIdValue; }

            set { SetProperty(ref RunTicketIdValue, value); }

        }
        private string? SalesAgreementIdValue;

        public string? SalesAgreementId

        {

            get { return this.SalesAgreementIdValue; }

            set { SetProperty(ref SalesAgreementIdValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private DateTime SalesDateValue;

        public DateTime SalesDate

        {

            get { return this.SalesDateValue; }

            set { SetProperty(ref SalesDateValue, value); }

        }
        private decimal NetVolumeValue;

        public decimal NetVolume

        {

            get { return this.NetVolumeValue; }

            set { SetProperty(ref NetVolumeValue, value); }

        }
        private decimal PricePerBarrelValue;

        public decimal PricePerBarrel

        {

            get { return this.PricePerBarrelValue; }

            set { SetProperty(ref PricePerBarrelValue, value); }

        }
        private decimal TotalAmountValue;

        public decimal TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private decimal? TotalCostsValue;

        public decimal? TotalCosts

        {

            get { return this.TotalCostsValue; }

            set { SetProperty(ref TotalCostsValue, value); }

        }
        private decimal? TotalTaxesValue;

        public decimal? TotalTaxes

        {

            get { return this.TotalTaxesValue; }

            set { SetProperty(ref TotalTaxesValue, value); }

        }
        private decimal? NetRevenueValue;

        public decimal? NetRevenue

        {

            get { return this.NetRevenueValue; }

            set { SetProperty(ref NetRevenueValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private string ApprovalStatusValue;

        public string ApprovalStatus

        {

            get { return this.ApprovalStatusValue; }

            set { SetProperty(ref ApprovalStatusValue, value); }

        }
    }

    public class CreateReceivableRequest : ModelEntityBase
    {
        private string SalesTransactionIdValue;

        public string SalesTransactionId

        {

            get { return this.SalesTransactionIdValue; }

            set { SetProperty(ref SalesTransactionIdValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private string? InvoiceNumberValue;

        public string? InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private DateTime InvoiceDateValue;

        public DateTime InvoiceDate

        {

            get { return this.InvoiceDateValue; }

            set { SetProperty(ref InvoiceDateValue, value); }

        }
        private DateTime DueDateValue;

        public DateTime DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal OriginalAmountValue;

        public decimal OriginalAmount

        {

            get { return this.OriginalAmountValue; }

            set { SetProperty(ref OriginalAmountValue, value); }

        }
    }

    public class ReceivableResponse : ModelEntityBase
    {
        private string ReceivableIdValue;

        public string ReceivableId

        {

            get { return this.ReceivableIdValue; }

            set { SetProperty(ref ReceivableIdValue, value); }

        }
        private string SalesTransactionIdValue;

        public string SalesTransactionId

        {

            get { return this.SalesTransactionIdValue; }

            set { SetProperty(ref SalesTransactionIdValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private string? InvoiceNumberValue;

        public string? InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private DateTime InvoiceDateValue;

        public DateTime InvoiceDate

        {

            get { return this.InvoiceDateValue; }

            set { SetProperty(ref InvoiceDateValue, value); }

        }
        private DateTime DueDateValue;

        public DateTime DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal OriginalAmountValue;

        public decimal OriginalAmount

        {

            get { return this.OriginalAmountValue; }

            set { SetProperty(ref OriginalAmountValue, value); }

        }
        private decimal AmountPaidValue;

        public decimal AmountPaid

        {

            get { return this.AmountPaidValue; }

            set { SetProperty(ref AmountPaidValue, value); }

        }
        private decimal OutstandingBalanceValue;

        public decimal OutstandingBalance

        {

            get { return this.OutstandingBalanceValue; }

            set { SetProperty(ref OutstandingBalanceValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    public class SalesApprovalResult : ModelEntityBase
    {
        private string SalesTransactionIdValue;

        public string SalesTransactionId

        {

            get { return this.SalesTransactionIdValue; }

            set { SetProperty(ref SalesTransactionIdValue, value); }

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
        private string? RejectionReasonValue;

        public string? RejectionReason

        {

            get { return this.RejectionReasonValue; }

            set { SetProperty(ref RejectionReasonValue, value); }

        }
    }

    public class SalesReconciliationRequest : ModelEntityBase
    {
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private string? PropertyOrLeaseIdValue;

        public string? PropertyOrLeaseId

        {

            get { return this.PropertyOrLeaseIdValue; }

            set { SetProperty(ref PropertyOrLeaseIdValue, value); }

        }
        private string? CustomerBaIdValue;

        public string? CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
    }

    public class SalesReconciliationResult : ModelEntityBase
    {
        private string ReconciliationIdValue;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private DateTime StartDateValue;

        public DateTime StartDate

        {

            get { return this.StartDateValue; }

            set { SetProperty(ref StartDateValue, value); }

        }
        private DateTime EndDateValue;

        public DateTime EndDate

        {

            get { return this.EndDateValue; }

            set { SetProperty(ref EndDateValue, value); }

        }
        private decimal TotalProductionVolumeValue;

        public decimal TotalProductionVolume

        {

            get { return this.TotalProductionVolumeValue; }

            set { SetProperty(ref TotalProductionVolumeValue, value); }

        }
        private decimal TotalSalesVolumeValue;

        public decimal TotalSalesVolume

        {

            get { return this.TotalSalesVolumeValue; }

            set { SetProperty(ref TotalSalesVolumeValue, value); }

        }
        private decimal VolumeDifferenceValue;

        public decimal VolumeDifference

        {

            get { return this.VolumeDifferenceValue; }

            set { SetProperty(ref VolumeDifferenceValue, value); }

        }
        private decimal TotalProductionRevenueValue;

        public decimal TotalProductionRevenue

        {

            get { return this.TotalProductionRevenueValue; }

            set { SetProperty(ref TotalProductionRevenueValue, value); }

        }
        private decimal TotalSalesRevenueValue;

        public decimal TotalSalesRevenue

        {

            get { return this.TotalSalesRevenueValue; }

            set { SetProperty(ref TotalSalesRevenueValue, value); }

        }
        private decimal RevenueDifferenceValue;

        public decimal RevenueDifference

        {

            get { return this.RevenueDifferenceValue; }

            set { SetProperty(ref RevenueDifferenceValue, value); }

        }
        private List<SalesReconciliationIssue> IssuesValue = new List<SalesReconciliationIssue>();

        public List<SalesReconciliationIssue> Issues

        {

            get { return this.IssuesValue; }

            set { SetProperty(ref IssuesValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
    }

    public class SalesReconciliationIssue : ModelEntityBase
    {
        private string IssueTypeValue;

        public string IssueType

        {

            get { return this.IssueTypeValue; }

            set { SetProperty(ref IssueTypeValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private string ReferenceIdValue;

        public string ReferenceId

        {

            get { return this.ReferenceIdValue; }

            set { SetProperty(ref ReferenceIdValue, value); }

        }
        private decimal? AmountValue;

        public decimal? Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private decimal? VolumeValue;

        public decimal? Volume

        {

            get { return this.VolumeValue; }

            set { SetProperty(ref VolumeValue, value); }

        }
    }
}








