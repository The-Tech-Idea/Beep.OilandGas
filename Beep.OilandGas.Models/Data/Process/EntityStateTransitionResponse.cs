using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Validation;
using Beep.OilandGas.Models.Data.Process;

namespace Beep.OilandGas.Models.Data
{
    public class EntityStateTransitionResponse : ModelEntityBase
    {
        private string EntityIdValue = string.Empty;

        public string EntityId

        {

            get { return this.EntityIdValue; }

            set { SetProperty(ref EntityIdValue, value); }

        }
        private string EntityTypeValue = string.Empty;

        public string EntityType

        {

            get { return this.EntityTypeValue; }

            set { SetProperty(ref EntityTypeValue, value); }

        }
        private string PreviousStateValue = string.Empty;

        public string PreviousState

        {

            get { return this.PreviousStateValue; }

            set { SetProperty(ref PreviousStateValue, value); }

        }
        private string NewStateValue = string.Empty;

        public string NewState

        {

            get { return this.NewStateValue; }

            set { SetProperty(ref NewStateValue, value); }

        }
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private string MessageValue = string.Empty;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
        private List<string> AvailableTransitionsValue = new List<string>();

        public List<string> AvailableTransitions

        {

            get { return this.AvailableTransitionsValue; }

            set { SetProperty(ref AvailableTransitionsValue, value); }

        }
    }
}
