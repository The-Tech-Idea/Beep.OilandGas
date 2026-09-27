using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class ObtainRegulatoryClosureRequest : ModelEntityBase
    {
        private string InstanceIdValue = string.Empty;

        public string InstanceId

        {

            get { return this.InstanceIdValue; }

            set { SetProperty(ref InstanceIdValue, value); }

        }
    }
}
