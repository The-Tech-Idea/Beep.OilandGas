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
    /// <remarks>
    /// OILGAS-CATCH-01. Each rule is written once, as a question (<see cref="FindProblem"/> and the private
    /// <c>…Problem</c> methods answer the first problem, or null); the <c>Validate…</c> methods refuse that problem with
    /// <see cref="InvalidWellTestDataException"/>. A caller that wants the answer rather than the refusal — the
    /// service's data check — asks, instead of catching the refusal.
    /// </remarks>
    public static class WellTestDataValidator
    {
        /// <summary>
        /// Validates well test data, refusing the first problem found.
        /// </summary>
        public static void Validate(WELL_TEST_DATA data) => Refuse(FindProblem(data));

        /// <summary>
        /// The first problem that keeps <paramref name="data"/> from being analysed, or null when there is none.
        /// </summary>
        public static WellTestDataProblem? FindProblem(WELL_TEST_DATA data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            return TimeDataProblem(data.Time, nameof(data.Time))
                ?? PressureDataProblem(data.Pressure, nameof(data.Pressure))
                ?? MatchingLengthsProblem(data.Time, data.Pressure, nameof(data.Time), nameof(data.Pressure))
                ?? FlowRateProblem((double)data.FLOW_RATE, nameof(data.FLOW_RATE))
                ?? (data.WELLBORE_RADIUS.HasValue
                    ? WellboreRadiusProblem((double)data.WELLBORE_RADIUS.Value, nameof(data.WELLBORE_RADIUS))
                    : new WellTestDataProblem(nameof(data.WELLBORE_RADIUS), "Wellbore radius is required."))
                ?? FormationThicknessProblem((double)data.FORMATION_THICKNESS, nameof(data.FORMATION_THICKNESS))
                ?? (data.POROSITY.HasValue
                    ? PorosityProblem((double)data.POROSITY.Value, nameof(data.POROSITY))
                    : new WellTestDataProblem(nameof(data.POROSITY), "Porosity is required."))
                ?? (data.TOTAL_COMPRESSIBILITY.HasValue
                    ? CompressibilityProblem((double)data.TOTAL_COMPRESSIBILITY.Value, nameof(data.TOTAL_COMPRESSIBILITY))
                    : new WellTestDataProblem(nameof(data.TOTAL_COMPRESSIBILITY), "Total compressibility is required."))
                ?? (data.OIL_VISCOSITY.HasValue
                    ? ViscosityProblem((double)data.OIL_VISCOSITY.Value, nameof(data.OIL_VISCOSITY))
                    : new WellTestDataProblem(nameof(data.OIL_VISCOSITY), "Oil viscosity is required."))
                ?? (data.OIL_FORMATION_VOLUME_FACTOR.HasValue
                    ? FormationVolumeFactorProblem((double)data.OIL_FORMATION_VOLUME_FACTOR.Value, nameof(data.OIL_FORMATION_VOLUME_FACTOR))
                    : new WellTestDataProblem(nameof(data.OIL_FORMATION_VOLUME_FACTOR), "Oil formation volume factor is required."))
                ?? BuildUpProductionTimeProblem(data);
        }

        /// <summary>
        /// Validates time data array.
        /// </summary>
        public static void ValidateTimeData(System.Collections.Generic.List<double> time, string parameterName)
            => Refuse(TimeDataProblem(time, parameterName));

        /// <summary>
        /// Validates pressure data array.
        /// </summary>
        public static void ValidatePressureData(System.Collections.Generic.List<double> pressure, string parameterName)
            => Refuse(PressureDataProblem(pressure, parameterName));

        /// <summary>
        /// Validates that two arrays have matching lengths.
        /// </summary>
        public static void ValidateMatchingLengths(System.Collections.Generic.List<double> array1,
            System.Collections.Generic.List<double> array2, string name1, string name2)
            => Refuse(MatchingLengthsProblem(array1, array2, name1, name2));

        /// <summary>
        /// Validates flow rate.
        /// </summary>
        public static void ValidateFlowRate(double flowRate, string parameterName)
            => Refuse(FlowRateProblem(flowRate, parameterName));

        /// <summary>
        /// Validates wellbore radius.
        /// </summary>
        public static void ValidateWellboreRadius(double radius, string parameterName)
            => Refuse(WellboreRadiusProblem(radius, parameterName));

        /// <summary>
        /// Validates formation thickness.
        /// </summary>
        public static void ValidateFormationThickness(double thickness, string parameterName)
            => Refuse(FormationThicknessProblem(thickness, parameterName));

        /// <summary>
        /// Validates porosity.
        /// </summary>
        public static void ValidatePorosity(double porosity, string parameterName)
            => Refuse(PorosityProblem(porosity, parameterName));

        /// <summary>
        /// Validates compressibility.
        /// </summary>
        public static void ValidateCompressibility(double compressibility, string parameterName)
            => Refuse(CompressibilityProblem(compressibility, parameterName));

        /// <summary>
        /// Validates viscosity.
        /// </summary>
        public static void ValidateViscosity(double viscosity, string parameterName)
            => Refuse(ViscosityProblem(viscosity, parameterName));

        /// <summary>
        /// Validates formation volume factor.
        /// </summary>
        public static void ValidateFormationVolumeFactor(double fvf, string parameterName)
            => Refuse(FormationVolumeFactorProblem(fvf, parameterName));

        private static void Refuse(WellTestDataProblem? problem)
        {
            if (problem is not null)
                throw new InvalidWellTestDataException(problem.ParameterName, problem.Sentence);
        }

        private static WellTestDataProblem? BuildUpProductionTimeProblem(WELL_TEST_DATA data)
        {
            if (!string.IsNullOrWhiteSpace(data.TEST_TYPE) &&
                Enum.TryParse<WellTestType>(data.TEST_TYPE, ignoreCase: true, out var testType) &&
                testType == WellTestType.BuildUp &&
                (!data.PRODUCTION_TIME.HasValue || data.PRODUCTION_TIME.Value <= 0))
            {
                return new WellTestDataProblem(nameof(data.PRODUCTION_TIME),
                    "Production time must be positive for build-up (Horner / MDH) analysis.");
            }

            return null;
        }

        private static WellTestDataProblem? TimeDataProblem(System.Collections.Generic.List<double> time, string parameterName)
        {
            if (time == null || time.Count == 0)
                return new WellTestDataProblem(parameterName, "Time data cannot be null or empty.");

            if (time.Count < 3)
                return new WellTestDataProblem(parameterName, "At least 3 time points are required for analysis.");

            if (time.Any(t => t < WellTestConstants.MinTime || t > WellTestConstants.MaxTime))
                return new WellTestDataProblem(parameterName,
                    $"Time values must be between {WellTestConstants.MinTime} and {WellTestConstants.MaxTime} hours.");

            // Check for chronological order
            for (int i = 1; i < time.Count; i++)
            {
                if (time[i] <= time[i - 1])
                    return new WellTestDataProblem(parameterName, "Time values must be in chronological order.");
            }

            return null;
        }

        private static WellTestDataProblem? PressureDataProblem(System.Collections.Generic.List<double> pressure, string parameterName)
        {
            if (pressure == null || pressure.Count == 0)
                return new WellTestDataProblem(parameterName, "Pressure data cannot be null or empty.");

            if (pressure.Any(p => p < WellTestConstants.MinPressure || p > WellTestConstants.MaxPressure))
                return new WellTestDataProblem(parameterName,
                    $"Pressure values must be between {WellTestConstants.MinPressure} and {WellTestConstants.MaxPressure} psi.");

            return null;
        }

        private static WellTestDataProblem? MatchingLengthsProblem(System.Collections.Generic.List<double> array1,
            System.Collections.Generic.List<double> array2, string name1, string name2)
        {
            if (array1.Count != array2.Count)
                return new WellTestDataProblem(name1, $"{name1} and {name2} must have the same length.");

            return null;
        }

        private static WellTestDataProblem? FlowRateProblem(double flowRate, string parameterName)
        {
            if (flowRate < WellTestConstants.MinFlowRate || flowRate > WellTestConstants.MaxFlowRate)
                return new WellTestDataProblem(parameterName,
                    $"Flow rate must be between {WellTestConstants.MinFlowRate} and {WellTestConstants.MaxFlowRate} BPD.");

            return null;
        }

        private static WellTestDataProblem? WellboreRadiusProblem(double radius, string parameterName)
        {
            if (radius < WellTestConstants.MinWellboreRadius || radius > WellTestConstants.MaxWellboreRadius)
                return new WellTestDataProblem(parameterName,
                    $"Wellbore radius must be between {WellTestConstants.MinWellboreRadius} and {WellTestConstants.MaxWellboreRadius} feet.");

            return null;
        }

        private static WellTestDataProblem? FormationThicknessProblem(double thickness, string parameterName)
        {
            if (thickness < WellTestConstants.MinFormationThickness || thickness > WellTestConstants.MaxFormationThickness)
                return new WellTestDataProblem(parameterName,
                    $"Formation thickness must be between {WellTestConstants.MinFormationThickness} and {WellTestConstants.MaxFormationThickness} feet.");

            return null;
        }

        private static WellTestDataProblem? PorosityProblem(double porosity, string parameterName)
        {
            if (porosity < WellTestConstants.MinPorosity || porosity > WellTestConstants.MaxPorosity)
                return new WellTestDataProblem(parameterName,
                    $"Porosity must be between {WellTestConstants.MinPorosity} and {WellTestConstants.MaxPorosity}.");

            return null;
        }

        private static WellTestDataProblem? CompressibilityProblem(double compressibility, string parameterName)
        {
            if (compressibility < WellTestConstants.MinCompressibility || compressibility > WellTestConstants.MaxCompressibility)
                return new WellTestDataProblem(parameterName,
                    $"Compressibility must be between {WellTestConstants.MinCompressibility} and {WellTestConstants.MaxCompressibility} psi^-1.");

            return null;
        }

        private static WellTestDataProblem? ViscosityProblem(double viscosity, string parameterName)
        {
            if (viscosity < WellTestConstants.MinViscosity || viscosity > WellTestConstants.MaxViscosity)
                return new WellTestDataProblem(parameterName,
                    $"Viscosity must be between {WellTestConstants.MinViscosity} and {WellTestConstants.MaxViscosity} cp.");

            return null;
        }

        private static WellTestDataProblem? FormationVolumeFactorProblem(double fvf, string parameterName)
        {
            if (fvf <= 0 || fvf > 10.0)
                return new WellTestDataProblem(parameterName,
                    "Formation volume factor must be positive and less than 10.0 RB/STB.");

            return null;
        }
    }
}
