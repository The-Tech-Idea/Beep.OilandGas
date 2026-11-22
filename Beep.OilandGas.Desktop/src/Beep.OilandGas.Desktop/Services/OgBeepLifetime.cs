using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;

namespace TheTechIdea.Beep.OilandGas.Desktop.Services;

/// <summary>
/// Creates the BeepDM runtime on first use and keeps it for the process lifetime.
/// </summary>
/// <remarks>
/// Startup must stay fast and must not fail because a driver folder is missing, so the runtime is lazy:
/// the first screen that needs a connection pays the cost. The configuration folder comes from
/// <see cref="OgPaths"/> here, never from the data layer — that is the single writable-path owner
/// (ADR-013).
/// </remarks>
public sealed class OgBeepLifetime : IDisposable
{
    private readonly Lazy<OgBeepHost> _host;
    private bool _disposed;

    public OgBeepLifetime()
    {
        _host = new Lazy<OgBeepHost>(Create);
    }

    /// <summary>The runtime. The first access creates it.</summary>
    public OgBeepHost Host => _host.Value;

    public bool IsCreated => _host.IsValueCreated;

    private static OgBeepHost Create()
    {
        var driverDirectory = Path.Combine(AppContext.BaseDirectory, "ConnectionDrivers");

        var host = OgBeepHost.Create(new OgBeepHostOptions(
            ConfigDirectory: OgPaths.BeepDirectory,
            AppRepoName: OgPaths.AppRepoName,
            DriverDirectory: driverDirectory));

        // Packaged folder first (an installation may ship extra drivers), then the assemblies this build
        // already references. Both are idempotent, so a driver is never registered twice.
        OgDriverDiscovery.LoadFrom(host.Beep, driverDirectory);
        OgBundledDrivers.Register(host.Beep);

        return host;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (_host.IsValueCreated)
        {
            _host.Value.Dispose();
        }
    }
}
