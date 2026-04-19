using System;

namespace Beep.OilandGas.Models.Data
{
    public class ProcessTransitionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success
        {
            get { return this.SuccessValue; }
            set { SetProperty(ref SuccessValue, value); }
        }

        private string InstanceIdValue = string.Empty;

        public string InstanceId
        {
            get { return this.InstanceIdValue; }
            set { SetProperty(ref InstanceIdValue, value); }
        }

        private string TransitionNameValue = string.Empty;

        public string TransitionName
        {
            get { return this.TransitionNameValue; }
            set { SetProperty(ref TransitionNameValue, value); }
        }

        private string FromStateValue = string.Empty;

        public string FromState
        {
            get { return this.FromStateValue; }
            set { SetProperty(ref FromStateValue, value); }
        }

        private string NewStepIdValue = string.Empty;

        public string NewStepId
        {
            get { return this.NewStepIdValue; }
            set { SetProperty(ref NewStepIdValue, value); }
        }

        private string MessageValue = string.Empty;

        public string Message
        {
            get { return this.MessageValue; }
            set { SetProperty(ref MessageValue, value); }
        }

        private DateTime TransitionedAtValue = DateTime.UtcNow;

        public DateTime TransitionedAt
        {
            get { return this.TransitionedAtValue; }
            set { SetProperty(ref TransitionedAtValue, value); }
        }
    }
}
