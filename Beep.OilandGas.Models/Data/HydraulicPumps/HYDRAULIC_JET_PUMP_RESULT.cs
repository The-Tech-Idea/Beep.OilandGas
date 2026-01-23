using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    public partial class HYDRAULIC_JET_PUMP_RESULT : ModelEntityBase {
        private String HYDRAULIC_JET_PUMP_RESULT_IDValue;
        public String HYDRAULIC_JET_PUMP_RESULT_ID
        {
            get { return this.HYDRAULIC_JET_PUMP_RESULT_IDValue; }
            set { SetProperty(ref HYDRAULIC_JET_PUMP_RESULT_IDValue, value); }
        }

        private String HYDRAULIC_JET_PUMP_PROPERTIES_IDValue;
        public String HYDRAULIC_JET_PUMP_PROPERTIES_ID
        {
            get { return this.HYDRAULIC_JET_PUMP_PROPERTIES_IDValue; }
            set { SetProperty(ref HYDRAULIC_JET_PUMP_PROPERTIES_IDValue, value); }
        }

        private Decimal? PRODUCTION_RATEValue;
        public Decimal? PRODUCTION_RATE
        {
            get { return this.PRODUCTION_RATEValue; }
            set { SetProperty(ref PRODUCTION_RATEValue, value); }
        }

        private Decimal? TOTAL_FLOW_RATEValue;
        public Decimal? TOTAL_FLOW_RATE
        {
            get { return this.TOTAL_FLOW_RATEValue; }
            set { SetProperty(ref TOTAL_FLOW_RATEValue, value); }
        }

        private Decimal? PRODUCTION_RATIOValue;
        public Decimal? PRODUCTION_RATIO
        {
            get { return this.PRODUCTION_RATIOValue; }
            set { SetProperty(ref PRODUCTION_RATIOValue, value); }
        }

        private Decimal? PUMP_EFFICIENCYValue;
        public Decimal? PUMP_EFFICIENCY
        {
            get { return this.PUMP_EFFICIENCYValue; }
            set { SetProperty(ref PUMP_EFFICIENCYValue, value); }
        }

        private Decimal? POWER_FLUID_HORSEPOWERValue;
        public Decimal? POWER_FLUID_HORSEPOWER
        {
            get { return this.POWER_FLUID_HORSEPOWERValue; }
            set { SetProperty(ref POWER_FLUID_HORSEPOWERValue, value); }
        }

        private Decimal? HYDRAULIC_HORSEPOWERValue;
        public Decimal? HYDRAULIC_HORSEPOWER
        {
            get { return this.HYDRAULIC_HORSEPOWERValue; }
            set { SetProperty(ref HYDRAULIC_HORSEPOWERValue, value); }
        }

        private Decimal? SYSTEM_EFFICIENCYValue;
        public Decimal? SYSTEM_EFFICIENCY
        {
            get { return this.SYSTEM_EFFICIENCYValue; }
            set { SetProperty(ref SYSTEM_EFFICIENCYValue, value); }
        }

        private Decimal? PUMP_INTAKE_PRESSUREValue;
        public Decimal? PUMP_INTAKE_PRESSURE
        {
            get { return this.PUMP_INTAKE_PRESSUREValue; }
            set { SetProperty(ref PUMP_INTAKE_PRESSUREValue, value); }
        }

        private Decimal? PUMP_DISCHARGE_PRESSUREValue;
        public Decimal? PUMP_DISCHARGE_PRESSURE
        {
            get { return this.PUMP_DISCHARGE_PRESSUREValue; }
            set { SetProperty(ref PUMP_DISCHARGE_PRESSUREValue, value); }
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


