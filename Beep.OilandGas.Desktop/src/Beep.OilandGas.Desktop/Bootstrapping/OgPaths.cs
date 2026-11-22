namespace TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

/// <summary>
/// The one owner of every writable path in the desktop.
/// </summary>
/// <remarks>
/// <para>
/// Nothing may be written beside the executable: a per-machine install lives under
/// <c>C:\Program Files</c>, where a standard user cannot write, and anything written there is left
/// behind by uninstall. Every writable location therefore resolves under
/// <c>%LOCALAPPDATA%\TheTechIdea\BeepOilGas</c>, or under whatever <c>BEEP_OILGAS_DATA_ROOT</c> names.
/// </para>
/// <para>
/// ADR-013: the failure store is <see cref="FailureStorePath"/> and never the install folder. Guard
/// test <c>DataRootGuardTests</c> fails the build when a path escapes the root.
/// </para>
/// </remarks>
public static class OgPaths
{
    public const string AppRepoName = "BeepOilGas";
    public const string ConnectionName = "BeepOilGas";
    public const string DefaultCompanyId = "COMPANY-001";
    public const string DefaultCompanyName = "Demo Company";
    public const string DefaultFieldId = "FIELD-001";
    public const string DefaultFieldName = "Demo Field";
    public const string DefaultBookCode = "GL";
    public const string SystemUserId = "system";

    public const string DataRootVariable = "BEEP_OILGAS_DATA_ROOT";
    public const string LogDirectoryVariable = "BEEP_LOG_DIR";

    public static string DataRoot { get; } = ResolveDataRoot();

    /// <summary>This build's version, shown beside each forwarded failure.</summary>
    public static string ProductVersion =>
        typeof(OgPaths).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";

    public static string BeepDirectory => Path.Combine(DataRoot, "Beep");

    public static string DataDirectory => Path.Combine(DataRoot, "Data");

    public static string DiagnosticsDirectory => Path.Combine(DataRoot, "Diagnostics");

    /// <summary>The failure store. Never beside the executable (ADR-013).</summary>
    public static string FailureStorePath => Path.Combine(DiagnosticsDirectory, "failures.db");

    public static string LogDirectory => Path.Combine(DataRoot, "Logs");

    /// <summary>ADR-008: the OilandGasTools environment lives here, one JSON file per customer.</summary>
    public static string WellboreDirectory => Path.Combine(DataRoot, "Wellbore");

    /// <summary>The active wellbore environment file. A missing file means built-in defaults.</summary>
    public static string WellboreEnvironmentPath => Path.Combine(WellboreDirectory, "environment.json");

    /// <summary>The desktop's own local database lives here, separate from the failure store.</summary>
    public static string LocalDatabaseDirectory => Path.Combine(DataDirectory, "Local");

    /// <summary>The local working database for this installation's default connection.</summary>
    public static string LocalDatabasePath =>
        Path.Combine(LocalDatabaseDirectory, ConnectionName + ".db");

    /// <summary>VSB-09: exported PDF, SVG and PNG files live here.</summary>
    public static string ExportsDirectory => Path.Combine(DataRoot, "Exports");

    /// <summary>REL-02: the installed licence file.</summary>
    public static string LicencePath => Path.Combine(DataRoot, "licence.json");

    /// <summary>REL-02: whether a licence problem stops the product or is only reported.</summary>
    public static string LicenceSettingsPath => Path.Combine(DataRoot, "licence.settings.json");

    /// <summary>FND-06: failures waiting to reach the API, one file per report.</summary>
    public static string FailureOutboxDirectory => Path.Combine(DataRoot, "Outbox");

    /// <summary>SETUP-12: the dated copies of the installation's database.</summary>
    public static string BackupDirectory => Path.Combine(DataRoot, "Backups");

    public static string SkinPath => Path.Combine(DataRoot, "skin.json");

    public static string SmokeResultPath => Path.Combine(DataRoot, "smoke-result.txt");

    /// <summary>REL-04: the last benchmark report, beside the failure store (never beside the executable).</summary>
    public static string BenchmarkReportPath => Path.Combine(DiagnosticsDirectory, "benchmark-report.json");

    /// <summary>DAT-02: which modules completed for this connection, so a failed run resumes.</summary>
    public static string ModuleSetupStatePath => Path.Combine(DataDirectory, "module-setup-state.json");

    /// <summary>REL-04 AC-5: the last soak report, beside the benchmark report.</summary>
    public static string SoakReportPath => Path.Combine(DiagnosticsDirectory, "soak-report.json");

    /// <summary>REL-04: deterministic benchmark inputs, so a run needs no customer data.</summary>
    public static string BenchmarkDirectory => Path.Combine(DataDirectory, "Benchmark");

    /// <summary>Every directory this application may create, all under <see cref="DataRoot"/>.</summary>
    public static IReadOnlyList<string> WritableDirectories =>
        [DataRoot, BeepDirectory, DataDirectory, DiagnosticsDirectory, LogDirectory, WellboreDirectory, FailureOutboxDirectory, ExportsDirectory, BackupDirectory];

    private static string ResolveDataRoot()
    {
        var overrideRoot = Environment.GetEnvironmentVariable(DataRootVariable);
        return string.IsNullOrWhiteSpace(overrideRoot)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TheTechIdea",
                "BeepOilGas")
            : Path.GetFullPath(overrideRoot);
    }

    /// <summary>
    /// True when <paramref name="path"/> resolves under <see cref="DataRoot"/> and not under the
    /// directory the executable runs from.
    /// </summary>
    public static bool IsWritableLocation(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var full = Path.GetFullPath(path);
        var root = Path.GetFullPath(DataRoot);
        var baseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

        var separator = Path.DirectorySeparatorChar;
        var rootPrefix = root.EndsWith(separator) ? root : root + separator;
        var basePrefix = baseDirectory.EndsWith(separator)
            ? baseDirectory
            : baseDirectory + separator;

        // The root itself is a valid location; anything beneath it is too.
        var underRoot = string.Equals(full, root, StringComparison.OrdinalIgnoreCase)
                        || full.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase);

        var underBase = string.Equals(full, baseDirectory, StringComparison.OrdinalIgnoreCase)
                        || full.StartsWith(basePrefix, StringComparison.OrdinalIgnoreCase);

        return underRoot && !underBase;
    }

    /// <summary>Creates the writable directories. Called once at startup, before any store is opened.</summary>
    public static void EnsureDirectories()
    {
        foreach (var directory in WritableDirectories)
        {
            Directory.CreateDirectory(directory);
        }
    }
}
