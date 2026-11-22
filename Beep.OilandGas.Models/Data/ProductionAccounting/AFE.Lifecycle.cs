using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The lifecycle columns of an AFE (ECO-02): the kind, the author and approver, the approved amount, the revision
    /// chain and the closure.
    /// </summary>
    /// <remarks>
    /// The generated table held the number, the name, the estimated and actual cost, a status and an approval date - which
    /// cannot satisfy AC-1 (a revision must be distinguishable from the approved version it supersedes), AC-3 (the author
    /// cannot approve, so the author has to be recorded) or AC-5 (a closure has to be recorded). These columns are
    /// additive, so an existing row stays valid. The currency reuses the <c>CURRENCY_CODE</c> the AFE already carries.
    /// </remarks>
    public partial class AFE
    {
        /// <summary>DRILL or FACILITY or WORKOVER or OTHER - the kind of authorisation.</summary>
        [MaxLength(32)]
        public string? AFE_KIND
        {
            get => KindValue;
            set => SetProperty(ref KindValue, value);
        }

        private string? KindValue;

        /// <summary>The well, pool or field the authorisation is for.</summary>
        [MaxLength(128)]
        public string? ENTITY_ID
        {
            get => EntityValue;
            set => SetProperty(ref EntityValue, value);
        }

        private string? EntityValue;

        /// <summary>Who raised it. AC-3: this is the person who may not approve it.</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        /// <summary>Who approved it, once it is approved.</summary>
        [MaxLength(128)]
        public string? APPROVER
        {
            get => ApproverValue;
            set => SetProperty(ref ApproverValue, value);
        }

        private string? ApproverValue;

        /// <summary>
        /// The amount that was approved - the ceiling commitments are measured against. It is copied from the estimate at
        /// approval and never edited afterwards, so a supplement cannot quietly raise it.
        /// </summary>
        public decimal? APPROVED_AMOUNT
        {
            get => ApprovedAmountValue;
            set => SetProperty(ref ApprovedAmountValue, value);
        }

        private decimal? ApprovedAmountValue;

        /// <summary>How many revisions of this AFE number have been raised; the first is 1.</summary>
        public int REVISION_NO
        {
            get => RevisionValue;
            set => SetProperty(ref RevisionValue, value);
        }

        private int RevisionValue = 1;

        /// <summary>
        /// The approved revision this one replaces. AC-1: this column lives on the <b>new</b> row, so the approved row is
        /// never written to record that it was superseded - the relationship is derivable from here.
        /// </summary>
        [MaxLength(128)]
        public string? SUPERSEDES_AFE_ID
        {
            get => SupersedesValue;
            set => SetProperty(ref SupersedesValue, value);
        }

        private string? SupersedesValue;

        /// <summary>The first revision of the AFE number, so the whole chain is one query.</summary>
        [MaxLength(128)]
        public string? ROOT_AFE_ID
        {
            get => RootValue;
            set => SetProperty(ref RootValue, value);
        }

        private string? RootValue;

        /// <summary>Why the revision was raised - what the supplement adds or changes.</summary>
        [MaxLength(512)]
        public string? REVISION_REASON
        {
            get => ReasonValue;
            set => SetProperty(ref ReasonValue, value);
        }

        private string? ReasonValue;

        public DateTime? CLOSED_AT
        {
            get => ClosedAtValue;
            set => SetProperty(ref ClosedAtValue, value);
        }

        private DateTime? ClosedAtValue;

        [MaxLength(128)]
        public string? CLOSED_BY
        {
            get => ClosedByValue;
            set => SetProperty(ref ClosedByValue, value);
        }

        private string? ClosedByValue;

        [MaxLength(512)]
        public string? CLOSE_NOTE
        {
            get => CloseNoteValue;
            set => SetProperty(ref CloseNoteValue, value);
        }

        private string? CloseNoteValue;
    }
}
