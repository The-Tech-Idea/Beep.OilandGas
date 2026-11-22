using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    /// <summary>
    /// The procurement columns of a purchase order (ACC-05): the author, the approver, the AFE it commits against, and the
    /// closure.
    /// </summary>
    /// <remarks>
    /// The generated table held the number, the vendor, the dates, the total and a status. It had no author, so AC-6's
    /// "the author cannot approve" had nothing to compare against, and no AFE link, so a commitment could not be measured
    /// against the authorisation it belongs to (AC-4). These columns are additive, so an existing row stays valid.
    /// </remarks>
    public partial class PURCHASE_ORDER
    {
        /// <summary>Who raised the order - the person who may not approve it.</summary>
        [MaxLength(128)]
        public string? AUTHOR
        {
            get => AuthorValue;
            set => SetProperty(ref AuthorValue, value);
        }

        private string? AuthorValue;

        [MaxLength(128)]
        public string? APPROVED_BY
        {
            get => ApprovedByValue;
            set => SetProperty(ref ApprovedByValue, value);
        }

        private string? ApprovedByValue;

        public DateTime? APPROVED_AT
        {
            get => ApprovedAtValue;
            set => SetProperty(ref ApprovedAtValue, value);
        }

        private DateTime? ApprovedAtValue;

        /// <summary>The authorisation the order commits against (AC-4).</summary>
        [MaxLength(128)]
        public string? AFE_ID
        {
            get => AfeValue;
            set => SetProperty(ref AfeValue, value);
        }

        private string? AfeValue;

        public DateTime? CLOSED_AT
        {
            get => ClosedAtValue;
            set => SetProperty(ref ClosedAtValue, value);
        }

        private DateTime? ClosedAtValue;
    }
}
