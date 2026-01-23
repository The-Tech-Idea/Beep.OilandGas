using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class GOVERNMENTAL_ROYALTY_DATA : ModelEntityBase {
        private System.String GOVERNMENTAL_ROYALTY_DATA_IDValue;
        public System.String GOVERNMENTAL_ROYALTY_DATA_ID
        {
            get { return this.GOVERNMENTAL_ROYALTY_DATA_IDValue; }
            set { SetProperty(ref GOVERNMENTAL_ROYALTY_DATA_IDValue, value); }
        }

        private System.Decimal? ROYALTY_VOLUMEValue;
        public System.Decimal? ROYALTY_VOLUME
        {
            get { return this.ROYALTY_VOLUMEValue; }
            set { SetProperty(ref ROYALTY_VOLUMEValue, value); }
        }

        private System.Decimal? ROYALTY_VALUEValue;
        public System.Decimal? ROYALTY_VALUE
        {
            get { return this.ROYALTY_VALUEValue; }
            set { SetProperty(ref ROYALTY_VALUEValue, value); }
        }

        private System.Decimal? ROYALTY_RATEValue;
        public System.Decimal? ROYALTY_RATE
        {
            get { return this.ROYALTY_RATEValue; }
            set { SetProperty(ref ROYALTY_RATEValue, value); }
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


