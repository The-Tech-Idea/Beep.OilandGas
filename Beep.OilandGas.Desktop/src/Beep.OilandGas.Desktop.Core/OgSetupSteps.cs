namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The first-run wizard's steps and the rule for moving between them.
/// </summary>
/// <remarks>
/// Nielsen Norman Group's wizard guidance is explicit: show the steps, keep a clear sequential order, and do not let
/// a person pick a step before completing the ones before it. A <c>TabControl</c> allows exactly that jump, so the
/// order is enforced here — a person may go back to any step already reached, or forward to the next one, and
/// nothing further until the current step is accepted.
/// </remarks>
public static class OgSetupSteps
{
    /// <summary>The step names, in order; the index is the step number used by the window.</summary>
    public static IReadOnlyList<string> Names { get; } =
    [
        "Welcome",
        "Database",
        "Company",
        "Modules",
        "Administrator",
        "Review",
    ];

    public static int Count => Names.Count;

    /// <summary>The name of a step, for a button label; an unknown index returns an empty string.</summary>
    public static string NameOf(int index) =>
        index >= 0 && index < Names.Count ? Names[index] : string.Empty;

    /// <summary>
    /// True when the person may select <paramref name="target"/> after having reached <paramref name="furthest"/>.
    /// Backwards always, the next step, and nothing beyond it.
    /// </summary>
    public static bool MaySelect(int furthest, int target) =>
        target >= 0
        && target < Count
        && target <= furthest + 1;

    /// <summary>True when the step has been reached, so it is shown as completed in a step list.</summary>
    public static bool IsReached(int furthest, int index) => index >= 0 && index <= furthest;

    /// <summary>Where a step stands, for the rail beside the wizard: done, here, or still ahead.</summary>
    public static OgSetupStepState StateOf(int furthest, int index)
    {
        if (index < 0 || index >= Count)
        {
            return OgSetupStepState.Upcoming;
        }

        if (index == furthest)
        {
            return OgSetupStepState.Current;
        }

        return index < furthest ? OgSetupStepState.Completed : OgSetupStepState.Upcoming;
    }

    /// <summary>
    /// The rail's label for a step: a tick for a finished step, an arrow for the one in front of the person, and the
    /// plain name for one still ahead. Text rather than a glyph font, so it reads in every skin and to a screen reader.
    /// </summary>
    public static string LabelFor(int furthest, int index) => StateOf(furthest, index) switch
    {
        OgSetupStepState.Completed => $"{index + 1}. {NameOf(index)} - done",
        OgSetupStepState.Current => $"{index + 1}. {NameOf(index)} - here",
        OgSetupStepState.Upcoming => $"{index + 1}. {NameOf(index)}",
    };

    /// <summary>
    /// Where a wizard should open after an interruption: the step after the last one accepted, never past the review.
    /// A record at or beyond the end resumes at the review, where the work is performed.
    /// </summary>
    public static int ResumeIndex(int lastCompletedStep) =>
        lastCompletedStep < 0
            ? 0
            : Math.Min(lastCompletedStep + 1, Count - 1);
}

/// <summary>Where one step stands in the wizard's rail.</summary>
public enum OgSetupStepState
{
    /// <summary>Reached and accepted.</summary>
    Completed,

    /// <summary>The step in front of the person.</summary>
    Current,

    /// <summary>Not reached yet.</summary>
    Upcoming,
}
