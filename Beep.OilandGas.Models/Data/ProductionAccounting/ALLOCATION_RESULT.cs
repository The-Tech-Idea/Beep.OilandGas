using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ALLOCATION_RESULT : ModelEntityBase {
        private System.String ALLOCATION_RESULT_IDValue;
        public System.String ALLOCATION_RESULT_ID
        {
            get { return this.ALLOCATION_RESULT_IDValue; }
            set { SetProperty(ref ALLOCATION_RESULT_IDValue, value); }
        }

        private System.String ALLOCATION_REQUEST_IDValue;
        public System.String ALLOCATION_REQUEST_ID
        {
            get { return this.ALLOCATION_REQUEST_IDValue; }
            set { SetProperty(ref ALLOCATION_REQUEST_IDValue, value); }
        }

        private System.DateTime? ALLOCATION_DATEValue;
        public System.DateTime? ALLOCATION_DATE
        {
            get { return this.ALLOCATION_DATEValue; }
            set { SetProperty(ref ALLOCATION_DATEValue, value); }
        }

        private System.String ALLOCATION_METHODValue;
        public System.String ALLOCATION_METHOD
        {
            get { return this.ALLOCATION_METHODValue; }
            set { SetProperty(ref ALLOCATION_METHODValue, value); }
        }

        private System.String AFE_IDValue;
        public System.String AFE_ID
        {
            get { return this.AFE_IDValue; }
            set { SetProperty(ref AFE_IDValue, value); }
        }

        private System.Decimal? TOTAL_VOLUMEValue;
        public System.Decimal? TOTAL_VOLUME
        {
            get { return this.TOTAL_VOLUMEValue; }
            set { SetProperty(ref TOTAL_VOLUMEValue, value); }
        }

        private System.Decimal? ALLOCATED_VOLUMEValue;
        public System.Decimal? ALLOCATED_VOLUME
        {
            get { return this.ALLOCATED_VOLUMEValue; }
            set { SetProperty(ref ALLOCATED_VOLUMEValue, value); }
        }

        private System.Decimal? ALLOCATION_VARIANCEValue;
        public System.Decimal? ALLOCATION_VARIANCE
        {
            get { return this.ALLOCATION_VARIANCEValue; }
            set { SetProperty(ref ALLOCATION_VARIANCEValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
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




