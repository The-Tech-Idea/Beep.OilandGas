using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// Daily Drilling Report (DDR) — captures 24-hour operational summary.
    /// Per IADC DDR standards and PPDM 3.9 drilling activity reporting.
    /// </summary>
    public partial class DRILLING_DAILY_REPORT : ModelEntityBase
    {
        private System.String DDR_IDValue = string.Empty;
        public System.String DDR_ID
        {
            get { return this.DDR_IDValue; }
            set { SetProperty(ref DDR_IDValue, value); }
        }

        private System.String PROGRAM_IDValue = string.Empty;
        /// <summary>FK to DRILLING_PROGRAM.</summary>
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

        private System.DateTime? REPORT_DATEValue;
        /// <summary>Date this DDR covers (midnight to midnight).</summary>
        public System.DateTime? REPORT_DATE
        {
            get { return this.REPORT_DATEValue; }
            set { SetProperty(ref REPORT_DATEValue, value); }
        }

        private System.Int32 REPORT_NUMBERValue;
        /// <summary>Sequential DDR number from spud.</summary>
        public System.Int32 REPORT_NUMBER
        {
            get { return this.REPORT_NUMBERValue; }
            set { SetProperty(ref REPORT_NUMBERValue, value); }
        }

        private System.Decimal? DEPTH_START_MDValue;
        /// <summary>Measured depth at start of reporting period.</summary>
        public System.Decimal? DEPTH_START_MD
        {
            get { return this.DEPTH_START_MDValue; }
            set { SetProperty(ref DEPTH_START_MDValue, value); }
        }

        private System.Decimal? DEPTH_END_MDValue;
        /// <summary>Measured depth at end of reporting period (current bit depth).</summary>
        public System.Decimal? DEPTH_END_MD
        {
            get { return this.DEPTH_END_MDValue; }
            set { SetProperty(ref DEPTH_END_MDValue, value); }
        }

        private System.String DEPTH_OUOMValue = string.Empty;
        public System.String DEPTH_OUOM
        {
            get { return this.DEPTH_OUOMValue; }
            set { SetProperty(ref DEPTH_OUOMValue, value); }
        }

        private System.Decimal? FOOTAGE_DRILLEDValue;
        /// <summary>Footage drilled in this 24-hour period.</summary>
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

        private System.Decimal? NPT_HOURSValue;
        /// <summary>Non-Productive Time hours in this period.</summary>
        public System.Decimal? NPT_HOURS
        {
            get { return this.NPT_HOURSValue; }
            set { SetProperty(ref NPT_HOURSValue, value); }
        }

        private System.String NPT_REASONValue = string.Empty;
        public System.String NPT_REASON
        {
            get { return this.NPT_REASONValue; }
            set { SetProperty(ref NPT_REASONValue, value); }
        }

        private System.Decimal? DAILY_COSTValue;
        public System.Decimal? DAILY_COST
        {
            get { return this.DAILY_COSTValue; }
            set { SetProperty(ref DAILY_COSTValue, value); }
        }

        private System.Decimal? CUMULATIVE_COSTValue;
        public System.Decimal? CUMULATIVE_COST
        {
            get { return this.CUMULATIVE_COSTValue; }
            set { SetProperty(ref CUMULATIVE_COSTValue, value); }
        }

        private System.String COST_CURRENCYValue = string.Empty;
        public System.String COST_CURRENCY
        {
            get { return this.COST_CURRENCYValue; }
            set { SetProperty(ref COST_CURRENCYValue, value); }
        }

        private System.String DAILY_OPERATIONS_SUMMARYValue = string.Empty;
        public System.String DAILY_OPERATIONS_SUMMARY
        {
            get { return this.DAILY_OPERATIONS_SUMMARYValue; }
            set { SetProperty(ref DAILY_OPERATIONS_SUMMARYValue, value); }
        }

        private System.String NEXT_24HR_PLANValue = string.Empty;
        public System.String NEXT_24HR_PLAN
        {
            get { return this.NEXT_24HR_PLANValue; }
            set { SetProperty(ref NEXT_24HR_PLANValue, value); }
        }

        private System.String DRILLER_NAMEValue = string.Empty;
        public System.String DRILLER_NAME
        {
            get { return this.DRILLER_NAMEValue; }
            set { SetProperty(ref DRILLER_NAMEValue, value); }
        }

        private System.String TOOL_PUSHER_NAMEValue = string.Empty;
        public System.String TOOL_PUSHER_NAME
        {
            get { return this.TOOL_PUSHER_NAMEValue; }
            set { SetProperty(ref TOOL_PUSHER_NAMEValue, value); }
        }

        private System.String REPORT_STATUSValue = string.Empty;
        /// <summary>DRAFT / SUBMITTED / APPROVED.</summary>
        public System.String REPORT_STATUS
        {
            get { return this.REPORT_STATUSValue; }
            set { SetProperty(ref REPORT_STATUSValue, value); }
        }

        public DRILLING_DAILY_REPORT() { }
    }
}
