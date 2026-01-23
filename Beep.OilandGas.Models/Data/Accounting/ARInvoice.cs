using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class ARInvoice : ModelEntityBase
    {
        private string ArInvoiceIdValue;

        public string ArInvoiceId

        {

            get { return this.ArInvoiceIdValue; }

            set { SetProperty(ref ArInvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private DateTime? InvoiceDateValue;

        public DateTime? InvoiceDate

        {

            get { return this.InvoiceDateValue; }

            set { SetProperty(ref InvoiceDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? TotalAmountValue;

        public decimal? TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private decimal? PaidAmountValue;

        public decimal? PaidAmount

        {

            get { return this.PaidAmountValue; }

            set { SetProperty(ref PaidAmountValue, value); }

        }
        private decimal? BalanceDueValue;

        public decimal? BalanceDue

        {

            get { return this.BalanceDueValue; }

            set { SetProperty(ref BalanceDueValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    public class CreateARInvoiceRequest : ModelEntityBase
    {
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

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
        private decimal TotalAmountValue;

        public decimal TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
    }

    public class ARInvoiceResponse : ARInvoice
    {
    }

    public class UpdateARInvoiceRequest : ModelEntityBase
    {
        private string ArInvoiceIdValue;

        public string ArInvoiceId

        {

            get { return this.ArInvoiceIdValue; }

            set { SetProperty(ref ArInvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private DateTime? InvoiceDateValue;

        public DateTime? InvoiceDate

        {

            get { return this.InvoiceDateValue; }

            set { SetProperty(ref InvoiceDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? TotalAmountValue;

        public decimal? TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private string StatusValue;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
    }

    public class CreateARPaymentRequest : ModelEntityBase
    {
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private DateTime PaymentDateValue;

        public DateTime PaymentDate

        {

            get { return this.PaymentDateValue; }

            set { SetProperty(ref PaymentDateValue, value); }

        }
        private decimal PaymentAmountValue;

        public decimal PaymentAmount

        {

            get { return this.PaymentAmountValue; }

            set { SetProperty(ref PaymentAmountValue, value); }

        }
        private string PaymentMethodValue;

        public string PaymentMethod

        {

            get { return this.PaymentMethodValue; }

            set { SetProperty(ref PaymentMethodValue, value); }

        } // Check, ACH, Wire, Cash
        private string CheckNumberValue;

        public string CheckNumber

        {

            get { return this.CheckNumberValue; }

            set { SetProperty(ref CheckNumberValue, value); }

        }
        private string ReferenceNumberValue;

        public string ReferenceNumber

        {

            get { return this.ReferenceNumberValue; }

            set { SetProperty(ref ReferenceNumberValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class CreateARCreditMemoRequest : ModelEntityBase
    {
        private string ArInvoiceIdValue;

        public string ArInvoiceId

        {

            get { return this.ArInvoiceIdValue; }

            set { SetProperty(ref ArInvoiceIdValue, value); }

        }
        private string CreditMemoNumberValue;

        public string CreditMemoNumber

        {

            get { return this.CreditMemoNumberValue; }

            set { SetProperty(ref CreditMemoNumberValue, value); }

        }
        private DateTime CreditMemoDateValue;

        public DateTime CreditMemoDate

        {

            get { return this.CreditMemoDateValue; }

            set { SetProperty(ref CreditMemoDateValue, value); }

        }
        private decimal CreditAmountValue;

        public decimal CreditAmount

        {

            get { return this.CreditAmountValue; }

            set { SetProperty(ref CreditAmountValue, value); }

        }
        private string ReasonValue;

        public string Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
        private string DescriptionValue;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
    }

    public class ARApprovalResult : ModelEntityBase
    {
        private string ArInvoiceIdValue;

        public string ArInvoiceId

        {

            get { return this.ArInvoiceIdValue; }

            set { SetProperty(ref ArInvoiceIdValue, value); }

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
        private string CommentsValue;

        public string Comments

        {

            get { return this.CommentsValue; }

            set { SetProperty(ref CommentsValue, value); }

        }
    }

    public class ARAgingSummary : ModelEntityBase
    {
        private string ArInvoiceIdValue;

        public string ArInvoiceId

        {

            get { return this.ArInvoiceIdValue; }

            set { SetProperty(ref ArInvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private DateTime? InvoiceDateValue;

        public DateTime? InvoiceDate

        {

            get { return this.InvoiceDateValue; }

            set { SetProperty(ref InvoiceDateValue, value); }

        }
        private DateTime? DueDateValue;

        public DateTime? DueDate

        {

            get { return this.DueDateValue; }

            set { SetProperty(ref DueDateValue, value); }

        }
        private decimal? TotalAmountValue;

        public decimal? TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private decimal? PaidAmountValue;

        public decimal? PaidAmount

        {

            get { return this.PaidAmountValue; }

            set { SetProperty(ref PaidAmountValue, value); }

        }
        private decimal? BalanceDueValue;

        public decimal? BalanceDue

        {

            get { return this.BalanceDueValue; }

            set { SetProperty(ref BalanceDueValue, value); }

        }
        private int DaysPastDueValue;

        public int DaysPastDue

        {

            get { return this.DaysPastDueValue; }

            set { SetProperty(ref DaysPastDueValue, value); }

        }
        private string AgingBucketValue;

        public string AgingBucket

        {

            get { return this.AgingBucketValue; }

            set { SetProperty(ref AgingBucketValue, value); }

        } // Current, 1-30, 31-60, 61-90, 90+
    }

    public class CustomerSummary : ModelEntityBase
    {
        private string CustomerBaIdValue;

        public string CustomerBaId

        {

            get { return this.CustomerBaIdValue; }

            set { SetProperty(ref CustomerBaIdValue, value); }

        }
        private decimal TotalInvoicesValue;

        public decimal TotalInvoices

        {

            get { return this.TotalInvoicesValue; }

            set { SetProperty(ref TotalInvoicesValue, value); }

        }
        private decimal TotalPaidValue;

        public decimal TotalPaid

        {

            get { return this.TotalPaidValue; }

            set { SetProperty(ref TotalPaidValue, value); }

        }
        private decimal TotalOutstandingValue;

        public decimal TotalOutstanding

        {

            get { return this.TotalOutstandingValue; }

            set { SetProperty(ref TotalOutstandingValue, value); }

        }
        private int NumberOfInvoicesValue;

        public int NumberOfInvoices

        {

            get { return this.NumberOfInvoicesValue; }

            set { SetProperty(ref NumberOfInvoicesValue, value); }

        }
        private int NumberOfOverdueInvoicesValue;

        public int NumberOfOverdueInvoices

        {

            get { return this.NumberOfOverdueInvoicesValue; }

            set { SetProperty(ref NumberOfOverdueInvoicesValue, value); }

        }
        private decimal AverageDaysToPayValue;

        public decimal AverageDaysToPay

        {

            get { return this.AverageDaysToPayValue; }

            set { SetProperty(ref AverageDaysToPayValue, value); }

        }
    }

    public class PaymentApplicationRequest : ModelEntityBase
    {
        private string PaymentIdValue;

        public string PaymentId

        {

            get { return this.PaymentIdValue; }

            set { SetProperty(ref PaymentIdValue, value); }

        }
        private List<PaymentApplicationItem> ApplicationsValue = new List<PaymentApplicationItem>();

        public List<PaymentApplicationItem> Applications

        {

            get { return this.ApplicationsValue; }

            set { SetProperty(ref ApplicationsValue, value); }

        }
    }

    public class PaymentApplicationItem : ModelEntityBase
    {
        private string InvoiceIdValue;

        public string InvoiceId

        {

            get { return this.InvoiceIdValue; }

            set { SetProperty(ref InvoiceIdValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
    }

    public class PaymentApplicationResult : ModelEntityBase
    {
        private string PaymentIdValue;

        public string PaymentId

        {

            get { return this.PaymentIdValue; }

            set { SetProperty(ref PaymentIdValue, value); }

        }
        private decimal TotalAppliedValue;

        public decimal TotalApplied

        {

            get { return this.TotalAppliedValue; }

            set { SetProperty(ref TotalAppliedValue, value); }

        }
        private int SuccessfulApplicationsValue;

        public int SuccessfulApplications

        {

            get { return this.SuccessfulApplicationsValue; }

            set { SetProperty(ref SuccessfulApplicationsValue, value); }

        }
        private int FailedApplicationsValue;

        public int FailedApplications

        {

            get { return this.FailedApplicationsValue; }

            set { SetProperty(ref FailedApplicationsValue, value); }

        }
        private List<string> AppliedInvoiceIdsValue = new List<string>();

        public List<string> AppliedInvoiceIds

        {

            get { return this.AppliedInvoiceIdsValue; }

            set { SetProperty(ref AppliedInvoiceIdsValue, value); }

        }
        private List<string> FailedInvoiceIdsValue = new List<string>();

        public List<string> FailedInvoiceIds

        {

            get { return this.FailedInvoiceIdsValue; }

            set { SetProperty(ref FailedInvoiceIdsValue, value); }

        }
    }
}








