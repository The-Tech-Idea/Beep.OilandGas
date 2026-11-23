namespace Beep.OilandGas.Models.Data;

/// <summary>A new module database target; this path never accepts passwords or arbitrary connection strings.</summary>
public sealed record LocalDbDatabaseTargetRequest(string ConnectionName, string Server, string Database);

public sealed record LocalDbDatabaseTargetReview(LocalDbDatabaseTargetRequest Target, string ReviewToken,
    bool DatabaseExists, bool OwnershipConfirmed, bool RequiresReconciliation, string Message)
{
    public bool CanCreate => !DatabaseExists && !OwnershipConfirmed && !RequiresReconciliation;
}

public sealed record LocalDbDatabaseTargetCreate(LocalDbDatabaseTargetRequest Target, string ReviewToken);

public sealed record LocalDbDatabaseTargetResult(LocalDbDatabaseTargetRequest Target, string ReviewToken,
    bool OwnershipConfirmed);
