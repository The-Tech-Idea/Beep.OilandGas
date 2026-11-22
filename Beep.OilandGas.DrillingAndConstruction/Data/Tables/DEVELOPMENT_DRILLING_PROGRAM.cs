using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// A drilling program as the development plan authors it (DEV-05), before an AFE or an investment handoff exists.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The module's <c>DRILLING_PROGRAM</c> is the funded company program: its schema requires an AFE, an investment
    /// handoff and the bounded original draft proof, and <c>DrillingOperationService</c> reads it by AFE. A plan-stage
    /// program has none of those yet, so it is a separate record and this row is the one the plan screen writes. When
    /// the funding is approved, the company program is created from it through the module's own flow; the two are never
    /// the same row and neither is a copy of the other.
    /// </para>
    /// <para>
    /// The record is versioned: a revision appends a row with a higher <see cref="VERSION"/> and the same
    /// <see cref="LINEAGE_ID"/>, so an approved version is never mutated (DEV-05 AC-4).
    /// </para>
    /// </remarks>
    public class DEVELOPMENT_DRILLING_PROGRAM : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string ProgramIdValue = string.Empty;

        /// <summary>The first version's id, repeated on every revision, so the versions form one lineage.</summary>
        [MaxLength(64)]
        public string LINEAGE_ID
        {
            get => LineageIdValue;
            set => SetProperty(ref LineageIdValue, value);
        }

        private string LineageIdValue = string.Empty;

        public int VERSION
        {
            get => VersionValue;
            set => SetProperty(ref VersionValue, value);
        }

        private int VersionValue = 1;

        /// <summary>The company this program was authored for.</summary>
        [MaxLength(128)]
        public string? COMPANY_ID
        {
            get => CompanyIdValue;
            set => SetProperty(ref CompanyIdValue, value);
        }

        private string? CompanyIdValue;

        [MaxLength(64)]
        public string? FDP_ID
        {
            get => FdpIdValue;
            set => SetProperty(ref FdpIdValue, value);
        }

        private string? FdpIdValue;

        /// <summary>The plan baseline the program was authored against (DEV-02), when one exists.</summary>
        [MaxLength(64)]
        public string? FDP_BASELINE_ID
        {
            get => FdpBaselineIdValue;
            set => SetProperty(ref FdpBaselineIdValue, value);
        }

        private string? FdpBaselineIdValue;

        [MaxLength(64)]
        public string? FIELD_ID
        {
            get => FieldIdValue;
            set => SetProperty(ref FieldIdValue, value);
        }

        private string? FieldIdValue;

        [MaxLength(64)]
        public string WELL_ID
        {
            get => WellIdValue;
            set => SetProperty(ref WellIdValue, value);
        }

        private string WellIdValue = string.Empty;

        /// <summary>The DEV-03 design the program was authored from.</summary>
        [MaxLength(64)]
        public string? DESIGN_ID
        {
            get => DesignIdValue;
            set => SetProperty(ref DesignIdValue, value);
        }

        private string? DesignIdValue;

        [MaxLength(128)]
        public string PROGRAM_NAME
        {
            get => ProgramNameValue;
            set => SetProperty(ref ProgramNameValue, value);
        }

        private string ProgramNameValue = string.Empty;

        public string? DESCRIPTION_OBJECTIVE
        {
            get => DescriptionObjectiveValue;
            set => SetProperty(ref DescriptionObjectiveValue, value);
        }

        private string? DescriptionObjectiveValue;

        /// <summary>DRAFT, SUBMITTED, APPROVED, APPROVED_WITH_COMMENTS, RETURNED or REJECTED.</summary>
        [MaxLength(32)]
        public string? STATE
        {
            get => StateValue;
            set => SetProperty(ref StateValue, value);
        }

        private string? StateValue;

        public decimal? PLANNED_MEASURED_DEPTH
        {
            get => PlannedMeasuredDepthValue;
            set => SetProperty(ref PlannedMeasuredDepthValue, value);
        }

        private decimal? PlannedMeasuredDepthValue;

        public decimal? PLANNED_TRUE_VERTICAL_DEPTH
        {
            get => PlannedTrueVerticalDepthValue;
            set => SetProperty(ref PlannedTrueVerticalDepthValue, value);
        }

        private decimal? PlannedTrueVerticalDepthValue;

        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthOuomValue;
            set => SetProperty(ref DepthOuomValue, value);
        }

        private string? DepthOuomValue;

        public decimal? PLANNED_DURATION_DAYS
        {
            get => PlannedDurationDaysValue;
            set => SetProperty(ref PlannedDurationDaysValue, value);
        }

        private decimal? PlannedDurationDaysValue;

        /// <summary>The currency every section cost is stated in (AC-6).</summary>
        [MaxLength(16)]
        public string? CURRENCY
        {
            get => CurrencyValue;
            set => SetProperty(ref CurrencyValue, value);
        }

        private string? CurrencyValue;
    }
}
