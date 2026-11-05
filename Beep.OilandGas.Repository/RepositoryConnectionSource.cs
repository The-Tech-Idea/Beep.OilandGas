using Microsoft.Extensions.Configuration;

namespace Beep.OilandGas.Repository;

/// <summary>One captured identity target per request scope; connection strings never leave the server.</summary>
public sealed record RepositoryConnectionSettings(string Provider, string ConnectionString);
public interface IRepositoryConnectionSource { RepositoryConnectionSettings? Resolve(); }
public sealed class ConfigurationRepositoryConnectionSource(IConfiguration configuration) : IRepositoryConnectionSource
{
    public RepositoryConnectionSettings? Resolve()
    {
        var provider = configuration["Repository:Provider"]?.Trim().ToUpperInvariant();
        var connection = configuration["Repository:ConnectionString"];
        return !string.IsNullOrWhiteSpace(connection) && provider is "SQLSERVER" or "POSTGRESQL" or "ORACLE"
            ? new(provider, connection) : null;
    }
}
public sealed record RepositoryConnectionSnapshot(RepositoryConnectionSettings? Settings)
{
    public static RepositoryConnectionSnapshot Capture(RepositoryConnectionSettings? settings)
    {
        var provider = settings?.Provider?.Trim().ToUpperInvariant();
        return new RepositoryConnectionSnapshot(!string.IsNullOrWhiteSpace(settings?.ConnectionString) && provider is "SQLSERVER" or "POSTGRESQL" or "ORACLE"
            ? new RepositoryConnectionSettings(provider, settings.ConnectionString) : null);
    }
    public string Require(string provider) => Settings is { } value && value.Provider == provider && !string.IsNullOrWhiteSpace(value.ConnectionString)
        ? value.ConnectionString : throw new InvalidOperationException("The identity repository connection is unavailable or has changed provider.");
}
