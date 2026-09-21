namespace Beep.OilandGas.Models.Data.ProductionAccounting;

public sealed record JournalPostingEvidence(JOURNAL_ENTRY Header,
    System.Collections.Generic.List<JOURNAL_ENTRY_LINE> Lines,
    System.Collections.Generic.List<GL_ENTRY> LedgerEntries);
