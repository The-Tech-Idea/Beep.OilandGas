using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ConditionAssessment : ModelEntityBase
    {
        private string PumpIdValue = string.Empty;

        public string PumpId

        {

            get { return this.PumpIdValue; }

            set { SetProperty(ref PumpIdValue, value); }

        }
        private string ConditionRatingValue = string.Empty;

        public string ConditionRating

        {

            get { return this.ConditionRatingValue; }

            set { SetProperty(ref ConditionRatingValue, value); }

        }
    }
}
