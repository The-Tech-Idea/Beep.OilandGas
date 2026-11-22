namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// The sign-in lockout rule: how many failures lock an account, over what window, and for how long.
/// </summary>
/// <remarks>
/// <para>
/// OWASP's Authentication Cheat Sheet asks for account lockout counted <b>against the account, not the source IP</b>
/// (an attacker with many addresses defeats an IP rule), and for a balance between security and usability. The values
/// here are that balance for a desktop client used by named people on a company network: five failures inside fifteen
/// minutes lock the account for fifteen minutes, and a correct password from the owner while locked is told the account
/// is locked until a stated time rather than being left to guess.
/// </para>
/// <para>
/// The window matters as much as the threshold: without it, a person who mistypes once a day would eventually be
/// locked out for no reason. The last attempt is the row's change time — the directory has no separate column for it,
/// and inventing one would be a schema change for a value the audit trail already records.
/// </para>
/// </remarks>
public static class OgSignInPolicy
{
    /// <summary>Failures inside the window that lock the account.</summary>
    public const int MaximumFailures = 5;

    /// <summary>Failures older than this are not counted towards the threshold.</summary>
    public static TimeSpan FailureWindow { get; } = TimeSpan.FromMinutes(15);

    /// <summary>How long the account stays locked.</summary>
    public static TimeSpan LockoutDuration { get; } = TimeSpan.FromMinutes(15);

    /// <summary>What one more failure means: the new count, and whether this failure locks the account.</summary>
    public static OgFailedAttempt NextFailure(int failures, DateTimeOffset? lastAttempt, DateTimeOffset now)
    {
        var withinWindow = lastAttempt is { } attempted && now - attempted <= FailureWindow;

        // A failure outside the window starts the count again, so an occasional mistype never accumulates.
        var count = withinWindow ? Math.Max(0, failures) + 1 : 1;

        return count >= MaximumFailures
            ? new OgFailedAttempt(count, Locked: true, LockedUntil: now + LockoutDuration)
            : new OgFailedAttempt(count, Locked: false, LockedUntil: null);
    }

    /// <summary>True when a lock set for <paramref name="lockedUntil"/> still applies at <paramref name="now"/>.</summary>
    public static bool IsLocked(DateTimeOffset? lockedUntil, DateTimeOffset now) =>
        lockedUntil is { } until && until > now;

    /// <summary>The sentence the owner sees when they proved the password but the account is locked.</summary>
    public static string LockedSentence(DateTimeOffset lockedUntil, string? timeZoneId)
    {
        var zone = TimeZoneInfo.Local;

        // TryFind… rather than Find…: an unknown zone id must not throw on the sign-in path.
        if (!string.IsNullOrWhiteSpace(timeZoneId)
            && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var found))
        {
            zone = found;
        }

        var local = TimeZoneInfo.ConvertTime(lockedUntil, zone);

        return $"This account is locked until {local:HH:mm}. Either wait, or ask an administrator to unlock it.";
    }
}

/// <summary>What one failed attempt produced.</summary>
/// <param name="Count">The failures counted inside the window, including this one.</param>
/// <param name="Locked">True when this attempt reached the threshold.</param>
/// <param name="LockedUntil">When the lock ends, when it was set.</param>
public sealed record OgFailedAttempt(int Count, bool Locked, DateTimeOffset? LockedUntil);
