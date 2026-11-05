using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.BusinessProcess;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Processes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

/// <summary>
/// The field's gate reviews are every gate-review instance started in the field, whatever entity each reviews. The lists
/// had read only instances whose entity was the field itself, so a gate submitted for a well never appeared.
/// </summary>
public sealed class GateReviewListingTests
{
    private const string FieldId = "FIELD-G-1";

    [Fact]
    public async Task Gates_ListsAWellsGateReview_AndLeavesOtherProcessesOut()
    {
        var controller = CreateController(FieldInstances());

        var result = await controller.GetGatesAsync();

        var listed = Assert.IsType<List<ProcessInstanceSummary>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(new[] { "fdp-gate", "well-design-gate" }, listed.Select(g => g.InstanceId).OrderBy(id => id));
        Assert.Contains(listed, g => g.EntityId == "UWI-100" && g.ProcessId == "GATE_WELL_DESIGN");
    }

    [Fact]
    public async Task PendingGates_ListsAWellsGateReviewInProgress()
    {
        var controller = CreateController(FieldInstances());

        var result = await controller.GetPendingGatesAsync();

        var pending = Assert.IsType<List<ProcessInstanceSummary>>(Assert.IsType<OkObjectResult>(result.Result).Value);
        Assert.Equal(new[] { "well-design-gate" }, pending.Select(g => g.InstanceId));
    }

    private static List<ProcessInstance> FieldInstances() =>
    [
        new() { InstanceId = "well-design-gate", ProcessId = "GATE_WELL_DESIGN", EntityId = "UWI-100", EntityType = "GATE_REVIEW",
                FieldId = FieldId, Status = ProcessStatus.IN_PROGRESS },
        new() { InstanceId = "fdp-gate", ProcessId = "GATE_FDP_REVIEW", EntityId = FieldId, EntityType = "FIELD",
                FieldId = FieldId, Status = ProcessStatus.COMPLETED },
        new() { InstanceId = "well-development", ProcessId = "WELL_DEVELOPMENT", EntityId = "UWI-100", EntityType = "WELL",
                FieldId = FieldId, Status = ProcessStatus.IN_PROGRESS },
    ];

    private static GateReviewController CreateController(List<ProcessInstance> fieldInstances)
    {
        var orchestrator = new Mock<IFieldOrchestrator>(MockBehavior.Strict);
        orchestrator.SetupGet(o => o.CurrentFieldId).Returns(FieldId);

        var process = new Mock<IProcessService>(MockBehavior.Strict);
        process.Setup(p => p.GetProcessDefinitionsByTypeAsync("GATE_REVIEW")).ReturnsAsync(
        [
            new ProcessDefinition { ProcessId = "GATE_WELL_DESIGN", ProcessType = "GATE_REVIEW" },
            new ProcessDefinition { ProcessId = "GATE_FDP_REVIEW", ProcessType = "GATE_REVIEW" },
        ]);
        process.Setup(p => p.GetProcessInstancesForFieldAsync(FieldId)).ReturnsAsync(fieldInstances);

        return new GateReviewController(orchestrator.Object, process.Object, NullLogger<GateReviewController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "user-1")], "TestAuth"))
                }
            }
        };
    }
}
