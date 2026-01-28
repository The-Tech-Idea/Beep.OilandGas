using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Analytics
{
    public partial class ANALYTICS_RESULT : ModelEntityBase
    {
        private System.String ANALYTICS_RESULT_IDValue;
        public System.String ANALYTICS_RESULT_ID
        {
            get { return this.ANALYTICS_RESULT_IDValue; }
            set { SetProperty(ref ANALYTICS_RESULT_IDValue, value); }
        }

        private System.String ANALYTICS_TYPEValue;
        public System.String ANALYTICS_TYPE
        {
            get { return this.ANALYTICS_TYPEValue; }
            set { SetProperty(ref ANALYTICS_TYPEValue, value); }
        }

        private System.DateTime? CALCULATION_DATEValue;
        public System.DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private System.DateTime? PERIOD_STARTValue;
        public System.DateTime? PERIOD_START
        {
            get { return this.PERIOD_STARTValue; }
            set { SetProperty(ref PERIOD_STARTValue, value); }
        }

        private System.DateTime? PERIOD_ENDValue;
        public System.DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private System.String RESULT_DATAValue;
        public System.String RESULT_DATA
        {
            get { return this.RESULT_DATAValue; }
            set { SetProperty(ref RESULT_DATAValue, value); }
        }

        private System.String CALCULATED_BYValue;
        public System.String CALCULATED_BY
        {
            get { return this.CALCULATED_BYValue; }
            set { SetProperty(ref CALCULATED_BYValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;


        private string ANALYTICS_IDValue;
        public string ANALYTICS_ID
        {
            get { return this.ANALYTICS_IDValue; }
            set { SetProperty(ref ANALYTICS_IDValue, value); }
        }

      
    }
}
