using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;
using TheTechIdeaWeb.Diagnostics;
using System.IO;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Connections;

/// <summary>What the desktop knows about one connection.</summary>
public sealed record OgConnectionInfo(
    string Name,
    string Provider,
    string? Location,
    OgConnectionState State,
    string? Detail,
    bool IsLocal = false,
    bool IsFile = false);

/// <summary>A request to create (or repair) a local file database.</summary>
public sealed record OgLocalDatabaseRequest(string Name, string Directory, string FileName);

/// <summary>
/// A request to register a company database on a server. The password travels to BeepDM's connection store,
/// which protects it with the host cipher, so it is never written in clear text.
/// </summary>
public sealed record OgRemoteDatabaseRequest(
    string Name,
    string Provider,
    string Host,
    int Port,
    string Database,
    string? UserId = null,
    string? Password = null,
    string? Schema = null,
    bool UseWindowsAuthentication = false,
    bool TrustServerCertificate = false,
    string? ConnectionString = null);


/// <summary>One installed provider, as the setup screens offer it.</summary>
/// <param name="Type">The database type name BeepDM uses, e.g. <c>SqlServer</c>.</param>
/// <param name="DriverName">The driver package, when the driver records one.</param>
/// <param name="CreateLocal">True when the driver advertises local file creation.</param>
/// <param name="InMemory">True for a provider that keeps nothing on disk.</param>
public sealed record OgConnectionProvider(
    string Type,
    string? DriverName,
    bool CreateLocal,
    bool InMemory);

/// <summary>
/// The connection surface. This is the one place in the desktop that holds BeepDM types, and it holds
/// them behind this interface so no view model ever sees <c>IDMEEditor</c>.
/// </summary>
public interface IOgConnectionCatalog
{
    Task<OgResult<IReadOnlyList<OgConnectionInfo>>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>The database products this installation has a driver for, so a setup screen names none.</summary>
    Task<OgResult<IReadOnlyList<OgConnectionProvider>>> ProvidersAsync(
        CancellationToken cancellationToken = default);

    Task<OgResult<OgConnectionInfo>> TestAsync(string name, CancellationToken cancellationToken = default);

    Task<OgResult<OgConnectionInfo>> EnsureLocalAsync(
        OgLocalDatabaseRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a company database on a server and proves it opens. Unlike the local path there is nothing to
    /// create: the schema belongs to the module setup, and this only makes the connection usable.
    /// </summary>
    Task<OgResult<OgConnectionInfo>> EnsureRemoteAsync(
        OgRemoteDatabaseRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renames a configured connection to the name this installation uses (see <c>OgPaths.ConnectionName</c>), so
    /// every service bound to that name reads the database the person chose. The credential stays protected: the
    /// connection keeps its identity, only its name and definition change.
    /// </summary>
    Task<OgResult<OgConnectionInfo>> AdoptAsync(
        string sourceName,
        string targetName,
        CancellationToken cancellationToken = default);

    Task<OgResult> RemoveAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// DAT-01: connections over BeepDM. Listing reads the configured connections, testing opens the data
/// source and checks that BeepDM produced one, and <see cref="EnsureLocalAsync"/> provisions a local
/// file database using whichever installed driver advertises local creation — no database product is
/// named in code.
/// </summary>
public sealed class BeepDmConnectionCatalog : IOgConnectionCatalog
{
    private readonly OgBeepHost _host;
    private readonly IFailureReporter _failures;

    public BeepDmConnectionCatalog(OgBeepHost host, IFailureReporter failures)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    private IDMEEditor Editor => _host.Editor;

    public Task<OgResult<IReadOnlyList<OgConnectionInfo>>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connections = Editor.ConfigEditor?.DataConnections ?? [];
            IReadOnlyList<OgConnectionInfo> items = connections.Select(Describe).ToList();
            return Task.FromResult(OgResult<IReadOnlyList<OgConnectionInfo>>.Ok(items));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                "listing the configured data connections",
                "no connections are shown",
                FailureSeverity.Error);

            return Task.FromResult(OgResult<IReadOnlyList<OgConnectionInfo>>.Failed(
                "The connection list could not be read.", reference));
        }
    }

