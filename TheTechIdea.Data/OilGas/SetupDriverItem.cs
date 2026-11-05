namespace TheTechIdea.Data.OilGas;

/// <summary>A server-registered Beep driver package, without filesystem or connection secrets.</summary>
public sealed record SetupDriverItem(string Id, string Package, string DataSourceType, string Category, string? PackageVersion, bool Loaded, bool Cached)
{
    public bool ProviderLoaded { get; init; }
    public bool ConnectorLoaded { get; init; }
    public string? ConnectorPackage { get; init; }
    public string? ConnectorVersion { get; init; }
    public bool BundledConnector { get; init; }
    public bool CanInstall { get; init; }
    /// <summary>Whether a loaded SQL Server connector declares the owned-transaction contract; null when not assessed.</summary>
    public bool? SqlServerOwnedTransactionContract { get; init; }
}
