using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASE_EXPIRY_EVENT : ModelEntityBase {
        private string LEASE_EXPIRY_EVENT_IDValue;
        public string LEASE_EXPIRY_EVENT_ID
        {
            get { return this.LEASE_EXPIRY_EVENT_IDValue; }
            set { SetProperty(ref LEASE_EXPIRY_EVENT_IDValue, value); }
        }

        private string LEASE_IDValue;
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private string ACTION_TAKENValue;
        public string ACTION_TAKEN
        {
            get { return this.ACTION_TAKENValue; }
            set { SetProperty(ref ACTION_TAKENValue, value); }
        }

        private string NOTESValue;
        public string NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
