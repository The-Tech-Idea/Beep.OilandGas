using System;
using System.Collections.Generic;
using System.Linq;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Trading
{
    public partial class ExchangeNetPosition : ModelEntityBase
    {
        private System.Decimal NetVolumeValue;
        public System.Decimal NetVolume
        {
            get { return this.NetVolumeValue; }
            set { SetProperty(ref NetVolumeValue, value); }
        }

        private System.Decimal NetValueValue;
        public System.Decimal NetValue
        {
            get { return this.NetValueValue; }
            set { SetProperty(ref NetValueValue, value); }
        }

        public bool IsLong => NetVolume > 0;
        public bool IsShort => NetVolume < 0;
        public bool IsFlat => NetVolume == 0;
    }
}
