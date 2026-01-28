using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class StartDiscoveryToDevelopmentRequest : ModelEntityBase
    {
        private string DiscoveryIdValue = string.Empty;

        public string DiscoveryId

        {

            get { return this.DiscoveryIdValue; }

            set { SetProperty(ref DiscoveryIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
