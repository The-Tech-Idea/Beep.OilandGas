namespace Beep.OilandGas.Models.Data.ProductionAccounting;

public enum RoyaltyPostingFinding { NoJournalRequired, MissingJournal, AmbiguousJournals, JournalMismatch, IncompletePosting, MatchingPostedEvidence }
public sealed record RoyaltyPostingReview(string RecordId, string RecordKind, string RecordedStatus,
    decimal? Amount, string? LinkedJournalId, RoyaltyPostingFinding Finding,
    System.Collections.Generic.List<string> CandidateJournalIds);
