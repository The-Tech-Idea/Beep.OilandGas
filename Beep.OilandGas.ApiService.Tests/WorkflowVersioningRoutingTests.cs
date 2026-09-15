using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class WorkflowVersioningRoutingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingBindingRejectsEveryPersistenceEntryPoint(bool blankResolver)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = new WorkflowVersioningService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            blankResolver ? () => Task.FromResult(" ") : null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateVersionAsync(new ProcessDefinition { ProcessId = "process" }, "change", "actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetVersionHistoryAsync("process"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetLatestVersionAsync("process"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetInstanceVersionAsync("instance"));
        var migration = await service.MigrateInstanceAsync("instance", "version", "actor");
        Assert.False(migration.Success);
        Assert.Contains("bound LIFECYCLE", migration.ErrorMessage);
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task VersionCreationPinsOneBindingAcrossHistoryReadAndInsert()
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("WORKFLOW_VERSION", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<WORKFLOW_VERSION> { new() { VERSION_NUMBER = "1.0" } });
        source.Setup(x => x.InsertEntity("WORKFLOW_VERSION", It.IsAny<object>()))
            .Returns(new ErrorsInfo { Flag = Errors.Ok });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("workflow-selected")).Returns(source.Object);
        var resolutions = 0;
        var service = new WorkflowVersioningService(editor.Object, Mock.Of<ICommonColumnHandler>(),
            Mock.Of<IPPDM39DefaultsRepository>(), Mock.Of<IPPDMMetadataRepository>(),
            () => Task.FromResult(++resolutions == 1 ? "workflow-selected" : "wrong-target"));
        var definition = new ProcessDefinition { ProcessId = "process" };
        var version = await service.CreateVersionAsync(definition, "change", "actor");
        Assert.Equal("1.1", version.VERSION_NUMBER);
        Assert.Equal(1, resolutions);
        editor.Verify(x => x.GetDataSource("workflow-selected"), Times.Exactly(2));
        source.Verify(x => x.InsertEntity("WORKFLOW_VERSION", version), Times.Once);
        source.Verify(x => x.GetEntityAsync("WORKFLOW_VERSION", It.IsAny<List<AppFilter>>()), Times.Once);
        editor.VerifyNoOtherCalls();
    }
}
