using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// One hole section of a drilling program (DEV-05): the hole it makes, the casing it runs, the mud it uses and what
    /// it costs. This is the program's own content; <c>CASING_PROGRAM</c> remains the detailed casing record (grade,
    /// weight, ratings) that the execution side owns.
    /// </summary>
    public class DRILLING_PROGRAM_SECTION : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string SECTION_ID
        {
            get => SectionIdValue;
            set => SetProperty(ref SectionIdValue, value);
        }

        private string SectionIdValue = string.Empty;

        [MaxLength(64)]
        public string PROGRAM_ID
        {
            get => ProgramIdValue;
            set => SetProperty(ref ProgramIdValue, value);
        }

        private string ProgramIdValue = string.Empty;

        /// <summary>1 for the shallowest section; the program shows them in this order.</summary>
        public int SECTION_SEQ
        {
            get => SectionSeqValue;
            set => SetProperty(ref SectionSeqValue, value);
        }

        private int SectionSeqValue;

        [MaxLength(64)]
        public string? SECTION_NAME
        {
            get => SectionNameValue;
            set => SetProperty(ref SectionNameValue, value);
        }

        private string? SectionNameValue;

        public decimal HOLE_SIZE
        {
            get => HoleSizeValue;
            set => SetProperty(ref HoleSizeValue, value);
        }

        private decimal HoleSizeValue;

        /// <summary>The unit of <see cref="HOLE_SIZE"/>; a diameter without it is refused (AC-5).</summary>
        [MaxLength(16)]
        public string? HOLE_SIZE_OUOM
        {
            get => HoleSizeOuomValue;
            set => SetProperty(ref HoleSizeOuomValue, value);
        }

        private string? HoleSizeOuomValue;

        public decimal TOP_MD
        {
            get => TopMdValue;
            set => SetProperty(ref TopMdValue, value);
        }

        private decimal TopMdValue;

        public decimal BOTTOM_MD
        {
            get => BottomMdValue;
            set => SetProperty(ref BottomMdValue, value);
        }

        private decimal BottomMdValue;

        /// <summary>The unit of <see cref="TOP_MD"/> and <see cref="BOTTOM_MD"/>; required (AC-5).</summary>
        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthOuomValue;
            set => SetProperty(ref DepthOuomValue, value);
        }

        private string? DepthOuomValue;

        public decimal CASING_OD
        {
            get => CasingOdValue;
            set => SetProperty(ref CasingOdValue, value);
        }

        private decimal CasingOdValue;

        /// <summary>The unit of <see cref="CASING_OD"/>; when a casing OD is recorded its unit is required (AC-5).</summary>
        [MaxLength(16)]
        public string? CASING_OD_OUOM
        {
            get => CasingOdOuomValue;
            set => SetProperty(ref CasingOdOuomValue, value);
        }

        private string? CasingOdOuomValue;

        public decimal MUD_WEIGHT
        {
            get => MudWeightValue;
            set => SetProperty(ref MudWeightValue, value);
        }

        private decimal MudWeightValue;

        /// <summary>The unit of <see cref="MUD_WEIGHT"/> (for example ppg or sg).</summary>
        [MaxLength(16)]
        public string? MUD_WEIGHT_OUOM
        {
            get => MudWeightOuomValue;
            set => SetProperty(ref MudWeightOuomValue, value);
        }

        private string? MudWeightOuomValue;

        public decimal? DURATION_DAYS
        {
            get => DurationDaysValue;
            set => SetProperty(ref DurationDaysValue, value);
        }

        private decimal? DurationDaysValue;

        public decimal? COST
        {
            get => CostValue;
            set => SetProperty(ref CostValue, value);
        }

        private decimal? CostValue;

        /// <summary>The unit of <see cref="COST"/> and of every section of the same program (AC-6).</summary>
        [MaxLength(16)]
        public string? COST_CURRENCY
        {
            get => CostCurrencyValue;
            set => SetProperty(ref CostCurrencyValue, value);
        }

        private string? CostCurrencyValue;
    }
}
