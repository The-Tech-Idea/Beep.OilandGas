using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Accounting.Models
{
    /// <summary>
    /// Represents the status of a period close workflow step
    /// </summary>
    public class PeriodCloseChecklistItem
    {
        public string RuleId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
        public bool IsComplete { get; set; }
        public string Details { get; set; }
        public string Module { get; set; } // AP, AR, GL, INVENTORY
    }

    /// <summary>
    /// Represents the full checklist for closing a period
    /// </summary>
    public class PeriodCloseChecklist
    {
        public DateTime PeriodEndDate { get; set; }
        public bool IsReadyToClose { get; set; }
        public List<PeriodCloseChecklistItem> Items { get; set; } = new List<PeriodCloseChecklistItem>();
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime ChecklistGeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Result of attempting to close a period
    /// </summary>
    public class PeriodCloseResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> StepsCompleted { get; set; } = new List<string>();
        public int ClosingEntriesCount { get; set; }
        public int ReversalEntriesCount { get; set; }
    }
}
