using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Constants;
using TheTechIdea.Beep.Report;
using JournalStatus = Beep.OilandGas.Models.Constants.AccountingReferenceCodes.JournalEntryStatusCodes;

namespace Beep.OilandGas.ProductionAccounting.Services
{
    /// <summary>
    /// How a closed period is recognized, written and read in one place: the close posts a journal with
    /// <c>SOURCE_MODULE</c> PERIOD_CLOSING and <c>REFERENCE_NUMBER</c> <c>CLOSE-{field}-{yyyyMM}</c>, and every reader of a
    /// period's status (the production summary, the close page, the accounting aggregation) asks for that journal through
    /// this service. Two readers had looked for a journal no close writes (<c>ENTRY_NUMBER</c> "PC-…", <c>ENTRY_TYPE</c>
    /// PERIOD_CLOSE; the journal service numbers every entry "JE-…" and types it GENERAL), so every period read "Open".
    /// </summary>
    public partial class PeriodClosingService
    {
        private const string ClosingReferencePrefix = "CLOSE-";

        private const string UnfinishedClosingJournal =
            "An unfinished closing journal requires review before this period can be closed.";

        /// <summary>The reference the close writes on the field's closing journal for the period's month.</summary>
        private static string ClosingReference(string fieldId, DateTime periodEnd) =>
            ClosingReferencePrefix + fieldId + "-" + periodEnd.ToString("yyyyMM", CultureInfo.InvariantCulture);

        /// <summary>Reads a closing reference back into its field and month; false for any other reference.</summary>
        private static bool TryReadClosingReference(string reference, out string fieldId, out DateTime periodStart)
        {
            fieldId = string.Empty;
            periodStart = default;
            if (string.IsNullOrWhiteSpace(reference) || !reference.StartsWith(ClosingReferencePrefix, StringComparison.Ordinal))
                return false;

            // A field id may hold hyphens; the month is what follows the last one.
            var monthSeparator = reference.LastIndexOf('-');
            if (monthSeparator <= ClosingReferencePrefix.Length)
                return false;
            if (!DateTime.TryParseExact(reference.Substring(monthSeparator + 1), "yyyyMM", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out periodStart))
                return false;

            fieldId = reference.Substring(ClosingReferencePrefix.Length, monthSeparator - ClosingReferencePrefix.Length);
            return true;
        }

        /// <summary>What one closing journal says of its period; null when it closes nothing (voided).</summary>
        private static string CloseStatusOf(JOURNAL_ENTRY journal)
        {
            if (string.Equals(journal.STATUS, JournalStatus.Posted, StringComparison.OrdinalIgnoreCase))
                return PeriodCloseStatusCodes.Closed;
            if (string.Equals(journal.STATUS, JournalStatus.Draft, StringComparison.OrdinalIgnoreCase))
                return PeriodCloseStatusCodes.Closing;
            if (string.Equals(journal.STATUS, JournalStatus.Reversed, StringComparison.OrdinalIgnoreCase))
                return PeriodCloseStatusCodes.Reopened;
            return null;
        }

        /// <summary>
        /// A period's status from its closing journals: closed while one stands, closing while one was never posted,
        /// reopened once every one was reversed, open when there is none.
        /// </summary>
        private static string PeriodStatusOf(IEnumerable<JOURNAL_ENTRY> journals)
        {
            var statuses = journals.Select(CloseStatusOf).Where(status => status != null).ToList();
            if (statuses.Contains(PeriodCloseStatusCodes.Closed))
                return PeriodCloseStatusCodes.Closed;
            if (statuses.Contains(PeriodCloseStatusCodes.Closing))
                return PeriodCloseStatusCodes.Closing;
            if (statuses.Contains(PeriodCloseStatusCodes.Reopened))
                return PeriodCloseStatusCodes.Reopened;
            return PeriodCloseStatusCodes.Open;
        }

        /// <summary>The active closing journals, every one or those under <paramref name="reference"/>.</summary>
        private async Task<List<JOURNAL_ENTRY>> ReadClosingJournalsAsync(string reference)
        {
            var repo = await CreateRepositoryAsync<JOURNAL_ENTRY>("JOURNAL_ENTRY");
            var filters = new List<AppFilter>
            {
                new AppFilter { FieldName = "SOURCE_MODULE", Operator = "=", FilterValue = AccountingSourceModuleCodes.PeriodClosing },
                new AppFilter { FieldName = "ACTIVE_IND", Operator = "=", FilterValue = _defaults.GetActiveIndicatorYes() }
            };
            if (reference != null)
                filters.Add(new AppFilter { FieldName = "REFERENCE_NUMBER", Operator = "=", FilterValue = reference });

            var rows = await repo.GetAsync(filters);
            return rows?.OfType<JOURNAL_ENTRY>().ToList() ?? new List<JOURNAL_ENTRY>();
        }

        /// <inheritdoc />
        public async Task<string> GetPeriodStatusAsync(string fieldId, DateTime periodEnd)
        {
            if (string.IsNullOrWhiteSpace(fieldId))
                throw new ArgumentNullException(nameof(fieldId));
            if (periodEnd == default)
                throw RefusalException.Invalid("A period end date is required.");

            return PeriodStatusOf(await ReadClosingJournalsAsync(ClosingReference(fieldId, periodEnd)));
        }

