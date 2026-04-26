using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Constants
{
    /// <summary>
    /// Canonical string tokens for <see cref="PermitApplicationStatus"/> in persistence
    /// (PERMIT_APPLICATION filters, PERMIT_STATUS_HISTORY.STATUS, transition rules).
    /// </summary>
    public static class PermitApplicationStatusCodes
    {
        public const string Draft = "DRAFT";
        public const string Submitted = "SUBMITTED";
        public const string UnderReview = "UNDER_REVIEW";
        public const string AdditionalInformationRequired = "ADDITIONAL_INFO_REQUIRED";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string Withdrawn = "WITHDRAWN";
        public const string Expired = "EXPIRED";
        public const string Renewed = "RENEWED";

        /// <summary>
        /// Returns the uppercase underscore key stored in the database and used by transition rules.
        /// </summary>
        public static string ToStorageKey(PermitApplicationStatus? status)
        {
            if (status == null)
                return Draft;

            return status.Value switch
            {
                PermitApplicationStatus.Draft => Draft,
                PermitApplicationStatus.Submitted => Submitted,
                PermitApplicationStatus.UnderReview => UnderReview,
                PermitApplicationStatus.AdditionalInformationRequired => AdditionalInformationRequired,
                PermitApplicationStatus.Approved => Approved,
                PermitApplicationStatus.Rejected => Rejected,
                PermitApplicationStatus.Withdrawn => Withdrawn,
                PermitApplicationStatus.Expired => Expired,
                PermitApplicationStatus.Renewed => Renewed,
                _ => Draft
            };
        }

        /// <summary>
        /// Parses persisted status strings (including legacy Pascal-case enum names).
        /// </summary>
        public static PermitApplicationStatus FromStorageKey(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return PermitApplicationStatus.Draft;

            var upper = status.Trim().ToUpperInvariant();
            return upper switch
            {
                Draft => PermitApplicationStatus.Draft,
                Submitted => PermitApplicationStatus.Submitted,
                UnderReview => PermitApplicationStatus.UnderReview,
                AdditionalInformationRequired => PermitApplicationStatus.AdditionalInformationRequired,
                Approved => PermitApplicationStatus.Approved,
                Rejected => PermitApplicationStatus.Rejected,
                Withdrawn => PermitApplicationStatus.Withdrawn,
                Expired => PermitApplicationStatus.Expired,
                Renewed => PermitApplicationStatus.Renewed,
                _ => Enum.TryParse<PermitApplicationStatus>(status, ignoreCase: true, out var parsed)
                    ? parsed
                    : PermitApplicationStatus.Draft
            };
        }
    }
}
