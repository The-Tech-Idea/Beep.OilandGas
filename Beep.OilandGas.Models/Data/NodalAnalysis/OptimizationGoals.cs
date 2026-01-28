using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
    public class OptimizationGoals : ModelEntityBase
    {
        private string OptimizationTypeValue = "MaximizeProduction";

        public string OptimizationType

        {

            get { return this.OptimizationTypeValue; }

            set { SetProperty(ref OptimizationTypeValue, value); }

        } // MaximizeProduction, MinimizePressure, OptimizeEfficiency
        private decimal? TargetFlowRateValue;

        public decimal? TargetFlowRate

        {

            get { return this.TargetFlowRateValue; }

            set { SetProperty(ref TargetFlowRateValue, value); }

        }
        private decimal? TargetBottomholePressureValue;

        public decimal? TargetBottomholePressure

        {

            get { return this.TargetBottomholePressureValue; }

            set { SetProperty(ref TargetBottomholePressureValue, value); }

        }
        public Dictionary<string, object> Constraints { get; set; } = new();
    }
}
