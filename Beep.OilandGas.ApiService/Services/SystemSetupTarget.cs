using System.Security.Cryptography;
using System.Text.Json;
using System.Data;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Installation-wide Beep target, configured independently of tenant PPDM module bindings.</summary>
public sealed class SystemSetupTarget(IConfiguration configuration, IDMEEditor editor)
{
    public (string ConnectionName, IDataSource Source, string Fingerprint) Resolve()
    {
        var name = configuration["SystemSetup:ConnectionName"]?.Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 128 || name.Any(char.IsControl))
            throw new InvalidOperationException("Configure SystemSetup:ConnectionName with a registered Beep connection.");
        var connections = editor.ConfigEditor?.DataConnections ?? throw new InvalidOperationException("The Beep connection registry is unavailable.");
        var matches = connections.Where(c => string.Equals(c.ConnectionName, name, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (matches.Length != 1) throw new InvalidOperationException("The system setup connection is missing or ambiguous.");
        var target = ModuleDatabaseTarget.Key(editor, matches[0]);
        if (!string.Equals(configuration["SystemSetup:ConnectionTarget"], target, StringComparison.Ordinal))
            throw new InvalidOperationException("The configured system database target is missing or has changed. Operator recovery is required.");
        var source = editor.GetDataSource(matches[0].ConnectionName) ?? throw new InvalidOperationException("The system setup datasource is unavailable.");
        MigrationConnectionTarget.Validate(editor, source, matches[0].ConnectionName);
        if (source.ConnectionStatus != ConnectionState.Open && source.Openconnection() != ConnectionState.Open)
            throw new InvalidOperationException("The system setup datasource could not be opened.");
        var fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(new
        { Name = matches[0].ConnectionName.ToUpperInvariant(), Target = MigrationConnectionTarget.Fingerprint(matches[0]) })));
        return (matches[0].ConnectionName, source, fingerprint);
    }

    public string FingerprintForMigration(IReadOnlyList<string> modules, string connection)
    {
        if (modules.Count != 1 || !string.Equals(modules[0], "SYSTEM_SETUP", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("System setup must be migrated independently of tenant modules.");
        var target = Resolve();
        if (!string.Equals(target.ConnectionName, connection, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The system setup migration target changed.");
        return target.Fingerprint;
    }
}
