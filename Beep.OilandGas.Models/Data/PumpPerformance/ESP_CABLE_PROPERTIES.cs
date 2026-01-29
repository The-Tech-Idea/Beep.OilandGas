using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PumpPerformance
{
    public partial class ESP_CABLE_PROPERTIES : ModelEntityBase {
        private String ESP_CABLE_PROPERTIES_IDValue;
        public String ESP_CABLE_PROPERTIES_ID
        {
            get { return this.ESP_CABLE_PROPERTIES_IDValue; }
            set { SetProperty(ref ESP_CABLE_PROPERTIES_IDValue, value); }
        }

        private String ESP_DESIGN_RESULT_IDValue;
        public String ESP_DESIGN_RESULT_ID
        {
            get { return this.ESP_DESIGN_RESULT_IDValue; }
            set { SetProperty(ref ESP_DESIGN_RESULT_IDValue, value); }
        }

        private Int32? CABLE_SIZEValue;
        public Int32? CABLE_SIZE
        {
            get { return this.CABLE_SIZEValue; }
            set { SetProperty(ref CABLE_SIZEValue, value); }
        }

        private Decimal  CABLE_LENGTHValue;
        public Decimal  CABLE_LENGTH
        {
            get { return this.CABLE_LENGTHValue; }
            set { SetProperty(ref CABLE_LENGTHValue, value); }
        }

        private Decimal  RESISTANCE_PER_1000_FEETValue;
        public Decimal  RESISTANCE_PER_1000_FEET
        {
            get { return this.RESISTANCE_PER_1000_FEETValue; }
            set { SetProperty(ref RESISTANCE_PER_1000_FEETValue, value); }
        }

        private Decimal  VOLTAGE_DROPValue;
        public Decimal  VOLTAGE_DROP
        {
            get { return this.VOLTAGE_DROPValue; }
            set { SetProperty(ref VOLTAGE_DROPValue, value); }
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