    public Task<OgResult<IReadOnlyList<OgConnectionProvider>>> ProvidersAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<OgConnectionProvider> providers =
                (Editor.ConfigEditor?.DataDriversClasses ?? [])
                .Where(driver => !string.IsNullOrWhiteSpace(driver.DatasourceType.ToString()))
                .GroupBy(driver => driver.DatasourceType)
                .Select(group =>
                {
                    // One row per product: the first driver that can create a local file wins the flag, because a
                    // product may ship several drivers and only one of them writes files.
                    var driver = group.FirstOrDefault(item => item.CreateLocal && !item.InMemory) ?? group.First();

                    return new OgConnectionProvider(
                        group.Key.ToString(),
                        driver.PackageName,
                        group.Any(item => item.CreateLocal && !item.InMemory),
                        driver.InMemory);
                })
                .OrderBy(provider => provider.Type, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult(OgResult<IReadOnlyList<OgConnectionProvider>>.Ok(providers));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                "listing the installed database drivers",
                "no database products are offered",
                FailureSeverity.Error);

            return Task.FromResult(OgResult<IReadOnlyList<OgConnectionProvider>>.Failed(
                $"The installed database drivers could not be listed. Reference: {reference}",
                OgConnectionFailureClassifier.CodeDriverMissing));
        }
    }

    public Task<OgResult<OgConnectionInfo>> TestAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var connection = Find(name);
            if (connection is null)
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"No connection named '{name}' is configured.", "connection-not-found"));
            }

            // A connection whose database type no installed driver can open fails the same way whatever the
            // server does, so it is reported before anything is opened.
            if (!OgConnectionFailureClassifier.HasDriverFor(
                    Editor.ConfigEditor?.DataDriversClasses,
                    connection.DatabaseType))
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"No installed driver can open {connection.DatabaseType}.",
                    OgConnectionFailureClassifier.CodeDriverMissing));
            }

            var file = ConnectionFile(connection);
            if (file is not null && !File.Exists(file))
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"The database file is missing: {file}",
                    OgConnectionFailureClassifier.CodeDatabaseMissing));
            }

            var state = Editor.OpenDataSource(connection.ConnectionName);
            var source = Editor.GetDataSource(connection.ConnectionName);

            if (source is null)
            {
                var providerMessage = Editor.ErrorObject?.Ex?.Message
                                      ?? Editor.ErrorObject?.Message
                                      ?? $"The provider reported {state} and produced no data source.";

                var (classified, code) = OgConnectionFailureClassifier.Classify(providerMessage);

                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    OgConnectionFailureClassifier.Describe(classified),
                    code));
            }

            return Task.FromResult(OgResult<OgConnectionInfo>.Ok(
                Describe(connection) with
                {
                    State = OgConnectionState.Ok,
                    Detail = source.DatasourceType.ToString(),
                }));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"testing the connection '{name}'",
                "the connection was not proven usable",
                FailureSeverity.Error);

            // AC-2: the caller gets a code it can act on, and the failure record keeps the full detail.
            var (classified, code) = OgConnectionFailureClassifier.Classify(ex);

            return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                $"{OgConnectionFailureClassifier.Describe(classified)} Reference: {reference}",
                code));
        }
    }

    public Task<OgResult<OgConnectionInfo>> EnsureLocalAsync(
        OgLocalDatabaseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var driver = OgDriverDiscovery.FirstLocalDriver(_host.Beep);
            if (driver is null)
            {
                // AC-3: no silent fallback to a driver that cannot create a file. The refusal names the
                // requirement so an operator knows what the installation is missing.
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    "No installed driver advertises local database creation.",
                    OgConnectionFailureClassifier.CodeDriverMissing));
            }

            var directory = Path.GetFullPath(request.Directory);
            var file = Path.Combine(directory, request.FileName);
            Directory.CreateDirectory(directory);

            var connection = new ConnectionProperties
            {
                ConnectionName = request.Name,
                DatabaseType = driver.DatasourceType,
                DriverName = driver.DriverClass,
                Category = driver.DatasourceCategory,
                FilePath = directory,
                FileName = request.FileName,
                IsLocal = true,
                IsRemote = false,
                IsFile = true,
                IsInMemory = driver.InMemory,
            };

            // A second run replaces the definition rather than failing on a duplicate name.
            Editor.ConfigEditor.RemoveDataConnection(connection.ConnectionName);
            Editor.ConfigEditor.AddDataConnection(connection);
            Editor.ConfigEditor.SaveDataconnectionsValues();
            Editor.ConfigEditor.SaveConfigValues();

            // Ask the driver itself to create the file; opening a missing file is provider-dependent.
            var source = Editor.CreateNewDataSourceConnection(connection, connection.ConnectionName);

            if (source is ILocalDB local && !local.CreateDB(file))
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"The provider refused to create '{file}'.", "local-file-not-created"));
            }

            Editor.OpenDataSource(connection.ConnectionName);

            if (!File.Exists(file))
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"The provider did not create '{file}'.", "local-file-not-created"));
            }

            return Task.FromResult(OgResult<OgConnectionInfo>.Ok(
                Describe(connection) with
                {
                    State = OgConnectionState.Ok,
                    Detail = $"{file}  ·  driver: {driver.DatasourceType} (advertises local creation)",
                }));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating the local database '{request.Name}'",
                "no local database was created",
                FailureSeverity.Error);

            return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                "The local database could not be created.", reference));
        }
    }

    public Task<OgResult<OgConnectionInfo>> EnsureRemoteAsync(
        OgRemoteDatabaseRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Enum.TryParse<DataSourceType>(request.Provider, ignoreCase: true, out var databaseType))
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"'{request.Provider}' is not a database type this build knows.",
                    OgConnectionFailureClassifier.CodeDriverMissing));
            }

            // AC-3 shape: a database type no installed driver can open is refused before the server is contacted.
            if (!OgConnectionFailureClassifier.HasDriverFor(
                    Editor.ConfigEditor?.DataDriversClasses,
                    databaseType))
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"No installed driver can open {databaseType}. Install the driver, then try again.",
                    OgConnectionFailureClassifier.CodeDriverMissing));
            }

            var usesConnectionString = !string.IsNullOrWhiteSpace(request.ConnectionString);
            var userAndPassword = !request.UseWindowsAuthentication && !usesConnectionString;

            var connection = new ConnectionProperties
            {
                ConnectionName = request.Name,
                DatabaseType = databaseType,
                Category = DatasourceCategory.RDBMS,
                Host = request.Host,
                Port = request.Port,
                Database = request.Database,
                SchemaName = request.Schema,
                ConnectionString = request.ConnectionString,
                UserID = request.UseWindowsAuthentication ? string.Empty : request.UserId,
                Password = request.UseWindowsAuthentication ? string.Empty : request.Password,
                IsLocal = false,
                IsRemote = true,
                IsFile = false,
                IsDatabase = true,
                IsInMemory = false,
                UseWindowsAuthentication = request.UseWindowsAuthentication,
                UseUserAndPassword = userAndPassword,
                // The connection store protects the password with the host cipher, so it is never written as text.
                SavePassword = userAndPassword && !string.IsNullOrEmpty(request.Password),
                TrustServerCertificate = request.TrustServerCertificate,
                Timeout = 30,
            };

            // A second run replaces the definition rather than failing on a duplicate name.
            Editor.ConfigEditor.RemoveDataConnection(connection.ConnectionName);
            Editor.ConfigEditor.AddDataConnection(connection);
            Editor.ConfigEditor.SaveDataconnectionsValues();
            Editor.ConfigEditor.SaveConfigValues();

            var state = Editor.OpenDataSource(connection.ConnectionName);
            var source = Editor.GetDataSource(connection.ConnectionName);

            if (source is null)
            {
                var providerMessage = Editor.ErrorObject?.Ex?.Message
                                      ?? Editor.ErrorObject?.Message
                                      ?? $"The provider reported {state} and produced no data source.";

                var (classified, code) = OgConnectionFailureClassifier.Classify(providerMessage);

                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    OgConnectionFailureClassifier.Describe(classified),
                    code));
            }

            // Proving it opens is not proving it is usable: the table list is the first read every page makes.
            int tableCount;
            try
            {
                tableCount = source.GetEntitesList()?.Count() ?? 0;
            }
            catch (Exception ex)
            {
                var reference = _failures.ReportHandled(
                    ex,
                    $"reading the table list of '{request.Name}'",
                    "the server answered but its tables could not be read",
                    FailureSeverity.Error);

                var (classified, code) = OgConnectionFailureClassifier.Classify(ex);

                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"{OgConnectionFailureClassifier.Describe(classified)} Reference: {reference}",
                    code));
            }

            var endpoint = request.Port > 0
                ? $"{request.Host}:{request.Port}/{request.Database}"
                : $"{request.Host}/{request.Database}";

            return Task.FromResult(OgResult<OgConnectionInfo>.Ok(
                Describe(connection) with
                {
                    State = OgConnectionState.Ok,
                    Detail = tableCount == 0
                        ? $"{endpoint}  ·  driver: {databaseType} (opened; the database has no tables yet)"
                        : $"{endpoint}  ·  driver: {databaseType} ({tableCount} table(s))",
                }));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"registering the company database '{request.Name}'",
                "the company database was not registered",
                FailureSeverity.Error);

            var (classified, code) = OgConnectionFailureClassifier.Classify(ex);

            return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                $"{OgConnectionFailureClassifier.Describe(classified)} Reference: {reference}",
                code));
        }
    }

    public Task<OgResult<OgConnectionInfo>> AdoptAsync(
        string sourceName,
        string targetName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceName);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetName);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = Find(sourceName);

            if (source is null)
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"No connection named '{sourceName}' is configured.",
                    "connection-not-found"));
            }

            if (string.Equals(source.ConnectionName, targetName, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(OgResult<OgConnectionInfo>.Ok(Describe(source)));
            }

            var taken = Find(targetName);

            if (taken is not null && !string.Equals(taken.GuidID, source.GuidID, StringComparison.OrdinalIgnoreCase))
            {
                // Refused rather than overwritten: another definition pointing at another database is not this
                // installation's to replace, and replacing it would leave its owner reading the wrong data.
                return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                    $"The name '{targetName}' is already used by another connection. Remove it first, or choose another name.",
                    "connection-name-in-use"));
            }

            var identity = source.GuidID;
            var previousName = source.ConnectionName;

            // The open source is bound to the old name; closing it here keeps one datasource per definition.
            if (Editor.GetDataSource(previousName) is not null)
            {
                Editor.CloseDataSource(previousName);
                Editor.RemoveDataDource(previousName);
            }

            source.ConnectionName = targetName;
            Editor.ConfigEditor.UpdateDataConnection(source, identity);
            Editor.ConfigEditor.SaveDataconnectionsValues();
            Editor.ConfigEditor.SaveConfigValues();

            var state = Editor.OpenDataSource(targetName);
            var opened = Editor.GetDataSource(targetName);

            return Task.FromResult(opened is null
                ? OgResult<OgConnectionInfo>.Failed(
                    $"The connection was renamed but did not open (state {state}).",
                    OgConnectionFailureClassifier.CodeFailed)
                : OgResult<OgConnectionInfo>.Ok(Describe(source) with
                {
                    State = OgConnectionState.Ok,
                    Detail = $"adopted as '{targetName}'",
                }));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"adopting the connection '{sourceName}' as '{targetName}'",
                "the connection was not adopted",
                FailureSeverity.Error);

            return Task.FromResult(OgResult<OgConnectionInfo>.Failed(
                $"The connection could not be adopted. Reference: {reference}",
                OgConnectionFailureClassifier.CodeFailed));
        }
    }

    public Task<OgResult> RemoveAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var existing = Find(name);
            if (existing is null)
            {
                return Task.FromResult(OgResult.Failed(
                    $"No connection named '{name}' is configured.", "connection-not-found"));
            }

            // Refuse while the data source is open: removing it would leave a live source without a definition.
            if (Editor.GetDataSource(existing.ConnectionName) is not null)
            {
                return Task.FromResult(OgResult.Failed(
                    $"Connection '{name}' is open and cannot be removed.", "connection-in-use"));
            }

            if (Editor.ConfigEditor.RemoveDataConnection(name))
            {
                Editor.ConfigEditor.SaveDataconnectionsValues();
                Editor.ConfigEditor.SaveConfigValues();
                return Task.FromResult(OgResult.Ok($"Connection '{name}' removed."));
            }

            return Task.FromResult(OgResult.Failed(
                $"No connection named '{name}' is configured.", "connection-not-found"));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"removing the connection '{name}'",
                "the connection is still configured",
                FailureSeverity.Error);

            return Task.FromResult(OgResult.Failed("The connection could not be removed.", reference));
        }
    }

    private ConnectionProperties? Find(string name) =>
        Editor.ConfigEditor?.DataConnections?.FirstOrDefault(connection =>
            string.Equals(connection.ConnectionName, name, StringComparison.OrdinalIgnoreCase));

    private static string? ConnectionFile(ConnectionProperties connection)
    {
        if (!connection.IsFile)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(connection.FilePath) || string.IsNullOrWhiteSpace(connection.FileName))
        {
            return null;
        }

        return Path.Combine(connection.FilePath, connection.FileName);
    }

    private static OgConnectionInfo Describe(ConnectionProperties connection) =>
        new(
            connection.ConnectionName ?? "(unnamed)",
            connection.DatabaseType.ToString(),
            ConnectionFile(connection) ?? connection.Database ?? connection.Host,
            connection.IsFile && ConnectionFile(connection) is { } file && !File.Exists(file)
                ? OgConnectionState.MissingDatabase
                : OgConnectionState.NeverTested,
            null,
            connection.IsLocal,
            connection.IsFile);
}
