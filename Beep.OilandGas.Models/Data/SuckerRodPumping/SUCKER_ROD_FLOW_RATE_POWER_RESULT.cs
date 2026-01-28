using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    public partial class SUCKER_ROD_FLOW_RATE_POWER_RESULT : ModelEntityBase {
        private String SUCKER_ROD_FLOW_RATE_POWER_RESULT_IDValue;
        public String SUCKER_ROD_FLOW_RATE_POWER_RESULT_ID
        {
            get { return this.SUCKER_ROD_FLOW_RATE_POWER_RESULT_IDValue; }
            set { SetProperty(ref SUCKER_ROD_FLOW_RATE_POWER_RESULT_IDValue, value); }
        }

        private String SUCKER_ROD_SYSTEM_PROPERTIES_IDValue;
        public String SUCKER_ROD_SYSTEM_PROPERTIES_ID
        {
            get { return this.SUCKER_ROD_SYSTEM_PROPERTIES_IDValue; }
            set { SetProperty(ref SUCKER_ROD_SYSTEM_PROPERTIES_IDValue, value); }
        }

        private Decimal? PRODUCTION_RATEValue;
        public Decimal? PRODUCTION_RATE
        {
            get { return this.PRODUCTION_RATEValue; }
            set { SetProperty(ref PRODUCTION_RATEValue, value); }
        }

        private Decimal? PUMP_DISPLACEMENTValue;
        public Decimal? PUMP_DISPLACEMENT
        {
            get { return this.PUMP_DISPLACEMENTValue; }
            set { SetProperty(ref PUMP_DISPLACEMENTValue, value); }
        }

        private Decimal? VOLUMETRIC_EFFICIENCYValue;
        public Decimal? VOLUMETRIC_EFFICIENCY
        {
            get { return this.VOLUMETRIC_EFFICIENCYValue; }
            set { SetProperty(ref VOLUMETRIC_EFFICIENCYValue, value); }
        }

        private Decimal? POLISHED_ROD_HORSEPOWERValue;
        public Decimal? POLISHED_ROD_HORSEPOWER
        {
            get { return this.POLISHED_ROD_HORSEPOWERValue; }
            set { SetProperty(ref POLISHED_ROD_HORSEPOWERValue, value); }
        }

        private Decimal? HYDRAULIC_HORSEPOWERValue;
        public Decimal? HYDRAULIC_HORSEPOWER
        {
            get { return this.HYDRAULIC_HORSEPOWERValue; }
            set { SetProperty(ref HYDRAULIC_HORSEPOWERValue, value); }
        }

        private Decimal? FRICTION_HORSEPOWERValue;
        public Decimal? FRICTION_HORSEPOWER
        {
            get { return this.FRICTION_HORSEPOWERValue; }
            set { SetProperty(ref FRICTION_HORSEPOWERValue, value); }
        }

        private Decimal? TOTAL_HORSEPOWERValue;
        public Decimal? TOTAL_HORSEPOWER
        {
            get { return this.TOTAL_HORSEPOWERValue; }
            set { SetProperty(ref TOTAL_HORSEPOWERValue, value); }
        }

        private Decimal? MOTOR_HORSEPOWERValue;
        public Decimal? MOTOR_HORSEPOWER
        {
            get { return this.MOTOR_HORSEPOWERValue; }
            set { SetProperty(ref MOTOR_HORSEPOWERValue, value); }
        }

        private Decimal? SYSTEM_EFFICIENCYValue;
        public Decimal? SYSTEM_EFFICIENCY
        {
            get { return this.SYSTEM_EFFICIENCYValue; }
            set { SetProperty(ref SYSTEM_EFFICIENCYValue, value); }
        }

        private Decimal? ENERGY_CONSUMPTIONValue;
        public Decimal? ENERGY_CONSUMPTION
        {
            get { return this.ENERGY_CONSUMPTIONValue; }
            set { SetProperty(ref ENERGY_CONSUMPTIONValue, value); }
        }

        // Standard PPDM columns

    }
}
