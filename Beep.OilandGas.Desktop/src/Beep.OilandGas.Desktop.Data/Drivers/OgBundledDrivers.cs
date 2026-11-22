using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.DriversConfigurations;
using TheTechIdea.Beep.Services;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Drivers;

/// <summary>
/// Registers the drivers this build ships, from the data-source assemblies that are already loaded.
/// </summary>
/// <remarks>
/// The packaged <c>ConnectionDrivers</c> folder is empty on a developer build and on most installed
/// builds, because the local database driver is a project reference rather than a copied assembly. Relying
/// on the folder alone meant <c>OgDriverDiscovery.FirstLocalDriver</c> could find nothing in the running
/// application even though the SQLite data source was loaded — the DAT-01 integration test passed only
/// because it registered the assembly itself. This class closes that gap in one place.
/// </remarks>
public static class OgBundledDrivers
{
    /// <summary>
    /// The data-source assemblies this build references. Adding a provider means adding it here and to the
    /// project file; nothing else changes.
    /// </summary>
    public static IReadOnlyList<Type> AssemblyAnchors { get; } = [typeof(SQLiteDataSource)];

    /// <summary>Registers every driver found in the bundled data-source assemblies. Idempotent.</summary>
    public static IReadOnlyList<ConnectionDriversConfig> Register(IBeepService beep)
    {
        ArgumentNullException.ThrowIfNull(beep);

        var added = new List<ConnectionDriversConfig>();

        foreach (var anchor in AssemblyAnchors)
        {
            added.AddRange(OgDriverDiscovery.LoadFromAssembly(beep, anchor.Assembly));
        }

        return added;
    }
}
