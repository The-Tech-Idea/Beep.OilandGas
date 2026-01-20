using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public partial class PLUNGER_LIFT_GAS_REQUIREMENTS : ModelEntityBase {
        private String PLUNGER_LIFT_GAS_REQUIREMENTS_IDValue;
        public String PLUNGER_LIFT_GAS_REQUIREMENTS_ID
        {
            get { return this.PLUNGER_LIFT_GAS_REQUIREMENTS_IDValue; }
            set { SetProperty(ref PLUNGER_LIFT_GAS_REQUIREMENTS_IDValue, value); }
        }

        private String PLUNGER_LIFT_WELL_PROPERTIES_IDValue;
        public String PLUNGER_LIFT_WELL_PROPERTIES_ID
        {
            get { return this.PLUNGER_LIFT_WELL_PROPERTIES_IDValue; }
            set { SetProperty(ref PLUNGER_LIFT_WELL_PROPERTIES_IDValue, value); }
        }

        private Decimal? REQUIRED_GAS_INJECTION_RATEValue;
        public Decimal? REQUIRED_GAS_INJECTION_RATE
        {
            get { return this.REQUIRED_GAS_INJECTION_RATEValue; }
            set { SetProperty(ref REQUIRED_GAS_INJECTION_RATEValue, value); }
        }

        private Decimal? AVAILABLE_GASValue;
        public Decimal? AVAILABLE_GAS
        {
            get { return this.AVAILABLE_GASValue; }
            set { SetProperty(ref AVAILABLE_GASValue, value); }
        }

        private Decimal? ADDITIONAL_GAS_REQUIREDValue;
        public Decimal? ADDITIONAL_GAS_REQUIRED
        {
            get { return this.ADDITIONAL_GAS_REQUIREDValue; }
            set { SetProperty(ref ADDITIONAL_GAS_REQUIREDValue, value); }
        }

        private Decimal? REQUIRED_GAS_LIQUID_RATIOValue;
        public Decimal? REQUIRED_GAS_LIQUID_RATIO
        {
            get { return this.REQUIRED_GAS_LIQUID_RATIOValue; }
            set { SetProperty(ref REQUIRED_GAS_LIQUID_RATIOValue, value); }
        }

        private Decimal? MINIMUM_CASING_PRESSUREValue;
        public Decimal? MINIMUM_CASING_PRESSURE
        {
            get { return this.MINIMUM_CASING_PRESSUREValue; }
            set { SetProperty(ref MINIMUM_CASING_PRESSUREValue, value); }
        }

        private Decimal? MAXIMUM_CASING_PRESSUREValue;
        public Decimal? MAXIMUM_CASING_PRESSURE
        {
            get { return this.MAXIMUM_CASING_PRESSUREValue; }
            set { SetProperty(ref MAXIMUM_CASING_PRESSUREValue, value); }
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



