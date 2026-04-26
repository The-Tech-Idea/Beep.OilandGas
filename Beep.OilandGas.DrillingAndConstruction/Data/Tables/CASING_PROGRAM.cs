using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// Casing string design record — one row per casing/liner string.
    /// Per PPDM 3.9 WELL_TUBULAR complement; project-owned design data.
    /// </summary>
    public partial class CASING_PROGRAM : ModelEntityBase
    {
        private System.String CASING_IDValue = string.Empty;
        public System.String CASING_ID
        {
            get { return this.CASING_IDValue; }
            set { SetProperty(ref CASING_IDValue, value); }
        }

        private System.String PROGRAM_IDValue = string.Empty;
        public System.String PROGRAM_ID
        {
            get { return this.PROGRAM_IDValue; }
            set { SetProperty(ref PROGRAM_IDValue, value); }
        }

        private System.String WELL_IDValue = string.Empty;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.String CASING_STRINGValue = string.Empty;
        /// <summary>String designation: CONDUCTOR / SURFACE / INTERMEDIATE / PRODUCTION / LINER / TIEBACK.</summary>
        public System.String CASING_STRING
        {
            get { return this.CASING_STRINGValue; }
            set { SetProperty(ref CASING_STRINGValue, value); }
        }

        private System.Decimal? NOMINAL_OD_SIZEValue;
        /// <summary>Nominal outer diameter (inches).</summary>
        public System.Decimal? NOMINAL_OD_SIZE
        {
            get { return this.NOMINAL_OD_SIZEValue; }
            set { SetProperty(ref NOMINAL_OD_SIZEValue, value); }
        }

        private System.Decimal? WEIGHT_PER_FTValue;
        /// <summary>Linear weight (lb/ft or kg/m).</summary>
        public System.Decimal? WEIGHT_PER_FT
        {
            get { return this.WEIGHT_PER_FTValue; }
            set { SetProperty(ref WEIGHT_PER_FTValue, value); }
        }

        private System.String GRADE_STEELValue = string.Empty;
        /// <summary>Steel grade (e.g. J-55, K-55, N-80, P-110).</summary>
        public System.String GRADE_STEEL
        {
            get { return this.GRADE_STEELValue; }
            set { SetProperty(ref GRADE_STEELValue, value); }
        }

        private System.String CONNECTION_TYPEValue = string.Empty;
        /// <summary>Thread connection type (e.g. BTC, LTC, VAM TOP, Premium).</summary>
        public System.String CONNECTION_TYPE
        {
            get { return this.CONNECTION_TYPEValue; }
            set { SetProperty(ref CONNECTION_TYPEValue, value); }
        }

        private System.Decimal? PLANNED_SET_DEPTH_MDValue;
        /// <summary>Planned shoe setting depth — measured depth.</summary>
        public System.Decimal? PLANNED_SET_DEPTH_MD
        {
            get { return this.PLANNED_SET_DEPTH_MDValue; }
            set { SetProperty(ref PLANNED_SET_DEPTH_MDValue, value); }
        }

        private System.Decimal? ACTUAL_SET_DEPTH_MDValue;
        public System.Decimal? ACTUAL_SET_DEPTH_MD
        {
            get { return this.ACTUAL_SET_DEPTH_MDValue; }
            set { SetProperty(ref ACTUAL_SET_DEPTH_MDValue, value); }
        }

        private System.String DEPTH_OUOMValue = string.Empty;
        public System.String DEPTH_OUOM
        {
            get { return this.DEPTH_OUOMValue; }
            set { SetProperty(ref DEPTH_OUOMValue, value); }
        }

        private System.Decimal? BURST_RATINGValue;
        /// <summary>Internal yield pressure (psi or kPa).</summary>
        public System.Decimal? BURST_RATING
        {
            get { return this.BURST_RATINGValue; }
            set { SetProperty(ref BURST_RATINGValue, value); }
        }

        private System.Decimal? COLLAPSE_RATINGValue;
        public System.Decimal? COLLAPSE_RATING
        {
            get { return this.COLLAPSE_RATINGValue; }
            set { SetProperty(ref COLLAPSE_RATINGValue, value); }
        }

        private System.Decimal? TENSION_RATINGValue;
        /// <summary>Pipe body yield strength (klbs or kN).</summary>
        public System.Decimal? TENSION_RATING
        {
            get { return this.TENSION_RATINGValue; }
            set { SetProperty(ref TENSION_RATINGValue, value); }
        }

        private System.String STATUS_CASINGValue = string.Empty;
        /// <summary>PLANNED / RUN / CEMENTED / ABANDONED.</summary>
        public System.String STATUS_CASING
        {
            get { return this.STATUS_CASINGValue; }
            set { SetProperty(ref STATUS_CASINGValue, value); }
        }

        public CASING_PROGRAM() { }
    }
}
