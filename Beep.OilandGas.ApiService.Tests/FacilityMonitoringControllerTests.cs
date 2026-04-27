using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Facility;
using Beep.OilandGas.Models.Data.ProductionOperations;
using Beep.OilandGas.ProductionOperations.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public class FacilityMonitoringControllerTests
{
    [Fact]
    public async Task ListMeasurementsAsync_ReturnsOk_WithRows()
    {
        const string facilityId = "FAC-100";
        var expected = new List<FACILITY_MEASUREMENT>
        {
            new FACILITY_MEASUREMENT { FACILITY_ID = facilityId, MEASUREMENT_TYPE = "TANK_LEVEL", MEASURED_VALUE = 82m }
        };

        var service = new Mock<IFacilityManagementService>(MockBehavior.Strict);
        service.Setup(s => s.ListFacilityMeasurementsAsync(
                facilityId,
                "BATTERY",
                "EQ-1",
                "TANK_LEVEL",
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = CreateController(service.Object, "user-1");
        var startDate = new DateTime(2026, 4, 1);
        var endDate = new DateTime(2026, 4, 30);

        var action = await controller.ListMeasurementsAsync(
            facilityId,
            "BATTERY",
            "EQ-1",
            "TANK_LEVEL",
            startDate,
            endDate,
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyList<FACILITY_MEASUREMENT>>(ok.Value);
        Assert.Single(payload);
        Assert.Equal("TANK_LEVEL", payload[0].MEASUREMENT_TYPE);
        service.VerifyAll();
    }

    [Fact]
    public async Task ListMeasurementsAsync_ReturnsBadRequest_WhenDateRangeInvalid()
    {
        var service = new Mock<IFacilityManagementService>(MockBehavior.Strict);
        var controller = CreateController(service.Object, "user-2");

        var action = await controller.ListMeasurementsAsync(
            "FAC-200",
            null,
            null,
            null,
            new DateTime(2026, 5, 10),
            new DateTime(2026, 5, 1),
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action.Result);
        Assert.NotNull(badRequest.Value);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RecordMeasurementAsync_PopulatesFacilityAndUserBeforePersist()
    {
        const string facilityId = "FAC-300";
        FACILITY_MEASUREMENT? captured = null;
        string? capturedUser = null;

        var service = new Mock<IFacilityManagementService>(MockBehavior.Strict);
        service.Setup(s => s.RecordFacilityMeasurementAsync(It.IsAny<FACILITY_MEASUREMENT>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<FACILITY_MEASUREMENT, string, CancellationToken>((row, user, _) =>
            {
                captured = row;
                capturedUser = user;
            })
            .ReturnsAsync((FACILITY_MEASUREMENT row, string _, CancellationToken _) => row);

        var controller = CreateController(service.Object, "facility-user");
        var request = new FACILITY_MEASUREMENT
        {
            MEASUREMENT_TYPE = "TANK_LEVEL",
            MEASURED_VALUE = 76.5m
        };

        var action = await controller.RecordMeasurementAsync(
            facilityId,
            request,
            " BATTERY ",
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var returned = Assert.IsType<FACILITY_MEASUREMENT>(ok.Value);
        Assert.Equal(facilityId, returned.FACILITY_ID);
        Assert.Equal("BATTERY", returned.FACILITY_TYPE);
        Assert.NotNull(captured);
        Assert.Equal("facility-user", capturedUser);
        Assert.Equal(facilityId, captured!.FACILITY_ID);
        Assert.Equal("BATTERY", captured.FACILITY_TYPE);
        service.VerifyAll();
    }

    [Fact]
    public async Task RecordEquipmentActivityAsync_PopulatesFacilityEquipmentAndUserBeforePersist()
    {
        const string facilityId = "FAC-400";
        const string equipmentId = "EQ-400";
        FACILITY_EQUIPMENT_ACTIVITY? captured = null;
        string? capturedUser = null;

        var service = new Mock<IFacilityManagementService>(MockBehavior.Strict);
        service.Setup(s => s.RecordEquipmentActivityAsync(It.IsAny<FACILITY_EQUIPMENT_ACTIVITY>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<FACILITY_EQUIPMENT_ACTIVITY, string, CancellationToken>((row, user, _) =>
            {
                captured = row;
                capturedUser = user;
            })
            .ReturnsAsync((FACILITY_EQUIPMENT_ACTIVITY row, string _, CancellationToken _) => row);

        var controller = CreateController(service.Object, "ops-user");
        var request = new FACILITY_EQUIPMENT_ACTIVITY
        {
            ACTIVITY_TYPE = "INSTALL",
            LOCATION_DESC = "Tank Farm #2"
        };

        var action = await controller.RecordEquipmentActivityAsync(
            facilityId,
            equipmentId,
            request,
            " TERMINAL ",
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var returned = Assert.IsType<FACILITY_EQUIPMENT_ACTIVITY>(ok.Value);
        Assert.Equal(facilityId, returned.FACILITY_ID);
        Assert.Equal(equipmentId, returned.EQUIPMENT_ID);
        Assert.Equal("TERMINAL", returned.FACILITY_TYPE);
        Assert.NotNull(captured);
        Assert.Equal("ops-user", capturedUser);
        Assert.Equal(facilityId, captured!.FACILITY_ID);
        Assert.Equal(equipmentId, captured.EQUIPMENT_ID);
        service.VerifyAll();
    }

    [Fact]
    public async Task ListEquipmentActivityAsync_ReturnsBadRequest_WhenEquipmentMissing()
    {
        var service = new Mock<IFacilityManagementService>(MockBehavior.Strict);
        var controller = CreateController(service.Object, "user-5");

        var action = await controller.ListEquipmentActivityAsync(
            "FAC-500",
            "",
            null,
            null,
            null,
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action.Result);
        Assert.NotNull(badRequest.Value);
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RecordEquipmentActivityAsync_ReturnsBadRequest_WhenServiceThrowsInvalidOperation()
    {
        var service = new Mock<IFacilityManagementService>(MockBehavior.Strict);
        service.Setup(s => s.RecordEquipmentActivityAsync(It.IsAny<FACILITY_EQUIPMENT_ACTIVITY>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Facility not found."));

        var controller = CreateController(service.Object, "user-6");

        var action = await controller.RecordEquipmentActivityAsync(
            "FAC-600",
            "EQ-600",
            new FACILITY_EQUIPMENT_ACTIVITY { ACTIVITY_TYPE = "MOVE" },
            null,
            CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action.Result);
        Assert.NotNull(badRequest.Value);
        service.VerifyAll();
    }

    private static FacilityMonitoringController CreateController(IFacilityManagementService service, string userId)
    {
        var controller = new FacilityMonitoringController(service, NullLogger<FacilityMonitoringController>.Instance);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                    [
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    ], "TestAuth"))
            }
        };
        return controller;
    }
}

