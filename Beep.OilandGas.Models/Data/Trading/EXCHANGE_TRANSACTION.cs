using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public partial class EXCHANGE_TRANSACTION : ModelEntityBase
    {
        private System.String EXCHANGE_TRANSACTION_IDValue;
        public System.String EXCHANGE_TRANSACTION_ID
        {
            get { return this.EXCHANGE_TRANSACTION_IDValue; }
            set { SetProperty(ref EXCHANGE_TRANSACTION_IDValue, value); }
        }

        private System.String EXCHANGE_CONTRACT_IDValue;
        public System.String EXCHANGE_CONTRACT_ID
        {
            get { return this.EXCHANGE_CONTRACT_IDValue; }
            set { SetProperty(ref EXCHANGE_CONTRACT_IDValue, value); }
        }

        private System.DateTime? TRANSACTION_DATEValue;
        public System.DateTime? TRANSACTION_DATE
        {
            get { return this.TRANSACTION_DATEValue; }
            set { SetProperty(ref TRANSACTION_DATEValue, value); }
        }

        private System.Decimal  RECEIPT_VOLUMEValue;
        public System.Decimal  RECEIPT_VOLUME
        {
            get { return this.RECEIPT_VOLUMEValue; }
            set { SetProperty(ref RECEIPT_VOLUMEValue, value); }
        }

        private System.Decimal  RECEIPT_PRICEValue;
        public System.Decimal  RECEIPT_PRICE
        {
            get { return this.RECEIPT_PRICEValue; }
            set { SetProperty(ref RECEIPT_PRICEValue, value); }
        }

        private System.String RECEIPT_LOCATIONValue;
        public System.String RECEIPT_LOCATION
        {
            get { return this.RECEIPT_LOCATIONValue; }
            set { SetProperty(ref RECEIPT_LOCATIONValue, value); }
        }

        private System.Decimal  DELIVERY_VOLUMEValue;
        public System.Decimal  DELIVERY_VOLUME
        {
            get { return this.DELIVERY_VOLUMEValue; }
            set { SetProperty(ref DELIVERY_VOLUMEValue, value); }
        }

        private System.Decimal  DELIVERY_PRICEValue;
        public System.Decimal  DELIVERY_PRICE
        {
            get { return this.DELIVERY_PRICEValue; }
            set { SetProperty(ref DELIVERY_PRICEValue, value); }
        }

        private System.String DELIVERY_LOCATIONValue;
        public System.String DELIVERY_LOCATION
        {
            get { return this.DELIVERY_LOCATIONValue; }
            set { SetProperty(ref DELIVERY_LOCATIONValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.String DESCRIPTIONValue;
        public System.String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
