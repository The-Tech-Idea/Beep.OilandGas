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

        private Decimal? CYCLE_TIMEValue;
        public Decimal? CYCLE_TIME
        {
            get { return this.CYCLE_TIMEValue; }
            set { SetProperty(ref CYCLE_TIMEValue, value); }
        }

        private Decimal? FALL_TIMEValue;
        public Decimal? FALL_TIME
        {
            get { return this.FALL_TIMEValue; }
            set { SetProperty(ref FALL_TIMEValue, value); }
        }

        private Decimal? RISE_TIMEValue;
        public Decimal? RISE_TIME
        {
            get { return this.RISE_TIMEValue; }
            set { SetProperty(ref RISE_TIMEValue, value); }
        }

        private Decimal? SHUT_IN_TIMEValue;
        public Decimal? SHUT_IN_TIME
        {
            get { return this.SHUT_IN_TIMEValue; }
            set { SetProperty(ref SHUT_IN_TIMEValue, value); }
        }

        private Decimal? FALL_VELOCITYValue;
        public Decimal? FALL_VELOCITY
        {
            get { return this.FALL_VELOCITYValue; }
            set { SetProperty(ref FALL_VELOCITYValue, value); }
        }

        private Decimal? RISE_VELOCITYValue;
        public Decimal? RISE_VELOCITY
        {
            get { return this.RISE_VELOCITYValue; }
            set { SetProperty(ref RISE_VELOCITYValue, value); }
        }

        private Decimal? LIQUID_SLUG_SIZEValue;
        public Decimal? LIQUID_SLUG_SIZE
        {
            get { return this.LIQUID_SLUG_SIZEValue; }
            set { SetProperty(ref LIQUID_SLUG_SIZEValue, value); }
        }

        private Decimal? PRODUCTION_PER_CYCLEValue;
        public Decimal? PRODUCTION_PER_CYCLE
        {
            get { return this.PRODUCTION_PER_CYCLEValue; }
            set { SetProperty(ref PRODUCTION_PER_CYCLEValue, value); }
        }

        private Decimal? DAILY_PRODUCTION_RATEValue;
        public Decimal? DAILY_PRODUCTION_RATE
        {
            get { return this.DAILY_PRODUCTION_RATEValue; }
            set { SetProperty(ref DAILY_PRODUCTION_RATEValue, value); }
        }

        private Decimal? CYCLES_PER_DAYValue;
        public Decimal? CYCLES_PER_DAY
        {
            get { return this.CYCLES_PER_DAYValue; }
            set { SetProperty(ref CYCLES_PER_DAYValue, value); }
        }

        // Standard PPDM columns

    }
}


