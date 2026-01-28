using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class AccessCheckResponse : ModelEntityBase
    {
        private bool HasAccessValue;

        public bool HasAccess

        {

            get { return this.HasAccessValue; }

            set { SetProperty(ref HasAccessValue, value); }

        }
        private string? AccessLevelValue;

        public string? AccessLevel

        {

            get { return this.AccessLevelValue; }

            set { SetProperty(ref AccessLevelValue, value); }

        }
        private string? ReasonValue;

        public string? Reason

        {

            get { return this.ReasonValue; }

            set { SetProperty(ref ReasonValue, value); }

        }
    }
}
