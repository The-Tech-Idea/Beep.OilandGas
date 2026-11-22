using System.Collections.Concurrent;

namespace TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

/// <summary>
/// REL-04 AC-1: the marks a startup makes, so "start to sign-in prompt" can be reported as one number instead
/// of asserted.
/// </summary>
/// <remarks>
/// Time is measured from <see cref="Begin"/>, which runs as the first statement of
/// <see cref="OgBootstrapper.Create"/>. That deliberately excludes the runtime's own start-up before the first
/// desktop code runs, and the report says so: claiming to measure the process would be a claim this code cannot
/// support, and asking the operating system for the process start time can fail in a sandbox in a way that is
/// not worth breaking startup over.
/// </remarks>
public static class OgStartupTimeline
{
    private static readonly ConcurrentDictionary<string, double> Marks = new(StringComparer.OrdinalIgnoreCase);

    private static long _origin = -1;

    /// <summary>Starts the clock. The first call wins, so a second application in the same process does not reset it.</summary>
    public static void Begin() => Interlocked.CompareExchange(ref _origin, Environment.TickCount64, -1);

    /// <summary>Records how long after <see cref="Begin"/> this point was reached. The first mark wins.</summary>
    public static void Mark(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A mark needs a name.", nameof(name));
        }

        Begin();
        var elapsed = Environment.TickCount64 - Interlocked.Read(ref _origin);

        Marks.TryAdd(name, Math.Max(0, elapsed));
    }

    /// <summary>The mark's milliseconds after <see cref="Begin"/>, or null when it was never reached.</summary>
    public static double? At(string name) =>
        Marks.TryGetValue(name, out var value) ? value : null;

    /// <summary>Every mark, in the order it was first recorded.</summary>
    public static IReadOnlyDictionary<string, double> All => Marks;

    public static string Describe() =>
        string.Join(
            ", ",
            Marks.OrderBy(pair => pair.Value).Select(pair => $"{pair.Key}={pair.Value:0} ms"));
}
