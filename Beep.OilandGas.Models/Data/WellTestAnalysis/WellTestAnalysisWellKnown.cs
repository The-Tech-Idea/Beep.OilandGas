using System;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// Canonical wire values for well test PTA requests, persisted <c>ANALYSIS_TYPE</c> / <c>ANALYSIS_METHOD</c> tokens,
    /// and stable fragments used when interpreting PPDM <c>WELL_TEST.TEST_TYPE</c> (reference set <c>R_WELL_TEST_TYPE</c>
    /// seeded from <see cref="WellTestType"/> in data-management seeders).
    /// Compare user or legacy input with <see cref="StringComparison.OrdinalIgnoreCase"/> unless noted otherwise.
    /// </summary>
    public static class WellTestAnalysisWellKnown
    {
        /// <summary>API / storage tokens for build-up vs drawdown routing (uppercase).</summary>
        public static class AnalysisClassification
        {
            public const string BuildUp = "BUILDUP";
            public const string DrawDown = "DRAWDOWN";
        }

        /// <summary>API / storage tokens for Horner vs MDH (uppercase).</summary>
        public static class AnalysisMethod
        {
            public const string Horner = "HORNER";
            public const string Mdh = "MDH";
        }

        /// <summary>Uppercase fragments after normalizing free-text <c>WELL_TEST.TEST_TYPE</c> for mapper-style heuristics.</summary>
        public static class PpdmTestTypeFragmentUpper
        {
            public const string Build = "BUILD";
            public const string Bu = "BU";
            public const string ShutIn = "SHUTIN";
            public const string DrawDown = "DRAWDOWN";
            public const string Dd = "DD";
            public const string Flow = "FLOW";
        }

        /// <summary>Case-insensitive substring markers on raw <c>TEST_TYPE</c> text (legacy / mixed casing).</summary>
        public static class TestTypeToken
        {
            public const string Draw = "DRAW";
            public const string Mdh = "MDH";
        }

        /// <summary>Human-readable labels stored on <see cref="WELL_TEST_ANALYSIS_RESULT.ANALYSIS_METHOD"/> from the analyzer library (not the uppercase API tokens).</summary>
        public static class ResultAnalysisMethodLabel
        {
            public const string Horner = "Horner";
            public const string Mdh = "MDH";
            public const string DrawdownSemiLog = "Drawdown (Semi-Log)";
            public const string GasPseudoPressureHorner = "Gas Pseudo-Pressure (Horner)";
        }

        public static bool ClassificationEqualsBuildUp(string? value) =>
            string.Equals(value, AnalysisClassification.BuildUp, StringComparison.OrdinalIgnoreCase);

        public static bool ClassificationEqualsDrawDown(string? value) =>
            string.Equals(value, AnalysisClassification.DrawDown, StringComparison.OrdinalIgnoreCase);

        public static bool MethodEqualsHorner(string? value) =>
            string.Equals(value, AnalysisMethod.Horner, StringComparison.OrdinalIgnoreCase);

        public static bool MethodEqualsMdh(string? value) =>
            string.Equals(value, AnalysisMethod.Mdh, StringComparison.OrdinalIgnoreCase);
    }
}
