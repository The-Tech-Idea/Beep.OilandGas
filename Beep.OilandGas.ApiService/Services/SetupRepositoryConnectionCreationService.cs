using Microsoft.Extensions.Configuration;
using System.Data.Common;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Creates a Beep registry entry for first-time identity setup.</summary>
public sealed class SetupRepositoryConnectionCreationService(IConfiguration configuration, IDMEEditor editor, SetupDriverService drivers)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    // Beep 3.1.1 persists connection strings as plaintext. Only non-secret connection options can be written until
    // the application uses a Beep package with verified credential protection and an appropriate key provider.
    private static readonly HashSet<string> SafeKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "server", "datasource", "address", "addr", "networkaddress", "host", "port", "database", "initialcatalog",
        "integratedsecurity", "trustedconnection", "userid", "user", "username", "authentication",
        "encrypt", "trustservercertificate", "sslmode", "connecttimeout", "connectiontimeout", "timeout", "pooling", "applicationname"
    };
    public async Task<SetupRepositoryConnectionSelection> CreateAsync(SetupRepositoryConnectionCreate request, Func<Task> authority, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!BeepConnectionName.IsValid(request.ConnectionName) || string.IsNullOrWhiteSpace(request.DriverId) || string.IsNullOrWhiteSpace(request.ConnectionString) ||
            request.ConnectionString.Length > 8192 || request.ConnectionString.Any(char.IsControl))
            throw new InvalidOperationException("Enter a valid connection name and connection string.");
        DbConnectionStringBuilder parsed;
        try { parsed = new DbConnectionStringBuilder { ConnectionString = request.ConnectionString }; }
        catch (ArgumentException) { throw new InvalidOperationException("Enter a valid connection string."); }
        var hasSecretOptions = parsed.Keys.Cast<string>().Any(key => !SafeKeys.Contains(new string(key.Where(char.IsAsciiLetterOrDigit).ToArray())));
#if !BEEP_SECURE_CONNECTIONS && !BEEP_SOURCE_TEST
        if (hasSecretOptions)
            throw new InvalidOperationException("This Beep installation cannot protect connection credentials. Use external authentication or configure a protected Beep runtime.");
#endif
        var (type, catalogType) = request.Provider?.Trim().ToUpperInvariant() switch
        {
            "SQLSERVER" => (DataSourceType.SqlServer, "SqlServer"),
            "POSTGRESQL" => (DataSourceType.Postgre, "Postgre"),
            "ORACLE" => (DataSourceType.Oracle, "Oracle"),
            _ => throw new InvalidOperationException("Select a supported identity repository provider.")
        };
        var proposed = new ConnectionProperties { ConnectionName = request.ConnectionName, DatabaseType = type,
            Category = DatasourceCategory.RDBMS, IsDatabase = true, ConnectionString = request.ConnectionString, GuidID = Guid.NewGuid().ToString() };
        await Gate.WaitAsync(token);
        try
        {
            await authority();
            SetupRepositoryConnectionSelectionService.ReloadPersistedSelectionIfNeeded(configuration);
            if (!string.IsNullOrWhiteSpace(configuration["Repository:Provider"]) || !string.IsNullOrWhiteSpace(configuration["Repository:ConnectionString"]) ||
                !string.IsNullOrWhiteSpace(configuration["Repository:BeepConnectionName"]))
                throw new InvalidOperationException("The identity repository is already configured.");
            var registry = editor.ConfigEditor ?? throw new InvalidOperationException("The Beep connection registry is unavailable.");
#if BEEP_SECURE_CONNECTIONS || BEEP_SOURCE_TEST
            if (registry is IConnectionProtectionContext protection && registry is IConnectionConfigurationPersistence)
            {
                var protectedConnection = protection.ConnectionSecretProtector.Protect(proposed);
                if (!string.IsNullOrEmpty(protectedConnection.ConnectionString) || string.IsNullOrEmpty(protectedConnection.ProtectedCredentialPayload) ||
                    protection.ConnectionSecretProtector.Unprotect(protectedConnection).ConnectionString != proposed.ConnectionString)
                    throw new InvalidOperationException("Beep credential protection could not be confirmed.");
            }
            else if (hasSecretOptions)
                throw new InvalidOperationException("This Beep installation cannot protect connection credentials. Use external authentication or configure a protected Beep runtime.");
#endif
            var intended = BeepRepositoryConnectionSource.Candidate(editor, proposed);
            var installed = drivers.Catalog().Where(x => x.Id == request.DriverId && x.Loaded &&
                string.Equals(x.DataSourceType, catalogType, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (installed.Length != 1)
                throw new InvalidOperationException("Select a loaded Beep connector and provider for this database.");
            var registered = registry.DataDriversClasses.Where(x => x.GuidID == installed[0].Id).ToArray();
            if (registered.Length != 1) throw new InvalidOperationException("The selected Beep driver is unavailable or ambiguous.");
            proposed.DriverName = registered[0].PackageName;
            proposed.DriverVersion = registered[0].version;
            var existing = registry.DataConnections.Where(x => string.Equals(x.ConnectionName, request.ConnectionName, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (existing.Length > 0)
            {
                if (existing.Length == 1 && BeepRepositoryConnectionSource.Candidate(editor, existing[0]) == intended &&
                    existing[0].DriverName == proposed.DriverName && existing[0].DriverVersion == proposed.DriverVersion)
                {
                    await authority(); token.ThrowIfCancellationRequested();
                    Save(registry, token);
                    if (!Confirmed(registry, proposed, intended))
                        throw new InvalidOperationException("The saved connection could not be confirmed.");
                    await authority(); token.ThrowIfCancellationRequested();
                    return new(existing[0].ConnectionName);
                }
                throw new InvalidOperationException("The connection name is already in use.");
            }
            await authority(); token.ThrowIfCancellationRequested();
            if (registry.DataConnections.Any(x => string.Equals(x.ConnectionName, request.ConnectionName, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("The connection name is already in use.");
            if (!registry.AddDataConnection(proposed)) throw new InvalidOperationException("The Beep connection could not be added.");
            Save(registry, token);
            if (!Confirmed(registry, proposed, intended)) throw new InvalidOperationException("The saved connection could not be confirmed.");
            await authority(); token.ThrowIfCancellationRequested();
            return new(request.ConnectionName);
        }
        finally { Gate.Release(); }
    }

    private bool Confirmed(IConfigEditor registry, ConnectionProperties proposed, (Beep.OilandGas.Repository.RepositoryConnectionSettings Settings, string Target) intended)
    {
        var matches = registry.DataConnections.Where(x => string.Equals(x.ConnectionName, proposed.ConnectionName, StringComparison.OrdinalIgnoreCase)).ToArray();
        return matches.Length == 1 && matches[0].DriverName == proposed.DriverName && matches[0].DriverVersion == proposed.DriverVersion &&
            BeepRepositoryConnectionSource.Candidate(editor, matches[0]) == intended;
    }

    private static void Save(IConfigEditor registry, CancellationToken token)
    {
#if BEEP_SECURE_CONNECTIONS || BEEP_SOURCE_TEST
        if (registry is IConnectionConfigurationPersistence persistence)
        {
            persistence.SaveDataConnectionsAcknowledged(token).ThrowIfNotSaved();
            return;
        }
#endif
        registry.SaveDataconnectionsValues();
    }
}
