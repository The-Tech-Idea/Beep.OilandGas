namespace TheTechIdea.Beep.OilandGas.Desktop.Headless;

/// <summary>The exit codes every headless verb returns.</summary>
/// <remarks>
/// <para><b>0</b> — the verb did what it says.</para>
/// <para><b>1</b> — the verb could not complete; the failure was recorded and its reference is printed.</para>
/// <para><b>2</b> — the verb was refused for a stated reason (no licence, no backup service, a verb that
/// is not available yet). A refusal is not an unexpected failure, so it is not reported as one.</para>
/// </remarks>
public static class OgExitCode
{
    public const int Ok = 0;
    public const int Failed = 1;
    public const int Refused = 2;
}

/// <summary>A headless verb. <see cref="None"/> means "open the application normally".</summary>
public enum OgHeadlessVerb
{
    None,
    Smoke,
    DriversReport,
    EntitlementReport,
    LicenceCheck,
    Restore,
    SchemaApply,
    SchemaResume,
    SeedBenchmark,
    ReportBenchmark,
    Soak,
    Unknown,
}

/// <summary>
/// The parsed command line: one verb and, where the verb takes one, a single argument.
/// </summary>
/// <remarks>
/// Kept free of WPF and of the service graph so the mapping from words to verbs — including the unknown
/// and missing-argument cases — can be tested without starting anything.
/// </remarks>
public sealed record OgCommandLine(OgHeadlessVerb Verb, string? Argument = null)
{
    public const string Smoke = "--smoke";
    public const string DriversReport = "--drivers-report";
    public const string EntitlementReport = "--entitlement-report";
    public const string LicenceCheck = "--licence-check";
    public const string Restore = "--restore";
    public const string SchemaApply = "--schema-apply";
    public const string SchemaResume = "--schema-resume";
    public const string SeedBenchmark = "--seed-benchmark";
    public const string ReportBenchmark = "--report-benchmark";
    public const string Benchmark = "--benchmark";
    public const string Soak = "--soak";

    /// <summary>True when the verb needs a file argument and none was supplied.</summary>
    public bool IsMissingArgument =>
        Verb is OgHeadlessVerb.LicenceCheck or OgHeadlessVerb.Restore
            && string.IsNullOrWhiteSpace(Argument);

    /// <summary>True when the application should not open a window.</summary>
    public bool IsHeadless => Verb != OgHeadlessVerb.None;

    public static OgCommandLine Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Count == 0)
        {
            return new OgCommandLine(OgHeadlessVerb.None);
        }

        // Only the first argument selects a verb; everything after it is that verb's argument. A verb
        // anywhere else in the line is ignored, so a path containing "--restore" cannot change behaviour.
        var head = args[0];
        var argument = args.Count > 1 ? args[1] : null;

        var verb = head.ToLowerInvariant() switch
        {
            Smoke => OgHeadlessVerb.Smoke,
            DriversReport => OgHeadlessVerb.DriversReport,
            EntitlementReport => OgHeadlessVerb.EntitlementReport,
            LicenceCheck => OgHeadlessVerb.LicenceCheck,
            Restore => OgHeadlessVerb.Restore,
            SchemaApply => OgHeadlessVerb.SchemaApply,
            SchemaResume => OgHeadlessVerb.SchemaResume,
            SeedBenchmark => OgHeadlessVerb.SeedBenchmark,
            ReportBenchmark or Benchmark => OgHeadlessVerb.ReportBenchmark,
        Soak => OgHeadlessVerb.Soak,
            _ => head.StartsWith("--", StringComparison.Ordinal)
                ? OgHeadlessVerb.Unknown
                : OgHeadlessVerb.None,
        };

        return new OgCommandLine(verb, argument);
    }

    /// <summary>The one line an operator sees when the verb itself is wrong.</summary>
    public static string Usage =>
        "Usage: TheTechIdea.Beep.OilandGas.Desktop [verb]\n" +
        "  --smoke                       run the headless smoke check and write smoke-result.txt\n" +
        "  --drivers-report              list the database drivers this installation can use\n" +
        "  --entitlement-report          list the modules and pages with their two gates\n" +
        "  --licence-check <file>        verify a signed licence file\n" +
        "  --restore <file>              restore a database backup (not configured until REL-01)\n" +
        "  --schema-apply                create the database schema (needs DAT-02)\n" +
        "  --schema-resume               resume an interrupted schema run (needs DAT-02)\n" +
        "  --seed-benchmark              write the large synthetic log the benchmark uses\n" +
        "  --report-benchmark            measure the performance budgets and write the report\n" +
        "  --benchmark                   alias for --report-benchmark\n" +
        "  --soak <minutes>              run scripted use and record the working set (default 30)";

    /// <summary>The invocation quoted in a failure record, with the argument reduced to a file name.</summary>
    public string Describe() => Argument is null
        ? Verb.ToString()
        : $"{Verb} ({Path.GetFileName(Argument)})";
}
