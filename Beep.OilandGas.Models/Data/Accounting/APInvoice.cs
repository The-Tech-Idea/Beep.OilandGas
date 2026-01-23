using System;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    public class APInvoice : ModelEntityBase
    {
        private string ApInvoiceIdValue;

        public string ApInvoiceId

        {

            get { return this.ApInvoiceIdValue; }

            set { SetProperty(ref ApInvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

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

    public class CreateAPInvoiceRequest : ModelEntityBase
    {
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

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

    public class APInvoiceResponse : APInvoice
    {
    }

    public class UpdateAPInvoiceRequest : ModelEntityBase
    {
        private string ApInvoiceIdValue;

        public string ApInvoiceId

        {

            get { return this.ApInvoiceIdValue; }

            set { SetProperty(ref ApInvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

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

    public class CreateAPPaymentRequest : ModelEntityBase
    {
        private string ApInvoiceIdValue;

        public string ApInvoiceId

        {

            get { return this.ApInvoiceIdValue; }

            set { SetProperty(ref ApInvoiceIdValue, value); }

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

        } // Check, ACH, Wire
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

    public class CreateAPCreditMemoRequest : ModelEntityBase
    {
        private string ApInvoiceIdValue;

        public string ApInvoiceId

        {

            get { return this.ApInvoiceIdValue; }

            set { SetProperty(ref ApInvoiceIdValue, value); }

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

    public class APApprovalResult : ModelEntityBase
    {
        private string ApInvoiceIdValue;

        public string ApInvoiceId

        {

            get { return this.ApInvoiceIdValue; }

            set { SetProperty(ref ApInvoiceIdValue, value); }

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

    public class APAgingSummary : ModelEntityBase
    {
        private string ApInvoiceIdValue;

        public string ApInvoiceId

        {

            get { return this.ApInvoiceIdValue; }

            set { SetProperty(ref ApInvoiceIdValue, value); }

        }
        private string InvoiceNumberValue;

        public string InvoiceNumber

        {

            get { return this.InvoiceNumberValue; }

            set { SetProperty(ref InvoiceNumberValue, value); }

        }
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

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

    public class VendorSummary : ModelEntityBase
    {
        private string VendorBaIdValue;

        public string VendorBaId

        {

            get { return this.VendorBaIdValue; }

            set { SetProperty(ref VendorBaIdValue, value); }

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

    public class PaymentBatchRequest : ModelEntityBase
    {
        private List<string> PaymentIdsValue = new List<string>();

        public List<string> PaymentIds

        {

            get { return this.PaymentIdsValue; }

            set { SetProperty(ref PaymentIdsValue, value); }

        }
        private DateTime BatchDateValue;

        public DateTime BatchDate

        {

            get { return this.BatchDateValue; }

            set { SetProperty(ref BatchDateValue, value); }

        }
        private string PaymentMethodValue;

        public string PaymentMethod

        {

            get { return this.PaymentMethodValue; }

            set { SetProperty(ref PaymentMethodValue, value); }

        }
        private string BatchDescriptionValue;

        public string BatchDescription

        {

            get { return this.BatchDescriptionValue; }

            set { SetProperty(ref BatchDescriptionValue, value); }

        }
    }

    public class PaymentBatchResult : ModelEntityBase
    {
        private string BatchIdValue = Guid.NewGuid().ToString();

        public string BatchId

        {

            get { return this.BatchIdValue; }

            set { SetProperty(ref BatchIdValue, value); }

        }
        private DateTime BatchDateValue;

        public DateTime BatchDate

        {

            get { return this.BatchDateValue; }

            set { SetProperty(ref BatchDateValue, value); }

        }
        private int TotalPaymentsValue;

        public int TotalPayments

        {

            get { return this.TotalPaymentsValue; }

            set { SetProperty(ref TotalPaymentsValue, value); }

        }
        private decimal TotalAmountValue;

        public decimal TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
        private int SuccessfulPaymentsValue;

        public int SuccessfulPayments

        {

            get { return this.SuccessfulPaymentsValue; }

            set { SetProperty(ref SuccessfulPaymentsValue, value); }

        }
        private int FailedPaymentsValue;

        public int FailedPayments

        {

            get { return this.FailedPaymentsValue; }

            set { SetProperty(ref FailedPaymentsValue, value); }

        }
        private List<string> ProcessedPaymentIdsValue = new List<string>();

        public List<string> ProcessedPaymentIds

        {

            get { return this.ProcessedPaymentIdsValue; }

            set { SetProperty(ref ProcessedPaymentIdsValue, value); }

        }
        private List<string> FailedPaymentIdsValue = new List<string>();

        public List<string> FailedPaymentIds

        {

            get { return this.FailedPaymentIdsValue; }

            set { SetProperty(ref FailedPaymentIdsValue, value); }

        }
    }
}








