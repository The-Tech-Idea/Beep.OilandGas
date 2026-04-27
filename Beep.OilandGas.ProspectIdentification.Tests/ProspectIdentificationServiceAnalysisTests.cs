using System.Collections.Generic;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProspectIdentification.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ProspectIdentification.Tests;

/// <summary>
/// Deterministic branches of <see cref="ProspectIdentificationService"/> that do not require a live PPDM connection.
/// </summary>
public class ProspectIdentificationServiceAnalysisTests
{
    private static ProspectIdentificationService CreateSut(Mock<IPPDM39DefaultsRepository>? defaultsOverride = null)
    {
        var editor = new Mock<IDMEEditor>();
        var common = new Mock<ICommonColumnHandler>();
        var defaults = defaultsOverride ?? new Mock<IPPDM39DefaultsRepository>();
        if (defaultsOverride == null)
        {
            defaults.Setup(d => d.FormatIdForTable(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string table, string id) => $"{table}:{id}");
        }

        var metadata = new Mock<IPPDMMetadataRepository>();
        return new ProspectIdentificationService(
            editor.Object,
            common.Object,
            defaults.Object,
            metadata.Object,
            "PPDM39");
    }

    [Fact]
    public async Task AnalyzeSeismicInterpretationAsync_SetsCountsAndIds()
    {
        var sut = CreateSut();
        var horizons = new List<Horizon> { new() };
        var faults = new List<Fault> { new(), new() };

        var result = await sut.AnalyzeSeismicInterpretationAsync("P-1", "S-9", horizons, faults);

        Assert.Equal("P-1", result.ProspectId);
        Assert.Equal("S-9", result.SurveyId);
        Assert.Equal(1, result.HorizonCount);
        Assert.Equal(2, result.FaultCount);
        Assert.StartsWith("SEISMIC_INT:", result.AnalysisId, StringComparison.Ordinal);
    }

    [Fact]
    public async Task EstimateResourcesAsync_ComputesNetRockAndHydrocarbonVolumes()
    {
        var sut = CreateSut();
        const decimal grv = 1_000_000m;
        const decimal ngr = 0.5m;
        const decimal phi = 0.2m;
        const decimal sw = 0.3m;

        var result = await sut.EstimateResourcesAsync("P-2", grv, ngr, phi, sw, "analyst");

        Assert.Equal(500_000m, result.NetRockVolume);
        Assert.Equal(grv * ngr * phi * (1m - sw) * 0.15m, result.EstimatedOilVolume);
        Assert.Equal(grv * ngr * phi * (1m - sw) * 0.80m, result.EstimatedGasVolume);
    }

    [Fact]
    public async Task PerformRiskAssessmentAsync_MultipliesExplicitScores()
    {
        var sut = CreateSut();
        var scores = new Dictionary<string, decimal>
        {
            ["Trap"] = 0.5m,
            ["Seal"] = 0.5m,
            ["Source"] = 0.5m,
            ["Migration"] = 0.5m,
            ["Characterization"] = 0.5m
        };

        var result = await sut.PerformRiskAssessmentAsync("P-3", "geo", scores);

        const decimal expected = 0.5m * 0.5m * 0.5m * 0.5m * 0.5m;
        Assert.Equal(expected, result.OverallRisk);
        Assert.Equal(1m - expected, result.ProbabilityOfSuccess);
        Assert.Equal("Low", result.OverallRiskLevel);
    }

    [Fact]
    public async Task OptimizePortfolioAsync_HighScoreWithinBudget_GoesToRecommended()
    {
        var defaults = new Mock<IPPDM39DefaultsRepository>();
        defaults.Setup(d => d.FormatIdForTable(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string t, string id) => $"{t}:{id}");
        var sut = CreateSut(defaults);

        var ranked = new List<ProspectRanking>
        {
            new() { ProspectId = "A", Score = 80m, Rank = 1 }
        };

        var result = await sut.OptimizePortfolioAsync(ranked, riskTolerance: 1m, capitalBudget: 1_000_000m);

        Assert.Contains("A", result.RecommendedProspects);
        Assert.StartsWith("PORTFOLIO_OPT:", result.OptimizationId, StringComparison.Ordinal);
    }
}
