using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class SensitivityAnalysisRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private List<string> ParametersValue = new();

        public List<string> Parameters

        {

            get { return this.ParametersValue; }

            set { SetProperty(ref ParametersValue, value); }

        }
        private int NumberOfIterationsValue = 100;

        public int NumberOfIterations

        {

            get { return this.NumberOfIterationsValue; }

            set { SetProperty(ref NumberOfIterationsValue, value); }

        }
    }
}
