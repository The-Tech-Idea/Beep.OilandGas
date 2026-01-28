using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    public partial class ESP_MOTOR_PROPERTIES : ModelEntityBase {
        private String ESP_MOTOR_PROPERTIES_IDValue;
        public String ESP_MOTOR_PROPERTIES_ID
        {
            get { return this.ESP_MOTOR_PROPERTIES_IDValue; }
            set { SetProperty(ref ESP_MOTOR_PROPERTIES_IDValue, value); }
        }

        private String ESP_DESIGN_RESULT_IDValue;
        public String ESP_DESIGN_RESULT_ID
        {
            get { return this.ESP_DESIGN_RESULT_IDValue; }
            set { SetProperty(ref ESP_DESIGN_RESULT_IDValue, value); }
        }

        private Decimal? HORSEPOWERValue;
        public Decimal? HORSEPOWER
        {
            get { return this.HORSEPOWERValue; }
            set { SetProperty(ref HORSEPOWERValue, value); }
        }

        private Decimal? VOLTAGEValue;
        public Decimal? VOLTAGE
        {
            get { return this.VOLTAGEValue; }
            set { SetProperty(ref VOLTAGEValue, value); }
        }

        private Decimal? EFFICIENCYValue;
        public Decimal? EFFICIENCY
        {
            get { return this.EFFICIENCYValue; }
            set { SetProperty(ref EFFICIENCYValue, value); }
        }

        private Decimal? POWER_FACTORValue;
        public Decimal? POWER_FACTOR
        {
            get { return this.POWER_FACTORValue; }
            set { SetProperty(ref POWER_FACTORValue, value); }
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
