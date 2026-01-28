using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class MEASUREMENT_RECORD : ModelEntityBase {
        private System.String MEASUREMENT_IDValue;
        public System.String MEASUREMENT_ID
        {
            get { return this.MEASUREMENT_IDValue; }
            set { SetProperty(ref MEASUREMENT_IDValue, value); }
        }

        private System.DateTime? MEASUREMENT_DATETIMEValue;
        public System.DateTime? MEASUREMENT_DATETIME
        {
            get { return this.MEASUREMENT_DATETIMEValue; }
            set { SetProperty(ref MEASUREMENT_DATETIMEValue, value); }
        }

        private System.String MEASUREMENT_METHODValue;
        public System.String MEASUREMENT_METHOD
        {
            get { return this.MEASUREMENT_METHODValue; }
            set { SetProperty(ref MEASUREMENT_METHODValue, value); }
        }

        private System.String MEASUREMENT_STANDARDValue;
        public System.String MEASUREMENT_STANDARD
        {
            get { return this.MEASUREMENT_STANDARDValue; }
            set { SetProperty(ref MEASUREMENT_STANDARDValue, value); }
        }

        private System.Decimal? GROSS_VOLUMEValue;
        public System.Decimal? GROSS_VOLUME
        {
            get { return this.GROSS_VOLUMEValue; }
            set { SetProperty(ref GROSS_VOLUMEValue, value); }
        }

        private System.Decimal? BSWValue;
        public System.Decimal? BSW
        {
            get { return this.BSWValue; }
            set { SetProperty(ref BSWValue, value); }
        }

        private System.Decimal? BSW_PERCENTAGEValue;
        public System.Decimal? BSW_PERCENTAGE
        {
            get { return this.BSW_PERCENTAGEValue; }
            set { SetProperty(ref BSW_PERCENTAGEValue, value); }
        }

        private System.Decimal? NET_VOLUMEValue;
        public System.Decimal? NET_VOLUME
        {
            get { return this.NET_VOLUMEValue; }
            set { SetProperty(ref NET_VOLUMEValue, value); }
        }

        private System.String WELL_IDValue;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.String LEASE_IDValue;
        public System.String LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.String RUN_TICKET_IDValue;
        public System.String RUN_TICKET_ID
        {
            get { return this.RUN_TICKET_IDValue; }
            set { SetProperty(ref RUN_TICKET_IDValue, value); }
        }

        private System.String TANK_BATTERY_IDValue;
        public System.String TANK_BATTERY_ID
        {
            get { return this.TANK_BATTERY_IDValue; }
            set { SetProperty(ref TANK_BATTERY_IDValue, value); }
        }

        private System.Decimal? TEMPERATUREValue;
        public System.Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private System.Decimal? PRESSUREValue;
        public System.Decimal? PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private System.Decimal? API_GRAVITYValue;
        public System.Decimal? API_GRAVITY
        {
            get { return this.API_GRAVITYValue; }
            set { SetProperty(ref API_GRAVITYValue, value); }
        }

        private System.String CRUDE_OIL_PROPERTIES_IDValue;
        public System.String CRUDE_OIL_PROPERTIES_ID
        {
            get { return this.CRUDE_OIL_PROPERTIES_IDValue; }
            set { SetProperty(ref CRUDE_OIL_PROPERTIES_IDValue, value); }
        }

        private System.Decimal? ACCURACYValue;
        public System.Decimal? ACCURACY
        {
            get { return this.ACCURACYValue; }
            set { SetProperty(ref ACCURACYValue, value); }
        }

        private System.String MEASUREMENT_DEVICEValue;
        public System.String MEASUREMENT_DEVICE
        {
            get { return this.MEASUREMENT_DEVICEValue; }
            set { SetProperty(ref MEASUREMENT_DEVICEValue, value); }
        }

        private System.String OPERATORValue;
        public System.String OPERATOR
        {
            get { return this.OPERATORValue; }
            set { SetProperty(ref OPERATORValue, value); }
        }

        private System.String NOTESValue;
        public System.String NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        // Standard PPDM columns


        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private DateTime MEASUREMENT_DATE_TIMEValue;
        public DateTime MEASUREMENT_DATE_TIME
        {
            get { return this.MEASUREMENT_DATE_TIMEValue; }
            set { SetProperty(ref MEASUREMENT_DATE_TIMEValue, value); }
        }

        private MeasurementMethod METHODValue;
        public MeasurementMethod METHOD
        {
            get { return this.METHODValue; }
            set { SetProperty(ref METHODValue, value); }
        }

        private MeasurementStandard STANDARDValue;
        public MeasurementStandard STANDARD
        {
            get { return this.STANDARDValue; }
            set { SetProperty(ref STANDARDValue, value); }
        }

    }
}
