using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ADDRESS : Entity, IPPDMEntity
    {
        private System.String ADDRESS_IDValue;
        public System.String ADDRESS_ID
        {
            get { return this.ADDRESS_IDValue; }
            set { SetProperty(ref ADDRESS_IDValue, value); }
        }

        private System.String OWNER_INFORMATION_IDValue;
        public System.String OWNER_INFORMATION_ID
        {
            get { return this.OWNER_INFORMATION_IDValue; }
            set { SetProperty(ref OWNER_INFORMATION_IDValue, value); }
        }

        private System.String STREET_ADDRESSValue;
        public System.String STREET_ADDRESS
        {
            get { return this.STREET_ADDRESSValue; }
            set { SetProperty(ref STREET_ADDRESSValue, value); }
        }

        private System.String CITYValue;
        public System.String CITY
        {
            get { return this.CITYValue; }
            set { SetProperty(ref CITYValue, value); }
        }

        private System.String STATEValue;
        public System.String STATE
        {
            get { return this.STATEValue; }
            set { SetProperty(ref STATEValue, value); }
        }

        private System.String ZIP_CODEValue;
        public System.String ZIP_CODE
        {
            get { return this.ZIP_CODEValue; }
            set { SetProperty(ref ZIP_CODEValue, value); }
        }

        private System.String COUNTRYValue;
        public System.String COUNTRY
        {
            get { return this.COUNTRYValue; }
            set { SetProperty(ref COUNTRYValue, value); }
        }

        // Standard PPDM columns
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}




