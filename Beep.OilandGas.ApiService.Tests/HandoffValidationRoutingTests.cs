using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class HandoffValidationRoutingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingBindingRejectsDatabaseReads(bool blankResolver)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, blankResolver ? () => Task.FromResult(" ") : null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetHandoffContractAsync("process", "from"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ValidateHandoffAsync(
            "instance", "from", "to", "WELL", "well", new()));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ValidationPinsTheBindingButNextOperationResolvesAgain()
    {
        var first = new Mock<IDataSource>(MockBehavior.Strict);
        first.Setup(x => x.GetEntityAsync("PROCESS_INSTANCE", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<PROCESS_INSTANCE> { new() { PROCESS_DEFINITION_ID = "process" } });
        var contract = new ROLE_HANDOFF_CONTRACT { REQUIRED_DATA_FIELDS_JSON = "[\"DEPTH\"]" };
        first.Setup(x => x.GetEntityAsync("ROLE_HANDOFF_CONTRACT", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<ROLE_HANDOFF_CONTRACT> { contract });
        var second = new Mock<IDataSource>(MockBehavior.Strict);
        var nextContract = new ROLE_HANDOFF_CONTRACT { REQUIRED_DATA_FIELDS_JSON = "[\"PRESSURE\"]" };
        second.Setup(x => x.GetEntityAsync("ROLE_HANDOFF_CONTRACT", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<ROLE_HANDOFF_CONTRACT> { nextContract });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("first")).Returns(first.Object);
        editor.Setup(x => x.GetDataSource("second")).Returns(second.Object);
        var resolutions = 0;
        var service = Create(editor.Object, () => Task.FromResult(++resolutions == 1 ? "first" : "second"));

        var result = await service.ValidateHandoffAsync("instance", "from", "to", "WELL", "well", new());
        Assert.False(result.CanProceed);
        Assert.Contains(result.FailedChecks, x => x.Contains("DEPTH"));
        Assert.Equal(1, resolutions);
        Assert.Same(nextContract, await service.GetHandoffContractAsync("process", "from"));
        Assert.Equal(2, resolutions);
        first.Verify(x => x.GetEntityAsync("PROCESS_INSTANCE", It.IsAny<List<AppFilter>>()), Times.Once);
        first.Verify(x => x.GetEntityAsync("ROLE_HANDOFF_CONTRACT", It.IsAny<List<AppFilter>>()), Times.Once);
        second.Verify(x => x.GetEntityAsync("ROLE_HANDOFF_CONTRACT", It.IsAny<List<AppFilter>>()), Times.Once);
        editor.Verify(x => x.GetDataSource("first"), Times.Exactly(2));
        editor.Verify(x => x.GetDataSource("second"), Times.Once);
        editor.VerifyNoOtherCalls();
    }

    private static HandoffValidationService Create(IDMEEditor editor, Func<Task<string>>? connection) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), connection);
}
