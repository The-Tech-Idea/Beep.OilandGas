using System;
using System.Collections.Generic;

namespace Beep.OilandGas.HydraulicPumps.Models
{
    /// <summary>
    /// Represents hydraulic pump well properties.
    /// </summary>
    public class HydraulicPumpWellProperties
    {
        /// <summary>
        /// Well depth in feet.
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Tubing diameter in inches.
        /// </summary>
        public decimal TubingDiameter { get; set; }

        /// <summary>
        /// Casing diameter in inches.
        /// </summary>
        public decimal CasingDiameter { get; set; }

        /// <summary>
        /// Wellhead pressure in psia.
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Bottom hole pressure in psia.
        /// </summary>
        public decimal BottomHolePressure { get; set; }

        /// <summary>
        /// Wellhead temperature in Rankine.
        /// </summary>
        public decimal WellheadTemperature { get; set; }

        /// <summary>
        /// Bottom hole temperature in Rankine.
        /// </summary>
        public decimal BottomHoleTemperature { get; set; }

        /// <summary>
        /// Oil gravity in API.
        /// </summary>
        public decimal OilGravity { get; set; }

        /// <summary>
        /// Water cut (fraction, 0-1).
        /// </summary>
        public decimal WaterCut { get; set; }

        /// <summary>
        /// Gas-oil ratio in scf/bbl.
        /// </summary>
        public decimal GasOilRatio { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air).
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Desired production rate in bbl/day.
        /// </summary>
        public decimal DesiredProductionRate { get; set; }
        public decimal PumpDepth { get; set; }
    }

    /// <summary>
    /// Represents hydraulic jet pump properties.
    /// </summary>
    public class HydraulicJetPumpProperties
    {
        /// <summary>
        /// Nozzle diameter in inches.
        /// </summary>
        public decimal NozzleDiameter { get; set; }

        /// <summary>
        /// Throat diameter in inches.
        /// </summary>
        public decimal ThroatDiameter { get; set; }

        /// <summary>
        /// Diffuser diameter in inches.
        /// </summary>
        public decimal DiffuserDiameter { get; set; }

        /// <summary>
        /// Power fluid pressure in psia.
        /// </summary>
        public decimal PowerFluidPressure { get; set; }

        /// <summary>
        /// Power fluid rate in bbl/day.
        /// </summary>
        public decimal PowerFluidRate { get; set; }

        /// <summary>
        /// Power fluid specific gravity.
        /// </summary>
        public decimal PowerFluidSpecificGravity { get; set; } = 1.0m;
    }

    /// <summary>
    /// Represents hydraulic piston pump properties.
    /// </summary>
    public class HydraulicPistonPumpProperties
    {
        /// <summary>
        /// Piston diameter in inches.
        /// </summary>
        public decimal PistonDiameter { get; set; }

        /// <summary>
        /// Rod diameter in inches.
        /// </summary>
        public decimal RodDiameter { get; set; }

        /// <summary>
        /// Stroke length in inches.
        /// </summary>
        public decimal StrokeLength { get; set; }

        /// <summary>
        /// Strokes per minute (SPM).
        /// </summary>
        public decimal StrokesPerMinute { get; set; }

        /// <summary>
        /// Power fluid pressure in psia.
        /// </summary>
        public decimal PowerFluidPressure { get; set; }

        /// <summary>
        /// Power fluid rate in bbl/day.
        /// </summary>
        public decimal PowerFluidRate { get; set; }

        /// <summary>
        /// Power fluid specific gravity.
        /// </summary>
        public decimal PowerFluidSpecificGravity { get; set; } = 1.0m;
    }

    /// <summary>
    /// Represents hydraulic jet pump performance results.
    /// </summary>
    public class HydraulicJetPumpResult
    {
        /// <summary>
        /// Production rate in bbl/day.
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Total flow rate (power fluid + production) in bbl/day.
        /// </summary>
        public decimal TotalFlowRate { get; set; }

        /// <summary>
        /// Production to power fluid ratio.
        /// </summary>
        public decimal ProductionRatio { get; set; }

        /// <summary>
        /// Pump efficiency (0-1).
        /// </summary>
        public decimal PumpEfficiency { get; set; }

        /// <summary>
        /// Power fluid horsepower.
        /// </summary>
        public decimal PowerFluidHorsepower { get; set; }

        /// <summary>
        /// Hydraulic horsepower.
        /// </summary>
        public decimal HydraulicHorsepower { get; set; }

        /// <summary>
        /// System efficiency (0-1).
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Pressure at pump intake in psia.
        /// </summary>
        public decimal PumpIntakePressure { get; set; }

        /// <summary>
        /// Pressure at pump discharge in psia.
        /// </summary>
        public decimal PumpDischargePressure { get; set; }
    }

    /// <summary>
    /// Represents hydraulic piston pump performance results.
    /// </summary>
    public class HydraulicPistonPumpResult
    {
        /// <summary>
        /// Production rate in bbl/day.
        /// </summary>
        public decimal ProductionRate { get; set; }

        /// <summary>
        /// Pump displacement in bbl/day.
        /// </summary>
        public decimal PumpDisplacement { get; set; }

        /// <summary>
        /// Volumetric efficiency (0-1).
        /// </summary>
        public decimal VolumetricEfficiency { get; set; }

        /// <summary>
        /// Power fluid horsepower.
        /// </summary>
        public decimal PowerFluidHorsepower { get; set; }

        /// <summary>
        /// Hydraulic horsepower.
        /// </summary>
        public decimal HydraulicHorsepower { get; set; }

        /// <summary>
        /// System efficiency (0-1).
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Pressure at pump intake in psia.
        /// </summary>
        public decimal PumpIntakePressure { get; set; }

        /// <summary>
        /// Pressure at pump discharge in psia.
        /// </summary>
        public decimal PumpDischargePressure { get; set; }

        /// <summary>
        /// Power fluid consumption in bbl/day.
        /// </summary>
        public decimal PowerFluidConsumption { get; set; }
    }
}

