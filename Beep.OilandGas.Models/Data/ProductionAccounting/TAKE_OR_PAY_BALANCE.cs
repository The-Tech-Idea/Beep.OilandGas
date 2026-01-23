using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TAKE_OR_PAY_BALANCE : ModelEntityBase {
        private string TAKE_OR_PAY_BALANCE_IDValue;
        public string TAKE_OR_PAY_BALANCE_ID
        {
            get { return this.TAKE_OR_PAY_BALANCE_IDValue; }
            set { SetProperty(ref TAKE_OR_PAY_BALANCE_IDValue, value); }
        }

        private string SALES_CONTRACT_IDValue;
        public string SALES_CONTRACT_ID
        {
            get { return this.SALES_CONTRACT_IDValue; }
            set { SetProperty(ref SALES_CONTRACT_IDValue, value); }
        }

        private decimal? BALANCE_VOLUMEValue;
        public decimal? BALANCE_VOLUME
        {
            get { return this.BALANCE_VOLUMEValue; }
            set { SetProperty(ref BALANCE_VOLUMEValue, value); }
        }

        private DateTime? LAST_UPDATED_DATEValue;
        public DateTime? LAST_UPDATED_DATE
        {
            get { return this.LAST_UPDATED_DATEValue; }
            set { SetProperty(ref LAST_UPDATED_DATEValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


