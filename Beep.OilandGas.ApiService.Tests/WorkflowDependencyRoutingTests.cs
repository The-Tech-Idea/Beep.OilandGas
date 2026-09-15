using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class WorkflowDependencyRoutingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingBindingRejectsAllDatabaseOperations(bool blankResolver)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, blankResolver ? () => Task.FromResult(" ") : null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.AddDependencyAsync(
            "dependent", null, "prerequisite", null, "BLOCKING", null, "actor"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CheckPrerequisitesAsync("instance", null));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetDependenciesAsync("dependent"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetFullGraphAsync());
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PrerequisiteCheckPinsTheBindingAcrossAllReads()
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.SetupSequence(x => x.GetEntityAsync("PROCESS_INSTANCE", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<PROCESS_INSTANCE> { new()
            {
                PROCESS_INSTANCE_ID = "instance", PROCESS_DEFINITION_ID = "dependent",
                ENTITY_TYPE = "WELL", ENTITY_ID = "well"
            } })
            .ReturnsAsync(new List<PROCESS_INSTANCE> { new() { STATUS = "COMPLETED" } });
        source.Setup(x => x.GetEntityAsync("WORKFLOW_DEPENDENCY", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<WORKFLOW_DEPENDENCY> { new()
            {
                DEPENDENT_PROCESS_DEF_ID = "dependent", PREREQUISITE_PROCESS_DEF_ID = "prerequisite",
                DEPENDENCY_TYPE = "BLOCKING"
            } });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("selected")).Returns(source.Object);
        var resolutions = 0;
        var service = Create(editor.Object, () => Task.FromResult(++resolutions == 1 ? "selected" : "wrong"));

        var result = await service.CheckPrerequisitesAsync("instance", null);

        Assert.True(result.CanProceed);
        Assert.Single(result.SatisfiedPrerequisites);
        Assert.Equal(1, resolutions);
        source.Verify(x => x.GetEntityAsync("PROCESS_INSTANCE", It.IsAny<List<AppFilter>>()), Times.Exactly(2));
        source.Verify(x => x.GetEntityAsync("WORKFLOW_DEPENDENCY", It.IsAny<List<AppFilter>>()), Times.Once);
        editor.Verify(x => x.GetDataSource("selected"), Times.Exactly(3));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task LaterOperationsUseTheNewBindingForReadsAndWrites()
    {
        var first = new Mock<IDataSource>(MockBehavior.Strict);
        first.Setup(x => x.GetEntityAsync("WORKFLOW_DEPENDENCY", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<WORKFLOW_DEPENDENCY>());
        var second = new Mock<IDataSource>(MockBehavior.Strict);
        second.Setup(x => x.InsertEntity("WORKFLOW_DEPENDENCY", It.IsAny<object>()))
            .Returns(new ErrorsInfo { Flag = Errors.Ok });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("first")).Returns(first.Object);
        editor.Setup(x => x.GetDataSource("second")).Returns(second.Object);
        var binding = "first";
        var service = Create(editor.Object, () => Task.FromResult(binding));

        Assert.Empty(await service.GetFullGraphAsync());
        binding = "second";
        var dependency = await service.AddDependencyAsync("dependent", null, "prerequisite", null,
            "BLOCKING", null, "actor");

        first.Verify(x => x.GetEntityAsync("WORKFLOW_DEPENDENCY", It.IsAny<List<AppFilter>>()), Times.Once);
        second.Verify(x => x.InsertEntity("WORKFLOW_DEPENDENCY", dependency), Times.Once);
        editor.Verify(x => x.GetDataSource("first"), Times.Once);
        editor.Verify(x => x.GetDataSource("second"), Times.Once);
        editor.VerifyNoOtherCalls();
    }

    private static WorkflowDependencyGraphService Create(IDMEEditor editor, Func<Task<string>>? connection) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), connection);
}
