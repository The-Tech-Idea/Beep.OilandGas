using Beep.OilandGas.ApiService.Controllers.Facility;
using Beep.OilandGas.ApiService.Controllers.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class ProductionManagementServiceParityControllerTests
{
    [Fact]
    public async Task FacilityController_ListFacilityPdenAsync_DelegatesAndReturnsRows()
    {
        var expected = new List<PDEN>
        {
            new PDEN { PDEN_ID = "PDEN-FAC-1", PDEN_SUBTYPE = "FACILITY", ACTIVE_IND = "Y" }
        };

        var productionManagement = new Mock<IProductionManagementService>(MockBehavior.Strict);
        productionManagement
            .Setup(service => service.ListFacilityPdenDeclarationsAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var facilities = new Mock<IFacilityManagementService>(MockBehavior.Loose);
        var controller = new FacilityController(
            facilities.Object,
            productionManagement.Object,
            NullLogger<FacilityController>.Instance);

        var startDate = new DateTime(2026, 1, 1);
        var endDate = new DateTime(2026, 1, 31);
        var result = await controller.ListFacilityPdenAsync(startDate, endDate, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var rows = Assert.IsAssignableFrom<IReadOnlyList<PDEN>>(ok.Value);
        Assert.Single(rows);
        Assert.Equal("PDEN-FAC-1", rows[0].PDEN_ID);
        productionManagement.VerifyAll();
    }

    [Fact]
    public async Task FacilityController_ListFacilityPdenAsync_Returns500_WhenServiceThrows()
    {
        var productionManagement = new Mock<IProductionManagementService>(MockBehavior.Strict);
        productionManagement
            .Setup(service => service.ListFacilityPdenDeclarationsAsync(
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Query failed"));

        var facilities = new Mock<IFacilityManagementService>(MockBehavior.Loose);
        var controller = new FacilityController(
            facilities.Object,
            productionManagement.Object,
            NullLogger<FacilityController>.Instance);

        var result = await controller.ListFacilityPdenAsync(null, null, CancellationToken.None);

        var status = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, status.StatusCode);
        productionManagement.VerifyAll();
    }

    [Fact]
    public async Task ProductionOperationsController_CreateOperationCompatibility_MapsRequestAndReturnsLegacyShape()
    {
        var pden = new PDEN
        {
            PDEN_ID = "PDEN-OPS-1",
            PDEN_SUBTYPE = "PRODUCTION",
            CURRENT_STATUS_DATE = new DateTime(2026, 2, 1),
            PDEN_STATUS = "Planned",
            CURRENT_OPERATOR = "ops-user",
            REMARK = "initial op"
        };

        CreateProductionOperationRequest? captured = null;
        var productionManagement = new Mock<IProductionManagementService>(MockBehavior.Strict);
        productionManagement
            .Setup(service => service.CreateProductionOperationAsync(
                It.IsAny<CreateProductionOperationRequest>(),
                It.IsAny<CancellationToken>()))
            .Callback<CreateProductionOperationRequest, CancellationToken>((request, _) => captured = request)
            .ReturnsAsync(pden);

        var coreService = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Loose);
        var controller = new ProductionOperationsController(
            coreService.Object,
            productionManagement.Object,
            NullLogger<ProductionOperationsController>.Instance);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var request = new ProductionOperation
        {
            OperationType = "PRODUCTION",
            ScheduledDate = new DateTime(2026, 2, 1),
            Status = "Planned",
            AssignedTo = "ops-user",
            Remarks = "initial op"
        };

        var result = await controller.CreateOperationCompatibility(request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var mapped = Assert.IsType<ProductionOperation>(ok.Value);
        Assert.Equal("PDEN-OPS-1", mapped.OperationId);
        Assert.NotNull(captured);
        Assert.Equal("PRODUCTION", captured!.OperationType);
        Assert.Equal("Planned", captured.Status);
        Assert.Equal("ops-user", captured.AssignedTo);
        Assert.Equal("initial op", captured.Remarks);
        productionManagement.VerifyAll();
    }

    [Fact]
    public async Task ProductionOperationsController_CreateOperationCompatibility_ReturnsBadRequest_WhenManagementThrowsInvalidOperation()
    {
        var productionManagement = new Mock<IProductionManagementService>(MockBehavior.Strict);
        productionManagement
            .Setup(service => service.CreateProductionOperationAsync(
                It.IsAny<CreateProductionOperationRequest>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Insert failed"));

        var coreService = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Loose);
        var controller = new ProductionOperationsController(
            coreService.Object,
            productionManagement.Object,
            NullLogger<ProductionOperationsController>.Instance);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        var result = await controller.CreateOperationCompatibility(new ProductionOperation { OperationType = "PRODUCTION" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
        productionManagement.VerifyAll();
    }
}

