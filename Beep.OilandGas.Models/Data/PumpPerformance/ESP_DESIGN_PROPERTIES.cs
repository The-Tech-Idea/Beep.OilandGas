using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

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
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

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

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}



