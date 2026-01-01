using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.DTOs.Accounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for journal entry operations.
    /// </summary>
    public interface IJournalEntryService
    {
        /// <summary>
        /// Creates a journal entry with lines.
        /// </summary>
        Task<JOURNAL_ENTRY> CreateJournalEntryAsync(CreateJournalEntryRequest request, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets a journal entry by ID.
        /// </summary>
        Task<JOURNAL_ENTRY?> GetJournalEntryAsync(string entryId, string? connectionName = null);
        
        /// <summary>
        /// Gets journal entries by date range.
        /// </summary>
        Task<List<JOURNAL_ENTRY>> GetJournalEntriesByDateRangeAsync(DateTime startDate, DateTime endDate, string? connectionName = null);
        
        /// <summary>
        /// Gets journal entries by account.
        /// </summary>
        Task<List<JOURNAL_ENTRY>> GetJournalEntriesByAccountAsync(string accountId, DateTime? startDate, DateTime? endDate, string? connectionName = null);
        
        /// <summary>
        /// Posts a journal entry (changes status to Posted).
        /// </summary>
        Task<JOURNAL_ENTRY> PostJournalEntryAsync(string entryId, string userId, string? connectionName = null);
        
        /// <summary>
        /// Reverses a journal entry.
        /// </summary>
        Task<JOURNAL_ENTRY> ReverseJournalEntryAsync(string entryId, string reason, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets journal entry lines for an entry.
        /// </summary>
        Task<List<JOURNAL_ENTRY_LINE>> GetJournalEntryLinesAsync(string entryId, string? connectionName = null);
        
        /// <summary>
        /// Closes an accounting period.
        /// </summary>
        Task<PeriodClosingResult> ClosePeriodAsync(DateTime periodEndDate, string userId, string? connectionName = null);
        
        /// <summary>
        /// Gets unposted journal entries.
        /// </summary>
        Task<List<JOURNAL_ENTRY>> GetUnpostedEntriesAsync(string? connectionName = null);
        
        /// <summary>
        /// Approves a journal entry.
        /// </summary>
        Task<JournalEntryApprovalResult> ApproveJournalEntryAsync(string entryId, string approverId, string? connectionName = null);
    }
}

