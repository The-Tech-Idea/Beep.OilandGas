using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ConflictResolutionResult : ModelEntityBase
    {
        private bool SuccessValue;

        public bool Success

        {

            get { return this.SuccessValue; }

            set { SetProperty(ref SuccessValue, value); }

        }
        private object ResolvedEntityValue;

        public object ResolvedEntity

        {

            get { return this.ResolvedEntityValue; }

            set { SetProperty(ref ResolvedEntityValue, value); }

        }
        private ConflictResolutionStrategy StrategyUsedValue;

        public ConflictResolutionStrategy StrategyUsed

        {

            get { return this.StrategyUsedValue; }

            set { SetProperty(ref StrategyUsedValue, value); }

        }
        private string MessageValue;

        public string Message

        {

            get { return this.MessageValue; }

            set { SetProperty(ref MessageValue, value); }

        }
    }
}
