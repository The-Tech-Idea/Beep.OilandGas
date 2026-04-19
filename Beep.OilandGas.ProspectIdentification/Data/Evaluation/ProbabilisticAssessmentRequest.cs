using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ProbabilisticAssessmentRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private int NumberOfSimulationsValue = 10000;

        public int NumberOfSimulations

        {

            get { return this.NumberOfSimulationsValue; }

            set { SetProperty(ref NumberOfSimulationsValue, value); }

        }
    }
}
