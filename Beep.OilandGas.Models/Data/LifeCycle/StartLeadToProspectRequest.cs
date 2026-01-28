using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.LifeCycle
{
    public class StartLeadToProspectRequest : ModelEntityBase
    {
        private string LeadIdValue = string.Empty;

        public string LeadId

        {

            get { return this.LeadIdValue; }

            set { SetProperty(ref LeadIdValue, value); }

        }
        private string UserIdValue = string.Empty;

        public string UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
