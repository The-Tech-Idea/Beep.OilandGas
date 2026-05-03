using System.Collections.Generic;
using Beep.OilandGas.Models.Data.Calculations;

namespace Beep.OilandGas.CompressorAnalysis.Constants;

/// <summary>
/// Canonical seed rows for <c>R_COMPRESSOR_ANALYSIS_REFERENCE_CODE</c> — kept in sync with
/// <see cref="CompressorAnalysisWellKnown"/> so UI/API picklists match orchestration.
/// </summary>
public static class CompressorAnalysisReferenceCodeSeed
{
    public readonly record struct SeedRow(string ReferenceSet, string ReferenceCode, string LongName, string ActiveInd = "Y");

    public static IEnumerable<SeedRow> GetAllSeedRows()
    {
        yield return new SeedRow(
            CompressorAnalysisReferenceSets.AnalysisType,
            CompressorAnalysisWellKnown.AnalysisType.Power,
            "Power — polytropic head / brake horsepower path");

        yield return new SeedRow(
            CompressorAnalysisReferenceSets.AnalysisType,
            CompressorAnalysisWellKnown.AnalysisType.Pressure,
            "Pressure — discharge capability search within driver HP cap");

        yield return new SeedRow(
            CompressorAnalysisReferenceSets.AnalysisType,
            CompressorAnalysisWellKnown.AnalysisType.Efficiency,
            "Efficiency — packaged power path (efficiency emphasis)");

        yield return new SeedRow(
            CompressorAnalysisReferenceSets.CompressorKind,
            CompressorAnalysisWellKnown.CompressorType.Centrifugal,
            "Centrifugal");

        yield return new SeedRow(
            CompressorAnalysisReferenceSets.CompressorKind,
            CompressorAnalysisWellKnown.CompressorType.Reciprocating,
            "Reciprocating");
    }
}
