using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Drilling
{
    public partial class DRILLING_OPERATION : ModelEntityBase
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

        public DRILLING_OPERATION() { }

        private string OPERATION_IDValue;
        public string OPERATION_ID
        {
            get { return this.OPERATION_IDValue; }
            set { SetProperty(ref OPERATION_IDValue, value); }
        }

        public object Reports { get; set; }
    }
}
