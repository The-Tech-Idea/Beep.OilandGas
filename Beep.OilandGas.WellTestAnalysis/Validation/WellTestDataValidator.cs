using System;
using System.Linq;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Beep.OilandGas.WellTestAnalysis.Exceptions;
using Beep.OilandGas.Models.Data.WellTestAnalysis;

namespace Beep.OilandGas.WellTestAnalysis.Validation
{
    /// <summary>
    /// Validates well test data for analysis.
    /// </summary>
    public static class WellTestDataValidator
    {
        /// <summary>
        /// Validates well test data.
        /// </summary>
        public static void Validate(WELL_TEST_DATA data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            ValidateTimeData(data.Time, nameof(data.Time));
            ValidatePressureData(data.Pressure, nameof(data.Pressure));
            ValidateMatchingLengths(data.Time, data.Pressure, nameof(data.Time), nameof(data.Pressure));
            ValidateFlowRate((double)data.FLOW_RATE, nameof(data.FLOW_RATE));
            ValidateWellboreRadius((double)data.WELLBORE_RADIUS, nameof(data.WELLBORE_RADIUS));
            ValidateFormationThickness((double)data.FORMATION_THICKNESS, nameof(data.FORMATION_THICKNESS));
            ValidatePorosity((double)data.POROSITY, nameof(data.POROSITY));
            ValidateCompressibility((double)data.TOTAL_COMPRESSIBILITY, nameof(data.TOTAL_COMPRESSIBILITY));
            ValidateViscosity((double)data.OIL_VISCOSITY, nameof(data.OIL_VISCOSITY));
            ValidateFormationVolumeFactor((double)data.OIL_FORMATION_VOLUME_FACTOR, nameof(data.OIL_FORMATION_VOLUME_FACTOR));
        }

        /// <summary>
        /// Validates time data array.
        /// </summary>
        public static void ValidateTimeData(System.Collections.Generic.List<double> time, string parameterName)
        {
            if (time == null || time.Count == 0)
                throw new InvalidWellTestDataException(parameterName, "Time data cannot be null or empty.");

            if (time.Count < 3)
                throw new InvalidWellTestDataException(parameterName, "At least 3 time points are required for analysis.");

            if (time.Any(t => t < WellTestConstants.MinTime || t > WellTestConstants.MaxTime))
                throw new InvalidWellTestDataException(parameterName, 
                    $"Time values must be between {WellTestConstants.MinTime} and {WellTestConstants.MaxTime} hours.");

            // Check for chronological order
            for (int i = 1; i < time.Count; i++)
            {
                if (time[i] <= time[i - 1])
                    throw new InvalidWellTestDataException(parameterName, "Time values must be in chronological order.");
            }
        }

        /// <summary>
        /// Validates pressure data array.
        /// </summary>
        public static void ValidatePressureData(System.Collections.Generic.List<double> pressure, string parameterName)
        {
            if (pressure == null || pressure.Count == 0)
                throw new InvalidWellTestDataException(parameterName, "Pressure data cannot be null or empty.");

            if (pressure.Any(p => p < WellTestConstants.MinPressure || p > WellTestConstants.MaxPressure))
                throw new InvalidWellTestDataException(parameterName,
                    $"Pressure values must be between {WellTestConstants.MinPressure} and {WellTestConstants.MaxPressure} psi.");
        }

        /// <summary>
        /// Validates that two arrays have matching lengths.
        /// </summary>
        public static void ValidateMatchingLengths(System.Collections.Generic.List<double> array1, 
            System.Collections.Generic.List<double> array2, string name1, string name2)
        {
            if (array1.Count != array2.Count)
                throw new InvalidWellTestDataException(name1, 
                    $"{name1} and {name2} must have the same length.");
        }

        /// <summary>
        /// Validates flow rate.
        /// </summary>
        public static void ValidateFlowRate(double flowRate, string parameterName)
        {
            if (flowRate < WellTestConstants.MinFlowRate || flowRate > WellTestConstants.MaxFlowRate)
                throw new InvalidWellTestDataException(parameterName,
                    $"Flow rate must be between {WellTestConstants.MinFlowRate} and {WellTestConstants.MaxFlowRate} BPD.");
        }

        /// <summary>
        /// Validates wellbore radius.
        /// </summary>
        public static void ValidateWellboreRadius(double radius, string parameterName)
        {
            if (radius < WellTestConstants.MinWellboreRadius || radius > WellTestConstants.MaxWellboreRadius)
                throw new InvalidWellTestDataException(parameterName,
                    $"Wellbore radius must be between {WellTestConstants.MinWellboreRadius} and {WellTestConstants.MaxWellboreRadius} feet.");
        }

        /// <summary>
        /// Validates formation thickness.
        /// </summary>
        public static void ValidateFormationThickness(double thickness, string parameterName)
        {
            if (thickness < WellTestConstants.MinFormationThickness || thickness > WellTestConstants.MaxFormationThickness)
                throw new InvalidWellTestDataException(parameterName,
                    $"Formation thickness must be between {WellTestConstants.MinFormationThickness} and {WellTestConstants.MaxFormationThickness} feet.");
        }

        /// <summary>
        /// Validates porosity.
        /// </summary>
        public static void ValidatePorosity(double porosity, string parameterName)
        {
            if (porosity < WellTestConstants.MinPorosity || porosity > WellTestConstants.MaxPorosity)
                throw new InvalidWellTestDataException(parameterName,
                    $"Porosity must be between {WellTestConstants.MinPorosity} and {WellTestConstants.MaxPorosity}.");
        }

        /// <summary>
        /// Validates compressibility.
        /// </summary>
        public static void ValidateCompressibility(double compressibility, string parameterName)
        {
            if (compressibility < WellTestConstants.MinCompressibility || compressibility > WellTestConstants.MaxCompressibility)
                throw new InvalidWellTestDataException(parameterName,
                    $"Compressibility must be between {WellTestConstants.MinCompressibility} and {WellTestConstants.MaxCompressibility} psi^-1.");
        }

        /// <summary>
        /// Validates viscosity.
        /// </summary>
        public static void ValidateViscosity(double viscosity, string parameterName)
        {
            if (viscosity < WellTestConstants.MinViscosity || viscosity > WellTestConstants.MaxViscosity)
                throw new InvalidWellTestDataException(parameterName,
                    $"Viscosity must be between {WellTestConstants.MinViscosity} and {WellTestConstants.MaxViscosity} cp.");
        }

        /// <summary>
        /// Validates formation volume factor.
        /// </summary>
        public static void ValidateFormationVolumeFactor(double fvf, string parameterName)
        {
            if (fvf <= 0 || fvf > 10.0)
                throw new InvalidWellTestDataException(parameterName,
                    "Formation volume factor must be positive and less than 10.0 RB/STB.");
        }
    }
}

