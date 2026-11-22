using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Services;
using TheTechIdea.Beep.Utilities;
using System.Reflection;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;

/// <summary>
/// Discovers the database drivers shipped beside the executable and makes them visible to BeepDM.
/// </summary>
/// <remarks>
/// No database product is named here: the folder is scanned, each assembly's drivers are read, and a
/// driver is added only if BeepDM does not already know its package. This is the same shape the ERP
/// reference uses in <c>ErpBootstrapper.RegisterPackagedDrivers</c>.
/// </remarks>
public static class OgDriverDiscovery
{
    public static IReadOnlyList<ConnectionDriversConfig> LoadFrom(IBeepService beep, string driverDirectory)
    {
        ArgumentNullException.ThrowIfNull(beep);

        if (string.IsNullOrWhiteSpace(driverDirectory) || !Directory.Exists(driverDirectory))
        {
            return [];
        }

        var loader = beep.LLoader;
        var editor = beep.Config_editor;

        if (loader is null || editor is null)
        {
            return [];
        }

        var assemblies = loader.LoadAssembliesFromFolder(
            driverDirectory,
            FolderFileTypes.ConnectionDriver,
            scanForDataSources: true);

        var added = new List<ConnectionDriversConfig>();

        foreach (var assembly in assemblies)
        {
            foreach (var driver in loader.GetDrivers(assembly) ?? [])
            {
                if (string.IsNullOrWhiteSpace(driver.PackageName))
                {
                    continue;
                }

                editor.DataDriversClasses ??= [];
                var known = editor.DataDriversClasses.Any(existing =>
                    string.Equals(existing.PackageName, driver.PackageName, StringComparison.OrdinalIgnoreCase));

                if (known)
                {
                    continue;
                }

                editor.DataDriversClasses.Add(driver);
                added.Add(driver);
            }
        }

        return added;
    }

    /// <summary>
    /// FND-01-style selection for a local database: the first driver that advertises local creation **and**
    /// is not in-memory. Order matters — BeepDM's list contains local in-memory drivers (a cache, a memory
    /// provider) before SQLite, and picking one of those creates no file at all.
    /// </summary>
    public static ConnectionDriversConfig? SelectLocalDriver(IEnumerable<ConnectionDriversConfig>? drivers)
    {
        var candidates = drivers?.ToList() ?? [];

        return candidates.FirstOrDefault(driver => driver.CreateLocal && !driver.InMemory);
    }

    /// <summary>Finds the first installed driver that can create a local file database.</summary>
    public static ConnectionDriversConfig? FirstLocalDriver(IBeepService beep) =>
        SelectLocalDriver(beep.Config_editor?.DataDriversClasses);

    /// <summary>
    /// Registers the drivers found in an assembly that is already loaded. Used when the driver ships as a
    /// project reference or plugin rather than in the packaged folder, and by the integration test.
    /// </summary>
    public static IReadOnlyList<ConnectionDriversConfig> LoadFromAssembly(IBeepService beep, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(beep);
        ArgumentNullException.ThrowIfNull(assembly);

        var loader = beep.LLoader;
        var editor = beep.Config_editor;

        if (loader is null || editor is null)
        {
            return [];
        }

        var added = new List<ConnectionDriversConfig>();

        foreach (var driver in loader.GetDrivers(assembly) ?? [])
        {
            if (string.IsNullOrWhiteSpace(driver.PackageName))
            {
                continue;
            }

            editor.DataDriversClasses ??= [];
            if (editor.DataDriversClasses.Any(existing =>
                    string.Equals(existing.PackageName, driver.PackageName, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            editor.DataDriversClasses.Add(driver);
            added.Add(driver);
        }

        return added;
    }
}
