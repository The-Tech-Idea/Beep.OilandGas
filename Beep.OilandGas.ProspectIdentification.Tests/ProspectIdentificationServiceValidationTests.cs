using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProspectIdentification.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ProspectIdentification.Tests;

public class ProspectIdentificationServiceValidationTests
{
    private static ProspectIdentificationService CreateSut()
    {
        var editor = new Mock<IDMEEditor>();
        var common = new Mock<ICommonColumnHandler>();
        var defaults = new Mock<IPPDM39DefaultsRepository>();
        defaults.Setup(d => d.FormatIdForTable(It.IsAny<string>(), It.IsAny<string>()))
            .Returns((string t, string id) => $"{t}:{id}");
        var metadata = new Mock<IPPDMMetadataRepository>();
        return new ProspectIdentificationService(
            editor.Object,
            common.Object,
            defaults.Object,
            metadata.Object,
            "PPDM39");
    }

    [Fact]
    public async Task EvaluateProspectAsync_EmptyId_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(() => sut.EvaluateProspectAsync("  "));
    }

    [Fact]
    public async Task CreateProspectAsync_NullProspect_ThrowsArgumentNullException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentNullException>(() => sut.CreateProspectAsync(null!, "u1"));
    }

    [Fact]
    public async Task CreateProspectAsync_EmptyUserId_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.CreateProspectAsync(new Prospect { ProspectName = "X", FieldId = "F" }, "  "));
    }

    [Fact]
    public async Task RankProspectsAsync_EmptyIds_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.RankProspectsAsync(new List<string>(), new Dictionary<string, decimal> { ["k"] = 1m }));
    }

    [Fact]
    public async Task RankProspectsAsync_EmptyCriteria_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.RankProspectsAsync(new List<string> { "A" }, new Dictionary<string, decimal>()));
    }

    [Fact]
    public async Task AnalyzeSeismicInterpretationAsync_EmptyProspectId_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.AnalyzeSeismicInterpretationAsync("", "S1", new List<Horizon>(), new List<Fault>()));
    }

    [Fact]
    public async Task OptimizePortfolioAsync_EmptyRanked_ThrowsArgumentException()
    {
        var sut = CreateSut();
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.OptimizePortfolioAsync(new List<ProspectRanking>(), 1m, 1m));
    }
}
