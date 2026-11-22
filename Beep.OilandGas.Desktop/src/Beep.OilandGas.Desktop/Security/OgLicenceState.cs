using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Security;

/// <summary>Projects authenticated claims into the rights the product uses.</summary>
public static class OgLicenceState
{
    public static OgEntitlement ToEntitlement(
        OgLicenceClaims claims,
        string? keyId,
        OgLicenceEnforcement enforcement,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(claims);

        var expiresAt = claims.ExpiresAt;
        var expired = expiresAt is { } expiry && now > expiry;
        var graceEnds = expired ? expiresAt!.Value.AddDays(claims.GraceDays) : (DateTimeOffset?)null;
        var inGrace = expired && now <= graceEnds!.Value;
        var usable = !expired || inGrace;

        int? daysRemaining = expiresAt is { } at
            ? (int)Math.Ceiling(((expired ? at.AddDays(claims.GraceDays) : at) - now).TotalDays)
            : null;

        var status = expired switch
        {
            false when expiresAt is { } until =>
                $"Licensed to {claims.Customer} until {until:yyyy-MM-dd} ({daysRemaining} day(s) remaining).",
            false =>
                $"Licensed to {claims.Customer}; this licence does not expire.",
            true when inGrace =>
                $"The licence expired on {expiresAt:yyyy-MM-dd}; {daysRemaining} grace day(s) remain and everything works.",
            true when enforcement == OgLicenceEnforcement.Permissive =>
                $"The licence expired on {expiresAt:yyyy-MM-dd}. Enforcement is off, so nothing is blocked.",
            true =>
                $"The licence expired on {expiresAt:yyyy-MM-dd}. Reads continue; writes are blocked.",
        };

        return new OgEntitlement(
            Edition: claims.Edition,
            Customer: claims.Customer,
            Modules: claims.Modules.ToList(),
            ExpiresAt: expiresAt,
            IsLicensed: true,
            IsExpired: expired,
            IsInGrace: inGrace,
            IsUsable: usable,
            StatusText: status,
            Error: usable ? null : $"The licence expired on {expiresAt:yyyy-MM-dd}.",
            Enforcement: enforcement,
            SignatureVerified: true,
            KeyId: keyId,
            DaysRemaining: daysRemaining,
            Seats: claims.Seats);
    }
}
