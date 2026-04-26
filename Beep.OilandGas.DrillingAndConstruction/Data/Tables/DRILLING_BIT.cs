using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// Drilling bit record — performance tracking per bit run.
    /// Per IADC bit-record standards: size, type, footage, hours, IADC code.
    /// </summary>
    public partial class DRILLING_BIT : ModelEntityBase
    {
        private System.String BIT_IDValue = string.Empty;
        public System.String BIT_ID
        {
            get { return this.BIT_IDValue; }
            set { SetProperty(ref BIT_IDValue, value); }
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

        private System.Int32 BIT_RUN_NUMBERValue;
        /// <summary>Sequential bit run number from spud.</summary>
        public System.Int32 BIT_RUN_NUMBER
        {
            get { return this.BIT_RUN_NUMBERValue; }
            set { SetProperty(ref BIT_RUN_NUMBERValue, value); }
        }

        private System.String BIT_MANUFACTURERValue = string.Empty;
        public System.String BIT_MANUFACTURER
        {
            get { return this.BIT_MANUFACTURERValue; }
            set { SetProperty(ref BIT_MANUFACTURERValue, value); }
        }

        private System.String BIT_SERIALValue = string.Empty;
        public System.String BIT_SERIAL
        {
            get { return this.BIT_SERIALValue; }
            set { SetProperty(ref BIT_SERIALValue, value); }
        }

        private System.String IADC_CODEValue = string.Empty;
        /// <summary>IADC bit classification code (e.g. M313 for PDC).</summary>
        public System.String IADC_CODE
        {
            get { return this.IADC_CODEValue; }
            set { SetProperty(ref IADC_CODEValue, value); }
        }

        private System.Decimal? BIT_SIZEValue;
        /// <summary>Bit diameter (inches or mm).</summary>
        public System.Decimal? BIT_SIZE
        {
            get { return this.BIT_SIZEValue; }
            set { SetProperty(ref BIT_SIZEValue, value); }
        }

        private System.String BIT_SIZE_OUOMValue = string.Empty;
        public System.String BIT_SIZE_OUOM
        {
            get { return this.BIT_SIZE_OUOMValue; }
            set { SetProperty(ref BIT_SIZE_OUOMValue, value); }
        }

        private System.Decimal? DEPTH_IN_MDValue;
        /// <summary>Measured depth at which this bit run started.</summary>
        public System.Decimal? DEPTH_IN_MD
        {
            get { return this.DEPTH_IN_MDValue; }
            set { SetProperty(ref DEPTH_IN_MDValue, value); }
        }

        private System.Decimal? DEPTH_OUT_MDValue;
        /// <summary>Measured depth at which this bit run ended.</summary>
        public System.Decimal? DEPTH_OUT_MD
        {
            get { return this.DEPTH_OUT_MDValue; }
            set { SetProperty(ref DEPTH_OUT_MDValue, value); }
        }

        private System.String DEPTH_OUOMValue = string.Empty;
        public System.String DEPTH_OUOM
        {
            get { return this.DEPTH_OUOMValue; }
            set { SetProperty(ref DEPTH_OUOMValue, value); }
        }

        private System.Decimal? FOOTAGE_DRILLEDValue;
        public System.Decimal? FOOTAGE_DRILLED
        {
            get { return this.FOOTAGE_DRILLEDValue; }
            set { SetProperty(ref FOOTAGE_DRILLEDValue, value); }
        }

        private System.Decimal? ROTATING_HOURSValue;
        public System.Decimal? ROTATING_HOURS
        {
            get { return this.ROTATING_HOURSValue; }
            set { SetProperty(ref ROTATING_HOURSValue, value); }
        }

        private System.Decimal? WEIGHT_ON_BITValue;
        /// <summary>Average weight on bit (klbs or kN).</summary>
        public System.Decimal? WEIGHT_ON_BIT
        {
            get { return this.WEIGHT_ON_BITValue; }
            set { SetProperty(ref WEIGHT_ON_BITValue, value); }
        }

        private System.Decimal? ROP_AVGValue;
        /// <summary>Average rate of penetration (ft/hr or m/hr).</summary>
        public System.Decimal? ROP_AVG
        {
            get { return this.ROP_AVGValue; }
            set { SetProperty(ref ROP_AVGValue, value); }
        }

        private System.String PULL_REASON_CODEValue = string.Empty;
        /// <summary>IADC bit-pull reason code (e.g. TD / BHA / Plugged).</summary>
        public System.String PULL_REASON_CODE
        {
            get { return this.PULL_REASON_CODEValue; }
            set { SetProperty(ref PULL_REASON_CODEValue, value); }
        }

        private System.String DULL_GRADEValue = string.Empty;
        /// <summary>IADC dull grading at pull (e.g. 1-2-WT-A-X-I-CT-PR).</summary>
        public System.String DULL_GRADE
        {
            get { return this.DULL_GRADEValue; }
            set { SetProperty(ref DULL_GRADEValue, value); }
        }

        public DRILLING_BIT() { }
    }
}
