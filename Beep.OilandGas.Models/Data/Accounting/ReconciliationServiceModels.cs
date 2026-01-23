using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Variance identified during reconciliation.
    /// </summary>
    public class ReconciliationVariance : ModelEntityBase
    {
        private string VarianceIdValue = string.Empty;

        public string VarianceId

        {

            get { return this.VarianceIdValue; }

            set { SetProperty(ref VarianceIdValue, value); }

        }
        private string AccountIdValue = string.Empty;

        public string AccountId

        {

            get { return this.AccountIdValue; }

            set { SetProperty(ref AccountIdValue, value); }

        }
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private string GLReferenceValue = string.Empty;

        public string GLReference

        {

            get { return this.GLReferenceValue; }

            set { SetProperty(ref GLReferenceValue, value); }

        }
        private string SubledgerReferenceValue = string.Empty;

        public string SubledgerReference

        {

            get { return this.SubledgerReferenceValue; }

            set { SetProperty(ref SubledgerReferenceValue, value); }

        }
        private ReconciliationVarianceType TypeValue;

        public ReconciliationVarianceType Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // Open, Resolved, Investigated
    }

    /// <summary>
    /// Intercompany reconciliation result.
    /// </summary>
    public class IntercompanyReconciliation : ModelEntityBase
    {
        private string Company1IdValue = string.Empty;

        public string Company1Id

        {

            get { return this.Company1IdValue; }

            set { SetProperty(ref Company1IdValue, value); }

        }
        private string Company2IdValue = string.Empty;

        public string Company2Id

        {

            get { return this.Company2IdValue; }

            set { SetProperty(ref Company2IdValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private decimal Company1AmountValue;

        public decimal Company1Amount

        {

            get { return this.Company1AmountValue; }

            set { SetProperty(ref Company1AmountValue, value); }

        }
        private decimal Company2AmountValue;

        public decimal Company2Amount

        {

            get { return this.Company2AmountValue; }

            set { SetProperty(ref Company2AmountValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private List<UnmatchedTransaction> UnmatchedTransactionsValue = new();

        public List<UnmatchedTransaction> UnmatchedTransactions

        {

            get { return this.UnmatchedTransactionsValue; }

            set { SetProperty(ref UnmatchedTransactionsValue, value); }

        }
    }

    /// <summary>
    /// Aging analysis bucket.
    /// </summary>
    public class AgingAnalysis : ModelEntityBase
    {
        private string AccountIdValue = string.Empty;

        public string AccountId

        {

            get { return this.AccountIdValue; }

            set { SetProperty(ref AccountIdValue, value); }

        }
        private DateTime AsOfDateValue;

        public DateTime AsOfDate

        {

            get { return this.AsOfDateValue; }

            set { SetProperty(ref AsOfDateValue, value); }

        }
        private List<AgingBucket> BucketsValue = new();

        public List<AgingBucket> Buckets

        {

            get { return this.BucketsValue; }

            set { SetProperty(ref BucketsValue, value); }

        }
        private decimal TotalAmountValue;

        public decimal TotalAmount

        {

            get { return this.TotalAmountValue; }

            set { SetProperty(ref TotalAmountValue, value); }

        }
    }

    /// <summary>
    /// Aging bucket detail.
    /// </summary>
    public class AgingBucket : ModelEntityBase
    {
        private string BucketNameValue = string.Empty;

        public string BucketName

        {

            get { return this.BucketNameValue; }

            set { SetProperty(ref BucketNameValue, value); }

        }
        private int DaysMinValue;

        public int DaysMin

        {

            get { return this.DaysMinValue; }

            set { SetProperty(ref DaysMinValue, value); }

        }
        private int DaysMaxValue;

        public int DaysMax

        {

            get { return this.DaysMaxValue; }

            set { SetProperty(ref DaysMaxValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private decimal PercentageOfTotalValue;

        public decimal PercentageOfTotal

        {

            get { return this.PercentageOfTotalValue; }

            set { SetProperty(ref PercentageOfTotalValue, value); }

        }
        private List<string> ItemsValue = new();

        public List<string> Items

        {

            get { return this.ItemsValue; }

            set { SetProperty(ref ItemsValue, value); }

        }
    }

    /// <summary>
    /// History entry for reconciliation.
    /// </summary>
    public class ReconciliationHistory : ModelEntityBase
    {
        private string ReconciliationIdValue = string.Empty;

        public string ReconciliationId

        {

            get { return this.ReconciliationIdValue; }

            set { SetProperty(ref ReconciliationIdValue, value); }

        }
        private DateTime ReconciliationDateValue;

        public DateTime ReconciliationDate

        {

            get { return this.ReconciliationDateValue; }

            set { SetProperty(ref ReconciliationDateValue, value); }

        }
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        private string PerformedByValue = string.Empty;

        public string PerformedBy

        {

            get { return this.PerformedByValue; }

            set { SetProperty(ref PerformedByValue, value); }

        }
        private decimal VarianceAmountValue;

        public decimal VarianceAmount

        {

            get { return this.VarianceAmountValue; }

            set { SetProperty(ref VarianceAmountValue, value); }

        }
        private bool WasReconciledValue;

        public bool WasReconciled

        {

            get { return this.WasReconciledValue; }

            set { SetProperty(ref WasReconciledValue, value); }

        }
    }

    /// <summary>
    /// Unmatched transaction during reconciliation.
    /// </summary>
    public class UnmatchedTransaction : ModelEntityBase
    {
        private string TransactionIdValue = string.Empty;

        public string TransactionId

        {

            get { return this.TransactionIdValue; }

            set { SetProperty(ref TransactionIdValue, value); }

        }
        private DateTime TransactionDateValue;

        public DateTime TransactionDate

        {

            get { return this.TransactionDateValue; }

            set { SetProperty(ref TransactionDateValue, value); }

        }
        private string DescriptionValue = string.Empty;

        public string Description

        {

            get { return this.DescriptionValue; }

            set { SetProperty(ref DescriptionValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
    }

    /// <summary>
    /// Outstanding item (check, deposit) in bank reconciliation.
    /// </summary>
    public class OutstandingItem : ModelEntityBase
    {
        private string ItemIdValue = string.Empty;

        public string ItemId

        {

            get { return this.ItemIdValue; }

            set { SetProperty(ref ItemIdValue, value); }

        }
        private DateTime ItemDateValue;

        public DateTime ItemDate

        {

            get { return this.ItemDateValue; }

            set { SetProperty(ref ItemDateValue, value); }

        }
        private decimal AmountValue;

        public decimal Amount

        {

            get { return this.AmountValue; }

            set { SetProperty(ref AmountValue, value); }

        }
        private string TypeValue = string.Empty;

        public string Type

        {

            get { return this.TypeValue; }

            set { SetProperty(ref TypeValue, value); }

        } // Check, Deposit
        private int DaysOutstandingValue;

        public int DaysOutstanding

        {

            get { return this.DaysOutstandingValue; }

            set { SetProperty(ref DaysOutstandingValue, value); }

        }
    }

    /// <summary>
    /// Type of variance.
    /// </summary>
    public enum ReconciliationVarianceType
    {
        Timing,
        Amount,
        Missing,
        Extra,
        Description,
        Other
    }

    /// <summary>
    /// Compatibility fields for reconciliation summary.
    /// </summary>
    public partial class ReconciliationSummary
    {
        private DateTime PeriodEndValue;

        public DateTime PeriodEnd

        {

            get { return this.PeriodEndValue; }

            set { SetProperty(ref PeriodEndValue, value); }

        }
        public decimal GLBalance
        {
            get => GlBalance;
            set => GlBalance = value;
        }
        public decimal SubledgerBalance
        {
            get => SubledgerTotal;
            set => SubledgerTotal = value;
        }
        public decimal Variance
        {
            get => Difference;
            set => Difference = value;
        }
        private bool IsReconciledValue;

        public bool IsReconciled

        {

            get { return this.IsReconciledValue; }

            set { SetProperty(ref IsReconciledValue, value); }

        }
        private List<ReconciliationVariance> VariancesValue = new();

        public List<ReconciliationVariance> Variances

        {

            get { return this.VariancesValue; }

            set { SetProperty(ref VariancesValue, value); }

        }
    }

    /// <summary>
    /// Compatibility fields for bank reconciliation.
    /// </summary>
    public partial class BankReconciliation
    {
        public string CashAccountId
        {
            get => AccountNumber;
            set => AccountNumber = value;
        }
        public DateTime PeriodEnd
        {
            get => StatementDate;
            set => StatementDate = value;
        }
        public decimal BankBalance
        {
            get => BankStatementBalance;
            set => BankStatementBalance = value;
        }
        public decimal OutstandingDeposits
        {
            get => TotalDepositsInTransit;
            set => TotalDepositsInTransit = value;
        }
        public decimal ReconciledBalance
        {
            get => ReconciledGLBalance;
            set => ReconciledGLBalance = value;
        }
        private List<OutstandingItem> OutstandingItemsValue = new();

        public List<OutstandingItem> OutstandingItems

        {

            get { return this.OutstandingItemsValue; }

            set { SetProperty(ref OutstandingItemsValue, value); }

        }
    }
}



