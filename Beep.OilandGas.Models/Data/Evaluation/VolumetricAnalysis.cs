using Beep.OilandGas.Models.Data.ProspectIdentification;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class VolumetricAnalysis : ModelEntityBase
    {
        private string ProspectIdValue = string.Empty;

        public string ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private DateTime AnalysisDateValue = DateTime.UtcNow;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal EstimatedOilResourcesValue;

        public decimal EstimatedOilResources

        {

            get { return this.EstimatedOilResourcesValue; }

            set { SetProperty(ref EstimatedOilResourcesValue, value); }

        }
        private decimal EstimatedGasResourcesValue;

        public decimal EstimatedGasResources

        {

            get { return this.EstimatedGasResourcesValue; }

            set { SetProperty(ref EstimatedGasResourcesValue, value); }

        }
        private string ResourceUnitValue = "STB";

        public string ResourceUnit

        {

            get { return this.ResourceUnitValue; }

            set { SetProperty(ref ResourceUnitValue, value); }

        }
    }
}
