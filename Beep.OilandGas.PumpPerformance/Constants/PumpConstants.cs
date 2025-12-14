using System;

namespace Beep.OilandGas.PumpPerformance.Constants
{
    /// <summary>
    /// Constants used in pump performance calculations.
    /// </summary>
    public static class PumpConstants
    {
        /// <summary>
        /// Conversion factor for horsepower calculation (3960 = 550 ft-lbf/s / (62.4 lbm/ft³ * 1/7.48 ft³/gal)).
        /// Used in formula: BHP = (Q * H * SG) / 3960
        /// </summary>
        public const double HorsepowerConversionFactor = 3960.0;

        /// <summary>
        /// Specific gravity of water at standard conditions.
        /// </summary>
        public const double WaterSpecificGravity = 1.0;

        /// <summary>
        /// Standard acceleration due to gravity in ft/s².
        /// </summary>
        public const double GravityFeetPerSecondSquared = 32.174;

        /// <summary>
        /// Standard acceleration due to gravity in m/s².
        /// </summary>
        public const double GravityMetersPerSecondSquared = 9.80665;

        /// <summary>
        /// Conversion factor: feet to meters.
        /// </summary>
        public const double FeetToMeters = 0.3048;

        /// <summary>
        /// Conversion factor: meters to feet.
        /// </summary>
        public const double MetersToFeet = 3.28084;

        /// <summary>
        /// Conversion factor: horsepower to kilowatts.
        /// </summary>
        public const double HorsepowerToKilowatts = 0.7457;

        /// <summary>
        /// Conversion factor: kilowatts to horsepower.
        /// </summary>
        public const double KilowattsToHorsepower = 1.34102;

        /// <summary>
        /// Conversion factor: gallons per minute to barrels per day.
        /// </summary>
        public const double GpmToBpd = 34.2857;

        /// <summary>
        /// Conversion factor: barrels per day to gallons per minute.
        /// </summary>
        public const double BpdToGpm = 0.02917;

        /// <summary>
        /// Conversion factor: cubic meters per hour to gallons per minute.
        /// </summary>
        public const double CubicMetersPerHourToGpm = 4.40287;

        /// <summary>
        /// Minimum valid flow rate (GPM).
        /// </summary>
        public const double MinFlowRate = 0.0;

        /// <summary>
        /// Maximum reasonable flow rate (GPM) for validation.
        /// </summary>
        public const double MaxFlowRate = 100000.0;

        /// <summary>
        /// Minimum valid head (feet).
        /// </summary>
        public const double MinHead = 0.0;

        /// <summary>
        /// Maximum reasonable head (feet) for validation.
        /// </summary>
        public const double MaxHead = 10000.0;

        /// <summary>
        /// Minimum valid power (horsepower).
        /// </summary>
        public const double MinPower = 0.0;

        /// <summary>
        /// Maximum reasonable power (horsepower) for validation.
        /// </summary>
        public const double MaxPower = 100000.0;

        /// <summary>
        /// Minimum valid efficiency (0 to 1).
        /// </summary>
        public const double MinEfficiency = 0.0;

        /// <summary>
        /// Maximum valid efficiency (typically 1.0, but can exceed for theoretical calculations).
        /// </summary>
        public const double MaxEfficiency = 1.2;

        /// <summary>
        /// Minimum valid specific gravity.
        /// </summary>
        public const double MinSpecificGravity = 0.1;

        /// <summary>
        /// Maximum valid specific gravity.
        /// </summary>
        public const double MaxSpecificGravity = 10.0;

        /// <summary>
        /// Epsilon value for floating-point comparisons.
        /// </summary>
        public const double Epsilon = 1E-9;

        /// <summary>
        /// Minimum number of data points required for curve calculations.
        /// </summary>
        public const int MinDataPoints = 2;

        /// <summary>
        /// Standard atmospheric pressure in psia.
        /// </summary>
        public const double StandardAtmosphericPressure = 14.696;

        /// <summary>
        /// Standard atmospheric pressure in Pascals.
        /// </summary>
        public const double StandardAtmosphericPressurePascals = 101325.0;

        /// <summary>
        /// Vapor pressure of water at 60°F in psia.
        /// </summary>
        public const double WaterVaporPressureAt60F = 0.256;

        /// <summary>
        /// Density of water at 60°F in lbm/ft³.
        /// </summary>
        public const double WaterDensityAt60F = 62.37;
    }
}

