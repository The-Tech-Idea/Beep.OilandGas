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
                _ => await _journalEntryService.CreateDualBalancedEntryFromKeysAsync(
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
                _ => (await _journalEntryService.CreateBalancedEntryAsync(
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
                throw new ArgumentException("Journal entry must have at least one line item", nameof(lineItems));

            var ifrsBookId = string.IsNullOrWhiteSpace(bookId) ? AccountingBooks.Ifrs : bookId;
            var gaapBookId = string.IsNullOrWhiteSpace(bookId) ? AccountingBooks.Gaap : bookId;

            switch (basis)
            {
                case AccountingBasis.Ifrs:
                    {
                        var entry = await _journalEntryService.CreateEntryAsync(
                            entryDate,
                            description,
                            lineItems,
                            userId,
                            referenceNumber,
                            sourceModule,
                            ifrsBookId);
                        await _journalEntryService.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId);
                        entry.STATUS = "POSTED";
                        return (entry, null);
                    }
                case AccountingBasis.Gaap:
                    {
                        var entry = await _journalEntryService.CreateEntryAsync(
                            entryDate,
                            description,
                            lineItems,
                            userId,
                            referenceNumber,
                            sourceModule,
                            gaapBookId);
                        await _journalEntryService.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId);
                        entry.STATUS = "POSTED";
                        return (null, entry);
                    }
                default:
                    {
                        var ifrsLines = CloneLines(lineItems, null);
                        var gaapLines = CloneLines(lineItems, null);
                        var ifrsEntry = await _journalEntryService.CreateEntryAsync(
                            entryDate,
                            description,
                            ifrsLines,
                            userId,
                            referenceNumber,
                            sourceModule,
                            ifrsBookId);
                        await _journalEntryService.PostEntryAsync(ifrsEntry.JOURNAL_ENTRY_ID, userId);
                        ifrsEntry.STATUS = "POSTED";

                        var gaapEntry = await _journalEntryService.CreateEntryAsync(
                            entryDate,
                            description,
                            gaapLines,
                            userId,
                            referenceNumber,
                            sourceModule,
                            gaapBookId);
                        await _journalEntryService.PostEntryAsync(gaapEntry.JOURNAL_ENTRY_ID, userId);
                        gaapEntry.STATUS = "POSTED";
                        return (ifrsEntry, gaapEntry);
                    }
            }
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


