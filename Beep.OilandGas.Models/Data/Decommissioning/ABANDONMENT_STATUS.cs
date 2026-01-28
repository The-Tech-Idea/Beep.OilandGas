using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    public partial class ABANDONMENT_STATUS : ModelEntityBase
    {
        private System.String ABANDONMENT_STATUS_IDValue;
        public System.String ABANDONMENT_STATUS_ID
        {
            get
            {
                return this.ABANDONMENT_STATUS_IDValue;
            }
            set { SetProperty(ref ABANDONMENT_STATUS_IDValue, value); }
        }

        private System.String WELL_ABANDONMENT_IDValue;
        public System.String WELL_ABANDONMENT_ID
        {
            get
            {
                return this.WELL_ABANDONMENT_IDValue;
            }
            set { SetProperty(ref WELL_ABANDONMENT_IDValue, value); }
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

        public ABANDONMENT_STATUS() { }
    }
}
