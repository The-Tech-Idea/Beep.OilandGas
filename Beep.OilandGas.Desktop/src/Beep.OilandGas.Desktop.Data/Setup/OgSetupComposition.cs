using TheTechIdea.Beep.OilandGas.Desktop.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.Common;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Beep.OilandGas.PPDM39.DataManagement.SeedData;
using Beep.OilandGas.PPDM39.DataManagement.Services;using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Setup;

/// <summary>
/// Composes the PPDM module-setup services for one connection, over an existing <see cref="OgBeepHost"/>.
/// </summary>
/// <remarks>
/// This mirrors the registration block in <c>Beep.OilandGas.ApiService/Program.cs</c> — the canonical list of
/// what <c>PPDM39SetupService</c> and every <c>IModuleSetup</c> need. It is deliberately small: only the
/// setup path is registered, not the whole module graph.
/// </remarks>
public sealed class OgSetupComposition : IDisposable
{
    private readonly ServiceProvider _provider;

    public OgSetupComposition(OgBeepHost beepHost, IFailureReporter failures, string connectionName)
    {
        ArgumentNullException.ThrowIfNull(beepHost);
        ArgumentNullException.ThrowIfNull(failures);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);

        ConnectionName = connectionName;

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Warning));

        services.AddSingleton(beepHost);
        services.AddSingleton(beepHost.Beep);
        services.AddSingleton(beepHost.Editor);
        services.AddSingleton(beepHost.Editor.ConfigEditor);
        services.AddSingleton(failures);
        services.AddSingleton<ILoggerFactory>(_ => LoggerFactory.Create(builder => builder.AddDebug()));

        services.AddSingleton<ICommonColumnHandler, CommonColumnHandler>();

        services.AddSingleton<IPPDMMetadataRepository>(sp => new PPDMMetadataService(
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<PPDMMetadataService>(),
            jsonMetadataPath: null,
            contributors: []));

        services.AddScoped<IPPDM39DefaultsRepository>(sp => new PPDM39DefaultsRepository(
            sp.GetRequiredService<IDMEEditor>(),
            connectionName,
            sp.GetRequiredService<IPPDMMetadataRepository>()));

        services.AddScoped(sp => new LOVManagementService(
            sp.GetRequiredService<IDMEEditor>(),
            sp.GetRequiredService<ICommonColumnHandler>(),
            sp.GetRequiredService<IPPDM39DefaultsRepository>(),
            sp.GetRequiredService<IPPDMMetadataRepository>(),
            connectionName));

        services.AddScoped(sp => new PPDMReferenceDataSeeder(
            sp.GetRequiredService<IDMEEditor>(),
            sp.GetRequiredService<ICommonColumnHandler>(),
            sp.GetRequiredService<IPPDM39DefaultsRepository>(),
            sp.GetRequiredService<IPPDMMetadataRepository>(),
            sp.GetRequiredService<LOVManagementService>(),
            sp.GetRequiredService<IFailureReporter>(),
            connectionName));

        services.AddScoped(sp => new PPDMCSVSeeder(
            sp.GetRequiredService<IDMEEditor>(),
            sp.GetRequiredService<ICommonColumnHandler>(),
            sp.GetRequiredService<IPPDM39DefaultsRepository>(),
            sp.GetRequiredService<IPPDMMetadataRepository>(),
            connectionName));

        services.AddScoped(sp => new PPDMDemoDataSeeder(
            sp.GetRequiredService<IDMEEditor>(),
            sp.GetRequiredService<ICommonColumnHandler>(),
            sp.GetRequiredService<IPPDM39DefaultsRepository>(),
            sp.GetRequiredService<IPPDMMetadataRepository>(),
            sp.GetRequiredService<PPDMReferenceDataSeeder>(),
            sp.GetRequiredService<PPDMCSVSeeder>(),
            connectionName,
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<PPDMDemoDataSeeder>()));

        services.AddScoped(sp => new ModuleSetupContext
        {
            Editor = sp.GetRequiredService<IDMEEditor>(),
            CommonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>(),
            Defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>(),
            Metadata = sp.GetRequiredService<IPPDMMetadataRepository>(),
            ConnectionName = connectionName,
            Logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<ModuleSetupContext>(),
        });

        // SECURITY's own seed service. The desktop's setup needs it because the company database's RBAC schema and
        // its default roles and permissions are prepared by the system administrator's first run; later users are
        // authorised against those tables.
        services.AddScoped<global::Beep.OilandGas.UserManagement.Contracts.Services.IDefaultSecuritySeedService>(
            sp => new global::Beep.OilandGas.UserManagement.Services.DefaultSecuritySeedService(
                sp.GetRequiredService<IDMEEditor>(),
                sp.GetRequiredService<ICommonColumnHandler>(),
                sp.GetRequiredService<IPPDM39DefaultsRepository>(),
                sp.GetRequiredService<IPPDMMetadataRepository>(),
                connectionName,
                sp.GetRequiredService<ILoggerFactory>()
                    .CreateLogger<global::Beep.OilandGas.UserManagement.Services.DefaultSecuritySeedService>()));

        // Every IModuleSetup in a loaded Beep.OilandGas* assembly, in declared order. SECURITY is included: the
        // API excluded it because it seeds through its own EF repository, but a desktop company database needs the
        // USER/ROLE/PERMISSION tables, and the seed service above is the same one the module expects.
        services.AddDiscoveredModuleSetups(_ => true);

        services.AddScoped(sp => new ModuleSetupOrchestrator(
            sp.GetServices<IModuleSetup>(),
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<ModuleSetupOrchestrator>()));

        services.AddScoped<IPPDM39SetupService>(sp => new PPDM39SetupService(
            sp.GetRequiredService<IDMEEditor>(),
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<PPDM39SetupService>(),
            sp.GetRequiredService<ICommonColumnHandler>(),
            sp.GetRequiredService<IPPDM39DefaultsRepository>(),
            sp.GetRequiredService<IPPDMMetadataRepository>(),
            sp.GetRequiredService<IFailureReporter>(),
            sp.GetRequiredService<LOVManagementService>(),
            sp.GetRequiredService<ModuleSetupOrchestrator>(),
            migrationBindingFingerprint: BindingFingerprintAsync,
            migrationTargetIdentity: _ => string.IsNullOrWhiteSpace(MigrationScope)
                ? connectionName
                : $"{connectionName}:{MigrationScope}"));

        _provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = false,
        });
    }

    public string ConnectionName { get; }

    public IServiceScope CreateScope() => _provider.CreateScope();

    /// <summary>
    /// AC-6: opens the bound connection and reports whether it opened. A connection that is missing, or that
    /// opens into any state other than <c>Open</c>, is a refusal with the reason - so setup stops before it
    /// reads a schema or writes a row, instead of failing later with a provider message.
    /// </summary>
    public OgResult ProbeTarget()
    {
        var editor = _provider.GetRequiredService<IDMEEditor>();
        var source = editor.GetDataSource(ConnectionName);

        if (source is null)
        {
            return OgResult.Failed(
                $"No datasource named '{ConnectionName}' is configured, so setup cannot start.",
                OgModuleSetupRunner.CodeUnreachable);
        }

        var state = source.Openconnection();

        return state == System.Data.ConnectionState.Open
            ? OgResult.Ok($"The connection '{ConnectionName}' opened.")
            : OgResult.Failed(
                $"The connection '{ConnectionName}' did not open (state {state}), so setup was not started.",
                OgModuleSetupRunner.CodeUnreachable);
    }

    /// <summary>
    /// Opens the target datasource. The schema planner validates the cached connection before it opens it, so
    /// the caller must have opened the connection already; the host does this when a person connects, and the
    /// runner does it here so a plan does not depend on that step having happened first.
    /// </summary>
    /// <summary>Optional trace of the open sequence, for a performance harness.</summary>
    public Action<string>? Trace { get; set; }

    /// <summary>
    /// The migration unit currently being planned. Each module is its own migration target identity, because the
    /// migration manager keeps one execution claim/checkpoint per target: a shared identity meant the first
    /// module's checkpoint made every later module refusable ("Migration target admission was denied").
    /// </summary>
    public string? MigrationScope { get; set; }

    public void OpenTarget()
    {
        Trace?.Invoke("open:enter");
        var editor = _provider.GetRequiredService<IDMEEditor>();
        Trace?.Invoke("open:editor");

        var configured = editor.ConfigEditor?.DataConnections?
            .FirstOrDefault(connection =>
                string.Equals(connection.ConnectionName, ConnectionName, StringComparison.OrdinalIgnoreCase));

        var source = editor.GetDataSource(ConnectionName);
        Trace?.Invoke($"open:getDataSource={(source is null ? "null" : source.DatasourceName)}");

        // A datasource created before this connection was written to the configuration keeps the
        // ConnectionProp it was built with, and the schema planner refuses it: "The cached datasource does
        // not match the configured migration target." Reload the datasource from the configured entry so the
        // planner validates the connection that will actually be used. Reference equality is the test: the
        // SQLite datasource assigns the configured entry itself, so a matching fingerprint is the same object.
        if (configured is not null
            && (source is null || !ReferenceEquals(source.Dataconnection?.ConnectionProp, configured)))
        {
            if (source is not null && editor.CheckDataSourceExist(ConnectionName))
            {
                editor.CloseDataSource(ConnectionName);
                editor.RemoveDataDource(ConnectionName);
            }

            Trace?.Invoke("open:reload");
            source = editor.CreateNewDataSourceConnection(configured, ConnectionName);
            Trace?.Invoke("open:reloaded");
        }

        Trace?.Invoke("open:connect");
        source?.Openconnection();
        Trace?.Invoke("open:connected");
        editor.OpenDataSource(ConnectionName);
        Trace?.Invoke("open:done");
    }

    /// <summary>
    /// The desktop's module binding: one installation, one connection, the selected modules. The API's
    /// fingerprint comes from its EF binding table; the desktop has no such table, so the fingerprint is a
    /// stable hash of exactly what it binds - which is what the schema planner needs to know the target has
    /// not changed between plan and apply.
    /// </summary>
    private static Task<string> BindingFingerprintAsync(IReadOnlyList<string> moduleIds, string connectionName)
    {
        var canonical = connectionName + "|" + string.Join(
            ",",
            moduleIds.OrderBy(id => id, StringComparer.OrdinalIgnoreCase));

        return Task.FromResult(
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))));
    }

    public void Dispose() => _provider.Dispose();
}
