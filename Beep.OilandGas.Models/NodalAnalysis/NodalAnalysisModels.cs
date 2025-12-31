using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.NodalAnalysis
{
    /// <summary>
    /// Represents reservoir properties for IPR calculations.
    /// </summary>
    public class ReservoirProperties
    {
        /// <summary>
        /// Gets or sets the reservoir pressure in psi.
        /// </summary>
        public double ReservoirPressure { get; set; }

        /// <summary>
        /// Gets or sets the bubble point pressure in psi.
        /// </summary>
        public double BubblePointPressure { get; set; }

        /// <summary>
        /// Gets or sets the productivity index in BPD/psi.
        /// </summary>
        public double ProductivityIndex { get; set; }

        /// <summary>
        /// Gets or sets the water cut (fraction).
        /// </summary>
        public double WaterCut { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the gas-oil ratio in SCF/STB.
        /// </summary>
        public double GasOilRatio { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the oil gravity in API.
        /// </summary>
        public double OilGravity { get; set; } = 35.0;

        /// <summary>
        /// Gets or sets the formation volume factor in RB/STB.
        /// </summary>
        public double FormationVolumeFactor { get; set; } = 1.2;

        /// <summary>
        /// Gets or sets the oil viscosity in cp.
        /// </summary>
        public double OilViscosity { get; set; } = 1.5;
    }

    /// <summary>
    /// Represents a point on an IPR curve.
    /// </summary>
    public class IPRPoint
    {
        /// <summary>
        /// Gets or sets the flow rate in BPD.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the flowing bottomhole pressure in psi.
        /// </summary>
        public double FlowingBottomholePressure { get; set; }

        public IPRPoint() { }

        public IPRPoint(double flowRate, double flowingBottomholePressure)
        {
            FlowRate = flowRate;
            FlowingBottomholePressure = flowingBottomholePressure;
        }
    }

    /// <summary>
    /// Represents wellbore properties for VLP calculations.
    /// </summary>
    public class WellboreProperties
    {
        /// <summary>
        /// Gets or sets the tubing diameter in inches.
        /// </summary>
        public double TubingDiameter { get; set; }

        /// <summary>
        /// Gets or sets the tubing length in feet.
        /// </summary>
        public double TubingLength { get; set; }

        /// <summary>
        /// Gets or sets the wellhead pressure in psi.
        /// </summary>
        public double WellheadPressure { get; set; }

        /// <summary>
        /// Gets or sets the water cut (fraction).
        /// </summary>
        public double WaterCut { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the gas-oil ratio in SCF/STB.
        /// </summary>
        public double GasOilRatio { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the oil gravity in API.
        /// </summary>
        public double OilGravity { get; set; } = 35.0;

        /// <summary>
        /// Gets or sets the gas specific gravity.
        /// </summary>
        public double GasSpecificGravity { get; set; } = 0.65;

        /// <summary>
        /// Gets or sets the wellhead temperature in Fahrenheit.
        /// </summary>
        public double WellheadTemperature { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets the bottomhole temperature in Fahrenheit.
        /// </summary>
        public double BottomholeTemperature { get; set; } = 200.0;

        /// <summary>
        /// Gets or sets the tubing roughness in feet.
        /// </summary>
        public double TubingRoughness { get; set; } = 0.00015;
    }

    /// <summary>
    /// Represents a point on a VLP curve.
    /// </summary>
    public class VLPPoint
    {
        /// <summary>
        /// Gets or sets the flow rate in BPD.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the required bottomhole pressure in psi.
        /// </summary>
        public double RequiredBottomholePressure { get; set; }

        public VLPPoint() { }

        public VLPPoint(double flowRate, double requiredBottomholePressure)
        {
            FlowRate = flowRate;
            RequiredBottomholePressure = requiredBottomholePressure;
        }
    }

    /// <summary>
    /// Represents an operating point from nodal analysis.
    /// </summary>
    public class OperatingPoint
    {
        /// <summary>
        /// Gets or sets the operating flow rate in BPD.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the operating bottomhole pressure in psi.
        /// </summary>
        public double BottomholePressure { get; set; }

        /// <summary>
        /// Gets or sets the operating wellhead pressure in psi.
        /// </summary>
        public double WellheadPressure { get; set; }

        public OperatingPoint() { }

        public OperatingPoint(double flowRate, double bottomholePressure)
        {
            FlowRate = flowRate;
            BottomholePressure = bottomholePressure;
        }
    }

    /// <summary>
    /// Represents a lateral branch in a multilateral well.
    /// </summary>
    public class LateralBranch
    {
        /// <summary>
        /// Gets or sets the branch length in feet.
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Gets or sets the branch diameter in inches.
        /// </summary>
        public double Diameter { get; set; }

        /// <summary>
        /// Gets or sets the branch productivity index in BPD/psi.
        /// </summary>
        public double ProductivityIndex { get; set; }

        /// <summary>
        /// Gets or sets the branch skin factor.
        /// </summary>
        public double SkinFactor { get; set; } = 0.0;

        /// <summary>
        /// Gets or sets the branch drainage radius in feet.
        /// </summary>
        public double DrainageRadius { get; set; } = 1000.0;

        /// <summary>
        /// Gets or sets the branch wellbore radius in feet.
        /// </summary>
        public double WellboreRadius { get; set; } = 0.25;

        /// <summary>
        /// Gets or sets the branch reservoir pressure in psi.
        /// </summary>
        public double ReservoirPressure { get; set; }

        /// <summary>
        /// Gets or sets the branch permeability in md.
        /// </summary>
        public double Permeability { get; set; }

        /// <summary>
        /// Gets or sets the branch formation thickness in feet.
        /// </summary>
        public double FormationThickness { get; set; }

        /// <summary>
        /// Gets or sets the branch name or identifier.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents properties for a multilateral well.
    /// </summary>
    public class MultilateralWellProperties
    {
        /// <summary>
        /// Gets or sets the main wellbore properties.
        /// </summary>
        public WellboreProperties MainWellbore { get; set; }

        /// <summary>
        /// Gets or sets the list of lateral branches.
        /// </summary>
        public List<LateralBranch> LateralBranches { get; set; } = new();

        /// <summary>
        /// Gets or sets the junction depth in feet (where laterals connect to main wellbore).
        /// </summary>
        public double JunctionDepth { get; set; }

        /// <summary>
        /// Gets or sets the wellhead pressure in psi.
        /// </summary>
        public double WellheadPressure { get; set; }

        /// <summary>
        /// Gets or sets the total well depth in feet.
        /// </summary>
        public double TotalDepth { get; set; }
    }

    /// <summary>
    /// Represents deliverability results for a multilateral well.
    /// </summary>
    public class MultilateralDeliverabilityResult
    {
        /// <summary>
        /// Gets or sets the total production rate in BPD or Mscf/day.
        /// </summary>
        public double TotalProductionRate { get; set; }

        /// <summary>
        /// Gets or sets the production rate per branch.
        /// </summary>
        public Dictionary<string, double> BranchProductionRates { get; set; } = new();

        /// <summary>
        /// Gets or sets the bottomhole pressure at junction in psi.
        /// </summary>
        public double JunctionBottomholePressure { get; set; }

        /// <summary>
        /// Gets or sets the IPR curve points.
        /// </summary>
        public List<IPRPoint> IPRCurve { get; set; } = new();

        /// <summary>
        /// Gets or sets the operating point.
        /// </summary>
        public OperatingPoint OperatingPoint { get; set; }
    }
}

