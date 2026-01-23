using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class GOVERNMENTAL_REPORT : ModelEntityBase {
        private System.String GOVERNMENTAL_REPORT_IDValue;
        public System.String GOVERNMENTAL_REPORT_ID
        {
            get { return this.GOVERNMENTAL_REPORT_IDValue; }
            set { SetProperty(ref GOVERNMENTAL_REPORT_IDValue, value); }
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

        private System.String REPORTING_AGENCYValue;
        public System.String REPORTING_AGENCY
        {
            get { return this.REPORTING_AGENCYValue; }
            set { SetProperty(ref REPORTING_AGENCYValue, value); }
        }

        private System.String REPORT_FORMATValue;
        public System.String REPORT_FORMAT
        {
            get { return this.REPORT_FORMATValue; }
            set { SetProperty(ref REPORT_FORMATValue, value); }
        }

        private System.String GOVERNMENTAL_PRODUCTION_DATA_IDValue;
        public System.String GOVERNMENTAL_PRODUCTION_DATA_ID
        {
            get { return this.GOVERNMENTAL_PRODUCTION_DATA_IDValue; }
            set { SetProperty(ref GOVERNMENTAL_PRODUCTION_DATA_IDValue, value); }
        }

        private System.String GOVERNMENTAL_ROYALTY_DATA_IDValue;
        public System.String GOVERNMENTAL_ROYALTY_DATA_ID
        {
            get { return this.GOVERNMENTAL_ROYALTY_DATA_IDValue; }
            set { SetProperty(ref GOVERNMENTAL_ROYALTY_DATA_IDValue, value); }
        }

        private System.String GOVERNMENTAL_TAX_DATA_IDValue;
        public System.String GOVERNMENTAL_TAX_DATA_ID
        {
            get { return this.GOVERNMENTAL_TAX_DATA_IDValue; }
            set { SetProperty(ref GOVERNMENTAL_TAX_DATA_IDValue, value); }
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


