using System;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// One close of a field's accounting period, as the closing journal the close posts records it
    /// (<c>JOURNAL_ENTRY</c> with <c>SOURCE_MODULE</c> PERIOD_CLOSING and <c>REFERENCE_NUMBER</c>
    /// <c>CLOSE-{field}-{yyyyMM}</c>). Read from the journal, never kept beside it.
    /// </summary>
    public class PeriodCloseSummary
    {
        /// <summary>The field whose period the close is for.</summary>
        public string FieldId { get; set; } = string.Empty;

        /// <summary>The period end the close was made for: the closing journal's entry date.</summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// What the closing journal says now, as a <c>PERIOD_CLOSE_STATUS</c> code: <c>CLOSED</c> while it stands (posted),
        /// <c>REOPENED</c> once it was reversed, <c>CLOSING</c> while it was created and never posted.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>The closing journal's entry number.</summary>
        public string JournalEntryNumber { get; set; } = string.Empty;

        /// <summary>Who made the close: the account that created the closing journal.</summary>
        public string ClosedBy { get; set; } = string.Empty;

        /// <summary>When the close was made: when the closing journal was created.</summary>
        public DateTime? ClosedAt { get; set; }
    }
}
