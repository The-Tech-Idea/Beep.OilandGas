using Beep.OilandGas.LifeCycle.Services.Exploration;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.ProspectIdentification;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class LeadExplorationServiceTests
{
    [Fact]
    public void Constructor_Throws_WhenWorkflowOptionsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() =>
            new LeadExplorationService(
                Mock.Of<IProcessService>(),
                Mock.Of<IFieldExplorationService>(),
                null!,
                NullLogger<LeadExplorationService>.Instance));

        Assert.Equal("workflowOptions", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AfterProspectCreationStepCompletedAsync_Throws_WhenInstanceIdMissing(string? instanceId)
    {
        var sut = CreateSut(Mock.Of<IProcessService>(), Mock.Of<IFieldExplorationService>());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.AfterProspectCreationStepCompletedAsync(instanceId!, "user-1"));

        Assert.Equal("processInstanceId", ex.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task AfterProspectCreationStepCompletedAsync_Throws_WhenUserIdMissing(string? userId)
    {
        var sut = CreateSut(Mock.Of<IProcessService>(), Mock.Of<IFieldExplorationService>());

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            sut.AfterProspectCreationStepCompletedAsync("inst-1", userId!));

        Assert.Equal("userId", ex.ParamName);
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_NoExploration_WhenInstanceMissing()
    {
        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("missing"))
            .ReturnsAsync((ProcessInstance?)null);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);

        var sut = CreateSut(process.Object, exploration.Object);

        await sut.AfterProspectCreationStepCompletedAsync("missing", "user-1");

        exploration.Verify(
            e => e.GetProspectForFieldByLeadIdAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        process.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_NoExploration_WhenEntityTypeNotLead()
    {
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = "PROSPECT",
            EntityId = "P-1",
            FieldId = "F-1"
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);

        var sut = CreateSut(process.Object, exploration.Object);

        await sut.AfterProspectCreationStepCompletedAsync("i1", "user-1");

        exploration.Verify(
            e => e.GetProspectForFieldByLeadIdAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        process.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_NoExploration_WhenFieldIdMissing()
    {
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = "L-1",
            FieldId = ""
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);

        var sut = CreateSut(process.Object, exploration.Object);

        await sut.AfterProspectCreationStepCompletedAsync("i1", "user-1");

        exploration.Verify(
            e => e.GetProspectForFieldByLeadIdAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        process.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_NoExploration_WhenLeadIdMissing()
    {
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = "",
            FieldId = "F-1"
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);

        var sut = CreateSut(process.Object, exploration.Object);

        await sut.AfterProspectCreationStepCompletedAsync("i1", "user-1");

        exploration.Verify(
            e => e.GetProspectForFieldByLeadIdAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        process.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_SkipsCreate_WhenProspectAlreadyExists()
    {
        const string fieldId = "F-1";
        const string leadId = "L-1";
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = leadId,
            FieldId = fieldId
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);
        exploration.Setup(e => e.GetProspectForFieldByLeadIdAsync(fieldId, leadId))
            .ReturnsAsync(new PROSPECT());

        var sut = CreateSut(process.Object, exploration.Object);

        await sut.AfterProspectCreationStepCompletedAsync("i1", "user-1");

        exploration.Verify(e => e.CreateProspectForFieldAsync(It.IsAny<string>(), It.IsAny<ProspectRequest>(), It.IsAny<string>()), Times.Never);
        exploration.Verify(e => e.UpdateLeadStatusAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        process.VerifyAll();
        exploration.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_CreatesProspectAndPromotesLead()
    {
        const string fieldId = "F-1";
        const string leadId = "L-42";
        const string userId = "alice";
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = leadId,
            FieldId = fieldId
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        ProspectRequest? capturedRequest = null;
        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);
        exploration.Setup(e => e.GetProspectForFieldByLeadIdAsync(fieldId, leadId))
            .ReturnsAsync((PROSPECT?)null);
        exploration
            .Setup(e => e.CreateProspectForFieldAsync(fieldId, It.IsAny<ProspectRequest>(), userId))
            .Callback<string, ProspectRequest, string>((_, req, _) => capturedRequest = req)
            .ReturnsAsync(new PROSPECT());
        exploration
            .Setup(e => e.UpdateLeadStatusAsync(leadId, It.IsAny<string>(), userId))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(process.Object, exploration.Object);

        await sut.AfterProspectCreationStepCompletedAsync("i1", userId);

        Assert.NotNull(capturedRequest);
        Assert.Equal(leadId, capturedRequest!.LeadId);
        Assert.Equal("NEW", capturedRequest.Status);
        Assert.Equal($"Prospect from lead {leadId}", capturedRequest.ProspectName);

        exploration.Verify(e => e.UpdateLeadStatusAsync(leadId, ExplorationReferenceCodes.LeadStatusPromotedToProspect, userId), Times.Once);
        process.VerifyAll();
        exploration.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_AppliesProspectNameFromStepJson()
    {
        const string fieldId = "F-1";
        const string leadId = "L-9";
        var json = """{"ProspectName":"Custom Name","Description":"D1","ProspectType":"T1"}""";
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = leadId,
            FieldId = fieldId,
            StepInstances =
            {
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepProspectCreation,
                    SequenceNumber = 2,
                    StepData = new PROCESS_STEP_DATA { DataJson = json }
                }
            }
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        ProspectRequest? capturedRequest = null;
        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);
        exploration.Setup(e => e.GetProspectForFieldByLeadIdAsync(fieldId, leadId))
            .ReturnsAsync((PROSPECT?)null);
        exploration
            .Setup(e => e.CreateProspectForFieldAsync(fieldId, It.IsAny<ProspectRequest>(), "u"))
            .Callback<string, ProspectRequest, string>((_, req, _) => capturedRequest = req)
            .ReturnsAsync(new PROSPECT());
        exploration
            .Setup(e => e.UpdateLeadStatusAsync(leadId, It.IsAny<string>(), "u"))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(process.Object, exploration.Object);

        await sut.AfterProspectCreationStepCompletedAsync("i1", "u");

        Assert.NotNull(capturedRequest);
        Assert.Equal("Custom Name", capturedRequest!.ProspectName);
        Assert.Equal("D1", capturedRequest.Description);
        Assert.Equal("T1", capturedRequest.ProspectType);
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_ThrowsOperationCanceled_WhenCanceledAfterLoad()
    {
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = "L-1",
            FieldId = "F-1"
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);

        var sut = CreateSut(process.Object, exploration.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            sut.AfterProspectCreationStepCompletedAsync("i1", "user-1", cts.Token));

        exploration.Verify(
            e => e.GetProspectForFieldByLeadIdAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        process.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_UsesConfiguredPromotedLeadStatus()
    {
        const string fieldId = "F-1";
        const string leadId = "L-99";
        const string userId = "bob";
        const string customCode = "PROMOTED_CUSTOM";
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = leadId,
            FieldId = fieldId
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);
        exploration.Setup(e => e.GetProspectForFieldByLeadIdAsync(fieldId, leadId))
            .ReturnsAsync((PROSPECT?)null);
        exploration
            .Setup(e => e.CreateProspectForFieldAsync(fieldId, It.IsAny<ProspectRequest>(), userId))
            .ReturnsAsync(new PROSPECT());
        exploration
            .Setup(e => e.UpdateLeadStatusAsync(leadId, customCode, userId))
            .Returns(Task.CompletedTask);

        var options = Options.Create(new LeadExplorationWorkflowOptions { PromotedLeadStatusCode = customCode });
        var sut = CreateSut(process.Object, exploration.Object, options);

        await sut.AfterProspectCreationStepCompletedAsync("i1", userId);

        exploration.Verify(e => e.UpdateLeadStatusAsync(leadId, customCode, userId), Times.Once);
        process.VerifyAll();
        exploration.VerifyAll();
    }

    [Fact]
    public async Task AfterProspectCreationStepCompletedAsync_FallsBackToDefaultPromotedCode_WhenConfiguredWhitespace()
    {
        const string fieldId = "F-1";
        const string leadId = "L-ws";
        const string userId = "u1";
        var instance = new ProcessInstance
        {
            InstanceId = "i1",
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            EntityId = leadId,
            FieldId = fieldId
        };

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessInstanceAsync("i1")).ReturnsAsync(instance);

        var exploration = new Mock<IFieldExplorationService>(MockBehavior.Strict);
        exploration.Setup(e => e.GetProspectForFieldByLeadIdAsync(fieldId, leadId))
            .ReturnsAsync((PROSPECT?)null);
        exploration
            .Setup(e => e.CreateProspectForFieldAsync(fieldId, It.IsAny<ProspectRequest>(), userId))
            .ReturnsAsync(new PROSPECT());
        exploration
            .Setup(e => e.UpdateLeadStatusAsync(leadId, ExplorationReferenceCodes.LeadStatusPromotedToProspect, userId))
            .Returns(Task.CompletedTask);

        var options = Options.Create(new LeadExplorationWorkflowOptions { PromotedLeadStatusCode = "   " });
        var sut = CreateSut(process.Object, exploration.Object, options);

        await sut.AfterProspectCreationStepCompletedAsync("i1", userId);

        exploration.Verify(
            e => e.UpdateLeadStatusAsync(leadId, ExplorationReferenceCodes.LeadStatusPromotedToProspect, userId),
            Times.Once);
        process.VerifyAll();
        exploration.VerifyAll();
    }

    private static LeadExplorationService CreateSut(
        IProcessService process,
        IFieldExplorationService exploration,
        IOptions<LeadExplorationWorkflowOptions>? workflowOptions = null) =>
        new LeadExplorationService(
            process,
            exploration,
            workflowOptions ?? Options.Create(new LeadExplorationWorkflowOptions()),
            NullLogger<LeadExplorationService>.Instance);
}
