using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Storage
{
    public partial class STORAGE_FACILITY : Entity
    {
        private System.String STORAGE_FACILITY_IDValue;
        public System.String STORAGE_FACILITY_ID
        {
            get { return this.STORAGE_FACILITY_IDValue; }
            set { SetProperty(ref STORAGE_FACILITY_IDValue, value); }
        }

        private System.String FACILITY_NAMEValue;
        public System.String FACILITY_NAME
        {
            get { return this.FACILITY_NAMEValue; }
            set { SetProperty(ref FACILITY_NAMEValue, value); }
        }

        private System.String FACILITY_TYPEValue;
        public System.String FACILITY_TYPE
        {
            get { return this.FACILITY_TYPEValue; }
            set { SetProperty(ref FACILITY_TYPEValue, value); }
        }

        private System.String LOCATIONValue;
        public System.String LOCATION
        {
            get { return this.LOCATIONValue; }
            set { SetProperty(ref LOCATIONValue, value); }
        }

        private System.Decimal? CAPACITYValue;
        public System.Decimal? CAPACITY
        {
            get { return this.CAPACITYValue; }
            set { SetProperty(ref CAPACITYValue, value); }
        }

        private System.Decimal? CURRENT_INVENTORYValue;
        public System.Decimal? CURRENT_INVENTORY
        {
            get { return this.CURRENT_INVENTORYValue; }
            set { SetProperty(ref CURRENT_INVENTORYValue, value); }
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

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
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
    }
}




