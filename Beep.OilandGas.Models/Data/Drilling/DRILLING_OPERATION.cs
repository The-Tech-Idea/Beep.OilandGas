using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class DRILLING_OPERATION : Entity, IPPDMEntity
    {
        private String DRILLING_OPERATION_IDValue;
        public String DRILLING_OPERATION_ID
        {
            get { return this.DRILLING_OPERATION_IDValue; }
            set { SetProperty(ref DRILLING_OPERATION_IDValue, value); }
        }

        private String WELL_UWIValue;
        public String WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private String WELL_NAMEValue;
        public String WELL_NAME
        {
            get { return this.WELL_NAMEValue; }
            set { SetProperty(ref WELL_NAMEValue, value); }
        }

        private DateTime? SPUD_DATEValue;
        public DateTime? SPUD_DATE
        {
            get { return this.SPUD_DATEValue; }
            set { SetProperty(ref SPUD_DATEValue, value); }
        }

        private DateTime? COMPLETION_DATEValue;
        public DateTime? COMPLETION_DATE
        {
            get { return this.COMPLETION_DATEValue; }
            set { SetProperty(ref COMPLETION_DATEValue, value); }
        }

        private String STATUSValue;
        public String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private Decimal? CURRENT_DEPTHValue;
        public Decimal? CURRENT_DEPTH
        {
            get { return this.CURRENT_DEPTHValue; }
            set { SetProperty(ref CURRENT_DEPTHValue, value); }
        }

        private Decimal? TARGET_DEPTHValue;
        public Decimal? TARGET_DEPTH
        {
            get { return this.TARGET_DEPTHValue; }
            set { SetProperty(ref TARGET_DEPTHValue, value); }
        }

        private String DRILLING_CONTRACTORValue;
        public String DRILLING_CONTRACTOR
        {
            get { return this.DRILLING_CONTRACTORValue; }
            set { SetProperty(ref DRILLING_CONTRACTORValue, value); }
        }

        private String RIG_NAMEValue;
        public String RIG_NAME
        {
            get { return this.RIG_NAMEValue; }
            set { SetProperty(ref RIG_NAMEValue, value); }
        }

        private Decimal? DAILY_COSTValue;
        public Decimal? DAILY_COST
        {
            get { return this.DAILY_COSTValue; }
            set { SetProperty(ref DAILY_COSTValue, value); }
        }

        private Decimal? TOTAL_COSTValue;
        public Decimal? TOTAL_COST
        {
            get { return this.TOTAL_COSTValue; }
            set { SetProperty(ref TOTAL_COSTValue, value); }
        }

        private String CURRENCYValue;
        public String CURRENCY
        {
            get { return this.CURRENCYValue; }
            set { SetProperty(ref CURRENCYValue, value); }
        }

        // Standard PPDM columns
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        public DRILLING_OPERATION() { }
    }
}




