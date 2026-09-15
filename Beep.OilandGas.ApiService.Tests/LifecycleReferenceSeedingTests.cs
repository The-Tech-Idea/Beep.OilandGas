using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Modules;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class LifecycleReferenceSeedingTests
{
    [Fact]
    public async Task ReferenceSetsUseDeclaredTableAndSelectedConnectionAndSkipExistingRows()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var stores = new Dictionary<string, List<R_LIFECYCLE_STATE_REFERENCE>>();
        foreach (var connection in new[] { "lifecycle-a", "lifecycle-b" })
        {
            var rows = new List<R_LIFECYCLE_STATE_REFERENCE>();
            stores.Add(connection, rows);
            var source = new Mock<IDataSource>(MockBehavior.Strict);
            source.Setup(x => x.GetEntityAsync(nameof(R_LIFECYCLE_STATE_REFERENCE), It.IsAny<List<AppFilter>>()))
                .Returns((string _, List<AppFilter> filters) => Task.FromResult<IEnumerable<object>>(rows.Where(row =>
                    row.REFERENCE_SET == filters.Single(f => f.FieldName == "REFERENCE_SET").FilterValue &&
                    row.REFERENCE_CODE == filters.Single(f => f.FieldName == "REFERENCE_CODE").FilterValue).ToList()));
            source.Setup(x => x.InsertEntity(nameof(R_LIFECYCLE_STATE_REFERENCE), It.IsAny<object>()))
                .Callback<string, object>((_, row) => rows.Add((R_LIFECYCLE_STATE_REFERENCE)row))
                .Returns(new ErrorsInfo { Flag = Errors.Ok });
            editor.Setup(x => x.GetDataSource(connection)).Returns(source.Object);
        }
        var module = CreateModule(editor.Object);
        Assert.Contains(typeof(R_LIFECYCLE_STATE_REFERENCE), module.EntityTypes);

        var first = await module.SeedAsync("lifecycle-a", "admin-a");
        Assert.True(first.Success);
        Assert.Empty(first.Errors);
        Assert.True(first.RecordsInserted > 0);
        Assert.Empty(stores["lifecycle-b"]);
        Assert.All(stores["lifecycle-a"], row => Assert.Equal("admin-a", row.ROW_CREATED_BY));
        Assert.Contains(stores["lifecycle-a"], row => row.REFERENCE_SET == "PROCESS_STATUS");
        Assert.Contains(stores["lifecycle-a"], row => row.REFERENCE_SET == "TRANSITION_CONDITION");

        var replay = await module.SeedAsync("lifecycle-a", "admin-a");
        Assert.True(replay.Success);
        Assert.Equal(0, replay.RecordsInserted);
        var second = await module.SeedAsync("lifecycle-b", "admin-b");
        Assert.True(second.Success);
        Assert.Equal(first.RecordsInserted, second.RecordsInserted);
        Assert.All(stores["lifecycle-b"], row => Assert.Equal("admin-b", row.ROW_CREATED_BY));
        Assert.Equal(first.RecordsInserted, stores["lifecycle-a"].Count);
    }

    [Fact]
    public async Task CancelledSeedingDoesNotAccessAnyDatasource()
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateModule(editor.Object).SeedAsync("lifecycle-a", "admin", new CancellationToken(true)));
        editor.VerifyNoOtherCalls();
    }

    private static LifeCycleModule CreateModule(IDMEEditor editor) => new(new ModuleSetupContext
    {
        Editor = editor, CommonColumnHandler = Mock.Of<ICommonColumnHandler>(),
        Defaults = Mock.Of<IPPDM39DefaultsRepository>(), Metadata = Mock.Of<IPPDMMetadataRepository>(),
        ConnectionName = "forbidden-global-default", Logger = NullLogger.Instance
    });
}
