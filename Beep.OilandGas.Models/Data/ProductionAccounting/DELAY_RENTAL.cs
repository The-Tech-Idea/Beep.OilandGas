using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class DELAY_RENTAL : ModelEntityBase {
        private string DELAY_RENTAL_IDValue;
        public string DELAY_RENTAL_ID
        {
            get { return this.DELAY_RENTAL_IDValue; }
            set { SetProperty(ref DELAY_RENTAL_IDValue, value); }
        }

        private string LEASE_IDValue;
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private DateTime? RENTAL_DATEValue;
        public DateTime? RENTAL_DATE
        {
            get { return this.RENTAL_DATEValue; }
            set { SetProperty(ref RENTAL_DATEValue, value); }
        }

        private decimal? AMOUNTValue;
        public decimal? AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private DateTime? NEXT_DUE_DATEValue;
        public DateTime? NEXT_DUE_DATE
        {
            get { return this.NEXT_DUE_DATEValue; }
            set { SetProperty(ref NEXT_DUE_DATEValue, value); }
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


