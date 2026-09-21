using TheTechIdea.Beep.ConfigUtil;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;


using Beep.OilandGas.UserManagement.Services;
using Moq;
using TheTechIdea.Beep;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class PersonaRoutingAndSeedTests
{
    private readonly Mock<IDMEEditor> _editor = new();
    private readonly Mock<IDataSource> _source = new();
    private readonly Mock<ICommonColumnHandler> _columns = new();
    private readonly Mock<IPPDM39DefaultsRepository> _defaults = new();
    private readonly Mock<IPPDMMetadataRepository> _metadata = new();
    private readonly Dictionary<string, List<object>> _rows = new();

    public PersonaRoutingAndSeedTests()
    {
        _editor.Setup(editor => editor.GetDataSource("test")).Returns(_source.Object);
        _source.Setup(source => source.GetEntityAsync(It.IsAny<string>(), It.IsAny<List<AppFilter>>()))
            .Returns((string table, List<AppFilter> filters) => Task.FromResult<IEnumerable<object>>(
                Rows(table).Where(row => filters.All(filter =>
                    Equals(row.GetType().GetProperty(filter.FieldName)!.GetValue(row)?.ToString(), filter.FilterValue)))
                .ToList()));
        _source.Setup(source => source.InsertEntity(It.IsAny<string>(), It.IsAny<object>()))
            .Callback((string table, object row) => Rows(table).Add(row))
            .Returns(new ErrorsInfo { Flag = Errors.Ok });
    }

    private List<object> Rows(string table)
    {
        if (!_rows.TryGetValue(table, out var rows)) _rows[table] = rows = new();
        return rows;
    }

    [Fact]
    public async Task SodSeedCountsOnlyNewRulesAcrossRepeatRuns()
    {
        var service = new SodEvaluationEngine(_editor.Object, _columns.Object, _defaults.Object, _metadata.Object, () => Task.FromResult("test"));
        var first = await service.SeedDefaultRulesAsync("actor");
        Assert.True(first > 0);
        Assert.Equal(Rows("SOD_RULE").Count, first);
        Assert.Equal(0, await service.SeedDefaultRulesAsync("actor"));
        Rows("SOD_RULE").RemoveAt(0);
        Assert.Equal(1, await service.SeedDefaultRulesAsync("actor"));
        Assert.Equal(first, Rows("SOD_RULE").Count);
    }

    [Fact]
    public async Task SodSeedPropagatesInsertFailure()
    {
        _source.Setup(source => source.InsertEntity("SOD_RULE", It.IsAny<object>()))
            .Throws(new InvalidOperationException("write failed"));
        var service = new SodEvaluationEngine(_editor.Object, _columns.Object, _defaults.Object, _metadata.Object, () => Task.FromResult("test"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.SeedDefaultRulesAsync("actor"));
    }

    [Fact]
    public async Task SodSeedHonorsCancellationBeforeWriting()
    {
        var service = new SodEvaluationEngine(_editor.Object, _columns.Object, _defaults.Object, _metadata.Object, () => Task.FromResult("test"));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SeedDefaultRulesAsync("actor", new CancellationToken(true)));
        Assert.Empty(Rows("SOD_RULE"));
    }
}
