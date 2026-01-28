using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASE_REPORT : ModelEntityBase {
        private System.String LEASE_REPORT_IDValue;
        public System.String LEASE_REPORT_ID
        {
            get { return this.LEASE_REPORT_IDValue; }
            set { SetProperty(ref LEASE_REPORT_IDValue, value); }
        }

        private System.String REPORT_TYPEValue;
        public System.String REPORT_TYPE
        {
            get { return this.REPORT_TYPEValue; }
            set { SetProperty(ref REPORT_TYPEValue, value); }
        }

        private System.DateTime? REPORT_PERIOD_STARTValue;
        public System.DateTime? REPORT_PERIOD_START
        {
            get { return this.REPORT_PERIOD_STARTValue; }
            set { SetProperty(ref REPORT_PERIOD_STARTValue, value); }
        }

        private System.DateTime? REPORT_PERIOD_ENDValue;
        public System.DateTime? REPORT_PERIOD_END
        {
            get { return this.REPORT_PERIOD_ENDValue; }
            set { SetProperty(ref REPORT_PERIOD_ENDValue, value); }
        }

        private System.DateTime? GENERATION_DATEValue;
        public System.DateTime? GENERATION_DATE
        {
            get { return this.GENERATION_DATEValue; }
            set { SetProperty(ref GENERATION_DATEValue, value); }
        }

        private System.String GENERATED_BYValue;
        public System.String GENERATED_BY
        {
            get { return this.GENERATED_BYValue; }
            set { SetProperty(ref GENERATED_BYValue, value); }
        }

        private System.String LEASE_IDValue;
        public System.String LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private System.Decimal? PRODUCTION_VOLUMEValue;
        public System.Decimal? PRODUCTION_VOLUME
        {
            get { return this.PRODUCTION_VOLUMEValue; }
            set { SetProperty(ref PRODUCTION_VOLUMEValue, value); }
        }

        private System.Decimal? REVENUEValue;
        public System.Decimal? REVENUE
        {
            get { return this.REVENUEValue; }
            set { SetProperty(ref REVENUEValue, value); }
        }

        private System.Decimal? COSTSValue;
        public System.Decimal? COSTS
        {
            get { return this.COSTSValue; }
            set { SetProperty(ref COSTSValue, value); }
        }

        private System.Decimal? NET_PROFITValue;
        public System.Decimal? NET_PROFIT
        {
            get { return this.NET_PROFITValue; }
            set { SetProperty(ref NET_PROFITValue, value); }
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
