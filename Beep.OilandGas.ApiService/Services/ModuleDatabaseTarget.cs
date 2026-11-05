using System.Security.Cryptography;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Npgsql;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Helpers.ConnectionHelpers;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>A credential-free configured database identity, independent of connection names, driver versions and schemas.</summary>
public static class ModuleDatabaseTarget
{
    public static string Key(IDMEEditor editor, string connectionName)
    {
        var connections = editor.ConfigEditor.DataConnections.Where(connection =>
            string.Equals(connection.ConnectionName, connectionName, StringComparison.OrdinalIgnoreCase)).ToArray();
        if (connections.Length != 1) throw RefusalException.Conflict("Select one uniquely configured BeepDM connection.");
        return Key(editor, connections[0]);
    }

    public static string Key(IDMEEditor editor, IConnectionProperties configured)
    {
        try { return BuildKey(editor, configured); }
        catch (ArgumentException) { throw RefusalException.Invalid("Configure a valid concrete database target before assigning ownership."); }
        catch (KeyNotFoundException) { throw RefusalException.Invalid("Configure a valid concrete database target before assigning ownership."); }
    }

    private static string BuildKey(IDMEEditor editor, IConnectionProperties configured)
    {
        if (configured.IsInMemory || configured.IsComposite || !string.IsNullOrWhiteSpace(configured.Parameters) ||
            configured.ParameterList?.Count > 0)
            throw RefusalException.Invalid("The module requires a concrete database target without composite or parameter overrides.");
        // Beep's processor can normalize paths and initialize strings. Work on a copy, never mutate ConfigEditor while reading.
        var copy = new ConnectionProperties
        {
            ConnectionName = configured.ConnectionName, ConnectionString = configured.ConnectionString,
            Host = configured.Host, Port = configured.Port, Database = configured.Database,
            FilePath = configured.FilePath, FileName = configured.FileName, Url = configured.Url,
            UserID = configured.UserID, Password = configured.Password, OracleSIDorService = configured.OracleSIDorService
        };
        var value = string.IsNullOrWhiteSpace(copy.ConnectionString) ? null : copy.ConnectionString;
        if (value is not null && System.Text.RegularExpressions.Regex.IsMatch(value,
                @"\{(Host|Port|DataBase|UserID|Password|File|FilePath|FileName|Url)\}", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            value = ConnectionStringProcessingHelper.ReplaceValueFromConnectionString(new ConnectionDriversConfig(), copy, editor);
        var provider = configured.DatabaseType.ToString();
        object target = provider switch
        {
            "SqlServer" => SqlServer(value, copy),
            "Postgre" or "PostgreSql" => PostgreSql(value, copy),
            "Oracle" => Oracle(value, copy),
            "SqlLite" or "SQLite" => Sqlite(value, copy, editor.ConfigEditor.ExePath),
            _ => throw RefusalException.Invalid("Database target ownership is unavailable for this provider.")
        };
        return Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(target)));
    }

