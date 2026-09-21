namespace TheTechIdea.Data.OilGas;

/// <summary>Repository coordination state, not a financial posting or a time-expiring lease.</summary>
public sealed class FinancialOperationClaim
{
    public string OperationKey { get; set; } = "";
    public long Version { get; set; }
    public string? OwnerId { get; set; }
    public string? Token { get; set; }
    public string ChangedBy { get; set; } = "";
    public DateTime ChangedUtc { get; set; }
}

public sealed record FinancialClaimHandle(string OperationKey, long Version, string OwnerId, string Token);
