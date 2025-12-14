using System;
using System.Collections.Generic;

namespace Beep.OilandGas.CompressorAnalysis.Models
{
    /// <summary>
    /// Represents compressor operating conditions.
    /// </summary>
    public class CompressorOperatingConditions
    {
        /// <summary>
        /// Suction pressure in psia.
        /// </summary>
        public decimal SuctionPressure { get; set; }

        /// <summary>
        /// Discharge pressure in psia.
        /// </summary>
        public decimal DischargePressure { get; set; }

        /// <summary>
        /// Suction temperature in Rankine.
        /// </summary>
        public decimal SuctionTemperature { get; set; }

        /// <summary>
        /// Discharge temperature in Rankine.
        /// </summary>
        public decimal DischargeTemperature { get; set; }

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
        /// Compressor efficiency (0-1).
        /// </summary>
        public decimal CompressorEfficiency { get; set; } = 0.75m;

        /// <summary>
        /// Mechanical efficiency (0-1).
        /// </summary>
        public decimal MechanicalEfficiency { get; set; } = 0.95m;
    }

    /// <summary>
    /// Represents centrifugal compressor properties.
    /// </summary>
    public class CentrifugalCompressorProperties
    {
        /// <summary>
        /// Operating conditions.
        /// </summary>
        public CompressorOperatingConditions OperatingConditions { get; set; }

        /// <summary>
        /// Polytropic efficiency (0-1).
        /// </summary>
        public decimal PolytropicEfficiency { get; set; } = 0.75m;

        /// <summary>
        /// Specific heat ratio (k = Cp/Cv).
        /// </summary>
        public decimal SpecificHeatRatio { get; set; } = 1.3m;

        /// <summary>
        /// Number of stages.
        /// </summary>
        public int NumberOfStages { get; set; } = 1;

        /// <summary>
        /// Compressor speed in RPM.
        /// </summary>
        public decimal Speed { get; set; }
    }

    /// <summary>
    /// Represents reciprocating compressor properties.
    /// </summary>
    public class ReciprocatingCompressorProperties
    {
        /// <summary>
        /// Operating conditions.
        /// </summary>
        public CompressorOperatingConditions OperatingConditions { get; set; }

        /// <summary>
        /// Cylinder diameter in inches.
        /// </summary>
        public decimal CylinderDiameter { get; set; }

        /// <summary>
        /// Stroke length in inches.
        /// </summary>
        public decimal StrokeLength { get; set; }

        /// <summary>
        /// Rotational speed in RPM.
        /// </summary>
        public decimal RotationalSpeed { get; set; }

        /// <summary>
        /// Number of cylinders.
        /// </summary>
        public int NumberOfCylinders { get; set; } = 1;

        /// <summary>
        /// Volumetric efficiency (0-1).
        /// </summary>
        public decimal VolumetricEfficiency { get; set; } = 0.85m;

        /// <summary>
        /// Clearance factor (0-1).
        /// </summary>
        public decimal ClearanceFactor { get; set; } = 0.05m;
    }

    /// <summary>
    /// Represents compressor power calculation results.
    /// </summary>
    public class CompressorPowerResult
    {
        /// <summary>
        /// Theoretical power in horsepower.
        /// </summary>
        public decimal TheoreticalPower { get; set; }

        /// <summary>
        /// Brake horsepower (BHP).
        /// </summary>
        public decimal BrakeHorsepower { get; set; }

        /// <summary>
        /// Motor horsepower.
        /// </summary>
        public decimal MotorHorsepower { get; set; }

        /// <summary>
        /// Power consumption in kW.
        /// </summary>
        public decimal PowerConsumptionKW { get; set; }

        /// <summary>
        /// Compression ratio.
        /// </summary>
        public decimal CompressionRatio { get; set; }

        /// <summary>
        /// Polytropic head in feet.
        /// </summary>
        public decimal PolytropicHead { get; set; }

        /// <summary>
        /// Adiabatic head in feet.
        /// </summary>
        public decimal AdiabaticHead { get; set; }

        /// <summary>
        /// Discharge temperature in Rankine.
        /// </summary>
        public decimal DischargeTemperature { get; set; }

        /// <summary>
        /// Overall efficiency (0-1).
        /// </summary>
        public decimal OverallEfficiency { get; set; }
    }

    /// <summary>
    /// Represents compressor pressure calculation results.
    /// </summary>
    public class CompressorPressureResult
    {
        /// <summary>
        /// Required discharge pressure in psia.
        /// </summary>
        public decimal RequiredDischargePressure { get; set; }

        /// <summary>
        /// Compression ratio.
        /// </summary>
        public decimal CompressionRatio { get; set; }

        /// <summary>
        /// Required power in horsepower.
        /// </summary>
        public decimal RequiredPower { get; set; }

        /// <summary>
        /// Discharge temperature in Rankine.
        /// </summary>
        public decimal DischargeTemperature { get; set; }

        /// <summary>
        /// Is feasible.
        /// </summary>
        public bool IsFeasible { get; set; }
    }
}

