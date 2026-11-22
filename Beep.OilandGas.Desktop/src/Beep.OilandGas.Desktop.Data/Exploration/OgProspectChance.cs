using System.Globalization;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

/// <summary>
/// The one rule for a prospect's chance of success: the product of the recorded per-element success probabilities,
/// produced only when every required element carries a usable probability.
/// </summary>
/// <remarks>
/// The probabilities are risks (a probability of failure), so an element's success probability is its complement.
/// Nothing is estimated: a missing, unparsable or out-of-range element makes the chance null, and the caller shows
/// "incomplete" rather than a number (EXP-02 AC-3, EXP-03 AC-1/AC-2). EXP-02 and EXP-03 both use this class, so the
/// board and the editor cannot disagree about a prospect's chance.
/// </remarks>
public static class OgProspectChance
{
    /// <summary>The elements a defensible chance needs, in the order the editor shows them.</summary>
    public static IReadOnlyList<string> RequiredElements { get; } =
        ["TRAP", "RESERVOIR", "SEAL", "SOURCE", "TIMING"];

    /// <summary>True when every required element carries a probability in 0..1.</summary>
    public static bool IsComplete(IReadOnlyDictionary<string, decimal?> risks)
    {
        ArgumentNullException.ThrowIfNull(risks);

        return RequiredElements.All(element =>
            risks.TryGetValue(element, out var risk) && IsUsable(risk));
    }

    /// <summary>
    /// The chance of success as a whole percentage, or null when any required element is missing or out of range.
    /// </summary>
    public static decimal? Of(IReadOnlyDictionary<string, decimal?> risks)
    {
        ArgumentNullException.ThrowIfNull(risks);

        if (!IsComplete(risks))
        {
            return null;
        }

        var success = 1m;

        foreach (var element in RequiredElements)
        {
            success *= 1m - risks[element]!.Value;
        }

        return Math.Round(Math.Clamp(success * 100m, 0m, 100m), 0);
    }

    /// <summary>True when a probability is recorded and is a usable 0..1 value.</summary>
    public static bool IsUsable(decimal? risk) => risk is >= 0m and <= 1m;

    /// <summary>The sentence a card or tab shows instead of a chance.</summary>
    public static string Text(decimal? chance, bool complete) =>
        !complete
            ? "incomplete"
            : chance is { } value
                ? $"{value.ToString("0", CultureInfo.InvariantCulture)}%"
                : "no chance recorded";
}
