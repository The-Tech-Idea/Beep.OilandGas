using System.Security.Claims;
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
    public async Task FacilityController_ListFacilityPdenAsync_LeavesAFailureToTheApiHandler()
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

        // OILGAS-CATCH-01: the controller no longer answers a failure itself; it reaches the API's exception handler,
        // which reports it and answers 500 with its reference and never its text (ExceptionAnswerTests).
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.ListFacilityPdenAsync(null, null, CancellationToken.None));
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
                "ops-actor",
                It.IsAny<CancellationToken>()))
            .Callback<CreateProductionOperationRequest, string, CancellationToken>((request, _, _) => captured = request)
            .ReturnsAsync(pden);

        var coreService = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Loose);
        var controller = new ProductionOperationsController(
            coreService.Object,
            productionManagement.Object,
            NullLogger<ProductionOperationsController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "ops-actor")], "TestAuth"))
            }
        };

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
                "ops-actor",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Insert failed"));

        var coreService = new Mock<Beep.OilandGas.Models.Core.Interfaces.IProductionOperationsService>(MockBehavior.Loose);
        var controller = new ProductionOperationsController(
            coreService.Object,
            productionManagement.Object,
            NullLogger<ProductionOperationsController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "ops-actor")], "TestAuth"))
            }
        };

        var result = await controller.CreateOperationCompatibility(new ProductionOperation { OperationType = "PRODUCTION" });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.NotNull(badRequest.Value);
        productionManagement.VerifyAll();
    }
}

