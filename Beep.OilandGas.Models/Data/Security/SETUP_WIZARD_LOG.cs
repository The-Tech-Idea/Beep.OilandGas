using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Security
{
    public partial class SETUP_WIZARD_LOG : Entity, IPPDMEntity
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
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }
    }
}



