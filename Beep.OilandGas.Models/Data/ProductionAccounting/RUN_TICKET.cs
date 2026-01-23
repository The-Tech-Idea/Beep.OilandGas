using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class RUN_TICKET : ModelEntityBase {
        private System.String RUN_TICKET_IDValue;
        public System.String RUN_TICKET_ID
        {
            get { return this.RUN_TICKET_IDValue; }
            set { SetProperty(ref RUN_TICKET_IDValue, value); }
        }

        private System.String RUN_TICKET_NUMBERValue;
        public System.String RUN_TICKET_NUMBER
        {
            get { return this.RUN_TICKET_NUMBERValue; }
            set { SetProperty(ref RUN_TICKET_NUMBERValue, value); }
        }

        private System.String AFE_IDValue;
        public System.String AFE_ID
        {
            get { return this.AFE_IDValue; }
            set { SetProperty(ref AFE_IDValue, value); }
        }

        private System.DateTime? TICKET_DATE_TIMEValue;
        public System.DateTime? TICKET_DATE_TIME
        {
            get { return this.TICKET_DATE_TIMEValue; }
            set { SetProperty(ref TICKET_DATE_TIMEValue, value); }
        }

        private System.String LEASE_IDValue;
        public System.String LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private System.String WELL_IDValue;
        public System.String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private System.String TANK_BATTERY_IDValue;
        public System.String TANK_BATTERY_ID
        {
            get { return this.TANK_BATTERY_IDValue; }
            set { SetProperty(ref TANK_BATTERY_IDValue, value); }
        }

        private System.Decimal? GROSS_VOLUMEValue;
        public System.Decimal? GROSS_VOLUME
        {
            get { return this.GROSS_VOLUMEValue; }
            set { SetProperty(ref GROSS_VOLUMEValue, value); }
        }

        private System.Decimal? BSW_VOLUMEValue;
        public System.Decimal? BSW_VOLUME
        {
            get { return this.BSW_VOLUMEValue; }
            set { SetProperty(ref BSW_VOLUMEValue, value); }
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

        private System.Decimal? TEMPERATUREValue;
        public System.Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private System.Decimal? API_GRAVITYValue;
        public System.Decimal? API_GRAVITY
        {
            get { return this.API_GRAVITYValue; }
            set { SetProperty(ref API_GRAVITYValue, value); }
        }

        private System.String DISPOSITION_TYPEValue;
        public System.String DISPOSITION_TYPE
        {
            get { return this.DISPOSITION_TYPEValue; }
            set { SetProperty(ref DISPOSITION_TYPEValue, value); }
        }

        private System.String PURCHASERValue;
        public System.String PURCHASER
        {
            get { return this.PURCHASERValue; }
            set { SetProperty(ref PURCHASERValue, value); }
        }

        private System.Decimal? PRICE_PER_BARRELValue;
        public System.Decimal? PRICE_PER_BARREL
        {
            get { return this.PRICE_PER_BARRELValue; }
            set { SetProperty(ref PRICE_PER_BARRELValue, value); }
        }

        private System.String MEASUREMENT_METHODValue;
        public System.String MEASUREMENT_METHOD
        {
            get { return this.MEASUREMENT_METHODValue; }
            set { SetProperty(ref MEASUREMENT_METHODValue, value); }
        }

        private System.String MEASUREMENT_RECORD_IDValue;
        public System.String MEASUREMENT_RECORD_ID
        {
            get { return this.MEASUREMENT_RECORD_IDValue; }
            set { SetProperty(ref MEASUREMENT_RECORD_IDValue, value); }
        }

        private System.String NOTESValue;
        public System.String NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        private System.String IS_PROCESSEDValue;
        public System.String IS_PROCESSED
        {
            get { return this.IS_PROCESSEDValue; }
            set { SetProperty(ref IS_PROCESSEDValue, value); }
        }

        private System.DateTime? PROCESSED_DATEValue;
        public System.DateTime? PROCESSED_DATE
        {
            get { return this.PROCESSED_DATEValue; }
            set { SetProperty(ref PROCESSED_DATEValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


