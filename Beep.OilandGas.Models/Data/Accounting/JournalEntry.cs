using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public class JournalEntry : ModelEntityBase
    {
        public string JournalEntryId { get; set; }
        public string EntryNumber { get; set; }
        public DateTime? EntryDate { get; set; }
        public string EntryType { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public string SourceModule { get; set; }
        public decimal? TotalDebit { get; set; }
        public decimal? TotalCredit { get; set; }
        public List<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
    }

    public class JournalEntryLine : ModelEntityBase
    {
        public string JournalEntryLineId { get; set; }
        public string JournalEntryId { get; set; }
        public string GlAccountId { get; set; }
        public int? LineNumber { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public string Description { get; set; }
    }

    public class CreateJournalEntryRequest : ModelEntityBase
    {
        public string EntryNumber { get; set; }
        public DateTime EntryDate { get; set; }
        public string EntryType { get; set; }
        public string Description { get; set; }
        public string ReferenceNumber { get; set; }
        public string SourceModule { get; set; }
        public List<JournalEntryLineData> Lines { get; set; } = new List<JournalEntryLineData>();
    }

    public class JournalEntryLineData : ModelEntityBase
    {
        public string GlAccountId { get; set; }
        public decimal? DebitAmount { get; set; }
        public decimal? CreditAmount { get; set; }
        public string Description { get; set; }
    }

    public class PeriodClosingResult : ModelEntityBase
    {
        public string ClosingId { get; set; } = Guid.NewGuid().ToString();
        public DateTime PeriodEndDate { get; set; }
        public bool IsClosed { get; set; }
        public string Status { get; set; }
        public int JournalEntriesCreated { get; set; }
        public string UserId { get; set; }
        public DateTime ClosingDate { get; set; } = DateTime.UtcNow;
    }

    public class JournalEntryApprovalResult : ModelEntityBase
    {
        public string JournalEntryId { get; set; }
        public bool IsApproved { get; set; }
        public string ApproverId { get; set; }
        public DateTime ApprovalDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; }
    }
}





