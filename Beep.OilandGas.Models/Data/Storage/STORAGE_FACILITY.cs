using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Storage
{
    public partial class STORAGE_FACILITY : ModelEntityBase
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
