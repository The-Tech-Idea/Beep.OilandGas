using System;
using System.Collections.Generic;

namespace Beep.OilandGas.WellTestAnalysis.Models
{
    /// <summary>
    /// Represents well test data for pressure transient analysis.
    /// </summary>
    public class WellTestData
    {
        /// <summary>
        /// Gets or sets the time points in hours.
        /// </summary>
        public List<double> Time { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the pressure points in psi.
        /// </summary>
        public List<double> Pressure { get; set; } = new List<double>();

        /// <summary>
        /// Gets or sets the flow rate in BPD (for drawdown) or last flow rate before shut-in (for build-up).
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the wellbore radius in feet.
        /// </summary>
        public double WellboreRadius { get; set; }

        /// <summary>
        /// Gets or sets the formation thickness in feet.
        /// </summary>
        public double FormationThickness { get; set; }

        /// <summary>
        /// Gets or sets the porosity (fraction).
        /// </summary>
        public double Porosity { get; set; }

        /// <summary>
        /// Gets or sets the total compressibility in psi^-1.
        /// </summary>
        public double TotalCompressibility { get; set; }

        /// <summary>
        /// Gets or sets the oil viscosity in cp.
        /// </summary>
        public double OilViscosity { get; set; }

        /// <summary>
        /// Gets or sets the oil formation volume factor in RB/STB.
        /// </summary>
        public double OilFormationVolumeFactor { get; set; }

        /// <summary>
        /// Gets or sets the test type (BuildUp or Drawdown).
        /// </summary>
        public WellTestType TestType { get; set; } = WellTestType.BuildUp;

        /// <summary>
        /// Gets or sets the production time before shut-in (for build-up tests) in hours.
        /// </summary>
        public double ProductionTime { get; set; }

        /// <summary>
        /// Gets or sets whether this is a gas well test.
        /// </summary>
        public bool IsGasWell { get; set; } = false;

        /// <summary>
        /// Gets or sets the gas specific gravity (for gas wells).
        /// </summary>
        public double GasSpecificGravity { get; set; } = 0.65;

        /// <summary>
        /// Gets or sets the reservoir temperature in Fahrenheit.
        /// </summary>
        public double ReservoirTemperature { get; set; } = 150.0;

        /// <summary>
        /// Gets or sets the initial reservoir pressure in psi.
        /// </summary>
        public double InitialReservoirPressure { get; set; }
    }

    /// <summary>
    /// Well test type enumeration.
    /// </summary>
    public enum WellTestType
    {
        /// <summary>
        /// Pressure build-up test.
        /// </summary>
        BuildUp,

        /// <summary>
        /// Pressure drawdown test.
        /// </summary>
        Drawdown
    }

    /// <summary>
    /// Represents a point on a pressure-time curve.
    /// </summary>
    public class PressureTimePoint
    {
        /// <summary>
        /// Gets or sets the time in hours.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// Gets or sets the pressure in psi.
        /// </summary>
        public double Pressure { get; set; }

        /// <summary>
        /// Gets or sets the pressure derivative (for diagnostic plots).
        /// </summary>
        public double? PressureDerivative { get; set; }

        public PressureTimePoint() { }

        public PressureTimePoint(double time, double pressure)
        {
            Time = time;
            Pressure = pressure;
        }
    }

    /// <summary>
    /// Represents the result of a well test analysis.
    /// </summary>
    public class WellTestAnalysisResult
    {
        /// <summary>
        /// Gets or sets the calculated permeability in md.
        /// </summary>
        public double Permeability { get; set; }

        /// <summary>
        /// Gets or sets the skin factor (dimensionless).
        /// </summary>
        public double SkinFactor { get; set; }

        /// <summary>
        /// Gets or sets the reservoir pressure in psi.
        /// </summary>
        public double ReservoirPressure { get; set; }

        /// <summary>
        /// Gets or sets the productivity index in BPD/psi.
        /// </summary>
        public double ProductivityIndex { get; set; }

        /// <summary>
        /// Gets or sets the flow efficiency (dimensionless).
        /// </summary>
        public double FlowEfficiency { get; set; }

        /// <summary>
        /// Gets or sets the damage ratio (dimensionless).
        /// </summary>
        public double DamageRatio { get; set; }

        /// <summary>
        /// Gets or sets the radius of investigation in feet.
        /// </summary>
        public double RadiusOfInvestigation { get; set; }

        /// <summary>
        /// Gets or sets the identified reservoir model.
        /// </summary>
        public ReservoirModel IdentifiedModel { get; set; } = ReservoirModel.InfiniteActing;

        /// <summary>
        /// Gets or sets the R-squared value for the analysis.
        /// </summary>
        public double RSquared { get; set; }

        /// <summary>
        /// Gets or sets the analysis method used.
        /// </summary>
        public string AnalysisMethod { get; set; } = string.Empty;
    }

    /// <summary>
    /// Reservoir model types for well test interpretation.
    /// </summary>
    public enum ReservoirModel
    {
        /// <summary>
        /// Infinite acting reservoir.
        /// </summary>
        InfiniteActing,

        /// <summary>
        /// Closed boundary reservoir.
        /// </summary>
        ClosedBoundary,

        /// <summary>
        /// Constant pressure boundary.
        /// </summary>
        ConstantPressureBoundary,

        /// <summary>
        /// Single fault.
        /// </summary>
        SingleFault,

        /// <summary>
        /// Multiple faults.
        /// </summary>
        MultipleFaults,

        /// <summary>
        /// Channel reservoir.
        /// </summary>
        ChannelReservoir,

        /// <summary>
        /// Dual porosity.
        /// </summary>
        DualPorosity,

        /// <summary>
        /// Dual permeability.
        /// </summary>
        DualPermeability
    }
}

