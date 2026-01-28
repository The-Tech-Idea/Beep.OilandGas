using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ALLOCATION_DETAIL : ModelEntityBase
    {
        private System.String ALLOCATION_DETAIL_IDValue;
        public System.String ALLOCATION_DETAIL_ID
        {
            get { return this.ALLOCATION_DETAIL_IDValue; }
            set { SetProperty(ref ALLOCATION_DETAIL_IDValue, value); }
        }

        private System.String ALLOCATION_RESULT_IDValue;
        public System.String ALLOCATION_RESULT_ID
        {
            get { return this.ALLOCATION_RESULT_IDValue; }
            set { SetProperty(ref ALLOCATION_RESULT_IDValue, value); }
        }

        private System.String ENTITY_IDValue;
        public System.String ENTITY_ID
        {
            get { return this.ENTITY_IDValue; }
            set { SetProperty(ref ENTITY_IDValue, value); }
        }

        private System.String ENTITY_TYPEValue;
        public System.String ENTITY_TYPE
        {
            get { return this.ENTITY_TYPEValue; }
            set { SetProperty(ref ENTITY_TYPEValue, value); }
        }

        private System.String ENTITY_NAMEValue;
        public System.String ENTITY_NAME
        {
            get { return this.ENTITY_NAMEValue; }
            set { SetProperty(ref ENTITY_NAMEValue, value); }
        }

        private System.Decimal? ALLOCATED_VOLUMEValue;
        public System.Decimal? ALLOCATED_VOLUME
        {
            get { return this.ALLOCATED_VOLUMEValue; }
            set { SetProperty(ref ALLOCATED_VOLUMEValue, value); }
        }

        private System.Decimal? ALLOCATION_PERCENTAGEValue;
        public System.Decimal? ALLOCATION_PERCENTAGE
        {
            get { return this.ALLOCATION_PERCENTAGEValue; }
            set { SetProperty(ref ALLOCATION_PERCENTAGEValue, value); }
        }

        private System.Decimal? ALLOCATION_BASISValue;
        public System.Decimal? ALLOCATION_BASIS
        {
            get { return this.ALLOCATION_BASISValue; }
            set { SetProperty(ref ALLOCATION_BASISValue, value); }
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

        // Converted AllocationRequest_ID to Entity pattern
        private System.String AllocationRequest_IDValue;
        public System.String AllocationRequest_ID
        {
            get { return this.AllocationRequest_IDValue; }
            set { SetProperty(ref AllocationRequest_IDValue, value); }
        }

       
    }
}
