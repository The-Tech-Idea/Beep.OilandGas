using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class RESERVOIR_STATUS : ModelEntityBase
    {
        private System.String RESERVOIR_STATUS_IDValue;
        public System.String RESERVOIR_STATUS_ID
        {
            get
            {
                return this.RESERVOIR_STATUS_IDValue;
            }
            set { SetProperty(ref RESERVOIR_STATUS_IDValue, value); }
        }

        private System.String RESERVE_ENTITY_IDValue;
        public System.String RESERVE_ENTITY_ID
        {
            get
            {
                return this.RESERVE_ENTITY_IDValue;
            }
            set { SetProperty(ref RESERVE_ENTITY_IDValue, value); }
        }

        private System.String POOL_IDValue;
        public System.String POOL_ID
        {
            get
            {
                return this.POOL_IDValue;
            }
            set { SetProperty(ref POOL_IDValue, value); }
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

        public RESERVOIR_STATUS() { }
    }
}
