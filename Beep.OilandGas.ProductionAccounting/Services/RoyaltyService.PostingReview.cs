using Beep.OilandGas.Accounting.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.ProductionAccounting.Exceptions;

namespace Beep.OilandGas.ProductionAccounting.Services;

public partial class RoyaltyService
{
    /// <summary>Diagnostic evidence only. This is not a transaction snapshot or permission to replay posting.</summary>
    public async Task<List<RoyaltyPostingReview>> ReviewPostingsAsync(string royaltyId)
    {
        var royalty = await GetAsync(royaltyId) ?? throw new RoyaltyException("Stored royalty calculation not found.");
        var result = new List<RoyaltyPostingReview> {
            await ReviewPostingAsync(royalty.ROYALTY_CALCULATION_ID, "Accrual", royalty.ROYALTY_STATUS,
                royalty.ROYALTY_AMOUNT, royalty.JOURNAL_ENTRY_ID, royalty.ACTIVE_IND,
                DefaultGlAccounts.RoyaltyExpense, DefaultGlAccounts.AccruedRoyalties)
        };
        var payments = await GetPaymentsAsync(royaltyId);
        if (payments.Any(p => string.IsNullOrWhiteSpace(p.ROYALTY_PAYMENT_ID)) ||
            payments.Select(p => p.ROYALTY_PAYMENT_ID).Distinct().Count() != payments.Count)
            throw new RoyaltyException("Payment identities are missing or ambiguous.");
        foreach (var payment in payments.OrderBy(p => p.ROYALTY_PAYMENT_ID, StringComparer.Ordinal))
            result.Add(await ReviewPostingAsync(payment.ROYALTY_PAYMENT_ID, "Payment", payment.STATUS,
                payment.NET_PAYMENT_AMOUNT, payment.JOURNAL_ENTRY_ID, payment.ACTIVE_IND,
                DefaultGlAccounts.AccruedRoyalties, DefaultGlAccounts.Cash));
        return result;
    }

    private async Task<RoyaltyPostingReview> ReviewPostingAsync(string id, string kind, string status,
        decimal? amount, string? linkedId, string active, string debitAccount, string creditAccount)
    {
        var evidence = await _glService.GetPostingEvidenceAsync(id);
        RoyaltyPostingFinding finding;
        if (active != _defaults.GetActiveIndicatorYes() || amount is null or < 0 || (kind == "Payment" && amount == 0))
            finding = RoyaltyPostingFinding.JournalMismatch;
        else if (evidence.Count == 0)
            finding = amount == 0 && string.IsNullOrWhiteSpace(linkedId)
                ? RoyaltyPostingFinding.NoJournalRequired : RoyaltyPostingFinding.MissingJournal;
        else if (evidence.Count != 1) finding = RoyaltyPostingFinding.AmbiguousJournals;
        else
        {
            var e = evidence[0];
            var h = e.Header;
            bool SamePair(IEnumerable<(string Account, decimal? Debit, decimal? Credit)> rows)
            {
                var items = rows.ToList();
                return items.Count == 2 &&
                    items.Count(x => x.Account == debitAccount && x.Debit == amount && (x.Credit ?? 0) == 0) == 1 &&
                    items.Count(x => x.Account == creditAccount && x.Credit == amount && (x.Debit ?? 0) == 0) == 1;
            }
            bool Unique(IEnumerable<string> ids) { var list = ids.ToList(); return list.All(x => !string.IsNullOrWhiteSpace(x)) && list.Distinct().Count() == list.Count; }
            if (string.IsNullOrWhiteSpace(h.JOURNAL_ENTRY_ID) || h.REFERENCE_NUMBER != id ||
                (!string.IsNullOrWhiteSpace(linkedId) && linkedId != h.JOURNAL_ENTRY_ID) ||
                h.SOURCE_MODULE != "PRODUCTION_ACCOUNTING" || h.ACTIVE_IND != _defaults.GetActiveIndicatorYes() ||
                h.TOTAL_DEBIT != amount || h.TOTAL_CREDIT != amount || amount == 0 ||
                e.Lines.Any(l => l.JOURNAL_ENTRY_ID != h.JOURNAL_ENTRY_ID || l.ACTIVE_IND != _defaults.GetActiveIndicatorYes()) ||
                !Unique(e.Lines.Select(l => l.JOURNAL_ENTRY_LINE_ID)) ||
                !SamePair(e.Lines.Select(l => (l.GL_ACCOUNT_ID, l.DEBIT_AMOUNT, l.CREDIT_AMOUNT))))
                finding = RoyaltyPostingFinding.JournalMismatch;
            else if (h.STATUS != "POSTED" ||
                e.LedgerEntries.Any(l => l.JOURNAL_ENTRY_ID != h.JOURNAL_ENTRY_ID || l.ACTIVE_IND != _defaults.GetActiveIndicatorYes()) ||
                !Unique(e.LedgerEntries.Select(l => l.GL_ENTRY_ID)) ||
                !SamePair(e.LedgerEntries.Select(l => (l.GL_ACCOUNT_ID, l.DEBIT_AMOUNT, l.CREDIT_AMOUNT))))
                finding = RoyaltyPostingFinding.IncompletePosting;
            else finding = RoyaltyPostingFinding.MatchingPostedEvidence;
        }
        return new(id, kind, status, amount, linkedId, finding,
            evidence.Select(e => e.Header.JOURNAL_ENTRY_ID).OrderBy(x => x, StringComparer.Ordinal).ToList());
    }
}
