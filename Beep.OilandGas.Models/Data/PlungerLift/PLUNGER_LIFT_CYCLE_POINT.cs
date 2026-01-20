using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public partial class PLUNGER_LIFT_CYCLE_POINT : ModelEntityBase {
        private String PLUNGER_LIFT_CYCLE_POINT_IDValue;
        public String PLUNGER_LIFT_CYCLE_POINT_ID
        {
            get { return this.PLUNGER_LIFT_CYCLE_POINT_IDValue; }
            set { SetProperty(ref PLUNGER_LIFT_CYCLE_POINT_IDValue, value); }
        }

        private String PLUNGER_LIFT_CYCLE_RESULT_IDValue;
        public String PLUNGER_LIFT_CYCLE_RESULT_ID
        {
            get { return this.PLUNGER_LIFT_CYCLE_RESULT_IDValue; }
            set { SetProperty(ref PLUNGER_LIFT_CYCLE_RESULT_IDValue, value); }
        }

        private Decimal? TIMEValue;
        public Decimal? TIME
        {
            get { return this.TIMEValue; }
            set { SetProperty(ref TIMEValue, value); }
        }

        private String PHASEValue;
        public String PHASE
        {
            get { return this.PHASEValue; }
            set { SetProperty(ref PHASEValue, value); }
        }

        private Decimal? PLUNGER_DEPTHValue;
        public Decimal? PLUNGER_DEPTH
        {
            get { return this.PLUNGER_DEPTHValue; }
            set { SetProperty(ref PLUNGER_DEPTHValue, value); }
        }

        private Decimal? CASING_PRESSUREValue;
        public Decimal? CASING_PRESSURE
        {
            get { return this.CASING_PRESSUREValue; }
            set { SetProperty(ref CASING_PRESSUREValue, value); }
        }

        private Decimal? TUBING_PRESSUREValue;
        public Decimal? TUBING_PRESSURE
        {
            get { return this.TUBING_PRESSUREValue; }
            set { SetProperty(ref TUBING_PRESSUREValue, value); }
        }

        private Decimal? PRODUCTION_RATEValue;
        public Decimal? PRODUCTION_RATE
        {
            get { return this.PRODUCTION_RATEValue; }
            set { SetProperty(ref PRODUCTION_RATEValue, value); }
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
    }
}



