namespace TheTechIdea.Data.OilGas;

public sealed record ModuleBindingRequest(string ConnectionName, string? ConcurrencyStamp);
public sealed record ModuleSeedRequest(string ConcurrencyStamp);
public sealed record ModuleDatabaseSummary(string ModuleId, string ModuleName, string? ConnectionName, string? ConcurrencyStamp);
public sealed record ModulePlanRequest(string EnvironmentTier = "Development", bool BackupConfirmed = false,
    bool RestoreTestEvidenceProvided = false, string? RestoreTestEvidence = null, string? ConcurrencyStamp = null);
