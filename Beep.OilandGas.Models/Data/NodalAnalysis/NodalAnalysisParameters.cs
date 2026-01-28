using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
    public class NodalAnalysisParameters : ModelEntityBase
    {
        private ReservoirProperties ReservoirPropertiesValue = new();

        public ReservoirProperties ReservoirProperties

        {

            get { return this.ReservoirPropertiesValue; }

            set { SetProperty(ref ReservoirPropertiesValue, value); }

        }
        private WellboreProperties WellborePropertiesValue = new();

        public WellboreProperties WellboreProperties

        {

            get { return this.WellborePropertiesValue; }

            set { SetProperty(ref WellborePropertiesValue, value); }

        }
        private decimal MinFlowRateValue;

        public decimal MinFlowRate

        {

            get { return this.MinFlowRateValue; }

            set { SetProperty(ref MinFlowRateValue, value); }

        }
        private decimal MaxFlowRateValue;

        public decimal MaxFlowRate

        {

            get { return this.MaxFlowRateValue; }

            set { SetProperty(ref MaxFlowRateValue, value); }

        }
        private int NumberOfPointsValue = 50;

        public int NumberOfPoints

        {

            get { return this.NumberOfPointsValue; }

            set { SetProperty(ref NumberOfPointsValue, value); }

        }
    }
}
