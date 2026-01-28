using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_PERFORMANCE_POINT : ModelEntityBase {
        private String GAS_LIFT_PERFORMANCE_POINT_IDValue;
        public String GAS_LIFT_PERFORMANCE_POINT_ID
        {
            get { return this.GAS_LIFT_PERFORMANCE_POINT_IDValue; }
            set { SetProperty(ref GAS_LIFT_PERFORMANCE_POINT_IDValue, value); }
        }

        private String GAS_LIFT_POTENTIAL_RESULT_IDValue;
        public String GAS_LIFT_POTENTIAL_RESULT_ID
        {
            get { return this.GAS_LIFT_POTENTIAL_RESULT_IDValue; }
            set { SetProperty(ref GAS_LIFT_POTENTIAL_RESULT_IDValue, value); }
        }

        private Decimal? GAS_INJECTION_RATEValue;
        public Decimal? GAS_INJECTION_RATE
        {
            get { return this.GAS_INJECTION_RATEValue; }
            set { SetProperty(ref GAS_INJECTION_RATEValue, value); }
        }

        private Decimal? PRODUCTION_RATEValue;
        public Decimal? PRODUCTION_RATE
        {
            get { return this.PRODUCTION_RATEValue; }
            set { SetProperty(ref PRODUCTION_RATEValue, value); }
        }

        private Decimal? GAS_LIQUID_RATIOValue;
        public Decimal? GAS_LIQUID_RATIO
        {
            get { return this.GAS_LIQUID_RATIOValue; }
            set { SetProperty(ref GAS_LIQUID_RATIOValue, value); }
        }

        private Decimal? BOTTOM_HOLE_PRESSUREValue;
        public Decimal? BOTTOM_HOLE_PRESSURE
        {
            get { return this.BOTTOM_HOLE_PRESSUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_PRESSUREValue, value); }
        }

        private Int32? POINT_ORDERValue;
        public Int32? POINT_ORDER
        {
            get { return this.POINT_ORDERValue; }
            set { SetProperty(ref POINT_ORDERValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

    }
}
