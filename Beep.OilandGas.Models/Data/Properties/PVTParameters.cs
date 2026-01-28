using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class PVTParameters : ModelEntityBase
    {
        private decimal SaturationPressureValue;

        public decimal SaturationPressure

        {

            get { return this.SaturationPressureValue; }

            set { SetProperty(ref SaturationPressureValue, value); }

        }
        private decimal ReservoirTemperatureValue;

        public decimal ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private decimal InitialGORValue;

        public decimal InitialGOR

        {

            get { return this.InitialGORValue; }

            set { SetProperty(ref InitialGORValue, value); }

        }
        private decimal OilDensityValue;

        public decimal OilDensity

        {

            get { return this.OilDensityValue; }

            set { SetProperty(ref OilDensityValue, value); }

        }
        private decimal GasDensityValue;

        public decimal GasDensity

        {

            get { return this.GasDensityValue; }

            set { SetProperty(ref GasDensityValue, value); }

        }
        private decimal WaterDensityValue;

        public decimal WaterDensity

        {

            get { return this.WaterDensityValue; }

            set { SetProperty(ref WaterDensityValue, value); }

        }
        private decimal OilViscosityValue;

        public decimal OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        }
        private decimal GasViscosityValue;

        public decimal GasViscosity

        {

            get { return this.GasViscosityValue; }

            set { SetProperty(ref GasViscosityValue, value); }

        }
        private decimal WaterViscosityValue;

        public decimal WaterViscosity

        {

            get { return this.WaterViscosityValue; }

            set { SetProperty(ref WaterViscosityValue, value); }

        }
        private decimal FormationVolumeFactorValue;

        public decimal FormationVolumeFactor

        {

            get { return this.FormationVolumeFactorValue; }

            set { SetProperty(ref FormationVolumeFactorValue, value); }

        }
        private decimal SolutionGasOilRatioValue;

        public decimal SolutionGasOilRatio

        {

            get { return this.SolutionGasOilRatioValue; }

            set { SetProperty(ref SolutionGasOilRatioValue, value); }

        }
    }
}
