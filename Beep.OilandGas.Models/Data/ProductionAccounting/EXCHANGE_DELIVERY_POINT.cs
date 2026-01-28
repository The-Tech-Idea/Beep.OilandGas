using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EXCHANGE_DELIVERY_POINT : ModelEntityBase {
        private System.String EXCHANGE_DELIVERY_POINT_IDValue;
        public System.String EXCHANGE_DELIVERY_POINT_ID
        {
            get { return this.EXCHANGE_DELIVERY_POINT_IDValue; }
            set { SetProperty(ref EXCHANGE_DELIVERY_POINT_IDValue, value); }
        }

        private System.String EXCHANGE_CONTRACT_IDValue;
        public System.String EXCHANGE_CONTRACT_ID
        {
            get { return this.EXCHANGE_CONTRACT_IDValue; }
            set { SetProperty(ref EXCHANGE_CONTRACT_IDValue, value); }
        }

        private System.String DELIVERY_POINT_NAMEValue;
        public System.String DELIVERY_POINT_NAME
        {
            get { return this.DELIVERY_POINT_NAMEValue; }
            set { SetProperty(ref DELIVERY_POINT_NAMEValue, value); }
        }

        private System.String LOCATIONValue;
        public System.String LOCATION
        {
            get { return this.LOCATIONValue; }
            set { SetProperty(ref LOCATIONValue, value); }
        }

        private System.String IS_RECEIPT_POINTValue;
        public System.String IS_RECEIPT_POINT
        {
            get { return this.IS_RECEIPT_POINTValue; }
            set { SetProperty(ref IS_RECEIPT_POINTValue, value); }
        }

        private System.String IS_DELIVERY_POINTValue;
        public System.String IS_DELIVERY_POINT
        {
            get { return this.IS_DELIVERY_POINTValue; }
            set { SetProperty(ref IS_DELIVERY_POINTValue, value); }
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
