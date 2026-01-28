using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CONTRACT_PERFORMANCE_OBLIGATION : ModelEntityBase {
        private System.String CONTRACT_PERFORMANCE_OBLIGATION_IDValue;
        public System.String CONTRACT_PERFORMANCE_OBLIGATION_ID
        {
            get { return this.CONTRACT_PERFORMANCE_OBLIGATION_IDValue; }
            set { SetProperty(ref CONTRACT_PERFORMANCE_OBLIGATION_IDValue, value); }
        }

        private System.String SALES_CONTRACT_IDValue;
        public System.String SALES_CONTRACT_ID
        {
            get { return this.SALES_CONTRACT_IDValue; }
            set { SetProperty(ref SALES_CONTRACT_IDValue, value); }
        }

        private System.String OBLIGATION_TYPEValue;
        public System.String OBLIGATION_TYPE
        {
            get { return this.OBLIGATION_TYPEValue; }
            set { SetProperty(ref OBLIGATION_TYPEValue, value); }
        }

        private System.String OBLIGATION_DESCRIPTIONValue;
        public System.String OBLIGATION_DESCRIPTION
        {
            get { return this.OBLIGATION_DESCRIPTIONValue; }
            set { SetProperty(ref OBLIGATION_DESCRIPTIONValue, value); }
        }

        private System.Decimal? ALLOCATED_PRICEValue;
        public System.Decimal? ALLOCATED_PRICE
        {
            get { return this.ALLOCATED_PRICEValue; }
            set { SetProperty(ref ALLOCATED_PRICEValue, value); }
        }

        private System.DateTime? SATISFIED_DATEValue;
        public System.DateTime? SATISFIED_DATE
        {
            get { return this.SATISFIED_DATEValue; }
            set { SetProperty(ref SATISFIED_DATEValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
