using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// A reserves disclosure for one field and period (RES-05): the totals as they stood when it was approved, who
    /// prepared it, who approved it, and how many entries were excluded because they carry no category.
    /// </summary>
    /// <remarks>
    /// The approval is a record rather than a flag on the entries, because the command table's "approve disclosure"
    /// writes and because a disclosure has to be reproducible after the entries move on: the totals are copied onto this
    /// row at approval time.
    /// </remarks>
    public class RESERVE_DISCLOSURE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string DISCLOSURE_ID
        {
            get => DisclosureIdValue;
            set => SetProperty(ref DisclosureIdValue, value);
        }

        private string DisclosureIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        /// <summary>The first day of the period the disclosure covers.</summary>
        public DateTime PERIOD
        {
            get => PeriodValue;
            set => SetProperty(ref PeriodValue, value);
        }

        private DateTime PeriodValue;

        public decimal? GROSS_TOTAL
        {
            get => GrossTotalValue;
            set => SetProperty(ref GrossTotalValue, value);
        }

        private decimal? GrossTotalValue;

        public decimal? NET_TOTAL
        {
            get => NetTotalValue;
            set => SetProperty(ref NetTotalValue, value);
        }

        private decimal? NetTotalValue;

        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        /// <summary>Entries that went into the totals.</summary>
        public int ENTRY_COUNT
        {
            get => EntryCountValue;
            set => SetProperty(ref EntryCountValue, value);
        }

        private int EntryCountValue;

        /// <summary>Entries left out because they carry no category (AC-1) - stated, never silently dropped.</summary>
        public int EXCLUDED_COUNT
        {
            get => ExcludedCountValue;
            set => SetProperty(ref ExcludedCountValue, value);
        }

        private int ExcludedCountValue;

        /// <summary>The classifier who prepared the entries; the approver must be someone else (separation of duties).</summary>
        [MaxLength(128)]
        public string? PREPARED_BY
        {
            get => PreparedByValue;
            set => SetProperty(ref PreparedByValue, value);
        }

        private string? PreparedByValue;

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

        /// <summary>APPROVED. A disclosure only exists once it was approved.</summary>
        [MaxLength(16)]
        public string STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string StateValue = "APPROVED";
    }
}
