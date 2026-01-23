using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
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

    }
}


