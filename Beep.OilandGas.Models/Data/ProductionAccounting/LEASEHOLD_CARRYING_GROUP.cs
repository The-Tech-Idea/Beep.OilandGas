using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class LEASEHOLD_CARRYING_GROUP : ModelEntityBase {
        private string LEASEHOLD_CARRYING_GROUP_IDValue;
        public string LEASEHOLD_CARRYING_GROUP_ID
        {
            get { return this.LEASEHOLD_CARRYING_GROUP_IDValue; }
            set { SetProperty(ref LEASEHOLD_CARRYING_GROUP_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private string GROUP_NAMEValue;
        public string GROUP_NAME
        {
            get { return this.GROUP_NAMEValue; }
            set { SetProperty(ref GROUP_NAMEValue, value); }
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


