using System.Runtime.CompilerServices;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// Chooses a disposable per-user data root before any test touches <c>OgPaths</c>, so the tests never
/// write into the real installation's store.
/// </summary>
internal static class TestEnvironment
{
    public const string DataRootVariable = "BEEP_OILGAS_DATA_ROOT";

    [ModuleInitializer]
    internal static void Initialize()
    {
        var root = Path.Combine(
            Path.GetTempPath(),
            "TheTechIdea",
            "BeepOilGasTests",
            Guid.NewGuid().ToString("n")[..8]);

        Directory.CreateDirectory(root);
        Environment.SetEnvironmentVariable(DataRootVariable, root);
    }
}

/// <summary>Finds the <c>Beep.OilandGas.Desktop</c> folder from the test binary location.</summary>
internal static class SourceRoot
{
    public static string Locate()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "plan"))
                && Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            $"Could not find the desktop source root above '{AppContext.BaseDirectory}'.");
    }

    public static IReadOnlyList<string> AllSources()
    {
        var root = Path.Combine(Locate(), "src");
        return Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                           && !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
    }
}
