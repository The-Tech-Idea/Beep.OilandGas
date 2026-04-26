using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// Cement job record — primary and remedial cementing operations.
    /// Per API RP 10D / ISO 10426 cementing standards.
    /// </summary>
    public partial class CEMENT_JOB : ModelEntityBase
    {
        private System.String CEMENT_JOB_IDValue = string.Empty;
        public System.String CEMENT_JOB_ID
        {
            get { return this.CEMENT_JOB_IDValue; }
            set { SetProperty(ref CEMENT_JOB_IDValue, value); }
        }

        private System.String CASING_IDValue = string.Empty;
        /// <summary>FK to CASING_PROGRAM — the string being cemented.</summary>
        public System.String CASING_ID
        {
            get { return this.CASING_IDValue; }
            set { SetProperty(ref CASING_IDValue, value); }
        }

        private System.String WELL_IDValue = string.Empty;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.String JOB_TYPEValue = string.Empty;
        /// <summary>PRIMARY / SQUEEZE / PLUG_BACK / STAGE / REMEDIAL.</summary>
        public System.String JOB_TYPE
        {
            get { return this.JOB_TYPEValue; }
            set { SetProperty(ref JOB_TYPEValue, value); }
        }

        private System.DateTime? JOB_DATEValue;
        public System.DateTime? JOB_DATE
        {
            get { return this.JOB_DATEValue; }
            set { SetProperty(ref JOB_DATEValue, value); }
        }

        private System.String CEMENT_COMPANY_NAMEValue = string.Empty;
        public System.String CEMENT_COMPANY_NAME
        {
            get { return this.CEMENT_COMPANY_NAMEValue; }
            set { SetProperty(ref CEMENT_COMPANY_NAMEValue, value); }
        }

        private System.Decimal? SLURRY_VOLUME_MIXEDValue;
        /// <summary>Total slurry volume mixed (bbl or m³).</summary>
        public System.Decimal? SLURRY_VOLUME_MIXED
        {
            get { return this.SLURRY_VOLUME_MIXEDValue; }
            set { SetProperty(ref SLURRY_VOLUME_MIXEDValue, value); }
        }

        private System.Decimal? SLURRY_WEIGHTValue;
        /// <summary>Slurry density (ppg or kg/m³).</summary>
        public System.Decimal? SLURRY_WEIGHT
        {
            get { return this.SLURRY_WEIGHTValue; }
            set { SetProperty(ref SLURRY_WEIGHTValue, value); }
        }

        private System.String SLURRY_WEIGHT_OUOMValue = string.Empty;
        public System.String SLURRY_WEIGHT_OUOM
        {
            get { return this.SLURRY_WEIGHT_OUOMValue; }
            set { SetProperty(ref SLURRY_WEIGHT_OUOMValue, value); }
        }

        private System.Decimal? TOP_OF_CEMENT_MDValue;
        /// <summary>Measured depth of top-of-cement (TOC).</summary>
        public System.Decimal? TOP_OF_CEMENT_MD
        {
            get { return this.TOP_OF_CEMENT_MDValue; }
            set { SetProperty(ref TOP_OF_CEMENT_MDValue, value); }
        }

        private System.String DEPTH_OUOMValue = string.Empty;
        public System.String DEPTH_OUOM
        {
            get { return this.DEPTH_OUOMValue; }
            set { SetProperty(ref DEPTH_OUOMValue, value); }
        }

        private System.Decimal? MAX_PUMP_PRESSUREValue;
        /// <summary>Maximum surface pump pressure during job (psi or kPa).</summary>
        public System.Decimal? MAX_PUMP_PRESSURE
        {
            get { return this.MAX_PUMP_PRESSUREValue; }
            set { SetProperty(ref MAX_PUMP_PRESSUREValue, value); }
        }

        private System.String SQUEEZE_RESULT_CODEValue = string.Empty;
        /// <summary>Job outcome: SUCCESS / PARTIAL / FAILED / REMEDIAL_REQUIRED.</summary>
        public System.String SQUEEZE_RESULT_CODE
        {
            get { return this.SQUEEZE_RESULT_CODEValue; }
            set { SetProperty(ref SQUEEZE_RESULT_CODEValue, value); }
        }

        private System.String CBL_LOG_INDValue = string.Empty;
        /// <summary>Y/N — cement bond log run to verify cement quality.</summary>
        public System.String CBL_LOG_IND
        {
            get { return this.CBL_LOG_INDValue; }
            set { SetProperty(ref CBL_LOG_INDValue, value); }
        }

        private System.Decimal? CBL_QUALITY_SCOREValue;
        /// <summary>Cement bond quality score (0-100%) from CBL interpretation.</summary>
        public System.Decimal? CBL_QUALITY_SCORE
        {
            get { return this.CBL_QUALITY_SCOREValue; }
            set { SetProperty(ref CBL_QUALITY_SCOREValue, value); }
        }

        public CEMENT_JOB() { }
    }
}
