using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Refusals;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using System.Data;

namespace Beep.OilandGas.ApiService.Services;

public sealed class SetupRepositoryConnectionSelectionService(IConfiguration configuration, IDMEEditor editor, ILogger<SetupRepositoryConnectionSelectionService>? logger = null)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    public static string SettingsPath(IConfiguration configuration) => Path.Combine(
        Path.GetFullPath(configuration["Beep:ConfigPath"] ?? Path.Combine(AppContext.BaseDirectory, "Config")), "identity-repository-target.json");

    /// <summary>Observe a target saved by another process or before a response was lost.</summary>
    public static void ReloadPersistedSelectionIfNeeded(IConfiguration configuration)
    {
        if ((!string.IsNullOrWhiteSpace(configuration["Repository:BeepConnectionName"]) &&
             !string.IsNullOrWhiteSpace(configuration["Repository:BeepConnectionTarget"])) || !File.Exists(SettingsPath(configuration))) return;
        if (configuration is not IConfigurationRoot root)
            throw new InvalidOperationException("The saved identity target configuration cannot be reloaded.");
        try { root.Reload(); }
        catch (Exception error) when (error is IOException or InvalidDataException or FormatException or UnauthorizedAccessException)
        { throw new InvalidOperationException("The saved identity target configuration could not be reloaded.", error); }
        if (string.IsNullOrWhiteSpace(configuration["Repository:BeepConnectionName"]) ||
            string.IsNullOrWhiteSpace(configuration["Repository:BeepConnectionTarget"]))
            throw new InvalidOperationException("The saved identity target configuration is incomplete or hidden by a higher-priority setting.");
    }

    public IReadOnlyList<SetupRepositoryConnectionOption> Connections()
    {
        var names = editor.ConfigEditor?.DataConnections?.Select(c => c.ConnectionName).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            ?? throw new InvalidOperationException("The Beep connection registry is unavailable.");
        var result = new List<SetupRepositoryConnectionOption>();
        foreach (var name in names)
        {
            try { var candidate = BeepRepositoryConnectionSource.Candidate(editor, name); result.Add(new(name, candidate.Settings.Provider)); }
            catch (Exception error) when (error is InvalidOperationException or ArgumentException or RefusalException) { }
        }
        return result.OrderBy(x => x.ConnectionName, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public async Task<SetupRepositoryConnectionTest> TestAsync(string name, Func<Task> authority, CancellationToken token)
    {
        await authority(); token.ThrowIfCancellationRequested();
        var candidate = BeepRepositoryConnectionSource.Candidate(editor, name);
        var registered = editor.ConfigEditor.DataConnections.Single(c => string.Equals(c.ConnectionName, name, StringComparison.OrdinalIgnoreCase));
        var fingerprint = MigrationConnectionTarget.Fingerprint(registered);
        // ConnectionProbe changes the supplied name. Use a detached copy so the registry is never modified.
        var copy = CopyForProbe(registered);
        if (MigrationConnectionTarget.Fingerprint(copy) != fingerprint)
            throw new InvalidOperationException("The connection could not be copied for testing. Review it and try again.");
        bool open;
        try { open = ConnectionProbe.Test(editor, copy) == ConnectionState.Open; }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            logger?.LogWarning("The setup connection probe failed ({ErrorType})", error.GetType().Name);
            open = false;
        }
        await authority(); token.ThrowIfCancellationRequested();
        var current = BeepRepositoryConnectionSource.Candidate(editor, name);
        var currentRegistered = editor.ConfigEditor.DataConnections.Single(c => string.Equals(c.ConnectionName, name, StringComparison.OrdinalIgnoreCase));
        if (current != candidate || MigrationConnectionTarget.Fingerprint(currentRegistered) != fingerprint)
            throw new InvalidOperationException("The connection changed while it was tested. Review it and test again.");
        return new(name, open);
    }

    internal static ConnectionProperties CopyForProbe(ConnectionProperties source) => new()
    {
        GuidID = source.GuidID, ConnectionName = source.ConnectionName, DatabaseType = source.DatabaseType,
        Category = source.Category, IsDatabase = source.IsDatabase, DriverName = source.DriverName,
        DriverVersion = source.DriverVersion, Host = source.Host, Port = source.Port, Database = source.Database,
        SchemaName = source.SchemaName, OracleSIDorService = source.OracleSIDorService, FilePath = source.FilePath,
        FileName = source.FileName, Url = source.Url, ConnectionString = source.ConnectionString,
        UserID = source.UserID, Password = source.Password, Parameters = source.Parameters,
        ParameterList = source.ParameterList is null ? null : new Dictionary<string, string>(source.ParameterList),
        IntegratedSecurity = source.IntegratedSecurity, TrustedConnection = source.TrustedConnection,
        UseWindowsAuthentication = source.UseWindowsAuthentication, ReadOnly = source.ReadOnly,
        IsInMemory = source.IsInMemory, IsComposite = source.IsComposite, CompositeLayerName = source.CompositeLayerName,
        UseSSL = source.UseSSL, RequireSSL = source.RequireSSL, SSLMode = source.SSLMode,
        EncryptConnection = source.EncryptConnection, TrustServerCertificate = source.TrustServerCertificate,
        BypassServerCertificateValidation = source.BypassServerCertificateValidation
    };

    public async Task<SetupRepositoryConnectionSelection> SelectAsync(string name, Func<Task> authority, CancellationToken token)
    {
        await Gate.WaitAsync(token);
        try
        {
            await authority();
            ReloadPersistedSelectionIfNeeded(configuration);
            if (!string.IsNullOrWhiteSpace(configuration["Repository:Provider"]) || !string.IsNullOrWhiteSpace(configuration["Repository:ConnectionString"]))
                throw new InvalidOperationException("The identity repository is already configured by the server operator.");
            var existing = configuration["Repository:BeepConnectionName"];
            var candidate = BeepRepositoryConnectionSource.Candidate(editor, name);
            if (!string.IsNullOrWhiteSpace(existing))
            {
                if (!string.Equals(existing, name, StringComparison.OrdinalIgnoreCase) || configuration["Repository:BeepConnectionTarget"] != candidate.Target)
                    throw new InvalidOperationException("The identity repository is already configured; changing its target requires operator recovery.");
                return new(name);
            }
            await authority(); token.ThrowIfCancellationRequested();
            var closing = BeepRepositoryConnectionSource.Candidate(editor, name);
            if (closing != candidate) throw new InvalidOperationException("The Beep connection changed while it was being selected.");
            var path = SettingsPath(configuration); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(new { Repository = new { BeepConnectionName = name, BeepConnectionTarget = candidate.Target } }), token);
                await authority(); token.ThrowIfCancellationRequested();
                if (BeepRepositoryConnectionSource.Candidate(editor, name) != candidate)
                    throw new InvalidOperationException("The Beep connection changed while it was being selected.");
                File.Move(temporary, path, false);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            if (configuration is not IConfigurationRoot root) throw new InvalidOperationException("The identity repository configuration cannot be reloaded.");
            root.Reload();
            if (!string.Equals(configuration["Repository:BeepConnectionName"], name, StringComparison.OrdinalIgnoreCase) ||
                configuration["Repository:BeepConnectionTarget"] != candidate.Target)
                throw new InvalidOperationException("The saved identity target could not be confirmed.");
            return new(name);
        }
        finally { Gate.Release(); }
    }
}
