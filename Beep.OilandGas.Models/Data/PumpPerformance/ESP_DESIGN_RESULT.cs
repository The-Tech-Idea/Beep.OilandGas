using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    public partial class ESP_DESIGN_RESULT : ModelEntityBase {
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

        public List<ESP_PUMP_POINT> PERFORMANCE_POINTS { get; set; }
    }
}
