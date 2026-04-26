using Beep.OilandGas.LifeCycle.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Exploration;
using Beep.OilandGas.LifeCycle.Services.Exploration.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services;
using Beep.OilandGas.PPDM39.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.Editor;
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
            ProcessId = "LEAD_TO_PROSPECT",
            EntityId = leadId,
            EntityType = "LEAD",
            FieldId = fieldId,
            CurrentState = "NOT_STARTED",
            StartedBy = userId
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetProcessDefinitionsByTypeAsync("EXPLORATION"))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition { ProcessId = "LEAD_TO_PROSPECT", ProcessName = "LeadToProspect", ProcessType = "EXPLORATION" }
            });

        processService
            .Setup(service => service.StartProcessAsync("LEAD_TO_PROSPECT", leadId, "LEAD", fieldId, userId))
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
            ProcessId = "PROSPECT_TO_DISCOVERY",
            EntityId = prospectId,
            EntityType = "PROSPECT",
            FieldId = fieldId,
            CurrentState = "NOT_STARTED",
            StartedBy = userId
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetProcessDefinitionsByTypeAsync("EXPLORATION"))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition { ProcessId = "PROSPECT_TO_DISCOVERY", ProcessName = "ProspectToDiscovery", ProcessType = "EXPLORATION" }
            });

        processService
            .Setup(service => service.StartProcessAsync("PROSPECT_TO_DISCOVERY", prospectId, "PROSPECT", fieldId, userId))
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
            .Setup(service => service.GetProcessDefinitionsByTypeAsync("EXPLORATION"))
            .ReturnsAsync(new List<ProcessDefinition>
            {
                new ProcessDefinition { ProcessId = "LEAD_TO_PROSPECT", ProcessName = "LeadToProspect", ProcessType = "EXPLORATION" }
            });

        var sut = CreateService(processService.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.StartProspectToDiscoveryProcessAsync("PROSPECT-404", "FIELD-001", "user-3"));

        processService.VerifyAll();
    }

    private static ExplorationProcessService CreateService(IProcessService processService)
    {
        var editor = new Mock<IDMEEditor>(MockBehavior.Loose).Object;
        var commonColumnHandler = new Mock<ICommonColumnHandler>(MockBehavior.Loose).Object;
        var defaults = new Mock<IPPDM39DefaultsRepository>(MockBehavior.Loose).Object;
        var metadata = new Mock<IPPDMMetadataRepository>(MockBehavior.Loose).Object;
        var mappingService = new PPDMMappingService();

        var explorationService = new PPDMExplorationService(
            editor,
            commonColumnHandler,
            defaults,
            metadata,
            mappingService,
            "PPDM39",
            NullLogger<PPDMExplorationService>.Instance);

        return new ExplorationProcessService(
            processService,
            explorationService,
            NullLogger<ExplorationProcessService>.Instance);
    }
}
