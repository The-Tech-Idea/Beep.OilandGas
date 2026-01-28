using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class MEASUREMENT_REPORT_SUMMARY : ModelEntityBase {
        private System.String MEASUREMENT_REPORT_SUMMARY_IDValue;
        public System.String MEASUREMENT_REPORT_SUMMARY_ID
        {
            get { return this.MEASUREMENT_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref MEASUREMENT_REPORT_SUMMARY_IDValue, value); }
        }

        private System.Int32? MEASUREMENT_COUNTValue;
        public System.Int32? MEASUREMENT_COUNT
        {
            get { return this.MEASUREMENT_COUNTValue; }
            set { SetProperty(ref MEASUREMENT_COUNTValue, value); }
        }

        private System.Decimal? AVERAGE_ACCURACYValue;
        public System.Decimal? AVERAGE_ACCURACY
        {
            get { return this.AVERAGE_ACCURACYValue; }
            set { SetProperty(ref AVERAGE_ACCURACYValue, value); }
        }

        private System.Int32? CALIBRATIONS_DUEValue;
        public System.Int32? CALIBRATIONS_DUE
        {
            get { return this.CALIBRATIONS_DUEValue; }
            set { SetProperty(ref CALIBRATIONS_DUEValue, value); }
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
