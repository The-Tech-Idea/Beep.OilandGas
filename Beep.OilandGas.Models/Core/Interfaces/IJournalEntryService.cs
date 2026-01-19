using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service for general ledger journal entry recording.
    /// </summary>
    public interface IJournalEntryService
    {
        Task<JOURNAL_ENTRY> CreateEntryAsync(string glAccount, decimal amount, string description, string userId, string cn = "PPDM39");
        Task<JOURNAL_ENTRY> CreateBalancedEntryAsync(string debitAccount, string creditAccount, decimal amount, string description, string userId, string cn = "PPDM39");
        Task<List<GL_ENTRY>> GetEntriesByAccountAsync(string glAccount, DateTime start, DateTime end, string cn = "PPDM39");
        Task<decimal> GetAccountBalanceAsync(string glAccount, DateTime asOfDate, string cn = "PPDM39");
        Task<bool> ValidateAsync(JOURNAL_ENTRY entry, string cn = "PPDM39");
    }
}
