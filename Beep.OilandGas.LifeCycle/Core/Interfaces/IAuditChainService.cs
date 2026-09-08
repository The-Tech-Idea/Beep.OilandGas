using Beep.OilandGas.LifeCycle.Data.Tables;

namespace Beep.OilandGas.UserManagement.Services;

/// <summary>
/// Provides cryptographic audit chain integrity for PROCESS_HISTORY entries.
/// Each history entry's hash includes the previous entry's hash, creating an
/// immutable chain. Any tampering is detectable via VerifyChainIntegrityAsync.
/// Required for SOX, SEC, and ISO 27001 non-repudiation compliance.
/// Part of Phase 4 governance & compliance.
/// </summary>
public interface IAuditChainService
{
    /// <summary>
    /// Compute the chain hash for a new history entry, chaining from the previous entry.
    /// The resulting hash should be stored in the entry's EVENT_DATA_JSON or a dedicated field.
    /// </summary>
    Task<string> ComputeChainHashAsync(
        string? previousEntryHash,
        string processHistoryId,
        string eventType,
        DateTime eventDate,
        string userId,
        string? fromStatus,
        string? toStatus,
        string? details);

    /// <summary>
    /// Verify the integrity of the entire audit chain for a process instance.
    /// Returns whether the chain is intact and details of any breaks.
    /// </summary>
    Task<ChainVerificationResult> VerifyChainIntegrityAsync(string processInstanceId);

    /// <summary>
    /// When a new history entry is inserted, chain it to the previous entry.
    /// </summary>
    Task ChainHistoryEntryAsync(PROCESS_HISTORY entry, string userId);
}

public class ChainVerificationResult
{
    public bool IsIntact { get; set; }
    public int TotalEntries { get; set; }
    public int VerifiedEntries { get; set; }
    public List<ChainBreakInfo> Breaks { get; set; } = new();
    public DateTime VerificationTimestamp { get; set; } = DateTime.UtcNow;
}

public class ChainBreakInfo
{
    public int EntryIndex { get; set; }
    public string? ProcessHistoryId { get; set; }
    public string? ExpectedHash { get; set; }
    public string? ComputedHash { get; set; }
    public string BreakType { get; set; } = "HASH_MISMATCH";
}
