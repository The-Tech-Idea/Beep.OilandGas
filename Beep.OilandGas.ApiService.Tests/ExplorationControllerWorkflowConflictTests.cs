using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.LifeCycle.Services.Exploration.Processes;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.LifeCycle;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.ProductionAccounting.Services;
using Beep.OilandGas.ProspectIdentification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ExplorationControllerWorkflowConflictTests
{
    [Fact]
    public async Task ProspectToDiscoveryRiskAssessment_ReturnsConflict_WithPrerequisitePayload()
    {
        const string fieldId = "FIELD-1";
        const string instanceId = "INST-P2D-1";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "alice"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepProspectCreation,
                    Status = StepStatus.PENDING
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var explorationService = new Mock<IFieldExplorationService>(MockBehavior.Loose);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            explorationService.Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryRiskAssessment(request, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(action.Result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(conflict.Value));
        var root = doc.RootElement;
        Assert.Equal("Workflow prerequisite not satisfied.", root.GetProperty("error").GetString());
        Assert.True(root.TryGetProperty("InstanceId", out var instanceIdElement));
        Assert.False(root.TryGetProperty("instanceId", out _));
        Assert.Equal(instanceId, instanceIdElement.GetString());
        Assert.Equal(instanceId, root.GetProperty("InstanceId").GetString());
        Assert.Equal(ExplorationReferenceCodes.StepRiskAssessment, root.GetProperty("attemptedStep").GetString());
        Assert.Equal(ExplorationReferenceCodes.StepProspectCreation, root.GetProperty("prerequisiteStep").GetString());
    }

    [Fact]
    public async Task DiscoveryToDevelopmentApprove_ReturnsConflict_WithPrerequisitePayload()
    {
        const string fieldId = "FIELD-2";
        const string instanceId = "INST-D2D-1";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "bob"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                    Status = StepStatus.PENDING
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var explorationService = new Mock<IFieldExplorationService>(MockBehavior.Loose);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            explorationService.Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentApprove(request, CancellationToken.None);

        var conflict = Assert.IsType<ConflictObjectResult>(action.Result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(conflict.Value));
        var root = doc.RootElement;
        Assert.Equal("Workflow prerequisite not satisfied.", root.GetProperty("error").GetString());
        Assert.True(root.TryGetProperty("InstanceId", out var instanceIdElement));
        Assert.False(root.TryGetProperty("instanceId", out _));
        Assert.Equal(instanceId, instanceIdElement.GetString());
        Assert.Equal(instanceId, root.GetProperty("InstanceId").GetString());
        Assert.Equal(ExplorationReferenceCodes.StepDevelopmentApproval, root.GetProperty("attemptedStep").GetString());
        Assert.Equal(ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis, root.GetProperty("prerequisiteStep").GetString());
    }

    [Fact]
    public async Task ProspectToDiscoveryRiskAssessment_ReturnsBadRequest_WhenInstanceNotInCurrentField_BeforePrerequisiteEvaluation()
    {
        const string currentFieldId = "FIELD-CURRENT";
        const string instanceId = "INST-P2D-SCOPE";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "scope-user"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = "FIELD-OTHER",
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepProspectCreation,
                    Status = StepStatus.PENDING
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(currentFieldId);

        var explorationService = new Mock<IFieldExplorationService>(MockBehavior.Loose);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            explorationService.Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryRiskAssessment(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action.Result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(badRequest.Value));
        var root = doc.RootElement;
        Assert.Equal("Process instance not found or is not tied to the current field.", root.GetProperty("error").GetString());
    }

    [Fact]
    public async Task DiscoveryToDevelopmentApprove_ReturnsBadRequest_WhenInstanceNotInCurrentField_BeforePrerequisiteEvaluation()
    {
        const string currentFieldId = "FIELD-CURRENT-D2D";
        const string instanceId = "INST-D2D-SCOPE";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "scope-user-2"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = "FIELD-OTHER-D2D",
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                    Status = StepStatus.PENDING
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(currentFieldId);

        var explorationService = new Mock<IFieldExplorationService>(MockBehavior.Loose);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            explorationService.Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentApprove(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action.Result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(badRequest.Value));
        var root = doc.RootElement;
        Assert.Equal("Process instance not found or is not tied to the current field.", root.GetProperty("error").GetString());
    }

    [Fact]
    public async Task ProspectToDiscoveryRiskAssessment_ReturnsOkTrue_WhenInFieldAndPrerequisiteAdvanced()
    {
        const string fieldId = "FIELD-OK-P2D";
        const string instanceId = "INST-P2D-OK";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "ok-user-1",
            StepData = new Dictionary<string, object> { ["risk"] = "set" }
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepProspectCreation,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepRiskAssessment,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var explorationService = new Mock<IFieldExplorationService>(MockBehavior.Loose);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            explorationService.Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryRiskAssessment(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        processService.VerifyAll();
    }

    [Fact]
    public async Task DiscoveryToDevelopmentApprove_ReturnsOkTrue_WhenInFieldAndPrerequisiteAdvanced()
    {
        const string fieldId = "FIELD-OK-D2D";
        const string instanceId = "INST-D2D-OK";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "ok-user-2"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepDevelopmentApproval,
                ExplorationReferenceCodes.OutcomeApproved,
                request.UserId))
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var explorationService = new Mock<IFieldExplorationService>(MockBehavior.Loose);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            explorationService.Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentApprove(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryRiskAssessment_MapsStepData_ToProcessStepData()
    {
        const string fieldId = "FIELD-DATA-P2D";
        const string instanceId = "INST-DATA-P2D";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "payload-user-1",
            StepData = new Dictionary<string, object>
            {
                ["StepType"] = "ANALYSIS",
                ["Status"] = "READY",
                ["CustomKey"] = "custom-value"
            }
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepProspectCreation,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepRiskAssessment,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryRiskAssessment(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("ANALYSIS", captured!.StepType);
        Assert.Equal("READY", captured.Status);
        using var json = JsonDocument.Parse(captured.DataJson);
        Assert.Equal("custom-value", json.RootElement.GetProperty("CustomKey").GetString());
        processService.VerifyAll();
    }

    [Fact]
    public async Task DiscoveryToDevelopmentAppraisal_MapsStepData_ToProcessStepData()
    {
        const string fieldId = "FIELD-DATA-D2D";
        const string instanceId = "INST-DATA-D2D";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "payload-user-2",
            StepData = new Dictionary<string, object>
            {
                ["StepType"] = "APPRAISAL",
                ["Status"] = "QUEUED",
                ["Phase"] = "APPRAISAL-1"
            }
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDiscoveryRecording,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepAppraisal,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentAppraisal(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("APPRAISAL", captured!.StepType);
        Assert.Equal("QUEUED", captured.Status);
        using var json = JsonDocument.Parse(captured.DataJson);
        Assert.Equal("APPRAISAL-1", json.RootElement.GetProperty("Phase").GetString());
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryRiskAssessment_UsesEmptyProcessStepData_WhenStepDataIsNull()
    {
        const string fieldId = "FIELD-NULL-PAYLOAD";
        const string instanceId = "INST-NULL-PAYLOAD";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "null-payload-user",
            StepData = null
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepProspectCreation,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepRiskAssessment,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryRiskAssessment(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("{}", captured!.DataJson);
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryRiskAssessment_UsesEmptyProcessStepData_WhenStepDataIsEmptyDictionary()
    {
        const string fieldId = "FIELD-EMPTY-PAYLOAD";
        const string instanceId = "INST-EMPTY-PAYLOAD";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "empty-payload-user",
            StepData = new Dictionary<string, object>()
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepProspectCreation,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepRiskAssessment,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryRiskAssessment(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("{}", captured!.DataJson);
        processService.VerifyAll();
    }

    [Fact]
    public async Task DiscoveryToDevelopmentAppraisal_UsesEmptyProcessStepData_WhenStepDataIsNull()
    {
        const string fieldId = "FIELD-D2D-NULL-PAYLOAD";
        const string instanceId = "INST-D2D-NULL-PAYLOAD";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "d2d-null-payload-user",
            StepData = null
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDiscoveryRecording,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepAppraisal,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentAppraisal(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("{}", captured!.DataJson);
        processService.VerifyAll();
    }

    [Fact]
    public async Task DiscoveryToDevelopmentAppraisal_UsesEmptyProcessStepData_WhenStepDataIsEmptyDictionary()
    {
        const string fieldId = "FIELD-D2D-EMPTY-PAYLOAD";
        const string instanceId = "INST-D2D-EMPTY-PAYLOAD";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "d2d-empty-payload-user",
            StepData = new Dictionary<string, object>()
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDiscoveryRecording,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepAppraisal,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentAppraisal(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("{}", captured!.DataJson);
        processService.VerifyAll();
    }

    [Fact]
    public async Task DiscoveryToDevelopmentApprove_UsesEmptyProcessStepData_WhenStepDataIsNull()
    {
        const string fieldId = "FIELD-D2D-APPROVE-NULL";
        const string instanceId = "INST-D2D-APPROVE-NULL";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "d2d-approve-null-user",
            StepData = null
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepDevelopmentApproval,
                ExplorationReferenceCodes.OutcomeApproved,
                request.UserId))
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentApprove(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        processService.VerifyAll();
    }

    [Fact]
    public async Task DiscoveryToDevelopmentApprove_UsesEmptyProcessStepData_WhenStepDataIsEmptyDictionary()
    {
        const string fieldId = "FIELD-D2D-APPROVE-EMPTY";
        const string instanceId = "INST-D2D-APPROVE-EMPTY";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "d2d-approve-empty-user",
            StepData = new Dictionary<string, object>()
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdDiscoveryToDevelopment,
            EntityType = ExplorationReferenceCodes.EntityTypeDiscovery,
            StepInstances =
            [
                new ProcessStepInstance
                {
                    StepId = ExplorationReferenceCodes.StepDevelopmentEconomicAnalysis,
                    Status = StepStatus.COMPLETED
                }
            ]
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepDevelopmentApproval,
                ExplorationReferenceCodes.OutcomeApproved,
                request.UserId))
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.DiscoveryToDevelopmentApprove(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryProspectReadiness_UsesEmptyProcessStepData_WhenStepDataIsNull()
    {
        const string fieldId = "FIELD-P2D-READY-NULL";
        const string instanceId = "INST-P2D-READY-NULL";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "p2d-ready-null-user",
            StepData = null
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                ExplorationReferenceCodes.OutcomeSuccess,
                request.UserId))
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryProspectReadiness(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("{}", captured!.DataJson);
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryProspectReadiness_UsesEmptyProcessStepData_WhenStepDataIsEmptyDictionary()
    {
        const string fieldId = "FIELD-P2D-READY-EMPTY";
        const string instanceId = "INST-P2D-READY-EMPTY";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "p2d-ready-empty-user",
            StepData = new Dictionary<string, object>()
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect
        };

        PROCESS_STEP_DATA? captured = null;
        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .Callback<string, string, PROCESS_STEP_DATA, string>((_, _, data, _) => captured = data)
            .ReturnsAsync(true);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                ExplorationReferenceCodes.OutcomeSuccess,
                request.UserId))
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryProspectReadiness(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        Assert.NotNull(captured);
        Assert.Equal("{}", captured!.DataJson);
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryProspectReadiness_ReturnsBadRequest_WhenInstanceNotInCurrentField()
    {
        const string currentFieldId = "FIELD-CURRENT-P2D-READY";
        const string instanceId = "INST-P2D-READY-SCOPE";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "scope-ready-user"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = "FIELD-OTHER-P2D-READY",
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(currentFieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryProspectReadiness(request, CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action.Result);
        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(badRequest.Value));
        var root = doc.RootElement;
        Assert.Equal("Process instance not found or is not tied to the current field.", root.GetProperty("error").GetString());
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryProspectReadiness_ReturnsOkTrue_WhenInstanceInCurrentField()
    {
        const string fieldId = "FIELD-OK-P2D-READY";
        const string instanceId = "INST-P2D-READY-OK";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "ok-ready-user"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .ReturnsAsync(true);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                ExplorationReferenceCodes.OutcomeSuccess,
                request.UserId))
            .ReturnsAsync(true);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryProspectReadiness(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(true, ok.Value);
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryProspectReadiness_ReturnsOkFalse_WhenExecuteStepReturnsFalse()
    {
        const string fieldId = "FIELD-READY-EXEC-FAIL";
        const string instanceId = "INST-READY-EXEC-FAIL";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "exec-fail-user"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .ReturnsAsync(false);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryProspectReadiness(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(false, ok.Value);
        processService.VerifyAll();
    }

    [Fact]
    public async Task ProspectToDiscoveryProspectReadiness_ReturnsOkFalse_WhenCompleteStepReturnsFalse()
    {
        const string fieldId = "FIELD-READY-COMPLETE-FAIL";
        const string instanceId = "INST-READY-COMPLETE-FAIL";
        var request = new ExplorationWorkflowStepRequest
        {
            InstanceId = instanceId,
            UserId = "complete-fail-user"
        };

        var processInstance = new ProcessInstance
        {
            InstanceId = instanceId,
            FieldId = fieldId,
            ProcessId = ExplorationReferenceCodes.ProcessIdProspectToDiscovery,
            EntityType = ExplorationReferenceCodes.EntityTypeProspect
        };

        var processService = new Mock<IProcessService>(MockBehavior.Strict);
        processService.Setup(p => p.GetProcessInstanceAsync(instanceId)).ReturnsAsync(processInstance);
        processService
            .Setup(p => p.ExecuteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                It.IsAny<PROCESS_STEP_DATA>(),
                request.UserId))
            .ReturnsAsync(true);
        processService
            .Setup(p => p.CompleteStepAsync(
                instanceId,
                ExplorationReferenceCodes.StepProspectCreation,
                ExplorationReferenceCodes.OutcomeSuccess,
                request.UserId))
            .ReturnsAsync(false);

        var explorationProcessService = new ExplorationProcessService(
            processService.Object,
            NullLogger<ExplorationProcessService>.Instance);

        var fieldOrchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        fieldOrchestrator.SetupGet(o => o.CurrentFieldId).Returns(fieldId);

        var controller = new ExplorationController(
            fieldOrchestrator.Object,
            explorationProcessService,
            new Mock<IFieldExplorationService>(MockBehavior.Loose).Object,
            CreateDummyProductionAccountingService(),
            NullLogger<ExplorationController>.Instance);

        var action = await controller.ProspectToDiscoveryProspectReadiness(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        Assert.Equal(false, ok.Value);
        processService.VerifyAll();
    }

    private static ProductionAccountingService CreateDummyProductionAccountingService() =>
        (ProductionAccountingService)RuntimeHelpers.GetUninitializedObject(typeof(ProductionAccountingService));
}
