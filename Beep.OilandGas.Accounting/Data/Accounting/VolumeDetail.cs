using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Accounting
{
    public partial class VolumeDetail : ModelEntityBase
    {
        private System.DateTime DateValue;
        public System.DateTime Date
        {
            get { return this.DateValue; }
            set { SetProperty(ref DateValue, value); }
        }

        private System.Decimal NetVolumeValue;
        public System.Decimal NetVolume
        {
            get { return this.NetVolumeValue; }
            set { SetProperty(ref NetVolumeValue, value); }
        }

        private System.String? RunTicketNumberValue;
        public System.String? RunTicketNumber
        {
            get { return this.RunTicketNumberValue; }
            set { SetProperty(ref RunTicketNumberValue, value); }
        }
    }
}
