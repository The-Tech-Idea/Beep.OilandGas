using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    public partial class ESP_DESIGN_PROPERTIES : ModelEntityBase {
        private String ESP_DESIGN_PROPERTIES_IDValue;
        public String ESP_DESIGN_PROPERTIES_ID
        {
            get { return this.ESP_DESIGN_PROPERTIES_IDValue; }
            set { SetProperty(ref ESP_DESIGN_PROPERTIES_IDValue, value); }
        }

        private Decimal? DESIRED_FLOW_RATEValue;
        public Decimal? DESIRED_FLOW_RATE
        {
            get { return this.DESIRED_FLOW_RATEValue; }
            set { SetProperty(ref DESIRED_FLOW_RATEValue, value); }
        }

        private Decimal? TOTAL_DYNAMIC_HEADValue;
        public Decimal? TOTAL_DYNAMIC_HEAD
        {
            get { return this.TOTAL_DYNAMIC_HEADValue; }
            set { SetProperty(ref TOTAL_DYNAMIC_HEADValue, value); }
        }

        private Decimal? WELL_DEPTHValue;
        public Decimal? WELL_DEPTH
        {
            get { return this.WELL_DEPTHValue; }
            set { SetProperty(ref WELL_DEPTHValue, value); }
        }

        private Decimal? CASING_DIAMETERValue;
        public Decimal? CASING_DIAMETER
        {
            get { return this.CASING_DIAMETERValue; }
            set { SetProperty(ref CASING_DIAMETERValue, value); }
        }

        private Decimal? TUBING_DIAMETERValue;
        public Decimal? TUBING_DIAMETER
        {
            get { return this.TUBING_DIAMETERValue; }
            set { SetProperty(ref TUBING_DIAMETERValue, value); }
        }

        private Decimal? OIL_GRAVITYValue;
        public Decimal? OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private Decimal? WATER_CUTValue;
        public Decimal? WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private Decimal? GAS_OIL_RATIOValue;
        public Decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private Decimal? WELLHEAD_PRESSUREValue;
        public Decimal? WELLHEAD_PRESSURE
        {
            get { return this.WELLHEAD_PRESSUREValue; }
            set { SetProperty(ref WELLHEAD_PRESSUREValue, value); }
        }

        private Decimal? BOTTOM_HOLE_TEMPERATUREValue;
        public Decimal? BOTTOM_HOLE_TEMPERATURE
        {
            get { return this.BOTTOM_HOLE_TEMPERATUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_TEMPERATUREValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? PUMP_SETTING_DEPTHValue;
        public Decimal? PUMP_SETTING_DEPTH
        {
            get { return this.PUMP_SETTING_DEPTHValue; }
            set { SetProperty(ref PUMP_SETTING_DEPTHValue, value); }
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


