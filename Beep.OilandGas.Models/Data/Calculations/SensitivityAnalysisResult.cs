using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class SensitivityAnalysisResult : ModelEntityBase
    {
        private List<double> PriceVariationValue;

        public List<double> PriceVariation

        {

            get { return this.PriceVariationValue; }

            set { SetProperty(ref PriceVariationValue, value); }

        }
        private List<double> VolumeVariationValue;

        public List<double> VolumeVariation

        {

            get { return this.VolumeVariationValue; }

            set { SetProperty(ref VolumeVariationValue, value); }

        }
        private List<double> CostVariationValue;

        public List<double> CostVariation

        {

            get { return this.CostVariationValue; }

            set { SetProperty(ref CostVariationValue, value); }

        }
    }
}
