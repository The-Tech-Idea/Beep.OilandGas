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

public class DoaRoutingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task MissingBindingRejectsAllRuleReads(bool blankResolver)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        var service = Create(editor.Object, blankResolver ? () => Task.FromResult(" ") : null);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetRulesForEntityAsync("AFE"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EvaluateThresholdsAsync("AFE", new()));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetEscalationPathAsync("rule", "L1"));
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ThresholdAndEscalationReadsUseTheCurrentBinding()
    {
        var first = new Mock<IDataSource>(MockBehavior.Strict);
        first.Setup(x => x.GetEntityAsync("DELEGATION_OF_AUTHORITY", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<DELEGATION_OF_AUTHORITY> { new()
            {
                DOA_ID = "rule", FIELD_NAME = "COST", THRESHOLD_VALUE = 100,
                COMPARISON_OPERATOR = "GREATER_THAN", APPROVAL_LEVEL = "L1", REQUIRED_ROLE = "Reviewer"
            } });
        var second = new Mock<IDataSource>(MockBehavior.Strict);
        second.Setup(x => x.GetEntityAsync("DELEGATION_OF_AUTHORITY", It.IsAny<List<AppFilter>>()))
            .ReturnsAsync(new List<DELEGATION_OF_AUTHORITY> { new()
            {
                REQUIRED_ROLE = "Reviewer", ESCALATION_ROLE = "Manager", ESCALATION_HOURS = 24
            } });
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("first")).Returns(first.Object);
        editor.Setup(x => x.GetDataSource("second")).Returns(second.Object);
        var calls = 0;
        var service = Create(editor.Object, () => Task.FromResult(++calls == 1 ? "first" : "second"));
        var levels = await service.EvaluateThresholdsAsync("AFE", new() { ["COST"] = 101m });
        Assert.Equal("Reviewer", Assert.Single(levels).RequiredRole);
        Assert.Equal(1, calls);
        var escalation = await service.GetEscalationPathAsync("rule", "L1");
        Assert.Equal("Manager", escalation!.EscalationRole);
        Assert.Equal(2, calls);
        first.Verify(x => x.GetEntityAsync("DELEGATION_OF_AUTHORITY", It.IsAny<List<AppFilter>>()), Times.Once);
        second.Verify(x => x.GetEntityAsync("DELEGATION_OF_AUTHORITY", It.IsAny<List<AppFilter>>()), Times.Once);
        editor.Verify(x => x.GetDataSource("first"), Times.Once);
        editor.Verify(x => x.GetDataSource("second"), Times.Once);
        editor.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DatabaseFailureDoesNotBecomeAnEmptyApprovalRequirement()
    {
        var source = new Mock<IDataSource>(MockBehavior.Strict);
        source.Setup(x => x.GetEntityAsync("DELEGATION_OF_AUTHORITY", It.IsAny<List<AppFilter>>()))
            .ThrowsAsync(new InvalidOperationException("unavailable"));
        var editor = new Mock<IDMEEditor>(MockBehavior.Strict);
        editor.Setup(x => x.GetDataSource("selected")).Returns(source.Object);
        var service = Create(editor.Object, () => Task.FromResult("selected"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.EvaluateThresholdsAsync("AFE", new()));
    }

    private static DoAEvaluationService Create(IDMEEditor editor, Func<Task<string>>? connection) =>
        new(editor, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<IPPDMMetadataRepository>(), connection);
}
