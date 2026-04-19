using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ALLOCATION_REPORT_SUMMARY : ModelEntityBase {
        private System.String ALLOCATION_REPORT_SUMMARY_IDValue;
        public System.String ALLOCATION_REPORT_SUMMARY_ID
        {
            get { return this.ALLOCATION_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref ALLOCATION_REPORT_SUMMARY_IDValue, value); }
        }

        private System.Int32? WELLS_ALLOCATEDValue;
        public System.Int32? WELLS_ALLOCATED
        {
            get { return this.WELLS_ALLOCATEDValue; }
            set { SetProperty(ref WELLS_ALLOCATEDValue, value); }
        }

        private System.Int32? LEASES_ALLOCATEDValue;
        public System.Int32? LEASES_ALLOCATED
        {
            get { return this.LEASES_ALLOCATEDValue; }
            set { SetProperty(ref LEASES_ALLOCATEDValue, value); }
        }

        private System.Decimal  TOTAL_ALLOCATED_VOLUMEValue;
        public System.Decimal  TOTAL_ALLOCATED_VOLUME
        {
            get { return this.TOTAL_ALLOCATED_VOLUMEValue; }
            set { SetProperty(ref TOTAL_ALLOCATED_VOLUMEValue, value); }
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
