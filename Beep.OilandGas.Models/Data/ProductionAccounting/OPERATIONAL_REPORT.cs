using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class OPERATIONAL_REPORT : ModelEntityBase {
        private System.String OPERATIONAL_REPORT_IDValue;
        public System.String OPERATIONAL_REPORT_ID
        {
            get { return this.OPERATIONAL_REPORT_IDValue; }
            set { SetProperty(ref OPERATIONAL_REPORT_IDValue, value); }
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

        private System.String PRODUCTION_REPORT_SUMMARY_IDValue;
        public System.String PRODUCTION_REPORT_SUMMARY_ID
        {
            get { return this.PRODUCTION_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref PRODUCTION_REPORT_SUMMARY_IDValue, value); }
        }

        private System.String INVENTORY_REPORT_SUMMARY_IDValue;
        public System.String INVENTORY_REPORT_SUMMARY_ID
        {
            get { return this.INVENTORY_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref INVENTORY_REPORT_SUMMARY_IDValue, value); }
        }

        private System.String ALLOCATION_REPORT_SUMMARY_IDValue;
        public System.String ALLOCATION_REPORT_SUMMARY_ID
        {
            get { return this.ALLOCATION_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref ALLOCATION_REPORT_SUMMARY_IDValue, value); }
        }

        private System.String MEASUREMENT_REPORT_SUMMARY_IDValue;
        public System.String MEASUREMENT_REPORT_SUMMARY_ID
        {
            get { return this.MEASUREMENT_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref MEASUREMENT_REPORT_SUMMARY_IDValue, value); }
        }

        private System.String COST_REPORT_SUMMARY_IDValue;
        public System.String COST_REPORT_SUMMARY_ID
        {
            get { return this.COST_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref COST_REPORT_SUMMARY_IDValue, value); }
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