    private static object SqlServer(string? value, IConnectionProperties copy)
    {
        var connection = value is null ? new SqlConnectionStringBuilder
        { DataSource = Required(copy.Host), InitialCatalog = Required(copy.Database) } : new SqlConnectionStringBuilder(value);
        if (!string.IsNullOrWhiteSpace(connection.AttachDBFilename))
            throw RefusalException.Invalid("Attached SQL Server files require explicit database provisioning.");
        var endpoint = Required(connection.DataSource);
        if (endpoint.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase)) endpoint = endpoint[4..];
        else if (endpoint.Contains(':') && !endpoint.StartsWith('[')) throw RefusalException.Invalid("Use a concrete TCP database endpoint.");
        var pieces = endpoint.Split(',');
        if (pieces.Length > 2) throw RefusalException.Invalid("Select one database endpoint.");
        var hostInstance = pieces[0].Split('\\');
        if (hostInstance.Length > 2) throw RefusalException.Invalid("Select one database instance.");
        var instance = hostInstance.Length == 2 ? Required(hostInstance[1]).ToLowerInvariant() : "";
        var port = pieces.Length == 2 ? Port(pieces[1], 1433) : instance.Length == 0 ? (copy.Port > 0 && value is null ? copy.Port : 1433) : 0;
        return new { Provider = "SqlServer", Host = Host(hostInstance[0]), Instance = instance, Port = port,
            Database = Required(connection.InitialCatalog).ToUpperInvariant() };
    }

    private static object PostgreSql(string? value, IConnectionProperties copy)
    {
        var connection = value is null ? new NpgsqlConnectionStringBuilder
        { Host = Required(copy.Host), Database = Required(copy.Database), Port = copy.Port > 0 ? copy.Port : 5432 } : new NpgsqlConnectionStringBuilder(value);
        var host = Required(connection.Host);
        if (host.Contains(',') || host.Contains('/') || host.Contains('\\'))
            throw RefusalException.Invalid("Use one concrete PostgreSQL network endpoint.");
        return new { Provider = "PostgreSql", Host = Host(host), connection.Port, Database = Required(connection.Database) };
    }

    private static object Oracle(string? value, IConnectionProperties copy)
    {
        var endpoint = value is null ? $"//{Required(copy.Host)}:{(copy.Port > 0 ? copy.Port : 1521)}/{Required(copy.OracleSIDorService)}" :
            new System.Data.Common.DbConnectionStringBuilder { ConnectionString = value }["Data Source"].ToString();
        if (endpoint is null || endpoint.Contains('(') || endpoint.Contains('?') || endpoint.Contains('#'))
            throw RefusalException.Invalid("Use an explicit Oracle host/port/service target; TNS aliases require identity reconciliation.");
        if (!Uri.TryCreate("oracle://" + endpoint.TrimStart('/'), UriKind.Absolute, out var uri) ||
            string.IsNullOrWhiteSpace(uri.Host) || uri.AbsolutePath is "" or "/" || !string.IsNullOrEmpty(uri.UserInfo))
            throw RefusalException.Invalid("Use an explicit Oracle host/port/service target.");
        return new { Provider = "Oracle", Host = Host(uri.Host), Port = uri.Port > 0 ? uri.Port : 1521,
            Service = uri.AbsolutePath.Trim('/').ToUpperInvariant() };
    }

    private static object Sqlite(string? value, IConnectionProperties copy, string? configuredRoot)
    {
        var connection = value is null ? new SqliteConnectionStringBuilder { DataSource = Path.Combine(copy.FilePath ?? "", copy.FileName ?? "") } : new SqliteConnectionStringBuilder(value);
        if (connection.Mode == SqliteOpenMode.Memory || connection.DataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase) ||
            connection.DataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            throw RefusalException.Invalid("Use a concrete SQLite file for module ownership.");
        var file = Required(connection.DataSource);
        if (!Path.IsPathFullyQualified(file))
        {
            if (string.IsNullOrWhiteSpace(configuredRoot) || !Path.IsPathFullyQualified(configuredRoot))
                throw RefusalException.Invalid("Configure an absolute BeepDM root before assigning a relative database file.");
            file = Path.GetFullPath(file, configuredRoot);
        }
        else file = Path.GetFullPath(file);
        // A fresh handle resolves the full existing Windows path, including parent junctions, in one OS lookup.
        if (OperatingSystem.IsWindows() && ExistingWindowsFileTarget(file) is { } existing)
            return new { Provider = "Sqlite", File = existing.ToUpperInvariant() };
        // Resolve symbolic links on existing path components as well as the final file.
        var root = Path.GetPathRoot(file)!;
        foreach (var part in file[root.Length..].Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            root = Path.Combine(root, part);
            FileAttributes attributes;
            try { attributes = File.GetAttributes(root); }
            // Missing components are valid for a database that setup has not created yet.
            // Permission and other I/O failures still propagate to the existing refusal boundary.
            catch (FileNotFoundException) { continue; }
            catch (DirectoryNotFoundException) { continue; }
            if ((attributes & FileAttributes.ReparsePoint) == 0) continue;
            FileSystemInfo info = (attributes & FileAttributes.Directory) != 0 ? new DirectoryInfo(root) : new FileInfo(root);
            if (info.LinkTarget is not null) root = info.ResolveLinkTarget(returnFinalTarget: true)?.FullName ??
                throw RefusalException.Invalid("Resolve the database file link before assigning ownership.");
        }
        return new { Provider = "Sqlite", File = OperatingSystem.IsWindows() ? root.ToUpperInvariant() : root };
    }

    private static string? ExistingWindowsFileTarget(string file)
    {
        // Device namespaces are not ordinary DOS/UNC file paths; retain the component evaluator for them.
        if (file.StartsWith(@"\\.\", StringComparison.Ordinal)) return null;
        var nativePath = file.StartsWith(@"\\?\", StringComparison.Ordinal) ? file :
            file.StartsWith(@"\\", StringComparison.Ordinal) ? @"\\?\UNC\" + file[2..] : @"\\?\" + file;
        // Zero desired access queries metadata only. Open existing, with read/write/delete sharing and directory support.
        using var handle = CreateFileW(nativePath, 0, 7, IntPtr.Zero, 3, 0x02000000, IntPtr.Zero);
        if (handle.IsInvalid)
        {
            var error = Marshal.GetLastPInvokeError();
            if (error is 2 or 3) return null; // Setup may not have created the file or its parent directory yet.
            throw new IOException("The database file target could not be resolved.", new Win32Exception(error));
        }
        var capacity = 512;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var buffer = new StringBuilder(capacity);
            var length = GetFinalPathNameByHandleW(handle, buffer, (uint)capacity, 0);
            if (length == 0) throw new IOException("The database file target could not be resolved.", new Win32Exception(Marshal.GetLastPInvokeError()));
            if (length < capacity)
            {
                var target = buffer.ToString();
                if (target.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase)) return @"\\" + target[8..];
                return target.StartsWith(@"\\?\", StringComparison.Ordinal) && target.Length > 5 && target[5] == ':' ? target[4..] : target;
            }
            if (length >= 32768) throw new IOException("The resolved database file target exceeds the supported path length.");
            capacity = checked((int)length + 1);
        }
        throw new IOException("The database file target changed repeatedly during resolution.");
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern SafeFileHandle CreateFileW(string fileName, uint desiredAccess, uint shareMode,
        IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern uint GetFinalPathNameByHandleW(SafeFileHandle handle, StringBuilder filePath, uint capacity, uint flags);

    private static string Required(string? value) => !string.IsNullOrWhiteSpace(value) && !value.Any(char.IsControl) &&
        !value.Contains('{') && !value.Contains('}') ? value.Trim() :
        throw RefusalException.Invalid("Configure an explicit database endpoint and database name.");
    private static string Host(string value)
    {
        var host = Required(value).Trim('[', ']').ToLowerInvariant();
        return host is "." or "(local)" or "localhost" or "127.0.0.1" or "::1" ? Environment.MachineName.ToLowerInvariant() : host.TrimEnd('.');
    }
    private static int Port(string value, int defaultPort) => string.IsNullOrWhiteSpace(value) ? defaultPort :
        int.TryParse(value, out var port) && port is > 0 and <= 65535 ? port : throw RefusalException.Invalid("Configure a valid database port.");
}
