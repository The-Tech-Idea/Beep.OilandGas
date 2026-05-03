using System;
using System.Collections.Generic;

namespace Beep.OilandGas.LeaseAcquisition.Constants;

/// <summary>
/// Well-known <c>REFERENCE_CODE</c> values for <see cref="LeaseReferenceSets.LandRightOperationalStatus"/>.
/// </summary>
public static class LeaseReferenceCodes
{
    public const string Active = "ACTIVE";
    public const string Inactive = "INACTIVE";
    public const string Pending = "PENDING";
    public const string Terminated = "TERMINATED";

    private static readonly HashSet<string> LandRightOperationalCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        Active, Inactive, Pending, Terminated
    };

    public static bool IsDefinedLandRightOperationalStatus(string? code)
        => !string.IsNullOrWhiteSpace(code) && LandRightOperationalCodes.Contains(code.Trim());
}
