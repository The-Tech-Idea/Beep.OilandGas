using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_VALVE : Entity, IPPDMEntity
    {
        private String GAS_LIFT_VALVE_IDValue;
        public String GAS_LIFT_VALVE_ID
        {
            get { return this.GAS_LIFT_VALVE_IDValue; }
            set { SetProperty(ref GAS_LIFT_VALVE_IDValue, value); }
        }

        private String GAS_LIFT_DESIGN_IDValue;
        public String GAS_LIFT_DESIGN_ID
        {
            get { return this.GAS_LIFT_DESIGN_IDValue; }
            set { SetProperty(ref GAS_LIFT_DESIGN_IDValue, value); }
        }

        private Decimal? DEPTHValue;
        public Decimal? DEPTH
        {
            get { return this.DEPTHValue; }
            set { SetProperty(ref DEPTHValue, value); }
        }

        private Decimal? PORT_SIZEValue;
        public Decimal? PORT_SIZE
        {
            get { return this.PORT_SIZEValue; }
            set { SetProperty(ref PORT_SIZEValue, value); }
        }

        private Decimal? OPENING_PRESSUREValue;
        public Decimal? OPENING_PRESSURE
        {
            get { return this.OPENING_PRESSUREValue; }
            set { SetProperty(ref OPENING_PRESSUREValue, value); }
        }

        private Decimal? CLOSING_PRESSUREValue;
        public Decimal? CLOSING_PRESSURE
        {
            get { return this.CLOSING_PRESSUREValue; }
            set { SetProperty(ref CLOSING_PRESSUREValue, value); }
        }

        private String VALVE_TYPEValue;
        public String VALVE_TYPE
        {
            get { return this.VALVE_TYPEValue; }
            set { SetProperty(ref VALVE_TYPEValue, value); }
        }

        private Decimal? TEMPERATUREValue;
        public Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private Decimal? GAS_INJECTION_RATEValue;
        public Decimal? GAS_INJECTION_RATE
        {
            get { return this.GAS_INJECTION_RATEValue; }
            set { SetProperty(ref GAS_INJECTION_RATEValue, value); }
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



