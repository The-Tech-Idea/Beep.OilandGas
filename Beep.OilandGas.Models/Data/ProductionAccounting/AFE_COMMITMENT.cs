using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// One commitment made against an AFE (ECO-02): who committed how much, against what reference, and when.
    /// </summary>
    /// <remarks>
    /// The AFE table carries an estimate and an actual cost but no commitments, so "committed" was not a fact anywhere and
    /// AC-2 (a commitment above the remaining amount is refused) had nothing to be measured against. A commitment is its
    /// own row because there are many per AFE and each has its own reference and date; the AFE's committed total is the
    /// sum of its rows, never a scalar that could drift from them.
    /// </remarks>
    public class AFE_COMMITMENT : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string AFE_COMMITMENT_ID
        {
            get => CommitmentIdValue;
            set => SetProperty(ref CommitmentIdValue, value);
        }

        private string CommitmentIdValue = string.Empty;

        /// <summary>The AFE revision this commitment is made against.</summary>
        [MaxLength(128)]
        public string AFE_ID
        {
            get => AfeValue;
            set => SetProperty(ref AfeValue, value);
        }

        private string AfeValue = string.Empty;

        /// <summary>The purchase order, contract or work order the money is committed to (ACC-05 linkage or a manual reference).</summary>
        [MaxLength(128)]
        public string? REFERENCE
        {
            get => ReferenceValue;
            set => SetProperty(ref ReferenceValue, value);
        }

        private string? ReferenceValue;

        public decimal AMOUNT
        {
            get => AmountValue;
            set => SetProperty(ref AmountValue, value);
        }

        private decimal AmountValue;

        [MaxLength(16)]
        public string? CURRENCY_CODE
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;

        public DateTime? COMMITTED_AT
        {
            get => CommittedAtValue;
            set => SetProperty(ref CommittedAtValue, value);
        }

        private DateTime? CommittedAtValue;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;
    }
}
