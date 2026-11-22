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

        services.AddScoped(sp => new ModuleSetupContext
        {
            Editor = sp.GetRequiredService<IDMEEditor>(),
            CommonColumnHandler = sp.GetRequiredService<ICommonColumnHandler>(),
            Defaults = sp.GetRequiredService<IPPDM39DefaultsRepository>(),
            Metadata = sp.GetRequiredService<IPPDMMetadataRepository>(),
            ConnectionName = connectionName,
            Logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger<ModuleSetupContext>(),
        });

        // Every IModuleSetup in a loaded Beep.OilandGas* assembly, in declared order.
        services.AddDiscoveredModuleSetups();

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
            sp.GetRequiredService<ModuleSetupOrchestrator>()));

        _provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = false,
        });
    }

    public string ConnectionName { get; }

    public IServiceScope CreateScope() => _provider.CreateScope();

    public void Dispose() => _provider.Dispose();
}
