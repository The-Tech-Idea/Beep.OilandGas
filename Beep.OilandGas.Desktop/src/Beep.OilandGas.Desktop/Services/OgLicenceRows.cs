using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Security;

namespace TheTechIdea.Beep.OilandGas.Desktop.Services;

/// <summary>One labelled fact on the licence page.</summary>
public sealed record OgLicenceRow(string Field, string Value);

/// <summary>
/// REL-02: the licence page's facts, projected from the entitlement so the page and its tests cannot disagree.
/// </summary>
/// <remarks>
/// Every value here is either what the licence says or the exact reason it is not usable. Nothing is inferred:
/// a missing seat count reads "(not stated)", and an unverified signature names its code instead of saying
/// "unknown".
/// </remarks>
public static class OgLicenceRows
{
    public static IReadOnlyList<OgLicenceRow> From(
        OgEntitlement entitlement,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(entitlement);

        return
        [
            new("Status", entitlement.StatusText),
            new("Edition", entitlement.Edition),
            new("Customer", entitlement.Customer),
            new("Modules", entitlement.Modules.Count == 0
                ? "(none)"
                : string.Join(", ", entitlement.Modules)),
            new("Expires", entitlement.ExpiresAt is { } at ? at.ToString("yyyy-MM-dd") : "(none)"),
            new("Days remaining", entitlement.DaysRemaining?.ToString() ?? "(none)"),
            new("Seats", entitlement.Seats?.ToString() ?? "(not stated)"),
            new("Signature", entitlement.SignatureVerified
                ? $"verified ({entitlement.KeyId})"
                : "not verified"),
            new("Key", entitlement.KeyId ?? "(none)"),
            new("Enforcement", entitlement.Enforcement.ToString()),
            new("Reads", entitlement.IsUsable
                ? "every granted module"
                : entitlement.Enforcement == OgLicenceEnforcement.Permissive
                    ? "every module (enforcement is off)"
                    : "core only"),
            new("Writes", entitlement.IsUsable
                ? "every granted module"
                : entitlement.Enforcement == OgLicenceEnforcement.Permissive
                    ? "every module (enforcement is off)"
                    : "core only"),
            new("File", filePath),
        ];
    }
}
