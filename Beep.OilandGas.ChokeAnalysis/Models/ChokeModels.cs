using System;

namespace Beep.OilandGas.ChokeAnalysis.Models
{
    /// <summary>
    /// Represents choke properties.
    /// </summary>
    public class ChokeProperties
    {
        /// <summary>
        /// Choke diameter in inches.
        /// </summary>
        public decimal ChokeDiameter { get; set; }

        /// <summary>
        /// Choke type (bean, adjustable, etc.).
        /// </summary>
        public ChokeType ChokeType { get; set; }

        /// <summary>
        /// Discharge coefficient.
        /// </summary>
        public decimal DischargeCoefficient { get; set; } = 0.85m;

        /// <summary>
        /// Choke area in square inches.
        /// </summary>
        public decimal ChokeArea => (decimal)Math.PI * ChokeDiameter * ChokeDiameter / 4m;
    }

    /// <summary>
    /// Choke type enumeration.
    /// </summary>
    public enum ChokeType
    {
        /// <summary>
        /// Fixed bean choke.
        /// </summary>
        Bean,

        /// <summary>
        /// Adjustable choke.
        /// </summary>
        Adjustable,

        /// <summary>
        /// Positive choke.
        /// </summary>
        Positive
    }

    /// <summary>
    /// Represents gas properties for choke calculations.
    /// </summary>
    public class GasChokeProperties
    {
        /// <summary>
        /// Gas specific gravity (relative to air).
        /// </summary>
        public decimal GasSpecificGravity { get; set; }

        /// <summary>
        /// Upstream pressure in psia.
        /// </summary>
        public decimal UpstreamPressure { get; set; }

        /// <summary>
        /// Downstream pressure in psia.
        /// </summary>
        public decimal DownstreamPressure { get; set; }

        /// <summary>
        /// Temperature in Rankine.
        /// </summary>
        public decimal Temperature { get; set; }

        /// <summary>
        /// Z-factor (compressibility factor).
        /// </summary>
        public decimal ZFactor { get; set; }

        /// <summary>
        /// Gas flow rate in Mscf/day.
        /// </summary>
        public decimal FlowRate { get; set; }
    }

    /// <summary>
    /// Represents choke flow calculation results.
    /// </summary>
    public class ChokeFlowResult
    {
        /// <summary>
        /// Calculated flow rate in Mscf/day.
        /// </summary>
        public decimal FlowRate { get; set; }

        /// <summary>
        /// Calculated downstream pressure in psia.
        /// </summary>
        public decimal DownstreamPressure { get; set; }

        /// <summary>
        /// Calculated upstream pressure in psia.
        /// </summary>
        public decimal UpstreamPressure { get; set; }

        /// <summary>
        /// Pressure ratio (P2/P1).
        /// </summary>
        public decimal PressureRatio { get; set; }

        /// <summary>
        /// Flow regime (subsonic or sonic).
        /// </summary>
        public FlowRegime FlowRegime { get; set; }

        /// <summary>
        /// Critical pressure ratio.
        /// </summary>
        public decimal CriticalPressureRatio { get; set; }
    }

    /// <summary>
    /// Flow regime enumeration.
    /// </summary>
    public enum FlowRegime
    {
        /// <summary>
        /// Subsonic flow.
        /// </summary>
        Subsonic,

        /// <summary>
        /// Sonic (critical) flow.
        /// </summary>
        Sonic
    }
}

