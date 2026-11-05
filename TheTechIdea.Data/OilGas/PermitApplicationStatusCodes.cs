using System;
using Beep.OilandGas.Models.Data.PermitsAndApplications;

namespace Beep.OilandGas.PermitsAndApplications.Constants
{
    /// <summary>
    /// Canonical history and transition tokens for <see cref="PermitApplicationStatus"/>.
    /// Native header status is read as its canonical enum, without guessing a provider filter representation.
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
                PermitApplicationStatus.Renewed => Renewed
            };
        }

    }
}
