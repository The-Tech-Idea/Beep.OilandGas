using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

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

        private decimal REQUIRED_GAS_INJECTION_RATEValue;
        public decimal REQUIRED_GAS_INJECTION_RATE
        {
            get { return this.REQUIRED_GAS_INJECTION_RATEValue; }
            set { SetProperty(ref REQUIRED_GAS_INJECTION_RATEValue, value); }
        }

        private decimal AVAILABLE_GASValue;
        public decimal AVAILABLE_GAS
        {
            get { return this.AVAILABLE_GASValue; }
            set { SetProperty(ref AVAILABLE_GASValue, value); }
        }

        private decimal ADDITIONAL_GAS_REQUIREDValue;
        public decimal ADDITIONAL_GAS_REQUIRED
        {
            get { return this.ADDITIONAL_GAS_REQUIREDValue; }
            set { SetProperty(ref ADDITIONAL_GAS_REQUIREDValue, value); }
        }

        private decimal REQUIRED_GAS_LIQUID_RATIOValue;
        public decimal REQUIRED_GAS_LIQUID_RATIO
        {
            get { return this.REQUIRED_GAS_LIQUID_RATIOValue; }
            set { SetProperty(ref REQUIRED_GAS_LIQUID_RATIOValue, value); }
        }

        private decimal MINIMUM_CASING_PRESSUREValue;
        public decimal MINIMUM_CASING_PRESSURE
        {
            get { return this.MINIMUM_CASING_PRESSUREValue; }
            set { SetProperty(ref MINIMUM_CASING_PRESSUREValue, value); }
        }

        private decimal MAXIMUM_CASING_PRESSUREValue;
        public decimal MAXIMUM_CASING_PRESSURE
        {
            get { return this.MAXIMUM_CASING_PRESSUREValue; }
            set { SetProperty(ref MAXIMUM_CASING_PRESSUREValue, value); }
        }

        // Standard PPDM columns

    }
}
