using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EXCHANGE_PARTY : ModelEntityBase {
        private System.String EXCHANGE_PARTY_IDValue;
        public System.String EXCHANGE_PARTY_ID
        {
            get { return this.EXCHANGE_PARTY_IDValue; }
            set { SetProperty(ref EXCHANGE_PARTY_IDValue, value); }
        }

        private System.String EXCHANGE_CONTRACT_IDValue;
        public System.String EXCHANGE_CONTRACT_ID
        {
            get { return this.EXCHANGE_CONTRACT_IDValue; }
            set { SetProperty(ref EXCHANGE_CONTRACT_IDValue, value); }
        }

        private System.String PARTY_NAMEValue;
        public System.String PARTY_NAME
        {
            get { return this.PARTY_NAMEValue; }
            set { SetProperty(ref PARTY_NAMEValue, value); }
        }

        private System.String IS_INITIATORValue;
        public System.String IS_INITIATOR
        {
            get { return this.IS_INITIATORValue; }
            set { SetProperty(ref IS_INITIATORValue, value); }
        }

        private System.String ROLEValue;
        public System.String ROLE
        {
            get { return this.ROLEValue; }
            set { SetProperty(ref ROLEValue, value); }
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
