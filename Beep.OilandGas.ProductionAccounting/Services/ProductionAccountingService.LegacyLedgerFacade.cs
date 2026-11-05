using Beep.OilandGas.Accounting.Models;
using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.ProductionAccounting.Services;
public partial class ProductionAccountingService
{
        /// <summary>Retired companyless ledger facade; use the recorded company account/configuration owner.</summary>
        public sealed class GeneralLedgerCompatibility
        {
            public GeneralLedgerCompatibility(ProductionAccountingService service){ArgumentNullException.ThrowIfNull(service);}
            public List<GL_ACCOUNT> GetAllAccounts()=>throw new InvalidOperationException("Use the recorded company/book/currency account review.");
            public GL_ACCOUNT? GetAccount(string id)=>throw new InvalidOperationException("Use the recorded company/book/currency account review.");
            public GL_ACCOUNT CreateAccount(CreateGLAccountRequest request,string userId)=>throw new InvalidOperationException("Use the original reviewed company account configuration command.");
        }

        /// <summary>Retired companyless journal facade; no fallback may create or report a financial outcome.</summary>
        public sealed class JournalEntryCompatibility
        {
            public JournalEntryCompatibility(ProductionAccountingService service){ArgumentNullException.ThrowIfNull(service);}
            public JOURNAL_ENTRY CreateJournalEntry(string entryNumber,DateTime entryDate,string entryType,string description,List<JournalEntryLineData> lines,string userId)=>throw new InvalidOperationException("Company journal preparation requires the recorded source-bound workflow.");
            public void PostJournalEntry(string id,string userId)=>throw new InvalidOperationException("Company journal posting requires its independently authorized atomic native workflow.");
            public JOURNAL_ENTRY? GetJournalEntry(string id)=>throw new InvalidOperationException("Company journal evidence requires its recorded source-bound workflow.");
            public IEnumerable<JOURNAL_ENTRY_LINE> GetJournalEntryLines(string id)=>throw new InvalidOperationException("Company journal line evidence requires its recorded source-bound workflow.");
        }

}
