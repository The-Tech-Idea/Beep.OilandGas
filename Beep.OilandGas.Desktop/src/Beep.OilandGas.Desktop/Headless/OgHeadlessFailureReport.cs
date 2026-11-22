using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

namespace TheTechIdea.Beep.OilandGas.Desktop.Headless;

/// <summary>
/// FND-01 AC-3: what a headless run leaves behind when startup throws before the verb can run.
/// </summary>
/// <remarks>
/// A scheduled run has no window and nobody to read a dialog, so the failure must reach the console and —
/// for <c>--smoke</c> — the result file the run exists to produce. The text carries the reference and never
/// the exception's own words (AC-4).
/// </remarks>
public static class OgHeadlessFailureReport
{
    /// <summary>The report text: which command, the outcome, and the reference to quote.</summary>
    public static string Compose(OgCommandLine command, string reference) =>
        $"command: {command.Describe()}{Environment.NewLine}" +
        $"result: failed{Environment.NewLine}" +
        $"reference: {reference}{Environment.NewLine}";

    /// <summary>
    /// Writes the report to <paramref name="console"/> and, for <c>--smoke</c>, to the result file — so a
    /// failed smoke run is still readable where an operator looks for it.
    /// </summary>
    public static void Write(
        OgCommandLine command,
        string reference,
        TextWriter console,
        string? resultPath = null)
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(console);

        var text = Compose(command, reference);
        console.Write(text);

        if (command.Verb != OgHeadlessVerb.Smoke)
        {
            return;
        }

        var path = resultPath ?? OgPaths.SmokeResultPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }
}
