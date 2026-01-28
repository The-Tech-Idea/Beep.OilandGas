using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ProcessingFacilitySpecification : ModelEntityBase
    {
        private double DesignCapacityValue;

        public double DesignCapacity

        {

            get { return this.DesignCapacityValue; }

            set { SetProperty(ref DesignCapacityValue, value); }

        }
        private double EfficiencyValue;

        public double Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        }
        private double CostPerUnitValue;

        public double CostPerUnit

        {

            get { return this.CostPerUnitValue; }

            set { SetProperty(ref CostPerUnitValue, value); }

        }
    }
}
