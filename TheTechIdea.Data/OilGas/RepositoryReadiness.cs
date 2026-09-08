namespace TheTechIdea.Data.OilGas;

public sealed record RepositoryStatusResponse(string Status);

public enum RepositoryReadiness
{
    Unavailable,
    MigrationRequired,
    BootstrapRequired,
    Ready,
    RecoveryRequired
}
