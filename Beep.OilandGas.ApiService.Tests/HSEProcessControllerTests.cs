using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.HSE;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class HSEProcessControllerTests
{
    [Fact]
    public async Task EnsureWorkflowAsync_WhenWorkflowMissing_StartsProcessSyncsStateAndAddsHistory()
    {
        const string fieldId = "FIELD-1";
        const string incidentId = "INC-100";
        const string userId = "user-1";
        const string instanceId = "PROC-100";

        var incident = new HSEIncidentRecord
        {
            IncidentId = incidentId,
            FieldId = fieldId,
            CurrentState = IncidentState.Reported,
            IncidentType = IncidentType.Spill,
            Description = "Release during transfer"
        };

        var startedInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            ProcessId = "HSE_INCIDENT_REPORTING",
            EntityId = incidentId,
            EntityType = "HSE_INCIDENT",
            FieldId = fieldId,
            CurrentState = "NOT_STARTED",
            StartedBy = userId,
            StartDate = new DateTime(2026, 4, 23, 8, 0, 0, DateTimeKind.Utc)
        };

        ProcessHistoryEntry? addedHistory = null;

        var hseService = new Mock<IFieldHSEService>(MockBehavior.Strict);
        hseService
            .Setup(service => service.GetIncidentAsync(incidentId))
            .ReturnsAsync(incident);

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetCurrentProcessForEntityAsync(incidentId, "HSE_INCIDENT"))
            .ReturnsAsync((ProcessInstance?)null);
        processService
            .Setup(service => service.GetProcessInstancesForEntityAsync(incidentId, "HSE_INCIDENT"))
            .ReturnsAsync([]);
        processService
            .Setup(service => service.StartProcessAsync("HSE_INCIDENT_REPORTING", incidentId, "HSE_INCIDENT", fieldId, userId))
            .ReturnsAsync(startedInstance);
        processService
            .Setup(service => service.GetProcessInstanceAsync(instanceId))
            .ReturnsAsync(startedInstance);
        processService
            .Setup(service => service.TransitionStateAsync(instanceId, IncidentState.Reported, userId))
            .ReturnsAsync(true);
        processService
            .Setup(service => service.AddHistoryEntryAsync(instanceId, It.IsAny<ProcessHistoryEntry>()))
            .Callback<string, ProcessHistoryEntry>((_, entry) => addedHistory = entry)
            .ReturnsAsync((string _, ProcessHistoryEntry entry) => entry);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(orchestrator => orchestrator.CurrentFieldId).Returns(fieldId);
        fieldOrchestrator.Setup(orchestrator => orchestrator.GetHSEService()).Returns(hseService.Object);

        var controller = CreateController(fieldOrchestrator.Object, processService.Object, userId);

        var result = await controller.EnsureWorkflowAsync(incidentId);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(HSEProcessController.GetIncidentAsync), created.ActionName);

        var createdInstance = Assert.IsType<ProcessInstance>(created.Value);
        Assert.Same(startedInstance, createdInstance);

        Assert.NotNull(addedHistory);
        Assert.Equal(instanceId, addedHistory!.InstanceId);
        Assert.Equal("WORKFLOW_ENROLLED", addedHistory.Action);
        Assert.Equal(userId, addedHistory.PerformedBy);
        Assert.Equal("Workflow created for an existing PPDM incident.", addedHistory.Notes);

        processService.VerifyAll();
        hseService.VerifyAll();
        fieldOrchestrator.VerifyAll();
    }

    [Fact]
    public async Task EnsureWorkflowAsync_WhenWorkflowAlreadyExists_ReturnsExistingInstance()
    {
        const string fieldId = "FIELD-1";
        const string incidentId = "INC-200";
        const string userId = "user-2";

        var incident = new HSEIncidentRecord
        {
            IncidentId = incidentId,
            FieldId = fieldId,
            CurrentState = IncidentState.UnderInvestigation,
            IncidentType = IncidentType.Injury
        };

        var existingInstance = new ProcessInstance
        {
            InstanceId = "PROC-200",
            ProcessId = "HSE_INCIDENT_REPORTING",
            EntityId = incidentId,
            EntityType = "HSE_INCIDENT",
            FieldId = fieldId,
            CurrentState = IncidentState.UnderInvestigation,
            StartedBy = userId
        };

        var hseService = new Mock<IFieldHSEService>(MockBehavior.Strict);
        hseService
            .Setup(service => service.GetIncidentAsync(incidentId))
            .ReturnsAsync(incident);

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService
            .Setup(service => service.GetCurrentProcessForEntityAsync(incidentId, "HSE_INCIDENT"))
            .ReturnsAsync(existingInstance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(orchestrator => orchestrator.CurrentFieldId).Returns(fieldId);
        fieldOrchestrator.Setup(orchestrator => orchestrator.GetHSEService()).Returns(hseService.Object);

        var controller = CreateController(fieldOrchestrator.Object, processService.Object, userId);

        var result = await controller.EnsureWorkflowAsync(incidentId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var returnedInstance = Assert.IsType<ProcessInstance>(ok.Value);
        Assert.Same(existingInstance, returnedInstance);

        processService.VerifyAll();
        hseService.VerifyAll();
        fieldOrchestrator.VerifyAll();
    }

    [Fact]
    public async Task EnsureWorkflowAsync_WhenIncidentDoesNotExist_ReturnsNotFound()
    {
        const string fieldId = "FIELD-1";
        const string incidentId = "INC-404";
        const string userId = "user-3";

        var hseService = new Mock<IFieldHSEService>(MockBehavior.Strict);
        hseService
            .Setup(service => service.GetIncidentAsync(incidentId))
            .ReturnsAsync((HSEIncidentRecord?)null);

        var processService = new Mock<IProcessService>(MockBehavior.Strict);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(orchestrator => orchestrator.CurrentFieldId).Returns(fieldId);
        fieldOrchestrator.Setup(orchestrator => orchestrator.GetHSEService()).Returns(hseService.Object);

        var controller = CreateController(fieldOrchestrator.Object, processService.Object, userId);

        var result = await controller.EnsureWorkflowAsync(incidentId);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFound.Value);

        processService.VerifyNoOtherCalls();
        hseService.VerifyAll();
        fieldOrchestrator.VerifyAll();
    }

    [Fact]
    public async Task EnsureWorkflowAsync_WhenNoActiveFieldSelected_ReturnsBadRequest()
    {
        const string incidentId = "INC-300";
        const string userId = "user-4";

        var hseService = new Mock<IFieldHSEService>(MockBehavior.Strict);
        var processService = new Mock<IProcessService>(MockBehavior.Strict);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(orchestrator => orchestrator.CurrentFieldId).Returns((string?)null);

        var controller = CreateController(fieldOrchestrator.Object, processService.Object, userId);

        var result = await controller.EnsureWorkflowAsync(incidentId);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);

        processService.VerifyNoOtherCalls();
        hseService.VerifyNoOtherCalls();
        fieldOrchestrator.VerifyGet(orchestrator => orchestrator.CurrentFieldId, Times.AtLeastOnce);
        fieldOrchestrator.VerifyNoOtherCalls();
    }

    private static HSEProcessController CreateController(
        IFieldOrchestrator fieldOrchestrator,
        IProcessService processService,
        string userId)
    {
        var controller = new HSEProcessController(
            fieldOrchestrator,
            processService,
            NullLogger<HSEProcessController>.Instance);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userId)
                ], "TestAuth"))
            }
        };

        return controller;
    }
}