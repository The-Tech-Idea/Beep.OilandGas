using System.Reflection;

namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The password rules, in one place, so the first-run wizard, the account administration and a company database
/// account cannot disagree about what is acceptable.
/// </summary>
/// <remarks>
/// <para>
/// The rule is <b>length, not composition</b>: OWASP's Authentication Cheat Sheet and NIST SP 800-63B-4 treat a
/// password shorter than 15 characters as weak when no second factor is used, recommend a maximum of at least 64
/// characters so passphrases fit, and advise against composition rules (upper/lower/digit/symbol) and against
/// periodic forced rotation. There are deliberately no character-class rules here: they push people towards short,
/// predictable shapes like <c>P@ssw0rd1!</c> and buy nothing a length floor does not.
/// </para>
/// <para>
/// The screen is against a real list, not a hand-written one: the `10k-most-common` list from SecLists
/// (<c>Passwords/Common-Credentials/10k-most-common.txt</c>, 10,001 entries as retrieved 2026-11-09), embedded in
/// this assembly. A
/// password is matched case-insensitively and exact — a listed password with extra characters is a different secret,
/// and rejecting every passphrase that contains a common word would punish exactly the people following the advice.
/// </para>
/// <para>
/// Because this product has no second factor yet, the 15-character floor applies. When MFA arrives the floor can drop
/// to 8 for accounts that have it; that is a decision for a later stage, not a silent change here.
/// </para>
/// </remarks>
public static class OgPasswordPolicy
{
    /// <summary>No second factor is available, so NIST's 15-character floor applies.</summary>
    public const int MinimumLength = 15;

    /// <summary>Passphrases must fit; nothing is ever silently truncated.</summary>
    public const int MaximumLength = 64;

    /// <summary>The embedded resource holding the list, and the name it is reported under.</summary>
    private const string ResourceName = "TheTechIdea.Beep.OilandGas.Desktop.Core.Security.breached-passwords.txt";

    private static readonly Lazy<IReadOnlySet<string>> Breached = new(LoadBreached, isThreadSafe: true);

    /// <summary>How many entries the embedded list held — used by the test that proves it was loaded.</summary>
    public static int BreachedPasswordCount => Breached.Value.Count;

    /// <summary>
    /// What is wrong with <paramref name="password"/>, in sentences a person can act on. An empty list means it
    /// passes. Nothing here says whether the password is *right*; it says whether it is allowed.
    /// </summary>
    public static IReadOnlyList<string> Problems(string? password)
    {
        var problems = new List<string>();

        if (string.IsNullOrEmpty(password) || password.Length < MinimumLength)
        {
            problems.Add(
                $"It must be at least {MinimumLength} characters long. A sentence of several words is a good password.");
        }
        else if (password.Length > MaximumLength)
        {
            problems.Add($"It must be no more than {MaximumLength} characters long.");
        }

        if (string.IsNullOrEmpty(password))
        {
            return problems;
        }

        if (password.All(char.IsDigit))
        {
            problems.Add("It must not be digits only.");
        }

        if (IsBreached(password))
        {
            problems.Add(
                "It appears on the published list of the most commonly breached passwords; choose another.");
        }

        return problems;
    }

    /// <summary>True when the password is one of the listed common/breached passwords (case-insensitive, exact).</summary>
    public static bool IsBreached(string? password) =>
        !string.IsNullOrWhiteSpace(password)
        && Breached.Value.Contains(password.Trim());

    /// <summary>
    /// Reads the embedded list once. A missing resource yields an empty set rather than an exception on the sign-in
    /// path; the test that asserts <see cref="BreachedPasswordCount"/> fails the build if the resource disappears.
    /// </summary>
    private static IReadOnlySet<string> LoadBreached()
    {
        var assembly = typeof(OgPasswordPolicy).Assembly;

        using var stream = assembly.GetManifestResourceStream(ResourceName);

        if (stream is null)
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        using var reader = new StreamReader(stream);

        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (reader.ReadLine() is { } line)
        {
            var value = line.Trim();

            // The file carries a source header; comments and blank lines are not passwords.
            if (value.Length == 0 || value.StartsWith('#'))
            {
                continue;
            }

            set.Add(value);
        }

        return set;
    }
}
