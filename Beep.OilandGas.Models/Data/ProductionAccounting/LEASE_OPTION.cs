using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASE_OPTION : ModelEntityBase {
        private string LEASE_OPTION_IDValue;
        public string LEASE_OPTION_ID
        {
            get { return this.LEASE_OPTION_IDValue; }
            set { SetProperty(ref LEASE_OPTION_IDValue, value); }
        }

        private string LEASE_IDValue;
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private DateTime? OPTION_DATEValue;
        public DateTime? OPTION_DATE
        {
            get { return this.OPTION_DATEValue; }
            set { SetProperty(ref OPTION_DATEValue, value); }
        }

        private DateTime? OPTION_EXPIRY_DATEValue;
        public DateTime? OPTION_EXPIRY_DATE
        {
            get { return this.OPTION_EXPIRY_DATEValue; }
            set { SetProperty(ref OPTION_EXPIRY_DATEValue, value); }
        }

        private decimal? BONUS_AMOUNTValue;
        public decimal? BONUS_AMOUNT
        {
            get { return this.BONUS_AMOUNTValue; }
            set { SetProperty(ref BONUS_AMOUNTValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


