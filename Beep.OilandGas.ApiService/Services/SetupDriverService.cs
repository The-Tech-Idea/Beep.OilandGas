using Beep.OilandGas.Models.Core.Refusals;
using System.Runtime.InteropServices;
using System.Reflection;
using NuGet.Versioning;
using TheTechIdea.Beep;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Provisions only drivers registered in the server's Beep catalog.</summary>
public sealed class SetupDriverService(IDMEEditor editor, IConfiguration configuration, ILogger<SetupDriverService> logger)
{
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static readonly object NativeGate = new();
    private static readonly HashSet<System.Reflection.Assembly> NativeResolvers = [];
    private IReadOnlyList<ConnectionDriversConfig> Drivers => editor.ConfigEditor?.DataDriversClasses
        ?? throw new InvalidOperationException("The Beep driver catalog is unavailable.");
    private bool ConnectorLoaded(ConnectionDriversConfig driver)
    {
        var matches = editor.ConfigEditor?.DataSourcesClasses?.Where(x => string.Equals(x.className, driver.classHandler, StringComparison.OrdinalIgnoreCase)).ToArray() ?? [];
        return matches.Length == 1 && matches[0].type is { } type && typeof(IDataSource).IsAssignableFrom(type);
    }
#if BEEP_SECURE_CONNECTIONS
    private bool RegisterBundledConnector(IAssemblyHandler loader, ConnectionDriversConfig driver)
    {
        if (loader is not AssemblyHandler handler) return false;
        var (assemblyName, typeName) = driver.DatasourceType switch
        {
            DataSourceType.SqlServer => ("SqlServerDataSourceCore", "TheTechIdea.Beep.DataBase.SQLServerDataSource"),
            DataSourceType.SqlLite => ("SqliteDatasourceCore", "TheTechIdea.Beep.DataBase.SQLiteDataSource"),
            _ => ((string?)null, (string?)null)
        };
        if (assemblyName is null || !string.Equals(driver.classHandler, typeName![(typeName.LastIndexOf('.') + 1)..], StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            // Only the known host dependencies are candidates for bundled registration.
            var assembly = Assembly.Load(new AssemblyName(assemblyName));
            var type = assembly.GetType(typeName!, false);
            if (type is null || !typeof(RDBSource).IsAssignableFrom(type) ||
                (driver.DatasourceType == DataSourceType.SqlServer && !typeof(IIsolationOwnedTransactionDataSource).IsAssignableFrom(type))) return false;
            handler.ScanAssembly(assembly);
            return ConnectorLoaded(driver);
        }
        catch (Exception error) when (error is FileNotFoundException or FileLoadException or BadImageFormatException or ReflectionTypeLoadException)
        { logger.LogWarning(error, "The bundled {DatabaseType} connector could not be registered", driver.DatasourceType); return false; }
    }
#endif
    private bool ProviderLoaded(ConnectionDriversConfig driver)
    {
        if (!driver.ADOType) return true;
        if (string.IsNullOrWhiteSpace(driver.DbConnectionType)) return false;
        // Prefer the runtime-selected asset when the provider is part of the host's
        // dependency graph. Some packages also contain nonfunctional reference DLLs.
        var assemblyName = Path.GetFileNameWithoutExtension(driver.dllname ?? string.Empty);
        var type = string.IsNullOrWhiteSpace(assemblyName) ? null :
            Type.GetType($"{driver.DbConnectionType}, {assemblyName}", throwOnError: false);
        type ??= editor.assemblyHandler?.GetType(driver.DbConnectionType);
        if (type is null || type.IsAbstract || type.IsInterface || !typeof(System.Data.IDbConnection).IsAssignableFrom(type)) return false;
        try
        {
            if (string.Equals(driver.DbConnectionType, "System.Data.SQLite.SQLiteConnection", StringComparison.Ordinal))
                ConfigureSqliteNativeResolver(type.Assembly);
            using var connection = (System.Data.IDbConnection)Activator.CreateInstance(type)!;
            return true;
        }
        catch (Exception error)
        {
            logger.LogWarning(error, "Database provider initialization failed for {ConnectionType}", driver.DbConnectionType);
            return false;
        }
    }

    private static void ConfigureSqliteNativeResolver(System.Reflection.Assembly assembly)
    {
        lock (NativeGate)
        {
            if (NativeResolvers.Contains(assembly)) return;
            var frameworkDirectory = Path.GetDirectoryName(assembly.Location);
            var libDirectory = frameworkDirectory is null ? null : Directory.GetParent(frameworkDirectory);
            var packageDirectory = libDirectory?.Name == "lib" ? libDirectory.Parent : null;
            var native = packageDirectory is null ? null : Path.Combine(packageDirectory.FullName, "runtimes", RuntimeInformation.RuntimeIdentifier, "native", "SQLite.Interop.dll");
            if (native is not null && File.Exists(native))
            {
                try
                {
                    NativeLibrary.SetDllImportResolver(assembly, (name, _, _) =>
                        string.Equals(name, "SQLite.Interop.dll", StringComparison.OrdinalIgnoreCase) ? NativeLibrary.Load(native) : IntPtr.Zero);
                }
                catch (InvalidOperationException) { /* An existing resolver is verified by constructing the provider below. */ }
                NativeResolvers.Add(assembly);
            }
        }
    }
    private (string? Package, string? Version) Connector(ConnectionDriversConfig driver)
    {
        var section = configuration.GetSection("SetupDrivers:Connectors:" + driver.classHandler);
        var package = section["Package"]; var version = section["Version"];
        return (string.IsNullOrWhiteSpace(package) ? null : package, string.IsNullOrWhiteSpace(version) ? null : version);
    }
    private (string Package, string? Version, bool Override) Provider(ConnectionDriversConfig driver)
    {
        var section = configuration.GetSection("SetupDrivers:Providers:" + driver.classHandler);
        var package = section["Package"];
        if (!string.IsNullOrWhiteSpace(package))
            return (package, string.IsNullOrWhiteSpace(section["Version"]) ? null : section["Version"], true);
        return (driver.PackageName, string.IsNullOrWhiteSpace(driver.NuggetVersion) ? null : driver.NuggetVersion, false);
    }
    private static bool CacheIdentity(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 &&
        value.All(c => char.IsAsciiLetterOrDigit(c) || c is '.' or '-' or '_') && value is not "." and not "..";
    private static bool CacheVersion(string? value) => CacheIdentity(value) && NuGetVersion.TryParse(value, out _);
    internal static string? CachePath(string root, string? package, string? version) =>
        CacheIdentity(package) && CacheVersion(version)
            ? Path.Combine(root, package!.ToLowerInvariant(), NuGetVersion.Parse(version!).ToNormalizedString().ToLowerInvariant())
            : null;

    private bool CanInstall(ConnectionDriversConfig driver, bool connectorLoaded, bool providerLoaded)
    {
        var connector = Connector(driver);
        var provider = Provider(driver);
        return (connectorLoaded || CacheIdentity(connector.Package) && CacheVersion(connector.Version)) &&
            (providerLoaded || CacheIdentity(provider.Package) && CacheVersion(provider.Version));
    }
    private IReadOnlyList<string>? PackageSources()
    {
        var configured = configuration.GetSection("SetupDrivers:PackageSources").GetChildren().ToArray();
        if (configured.Length == 0) return null;
        var sources = configured.Select(x => x.Value?.Trim()).ToArray();
        if (sources.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Every configured setup package source must be nonempty.");
        return sources.Select(x => x!).ToArray();
    }
    private SetupDriverItem Item(ConnectionDriversConfig driver)
    {
        var connector = ConnectorLoaded(driver); var provider = ProviderLoaded(driver); var connectorPackage = Connector(driver);
        var source = Provider(driver);
        var bundled = false;
        bool? sqlServerOwnedTransactionContract = null;
#if BEEP_SECURE_CONNECTIONS
        var connectorType = connector ? editor.ConfigEditor?.DataSourcesClasses?.FirstOrDefault(x =>
            string.Equals(x.className, driver.classHandler, StringComparison.OrdinalIgnoreCase))?.type : null;
        bundled = connector && connectorType is not null && typeof(RDBSource).IsAssignableFrom(connectorType) &&
            (driver.DatasourceType == DataSourceType.SqlLite || driver.DatasourceType == DataSourceType.SqlServer && typeof(IIsolationOwnedTransactionDataSource).IsAssignableFrom(connectorType)) &&
            !string.IsNullOrWhiteSpace(connectorType.Assembly.Location) &&
            string.Equals(Path.GetDirectoryName(Path.GetFullPath(connectorType.Assembly.Location)), Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory)), StringComparison.OrdinalIgnoreCase);
        if (driver.DatasourceType == DataSourceType.SqlServer && connectorType is not null)
            sqlServerOwnedTransactionContract = typeof(IIsolationOwnedTransactionDataSource).IsAssignableFrom(connectorType);
#endif
        return new(driver.GuidID, source.Package, driver.DatasourceType.ToString(), driver.DatasourceCategory.ToString(),
            source.Version, connector && provider,
            !source.Override && (editor.assemblyHandler?.HasLocalPackage(driver) ?? false))
            { ConnectorLoaded = connector, ProviderLoaded = provider, ConnectorPackage = connectorPackage.Package, ConnectorVersion = connectorPackage.Version, BundledConnector = bundled, CanInstall = CanInstall(driver, connector, provider), SqlServerOwnedTransactionContract = sqlServerOwnedTransactionContract };
    }
    public IReadOnlyList<SetupDriverItem> Catalog() => Drivers.Where(x => !string.IsNullOrWhiteSpace(x.GuidID) &&
        !string.IsNullOrWhiteSpace(x.PackageName) && !string.IsNullOrWhiteSpace(x.classHandler))
        .Select(Item).OrderBy(x => x.DataSourceType, StringComparer.Ordinal).ThenBy(x => x.Package, StringComparer.Ordinal).ToArray();

    /// <summary>Restore configured packages already in Beep's NuGet cache without contacting a feed.</summary>
    public Task RestoreCachedAsync(CancellationToken token)
    {
        var root = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
        if (string.IsNullOrWhiteSpace(root)) root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
        return RestoreCachedAsync(root, token);
    }

    internal async Task RestoreCachedAsync(string cacheRoot, CancellationToken token)
    {
        await Gate.WaitAsync(token);
        try
        {
            var loader = editor.assemblyHandler ?? throw new InvalidOperationException("The Beep assembly handler is unavailable.");
            _ = typeof(RDBSource).Assembly;
            foreach (var driver in Drivers.Where(d => !string.IsNullOrWhiteSpace(d.classHandler)).ToArray())
            {
                token.ThrowIfCancellationRequested();
                try
                {
                    if (!ConnectorLoaded(driver))
                    {
#if BEEP_SECURE_CONNECTIONS
                        if (!RegisterBundledConnector(loader, driver))
#endif
                        {
                            var connector = Connector(driver);
                            LoadCached(loader, cacheRoot, connector.Package, connector.Version);
                        }
                    }
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    logger.LogWarning(error, "Cached Beep connector restoration failed for driver {Driver}; the setup catalog remains available", driver.GuidID);
                }
                token.ThrowIfCancellationRequested();
                try
                {
                    if (!ProviderLoaded(driver))
                    {
                        var provider = Provider(driver);
                        LoadCached(loader, cacheRoot, provider.Package, provider.Version);
                    }
                }
                catch (Exception error) when (error is not OperationCanceledException)
                {
                    logger.LogWarning(error, "Cached database provider restoration failed for driver {Driver}; the setup catalog remains available", driver.GuidID);
                }
                token.ThrowIfCancellationRequested();
            }
        }
        finally { Gate.Release(); }
    }

    private void LoadCached(IAssemblyHandler loader, string root, string? package, string? version)
    {
        // Only exact configured package identities may be loaded from the shared NuGet cache.
        var path = CachePath(root, package, version);
        if (path is not null && Directory.Exists(path))
        {
            PreparePackageLoader(loader);
            if (!loader.LoadNugget(path))
                logger.LogWarning("Cached Beep package {Package} {Version} could not be loaded", package, version);
        }
    }

    private static void PreparePackageLoader(IAssemblyHandler loader)
    {
        // The standard Beep handler creates its package manager lazily, while its
        // package loading operations currently access the backing manager directly.
        if (loader is AssemblyHandler standardLoader) _ = standardLoader.NuggetManager;
    }

    public async Task<SetupDriverItem> InstallAsync(string id, SetupDriverItem reviewed, Func<Task> authority, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 128 || id.Any(char.IsControl)) throw RefusalException.Invalid("Select a registered driver.");
        ArgumentNullException.ThrowIfNull(authority); await Gate.WaitAsync(token);
        try
        {
            await authority();
            token.ThrowIfCancellationRequested();
            var matches = Drivers.Where(x => x.GuidID == id).ToArray();
            if (matches.Length != 1 || string.IsNullOrWhiteSpace(matches[0].PackageName) || string.IsNullOrWhiteSpace(matches[0].classHandler))
                throw RefusalException.Conflict("The selected driver is missing or ambiguous. Reload the driver catalog.");
            var driver = matches[0]; var loader = editor.assemblyHandler ?? throw new InvalidOperationException("The Beep assembly handler is unavailable.");
            var currentItem = Item(driver);
            if (reviewed is null || reviewed.Id != id || reviewed.Package != currentItem.Package ||
                reviewed.PackageVersion != currentItem.PackageVersion || reviewed.ConnectorPackage != currentItem.ConnectorPackage ||
                reviewed.ConnectorVersion != currentItem.ConnectorVersion || reviewed.DataSourceType != currentItem.DataSourceType ||
                reviewed.Category != currentItem.Category)
                throw RefusalException.Conflict("The driver catalog entry changed. Reload and review it before installation.");
            if (!CanInstall(driver, ConnectorLoaded(driver), ProviderLoaded(driver)))
                throw RefusalException.Conflict("Configure exact connector and provider package names and versions before installation so they can be restored after restart.");
            var sources = PackageSources();
            var connectorSelection = Connector(driver);
            var providerSelection = Provider(driver);
            var driverIdentity = (driver.GuidID, driver.PackageName, driver.classHandler, driver.DatasourceType,
                driver.DbConnectionType, driver.dllname, driver.ADOType, driver.NuggetVersion);
            void RequireCurrentSelection()
            {
                var current = Drivers.Where(x => x.GuidID == id).ToArray();
                var currentSources = PackageSources();
                if (current.Length != 1 || !ReferenceEquals(current[0], driver) ||
                    driverIdentity != (driver.GuidID, driver.PackageName, driver.classHandler, driver.DatasourceType,
                        driver.DbConnectionType, driver.dllname, driver.ADOType, driver.NuggetVersion) ||
                    connectorSelection != Connector(driver) || providerSelection != Provider(driver) ||
                    (sources is null ? currentSources is not null : currentSources is null || !sources.SequenceEqual(currentSources)))
                    throw RefusalException.Conflict("The driver or package configuration changed during installation. Reload the catalog before trying again.");
            }
            if (!ConnectorLoaded(driver))
            {
                var connector = connectorSelection;
                if (connector.Package is null) throw RefusalException.Conflict("No connector package is configured for this driver. Ask the server operator to configure it.");
                await authority();
                token.ThrowIfCancellationRequested(); RequireCurrentSelection();
                // The connector package can declare an older RDBDataSource dependency than the one pinned by the app.
                // Resolve the app's compatible runtime before NuGet loads the connector's dependency graph.
                _ = typeof(RDBSource).Assembly;
                PreparePackageLoader(loader);
                var assemblies = await loader.LoadNuggetFromNuGetAsync(connector.Package, connector.Version, sources, true, null, false);
                token.ThrowIfCancellationRequested(); RequireCurrentSelection();
                if (assemblies is null || assemblies.Count == 0)
                {
                    logger.LogWarning("Beep connector package {Package} version {Version} could not be downloaded or loaded from the configured feeds", connector.Package, connector.Version);
                    throw new InvalidOperationException("The configured connector package could not be downloaded or loaded. Check its version and the server package feeds.");
                }
                if (!ConnectorLoaded(driver)) throw new InvalidOperationException("The datasource implementation was not registered after loading.");
            }
            if (!ProviderLoaded(driver))
            {
                await authority();
                token.ThrowIfCancellationRequested(); RequireCurrentSelection();
                var provider = providerSelection;
                if (!provider.Override && loader.HasLocalPackage(driver)) loader.LoadDriverFromLocalPackage(driver, out _);
                token.ThrowIfCancellationRequested();
                if (!ProviderLoaded(driver))
                {
                    PreparePackageLoader(loader);
                    var assemblies = await loader.LoadNuggetFromNuGetAsync(provider.Package, provider.Version, sources, true, null, false);
                    token.ThrowIfCancellationRequested(); RequireCurrentSelection();
                    if (assemblies is null || assemblies.Count == 0)
                    {
                        logger.LogWarning("Database provider package {Package} version {Version} could not be downloaded or loaded from the configured feeds", provider.Package, provider.Version);
                        throw new InvalidOperationException("The configured provider package could not be downloaded or loaded. Check its version and the server package feeds.");
                    }
                }
                if (!ProviderLoaded(driver)) throw new InvalidOperationException("The database provider was not available after loading.");
            }
            await authority(); token.ThrowIfCancellationRequested(); RequireCurrentSelection();
            driver.IsMissing = false; driver.NuggetMissing = false;
            editor.ConfigEditor.SaveConnectionDriversConfigValues(); editor.ConfigEditor.SaveConfigValues();
            logger.LogInformation("Beep setup connector and provider loaded: Package={Package} Type={Type}", driver.PackageName, driver.DatasourceType);
            await authority(); token.ThrowIfCancellationRequested(); RequireCurrentSelection();
            return Item(driver);
        }
        finally { Gate.Release(); }
    }
}
