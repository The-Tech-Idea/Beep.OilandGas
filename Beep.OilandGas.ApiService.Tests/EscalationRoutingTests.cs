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

public class EscalationRoutingTests
{
    [Theory]
    [InlineData("REASSIGN_TO_BACKUP")]
    [InlineData("AUTO_ESCALATE_LEVEL")]
    [InlineData("SUSPEND_PROCESS")]
    public async Task MissingBindingCannotWriteOrReportSuccessfulUpdate(string action)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, null);
        var result = await service.ExecuteEscalationAsync("instance", "step", action, "target", "actor");
        Assert.False(result.Success);
        Assert.Contains("bound LIFECYCLE", result.Message);
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SuspensionPinsOneTargetAcrossReadAndUpdate()
    {
        var instance = new PROCESS_INSTANCE { PROCESS_INSTANCE_ID = "instance", STATUS = "IN_PROGRESS" };
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("PROCESS_INSTANCE", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<PROCESS_INSTANCE> { instance });
        source.Setup(x => x.UpdateEntity("PROCESS_INSTANCE", It.IsAny<object>()))
            .Returns(new ErrorsInfo { Flag = Errors.Ok });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("selected")).Returns(source.Object);
        var resolutions = 0;
        var service = Create(editor.Object, () => Task.FromResult(++resolutions == 1 ? "selected" : "wrong"));
        var result = await service.ExecuteEscalationAsync("instance", "step", "SUSPEND_PROCESS", null, "actor");
        Assert.True(result.Success, result.Message);
        Assert.Equal("SUSPENDED", instance.STATUS);
        Assert.Equal(1, resolutions);
        source.Verify(x => x.GetEntityAsync("PROCESS_INSTANCE", It.IsAny<List<AppFilter>>()), Times.Once);
        source.Verify(x => x.UpdateEntity("PROCESS_INSTANCE", instance), Times.Once);
        editor.Verify(x => x.GetDataSource("selected"), Times.Exactly(2));
        editor.VerifyNoOtherCalls();
    }

    private static EscalationActionService Create(IDMEEditor editor, Func<Task<string>>? connection) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), connection);
}
