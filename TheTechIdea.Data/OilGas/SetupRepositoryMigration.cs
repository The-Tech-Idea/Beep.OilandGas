namespace TheTechIdea.Data.OilGas;

/// <summary>Migration identifiers for the server-configured identity repository; no connection settings are returned.</summary>
public sealed record SetupRepositoryMigrationPlan(string Provider, string TargetToken, bool CreatesDatabase, IReadOnlyList<string> PendingMigrations);
public sealed record SetupRepositoryMigrationRequest(string Provider, string TargetToken, bool CreatesDatabase, IReadOnlyList<string> PendingMigrations);
public sealed record SetupRepositoryMigrationResult(RepositoryReadiness Readiness);
