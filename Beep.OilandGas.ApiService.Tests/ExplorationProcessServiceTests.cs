using System.Threading;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Exploration.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.ProspectIdentification;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ExplorationProcessServiceTests
{
    [Fact]
    public async Task StartLeadToProspectProcessAsync_UsesLeadWorkflowAndEntityType()
    {
        const string leadId = "LEAD-001";
        const string fieldId = "FIELD-001";
        const string userId = "user-1";

        var expectedInstance = new ProcessInstance
        {
            InstanceId = "PROC-LEAD-1",
            ProcessId = ExplorationReferenceCodes.ProcessIdLeadToProspect,
            EntityId = leadId,
            EntityType = ExplorationReferenceCodes.EntityTypeLead,
            FieldId = fieldId,
            CurrentState = "NOT_STARTED",
            StartedBy = userId
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition
                {
                    ProcessId = ExplorationReferenceCodes.ProcessIdLeadToProspect,
                    ProcessName = ExplorationReferenceCodes.ProcessNameLeadToProspect,
                    ProcessType = ExplorationReferenceCodes.ProcessTypeExploration
                }
            });

        processService
            .Setup(service => service.StartProcessAsync(
                ExplorationReferenceCodes.ProcessIdLeadToProspect,
                leadId,
                ExplorationReferenceCodes.EntityTypeLead,
                fieldId,
                userId))
            .ReturnsAsync(expectedInstance);

        var sut = CreateService(processService.Object);

        var instance = await sut.StartLeadToProspectProcessAsync(leadId, fieldId, userId);

        Assert.Same(expectedInstance, instance);
        processService.VerifyAll();
    }

    [Fact]
    public async Task StartProspectToDiscoveryProcessAsync_UsesProspectWorkflowAndEntityType()
    {
        const string prospectId = "PROSPECT-001";
        const string fieldId = "FIELD-001";
        const string userId = "user-2";

        var expectedInstance = new ProcessInstance
        {
            InstanceId = "PROC-PROS-1",
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityId = prospectId,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect,
            FieldId = fieldId,
            CurrentState = "NOT_STARTED",
            StartedBy = userId
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition
                {
                    ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
                    ProcessName = ExplorationReferenceCodes.ProcessNameProspectToDiscovery,
                    ProcessType = ExplorationReferenceCodes.ProcessTypeExploration
                }
            });

        processService
            .Setup(service => service.StartProcessAsync(
                ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
                prospectId,
                ExplorationReferenceCodes.EntityTypeProspect,
                fieldId,
                userId))
            .ReturnsAsync(expectedInstance);

        var sut = CreateService(processService.Object);

        var instance = await sut.StartProspectToDiscoveryProcessAsync(prospectId, fieldId, userId);

        Assert.Same(expectedInstance, instance);
        processService.VerifyAll();
    }

    [Fact]
    public async Task StartProspectToDiscoveryProcessAsync_ThrowsWhenWorkflowMissing()
    {
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition
                {
                    ProcessId = ExplorationReferenceCodes.ProcessIdLeadToProspect,
                    ProcessName = ExplorationReferenceCodes.ProcessNameLeadToProspect,
                    ProcessType = ExplorationReferenceCodes.ProcessTypeExploration
                }
            });

        var sut = CreateService(processService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.StartProspectToDiscoveryProcessAsync("PROSPECT-404", "FIELD-001", "user-3"));

        processService.VerifyAll();
    }

    [Fact]
    public async Task PerformRiskAssessmentAsync_Throws_WhenAlreadyCanceled()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        var sut = CreateService(processService.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            sut.PerformRiskAssessmentAsync("i1", new PROCESS_STEP_DATA(), "u", cts.Token));

        processService.Verify(
            p => p.ExecuteStepAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PROCESS_STEP_DATA>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task RejectLeadAsync_ReturnsFalse_WhenInstanceMissing()
    {
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync("missing")).ReturnsAsync((ProcessInstance?)null);

        var sut = CreateService(processService.Object);

        var ok = await sut.RejectLeadAsync("missing", "No budget", "u1");

        Assert.False(ok);
        processService.Verify(
            p => p.CompleteStepAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        processService.VerifyAll();
    }

    [Fact]
    public async Task RejectLeadAsync_CompletesLeadApprovalAsRejected()
    {
        const string instanceId = "i-rej-1";
        var instance = new ProcessInstance { InstanceId = instanceId };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(instance);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepLeadApproval,
                ExplorationReferenceCodes.OutcomeRejected,
                "alice"))
            .ReturnsAsync(true);

        var sut = CreateService(processService.Object);

        var ok = await sut.RejectLeadAsync(instanceId, "Screen failed", "alice", CancellationToken.None);

        Assert.True(ok);
        processService.VerifyAll();
    }

    [Fact]
    public async Task PromoteLeadToProspectAsync_ReturnsFalse_WhenInstanceMissing()
    {
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync("missing")).ReturnsAsync((ProcessInstance?)null);

        var sut = CreateService(processService.Object);

        var ok = await sut.PromoteLeadToProspectAsync("missing", new PROCESS_STEP_DATA(), "user-1");

        Assert.False(ok);
        processService.Verify(
            p => p.ExecuteStepAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<PROCESS_STEP_DATA>(), It.IsAny<string>()),
            Times.Never);
        processService.VerifyAll();
    }

    [Fact]
    public async Task PromoteLeadToProspectAsync_DoesNotCompleteOrInvokeLeadHook_WhenExecuteStepFails()
    {
        const string instanceId = "inst-1";
        var instance = new ProcessInstance { InstanceId = instanceId };
        var stepData = new PROCESS_STEP_DATA();

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(instance);
        processService
            .Setup(p => p.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepProspectCreation, stepData, "u"))
            .ReturnsAsync(false);

        var lead = new Mock<ILeadExplorationService>(MockBehavior.Strict);
        var sut = CreateService(processService.Object, lead.Object);

        var ok = await sut.PromoteLeadToProspectAsync(instanceId, stepData, "u");

        Assert.False(ok);
        processService.Verify(
            p => p.CompleteStepAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        lead.Verify(
            l => l.AfterProspectCreationStepCompletedAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        processService.VerifyAll();
    }

    [Fact]
    public async Task PromoteLeadToProspectAsync_CompletesStepAndInvokesLeadHook_WhenExecuteSucceeds()
    {
        const string instanceId = "inst-2";
        var instance = new ProcessInstance { InstanceId = instanceId };
        var stepData = new PROCESS_STEP_DATA();

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(instance);
        processService
            .Setup(p => p.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepProspectCreation, stepData, "u"))
            .ReturnsAsync(true);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                ExplorationReferenceCodes.OutcomeSuccess,
                "u"))
            .ReturnsAsync(true);

        using var cts = new CancellationTokenSource();
        var lead = new Mock<ILeadExplorationService>(MockBehavior.Strict);
        lead
            .Setup(l => l.AfterProspectCreationStepCompletedAsync(instanceId, "u", cts.Token))
            .Returns(Task.CompletedTask);

        var sut = CreateService(processService.Object, lead.Object);

        var ok = await sut.PromoteLeadToProspectAsync(instanceId, stepData, "u", cts.Token);

        Assert.True(ok);
        processService.VerifyAll();
        lead.VerifyAll();
    }

    [Fact]
    public async Task PromoteLeadToProspectAsync_CompletesStep_WhenLeadHookNotRegistered()
    {
        const string instanceId = "inst-3";
        var instance = new ProcessInstance { InstanceId = instanceId };
        var stepData = new PROCESS_STEP_DATA();

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(instance);
        processService
            .Setup(p => p.ExecuteStepAsync(instanceId, ExplorationReferenceCodes.StepProspectCreation, stepData, "u"))
            .ReturnsAsync(true);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                ExplorationReferenceCodes.OutcomeSuccess,
                "u"))
            .ReturnsAsync(true);

        var sut = CreateService(processService.Object);

        var ok = await sut.PromoteLeadToProspectAsync(instanceId, stepData, "u");

        Assert.True(ok);
        processService.VerifyAll();
    }

    [Fact]
    public async Task StartDiscoveryToDevelopmentProcessAsync_UsesDiscoveryWorkflowAndEntityType()
    {
        const string discoveryId = "DISC-001";
        const string fieldId = "FIELD-010";
        const string userId = "user-9";

        var expectedInstance = new ProcessInstance
        {
            InstanceId = "PROC-DISC-1",
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityId = discoveryId,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            FieldId = fieldId,
            CurrentState = "NOT_STARTED",
            StartedBy = userId
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition
                {
                    ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
                    ProcessName = ExplorationReferenceCodes.ProcessNameDiscoveryToDevelopment,
                    ProcessType = ExplorationReferenceCodes.ProcessTypeExploration
                }
            });

        processService
            .Setup(service => service.StartProcessAsync(
                ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
                discoveryId,
                ExplorationReferenceCodes.EntityTypeDiscovery,
                fieldId,
                userId))
            .ReturnsAsync(expectedInstance);

        var sut = CreateService(processService.Object);

        var instance = await sut.StartDiscoveryToDevelopmentProcessAsync(discoveryId, fieldId, userId);

        Assert.Same(expectedInstance, instance);
        processService.VerifyAll();
    }

    [Fact]
    public async Task StartDiscoveryToDevelopmentProcessAsync_ThrowsWhenWorkflowMissing()
    {
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetProcessDefinitionsByTypeAsync(ExplorationReferenceCodes.ProcessTypeExploration))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition
                {
                    ProcessId = ExplorationReferenceCodes.ProcessIdLeadToProspect,
                    ProcessName = ExplorationReferenceCodes.ProcessNameLeadToProspect,
                    ProcessType = ExplorationReferenceCodes.ProcessTypeExploration
                }
            });

        var sut = CreateService(processService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.StartDiscoveryToDevelopmentProcessAsync("DISC-404", "FIELD-001", "user-10"));

        processService.VerifyAll();
    }

    [Fact]
    public async Task IsProcessInstanceInFieldAsync_ReturnsFalse_WhenInstanceMissing()
    {
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync("missing")).ReturnsAsync((ProcessInstance?)null);

        var sut = CreateService(processService.Object);

        Assert.False(await sut.IsProcessInstanceInFieldAsync("missing", "FIELD-1"));

        processService.VerifyAll();
    }

    [Fact]
    public async Task IsProcessInstanceInFieldAsync_ReturnsFalse_WhenFieldMismatch()
    {
        const string instanceId = "i-1";
        var instance = new ProcessInstance { InstanceId = instanceId, FieldId = "FIELD-A" };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(instance);

        var sut = CreateService(processService.Object);

        Assert.False(await sut.IsProcessInstanceInFieldAsync(instanceId, "FIELD-B"));

        processService.VerifyAll();
    }

    [Fact]
    public async Task IsProcessInstanceInFieldAsync_ReturnsTrue_WhenFieldMatchesCaseInsensitive()
    {
        const string instanceId = "i-2";
        var instance = new ProcessInstance { InstanceId = instanceId, FieldId = "field-x" };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(instance);

        var sut = CreateService(processService.Object);

        Assert.True(await sut.IsProcessInstanceInFieldAsync(instanceId, "FIELD-X"));

        processService.VerifyAll();
    }

    [Fact]
    public async Task IsProcessInstanceInFieldAsync_ReturnsFalse_WhenInstanceFieldIdEmpty()
    {
        const string instanceId = "i-3";
        var instance = new ProcessInstance { InstanceId = instanceId, FieldId = "" };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(instance);

        var sut = CreateService(processService.Object);

        Assert.False(await sut.IsProcessInstanceInFieldAsync(instanceId, "FIELD-1"));

        processService.VerifyAll();
    }

    private static ExplorationProcessService CreateService(
        IProcessService processService,
        ILeadExplorationService? leadExplorationService = null) =>
        new ExplorationProcessService(
            processService,
            NullLogger<ExplorationProcessService>.Instance,
            leadExplorationService);
}
