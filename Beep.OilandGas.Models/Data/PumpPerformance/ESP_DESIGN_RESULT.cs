using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    public partial class ESP_DESIGN_RESULT : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private String ESP_DESIGN_RESULT_IDValue;
        public String ESP_DESIGN_RESULT_ID
        {
            get { return this.ESP_DESIGN_RESULT_IDValue; }
            set { SetProperty(ref ESP_DESIGN_RESULT_IDValue, value); }
        }

        private String ESP_DESIGN_PROPERTIES_IDValue;
        public String ESP_DESIGN_PROPERTIES_ID
        {
            get { return this.ESP_DESIGN_PROPERTIES_IDValue; }
            set { SetProperty(ref ESP_DESIGN_PROPERTIES_IDValue, value); }
        }

        private Int32? PUMP_STAGESValue;
        public Int32? PUMP_STAGES
        {
            get { return this.PUMP_STAGESValue; }
            set { SetProperty(ref PUMP_STAGESValue, value); }
        }

        private Decimal? REQUIRED_HORSEPOWERValue;
        public Decimal? REQUIRED_HORSEPOWER
        {
            get { return this.REQUIRED_HORSEPOWERValue; }
            set { SetProperty(ref REQUIRED_HORSEPOWERValue, value); }
        }

        private Decimal? MOTOR_HORSEPOWERValue;
        public Decimal? MOTOR_HORSEPOWER
        {
            get { return this.MOTOR_HORSEPOWERValue; }
            set { SetProperty(ref MOTOR_HORSEPOWERValue, value); }
        }

        private Decimal? MOTOR_VOLTAGEValue;
        public Decimal? MOTOR_VOLTAGE
        {
            get { return this.MOTOR_VOLTAGEValue; }
            set { SetProperty(ref MOTOR_VOLTAGEValue, value); }
        }

        private Decimal? MOTOR_CURRENTValue;
        public Decimal? MOTOR_CURRENT
        {
            get { return this.MOTOR_CURRENTValue; }
            set { SetProperty(ref MOTOR_CURRENTValue, value); }
        }

        private Int32? CABLE_SIZEValue;
        public Int32? CABLE_SIZE
        {
            get { return this.CABLE_SIZEValue; }
            set { SetProperty(ref CABLE_SIZEValue, value); }
        }

        private Decimal? CABLE_LENGTHValue;
        public Decimal? CABLE_LENGTH
        {
            get { return this.CABLE_LENGTHValue; }
            set { SetProperty(ref CABLE_LENGTHValue, value); }
        }

        private Decimal? SYSTEM_EFFICIENCYValue;
        public Decimal? SYSTEM_EFFICIENCY
        {
            get { return this.SYSTEM_EFFICIENCYValue; }
            set { SetProperty(ref SYSTEM_EFFICIENCYValue, value); }
        }

        private Decimal? PUMP_EFFICIENCYValue;
        public Decimal? PUMP_EFFICIENCY
        {
            get { return this.PUMP_EFFICIENCYValue; }
            set { SetProperty(ref PUMP_EFFICIENCYValue, value); }
        }

        private Decimal? MOTOR_EFFICIENCYValue;
        public Decimal? MOTOR_EFFICIENCY
        {
            get { return this.MOTOR_EFFICIENCYValue; }
            set { SetProperty(ref MOTOR_EFFICIENCYValue, value); }
        }

        private Decimal? POWER_CONSUMPTIONValue;
        public Decimal? POWER_CONSUMPTION
        {
            get { return this.POWER_CONSUMPTIONValue; }
            set { SetProperty(ref POWER_CONSUMPTIONValue, value); }
        }

        private Decimal? OPERATING_FLOW_RATEValue;
        public Decimal? OPERATING_FLOW_RATE
        {
            get { return this.OPERATING_FLOW_RATEValue; }
            set { SetProperty(ref OPERATING_FLOW_RATEValue, value); }
        }

        private Decimal? OPERATING_HEADValue;
        public Decimal? OPERATING_HEAD
        {
            get { return this.OPERATING_HEADValue; }
            set { SetProperty(ref OPERATING_HEADValue, value); }
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



