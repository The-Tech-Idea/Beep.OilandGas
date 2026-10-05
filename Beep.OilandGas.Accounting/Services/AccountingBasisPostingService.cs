using Beep.OilandGas.Models.Constants;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Accounting.Services
{
    /// <summary>
    /// Simplified posting helper for IFRS/GAAP/both using mapping keys.
    /// </summary>
    public class AccountingBasisPostingService
    {
        private readonly JournalEntryService _journalEntryService;
        private readonly IAccountMappingService? _ifrsMapping;
        private readonly IAccountMappingService? _gaapMapping;

        public AccountingBasisPostingService(
            JournalEntryService journalEntryService,
            IAccountMappingService? ifrsMapping = null,
            IAccountMappingService? gaapMapping = null)
        {
            _journalEntryService = journalEntryService ?? throw new ArgumentNullException(nameof(journalEntryService));
            _ifrsMapping = ifrsMapping;
            _gaapMapping = gaapMapping;
        }

        public async Task<(JOURNAL_ENTRY? IfrsEntry, JOURNAL_ENTRY? GaapEntry)> PostBalancedEntryAsync(
            string debitKey,
            string creditKey,
            decimal amount,
            string description,
            string userId,
            AccountingBasis basis = AccountingBasis.Ifrs,
            string cn = "PPDM39")
        {
            if (string.IsNullOrWhiteSpace(debitKey))
                throw new ArgumentNullException(nameof(debitKey));
            if (string.IsNullOrWhiteSpace(creditKey))
                throw new ArgumentNullException(nameof(creditKey));

            if (_ifrsMapping == null || _gaapMapping == null)
                throw new InvalidOperationException("Account mapping services are required for key-based posting.");

            return basis switch
            {
                AccountingBasis.Ifrs => (await _journalEntryService.CreateBalancedEntryAsync(
                        _ifrsMapping.GetAccountId(debitKey),
                        _ifrsMapping.GetAccountId(creditKey),
                        amount,
                        description,
                        userId,
                        cn,
                        AccountingBooks.Ifrs),
                    null),
                AccountingBasis.Gaap => (null,
                    await _journalEntryService.CreateBalancedEntryAsync(
                        _gaapMapping.GetAccountId(debitKey),
                        _gaapMapping.GetAccountId(creditKey),
                        amount,
                        description,
                        userId,
                        cn,
                        AccountingBooks.Gaap)),
                AccountingBasis.Both => await _journalEntryService.CreateDualBalancedEntryFromKeysAsync(
                    debitKey,
                    creditKey,
                    amount,
                    description,
                    userId,
                    _ifrsMapping,
                    _gaapMapping,
                    AccountingBooks.Ifrs,
                    AccountingBooks.Gaap,
                    cn)
            };
        }

        public async Task<(JOURNAL_ENTRY? IfrsEntry, JOURNAL_ENTRY? GaapEntry)> PostBalancedEntryByAccountAsync(
            string debitAccount,
            string creditAccount,
            decimal amount,
            string description,
            string userId,
            string cn = "PPDM39",
            string? bookId = null,
            AccountingBasis basis = AccountingBasis.Ifrs)
        {
            var ifrsBookId = string.IsNullOrWhiteSpace(bookId) ? AccountingBooks.Ifrs : bookId;
            var gaapBookId = string.IsNullOrWhiteSpace(bookId) ? AccountingBooks.Gaap : bookId;

            return basis switch
            {
                AccountingBasis.Ifrs => (await _journalEntryService.CreateBalancedEntryAsync(
                        debitAccount,
                        creditAccount,
                        amount,
                        description,
                        userId,
                        cn,
                        ifrsBookId),
                    null),
                AccountingBasis.Gaap => (null,
                    await _journalEntryService.CreateBalancedEntryAsync(
                        debitAccount,
                        creditAccount,
                        amount,
                        description,
                        userId,
                        cn,
                        gaapBookId)),
                AccountingBasis.Both => (await _journalEntryService.CreateBalancedEntryAsync(
                        debitAccount,
                        creditAccount,
                        amount,
                        description,
                        userId,
                        cn,
                        ifrsBookId),
                    await _journalEntryService.CreateBalancedEntryAsync(
                        debitAccount,
                        creditAccount,
                        amount,
                        description,
                        userId,
                        cn,
                        gaapBookId))
            };
        }

        public async Task<(JOURNAL_ENTRY? IfrsEntry, JOURNAL_ENTRY? GaapEntry)> PostEntryAsync(
            DateTime entryDate,
            string description,
            List<JOURNAL_ENTRY_LINE> lineItems,
            string userId,
            string? referenceNumber = null,
            string? sourceModule = null,
            string? bookId = null,
            AccountingBasis basis = AccountingBasis.Ifrs)
        {
            if (lineItems == null || lineItems.Count == 0)
                throw RefusalException.Invalid("A journal entry must have at least one line item.");

            var ifrsBookId = string.IsNullOrWhiteSpace(bookId) ? AccountingBooks.Ifrs : bookId;
            var gaapBookId = string.IsNullOrWhiteSpace(bookId) ? AccountingBooks.Gaap : bookId;

            // IFRS first, then GAAP: a tuple's elements are evaluated left to right.
            return basis switch
            {
                AccountingBasis.Ifrs => (await CreateAndPostAsync(
                        entryDate, description, lineItems, userId, referenceNumber, sourceModule, ifrsBookId),
                    null),
                AccountingBasis.Gaap => (null,
                    await CreateAndPostAsync(
                        entryDate, description, lineItems, userId, referenceNumber, sourceModule, gaapBookId)),
                AccountingBasis.Both => (
                    await CreateAndPostAsync(
                        entryDate, description, CloneLines(lineItems, null), userId, referenceNumber, sourceModule, ifrsBookId),
                    await CreateAndPostAsync(
                        entryDate, description, CloneLines(lineItems, null), userId, referenceNumber, sourceModule, gaapBookId)),
            };
        }

        /// <summary>
        /// Creates the entry and posts it; the entry is marked posted only when the journal service says it posted.
        /// </summary>
        private async Task<JOURNAL_ENTRY> CreateAndPostAsync(
            DateTime entryDate,
            string description,
            List<JOURNAL_ENTRY_LINE> lineItems,
            string userId,
            string? referenceNumber,
            string? sourceModule,
            string bookId)
        {
            var entry = await _journalEntryService.CreateEntryAsync(
                entryDate,
                description,
                lineItems,
                userId,
                referenceNumber,
                sourceModule,
                bookId);
            if (!await _journalEntryService.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId))
                throw new InvalidOperationException($"Journal {entry.JOURNAL_ENTRY_ID} was created but not posted.");
            entry.STATUS = AccountingReferenceCodes.JournalEntryStatusCodes.Posted;
            return entry;
        }

        private static List<JOURNAL_ENTRY_LINE> CloneLines(
            List<JOURNAL_ENTRY_LINE> lineItems,
            Func<string?, string?>? accountMap)
        {
            var cloned = new List<JOURNAL_ENTRY_LINE>(lineItems.Count);
            var props = typeof(JOURNAL_ENTRY_LINE).GetProperties();

            foreach (var line in lineItems)
            {
                var copy = new JOURNAL_ENTRY_LINE();
                foreach (var prop in props)
                {
                    if (!prop.CanRead || !prop.CanWrite)
                        continue;
                    prop.SetValue(copy, prop.GetValue(line));
                }

                copy.JOURNAL_ENTRY_LINE_ID = null;
                copy.JOURNAL_ENTRY_ID = null;
                if (accountMap != null)
                {
                    copy.GL_ACCOUNT_ID = accountMap(copy.GL_ACCOUNT_ID);
                }

                cloned.Add(copy);
            }

            return cloned;
        }

        public Task<bool> PostExistingEntryAsync(string journalEntryId, string userId)
        {
            return _journalEntryService.PostEntryAsync(journalEntryId, userId);
        }
    }
}


