using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DrillingAndConstruction
{
    /// <summary>
    /// Drilling activity record — fine-grained IADC time breakdown within a DDR.
    /// Each row is one activity interval: start time, end time, activity code.
    /// Per IADC NPT/drilling-time classification codes.
    /// </summary>
    public partial class DRILLING_ACTIVITY : ModelEntityBase
    {
        private System.String ACTIVITY_IDValue = string.Empty;
        public System.String ACTIVITY_ID
        {
            get { return this.ACTIVITY_IDValue; }
            set { SetProperty(ref ACTIVITY_IDValue, value); }
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

        private System.DateTime? START_TIMEValue;
        public System.DateTime? START_TIME
        {
            get { return this.START_TIMEValue; }
            set { SetProperty(ref START_TIMEValue, value); }
        }

        private System.DateTime? END_TIMEValue;
        public System.DateTime? END_TIME
        {
            get { return this.END_TIMEValue; }
            set { SetProperty(ref END_TIMEValue, value); }
        }

        private System.Decimal? DURATION_HOURSValue;
        public System.Decimal? DURATION_HOURS
        {
            get { return this.DURATION_HOURSValue; }
            set { SetProperty(ref DURATION_HOURSValue, value); }
        }

        private System.String ACTIVITY_CODEValue = string.Empty;
        /// <summary>
        /// IADC activity code group (e.g. DRILL/TRIP/CASE/CEMENT/TEST/NPT/WAIT).
        /// Maps to IADC time-classification taxonomy.
        /// </summary>
        public System.String ACTIVITY_CODE
        {
            get { return this.ACTIVITY_CODEValue; }
            set { SetProperty(ref ACTIVITY_CODEValue, value); }
        }

        private System.String NPT_CODE_IADCValue = string.Empty;
        /// <summary>
        /// IADC NPT reason code (populated when ACTIVITY_CODE = NPT).
        /// E.g. WX (weather), STUCK_PIPE, BHA_FAILURE, WELLBORE_INSTABILITY.
        /// </summary>
        public System.String NPT_CODE_IADC
        {
            get { return this.NPT_CODE_IADCValue; }
            set { SetProperty(ref NPT_CODE_IADCValue, value); }
        }

        private System.String NPT_INDValue = string.Empty;
        /// <summary>Y = Non-Productive Time; N = Productive Time.</summary>
        public System.String NPT_IND
        {
            get { return this.NPT_INDValue; }
            set { SetProperty(ref NPT_INDValue, value); }
        }

        private System.Decimal? DEPTH_AT_START_MDValue;
        public System.Decimal? DEPTH_AT_START_MD
        {
            get { return this.DEPTH_AT_START_MDValue; }
            set { SetProperty(ref DEPTH_AT_START_MDValue, value); }
        }

        private System.String DEPTH_OUOMValue = string.Empty;
        public System.String DEPTH_OUOM
        {
            get { return this.DEPTH_OUOMValue; }
            set { SetProperty(ref DEPTH_OUOMValue, value); }
        }

        private System.String ACTIVITY_DESCRIPTIONValue = string.Empty;
        public System.String ACTIVITY_DESCRIPTION
        {
            get { return this.ACTIVITY_DESCRIPTIONValue; }
            set { SetProperty(ref ACTIVITY_DESCRIPTIONValue, value); }
        }

        public DRILLING_ACTIVITY() { }
    }
}
