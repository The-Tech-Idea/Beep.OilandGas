using System.Collections.Generic;
using Beep.OilandGas.ApiService.Controllers.Operations;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Operations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class LeaseAcquisitionControllerTests
{
    [Fact]
    public async Task CreateLeaseAcquisition_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        var controller = new LeaseAcquisitionController(core.Object, NullLogger<LeaseAcquisitionController>.Instance);

        var result = await controller.CreateLeaseAcquisition(null);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateLeaseStatus_ReturnsBadRequest_WhenLeaseIdMissing()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        var controller = new LeaseAcquisitionController(core.Object, NullLogger<LeaseAcquisitionController>.Instance);

        var result = await controller.UpdateLeaseStatus(string.Empty, new UpdateLeaseStatusRequest { Status = "ACTIVE" });

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateLeaseStatus_ReturnsBadRequest_WhenBodyMissing()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        var controller = new LeaseAcquisitionController(core.Object, NullLogger<LeaseAcquisitionController>.Instance);

        var result = await controller.UpdateLeaseStatus("L-1", null);

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateLeaseStatus_ReturnsNotFound_WhenLeaseMissing()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        core.Setup(s => s.UpdateLeaseStatusAsync("L-404", "INACTIVE", It.IsAny<string>()))
            .ThrowsAsync(new KeyNotFoundException("missing"));
        var controller = new LeaseAcquisitionController(core.Object, NullLogger<LeaseAcquisitionController>.Instance);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.UpdateLeaseStatus("L-404", new UpdateLeaseStatusRequest { Status = "INACTIVE" });

        Assert.IsType<NotFoundObjectResult>(result);
        core.VerifyAll();
    }

    [Fact]
    public async Task UpdateLeaseStatus_ReturnsBadRequest_WhenArgumentException()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        core.Setup(s => s.UpdateLeaseStatusAsync("L-1", "INVALID_STATUS_XYZ", It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("bad status"));
        var controller = new LeaseAcquisitionController(core.Object, NullLogger<LeaseAcquisitionController>.Instance);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.UpdateLeaseStatus("L-1", new UpdateLeaseStatusRequest { Status = "INVALID_STATUS_XYZ" });

        Assert.IsType<BadRequestObjectResult>(result);
        core.VerifyAll();
    }

    [Fact]
    public async Task GetAvailableLeases_ReturnsOk()
    {
        var core = new Mock<ILeaseAcquisitionService>(MockBehavior.Strict);
        core.Setup(s => s.GetAvailableLeasesAsync(It.IsAny<Dictionary<string, string>>()))
            .ReturnsAsync(new List<LeaseSummary>());
        var controller = new LeaseAcquisitionController(core.Object, NullLogger<LeaseAcquisitionController>.Instance);

        var result = await controller.GetAvailableLeases();

        Assert.IsType<OkObjectResult>(result.Result);
        core.VerifyAll();
    }
}
