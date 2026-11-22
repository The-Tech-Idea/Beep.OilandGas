using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    /// <summary>
    /// One reserves entry for a pool (RES-05): its category and confidence, the gross and net volumes with their unit,
    /// the effective date, the revision it is, and who classified it.
    /// </summary>
    /// <remarks>
    /// A revision is a new row that supersedes the previous one through <see cref="SUPERSEDES_ENTRY_ID"/>; no existing row
    /// is ever updated or deleted, which is what makes AC-3 ("revisions never overwrite history") true of the data and not
    /// only of the screen. A category that is not recorded is stored as null and is excluded from the totals.
    /// </remarks>
    public class RESERVE_ENTRY : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string ENTRY_ID
        {
            get => EntryIdValue;
            set => SetProperty(ref EntryIdValue, value);
        }

        private string EntryIdValue = string.Empty;

        [MaxLength(64)]
        public string POOL_ID
        {
            get => PoolIdValue;
            set => SetProperty(ref PoolIdValue, value);
        }

        private string PoolIdValue = string.Empty;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        /// <summary>PROVED, PROBABLE or POSSIBLE; null is "unclassified" and is excluded from the totals (AC-1).</summary>
        [MaxLength(16)]
        public string? CATEGORY
        {
            get => CategoryValue;
            set => SetProperty(ref CategoryValue, value);
        }

        private string? CategoryValue;

        public decimal? GROSS_VOLUME
        {
            get => GrossVolumeValue;
            set => SetProperty(ref GrossVolumeValue, value);
        }

        private decimal? GrossVolumeValue;

        /// <summary>Net to the company's working interest; never above the gross volume.</summary>
        public decimal? NET_VOLUME
        {
            get => NetVolumeValue;
            set => SetProperty(ref NetVolumeValue, value);
        }

        private decimal? NetVolumeValue;

        [MaxLength(16)]
        public string? VOLUME_OUOM
        {
            get => VolumeOuomValue;
            set => SetProperty(ref VolumeOuomValue, value);
        }

        private string? VolumeOuomValue;

        public DateTime? EFFECTIVE_DATE
        {
            get => EffectiveDateValue;
            set => SetProperty(ref EffectiveDateValue, value);
        }

        private DateTime? EffectiveDateValue;

        /// <summary>1 for the first entry of a pool and category; each revision adds one.</summary>
        public int REVISION
        {
            get => RevisionValue;
            set => SetProperty(ref RevisionValue, value);
        }

        private int RevisionValue;

        /// <summary>Who classified the entry. Missing means the entry is flagged and the disclosure is blocked (AC-5).</summary>
        [MaxLength(128)]
        public string? CLASSIFIER
        {
            get => ClassifierValue;
            set => SetProperty(ref ClassifierValue, value);
        }

        private string? ClassifierValue;

        /// <summary>The entry this revision replaces; null for a first entry. History is never overwritten.</summary>
        [MaxLength(64)]
        public string? SUPERSEDES_ENTRY_ID
        {
            get => SupersedesEntryIdValue;
            set => SetProperty(ref SupersedesEntryIdValue, value);
        }

        private string? SupersedesEntryIdValue;

        /// <summary>Why the revision was made, kept so a later reader can see what changed and why.</summary>
        [MaxLength(512)]
        public string? BASIS_TEXT
        {
            get => BasisTextValue;
            set => SetProperty(ref BasisTextValue, value);
        }

        private string? BasisTextValue;

        [MaxLength(128)]
        public string? ACTOR
        {
            get => ActorValue;
            set => SetProperty(ref ActorValue, value);
        }

        private string? ActorValue;

        public DateTime? RECORDED_AT
        {
            get => RecordedAtValue;
            set => SetProperty(ref RecordedAtValue, value);
        }

        private DateTime? RecordedAtValue;
    }
}
