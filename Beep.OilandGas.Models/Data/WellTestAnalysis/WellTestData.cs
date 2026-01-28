using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class WellTestData : ModelEntityBase
    {
        /// <summary>
        /// Gets or sets the time points in hours
        /// </summary>
        private List<double> TimeValue = new List<double>();

        public List<double> Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        }

        /// <summary>
        /// Gets or sets the pressure points in psi
        /// </summary>
        private List<double> PressureValue = new List<double>();

        public List<double> Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }

        /// <summary>
        /// Gets or sets the flow rate in BPD (for drawdown) or last flow rate before shut-in (for build-up)
        /// </summary>
        private double FlowRateValue;

        public double FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }

        /// <summary>
        /// Gets or sets the wellbore radius in feet
        /// </summary>
        private double WellboreRadiusValue;

        public double WellboreRadius

        {

            get { return this.WellboreRadiusValue; }

            set { SetProperty(ref WellboreRadiusValue, value); }

        }

        /// <summary>
        /// Gets or sets the formation thickness in feet
        /// </summary>
        private double FormationThicknessValue;

        public double FormationThickness

        {

            get { return this.FormationThicknessValue; }

            set { SetProperty(ref FormationThicknessValue, value); }

        }

        /// <summary>
        /// Gets or sets the porosity (fraction)
        /// </summary>
        private double PorosityValue;

        public double Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }

        /// <summary>
        /// Gets or sets the total compressibility in psi^-1
        /// </summary>
        private double TotalCompressibilityValue;

        public double TotalCompressibility

        {

            get { return this.TotalCompressibilityValue; }

            set { SetProperty(ref TotalCompressibilityValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil viscosity in cp
        /// </summary>
        private double OilViscosityValue;

        public double OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        }

        /// <summary>
        /// Gets or sets the oil formation volume factor in RB/STB
        /// </summary>
        private double OilFormationVolumeFactorValue;

        public double OilFormationVolumeFactor

        {

            get { return this.OilFormationVolumeFactorValue; }

            set { SetProperty(ref OilFormationVolumeFactorValue, value); }

        }

        /// <summary>
        /// Gets or sets the test type (BuildUp or Drawdown)
        /// </summary>
        private WellTestType TestTypeValue = WellTestType.BuildUp;

        public WellTestType TestType

        {

            get { return this.TestTypeValue; }

            set { SetProperty(ref TestTypeValue, value); }

        }

        /// <summary>
        /// Gets or sets the production time before shut-in (for build-up tests) in hours
        /// </summary>
        private double ProductionTimeValue;

        public double ProductionTime

        {

            get { return this.ProductionTimeValue; }

            set { SetProperty(ref ProductionTimeValue, value); }

        }

        /// <summary>
        /// Gets or sets whether this is a gas well test
        /// </summary>
        private bool IsGasWellValue = false;

        public bool IsGasWell

        {

            get { return this.IsGasWellValue; }

            set { SetProperty(ref IsGasWellValue, value); }

        }

        /// <summary>
        /// Gets or sets the gas specific gravity (for gas wells)
        /// </summary>
        private double GasSpecificGravityValue = 0.65;

        public double GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }

        /// <summary>
        /// Gets or sets the reservoir temperature in Fahrenheit
        /// </summary>
        private double ReservoirTemperatureValue = 150.0;

        public double ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }

        /// <summary>
        /// Gets or sets the initial reservoir pressure in psi
        /// </summary>
        private double InitialReservoirPressureValue;

        public double InitialReservoirPressure

        {

            get { return this.InitialReservoirPressureValue; }

            set { SetProperty(ref InitialReservoirPressureValue, value); }

        }
    }
}
