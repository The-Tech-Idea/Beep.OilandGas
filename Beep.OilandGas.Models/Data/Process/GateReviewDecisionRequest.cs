using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public enum GateDecision
    {
        Approve,
        Reject,
        Defer
    }

    public class GateReviewDecisionRequest : ModelEntityBase
    {
        private GateDecision DecisionValue = GateDecision.Approve;

        public GateDecision Decision
        {
            get { return this.DecisionValue; }
            set { SetProperty(ref DecisionValue, value); }
        }

        private string CommentsValue = string.Empty;

        public string Comments
        {
            get { return this.CommentsValue; }
            set { SetProperty(ref CommentsValue, value); }
        }

        private DateTime? DeferTargetDateValue;

        public DateTime? DeferTargetDate
        {
            get { return this.DeferTargetDateValue; }
            set { SetProperty(ref DeferTargetDateValue, value); }
        }

        private List<string> ConditionsValue = new();

        public List<string> Conditions
        {
            get { return this.ConditionsValue; }
            set { SetProperty(ref ConditionsValue, value); }
        }
    }
}
