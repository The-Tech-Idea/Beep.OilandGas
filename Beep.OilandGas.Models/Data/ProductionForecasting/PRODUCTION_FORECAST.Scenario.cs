using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.ProductionForecasting
{
    /// <summary>
    /// The scenario, version and approval columns of a production forecast (RES-07). They are in a partial file so the
    /// generated columns stay untouched, and they are additive so an existing forecast row remains valid.
    /// </summary>
    /// <remarks>
    /// A forecast is never updated: a save inserts a new version, and an approved version is marked immutable. That is what
    /// makes AC-3 (versions never overwrite) and AC-4 (a referenced forecast cannot change) true of the data rather than
    /// only of the screen.
    /// </remarks>
    public partial class PRODUCTION_FORECAST
    {
        /// <summary>The RES-06 <c>DECLINE_FIT.FIT_ID</c> this forecast was generated from (AC-2).</summary>
        [MaxLength(64)]
        public string? SOURCE_FIT_ID
        {
            get => SourceFitIdValue;
            set => SetProperty(ref SourceFitIdValue, value);
        }

        private string? SourceFitIdValue;

        /// <summary>Draft until reviewed, then Approved; Archived when superseded. From the run-status reference set.</summary>
        [MaxLength(32)]
        public string STATUS
        {
            get => StatusValue;
            set => SetProperty(ref StatusValue, value);
        }

        private string StatusValue = "Draft";

        /// <summary>A risk rating code from the reference set; Unknown when none was chosen.</summary>
        [MaxLength(32)]
        public string? RISK_RATING
        {
            get => RiskRatingValue;
            set => SetProperty(ref RiskRatingValue, value);
        }

        private string? RiskRatingValue;

        /// <summary>1 for the first saved version of a name and entity; each save adds one.</summary>
        public int VERSION
        {
            get => VersionValue;
            set => SetProperty(ref VersionValue, value);
        }

        private int VersionValue;

        /// <summary>The forecast this version supersedes; null for the first.</summary>
        [MaxLength(64)]
        public string? VERSION_OF
        {
            get => VersionOfValue;
            set => SetProperty(ref VersionOfValue, value);
        }

        private string? VersionOfValue;

        /// <summary>'Y' once approved: an approved forecast is never edited or overwritten (AC-4).</summary>
        [MaxLength(1)]
        public string IMMUTABLE_IND
        {
            get => ImmutableIndValue;
            set => SetProperty(ref ImmutableIndValue, value);
        }

        private string ImmutableIndValue = "N";

        /// <summary>The rate the profile was stopped at, if an economic limit was given.</summary>
        public decimal? ECONOMIC_LIMIT
        {
            get => EconomicLimitValue;
            set => SetProperty(ref EconomicLimitValue, value);
        }

        private decimal? EconomicLimitValue;

        /// <summary>A ceiling the profile had to respect; a violation is refused rather than clamped (AC-5).</summary>
        public decimal? MAX_RATE
        {
            get => MaxRateValue;
            set => SetProperty(ref MaxRateValue, value);
        }

        private decimal? MaxRateValue;

        // ---- AC-6: a unit per stream ----

        [MaxLength(16)]
        public string? OIL_VOLUME_OUOM
        {
            get => OilVolumeOuomValue;
            set => SetProperty(ref OilVolumeOuomValue, value);
        }

        private string? OilVolumeOuomValue;

        [MaxLength(16)]
        public string? GAS_VOLUME_OUOM
        {
            get => GasVolumeOuomValue;
            set => SetProperty(ref GasVolumeOuomValue, value);
        }

        private string? GasVolumeOuomValue;

        [MaxLength(16)]
        public string? WATER_VOLUME_OUOM
        {
            get => WaterVolumeOuomValue;
            set => SetProperty(ref WaterVolumeOuomValue, value);
        }

        private string? WaterVolumeOuomValue;

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
    }
}
