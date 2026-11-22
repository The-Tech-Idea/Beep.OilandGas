using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Container;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;

/// <summary>What the host must tell BeepDM before it can manage connections.</summary>
public sealed record OgBeepHostOptions(
    string ConfigDirectory,
    string AppRepoName = "BeepOilGas",
    string? DriverDirectory = null);

/// <summary>
/// Owns the BeepDM runtime: one <c>IBeepService</c>, one <c>IDMEEditor</c>, one configuration folder.
/// </summary>
/// <remarks>
/// The configuration folder is supplied by the caller (the desktop host passes
/// <c>%LOCALAPPDATA%\TheTechIdea\BeepOilGas\Beep</c>) because this assembly must not know where the
/// application keeps its writable files — that is <c>OgPaths</c>' single job (ADR-013).
/// </remarks>
public sealed class OgBeepHost : IDisposable
{
    private readonly ServiceProvider _provider;
    private bool _disposed;

    private OgBeepHost(ServiceProvider provider, IBeepService beep)
    {
        _provider = provider;
        Beep = beep;
    }

    public IBeepService Beep { get; }

    public IDMEEditor Editor =>
        Beep.DMEEditor ?? throw new InvalidOperationException("The BeepDM editor is not available.");

    /// <summary>
    /// The BeepDM service provider, for the shared data controls that resolve the editor themselves.
    /// </summary>
    /// <remarks>
    /// The connection catalogue, the connection editor and the two downloaders are generic BeepDM hosts: they take
    /// an <see cref="IServiceProvider"/> and ask it for the editor. Handing them this provider keeps the editor type
    /// named only in this assembly, which is the rule <c>No_type_outside_the_data_project_references_IDMEEditor</c>
    /// enforces. The provider is the same one the runtime uses, so a connection saved through a control and a
    /// connection opened by this host are the same editor's connection.
    /// </remarks>
    public IServiceProvider BeepServices => _provider;

    public static OgBeepHost Create(OgBeepHostOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentException.ThrowIfNullOrWhiteSpace(options.ConfigDirectory);

        Directory.CreateDirectory(options.ConfigDirectory);

        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Warning));

        services.AddBeepRuntime(beep =>
        {
            beep.DirectoryPath = options.ConfigDirectory;
            beep.AppRepoName = options.AppRepoName;
            beep.ConfigType = BeepConfigType.Application;
            beep.EnableAutoMapping = true;
            beep.EnableAssemblyLoading = false;
            beep.ServiceLifetime = ServiceLifetime.Singleton;
        });

        var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = false,
        });

        var beepService = provider.GetRequiredService<IBeepService>();
        beepService.LoadAssemblies();

        // Connections are persisted as configuration values; they are only in memory after an explicit load.
        beepService.Config_editor?.LoadDataConnectionsValues();

        return new OgBeepHost(provider, beepService);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _provider.Dispose();
        }
    }
}
