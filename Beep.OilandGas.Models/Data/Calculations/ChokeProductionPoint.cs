using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeProductionPoint : ModelEntityBase
    {
        private int MonthValue;

        public int Month

        {

            get { return this.MonthValue; }

            set { SetProperty(ref MonthValue, value); }

        }
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal NaturalProductionValue;

        public decimal NaturalProduction

        {

            get { return this.NaturalProductionValue; }

            set { SetProperty(ref NaturalProductionValue, value); }

        }
        private decimal ProductionWithCurrentChokeValue;

        public decimal ProductionWithCurrentChoke

        {

            get { return this.ProductionWithCurrentChokeValue; }

            set { SetProperty(ref ProductionWithCurrentChokeValue, value); }

        }
        private decimal RecommendedChokeDiameterValue;

        public decimal RecommendedChokeDiameter

        {

            get { return this.RecommendedChokeDiameterValue; }

            set { SetProperty(ref RecommendedChokeDiameterValue, value); }

        }
        private decimal ProductionWithOptimalChokeValue;

        public decimal ProductionWithOptimalChoke

        {

            get { return this.ProductionWithOptimalChokeValue; }

            set { SetProperty(ref ProductionWithOptimalChokeValue, value); }

        }
    }
}
