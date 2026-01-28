using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public partial class SUCKER_ROD_LOAD_RESULT : ModelEntityBase {
        private String SUCKER_ROD_LOAD_RESULT_IDValue;
        public String SUCKER_ROD_LOAD_RESULT_ID
        {
            get { return this.SUCKER_ROD_LOAD_RESULT_IDValue; }
            set { SetProperty(ref SUCKER_ROD_LOAD_RESULT_IDValue, value); }
        }

        private String SUCKER_ROD_SYSTEM_PROPERTIES_IDValue;
        public String SUCKER_ROD_SYSTEM_PROPERTIES_ID
        {
            get { return this.SUCKER_ROD_SYSTEM_PROPERTIES_IDValue; }
            set { SetProperty(ref SUCKER_ROD_SYSTEM_PROPERTIES_IDValue, value); }
        }

        private Decimal? PEAK_LOADValue;
        public Decimal? PEAK_LOAD
        {
            get { return this.PEAK_LOADValue; }
            set { SetProperty(ref PEAK_LOADValue, value); }
        }

        private Decimal? MINIMUM_LOADValue;
        public Decimal? MINIMUM_LOAD
        {
            get { return this.MINIMUM_LOADValue; }
            set { SetProperty(ref MINIMUM_LOADValue, value); }
        }

        private Decimal? POLISHED_ROD_LOADValue;
        public Decimal? POLISHED_ROD_LOAD
        {
            get { return this.POLISHED_ROD_LOADValue; }
            set { SetProperty(ref POLISHED_ROD_LOADValue, value); }
        }

        private Decimal? ROD_STRING_WEIGHTValue;
        public Decimal? ROD_STRING_WEIGHT
        {
            get { return this.ROD_STRING_WEIGHTValue; }
            set { SetProperty(ref ROD_STRING_WEIGHTValue, value); }
        }

        private Decimal? FLUID_LOADValue;
        public Decimal? FLUID_LOAD
        {
            get { return this.FLUID_LOADValue; }
            set { SetProperty(ref FLUID_LOADValue, value); }
        }

        private Decimal? DYNAMIC_LOADValue;
        public Decimal? DYNAMIC_LOAD
        {
            get { return this.DYNAMIC_LOADValue; }
            set { SetProperty(ref DYNAMIC_LOADValue, value); }
        }

        private Decimal? LOAD_RANGEValue;
        public Decimal? LOAD_RANGE
        {
            get { return this.LOAD_RANGEValue; }
            set { SetProperty(ref LOAD_RANGEValue, value); }
        }

        private Decimal? STRESS_RANGEValue;
        public Decimal? STRESS_RANGE
        {
            get { return this.STRESS_RANGEValue; }
            set { SetProperty(ref STRESS_RANGEValue, value); }
        }

        private Decimal? MAXIMUM_STRESSValue;
        public Decimal? MAXIMUM_STRESS
        {
            get { return this.MAXIMUM_STRESSValue; }
            set { SetProperty(ref MAXIMUM_STRESSValue, value); }
        }

        private Decimal? LOAD_FACTORValue;
        public Decimal? LOAD_FACTOR
        {
            get { return this.LOAD_FACTORValue; }
            set { SetProperty(ref LOAD_FACTORValue, value); }
        }

        // Standard PPDM columns

    }
}
