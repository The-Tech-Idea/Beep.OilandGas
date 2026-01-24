using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PermitsAndApplications.Services
{
    public static class PermitStatusTransitionRules
    {
        private static readonly Dictionary<string, HashSet<string>> AllowedTransitions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["DRAFT"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SUBMITTED",
                "WITHDRAWN"
            },
            ["SUBMITTED"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "UNDER_REVIEW",
                "APPROVED",
                "REJECTED",
                "ADDITIONAL_INFO_REQUIRED",
                "WITHDRAWN"
            },
            ["UNDER_REVIEW"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "APPROVED",
                "REJECTED",
                "ADDITIONAL_INFO_REQUIRED",
                "WITHDRAWN"
            },
            ["ADDITIONAL_INFO_REQUIRED"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "SUBMITTED",
                "WITHDRAWN"
            },
            ["APPROVED"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "RENEWED",
                "EXPIRED"
            },
            ["REJECTED"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "WITHDRAWN"
            },
            ["RENEWED"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "APPROVED",
                "EXPIRED"
            },
            ["EXPIRED"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "RENEWED"
            },
            ["WITHDRAWN"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
            }
        };

        public static bool IsTransitionAllowed(string? currentStatus, string? nextStatus)
        {
            var normalizedCurrent = Normalize(currentStatus);
            var normalizedNext = Normalize(nextStatus);

            if (string.Equals(normalizedCurrent, normalizedNext, StringComparison.OrdinalIgnoreCase))
                return false;

            if (!AllowedTransitions.TryGetValue(normalizedCurrent, out var allowed))
                return false;

            return allowed.Contains(normalizedNext);
        }

        public static string Normalize(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "DRAFT";

            return status.Trim().ToUpperInvariant();
        }
    }
}
