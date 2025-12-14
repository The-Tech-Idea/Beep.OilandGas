using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PipelineAnalysis.Models
{
    /// <summary>
    /// Represents pipeline properties.
    /// </summary>
    public class PipelineProperties
    {
        /// <summary>
        /// Pipeline diameter in inches.
        /// </summary>
        public decimal Diameter { get; set; }

        /// <summary>
        /// Pipeline length in feet.
        /// </summary>
        public decimal Length { get; set; }

        /// <summary>
        /// Pipeline roughness in feet.
        /// </summary>
        public decimal Roughness { get; set; } = 0.00015m; // Typical for steel

        /// <summary>
        /// Elevation change in feet (positive = uphill).
        /// </summary>
        public decimal ElevationChange { get; set; } = 0m;

        /// <summary>
        /// Inlet pressure in psia.
        /// </summary>
        public decimal InletPressure { get; set; }

        /// <summary>
        /// Outlet pressure in psia.
        /// </summary>
        public decimal OutletPressure { get; set; }

        /// <summary>
        /// Average temperature in Rankine.
        /// </summary>
        public decimal AverageTemperature { get; set; }
    }

    /// <summary>
    /// Represents gas pipeline flow properties.
    /// </summary>
    public class GasPipelineFlowProperties
    {
        /// <summary>
        /// Pipeline properties.
        /// </summary>
        public PipelineProperties Pipeline { get; set; }

        /// <summary>
        /// Gas flow rate in Mscf/day.
        /// </summary>
        public decimal GasFlowRate { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air).
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Gas molecular weight.
        /// </summary>
        public decimal GasMolecularWeight { get; set; }

        /// <summary>
        /// Base pressure in psia.
        /// </summary>
        public decimal BasePressure { get; set; } = 14.7m;

        /// <summary>
        /// Base temperature in Rankine.
        /// </summary>
        public decimal BaseTemperature { get; set; } = 520m;
    }

    /// <summary>
    /// Represents liquid pipeline flow properties.
    /// </summary>
    public class LiquidPipelineFlowProperties
    {
        /// <summary>
        /// Pipeline properties.
        /// </summary>
        public PipelineProperties Pipeline { get; set; }

        /// <summary>
        /// Liquid flow rate in bbl/day.
        /// </summary>
        public decimal LiquidFlowRate { get; set; }

        /// <summary>
        /// Liquid specific gravity.
        /// </summary>
        public decimal LiquidSpecificGravity { get; set; }

        /// <summary>
        /// Liquid viscosity in cp.
        /// </summary>
        public decimal LiquidViscosity { get; set; } = 1.0m;
    }

    /// <summary>
    /// Represents pipeline capacity calculation results.
    /// </summary>
    public class PipelineCapacityResult
    {
        /// <summary>
        /// Maximum flow rate in Mscf/day (gas) or bbl/day (liquid).
        /// </summary>
        public decimal MaximumFlowRate { get; set; }

        /// <summary>
        /// Pressure drop in psi.
        /// </summary>
        public decimal PressureDrop { get; set; }

        /// <summary>
        /// Flow velocity in ft/s.
        /// </summary>
        public decimal FlowVelocity { get; set; }

        /// <summary>
        /// Reynolds number.
        /// </summary>
        public decimal ReynoldsNumber { get; set; }

        /// <summary>
        /// Friction factor.
        /// </summary>
        public decimal FrictionFactor { get; set; }

        /// <summary>
        /// Pressure gradient in psia/ft.
        /// </summary>
        public decimal PressureGradient { get; set; }

        /// <summary>
        /// Outlet pressure in psia.
        /// </summary>
        public decimal OutletPressure { get; set; }
    }

    /// <summary>
    /// Represents pipeline flow analysis results.
    /// </summary>
    public class PipelineFlowAnalysisResult
    {
        /// <summary>
        /// Flow rate in Mscf/day (gas) or bbl/day (liquid).
        /// </summary>
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Pressure drop in psi.
        /// </summary>
        public decimal PressureDrop { get; set; }

        /// <summary>
        /// Flow velocity in ft/s.
        /// </summary>
        public decimal FlowVelocity { get; set; }

        /// <summary>
        /// Reynolds number.
        /// </summary>
        public decimal ReynoldsNumber { get; set; }

        /// <summary>
        /// Friction factor.
        /// </summary>
        public decimal FrictionFactor { get; set; }

        /// <summary>
        /// Pressure gradient in psia/ft.
        /// </summary>
        public decimal PressureGradient { get; set; }

        /// <summary>
        /// Outlet pressure in psia.
        /// </summary>
        public decimal OutletPressure { get; set; }

        /// <summary>
        /// Flow regime (laminar, transitional, turbulent).
        /// </summary>
        public string FlowRegime { get; set; } = string.Empty;
    }
}

