using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// One casing string of a development well's design outline (DEV-03). This is the design's own outline - the
    /// number, order and set depth of the strings - and it is the input a casing-design check reads; the burst,
    /// collapse and tension arithmetic belongs to ENG-02 and is not performed here.
    /// </summary>
    public partial class DEVELOPMENT_CASING_STRING : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string CASING_STRING_ID
        {
            get => CasingStringIdValue;
            set => SetProperty(ref CasingStringIdValue, value);
        }

        private string CasingStringIdValue = string.Empty;

        [MaxLength(64)]
        public string DESIGN_ID
        {
            get => DesignIdValue;
            set => SetProperty(ref DesignIdValue, value);
        }

        private string DesignIdValue = string.Empty;

        /// <summary>1 for the shallowest string; the set depths must increase with the order.</summary>
        public int STRING_ORDER
        {
            get => StringOrderValue;
            set => SetProperty(ref StringOrderValue, value);
        }

        private int StringOrderValue;

        /// <summary>CONDUCTOR, SURFACE, INTERMEDIATE, PRODUCTION or LINER.</summary>
        [MaxLength(32)]
        public string? STRING_NAME
        {
            get => StringNameValue;
            set => SetProperty(ref StringNameValue, value);
        }

        private string? StringNameValue;

        public decimal OD
        {
            get => OdValue;
            set => SetProperty(ref OdValue, value);
        }

        private decimal OdValue;

        public decimal WEIGHT
        {
            get => WeightValue;
            set => SetProperty(ref WeightValue, value);
        }

        private decimal WeightValue;

        [MaxLength(32)]
        public string? GRADE
        {
            get => GradeValue;
            set => SetProperty(ref GradeValue, value);
        }

        private string? GradeValue;

        public decimal SET_DEPTH
        {
            get => SetDepthValue;
            set => SetProperty(ref SetDepthValue, value);
        }

        private decimal SetDepthValue;

        /// <summary>The unit of <see cref="SET_DEPTH"/>; it must be the design's depth unit (AC-2).</summary>
        [MaxLength(16)]
        public string? DEPTH_OUOM
        {
            get => DepthOuomValue;
            set => SetProperty(ref DepthOuomValue, value);
        }

        private string? DepthOuomValue;
    }
}
