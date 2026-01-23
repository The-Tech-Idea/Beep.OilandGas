using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Rendering
{
    public partial class CHART_CONFIGURATION : ModelEntityBase
    {
        private System.String CHART_CONFIGURATION_IDValue;
        public System.String CHART_CONFIGURATION_ID
        {
            get { return this.CHART_CONFIGURATION_IDValue; }
            set { SetProperty(ref CHART_CONFIGURATION_IDValue, value); }
        }

        private System.String CHART_TYPEValue;
        public System.String CHART_TYPE
        {
            get { return this.CHART_TYPEValue; }
            set { SetProperty(ref CHART_TYPEValue, value); }
        }

        private System.String CONFIGURATION_NAMEValue;
        public System.String CONFIGURATION_NAME
        {
            get { return this.CONFIGURATION_NAMEValue; }
            set { SetProperty(ref CONFIGURATION_NAMEValue, value); }
        }

        private System.String CONFIGURATION_DATAValue;
        public System.String CONFIGURATION_DATA
        {
            get { return this.CONFIGURATION_DATAValue; }
            set { SetProperty(ref CONFIGURATION_DATAValue, value); }
        }

        private System.String CREATED_BYValue;
        public System.String CREATED_BY
        {
            get { return this.CREATED_BYValue; }
            set { SetProperty(ref CREATED_BYValue, value); }
        }

        private System.String IS_DEFAULTValue;
        public System.String IS_DEFAULT
        {
            get { return this.IS_DEFAULTValue; }
            set { SetProperty(ref IS_DEFAULTValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


