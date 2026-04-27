// Physical location: Beep.OilandGas.ProspectIdentification — namespace matches Models.Core.Interfaces.
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Technical maturation and G&G-style analysis helpers for prospects (deterministic projections).
    /// Implemented by <see cref="Beep.OilandGas.ProspectIdentification.Services.ProspectIdentificationService"/>.
    /// </summary>
    public interface IProspectTechnicalMaturationService
    {
        Task<SeismicInterpretationAnalysis> AnalyzeSeismicInterpretationAsync(
            string prospectId,
            string surveyId,
            List<Horizon> horizons,
            List<Fault> faults);

        Task<ResourceEstimationResult> EstimateResourcesAsync(
            string prospectId,
            decimal grossRockVolume,
            decimal netToGrossRatio,
            decimal porosity,
            decimal waterSaturation,
            string estimatedBy);

        Task<TrapGeometryAnalysis> AnalyzeTrapGeometryAsync(
            string prospectId,
            string trapType,
            decimal crestDepth,
            decimal spillPointDepth,
            decimal area,
            decimal volume);

        Task<MigrationPathAnalysis> AnalyzeMigrationPathAsync(
            string prospectId,
            string sourceRockId,
            decimal maturityLevel,
            decimal distance);

        Task<SealSourceAssessment> AssessSealAndSourceAsync(
            string prospectId,
            string sealRockType,
            decimal sealThickness,
            string sourceRockType,
            decimal sourceMaturity);
    }
}
