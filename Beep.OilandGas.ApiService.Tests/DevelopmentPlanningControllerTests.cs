using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Field;
using Beep.OilandGas.DevelopmentPlanning.Services;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class DevelopmentPlanningControllerTests
{
    [Fact]
    public async Task GetPlansAsync_ReturnsPlanList()
    {
        var expected = new List<DevelopmentPlan>
        {
            new() { PlanId = "PLAN-A", FieldId = "FIELD-1", PlanName = "Alpha" }
        };

        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.GetDevelopmentPlansAsync("FIELD-1")).ReturnsAsync(expected);

        var controller = CreateController(service.Object, "user-list");
        var result = await controller.GetPlansAsync("FIELD-1");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<List<DevelopmentPlan>>(ok.Value);
        Assert.Single(payload);
        Assert.Equal("PLAN-A", payload[0].PlanId);
        service.VerifyAll();
    }

    [Fact]
    public async Task CreatePlanAsync_ReturnsCreatedPlan()
    {
        var request = new CreateDevelopmentPlan
        {
            PlanName = "New Plan",
            FieldId = "FIELD-2"
        };
        var created = new DevelopmentPlan { PlanId = "PLAN-NEW", FieldId = "FIELD-2", PlanName = "New Plan" };

        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.CreateDevelopmentPlanAsync(request)).ReturnsAsync(created);

        var controller = CreateController(service.Object, "creator");
        var result = await controller.CreatePlanAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<DevelopmentPlan>(ok.Value);
        Assert.Equal("PLAN-NEW", payload.PlanId);
        service.VerifyAll();
    }

    [Fact]
    public async Task UpdatePlanAsync_ReturnsUpdatedPlan()
    {
        var request = new UpdateDevelopmentPlan
        {
            Status = "APPROVED"
        };
        var updated = new DevelopmentPlan { PlanId = "PLAN-U", Status = "APPROVED" };

        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.UpdateDevelopmentPlanAsync("PLAN-U", request)).ReturnsAsync(updated);

        var controller = CreateController(service.Object, "updater");
        var result = await controller.UpdatePlanAsync("PLAN-U", request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<DevelopmentPlan>(ok.Value);
        Assert.Equal("APPROVED", payload.Status);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetPlanAsync_ReturnsNotFound_WhenPlanMissing()
    {
        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.GetDevelopmentPlanAsync("PLAN-404")).ReturnsAsync((DevelopmentPlan?)null);

        var controller = CreateController(service.Object, "user-1");
        var result = await controller.GetPlanAsync("PLAN-404");

        Assert.IsType<NotFoundObjectResult>(result.Result);
        service.VerifyAll();
    }

    [Fact]
    public async Task ApprovePlanAsync_UsesCurrentUserId()
    {
        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.ApproveDevelopmentPlanAsync("PLAN-1", "approver-1"))
            .ReturnsAsync(new DevelopmentPlan { PlanId = "PLAN-1", Status = "APPROVED" });

        var controller = CreateController(service.Object, "approver-1");
        var result = await controller.ApprovePlanAsync("PLAN-1");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<DevelopmentPlan>(ok.Value);
        Assert.Equal("PLAN-1", payload.PlanId);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetWellActivitiesAsync_ReturnsActivitiesFromService()
    {
        var expected = new List<WELL_ACTIVITY> { new() { UWI = "UWI-1", ACTIVITY_TYPE_ID = "WORKOVER" } };
        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.GetWellActivitiesAsync("PLAN-2", "UWI-1")).ReturnsAsync(expected);

        var controller = CreateController(service.Object, "user-2");
        var result = await controller.GetWellActivitiesAsync("PLAN-2", "UWI-1");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<List<WELL_ACTIVITY>>(ok.Value);
        Assert.Single(payload);
        service.VerifyAll();
    }

    [Fact]
    public async Task CreateMaintenanceAsync_ForwardsUserAndReturnsCreatedEntity()
    {
        var request = new CreateWellMaintenancePlan { PlanId = "PLAN-3", WellUwi = "UWI-3", MaintenanceType = "INSPECTION" };
        var created = new WELL_MAINTENANCE_PLAN { MAINT_PLAN_ID = "MAINT-1", FDP_ID = "PLAN-3", UWI = "UWI-3" };

        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.CreateWellMaintenancePlanAsync(request, "maint-user")).ReturnsAsync(created);

        var controller = CreateController(service.Object, "maint-user");
        var result = await controller.CreateMaintenanceAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<WELL_MAINTENANCE_PLAN>(ok.Value);
        Assert.Equal("MAINT-1", payload.MAINT_PLAN_ID);
        service.VerifyAll();
    }

    [Fact]
    public async Task CreateServiceJobAsync_ForwardsUserAndReturnsCreatedEntity()
    {
        var request = new CreateWellServiceJob { PlanId = "PLAN-4", WellUwi = "UWI-4", JobType = "WELL_TEST" };
        var created = new WELL_SERVICE_JOB { JOB_ID = "JOB-1", FDP_ID = "PLAN-4", UWI = "UWI-4" };

        var service = new Mock<IDevelopmentPlanService>(MockBehavior.Strict);
        service.Setup(s => s.CreateWellServiceJobAsync(request, "svc-user")).ReturnsAsync(created);

        var controller = CreateController(service.Object, "svc-user");
        var result = await controller.CreateServiceJobAsync(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<WELL_SERVICE_JOB>(ok.Value);
        Assert.Equal("JOB-1", payload.JOB_ID);
        service.VerifyAll();
    }

    private static DevelopmentPlanningController CreateController(IDevelopmentPlanService service, string userId)
    {
        var controller = new DevelopmentPlanningController(service, NullLogger<DevelopmentPlanningController>.Instance);
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
