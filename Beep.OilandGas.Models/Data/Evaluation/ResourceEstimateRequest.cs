using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class ResourceEstimateRequest : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string EstimationMethodologyValue = "Volumetric";

        public string EstimationMethodology

        {

            get { return this.EstimationMethodologyValue; }

            set { SetProperty(ref EstimationMethodologyValue, value); }

        }
    }
}
