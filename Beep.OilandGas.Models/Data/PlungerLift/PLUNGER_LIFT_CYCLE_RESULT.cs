using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public partial class PLUNGER_LIFT_CYCLE_RESULT : ModelEntityBase {
        private String PLUNGER_LIFT_CYCLE_RESULT_IDValue;
        public String PLUNGER_LIFT_CYCLE_RESULT_ID
        {
            get { return this.PLUNGER_LIFT_CYCLE_RESULT_IDValue; }
            set { SetProperty(ref PLUNGER_LIFT_CYCLE_RESULT_IDValue, value); }
        }

        private String PLUNGER_LIFT_WELL_PROPERTIES_IDValue;
        public String PLUNGER_LIFT_WELL_PROPERTIES_ID
        {
            get { return this.PLUNGER_LIFT_WELL_PROPERTIES_IDValue; }
            set { SetProperty(ref PLUNGER_LIFT_WELL_PROPERTIES_IDValue, value); }
        }

        private decimal  CYCLE_TIMEValue;
        public decimal  CYCLE_TIME
        {
            get { return this.CYCLE_TIMEValue; }
            set { SetProperty(ref CYCLE_TIMEValue, value); }
        }

        private decimal  FALL_TIMEValue;
        public decimal  FALL_TIME
        {
            get { return this.FALL_TIMEValue; }
            set { SetProperty(ref FALL_TIMEValue, value); }
        }

        private decimal  RISE_TIMEValue;
        public decimal  RISE_TIME
        {
            get { return this.RISE_TIMEValue; }
            set { SetProperty(ref RISE_TIMEValue, value); }
        }

        private decimal  SHUT_IN_TIMEValue;
        public decimal  SHUT_IN_TIME
        {
            get { return this.SHUT_IN_TIMEValue; }
            set { SetProperty(ref SHUT_IN_TIMEValue, value); }
        }

        private decimal  FALL_VELOCITYValue;
        public decimal  FALL_VELOCITY
        {
            get { return this.FALL_VELOCITYValue; }
            set { SetProperty(ref FALL_VELOCITYValue, value); }
        }

        private decimal  RISE_VELOCITYValue;
        public decimal  RISE_VELOCITY
        {
            get { return this.RISE_VELOCITYValue; }
            set { SetProperty(ref RISE_VELOCITYValue, value); }
        }

        private decimal  LIQUID_SLUG_SIZEValue;
        public decimal  LIQUID_SLUG_SIZE
        {
            get { return this.LIQUID_SLUG_SIZEValue; }
            set { SetProperty(ref LIQUID_SLUG_SIZEValue, value); }
        }

        private decimal  PRODUCTION_PER_CYCLEValue;
        public decimal  PRODUCTION_PER_CYCLE
        {
            get { return this.PRODUCTION_PER_CYCLEValue; }
            set { SetProperty(ref PRODUCTION_PER_CYCLEValue, value); }
        }

        private decimal  DAILY_PRODUCTION_RATEValue;
        public decimal  DAILY_PRODUCTION_RATE
        {
            get { return this.DAILY_PRODUCTION_RATEValue; }
            set { SetProperty(ref DAILY_PRODUCTION_RATEValue, value); }
        }

        private decimal  CYCLES_PER_DAYValue;
        public decimal  CYCLES_PER_DAY
        {
            get { return this.CYCLES_PER_DAYValue; }
            set { SetProperty(ref CYCLES_PER_DAYValue, value); }
        }

        // Standard PPDM columns

    }
}
