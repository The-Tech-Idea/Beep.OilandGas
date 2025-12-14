using System;
using System.Collections.Generic;

namespace Beep.OilandGas.PumpPerformance.Models
{
    /// <summary>
    /// Represents ESP (Electric Submersible Pump) design properties.
    /// </summary>
    public class ESPDesignProperties
    {
        /// <summary>
        /// Desired flow rate in bbl/day.
        /// </summary>
        public decimal DesiredFlowRate { get; set; }

        /// <summary>
        /// Total dynamic head in feet.
        /// </summary>
        public decimal TotalDynamicHead { get; set; }

        /// <summary>
        /// Well depth in feet.
        /// </summary>
        public decimal WellDepth { get; set; }

        /// <summary>
        /// Casing diameter in inches.
        /// </summary>
        public decimal CasingDiameter { get; set; }

        /// <summary>
        /// Tubing diameter in inches.
        /// </summary>
        public decimal TubingDiameter { get; set; }

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
        /// Wellhead pressure in psia.
        /// </summary>
        public decimal WellheadPressure { get; set; }

        /// <summary>
        /// Bottom hole temperature in Rankine.
        /// </summary>
        public decimal BottomHoleTemperature { get; set; }

        /// <summary>
        /// Gas specific gravity (relative to air).
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Pump setting depth in feet.
        /// </summary>
        public decimal PumpSettingDepth { get; set; }
    }

    /// <summary>
    /// Represents ESP design results.
    /// </summary>
    public class ESPDesignResult
    {
        /// <summary>
        /// Selected pump stage count.
        /// </summary>
        public int PumpStages { get; set; }

        /// <summary>
        /// Required pump horsepower.
        /// </summary>
        public decimal RequiredHorsepower { get; set; }

        /// <summary>
        /// Motor horsepower.
        /// </summary>
        public decimal MotorHorsepower { get; set; }

        /// <summary>
        /// Motor voltage.
        /// </summary>
        public decimal MotorVoltage { get; set; }

        /// <summary>
        /// Motor current in amps.
        /// </summary>
        public decimal MotorCurrent { get; set; }

        /// <summary>
        /// Cable size in AWG.
        /// </summary>
        public int CableSize { get; set; }

        /// <summary>
        /// Cable length in feet.
        /// </summary>
        public decimal CableLength { get; set; }

        /// <summary>
        /// Total system efficiency.
        /// </summary>
        public decimal SystemEfficiency { get; set; }

        /// <summary>
        /// Pump efficiency.
        /// </summary>
        public decimal PumpEfficiency { get; set; }

        /// <summary>
        /// Motor efficiency.
        /// </summary>
        public decimal MotorEfficiency { get; set; }

        /// <summary>
        /// Power consumption in kW.
        /// </summary>
        public decimal PowerConsumption { get; set; }

        /// <summary>
        /// Operating flow rate in bbl/day.
        /// </summary>
        public decimal OperatingFlowRate { get; set; }

        /// <summary>
        /// Operating head in feet.
        /// </summary>
        public decimal OperatingHead { get; set; }

        /// <summary>
        /// Pump performance points.
        /// </summary>
        public List<ESPPumpPoint> PerformancePoints { get; set; } = new();
    }

    /// <summary>
    /// Represents a point on ESP pump performance curve.
    /// </summary>
    public class ESPPumpPoint
    {
        /// <summary>
        /// Flow rate in bbl/day.
        /// </summary>
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Head in feet.
        /// </summary>
        public decimal Head { get; set; }

        /// <summary>
        /// Efficiency (0-1).
        /// </summary>
        public decimal Efficiency { get; set; }

        /// <summary>
        /// Horsepower.
        /// </summary>
        public decimal Horsepower { get; set; }
    }

    /// <summary>
    /// Represents ESP motor properties.
    /// </summary>
    public class ESPMotorProperties
    {
        /// <summary>
        /// Motor horsepower.
        /// </summary>
        public decimal Horsepower { get; set; }

        /// <summary>
        /// Motor voltage.
        /// </summary>
        public decimal Voltage { get; set; }

        /// <summary>
        /// Motor efficiency (0-1).
        /// </summary>
        public decimal Efficiency { get; set; } = 0.9m;

        /// <summary>
        /// Power factor.
        /// </summary>
        public decimal PowerFactor { get; set; } = 0.85m;
    }

    /// <summary>
    /// Represents ESP cable properties.
    /// </summary>
    public class ESPCableProperties
    {
        /// <summary>
        /// Cable size in AWG.
        /// </summary>
        public int CableSize { get; set; }

        /// <summary>
        /// Cable length in feet.
        /// </summary>
        public decimal CableLength { get; set; }

        /// <summary>
        /// Cable resistance in ohms per 1000 feet.
        /// </summary>
        public decimal ResistancePer1000Feet { get; set; }

        /// <summary>
        /// Voltage drop in volts.
        /// </summary>
        public decimal VoltageDrop { get; set; }
    }
}

