using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class MEASUREMENT_ACCURACY : ModelEntityBase {
        private System.String MEASUREMENT_ACCURACY_IDValue;
        public System.String MEASUREMENT_ACCURACY_ID
        {
            get { return this.MEASUREMENT_ACCURACY_IDValue; }
            set { SetProperty(ref MEASUREMENT_ACCURACY_IDValue, value); }
        }

        private System.Decimal? MINIMUM_ACCURACYValue;
        public System.Decimal? MINIMUM_ACCURACY
        {
            get { return this.MINIMUM_ACCURACYValue; }
            set { SetProperty(ref MINIMUM_ACCURACYValue, value); }
        }

        private System.Decimal? MAXIMUM_ERRORValue;
        public System.Decimal? MAXIMUM_ERROR
        {
            get { return this.MAXIMUM_ERRORValue; }
            set { SetProperty(ref MAXIMUM_ERRORValue, value); }
        }

        private System.String CALIBRATION_REQUIREDValue;
        public System.String CALIBRATION_REQUIRED
        {
            get { return this.CALIBRATION_REQUIREDValue; }
            set { SetProperty(ref CALIBRATION_REQUIREDValue, value); }
        }

        private System.Int32? CALIBRATION_FREQUENCY_DAYSValue;
        public System.Int32? CALIBRATION_FREQUENCY_DAYS
        {
            get { return this.CALIBRATION_FREQUENCY_DAYSValue; }
            set { SetProperty(ref CALIBRATION_FREQUENCY_DAYSValue, value); }
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


