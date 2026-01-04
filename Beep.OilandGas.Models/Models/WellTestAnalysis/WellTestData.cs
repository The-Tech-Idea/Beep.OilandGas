using System.Collections.Generic;

namespace Beep.OilandGas.Models.WellTestAnalysis
{
    /// <summary>
    /// Represents well test data for pressure transient analysis
    /// DTO for calculations - Entity class: WELL_TEST_DATA
    /// </summary>
    public class WellTestData
    {
        /// <summary>
        /// Gets or sets the time points in hours
        /// </summary>
        public List<double> Time { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the pressure points in psi
        /// </summary>
        public List<double> Pressure { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the flow rate in BPD (for drawdown) or last flow rate before shut-in (for build-up)
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the wellbore radius in feet
        /// </summary>
        public double WellboreRadius { get; set; }

        /// <summary>
        /// Gets or sets the formation thickness in feet
        /// </summary>
        public double FormationThickness { get; set; }

        /// <summary>
        /// Gets or sets the porosity (fraction)
        /// </summary>
        public double Porosity { get; set; }

        /// <summary>
        /// Gets or sets the total compressibility in psi^-1
        /// </summary>
        public double TotalCompressibility { get; set; }

        /// <summary>
        /// Gets or sets the oil viscosity in cp
        /// </summary>
        public double OilViscosity { get; set; }

        /// <summary>
        /// Gets or sets the oil formation volume factor in RB/STB
        /// </summary>
        public double OilFormationVolumeFactor { get; set; }

        /// <summary>
        /// Gets or sets the test type (BuildUp or Drawdown)
        /// </summary>
        public WellTestType TestType { get; set; } = WellTestType.BuildUp;

        /// <summary>
        /// Gets or sets the production time before shut-in (for build-up tests) in hours
        /// </summary>
        public double ProductionTime { get; set; }

        /// <summary>
        /// Gets or sets whether this is a gas well test
        /// </summary>
        public bool IsGasWell { get; set; } = false;

        /// <summary>
        /// Gets or sets the gas specific gravity (for gas wells)
        /// </summary>
        public double GasSpecificGravity { get; set; } = 0.65;

        /// <summary>
        /// Gets or sets the reservoir temperature in Fahrenheit
        /// </summary>
        public double ReservoirTemperature { get; set; } = 150.0;

        /// <summary>
        /// Gets or sets the initial reservoir pressure in psi
        /// </summary>
        public double InitialReservoirPressure { get; set; }
    }
}
