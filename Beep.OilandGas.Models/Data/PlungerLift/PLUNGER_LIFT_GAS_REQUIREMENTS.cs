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

    }
}