        /// <inheritdoc />
        public async Task<List<PeriodCloseSummary>> GetPeriodClosesAsync(string fieldId)
        {
            var closes = new List<PeriodCloseSummary>();
            foreach (var journal in await ReadClosingJournalsAsync(reference: null))
            {
                // The close writes every PERIOD_CLOSING journal under a closing reference; one under any other is not a close.
                if (!TryReadClosingReference(journal.REFERENCE_NUMBER, out var journalField, out var periodStart))
                    continue;
                if (!string.IsNullOrWhiteSpace(fieldId)
                    && !string.Equals(journalField, fieldId, StringComparison.OrdinalIgnoreCase))
                    continue;

                var status = CloseStatusOf(journal);
                if (status == null)
                    continue; // a voided closing journal closed nothing

                closes.Add(new PeriodCloseSummary
                {
                    FieldId = journalField,
                    PeriodEnd = journal.ENTRY_DATE?.Date ?? periodStart.AddMonths(1).AddDays(-1),
                    Status = status,
                    JournalEntryNumber = string.IsNullOrWhiteSpace(journal.ENTRY_NUMBER) ? journal.JOURNAL_ENTRY_ID : journal.ENTRY_NUMBER,
                    ClosedBy = journal.ROW_CREATED_BY,
                    ClosedAt = journal.ROW_CREATED_DATE
                });
            }

            return closes
                .OrderByDescending(close => close.PeriodEnd)
                .ThenByDescending(close => close.ClosedAt ?? DateTime.MinValue)
                .ToList();
        }

        /// <summary>
        /// Posts the closing journal for the field's period: revenue and royalty closed to retained earnings, or, for a
        /// period with neither, a single zero line, so that every close leaves the journal it is recognized by.
        /// </summary>
        private async Task PostPeriodCloseEntry(
            string fieldId,
            FieldProduction production,
            DateTime periodEnd,
            string userId)
        {
            _logger?.LogInformation("Posting period close GL entry for field {FieldId} as of {PeriodEnd}", fieldId, periodEnd.ToShortDateString());

            var referenceNumber = ClosingReference(fieldId, periodEnd);

            // The close refused a closed or unfinished period before it began; asked again for a close that ran beside it.
            var existingStatus = PeriodStatusOf(await ReadClosingJournalsAsync(referenceNumber));
            if (existingStatus == PeriodCloseStatusCodes.Closing)
                throw RefusalException.Conflict(UnfinishedClosingJournal);
            if (existingStatus == PeriodCloseStatusCodes.Closed)
                throw RefusalException.Conflict(
                    $"The period ending {periodEnd.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} was closed for field {fieldId} by another close while this one ran.");

            var (grossRevenue, totalRoyalty) = await GetRevenueRoyaltyTotalsAsync(production, periodEnd);
            var netIncome = grossRevenue - totalRoyalty;
            var month = periodEnd.ToString("MMMM yyyy", CultureInfo.InvariantCulture);

            var lines = new List<JOURNAL_ENTRY_LINE>();
            var lineNumber = 1;

            if (grossRevenue > 0m)
            {
                lines.Add(CloseLine(DefaultGlAccounts.Revenue, lineNumber++, grossRevenue, 0m,
                    PeriodCloseJournalLineDescriptionPhrases.CloseRevenue, userId));
            }

            if (totalRoyalty > 0m)
            {
                lines.Add(CloseLine(DefaultGlAccounts.RoyaltyExpense, lineNumber++, 0m, totalRoyalty,
                    PeriodCloseJournalLineDescriptionPhrases.CloseRoyaltyExpense, userId));
            }

            if (netIncome != 0m)
            {
                var retainedDebit = netIncome < 0m ? Math.Abs(netIncome) : 0m;
                var retainedCredit = netIncome > 0m ? netIncome : 0m;
                lines.Add(CloseLine(DefaultGlAccounts.RetainedEarnings, lineNumber++, retainedDebit, retainedCredit,
                    PeriodCloseJournalLineDescriptionPhrases.CloseNetIncomeToRetainedEarnings, userId));
            }

            string entryDescription;
            if (lines.Count == 0)
            {
                // A period with no revenue or royalty still closes. It wrote no journal, so it read "Open" after the close
                // said it closed; the zero line moves no balance (posting writes no ledger row for it).
                lines.Add(CloseLine(DefaultGlAccounts.RetainedEarnings, lineNumber, 0m, 0m,
                    PeriodCloseJournalLineDescriptionPhrases.NoActivityToClose, userId));
                entryDescription = $"Period close for {month}: no revenue or royalty activity to close";
            }
            else
            {
                entryDescription = $"Period close summary for {month} (Gross={grossRevenue}, Royalty={totalRoyalty})";
            }

            var entry = await _journalEntries.CreateEntryAsync(
                periodEnd, entryDescription, lines, userId,
                referenceNumber, AccountingSourceModuleCodes.PeriodClosing);
            if (!await _journalEntries.PostEntryAsync(entry.JOURNAL_ENTRY_ID, userId))
                throw new InvalidOperationException($"Period close journal {entry.JOURNAL_ENTRY_ID} was created but not posted.");

            _logger?.LogInformation(
                "Period close GL entry {EntryId} posted for field {FieldId} as of {PeriodEnd}",
                entry.JOURNAL_ENTRY_ID, fieldId, periodEnd.ToShortDateString());
        }

        private JOURNAL_ENTRY_LINE CloseLine(
            string glAccountId, int lineNumber, decimal debit, decimal credit, string description, string userId) =>
            new JOURNAL_ENTRY_LINE
            {
                JOURNAL_ENTRY_LINE_ID = Guid.NewGuid().ToString(),
                GL_ACCOUNT_ID = glAccountId,
                LINE_NUMBER = lineNumber,
                DEBIT_AMOUNT = debit,
                CREDIT_AMOUNT = credit,
                DESCRIPTION = description,
                ACTIVE_IND = _defaults.GetActiveIndicatorYes(),
                PPDM_GUID = Guid.NewGuid().ToString(),
                ROW_CREATED_BY = userId,
                ROW_CREATED_DATE = DateTime.UtcNow
            };
    }
}
