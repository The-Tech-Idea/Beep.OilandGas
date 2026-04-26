using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// Development well schedule — planned well delivery slot per FDP.
    /// Links FDP → planned well ID → AFE number → planned spud date.
    /// Per SPE PRMS 2018 Section 2.1.2 (Developed Reserves well-by-well assignment).
    /// </summary>
    public partial class DEVELOPMENT_WELL_SCHEDULE : ModelEntityBase
    {
        private System.String SCHEDULE_IDValue = string.Empty;
        public System.String SCHEDULE_ID
        {
            get { return this.SCHEDULE_IDValue; }
            set { SetProperty(ref SCHEDULE_IDValue, value); }
        }

        private System.String FDP_IDValue = string.Empty;
        /// <summary>FK to FIELD_DEVELOPMENT_PLAN.</summary>
        public System.String FDP_ID
        {
            get { return this.FDP_IDValue; }
            set { SetProperty(ref FDP_IDValue, value); }
        }

        private System.String FIELD_IDValue = string.Empty;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String PLANNED_WELL_IDValue = string.Empty;
        /// <summary>UWI or planned well identifier (pre-spud may be provisional).</summary>
        public System.String PLANNED_WELL_ID
        {
            get { return this.PLANNED_WELL_IDValue; }
            set { SetProperty(ref PLANNED_WELL_IDValue, value); }
        }

        private System.String WELL_NAMEValue = string.Empty;
        public System.String WELL_NAME
        {
            get { return this.WELL_NAMEValue; }
            set { SetProperty(ref WELL_NAMEValue, value); }
        }

        private System.String WELL_TYPEValue = string.Empty;
        /// <summary>PRODUCER / INJECTOR / OBSERVATION / DISPOSAL.</summary>
        public System.String WELL_TYPE
        {
            get { return this.WELL_TYPEValue; }
            set { SetProperty(ref WELL_TYPEValue, value); }
        }

        private System.String TARGET_RESERVOIRValue = string.Empty;
        public System.String TARGET_RESERVOIR
        {
            get { return this.TARGET_RESERVOIRValue; }
            set { SetProperty(ref TARGET_RESERVOIRValue, value); }
        }

        private System.String AFE_NUMBERValue = string.Empty;
        public System.String AFE_NUMBER
        {
            get { return this.AFE_NUMBERValue; }
            set { SetProperty(ref AFE_NUMBERValue, value); }
        }

        private System.Decimal? AFE_COST_MMValue;
        public System.Decimal? AFE_COST_MM
        {
            get { return this.AFE_COST_MMValue; }
            set { SetProperty(ref AFE_COST_MMValue, value); }
        }

        private System.String AFE_CURRENCYValue = string.Empty;
        public System.String AFE_CURRENCY
        {
            get { return this.AFE_CURRENCYValue; }
            set { SetProperty(ref AFE_CURRENCYValue, value); }
        }

        private System.DateTime? PLANNED_SPUD_DATEValue;
        public System.DateTime? PLANNED_SPUD_DATE
        {
            get { return this.PLANNED_SPUD_DATEValue; }
            set { SetProperty(ref PLANNED_SPUD_DATEValue, value); }
        }

        private System.DateTime? PLANNED_COMPLETION_DATEValue;
        public System.DateTime? PLANNED_COMPLETION_DATE
        {
            get { return this.PLANNED_COMPLETION_DATEValue; }
            set { SetProperty(ref PLANNED_COMPLETION_DATEValue, value); }
        }

        private System.String SCHEDULE_STATUSValue = string.Empty;
        /// <summary>PLANNED / APPROVED / SPUDDED / COMPLETED / DEFERRED / CANCELLED.</summary>
        public System.String SCHEDULE_STATUS
        {
            get { return this.SCHEDULE_STATUSValue; }
            set { SetProperty(ref SCHEDULE_STATUSValue, value); }
        }

        private System.Decimal? INITIAL_RATE_OIL_BOPDValue;
        /// <summary>Initial oil rate estimate from DCA/simulation (bopd).</summary>
        public System.Decimal? INITIAL_RATE_OIL_BOPD
        {
            get { return this.INITIAL_RATE_OIL_BOPDValue; }
            set { SetProperty(ref INITIAL_RATE_OIL_BOPDValue, value); }
        }

        private System.Decimal? INITIAL_RATE_GAS_MMSCFDValue;
        public System.Decimal? INITIAL_RATE_GAS_MMSCFD
        {
            get { return this.INITIAL_RATE_GAS_MMSCFDValue; }
            set { SetProperty(ref INITIAL_RATE_GAS_MMSCFDValue, value); }
        }

        public DEVELOPMENT_WELL_SCHEDULE() { }
    }
}
