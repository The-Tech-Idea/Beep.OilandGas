using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.ProspectIdentification.Services;
using Moq;
using TheTechIdea.Beep.Editor;
using Xunit;

namespace Beep.OilandGas.ProspectIdentification.Tests;

/// <summary>
/// What a caller sends that cannot be done as sent is refused (OILGAS-CATCH-01): a <see cref="RefusalException"/> of kind
/// <see cref="RefusalKind.Invalid"/>, which the API answers 400 with its sentence. A guard against the program's own misuse
/// (a null body, no acting user) stays an <see cref="ArgumentException"/>: a fault, reported and answered 500.
/// </summary>
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

    private static async Task AssertInvalidAsync(Func<Task> act)
    {
        var refusal = await Assert.ThrowsAsync<RefusalException>(act);
        Assert.Equal(RefusalKind.Invalid, refusal.Kind);
        Assert.False(string.IsNullOrWhiteSpace(refusal.Sentence));
    }

    [Fact]
    public async Task EvaluateProspectAsync_EmptyId_IsRefusedAsInvalid()
    {
        var sut = CreateSut();
        await AssertInvalidAsync(() => sut.EvaluateProspectAsync("  "));
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
    public async Task RankProspectsAsync_EmptyIds_IsRefusedAsInvalid()
    {
        var sut = CreateSut();
        await AssertInvalidAsync(() =>
            sut.RankProspectsAsync(new List<string>(), new Dictionary<string, decimal> { ["k"] = 1m }));
    }

    [Fact]
    public async Task RankProspectsAsync_EmptyCriteria_IsRefusedAsInvalid()
    {
        var sut = CreateSut();
        await AssertInvalidAsync(() =>
            sut.RankProspectsAsync(new List<string> { "A" }, new Dictionary<string, decimal>()));
    }

    [Fact]
    public async Task AnalyzeSeismicInterpretationAsync_EmptyProspectId_IsRefusedAsInvalid()
    {
        var sut = CreateSut();
        await AssertInvalidAsync(() =>
            sut.AnalyzeSeismicInterpretationAsync("", "S1", new List<Horizon>(), new List<Fault>()));
    }

    [Fact]
    public async Task OptimizePortfolioAsync_EmptyRanked_IsRefusedAsInvalid()
    {
        var sut = CreateSut();
        await AssertInvalidAsync(() =>
            sut.OptimizePortfolioAsync(new List<ProspectRanking>(), 1m, 1m));
    }
}
