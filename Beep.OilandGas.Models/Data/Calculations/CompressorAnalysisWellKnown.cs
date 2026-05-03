namespace Beep.OilandGas.Models.Data.Calculations
{
    /// <summary>
    /// Canonical string values for <see cref="CompressorAnalysisRequest"/> and facility/orchestrated compressor analysis.
    /// Compare with <c>Trim().ToUpperInvariant()</c> when matching user or API input.
    /// </summary>
    public static class CompressorAnalysisWellKnown
    {
        public static class AnalysisType
        {
            public const string Power = "POWER";
            public const string Pressure = "PRESSURE";
            public const string Efficiency = "EFFICIENCY";
        }

        public static class CompressorType
        {
            public const string Centrifugal = "CENTRIFUGAL";
            public const string Reciprocating = "RECIPROCATING";
        }
    }
}
