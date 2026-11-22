using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The investment review columns of an approval workflow row (ECO-03): the proposal's title, class, currency and due
    /// date, the economics it was reviewed against, the decision with its rationale and conditions, and the AFE handoff.
    /// </summary>
    /// <remarks>
    /// <c>APPROVAL_WORKFLOW</c> already holds the entity, the amount, who requested it, who approved it and when, which is
    /// most of what a decision needs and exactly the row ECO-03's contract describes. It lacked the currency, the class,
    /// the due date, the decision itself, its rationale, its conditions, the reviewed amount the decision was made against
    /// (AC-2) and the handoff (AC-4). These columns are additive, so an existing approval row stays valid. A row is a
    /// proposal and its decision together because a decision is final: "already decided - read-only".
    /// </remarks>
    public partial class APPROVAL_WORKFLOW
    {
        [MaxLength(256)]
        public string? PROPOSAL_TITLE
        {
            get => TitleValue;
            set => SetProperty(ref TitleValue, value);
        }

        private string? TitleValue;

        /// <summary>DRILL or FACILITY or WORKOVER or OTHER - the investment class the queue groups by.</summary>
        [MaxLength(32)]
        public string? INVESTMENT_CLASS
        {
            get => ClassValue;
            set => SetProperty(ref ClassValue, value);
        }

        private string? ClassValue;

        /// <summary>When the decision is due - the queue's age column is measured against it.</summary>
        public DateTime? DECISION_DUE
        {
            get => DueValue;
            set => SetProperty(ref DueValue, value);
        }

        private DateTime? DueValue;

        /// <summary>APPROVED, CONDITIONAL, RETURNED or REJECTED - from InvestmentDecisionCodes.</summary>
        [MaxLength(32)]
        public string? DECISION
        {
            get => DecisionValue;
            set => SetProperty(ref DecisionValue, value);
        }

        private string? DecisionValue;

        /// <summary>The reviewer's reason. AC-1: a decision without one is refused.</summary>
        [MaxLength(4096)]
        public string? RATIONALE
        {
            get => RationaleValue;
            set => SetProperty(ref RationaleValue, value);
        }

        private string? RationaleValue;

        /// <summary>The conditions, one per line; they travel to the AFE on handoff (AC-5).</summary>
        [MaxLength(4096)]
        public string? CONDITIONS_TEXT
        {
            get => ConditionsValue;
            set => SetProperty(ref ConditionsValue, value);
        }

        private string? ConditionsValue;

        /// <summary>The amount the decision was made on, kept so a later change is detectable (AC-2).</summary>
        public decimal? DECISION_AMOUNT
        {
            get => DecisionAmountValue;
            set => SetProperty(ref DecisionAmountValue, value);
        }

        private decimal? DecisionAmountValue;

        /// <summary>The economics evaluation the reviewer saw (ECO-01), or null when there is none.</summary>
        [MaxLength(128)]
        public string? ECONOMICS_ID
        {
            get => EconomicsIdValue;
            set => SetProperty(ref EconomicsIdValue, value);
        }

        private string? EconomicsIdValue;

        public decimal? ECONOMICS_NPV
        {
            get => EconomicsNpvValue;
            set => SetProperty(ref EconomicsNpvValue, value);
        }

        private decimal? EconomicsNpvValue;

        /// <summary>
        /// The amount as last reviewed. A decision compares the proposal's current amount with this, so an amount changed
        /// after the review is refused until it is reviewed again (AC-2).
        /// </summary>
        public decimal? REVIEWED_AMOUNT
        {
            get => ReviewedAmountValue;
            set => SetProperty(ref ReviewedAmountValue, value);
        }

        private decimal? ReviewedAmountValue;

        public DateTime? REVIEWED_AT
        {
            get => ReviewedAtValue;
            set => SetProperty(ref ReviewedAtValue, value);
        }

        private DateTime? ReviewedAtValue;

        /// <summary>The reviewer's risk notes, shown beside the decision.</summary>
        [MaxLength(4096)]
        public string? RISKS_TEXT
        {
            get => RisksValue;
            set => SetProperty(ref RisksValue, value);
        }

        private string? RisksValue;

        /// <summary>The AFE the decision was handed off as - one per decision (AC-4, AC-6).</summary>
        [MaxLength(128)]
        public string? AFE_ID
        {
            get => AfeValue;
            set => SetProperty(ref AfeValue, value);
        }

        private string? AfeValue;

        public DateTime? HANDOFF_AT
        {
            get => HandoffAtValue;
            set => SetProperty(ref HandoffAtValue, value);
        }

        private DateTime? HandoffAtValue;

        [MaxLength(128)]
        public string? HANDOFF_BY
        {
            get => HandoffByValue;
            set => SetProperty(ref HandoffByValue, value);
        }

        private string? HandoffByValue;
    }
}
