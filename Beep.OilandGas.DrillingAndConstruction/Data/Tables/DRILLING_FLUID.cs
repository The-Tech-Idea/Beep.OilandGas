using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// Drilling fluid (mud) properties log — recorded per DDR or per survey depth.
    /// Per IADC / API Spec 13A mud reporting standards.
    /// </summary>
    public partial class DRILLING_FLUID : ModelEntityBase
    {
        private System.String FLUID_IDValue = string.Empty;
        public System.String FLUID_ID
        {
            get { return this.FLUID_IDValue; }
            set { SetProperty(ref FLUID_IDValue, value); }
        }

        private System.String DDR_IDValue = string.Empty;
        /// <summary>FK to DRILLING_DAILY_REPORT.</summary>
        public System.String DDR_ID
        {
            get { return this.DDR_IDValue; }
            set { SetProperty(ref DDR_IDValue, value); }
        }

        private System.String WELL_IDValue = string.Empty;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.DateTime? MEASUREMENT_DATEValue;
        public System.DateTime? MEASUREMENT_DATE
        {
            get { return this.MEASUREMENT_DATEValue; }
            set { SetProperty(ref MEASUREMENT_DATEValue, value); }
        }

        private System.String MUD_TYPEValue = string.Empty;
        /// <summary>WBM (water-based) / OBM (oil-based) / SBM (synthetic-based) / AIR / FOAM.</summary>
        public System.String MUD_TYPE
        {
            get { return this.MUD_TYPEValue; }
            set { SetProperty(ref MUD_TYPEValue, value); }
        }

        private System.Decimal? MUD_WEIGHTValue;
        /// <summary>Mud density (ppg or kg/m³).</summary>
        public System.Decimal? MUD_WEIGHT
        {
            get { return this.MUD_WEIGHTValue; }
            set { SetProperty(ref MUD_WEIGHTValue, value); }
        }

        private System.String MUD_WEIGHT_OUOMValue = string.Empty;
        public System.String MUD_WEIGHT_OUOM
        {
            get { return this.MUD_WEIGHT_OUOMValue; }
            set { SetProperty(ref MUD_WEIGHT_OUOMValue, value); }
        }

        private System.Decimal? VISCOSITYValue;
        /// <summary>Funnel viscosity (seconds/quart per API RP 13B).</summary>
        public System.Decimal? VISCOSITY
        {
            get { return this.VISCOSITYValue; }
            set { SetProperty(ref VISCOSITYValue, value); }
        }

        private System.Decimal? PLASTIC_VISCOSITYValue;
        /// <summary>Plastic viscosity (cP) from Fann viscometer.</summary>
        public System.Decimal? PLASTIC_VISCOSITY
        {
            get { return this.PLASTIC_VISCOSITYValue; }
            set { SetProperty(ref PLASTIC_VISCOSITYValue, value); }
        }

        private System.Decimal? YIELD_POINTValue;
        /// <summary>Yield point (lb/100 sq ft) — measure of gel strength.</summary>
        public System.Decimal? YIELD_POINT
        {
            get { return this.YIELD_POINTValue; }
            set { SetProperty(ref YIELD_POINTValue, value); }
        }

        private System.Decimal? GEL_10SEC_STRENGTHValue;
        public System.Decimal? GEL_10SEC_STRENGTH
        {
            get { return this.GEL_10SEC_STRENGTHValue; }
            set { SetProperty(ref GEL_10SEC_STRENGTHValue, value); }
        }

        private System.Decimal? GEL_10MIN_STRENGTHValue;
        public System.Decimal? GEL_10MIN_STRENGTH
        {
            get { return this.GEL_10MIN_STRENGTHValue; }
            set { SetProperty(ref GEL_10MIN_STRENGTHValue, value); }
        }

        private System.Decimal? FLUID_LOSSValue;
        /// <summary>API fluid loss (mL/30 min).</summary>
        public System.Decimal? FLUID_LOSS
        {
            get { return this.FLUID_LOSSValue; }
            set { SetProperty(ref FLUID_LOSSValue, value); }
        }

        private System.Decimal? PHValue;
        public System.Decimal? PH
        {
            get { return this.PHValue; }
            set { SetProperty(ref PHValue, value); }
        }

        private System.Decimal? CHLORIDE_CONCENTRATIONValue;
        /// <summary>Chloride concentration (ppm) — indicator of formation water influx.</summary>
        public System.Decimal? CHLORIDE_CONCENTRATION
        {
            get { return this.CHLORIDE_CONCENTRATIONValue; }
            set { SetProperty(ref CHLORIDE_CONCENTRATIONValue, value); }
        }

        private System.Decimal? TOTAL_ACTIVE_VOLUMEValue;
        /// <summary>Total active mud volume in system (bbl or m³).</summary>
        public System.Decimal? TOTAL_ACTIVE_VOLUME
        {
            get { return this.TOTAL_ACTIVE_VOLUMEValue; }
            set { SetProperty(ref TOTAL_ACTIVE_VOLUMEValue, value); }
        }

        public DRILLING_FLUID() { }
    }
}
