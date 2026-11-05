using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Core.Refusals;
#if BEEP_SECURE_CONNECTIONS
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Utilities;
#endif

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Chooses the bootstrap pointer to a registered Beep system repository.</summary>
public sealed class SystemSetupConnectionSelectionService(IConfiguration configuration, IDMEEditor editor)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    public static string SettingsPath(IConfiguration configuration) => Path.Combine(
        Path.GetFullPath(configuration["Beep:ConfigPath"] ?? Path.Combine(AppContext.BaseDirectory, "Config")), "system-setup-target.json");

    /// <summary>Observe a selection saved by another process or before a lost response.</summary>
    public static void ReloadPersistedSelectionIfNeeded(IConfiguration configuration)
    {
        if (!string.IsNullOrWhiteSpace(configuration["SystemSetup:ConnectionName"]) || !File.Exists(SettingsPath(configuration))) return;
        if (configuration is not IConfigurationRoot root)
            throw new InvalidOperationException("The saved system target configuration cannot be reloaded.");
        try { root.Reload(); }
        catch (Exception error) when (error is IOException or InvalidDataException or FormatException or UnauthorizedAccessException)
        { throw new InvalidOperationException("The saved system target configuration could not be reloaded.", error); }
        if (string.IsNullOrWhiteSpace(configuration["SystemSetup:ConnectionName"]) ||
            string.IsNullOrWhiteSpace(configuration["SystemSetup:ConnectionTarget"]))
            throw new InvalidOperationException("The saved system target configuration is incomplete or hidden by a higher-priority setting.");
    }

    public IReadOnlyList<string> Connections()
    {
        var registered = editor.ConfigEditor?.DataConnections ?? throw new InvalidOperationException("The Beep connection registry is unavailable.");
        var eligible = new List<string>();
        foreach (var group in registered.Where(c => BeepConnectionName.IsValid(c.ConnectionName))
                     .GroupBy(c => c.ConnectionName, StringComparer.OrdinalIgnoreCase))
        {
            // A name must identify one concrete supported database before it can be selected.
            if (group.Count() != 1) continue;
            var connection = group.Single();
            try { _ = ModuleDatabaseTarget.Key(editor, connection); eligible.Add(connection.ConnectionName); }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException or RefusalException) { }
        }
        return eligible.OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public async Task<SystemSetupConnectionSelection> SelectAsync(string name, Func<Task> authority, CancellationToken token)
    {
        if (!BeepConnectionName.IsValid(name))
            throw new InvalidOperationException("Select a valid registered connection.");
        await Gate.WaitAsync(token);
        try
        {
            await authority();
            ReloadPersistedSelectionIfNeeded(configuration);
            var existing = configuration["SystemSetup:ConnectionName"];
            if (!string.IsNullOrWhiteSpace(existing) && !string.Equals(existing, name, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("The system repository is already configured; changing its target requires an operator-led migration.");
            var connections = editor.ConfigEditor?.DataConnections ?? throw new InvalidOperationException("The Beep connection registry is unavailable.");
            var matches = connections.Where(x => string.Equals(x.ConnectionName, name, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("The selected connection is missing or ambiguous.");
            var properties = matches[0];
            var target = ModuleDatabaseTarget.Key(editor, properties);
            var source = editor.GetDataSource(properties.ConnectionName) ?? throw new InvalidOperationException("The selected datasource is unavailable.");
            MigrationConnectionTarget.Validate(editor, source, properties.ConnectionName);
            var fingerprint = MigrationConnectionTarget.Fingerprint(properties);
            if (source.ConnectionStatus != ConnectionState.Open && source.Openconnection() != ConnectionState.Open)
                throw new InvalidOperationException("The selected datasource could not be opened.");
#if BEEP_SECURE_CONNECTIONS
            if (properties.DatabaseType == DataSourceType.SqlServer &&
                (source is not IIsolationOwnedTransactionDataSource sqlOwner || !sqlOwner.SupportsOwnedTransactions))
                throw new InvalidOperationException("The SQL Server connector does not support owned serializable transactions. Install a current connector before selecting it.");
#endif
            // Setup records require a serializable, owned transaction. Establish that
            // capability before persisting a target that could never save its setup.
            await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
            {
                await authority(); token.ThrowIfCancellationRequested();
                PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable);
            });
            await authority(); token.ThrowIfCancellationRequested();
            var current = editor.ConfigEditor?.DataConnections ?? throw new InvalidOperationException("The Beep connection registry is unavailable.");
            if (current.Count(x => string.Equals(x.ConnectionName, name, StringComparison.OrdinalIgnoreCase)) != 1 ||
                !ReferenceEquals(current.Single(x => string.Equals(x.ConnectionName, name, StringComparison.OrdinalIgnoreCase)), properties) ||
                !ReferenceEquals(editor.GetDataSource(properties.ConnectionName), source) ||
                MigrationConnectionTarget.Fingerprint(properties) != fingerprint)
                throw new InvalidOperationException("The selected connection changed while it was being checked.");
            MigrationConnectionTarget.Validate(editor, source, properties.ConnectionName);
            if (ModuleDatabaseTarget.Key(editor, properties) != target)
                throw new InvalidOperationException("The selected database target changed while it was being checked.");
            if (!string.IsNullOrWhiteSpace(existing))
            {
                if (!string.Equals(configuration["SystemSetup:ConnectionTarget"], target, StringComparison.Ordinal))
                    throw new InvalidOperationException("The selected system database target changed; operator recovery is required.");
                return new(properties.ConnectionName);
            }
            var path = SettingsPath(configuration); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(new { SystemSetup = new { ConnectionName = properties.ConnectionName, ConnectionTarget = target } }), token);
                await authority(); token.ThrowIfCancellationRequested();
                current = editor.ConfigEditor?.DataConnections ?? throw new InvalidOperationException("The Beep connection registry is unavailable.");
                if (current.Count(x => string.Equals(x.ConnectionName, name, StringComparison.OrdinalIgnoreCase)) != 1 ||
                    !ReferenceEquals(current.Single(x => string.Equals(x.ConnectionName, name, StringComparison.OrdinalIgnoreCase)), properties) ||
                    !ReferenceEquals(editor.GetDataSource(properties.ConnectionName), source) ||
                    MigrationConnectionTarget.Fingerprint(properties) != fingerprint || ModuleDatabaseTarget.Key(editor, properties) != target)
                    throw new InvalidOperationException("The selected connection changed while it was being checked.");
                MigrationConnectionTarget.Validate(editor, source, properties.ConnectionName);
                File.Move(temporary, path, false);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            if (configuration is not IConfigurationRoot root) throw new InvalidOperationException("The system target configuration cannot be reloaded.");
            root.Reload();
            if (!string.Equals(configuration["SystemSetup:ConnectionName"], properties.ConnectionName, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(configuration["SystemSetup:ConnectionTarget"], target, StringComparison.Ordinal))
                throw new InvalidOperationException("The saved system target could not be confirmed. Check the server configuration.");
            await authority();
            return new(properties.ConnectionName);
        }
        finally { Gate.Release(); }
    }
}
