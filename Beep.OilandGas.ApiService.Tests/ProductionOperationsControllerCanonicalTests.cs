using Beep.OilandGas.ApiService.Controllers.Production;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionOperationsControllerCanonicalTests
{
    [Fact]
    public async Task CreateOperation_ReturnsBadRequest_WhenPayloadMissing()
    {
        var core = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Strict);
        var management = new Mock<IProductionManagementService>(MockBehavior.Loose);
        var controller = new ProductionOperationsController(core.Object, management.Object, NullLogger<ProductionOperationsController>.Instance);

        var result = await controller.CreateOperation(null!);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetOperationStatus_ReturnsBadRequest_WhenIdMissing()
    {
        var core = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Strict);
        var management = new Mock<IProductionManagementService>(MockBehavior.Loose);
        var controller = new ProductionOperationsController(core.Object, management.Object, NullLogger<ProductionOperationsController>.Instance);

        var result = await controller.GetOperationStatus(string.Empty);

        Assert.IsType<BadRequestObjectResult>(result.Result);
        core.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetOperationStatus_ReturnsNotFound_WhenMissing()
    {
        var core = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Strict);
        core.Setup(s => s.GetOperationStatusAsync("OP-404")).ReturnsAsync((PRODUCTION_COSTS?)null);
        var management = new Mock<IProductionManagementService>(MockBehavior.Loose);

        var controller = new ProductionOperationsController(core.Object, management.Object, NullLogger<ProductionOperationsController>.Instance);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.GetOperationStatus("OP-404");

        Assert.IsType<NotFoundObjectResult>(result.Result);
        core.VerifyAll();
    }

    [Fact]
    public async Task UpdateOperation_ReturnsUpdatedEntity()
    {
        var request = new PRODUCTION_COSTS { PRODUCTION_COST_ID = "OP-200" };
        var updated = new PRODUCTION_COSTS { PRODUCTION_COST_ID = "OP-200" };
        var core = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Strict);
        core.Setup(s => s.UpdateOperationAsync("OP-200", request, It.IsAny<string>())).ReturnsAsync(updated);
        var management = new Mock<IProductionManagementService>(MockBehavior.Loose);

        var controller = new ProductionOperationsController(core.Object, management.Object, NullLogger<ProductionOperationsController>.Instance);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.UpdateOperation("OP-200", request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<PRODUCTION_COSTS>(ok.Value);
        Assert.Equal("OP-200", payload.PRODUCTION_COST_ID);
        core.VerifyAll();
    }
}
