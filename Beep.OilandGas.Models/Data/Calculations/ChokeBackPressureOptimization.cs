using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeBackPressureOptimization : ModelEntityBase
    {
        private string OptimizationIdValue = string.Empty;

        public string OptimizationId

        {

            get { return this.OptimizationIdValue; }

            set { SetProperty(ref OptimizationIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private decimal CurrentChokeDiameterValue;

        public decimal CurrentChokeDiameter

        {

            get { return this.CurrentChokeDiameterValue; }

            set { SetProperty(ref CurrentChokeDiameterValue, value); }

        }
        private decimal CurrentBackPressureValue;

        public decimal CurrentBackPressure

        {

            get { return this.CurrentBackPressureValue; }

            set { SetProperty(ref CurrentBackPressureValue, value); }

        }
        private decimal CurrentProductionRateValue;

        public decimal CurrentProductionRate

        {

            get { return this.CurrentProductionRateValue; }

            set { SetProperty(ref CurrentProductionRateValue, value); }

        }
        private decimal OptimalChokeDiameterValue;

        public decimal OptimalChokeDiameter

        {

            get { return this.OptimalChokeDiameterValue; }

            set { SetProperty(ref OptimalChokeDiameterValue, value); }

        }
        private decimal OptimalBackPressureValue;

        public decimal OptimalBackPressure

        {

            get { return this.OptimalBackPressureValue; }

            set { SetProperty(ref OptimalBackPressureValue, value); }

        }
        private decimal OptimalProductionRateValue;

        public decimal OptimalProductionRate

        {

            get { return this.OptimalProductionRateValue; }

            set { SetProperty(ref OptimalProductionRateValue, value); }

        }
        private decimal ProductionIncreaseValue;

        public decimal ProductionIncrease

        {

            get { return this.ProductionIncreaseValue; }

            set { SetProperty(ref ProductionIncreaseValue, value); }

        } // percent
        private decimal PressureDropReductionValue;

        public decimal PressureDropReduction

        {

            get { return this.PressureDropReductionValue; }

            set { SetProperty(ref PressureDropReductionValue, value); }

        } // psi
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private string OptimizationStrategyValue = string.Empty;

        public string OptimizationStrategy

        {

            get { return this.OptimizationStrategyValue; }

            set { SetProperty(ref OptimizationStrategyValue, value); }

        } // Maximize Production, Minimize Backpressure, etc.
        private List<ChokeOpeningPoint> OpeningCurveValue = new();

        public List<ChokeOpeningPoint> OpeningCurve

        {

            get { return this.OpeningCurveValue; }

            set { SetProperty(ref OpeningCurveValue, value); }

        }
    }
}
