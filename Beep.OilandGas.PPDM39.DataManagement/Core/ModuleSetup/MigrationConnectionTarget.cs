using System.Security.Cryptography;
using System.Text.Json;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;

public static class MigrationConnectionTarget
{
    public static void Validate(IDMEEditor editor, IDataSource source, string connectionName)
    {
        var matches = editor.ConfigEditor.DataConnections.Where(connection =>
            string.Equals(connection.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase)).ToList();
        if (matches.Count != 1 || !string.Equals(source.DatasourceName, connectionName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The migration datasource is missing or ambiguous.");
        var active = source.Dataconnection?.ConnectionProp;
        if (active is null || !string.Equals(Fingerprint(matches[0]), Fingerprint(active), StringComparison.Ordinal))
            throw new InvalidOperationException("The cached datasource does not match the configured migration target. Reload the datasource before planning again.");
    }

    public static string Fingerprint(IConnectionProperties connection)
    {
        // Hash credentials with the target settings; never return or log their serialized values.
        var target = new
        {
            connection.GuidID, connection.DatabaseType, connection.Category,
            connection.DriverName, connection.DriverVersion, connection.Host, connection.Port,
            connection.Database, connection.SchemaName, connection.OracleSIDorService,
            connection.FilePath, connection.FileName, connection.Url, connection.ConnectionString,
            connection.UserID, connection.Password, connection.Parameters,
            ParameterList = connection.ParameterList?.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray(),
            connection.IntegratedSecurity, connection.TrustedConnection, connection.UseWindowsAuthentication,
            connection.ReadOnly, connection.IsInMemory, connection.IsComposite,
            CompositeLayerName = (connection as ConnectionProperties)?.CompositeLayerName,
            connection.UseSSL, connection.RequireSSL, connection.SSLMode, connection.EncryptConnection,
            connection.TrustServerCertificate, connection.BypassServerCertificateValidation
        };
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(target)));
    }
}
