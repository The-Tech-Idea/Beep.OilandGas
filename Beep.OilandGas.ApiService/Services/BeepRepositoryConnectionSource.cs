using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Resolves an identity connection from Beep only when its physical target still matches the selected target.</summary>
public sealed class BeepRepositoryConnectionSource(IConfiguration configuration, IServiceProvider services) : IRepositoryConnectionSource
{
    public RepositoryConnectionSettings? Resolve()
    {
        try
        {
            SetupRepositoryConnectionSelectionService.ReloadPersistedSelectionIfNeeded(configuration);
            var name = configuration["Repository:BeepConnectionName"];
            if (string.IsNullOrWhiteSpace(name)) return new ConfigurationRepositoryConnectionSource(configuration).Resolve();
            if (!string.IsNullOrWhiteSpace(configuration["Repository:Provider"]) || !string.IsNullOrWhiteSpace(configuration["Repository:ConnectionString"])) return null;
            var editor = services.GetRequiredService<IDMEEditor>();
            var candidate = Candidate(editor, name);
            return string.Equals(candidate.Target, configuration["Repository:BeepConnectionTarget"], StringComparison.Ordinal)
                ? candidate.Settings : null;
        }
        catch (Exception error) when (error is InvalidOperationException or ArgumentException or RefusalException) { return null; }
    }

    public static (RepositoryConnectionSettings Settings, string Target) Candidate(IDMEEditor editor, string name)
    {
        if (!Beep.OilandGas.Models.Data.DataManagement.BeepConnectionName.IsValid(name)) throw new InvalidOperationException("Select a valid Beep connection.");
        var matches = editor.ConfigEditor?.DataConnections?.Where(c => string.Equals(c.ConnectionName, name, StringComparison.OrdinalIgnoreCase)).ToArray() ?? [];
        if (matches.Length != 1) throw new InvalidOperationException("The Beep connection is missing or ambiguous.");
        return Candidate(editor, matches[0]);
    }

    public static (RepositoryConnectionSettings Settings, string Target) Candidate(IDMEEditor editor, ConnectionProperties connection)
    {
        var provider = connection.DatabaseType.ToString() switch
        {
            "SqlServer" => "SQLSERVER", "Postgre" or "PostgreSql" => "POSTGRESQL", "Oracle" => "ORACLE",
            _ => throw new InvalidOperationException("The identity repository requires SQL Server, PostgreSQL or Oracle.")
        };
        var value = connection.ConnectionString;
        if (string.IsNullOrWhiteSpace(value) || value.Contains('{') || value.Contains('}'))
            throw new InvalidOperationException("The Beep connection needs a complete provider connection string.");
        return (new(provider, value), ModuleDatabaseTarget.Key(editor, connection));
    }
}
