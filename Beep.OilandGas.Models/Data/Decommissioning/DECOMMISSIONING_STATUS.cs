using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    public partial class DECOMMISSIONING_STATUS : Entity,IPPDMEntity
    {
        private System.String DECOMMISSIONING_STATUS_IDValue;
        public System.String DECOMMISSIONING_STATUS_ID
        {
            get
            {
                return this.DECOMMISSIONING_STATUS_IDValue;
            }
            set { SetProperty(ref DECOMMISSIONING_STATUS_IDValue, value); }
        }

        private System.String FACILITY_DECOMMISSIONING_IDValue;
        public System.String FACILITY_DECOMMISSIONING_ID
        {
            get
            {
                return this.FACILITY_DECOMMISSIONING_IDValue;
            }
            set { SetProperty(ref FACILITY_DECOMMISSIONING_IDValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get
            {
                return this.STATUSValue;
            }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.DateTime? STATUS_DATEValue;
        public System.DateTime? STATUS_DATE
        {
            get
            {
                return this.STATUS_DATEValue;
            }
            set { SetProperty(ref STATUS_DATEValue, value); }
        }

        private System.String STATUS_CHANGED_BYValue;
        public System.String STATUS_CHANGED_BY
        {
            get
            {
                return this.STATUS_CHANGED_BYValue;
            }
            set { SetProperty(ref STATUS_CHANGED_BYValue, value); }
        }

        private System.String NOTESValue;
        public System.String NOTES
        {
            get
            {
                return this.NOTESValue;
            }
            set { SetProperty(ref NOTESValue, value); }
        }

        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get
            {
                return this.ACTIVE_INDValue;
            }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get
            {
                return this.PPDM_GUIDValue;
            }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get
            {
                return this.REMARKValue;
            }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get
            {
                return this.SOURCEValue;
            }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get
            {
                return this.ROW_CHANGED_BYValue;
            }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get
            {
                return this.ROW_CHANGED_DATEValue;
            }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get
            {
                return this.ROW_CREATED_BYValue;
            }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get
            {
                return this.ROW_CREATED_DATEValue;
            }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get
            {
                return this.ROW_EFFECTIVE_DATEValue;
            }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get
            {
                return this.ROW_EXPIRY_DATEValue;
            }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get
            {
                return this.ROW_QUALITYValue;
            }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        public DECOMMISSIONING_STATUS() { }
    }
}

