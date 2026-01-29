using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AVAILS_ALLOCATION : ModelEntityBase {
        private System.String AVAILS_ALLOCATION_IDValue;
        public System.String AVAILS_ALLOCATION_ID
        {
            get { return this.AVAILS_ALLOCATION_IDValue; }
            set { SetProperty(ref AVAILS_ALLOCATION_IDValue, value); }
        }

        private System.String PRODUCTION_AVAILS_IDValue;
        public System.String PRODUCTION_AVAILS_ID
        {
            get { return this.PRODUCTION_AVAILS_IDValue; }
            set { SetProperty(ref PRODUCTION_AVAILS_IDValue, value); }
        }

        private System.String ENTITY_IDValue;
        public System.String ENTITY_ID
        {
            get { return this.ENTITY_IDValue; }
            set { SetProperty(ref ENTITY_IDValue, value); }
        }

        private System.Decimal  ALLOCATED_VOLUMEValue;
        public System.Decimal  ALLOCATED_VOLUME
        {
            get { return this.ALLOCATED_VOLUMEValue; }
            set { SetProperty(ref ALLOCATED_VOLUMEValue, value); }
        }

        private System.Decimal  ALLOCATION_PERCENTAGEValue;
        public System.Decimal  ALLOCATION_PERCENTAGE
        {
            get { return this.ALLOCATION_PERCENTAGEValue; }
            set { SetProperty(ref ALLOCATION_PERCENTAGEValue, value); }
        }

        // Standard PPDM columns

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

       
    }
}
