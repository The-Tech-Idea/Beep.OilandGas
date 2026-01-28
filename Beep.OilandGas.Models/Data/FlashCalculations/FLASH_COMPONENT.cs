using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.FlashCalculations
{
    public partial class FLASH_COMPONENT : ModelEntityBase {
        private String FLASH_COMPONENT_IDValue;
        public String FLASH_COMPONENT_ID
        {
            get { return this.FLASH_COMPONENT_IDValue; }
            set { SetProperty(ref FLASH_COMPONENT_IDValue, value); }
        }

        private String FLASH_CALCULATION_IDValue;
        public String FLASH_CALCULATION_ID
        {
            get { return this.FLASH_CALCULATION_IDValue; }
            set { SetProperty(ref FLASH_CALCULATION_IDValue, value); }
        }

        private String COMPONENT_NAMEValue;
        public String COMPONENT_NAME
        {
            get { return this.COMPONENT_NAMEValue; }
            set { SetProperty(ref COMPONENT_NAMEValue, value); }
        }

        private Decimal? MOLE_FRACTIONValue;
        public Decimal? MOLE_FRACTION
        {
            get { return this.MOLE_FRACTIONValue; }
            set { SetProperty(ref MOLE_FRACTIONValue, value); }
        }

        private Decimal? CRITICAL_TEMPERATUREValue;
        public Decimal? CRITICAL_TEMPERATURE
        {
            get { return this.CRITICAL_TEMPERATUREValue; }
            set { SetProperty(ref CRITICAL_TEMPERATUREValue, value); }
        }

        private Decimal? CRITICAL_PRESSUREValue;
        public Decimal? CRITICAL_PRESSURE
        {
            get { return this.CRITICAL_PRESSUREValue; }
            set { SetProperty(ref CRITICAL_PRESSUREValue, value); }
        }

        private Decimal? ACENTRIC_FACTORValue;
        public Decimal? ACENTRIC_FACTOR
        {
            get { return this.ACENTRIC_FACTORValue; }
            set { SetProperty(ref ACENTRIC_FACTORValue, value); }
        }

        private Decimal? MOLECULAR_WEIGHTValue;
        public Decimal? MOLECULAR_WEIGHT
        {
            get { return this.MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref MOLECULAR_WEIGHTValue, value); }
        }

        // Standard PPDM columns

    }
}
