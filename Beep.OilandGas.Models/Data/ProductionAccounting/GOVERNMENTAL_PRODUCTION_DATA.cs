using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class GOVERNMENTAL_PRODUCTION_DATA : ModelEntityBase {
        private System.String GOVERNMENTAL_PRODUCTION_DATA_IDValue;
        public System.String GOVERNMENTAL_PRODUCTION_DATA_ID
        {
            get { return this.GOVERNMENTAL_PRODUCTION_DATA_IDValue; }
            set { SetProperty(ref GOVERNMENTAL_PRODUCTION_DATA_IDValue, value); }
        }

        private System.Decimal? OIL_PRODUCTIONValue;
        public System.Decimal? OIL_PRODUCTION
        {
            get { return this.OIL_PRODUCTIONValue; }
            set { SetProperty(ref OIL_PRODUCTIONValue, value); }
        }

        private System.Decimal? GAS_PRODUCTIONValue;
        public System.Decimal? GAS_PRODUCTION
        {
            get { return this.GAS_PRODUCTIONValue; }
            set { SetProperty(ref GAS_PRODUCTIONValue, value); }
        }

        private System.Decimal? WATER_PRODUCTIONValue;
        public System.Decimal? WATER_PRODUCTION
        {
            get { return this.WATER_PRODUCTIONValue; }
            set { SetProperty(ref WATER_PRODUCTIONValue, value); }
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
