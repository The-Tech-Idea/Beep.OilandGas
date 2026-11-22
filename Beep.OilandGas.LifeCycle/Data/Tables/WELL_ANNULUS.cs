using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.LifeCycle.Data.Tables
{
    /// One surface-accessible annulus of a well (ENG-04): the strings that bound it, what fills it, and the inputs
    /// the MAWOP and MAASP screening needs.
    /// </summary>
    /// <remarks>
    /// The tools name annuli from the strings that reach surface, but the fluid, the fracture gradient, the wellhead
    /// rating and any MAASP set by integrity engineering are operator data with nowhere to live. Without them the
    /// screening can compute a MAWOP and nothing else, which is exactly why the feature asks for the basis to be shown.
    /// </remarks>
    public class WELL_ANNULUS : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string ANNULUS_ID
        {
            get => AnnulusidValue;
            set => SetProperty(ref AnnulusidValue, value);
        }

        private string AnnulusidValue = string.Empty;

        [MaxLength(64)]
        public string UWI
        {
            get => UwiValue;
            set => SetProperty(ref UwiValue, value);
        }

        private string UwiValue = string.Empty;

        /// <summary>A, B, C... from the inside out, as the tool names them.</summary>
        [MaxLength(16)]
        public string ANNULUS_NAME
        {
            get => AnnulusnameValue;
            set => SetProperty(ref AnnulusnameValue, value);
        }

        private string AnnulusnameValue = string.Empty;

        /// <summary>The string forming the inner wall, by the id the model assigns it.</summary>
        [MaxLength(128)]
        public string INNER_STRING_ID
        {
            get => InnerstringidValue;
            set => SetProperty(ref InnerstringidValue, value);
        }

        private string InnerstringidValue = string.Empty;

        /// <summary>The string forming the outer wall.</summary>
        [MaxLength(128)]
        public string OUTER_STRING_ID
        {
            get => OuterstringidValue;
            set => SetProperty(ref OuterstringidValue, value);
        }

        private string OuterstringidValue = string.Empty;

        /// <summary>What fills the annulus (BRINE, GAS, CEMENT...), as the tool's enum names it.</summary>
        [MaxLength(32)]
        public string? FLUID
        {
            get => FluidValue;
            set => SetProperty(ref FluidValue, value);
        }

        private string? FluidValue = null;

        /// <summary>Fluid density, used for the hydrostatic gradient at the outer shoe.</summary>
        public decimal? FLUID_DENSITY
        {
            get => FluiddensityValue;
            set => SetProperty(ref FluiddensityValue, value);
        }

        private decimal? FluiddensityValue = null;

        [MaxLength(16)]
        public string? FLUID_DENSITY_OUOM
        {
            get => FluiddensityouomValue;
            set => SetProperty(ref FluiddensityouomValue, value);
        }

        private string? FluiddensityouomValue = null;

        /// <summary>Fracture gradient at the outer string's shoe, as a gradient or an equivalent mud weight.</summary>
        public decimal? SHOE_FRACTURE_GRADIENT
        {
            get => ShoefracturegradientValue;
            set => SetProperty(ref ShoefracturegradientValue, value);
        }

        private decimal? ShoefracturegradientValue = null;

        [MaxLength(16)]
        public string? SHOE_FRACTURE_GRADIENT_OUOM
        {
            get => ShoefracturegradientouomValue;
            set => SetProperty(ref ShoefracturegradientouomValue, value);
        }

        private string? ShoefracturegradientouomValue = null;

        /// <summary>Rating of the wellhead outlet or section for this annulus.</summary>
        public decimal? WELLHEAD_RATING
        {
            get => WellheadratingValue;
            set => SetProperty(ref WellheadratingValue, value);
        }

        private decimal? WellheadratingValue = null;

        [MaxLength(16)]
        public string? WELLHEAD_RATING_OUOM
        {
            get => WellheadratingouomValue;
            set => SetProperty(ref WellheadratingouomValue, value);
        }

        private string? WellheadratingouomValue = null;

        /// <summary>The MAASP integrity engineering set, when one is recorded; the screening value is compared with it.</summary>
        public decimal? RECORDED_MAASP
        {
            get => RecordedmaaspValue;
            set => SetProperty(ref RecordedmaaspValue, value);
        }

        private decimal? RecordedmaaspValue = null;

        [MaxLength(16)]
        public string? RECORDED_MAASP_OUOM
        {
            get => RecordedmaaspouomValue;
            set => SetProperty(ref RecordedmaaspouomValue, value);
        }

        private string? RecordedmaaspouomValue = null;

        /// <summary>The MAWOP as recorded, for the same comparison.</summary>
        public decimal? RECORDED_MAWOP
        {
            get => RecordedmawopValue;
            set => SetProperty(ref RecordedmawopValue, value);
        }

        private decimal? RecordedmawopValue = null;

        [MaxLength(16)]
        public string? RECORDED_MAWOP_OUOM
        {
            get => RecordedmawopouomValue;
            set => SetProperty(ref RecordedmawopouomValue, value);
        }

        private string? RecordedmawopouomValue = null;

        [MaxLength(1000)]
        public string? NOTES
        {
            get => NotesValue;
            set => SetProperty(ref NotesValue, value);
        }

        private string? NotesValue = null;

        [MaxLength(128)]
        public string? RECORDED_BY
        {
            get => RecordedbyValue;
            set => SetProperty(ref RecordedbyValue, value);
        }

        private string? RecordedbyValue = null;

        public DateTime? RECORDED_AT_UTC
        {
            get => RecordedatutcValue;
            set => SetProperty(ref RecordedatutcValue, value);
        }

        private DateTime? RecordedatutcValue = null;

    }
}
