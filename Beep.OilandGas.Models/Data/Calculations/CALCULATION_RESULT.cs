using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Calculations
{
    public partial class CALCULATION_RESULT : ModelEntityBase
    {
        private System.String CALCULATION_RESULT_IDValue;
        public System.String CALCULATION_RESULT_ID
        {
            get { return this.CALCULATION_RESULT_IDValue; }
            set { SetProperty(ref CALCULATION_RESULT_IDValue, value); }
        }

        private System.String CALCULATION_TYPEValue;
        public System.String CALCULATION_TYPE
        {
            get { return this.CALCULATION_TYPEValue; }
            set { SetProperty(ref CALCULATION_TYPEValue, value); }
        }

        private System.DateTime? CALCULATION_DATEValue;
        public System.DateTime? CALCULATION_DATE
        {
            get { return this.CALCULATION_DATEValue; }
            set { SetProperty(ref CALCULATION_DATEValue, value); }
        }

        private System.String INPUT_DATAValue;
        public System.String INPUT_DATA
        {
            get { return this.INPUT_DATAValue; }
            set { SetProperty(ref INPUT_DATAValue, value); }
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


    }
}


