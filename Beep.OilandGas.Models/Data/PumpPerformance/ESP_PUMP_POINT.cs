using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    public partial class ESP_PUMP_POINT : ModelEntityBase {
        private String ESP_PUMP_POINT_IDValue;
        public String ESP_PUMP_POINT_ID
        {
            get { return this.ESP_PUMP_POINT_IDValue; }
            set { SetProperty(ref ESP_PUMP_POINT_IDValue, value); }
        }

        private String ESP_DESIGN_RESULT_IDValue;
        public String ESP_DESIGN_RESULT_ID
        {
            get { return this.ESP_DESIGN_RESULT_IDValue; }
            set { SetProperty(ref ESP_DESIGN_RESULT_IDValue, value); }
        }

        private Decimal FLOW_RATEValue;
        public Decimal FLOW_RATE
        {
            get { return this.FLOW_RATEValue; }
            set { SetProperty(ref FLOW_RATEValue, value); }
        }

        private Decimal HEADValue;
        public Decimal HEAD
        {
            get { return this.HEADValue; }
            set { SetProperty(ref HEADValue, value); }
        }

        private Decimal EFFICIENCYValue;
        public Decimal EFFICIENCY
        {
            get { return this.EFFICIENCYValue; }
            set { SetProperty(ref EFFICIENCYValue, value); }
        }

        private Decimal HORSEPOWERValue;
        public Decimal HORSEPOWER
        {
            get { return this.HORSEPOWERValue; }
            set { SetProperty(ref HORSEPOWERValue, value); }
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
