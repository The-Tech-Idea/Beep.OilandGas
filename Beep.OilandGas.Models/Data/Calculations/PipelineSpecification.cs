using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class PipelineSpecification : ModelEntityBase
    {
        private int DiameterValue;

        public int Diameter

        {

            get { return this.DiameterValue; }

            set { SetProperty(ref DiameterValue, value); }

        }
        private double LengthValue;

        public double Length

        {

            get { return this.LengthValue; }

            set { SetProperty(ref LengthValue, value); }

        }
        private int PressureValue;

        public int Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private string MaterialValue;

        public string Material

        {

            get { return this.MaterialValue; }

            set { SetProperty(ref MaterialValue, value); }

        }
        private double CostPerKmValue;

        public double CostPerKm

        {

            get { return this.CostPerKmValue; }

            set { SetProperty(ref CostPerKmValue, value); }

        }
    }
}
