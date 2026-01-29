using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class PROVED_PROPERTY : ModelEntityBase
    {
        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.Decimal  ACQUISITION_COSTValue;
        public System.Decimal  ACQUISITION_COST
        {
            get { return this.ACQUISITION_COSTValue; }
            set { SetProperty(ref ACQUISITION_COSTValue, value); }
        }

        private System.Decimal  EXPLORATION_COSTSValue;
        public System.Decimal  EXPLORATION_COSTS
        {
            get { return this.EXPLORATION_COSTSValue; }
            set { SetProperty(ref EXPLORATION_COSTSValue, value); }
        }

        private System.Decimal  DEVELOPMENT_COSTSValue;
        public System.Decimal  DEVELOPMENT_COSTS
        {
            get { return this.DEVELOPMENT_COSTSValue; }
            set { SetProperty(ref DEVELOPMENT_COSTSValue, value); }
        }

        private System.Decimal  ACCUMULATED_AMORTIZATIONValue;
        public System.Decimal  ACCUMULATED_AMORTIZATION
        {
            get { return this.ACCUMULATED_AMORTIZATIONValue; }
            set { SetProperty(ref ACCUMULATED_AMORTIZATIONValue, value); }
        }

        private System.DateTime? PROVED_DATEValue;
        public System.DateTime? PROVED_DATE
        {
            get { return this.PROVED_DATEValue; }
            set { SetProperty(ref PROVED_DATEValue, value); }
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

        private System.Decimal PROVED_RESERVES_GASValue;
        public System.Decimal PROVED_RESERVES_GAS
        {
            get { return this.PROVED_RESERVES_GASValue; }
            set { SetProperty(ref PROVED_RESERVES_GASValue, value); }
        }

        private System.Decimal PROVED_RESERVES_OILValue;
        public System.Decimal PROVED_RESERVES_OIL
        {
            get { return this.PROVED_RESERVES_OILValue; }
            set { SetProperty(ref PROVED_RESERVES_OILValue, value); }
        }

        private object PROVED_RESERVES_BOEValue;
        public object PROVED_RESERVES_BOE
        {
            get { return this.PROVED_RESERVES_BOEValue; }
            set { SetProperty(ref PROVED_RESERVES_BOEValue, value); }
        }
    }
}
