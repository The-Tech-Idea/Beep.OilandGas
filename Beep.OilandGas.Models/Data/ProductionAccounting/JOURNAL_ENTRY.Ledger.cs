using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The ledger columns of a journal entry (ACC-02): who wrote it, who posted it and when, the period it belongs to, and
    /// the entry it reverses.
    /// </summary>
    /// <remarks>
    /// The generated table held the number, date, type, status, the two totals and the source, which is not enough for
    /// AC-4 (the author cannot post their own journal, so the author has to be recorded), AC-3 (a write is checked against
    /// the period the date falls in), AC-2 (a posting records who and when) or the reversal. These columns are additive, so
    /// an existing row stays valid.
    /// </remarks>
    public partial class JOURNAL_ENTRY
    {
        /// <summary>Who wrote the journal - the person who may not post it (AC-4).</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        [MaxLength(128)]
        public string? POSTED_BY
        {
            get => PostedByValue;
            set => SetProperty(ref PostedByValue, value);
        }

        private string? PostedByValue;

        public DateTime? POSTED_AT
        {
            get => PostedAtValue;
            set => SetProperty(ref PostedAtValue, value);
        }

        private DateTime? PostedAtValue;

        /// <summary>The period the entry date falls in - the lock a write is checked against (AC-3).</summary>
        [MaxLength(16)]
        public string? PERIOD_CODE
        {
            get => PeriodValue;
            set => SetProperty(ref PeriodValue, value);
        }

        private string? PeriodValue;

        /// <summary>The posted entry this one reverses, when it is a reversal.</summary>
        [MaxLength(128)]
        public string? REVERSAL_OF_ENTRY_ID
        {
            get => ReversalValue;
            set => SetProperty(ref ReversalValue, value);
        }

        private string? ReversalValue;

        public DateTime? REVERSED_AT
        {
            get => ReversedAtValue;
            set => SetProperty(ref ReversedAtValue, value);
        }

        private DateTime? ReversedAtValue;
    }
}
