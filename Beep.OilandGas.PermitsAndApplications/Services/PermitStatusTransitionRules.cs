using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.PermitsAndApplications;
using Beep.OilandGas.PermitsAndApplications.Constants;

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

        /// <summary>
        /// The storage key for a status: a stored key as it is, or a <see cref="PermitApplicationStatus"/> member's name
        /// (the services pass <c>status.ToString()</c>) as its key. Upper-casing a member's name alone had made
        /// <c>UnderReview</c> into <c>UNDERREVIEW</c>, a key the table does not have, so nothing could leave review.
        /// </summary>
        public static string Normalize(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return "DRAFT";

            var upper = status.Trim().ToUpperInvariant();
            if (AllowedTransitions.ContainsKey(upper))
                return upper;

            return Enum.TryParse<PermitApplicationStatus>(status.Trim(), ignoreCase: true, out var member)
                ? PermitApplicationStatusCodes.ToStorageKey(member)
                : upper;
        }

        /// <summary>
        /// The refusal of a status change the rules do not allow, in words for the person who asked
        /// (OILGAS-CATCH-01: it had been an <see cref="InvalidOperationException"/>, answered 500 as a fault).
        /// </summary>
        public static RefusalException RefuseTransition(string? currentStatus, string? nextStatus)
        {
            var current = Normalize(currentStatus);
            var next = Normalize(nextStatus);

            return string.Equals(current, next, StringComparison.OrdinalIgnoreCase)
                ? RefusalException.Conflict($"The permit application is already {Describe(current)}.")
                : RefusalException.Conflict(
                    $"A permit application that is {Describe(current)} cannot be moved to {Describe(next)}.");
        }

        private static string Describe(string storageKey) => storageKey switch
        {
            PermitApplicationStatusCodes.AdditionalInformationRequired => "waiting for additional information",
            _ => storageKey.Replace('_', ' ').ToLowerInvariant()
        };
    }
}
