namespace TheTechIdea.Beep.OilandGas.Desktop.Headless;

/// <summary>
/// FND-01 AC-6: the root a <c>--smoke</c> run uses, prepared so a second run starts from nothing.
/// </summary>
/// <remarks>
/// A smoke run must prove the application works on a clean installation, so a root left by an earlier run —
/// a store with yesterday's records, a setup flag, a skin — would make the run pass while a real first run
/// fails. Deleting it is the whole point; a stale root is worse than no root.
/// </remarks>
public static class OgSmokeRoot
{
    public const string FolderName = "BeepOilGasSmoke";

    /// <summary>
    /// Deletes the smoke root if it exists and creates it empty. Safe to call on every start.
    /// </summary>
    /// <param name="baseDirectory">The directory to put the root under; the caller's temp by default.</param>
    public static string Prepare(string? baseDirectory = null)
    {
        var root = Path.Combine(
            baseDirectory ?? Path.GetTempPath(),
            "TheTechIdea",
            FolderName);

        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }

        Directory.CreateDirectory(root);
        return root;
    }
}
