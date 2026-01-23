using System;
using System.Collections.Generic;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Accounting
{
    /// <summary>
    /// Result of period closing operation.
    /// </summary>
    public class PeriodClosingResult : ModelEntityBase
    {
        private string ClosingIdValue = string.Empty;

        public string ClosingId

        {

            get { return this.ClosingIdValue; }

            set { SetProperty(ref ClosingIdValue, value); }

        }
        private DateTime PeriodEndDateValue;

        public DateTime PeriodEndDate

        {

            get { return this.PeriodEndDateValue; }

            set { SetProperty(ref PeriodEndDateValue, value); }

        }
        private bool IsClosedValue;

        public bool IsClosed

        {

            get { return this.IsClosedValue; }

            set { SetProperty(ref IsClosedValue, value); }

        }
        private string StatusValue = string.Empty;

        public string Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        }
        private int JournalEntriesCreatedValue;

        public int JournalEntriesCreated

        {

            get { return this.JournalEntriesCreatedValue; }

            set { SetProperty(ref JournalEntriesCreatedValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
        private DateTime ClosingDateValue = DateTime.UtcNow;

        public DateTime ClosingDate

        {

            get { return this.ClosingDateValue; }

            set { SetProperty(ref ClosingDateValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string PeriodIdValue = string.Empty;

        public string PeriodId

        {

            get { return this.PeriodIdValue; }

            set { SetProperty(ref PeriodIdValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private DateTime CompletedDateValue;

        public DateTime CompletedDate

        {

            get { return this.CompletedDateValue; }

            set { SetProperty(ref CompletedDateValue, value); }

        }
        private List<string> MessagesValue = new();

        public List<string> Messages

        {

            get { return this.MessagesValue; }

            set { SetProperty(ref MessagesValue, value); }

        }
        private List<string> ErrorsValue = new();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }

    /// <summary>
    /// Validation result for period closing.
    /// </summary>
    public class PeriodClosingValidation : ModelEntityBase
    {
        private bool IsReadyValue;

        public bool IsReady

        {

            get { return this.IsReadyValue; }

            set { SetProperty(ref IsReadyValue, value); }

        }
        private List<string> PrerequisitesValue = new();

        public List<string> Prerequisites

        {

            get { return this.PrerequisitesValue; }

            set { SetProperty(ref PrerequisitesValue, value); }

        }
        private List<string> FailedChecksValue = new();

        public List<string> FailedChecks

        {

            get { return this.FailedChecksValue; }

            set { SetProperty(ref FailedChecksValue, value); }

        }
        public Dictionary<string, object> CheckDetails { get; set; } = new();
    }

    /// <summary>
    /// Result of journal posting operation.
    /// </summary>
    public class PostingResult : ModelEntityBase
    {
        private int EntriesPostedValue;

        public int EntriesPosted

        {

            get { return this.EntriesPostedValue; }

            set { SetProperty(ref EntriesPostedValue, value); }

        }
        private int EntriesFailedValue;

        public int EntriesFailed

        {

            get { return this.EntriesFailedValue; }

            set { SetProperty(ref EntriesFailedValue, value); }

        }
        private List<string> ErrorsValue = new();

        public List<string> Errors

        {

            get { return this.ErrorsValue; }

            set { SetProperty(ref ErrorsValue, value); }

        }
    }

    /// <summary>
    /// Result of reconciliation operation.
    /// </summary>
    public class ReconciliationResult : ModelEntityBase
    {
        private bool ReconciliationBalanceValue;

        public bool ReconciliationBalance

        {

            get { return this.ReconciliationBalanceValue; }

            set { SetProperty(ref ReconciliationBalanceValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
        private List<UnreconciledItem> UnreconciledItemsValue = new();

        public List<UnreconciledItem> UnreconciledItems

        {

            get { return this.UnreconciledItemsValue; }

            set { SetProperty(ref UnreconciledItemsValue, value); }

        }
    }

    /// <summary>
    /// Status of a period close.
    /// </summary>
    public class PeriodClosingStatus : ModelEntityBase
    {
        private string PeriodIdValue = string.Empty;

        public string PeriodId

        {

            get { return this.PeriodIdValue; }

            set { SetProperty(ref PeriodIdValue, value); }

        }
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private PeriodClosingState StateValue;

        public PeriodClosingState State

        {

            get { return this.StateValue; }

            set { SetProperty(ref StateValue, value); }

        }
        private DateTime? OpenedDateValue;

        public DateTime? OpenedDate

        {

            get { return this.OpenedDateValue; }

            set { SetProperty(ref OpenedDateValue, value); }

        }
        private DateTime? ClosedDateValue;

        public DateTime? ClosedDate

        {

            get { return this.ClosedDateValue; }

            set { SetProperty(ref ClosedDateValue, value); }

        }
        private string ClosedByValue = string.Empty;

        public string ClosedBy

        {

            get { return this.ClosedByValue; }

            set { SetProperty(ref ClosedByValue, value); }

        }
        private bool IsLockedValue;

        public bool IsLocked

        {

            get { return this.IsLockedValue; }

            set { SetProperty(ref IsLockedValue, value); }

        }
    }

    /// <summary>
    /// Unreconciled item during reconciliation.
    /// </summary>
    public class UnreconciledItem : ModelEntityBase
    {
        private string AccountIdValue = string.Empty;

        public string AccountId

        {

            get { return this.AccountIdValue; }

            set { SetProperty(ref AccountIdValue, value); }

        }
        private decimal ExpectedBalanceValue;

        public decimal ExpectedBalance

        {

            get { return this.ExpectedBalanceValue; }

            set { SetProperty(ref ExpectedBalanceValue, value); }

        }
        private decimal ActualBalanceValue;

        public decimal ActualBalance

        {

            get { return this.ActualBalanceValue; }

            set { SetProperty(ref ActualBalanceValue, value); }

        }
        private decimal VarianceValue;

        public decimal Variance

        {

            get { return this.VarianceValue; }

            set { SetProperty(ref VarianceValue, value); }

        }
    }

    /// <summary>
    /// State of a period.
    /// </summary>
    public enum PeriodClosingState
    {
        Open,
        InProgress,
        Closed,
        Locked,
        Reopened
    }
}



