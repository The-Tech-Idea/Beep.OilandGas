using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.Security
{
    public partial class SETUP_WIZARD_LOG : ModelEntityBase
    {
        private String SETUP_LOG_IDValue;
        public String SETUP_LOG_ID
        {
            get { return this.SETUP_LOG_IDValue; }
            set { SetProperty(ref SETUP_LOG_IDValue, value); }
        }

        private String ORGANIZATION_IDValue;
        public String ORGANIZATION_ID
        {
            get { return this.ORGANIZATION_IDValue; }
            set { SetProperty(ref ORGANIZATION_IDValue, value); }
        }

        private String SETUP_STEPValue;
        public String SETUP_STEP
        {
            get { return this.SETUP_STEPValue; }
            set { SetProperty(ref SETUP_STEPValue, value); }
        }

        private String SETUP_DATAValue;
        public String SETUP_DATA
        {
            get { return this.SETUP_DATAValue; }
            set { SetProperty(ref SETUP_DATAValue, value); }
        }

        private String EXECUTED_BYValue;
        public String EXECUTED_BY
        {
            get { return this.EXECUTED_BYValue; }
            set { SetProperty(ref EXECUTED_BYValue, value); }
        }

        private DateTime? EXECUTION_DATEValue;
        public DateTime? EXECUTION_DATE
        {
            get { return this.EXECUTION_DATEValue; }
            set { SetProperty(ref EXECUTION_DATEValue, value); }
        }

        private String STATUSValue;
        public String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private String ERROR_MESSAGEValue;
        public String ERROR_MESSAGE
        {
            get { return this.ERROR_MESSAGEValue; }
            set { SetProperty(ref ERROR_MESSAGEValue, value); }
        }

        // Standard PPDM columns

    }
}


