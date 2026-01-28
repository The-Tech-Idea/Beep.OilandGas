using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

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
